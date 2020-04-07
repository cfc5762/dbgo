using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Discus
{
    public enum ballDir
    {
        upLeft=0,
        up=1,
        upRight=2,
        downRight=3,
        down=4,
        downLeft=5,
        noDir=6
    }
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        string identifier;
        Thread s;
        Thread r;
        int screenOffset = 0;
        int packetID = 0;
        int curScroll = 0;
        int prevScroll = 0;
        Dictionary<string, gameplayPacket> acknowledging;
        List<gameplayPacket> unacknowledged;
        Dictionary<string, gameplayPacket> total;
        matchMakingPacket relevant;
        string clientId;
        string enemyId;
        bool found;
        bool matchmaking;
        public List<matchMakingPacket> activeMMPackets;
        public IPEndPoint server = new IPEndPoint(new IPAddress(new byte[] { 54, 174, 33, 248 }), 57735);
        public IPEndPoint MainEndPoint;
        public IPEndPoint OpponentEndPoint;
        public Socket listener;
        public Socket outsock;
        public Team ballPlaceTeam;
        public bool online;
        public int actions;
        public bool ballFlying;
        public ballDir flightDir;
        public bool networkLeftClick;
        public bool networkRightClick;
        public Team whosTurn;//whos turn is it
        public Team playerTeam;
        public Team enemyTeam;
        public Team cyborgThrow;
        public SpriteFont font;
        public string action;
        public Hex actionHex;
        public List<Hex> abilityHexes;//hexes to use for special interactions
        public List<Hex> movementHexes;//hexes to use for movement
        public ButtonState prevLeftMouseState;//input
        public ButtonState prevRightMouseState;//input
        public int enemyHoveredRow;
        public int enemyHoveredCol;
        public int hoveredRow;
        public int hoveredCol;
        public int currentCommand;//the current command
        public Hex enemyHoveredSpace;//the enemy's currently hovered hex 
        public Hex hoveredSpace;//the currently hovered hex
        public List<Hex> boardLocations;
        public Hex ballHex;//the last place the ball was
        bool toSendLeft;
        bool toSendRight;
        Texture2D ball;
        Texture2D brute;
        Texture2D cyborg;
        Texture2D cursor;
        Texture2D interceptor;
        Texture2D enemycursor;
        Texture2D abilityOverlay;
        Texture2D hex;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        gameplayPacket previousPacket;
        gameplayPacket newestPacket;
        public static IPAddress GetLocalIPAddress()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip;
                    }
                }
                return new IPAddress(new byte[] { 127, 0, 0, 1 });
            }
            return new IPAddress(new byte[] { 127, 0, 0, 1 });
        }
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        public static IPEndPoint GetIPEndPointFromHostName(string hostName, int port, bool throwIfMoreThanOneIP)
        {
            var addresses = System.Net.Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                //  throw new ArgumentException(
                //      "Unable to retrieve address from specified host name.",
                //      "hostName"
                //  );
            }
            else if (throwIfMoreThanOneIP && addresses.Length > 1)
            {
                //  throw new ArgumentException(
                //      "There is more that one IP address to the specified host.",
                //      "hostName"
                //  );
            }
            return new IPEndPoint(addresses[0], port); // Port gets validated here.
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            identifier = RandomString(10);
            whosTurn = Team.Red;
            flightDir = ballDir.up;
            newestPacket = new gameplayPacket();
            // TODO: Add your initialization logic here
            found = false;
            matchmaking = true;
            MainEndPoint = new IPEndPoint(IPAddress.Any, 0);//bind to this
            OpponentEndPoint = new IPEndPoint(new IPAddress(new byte[] { 54, 208, 228, 240 }), 57735);//send to this
            playerTeam = Team.Blue;
            enemyTeam = Team.Red;
            cyborgThrow = Team.Neutral;
            abilityHexes = new List<Hex>();
            movementHexes = new List<Hex>();
            prevLeftMouseState = ButtonState.Released;
            prevRightMouseState = ButtonState.Released;
            action = "";
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1920;
            graphics.ApplyChanges();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            hex = Content.Load<Texture2D>("hex");
            interceptor = Content.Load<Texture2D>("interceptor");
            enemycursor = Content.Load<Texture2D>("movementhexoverlay");
            abilityOverlay = Content.Load<Texture2D>("abilityOverlay");
            cursor = Content.Load<Texture2D>("cursor");
            brute = Content.Load<Texture2D>("brute");
            cyborg = Content.Load<Texture2D>("cyborg");
            ball = Content.Load<Texture2D>("ball");
            boardLocations = new List<Hex>();
            for (int y = 0; y < 27; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    //skips
                    if (y == 26 && x % 2 == 1)
                    {
                        continue;
                    }
                    //red side
                    //left
                    if (y == 26 && x == 1)
                    {
                        continue;
                    }
                    if (y == 26 && x == 3)
                    {
                        continue;
                    }
                    if (y == 25 && x == 1)
                    {
                        continue;
                    }
                    if (y == 26 && x == 2)
                    {
                        continue;
                    }
                    if (y == 26 && x == 0)
                    {
                        continue;
                    }
                    if (y == 25 && x == 0)
                    {
                        continue;
                    }
                    //right
                    if (y == 26 && x == 15)
                    {
                        continue;
                    }
                    if (y == 26 && x == 13)
                    {
                        continue;
                    }
                    if (y == 25 && x == 15)
                    {
                        continue;
                    }
                    if (y == 26 && x == 14)
                    {
                        continue;
                    }
                    if (y == 26 && x == 16)
                    {
                        continue;
                    }
                    if (y == 25 && x == 16)
                    {
                        continue;
                    }
                    //blue side
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    if (x == 0 && y == 1)
                    {
                        continue;
                    }
                    if (x == 1 && y == 0)
                    {
                        continue;
                    }
                    if (x == 2 && y == 0)
                    {
                        continue;
                    }
                    if (x == 16 && y == 0)
                    {
                        continue;
                    }
                    if (x == 16 && y == 1)
                    {
                        continue;
                    }
                    if (x == 15 && y == 0)
                    {
                        continue;
                    }
                    if (x == 14 && y == 0)
                    {
                        continue;
                    }
                    Vector2 location = Hex.HexToPoints(hex.Width * .15f, y, x);
                    boardLocations.Add(new Hex(location + new Vector2(hex.Width * .075f, 0), y, x));

                    //blue side
                    if (x == 2 && y == 5)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Brute(Team.Blue);
                    }
                    if (y == 13 && x == 8)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Ball(Team.Neutral);
                    }
                    if (x == 14 && y == 5)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Brute(Team.Blue);
                    }
                    if (x == 8 && y == 4)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Brute(Team.Blue);
                    }
                    if (x == 8 && y == 9)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Interceptor(Team.Blue);
                    }
                    if (x == 11 && y == 10)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Interceptor(Team.Blue);
                    }
                    if (x == 5 && y == 10)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Interceptor(Team.Blue);
                    }
                    if (x == 1 && y == 10)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Cyborg(Team.Blue);
                    }
                    if (x == 15 && y == 10)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Cyborg(Team.Blue);
                    }
                    //red side
                    if (x == 2 && y == 21)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Brute(Team.Red);
                    }
                    if (x == 14 && y == 21)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Brute(Team.Red);
                    }
                    if (x == 8 && y == 22)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Brute(Team.Red);
                    }
                    if (x == 8 && y == 19)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Interceptor(Team.Red);
                    }
                    if (x == 11 && y == 15)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Interceptor(Team.Red);
                    }
                    if (x == 5 && y == 15)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Interceptor(Team.Red);
                    }
                    if (x == 1 && y == 15)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Cyborg(Team.Red);
                    }
                    if (x == 15 && y == 15)
                    {
                        boardLocations[boardLocations.Count - 1].piece = new Cyborg(Team.Red);
                    }
                }
            }
            for (int i = 0; i < boardLocations.Count; i++)
            {
                boardLocations[i].PopulateNeighbors(boardLocations);
            }
            StartNetworking();
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {

            // TODO: Unload any non ContentManager content here
        }
        void StartNetworking()
        {
            //the relevant matchmaking packet
            relevant = new matchMakingPacket();

            listener = new Socket(SocketType.Dgram, ProtocolType.Udp);


            acknowledging = new Dictionary<string, gameplayPacket>();
            unacknowledged = new List<gameplayPacket>();
            total = new Dictionary<string, gameplayPacket>();
            relevant.ip = MainEndPoint.Address.GetAddressBytes();
            relevant.port = MainEndPoint.Port;
            
            outsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //outsock.Bind(new IPEndPoint(GetLocalIPAddress(), 45454));
            //outsock.Blocking = false;
            s = new Thread(() => {
                Send(outsock);
            });
            s.Start();
            Thread.Sleep(300);
            r = new Thread(() => {
                Recieve(outsock);
            });
            r.Start();
            

        }
        public void resolveCommand(int cmd)
        {
            switch (cmd)
            {
                case 1:

                    if (ballFlying)
                    {
                        BoardGameHelpers.moveBall(ballHex, flightDir);
                    }
                    if (whosTurn == Team.Red)
                    {
                        whosTurn = Team.Blue;
                    }
                    else
                    {
                        whosTurn = Team.Red;
                    }

                    break;
                default:
                    break;
            }
        }
        void Send(Socket s)
        {
            Thread.Sleep(1000/90);
            while (true)
            {
                if (found)//we have a match
                {
                    lock (unacknowledged)//send out our unacknowledged packets in order
                    {
                        if (unacknowledged.Count > 0)
                        {

                            BinaryFormatter bf = new BinaryFormatter();
                            MemoryStream m = new MemoryStream();
                            bf.Serialize(m, unacknowledged[0]);


                            s.SendTo(m.GetBuffer(), OpponentEndPoint);
                            

                        }
                    }
                    lock (acknowledging)//send out packets from the opponent that we have acknowledged
                    {
                        if (acknowledging.Count > 0)
                        {
                            packetCluster pcl = new packetCluster();
                            pcl.acknowledged = true;
                            pcl.cluster = new List<gameplayPacket>();

                            foreach (string item in acknowledging.Keys)
                            {

                                BinaryFormatter bf = new BinaryFormatter();
                                MemoryStream m = new MemoryStream();
                                bf.Serialize(m, acknowledging[item]);


                                s.SendTo(m.GetBuffer(), OpponentEndPoint);

                                break;
                            }


                        }

                    }

                    MemoryStream temp = new MemoryStream();
                    EndPoint ep = OpponentEndPoint;
                    gameplayPacket latest = new gameplayPacket();
                    latest.col = hoveredCol;
                    latest.row = hoveredRow;
                    latest.rightClick = toSendRight;
                    latest.leftClick = toSendLeft;
                    if (toSendLeft)
                    {
                        toSendLeft = false;
                    }
                    if (toSendRight)
                    {
                        toSendRight = false;
                    }
                    latest.arbiterId = clientId;
                    int i = 0;
                    while (total.ContainsKey(packetID.ToString()))
                    {
                        packetID = (packetID + 1) % 9999;
                        i++;
                        if (i > 9999)
                        {
                            total = new Dictionary<string, gameplayPacket>();//this is very sloppy but should be ok???????????
                            packetID = 0;
                        }
                    }
                    latest.id = clientId + packetID;

                    latest.activeCommand = currentCommand;
                    resolveCommand(currentCommand);
                    currentCommand = 0;
                    lock (unacknowledged)
                    {
                        string[] keys = new string[unacknowledged.Count];

                        if (unacknowledged.Count > 0)
                        {
                            if (latest.leftClick != unacknowledged[unacknowledged.Count - 1].leftClick
                                ||
                                latest.rightClick != unacknowledged[unacknowledged.Count - 1].rightClick
                                ||
                                latest.row != unacknowledged[unacknowledged.Count - 1].row
                                ||
                                latest.col != unacknowledged[unacknowledged.Count - 1].col
                                ||
                                latest.activeCommand != unacknowledged[unacknowledged.Count - 1].activeCommand)//if this frame is different than the last one we 
                            {
                                lock (total)
                                {
                                    latest.acknowledged = false;
                                    unacknowledged.Add(latest);
                                    total.Add(packetID.ToString(), latest);


                                }
                            }
                        }
                        else
                        {
                            lock (total)
                            {
                                latest.acknowledged = false;
                                unacknowledged.Add(latest);
                                total.Add(packetID.ToString(), latest);


                            }
                        }
                    }


                }
                else if (matchmaking)//we are looking for a match
                {
                    MemoryStream temp = new MemoryStream();
                    EndPoint ep = server;
                    matchMakingPacket latest = new matchMakingPacket();
                    latest.acknowledged = false;
                    latest.identifier = identifier;
                    //latest.ip = new byte[] { ((IPEndPoint)outsock.LocalEndPoint).Address.GetAddressBytes()[12], ((IPEndPoint)outsock.LocalEndPoint).Address.GetAddressBytes()[13], ((IPEndPoint)outsock.LocalEndPoint).Address.GetAddressBytes()[14], ((IPEndPoint)outsock.LocalEndPoint).Address.GetAddressBytes()[15] };
                    //latest.port = ((IPEndPoint)outsock.LocalEndPoint).Port;
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(temp, latest);

                    s.SendTo(temp.GetBuffer(), ep);
                    
                    Thread.Sleep(2000);
                }
            }
        }
        void GameResponse(byte[] buffer , Socket s){ BinaryFormatter binaryFormatter = new BinaryFormatter();
        var latest = binaryFormatter.Deserialize(new MemoryStream(buffer));
                        if (latest is gameplayPacket)
                        {
                            gameplayPacket gpp = (gameplayPacket)latest;
                            if (gpp.arbiterId == enemyId)
                            {
                                if (gpp.acknowledged)
                                {
                                    lock (acknowledging)
                                    {
                                        acknowledging.Remove(gpp.id.Substring(enemyId.Length));
                                    }


}
                                else
                                {
                                    
                                    lock (acknowledging)
                                    {
                                    string key = gpp.id.Substring(enemyId.Length);
                                        if (!acknowledging.ContainsKey(key))
                                        {//acknowledge the packet
                                        resolveCommand(gpp.activeCommand);
gpp.acknowledged = true;
                                        previousPacket = newestPacket;
                                        newestPacket = gpp;
                                        networkLeftClick = gpp.leftClick;
                                        networkRightClick = gpp.rightClick;
                                        enemyHoveredCol = gpp.col;
                                        enemyHoveredRow = gpp.row;
                                        networkLeftClick = gpp.leftClick;
                                        networkRightClick = gpp.rightClick;
                                        acknowledging.Add(gpp.id.Substring(clientId.Length), gpp);
                                        }
                                        
                                    }
                                }
                            }
                            else if (gpp.arbiterId == clientId)
                            {
                               
                            if (gpp.acknowledged)
                                {
                               

                                    s.SendTo(buffer, OpponentEndPoint);
                                
                                if (unacknowledged.Count > 0)
                                {
                                    lock (unacknowledged)
                                    {
                                        if (unacknowledged[0].id == gpp.id)
                                        {
                                            unacknowledged.RemoveAt(0);
                                        }
                                    }
                                }
                               
                                }
                                else
                                {

                                }
                            }
                        }
                        else if (latest is packetCluster)
                        {
                            packetCluster pcl = (packetCluster)latest;
                            if (pcl.arbiterId == clientId)
                            {
                                if (pcl.acknowledged)
                                {
                                    for (int i = 0; i<pcl.cluster.Count; i++)
                                    {
                                    lock (unacknowledged)
                                        {
                                            if (unacknowledged[0] == pcl.cluster[i])
                                            {
                                            unacknowledged.RemoveAt(0);
                                            }
                                        }
                                    }
                                    pcl.acknowledged = true;
                                    MemoryStream t = new MemoryStream();
binaryFormatter.Serialize(t, pcl);
                                    
                                        s.SendTo(t.GetBuffer(), OpponentEndPoint);
                                   
                                }
                            }
                            else if (pcl.arbiterId == enemyId)
                            {
                                if (pcl.acknowledged)
                                {
                                    for (int i = 0; i<pcl.cluster.Count; i++)
                                    {
                                        gameplayPacket gpp = pcl.cluster[i];
                                        lock (acknowledging)
                                        {
                                            acknowledging.Remove(gpp.id.Substring(clientId.Length));
                                        }

                                        
                                        s.SendTo(buffer, OpponentEndPoint);
                                        
                                    }
                                }
                                else
                                {
                                pcl.acknowledged = true;
                                MemoryStream t = new MemoryStream();
binaryFormatter.Serialize(t, pcl);
                                s.SendTo(t.GetBuffer(), OpponentEndPoint);
                                
                                for (int i = 0; i<pcl.cluster.Count; i++)
                                    {
                                        
                                        lock (acknowledging)
                                        {
                                            gameplayPacket gpp = pcl.cluster[i];
                                            if (!acknowledging.ContainsKey(gpp.id.Substring(clientId.Length)))
                                            {
                                                
                                               
                                                gpp.acknowledged = true;
                                                newestPacket = gpp;
                                                networkLeftClick = gpp.leftClick;
                                                networkRightClick = gpp.rightClick;
                                                enemyHoveredCol = gpp.col;
                                                enemyHoveredRow = gpp.row;
                                                networkLeftClick = gpp.leftClick;
                                                networkRightClick = gpp.rightClick;
                                            acknowledging.Add(gpp.id.Substring(clientId.Length), gpp);
                                            Thread.Sleep((1000 / 10));
                                            }
                                            else
                                            {
                                                acknowledging[gpp.id.Substring(clientId.Length)] = gpp;
                                            }
                                        }
                                        
                                    }
                                    
                                }
                            }
                        }

        }
        void MMresponse(byte[] buffer,Socket s)
        {
            
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            var latest = binaryFormatter.Deserialize(new MemoryStream(buffer));
            if (latest is matchMakingPacket)
            {//when we hear back start matchmaking
                
                matchMakingPacket mm = (matchMakingPacket)latest;
                if (mm.identifier == identifier)
                {
                    s.SendTo(buffer, server);
                    OpponentEndPoint = new IPEndPoint(new IPAddress(mm.ip), mm.port);
                    clientId = mm.clientID;
                    enemyId = mm.enemyID;
                    playerTeam = (Discus.Team)mm.color;
                    if (playerTeam == Team.Red)
                    {
                        enemyTeam = Team.Blue;
                    }
                    else
                    {
                        enemyTeam = Team.Red;
                    }
                    whosTurn = Team.Red;
                    found = true;
                    matchmaking = false;
                    Thread.Sleep(3000);
                }
                
            }
            
            
        }
        void Recieve(Socket s)
        {
            while (true)
            {
                if (matchmaking)//look for match
                {
                    byte[] buffer = new byte[1024];

                    EndPoint enemyClient = new IPEndPoint(IPAddress.Any,0);
                    
                    s.ReceiveFrom(buffer,0,1024,SocketFlags.None, ref enemyClient);

                    MMresponse(buffer, s);
                    
                    
                }
                else
                {
                    byte[] buffer = new byte[1024];

                    EndPoint enemyClient = new IPEndPoint(IPAddress.Any, 0);
                    
                        s.ReceiveFrom(buffer, 0, 1024, SocketFlags.None, ref enemyClient);
                    GameResponse(buffer, s);




                   

                    }
                }
            }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                s.Abort();
                r.Abort();
               
                listener.Close();
                listener.Dispose();
                outsock.Close();
                outsock.Dispose();
                Exit();
            }
            curScroll = Mouse.GetState().ScrollWheelValue;
            if (curScroll > prevScroll)
            {
                screenOffset += 10;
            }
            else if (prevScroll > curScroll)
            {
                screenOffset -= 10;
            }
            prevScroll = curScroll;
            Hex.PointToHex(Mouse.GetState().Position.X, Mouse.GetState().Position.Y-screenOffset, hex.Width * .15f, out hoveredRow, out hoveredCol);

            for (int i = 0; i < boardLocations.Count; i++)
            {
                if (boardLocations[i].gridLocation[0] == hoveredRow && boardLocations[i].gridLocation[1] == hoveredCol)
                {
                    hoveredSpace = boardLocations[i];
                }
                if (boardLocations[i].gridLocation[0] == enemyHoveredRow && boardLocations[i].gridLocation[1] == enemyHoveredCol)
                {
                    enemyHoveredSpace = boardLocations[i];
                }
            }
            //start gamelogic here
            if (toSendLeft & prevLeftMouseState == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                prevLeftMouseState = Mouse.GetState().LeftButton;
            }
            if (toSendRight & prevRightMouseState == ButtonState.Released && Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                prevLeftMouseState = Mouse.GetState().RightButton;
            }
            
            if (!toSendLeft && prevLeftMouseState == ButtonState.Released)
            {
                toSendLeft = Mouse.GetState().LeftButton == ButtonState.Pressed;
            }
           
            if (!toSendRight && prevRightMouseState == ButtonState.Released)
            {
                toSendRight = Mouse.GetState().RightButton == ButtonState.Pressed;
            }
            

            if (Keyboard.GetState().IsKeyDown(Keys.E)&&whosTurn == playerTeam&&currentCommand == 0)//click end button
            {
                if (ballFlying && cyborgThrow == whosTurn)
                {
                    abilityHexes = new List<Hex>();
                    movementHexes = new List<Hex>();
                    action = "curvedisc";
                    actionHex = ballHex;
                    switch (flightDir)
                    {
                        case ballDir.upLeft:
                            abilityHexes.Add(actionHex.upLeftNeighbor);
                            abilityHexes.Add(actionHex.upNeighbor);
                            abilityHexes.Add(actionHex.downLeftNeighbor);
                            break;
                        case ballDir.up:
                            abilityHexes.Add(actionHex.upNeighbor);
                            abilityHexes.Add(actionHex.upLeftNeighbor);
                            abilityHexes.Add(actionHex.upRightNeighbor);
                            break;
                        case ballDir.upRight:
                            abilityHexes.Add(actionHex.upNeighbor);
                            abilityHexes.Add(actionHex.downRightNeighbor);
                            abilityHexes.Add(actionHex.upRightNeighbor);
                            break;
                        case ballDir.downRight:
                            abilityHexes.Add(actionHex.downNeighbor);
                            abilityHexes.Add(actionHex.downRightNeighbor);
                            abilityHexes.Add(actionHex.upRightNeighbor);
                            break;
                        case ballDir.down:
                            abilityHexes.Add(actionHex.downNeighbor);
                            abilityHexes.Add(actionHex.downRightNeighbor);
                            abilityHexes.Add(actionHex.downLeftNeighbor);
                            break;
                        case ballDir.downLeft:
                            abilityHexes.Add(actionHex.downNeighbor);
                            abilityHexes.Add(actionHex.upLeftNeighbor);
                            abilityHexes.Add(actionHex.downLeftNeighbor);
                            break;
                        case ballDir.noDir:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    currentCommand = 1;   
                }
                
            }
            if (action == "")
            {//no selected action
                if (whosTurn == playerTeam)
                {//its the clients turn
                    if (hoveredSpace.piece != null)//we are hovering a piece
                        if (hoveredSpace.piece.team == playerTeam && prevLeftMouseState == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed)//click on a friendly piece
                        {
                            Piece temp = hoveredSpace.piece;//save our piece as a temp
                            actionHex = hoveredSpace;//save our hovered space as the action hex

                            if (temp is Interceptor)
                            {
                                action = "move";
                                if (ballFlying)
                                {//interceptor passive
                                    movementHexes = actionHex.getMoveAreaInterceptor(4, flightDir);
                                }
                                else
                                {//regular movement prep
                                    movementHexes = actionHex.getMoveArea(4);
                                }
                            }
                            else if (temp is Brute)
                            {//regular movement prep
                                movementHexes = actionHex.getMoveArea(2);
                                action = "move";
                            }
                            else if (temp is Cyborg)
                            {//regular movement prep
                                movementHexes = actionHex.getMoveArea(3);
                                action = "move";
                            }
                            if (temp is Ball)
                            {//prompt moveball bc we are trying to throw the ball
                                movementHexes = new List<Hex>();
                                actionHex = ((Ball)temp).ownerSpace;
                                hoveredSpace.piece = null;
                                abilityHexes = actionHex.getMoveArea(1);
                                action = "moveball";
                            }
                        }
                }
                else
                {
                    if (enemyHoveredSpace != null)  
                        if (enemyHoveredSpace.piece != null && newestPacket.leftClick && !previousPacket.leftClick)
                        {
                            if (enemyHoveredSpace.piece.team == enemyTeam)//enemy side - see above comments
                            {

                                Piece temp = enemyHoveredSpace.piece;
                                actionHex = enemyHoveredSpace;

                                if (temp is Interceptor)
                                {
                                    action = "move";
                                    if (ballFlying)
                                    {
                                        movementHexes = actionHex.getMoveAreaInterceptor(4, flightDir);
                                    }
                                    else
                                    {
                                        movementHexes = actionHex.getMoveArea(4);
                                    }
                                }
                                else if (temp is Brute)
                                {
                                    movementHexes = actionHex.getMoveArea(2);
                                    action = "move";
                                }
                                else if (temp is Cyborg)
                                {
                                    movementHexes = actionHex.getMoveArea(3);
                                    action = "move";
                                }
                                if (temp is Ball)
                                {
                                    movementHexes = new List<Hex>();
                                    actionHex = ((Ball)temp).ownerSpace;
                                    enemyHoveredSpace.piece = null;
                                    abilityHexes = actionHex.getMoveArea(1);
                                    action = "moveball";

                                }
                            }
                        }

                }
            }
            else if (action == "move")
            {//trying to move a piece
             //movementhexes contains your valid positions
                if (whosTurn == playerTeam)
                {//clients turn
                    if (hoveredSpace.piece == null && prevLeftMouseState == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && movementHexes.Contains(hoveredSpace))
                    {//no one is there so move
                        movementHexes = new List<Hex>();
                        Piece temp = actionHex.piece;
                        action = "";
                        hoveredSpace.piece = temp;
                        actionHex.piece = null;
                    }
                    else if (hoveredSpace.piece is Ball && prevLeftMouseState == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && movementHexes.Contains(hoveredSpace))
                    {//a ball is there so grab it
                        movementHexes = new List<Hex>();
                        Piece temp = actionHex.piece;
                        ballPlaceTeam = whosTurn;
                        action = "placeball";//prompt ball placement
                        hoveredSpace.piece = temp;
                        actionHex.piece = null;
                        actionHex = hoveredSpace;
                        abilityHexes = actionHex.GetNeighbors();
                    }
                }
                if (!previousPacket.leftClick && newestPacket.leftClick)
                {
                    //enemy side see above comments
                    if (movementHexes.Contains(enemyHoveredSpace))
                    {
                        if (enemyHoveredSpace.piece == null)
                        {
                            
                            movementHexes = new List<Hex>();
                            Piece temp = actionHex.piece;
                            action = "";
                            enemyHoveredSpace.piece = temp;
                            actionHex.piece = null;
                        }
                        else if (enemyHoveredSpace.piece is Ball)
                        {
                            movementHexes = new List<Hex>();
                            Piece temp = actionHex.piece;
                            ballPlaceTeam = whosTurn;
                            action = "placeball";
                            enemyHoveredSpace.piece = temp;
                            actionHex.piece = null;
                            actionHex = enemyHoveredSpace;
                            abilityHexes = actionHex.GetNeighbors();
                        }
                    }
                }
            }
            else if (action == "placeball")
            {//abilityhexes contains all valid ballspots
                if (ballPlaceTeam == playerTeam)
                {
                    if (prevLeftMouseState == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && abilityHexes.Contains(hoveredSpace))
                    {
                        BoardGameHelpers.placeBall(hoveredSpace, new Ball(playerTeam), whosTurn);
                    }

                }
                else if (!previousPacket.leftClick && newestPacket.leftClick && abilityHexes.Contains(enemyHoveredSpace))
                {
                    BoardGameHelpers.placeBall(enemyHoveredSpace, new Ball(enemyTeam), whosTurn);
                }
            }
            else if (action == "moveball")
            {
                if (whosTurn == playerTeam)
                {
                    if (prevLeftMouseState == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && abilityHexes.Contains(hoveredSpace))
                    {
                        movementHexes = new List<Hex>();
                        
                        BoardGameHelpers.placeBall(hoveredSpace, new Ball(Team.Neutral), whosTurn);
                        actionHex = hoveredSpace;
                        abilityHexes = actionHex.GetNeighbors();
                        action = "throwball";
                    }

                }
                else if (!previousPacket.leftClick && newestPacket.leftClick && abilityHexes.Contains(enemyHoveredSpace))
                {
                    movementHexes = new List<Hex>();
                   
                    BoardGameHelpers.placeBall(enemyHoveredSpace, new Ball(Team.Neutral), whosTurn);
                    actionHex = enemyHoveredSpace;
                    abilityHexes = actionHex.GetNeighbors();
                    action = "throwball";
                }
            }
            else if (action == "throwball")
            {
                if (whosTurn == playerTeam)
                {
                    if (prevLeftMouseState == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && abilityHexes.Contains(hoveredSpace))
                    {
                        if (((Ball)actionHex.piece).ownerSpace.piece is Cyborg)
                        {
                            action = "";
                            abilityHexes = new List<Hex>();
                            cyborgThrow = whosTurn;
                            ballFlying = true;
                            flightDir = BoardGameHelpers.getDir(actionHex, hoveredSpace);
                            BoardGameHelpers.moveBall(actionHex, flightDir);
                        }
                        else
                        {
                            action = "";
                            abilityHexes = new List<Hex>();
                            cyborgThrow = Team.Neutral;
                            ballFlying = true;
                            flightDir = BoardGameHelpers.getDir(actionHex, hoveredSpace);
                            BoardGameHelpers.moveBall(actionHex, flightDir);
                        }
                    }

                }
                else if (!previousPacket.leftClick && newestPacket.leftClick && abilityHexes.Contains(enemyHoveredSpace))
                {
                    if (((Ball)actionHex.piece).ownerSpace.piece is Cyborg)
                    {
                        action = "";
                        abilityHexes = new List<Hex>();
                        cyborgThrow = whosTurn;
                        ballFlying = true;
                        flightDir = BoardGameHelpers.getDir(actionHex, enemyHoveredSpace);
                        BoardGameHelpers.moveBall(actionHex, flightDir);
                    }
                    else
                    {
                        action = "";
                        abilityHexes = new List<Hex>();
                        cyborgThrow = Team.Neutral;
                        ballFlying = true;
                        flightDir = BoardGameHelpers.getDir(actionHex, enemyHoveredSpace);
                        BoardGameHelpers.moveBall(actionHex, flightDir);
                    }
                }
            }
            else if (action == "curveball")
            {
                if (whosTurn == playerTeam)
                {
                    if (prevLeftMouseState == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && abilityHexes.Contains(hoveredSpace))
                    {
                        switch (flightDir)
                        {
                            case ballDir.upLeft:
                                if (hoveredSpace == actionHex.downLeftNeighbor)
                                {
                                    flightDir = ballDir.downLeft;
                                }
                                else if (hoveredSpace == actionHex.upNeighbor)
                                {
                                    flightDir = ballDir.up;
                                }
                                break;
                            case ballDir.up:
                                if (hoveredSpace == actionHex.upLeftNeighbor)
                                {
                                    flightDir = ballDir.upLeft;
                                }
                                else if (hoveredSpace == actionHex.upRightNeighbor)
                                {
                                    flightDir = ballDir.upRight;
                                }
                                break;
                            case ballDir.upRight:
                                if (hoveredSpace == actionHex.upNeighbor)
                                {
                                    flightDir = ballDir.up;
                                }
                                else if (hoveredSpace == actionHex.downRightNeighbor)
                                {
                                    flightDir = ballDir.downRight;
                                }
                                break;
                            case ballDir.downRight:
                                if (hoveredSpace == actionHex.upRightNeighbor)
                                {
                                    flightDir = ballDir.upRight;
                                }
                                else if (hoveredSpace == actionHex.downLeftNeighbor)
                                {
                                    flightDir = ballDir.downLeft;
                                }
                                break;
                            case ballDir.down:
                                if (hoveredSpace == actionHex.downLeftNeighbor)
                                {
                                    flightDir = ballDir.downLeft;
                                }
                                else if (hoveredSpace == actionHex.downRightNeighbor)
                                {
                                    flightDir = ballDir.downRight;
                                }
                                break;
                            case ballDir.downLeft:
                                if (hoveredSpace == actionHex.upLeftNeighbor)
                                {
                                    flightDir = ballDir.upLeft;
                                }
                                else if (hoveredSpace == actionHex.downRightNeighbor)
                                {
                                    flightDir = ballDir.downRight;
                                }
                                break;
                            case ballDir.noDir:
                                break;
                            default:
                                break;
                        }
                        currentCommand = 1;
                        abilityHexes = new List<Hex>();
                        action = "";
                        movementHexes = new List<Hex>();
                    }
                    
                }
                else if (!previousPacket.leftClick && newestPacket.leftClick && abilityHexes.Contains(enemyHoveredSpace))
                {
                    switch (flightDir)
                    {
                        case ballDir.upLeft:
                            if (enemyHoveredSpace == actionHex.downLeftNeighbor)
                            {
                                flightDir = ballDir.downLeft;
                            }
                            else if (enemyHoveredSpace == actionHex.upNeighbor)
                            {
                                flightDir = ballDir.up;
                            }
                            break;
                        case ballDir.up:
                            if (enemyHoveredSpace == actionHex.upLeftNeighbor)
                            {
                                flightDir = ballDir.upLeft;
                            }
                            else if (enemyHoveredSpace == actionHex.upRightNeighbor)
                            {
                                flightDir = ballDir.upRight;
                            }
                            break;
                        case ballDir.upRight:
                            if (enemyHoveredSpace == actionHex.upNeighbor)
                            {
                                flightDir = ballDir.up;
                            }
                            else if (enemyHoveredSpace == actionHex.downRightNeighbor)
                            {
                                flightDir = ballDir.downRight;
                            }
                            break;
                        case ballDir.downRight:
                            if (enemyHoveredSpace == actionHex.upRightNeighbor)
                            {
                                flightDir = ballDir.upRight;
                            }
                            else if (enemyHoveredSpace == actionHex.downLeftNeighbor)
                            {
                                flightDir = ballDir.downLeft;
                            }
                            break;
                        case ballDir.down:
                            if (enemyHoveredSpace == actionHex.downLeftNeighbor)
                            {
                                flightDir = ballDir.downLeft;
                            }
                            else if (enemyHoveredSpace == actionHex.downRightNeighbor)
                            {
                                flightDir = ballDir.downRight;
                            }
                            break;
                        case ballDir.downLeft:
                            if (enemyHoveredSpace == actionHex.upLeftNeighbor)
                            {
                                flightDir = ballDir.upLeft;
                            }
                            else if (enemyHoveredSpace == actionHex.downRightNeighbor)
                            {
                                flightDir = ballDir.downRight;
                            }
                            break;
                        case ballDir.noDir:
                            break;
                        default:
                            break;
                    }
                    currentCommand = 1;
                    abilityHexes = new List<Hex>();
                    action = "";
                    movementHexes = new List<Hex>();
                }
            }
            if (newestPacket.leftClick && !previousPacket.leftClick)
            {
                previousPacket.leftClick = true;
            }
            
            prevLeftMouseState = Mouse.GetState().LeftButton;
            prevRightMouseState = Mouse.GetState().RightButton;
            
           
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            //boardOverlay
            if (hoveredSpace != null)
            {
                spriteBatch.Draw(texture: enemycursor, position: hoveredSpace.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2, color: Color.White);
            }
            if (enemyHoveredSpace != null)
            {
                if (networkLeftClick)
                {
                    spriteBatch.Draw(texture: enemycursor, position: enemyHoveredSpace.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2, color: Color.Violet);

                }
                else
                {
                    spriteBatch.Draw(texture: enemycursor, position: enemyHoveredSpace.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2, color: Color.Black);
                }
            }
            //pieces
            foreach (Hex space in boardLocations)
            {
                spriteBatch.Draw(texture: hex, position: space.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2);
                spriteBatch.DrawString(font, space.gridLocation[0] + " " + space.gridLocation[1], space.location-new Vector2(60,-20-screenOffset), Color.White);
                foreach (Hex item in movementHexes)
                {
                    if (space.gridLocation[0] == item.gridLocation[0] && space.gridLocation[1] == item.gridLocation[1])
                    {
                        spriteBatch.Draw(texture: enemycursor, position: space.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2);
                        break;
                    }
                }

                foreach (Hex item in abilityHexes)
                {
                    if (space.gridLocation[0] == item.gridLocation[0] && space.gridLocation[1] == item.gridLocation[1])
                    {
                        spriteBatch.Draw(texture: enemycursor, position: space.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2);
                        break;
                    }
                }
                if (space.piece != null)
                {
                    if (space.piece.team == Team.Red)
                    {
                        if (space.piece is Interceptor)
                        {
                            spriteBatch.Draw(texture: interceptor, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f, color: Color.Red);
                        }
                        if (space.piece is Ball)
                        {
                            spriteBatch.Draw(texture: ball, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f, color: Color.Red);

                        }
                        if (space.piece is Cyborg)
                        {
                            spriteBatch.Draw(texture: cyborg, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f, color: Color.Red);

                        }
                        if (space.piece is Brute)
                        {
                            spriteBatch.Draw(texture: brute, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f, color: Color.Red);

                        }
                    }
                    else if(space.piece.team == Team.Blue)
                    {
                        if (space.piece is Interceptor)
                        {
                            spriteBatch.Draw(texture: interceptor, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f, color:Color.Blue);
                        }
                        if (space.piece is Ball)
                        {
                            spriteBatch.Draw(texture: ball, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f, color: Color.Blue);

                        }
                        if (space.piece is Cyborg)
                        {
                            spriteBatch.Draw(texture: cyborg, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f, color: Color.Blue);

                        }
                        if (space.piece is Brute)
                        {
                            spriteBatch.Draw(texture: brute, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2)- 1.04719755f, color: Color.Blue);

                        }
                    }
                    else
                    {
                        if (space.piece is Interceptor)
                        {
                            spriteBatch.Draw(texture: interceptor, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f);
                        }
                        if (space.piece is Ball)
                        {
                            spriteBatch.Draw(texture: ball, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f);

                        }
                        if (space.piece is Cyborg)
                        {
                            spriteBatch.Draw(texture: cyborg, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f);

                        }
                        if (space.piece is Brute)
                        {
                            spriteBatch.Draw(texture: brute, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f);

                        }
                        if (space.piece is Ball)
                        {
                            spriteBatch.Draw(texture: ball, position: space.location + (new Vector2(-91, screenOffset + -30)), scale: new Vector2(.15f, .15f), rotation: (MathHelper.Pi / 2) - 1.04719755f);
                        }
                    }
                }
            }
            if(actionHex!=null)
            spriteBatch.Draw(texture: hex, position: actionHex.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2,color:Color.Violet);
            //spriteBatch.Draw(texture: hex, position: space.downNeighbor.location, scale: new Vector2(.1f, .1f), rotation: MathHelper.Pi / 2);
            //ui
            spriteBatch.DrawString(font, action + " - current action\n" + whosTurn.ToString() + " current turn\n" + playerTeam + " - my team\nBall place team - " + ballPlaceTeam.ToString(), Vector2.Zero, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 1);
            try
            {
                spriteBatch.DrawString(font, action + " - current action\n" + whosTurn.ToString() + " current turn\n" + playerTeam + " - my team\nBall place team - " + ballPlaceTeam.ToString() + "\n" + ((IPEndPoint)outsock.LocalEndPoint).Address + " " + ((IPEndPoint)outsock.LocalEndPoint).Port, Vector2.Zero, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 1);
            }
            catch (System.Exception)
            {

            }
            //mouse
            spriteBatch.Draw(texture: cursor, position: Mouse.GetState().Position.ToVector2() + new Vector2(-5, 60),scale: new Vector2(1f / 6f, 1f / 6f));

            if (prevLeftMouseState == ButtonState.Pressed)
            {
                spriteBatch.Draw(texture: cursor, position: Mouse.GetState().Position.ToVector2() + new Vector2(-5, 60), scale: new Vector2(1.11f/6f, 1.11f/6f));
            }
            // TODO: Add your drawing code here
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
