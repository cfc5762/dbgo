using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Discus
{
    public enum ballDir
    {
        upLeft,
        up,
        upRight,
        downRight,
        down,
        downLeft
    }
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        int screenOffset = 0;
        int packetID = 0;
        int curScroll = 0;
        int prevScroll = 0;
        Dictionary<string, gameplayPacket> acknowledging;
        Dictionary<string,gameplayPacket> unacknowledged;
        Dictionary<string, gameplayPacket> total;
        matchMakingPacket relevant;
        string clientId;
        string enemyId;
        bool found;
        bool matchmaking;
        public List<matchMakingPacket> activeMMPackets;
        public IPEndPoint MainEndPoint;
        public IPEndPoint OpponentEndPoint;
        public Socket listener;
        public Socket sender;
        public Team ballPlaceTeam;
        public bool online;
        public int actions;
        public bool ballFlying;
        public bool networkLeftClick;
        public bool networkRightClick;
        public Team whosTurn;
        public Team playerTeam;
        public Team enemyTeam;
        public Team cyborgThrow;
        public string action;
        public Hex actionHex;
        public List<Hex> abilityHexes;
        public List<Hex> movementHexes;
        public ButtonState prevLeftMouseState;
        public ButtonState prevRightMouseState;
        public int enemyHoveredRow;
        public int enemyHoveredCol;
        public int hoveredRow;
        public int hoveredCol;
        public Hex enemyHoveredSpace;
        public Hex hoveredSpace;
        public List<Hex> boardLocations;
        Texture2D enemycursor;
        Texture2D hex;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        gameplayPacket newest;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            newest = new gameplayPacket();
            // TODO: Add your initialization logic here
            found = false;
            matchmaking = true;
            MainEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 0);//bind to this
            OpponentEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 57735);//send to this
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
            hex = Content.Load<Texture2D>("hex");
            enemycursor = Content.Load<Texture2D>("movementhexoverlay");
            boardLocations = new List<Hex>();
            for (int y = 0; y < 25; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    Vector2 location = Hex.HexToPoints(hex.Width * .15f, y, x);
                    boardLocations.Add(new Hex(location + new Vector2(hex.Width * .075f, 0), y, x));
                }
            }
            for (int i = 0; i < 20; i++)
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
            relevant = new matchMakingPacket();

            listener = new Socket(SocketType.Dgram, ProtocolType.Udp);

            listener.Bind(MainEndPoint);
            acknowledging = new Dictionary<string, gameplayPacket>();
            unacknowledged = new Dictionary<string, gameplayPacket>();
            total = new Dictionary<string, gameplayPacket>();
            relevant.ip = MainEndPoint.Address.GetAddressBytes();
            relevant.port = MainEndPoint.Port;
            sender = new Socket(SocketType.Dgram, ProtocolType.Udp);


            Thread r = new Thread(() => {
                Recieve(listener);
            });
            r.Start();
            Thread s = new Thread(() => {
                Send(sender);
            });
            s.Start();

        }
        void Send(Socket s)
        {
            
            Thread pulseUnacknowledged = new Thread(() => {
                while (true)
                {
                    Thread.Sleep(40);
                    lock (unacknowledged)
                    {
                        if (unacknowledged.Count > 0)
                        {
                            packetCluster pcl = new packetCluster();
                            pcl.acknowledged = false;
                            pcl.cluster = new List<gameplayPacket>();
                            int i = 0;
                            foreach (string item in unacknowledged.Keys)
                            {
                                if (pcl.arbiterId == null)
                                {
                                    pcl.arbiterId = unacknowledged[item].arbiterId;
                                }
                                i++;
                               
                                pcl.cluster.Add(unacknowledged[item]);
                            }

                            BinaryFormatter bf = new BinaryFormatter();
                            MemoryStream m = new MemoryStream();
                            bf.Serialize(m, pcl);
                            if (i >= 10)
                            {
                                bf = new BinaryFormatter();
                                m = new MemoryStream();
                                List<packetCluster> toSend = new List<packetCluster>();

                                for (int j = 0; j < i; j+=10)
                                {
                                    packetCluster tempPcl = new packetCluster();
                                    tempPcl.acknowledged = pcl.acknowledged;
                                    tempPcl.arbiterId = pcl.arbiterId;
                                    tempPcl.cluster = new List<gameplayPacket>();
                                    for (int k = 0;k<10;k++)
                                    {
                                        if (pcl.cluster.Count > (k + j))
                                        {
                                            tempPcl.cluster.Add(pcl.cluster[k + j]);
                                        }
                                        else {
                                            break;
                                        }
                                        
                                    }
                                    m = new MemoryStream();
                                    bf.Serialize(m, tempPcl);
                                    lock (sender)
                                    {
                                        sender.SendTo(m.GetBuffer(), OpponentEndPoint);
                                    }
                                    Thread.Sleep(5);
                                }
                            }
                            else
                            {
                                lock (sender)
                                {
                                    sender.SendTo(m.GetBuffer(), OpponentEndPoint);
                                }
                            }
                            
                        }
                    }
                }

            });
            Thread pulseAcknowledging = new Thread(() => {
                while (true)
                {
                    Thread.Sleep(40);
                    lock (acknowledging)
                    {
                        if (acknowledging.Count > 0)
                        {
                            packetCluster pcl = new packetCluster();
                            pcl.acknowledged = true;
                            pcl.cluster = new List<gameplayPacket>();
                            int i = 0;
                            foreach (string item in acknowledging.Keys)
                            {
                                if (pcl.arbiterId == null)
                                {
                                    pcl.arbiterId = acknowledging[item].arbiterId;
                                }
                                i++;

                                pcl.cluster.Add(acknowledging[item]);
                            }

                            BinaryFormatter bf = new BinaryFormatter();
                            MemoryStream m = new MemoryStream();
                            bf.Serialize(m, pcl);
                            if (i >= 10)
                            {
                                bf = new BinaryFormatter();
                                m = new MemoryStream();
                                List<packetCluster> toSend = new List<packetCluster>();

                                for (int j = 0; j < i; j += 10)
                                {
                                    packetCluster tempPcl = new packetCluster();
                                    tempPcl.acknowledged = pcl.acknowledged;
                                    tempPcl.arbiterId = pcl.arbiterId;
                                    tempPcl.cluster = new List<gameplayPacket>();
                                    for (int k = 0; k < 10; k++)
                                    {
                                        if (pcl.cluster.Count > (k + j))
                                        {
                                            tempPcl.cluster.Add(pcl.cluster[k + j]);
                                        }
                                        else
                                        {
                                            break;
                                        }

                                    }
                                    m = new MemoryStream();
                                    bf.Serialize(m, tempPcl);
                                    lock (sender)
                                    {
                                        sender.SendTo(m.GetBuffer(), OpponentEndPoint);
                                    }
                                    Thread.Sleep(5);
                                }
                            }
                            else
                            {
                                lock (sender)
                                {
                                    sender.SendTo(m.GetBuffer(), OpponentEndPoint);
                                }
                            }

                        }
                    }
                }

            });
            pulseAcknowledging.Start();
            pulseUnacknowledged.Start();
            while (true)
            {
               
                
                if (found)
                {
                    Thread.Sleep(1000 / 60);


                    networkLeftClick = newest.leftClick;
                    networkRightClick = newest.rightClick;
                    enemyHoveredCol = newest.col;
                    enemyHoveredRow = newest.row;
                    networkLeftClick = newest.leftClick;
                    networkRightClick = newest.rightClick;
                    MemoryStream temp = new MemoryStream();
                    EndPoint ep = OpponentEndPoint;
                    gameplayPacket latest = new gameplayPacket();
                    latest.col = hoveredCol;
                    latest.row = hoveredRow;
                    latest.rightClick = Mouse.GetState().LeftButton == ButtonState.Pressed;
                    latest.leftClick = Mouse.GetState().LeftButton == ButtonState.Pressed;
                    latest.arbiterId = clientId;
                    int i = 0; 
                    while (total.ContainsKey(packetID.ToString()))
                    {
                        packetID= (packetID+1)%9999;
                        i++;
                        if (i > 9999)
                        {
                            total = new Dictionary<string, gameplayPacket>();//this is very sloppy but should be ok???????????
                            packetID = 0;
                        }
                    }
                    latest.id = clientId + packetID;
                    
                    
                        lock (unacknowledged)
                        {
                        string[] keys = new string[unacknowledged.Count] ;
                        unacknowledged.Keys.CopyTo(keys,0);
                        if (unacknowledged.Keys.Count > 0)
                        {
                            if (latest.leftClick != unacknowledged[keys[unacknowledged.Count - 1]].leftClick
                                ||
                                latest.rightClick != unacknowledged[keys[unacknowledged.Count - 1]].rightClick
                                ||
                                latest.row != unacknowledged[keys[unacknowledged.Count - 1]].row
                                ||
                                latest.col != unacknowledged[keys[unacknowledged.Count - 1]].col)
                            {
                                lock (total)
                                {
                                    latest.acknowledged = false;
                                    unacknowledged.Add(packetID.ToString(), latest);
                                    total.Add(packetID.ToString(), latest);
                                }
                            }
                        }
                        else
                        {
                            lock (total)
                            {
                                latest.acknowledged = false;
                                unacknowledged.Add(packetID.ToString(), latest);
                                total.Add(packetID.ToString(), latest);
                            }
                        }
                        }
                    
                    
                    
                    BinaryFormatter bf = new BinaryFormatter();
                    temp = new MemoryStream();
                    bf.Serialize(temp, latest);
                    lock (sender)
                    {
                        
                        //sender.SendTo(temp.GetBuffer(), ep);
                    }
                    
                }
                else if (matchmaking)
                {
                    MemoryStream temp = new MemoryStream();
                    EndPoint ep = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 57735);
                    matchMakingPacket latest = new matchMakingPacket();
                    latest.acknowledged = false;
                    latest.ip =new byte[]{ ((IPEndPoint)listener.LocalEndPoint).Address.GetAddressBytes()[12],((IPEndPoint)listener.LocalEndPoint).Address.GetAddressBytes()[13],((IPEndPoint)listener.LocalEndPoint).Address.GetAddressBytes()[14],((IPEndPoint)listener.LocalEndPoint).Address.GetAddressBytes()[15]};
                    latest.port = ((IPEndPoint)listener.LocalEndPoint).Port;
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(temp, latest);
                    lock (sender)
                    {
                        sender.SendTo(temp.GetBuffer(), ep);
                    }
                    Thread.Sleep(2000);
                    
                }
                else
                {
                    //nicks code will go here someday i think
                }

            }
        }
        void Recieve(Socket s)
        {


            while (true)
            {
                if (matchmaking)
                {
                    byte[] buffer = new byte[1024];

                    EndPoint enemyClient = new IPEndPoint(IPAddress.Any, 0);

                    s.ReceiveFrom(buffer, ref enemyClient);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    var latest = binaryFormatter.Deserialize(new MemoryStream(buffer));
                    if (latest is matchMakingPacket)
                    {
                        matchMakingPacket mm = (matchMakingPacket)latest;
                        sender.SendTo(buffer, OpponentEndPoint);
                        OpponentEndPoint = new IPEndPoint(new IPAddress(mm.ip), mm.port);
                        clientId = mm.clientID;
                        enemyId = mm.enemyID;
                        playerTeam = (Discus.Team)mm.color;
                        found = true;
                        matchmaking = false;
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        found = true;
                        matchmaking = false;
                    }

                }
                else
                {
                    byte[] buffer = new byte[1024];

                    EndPoint enemyClient = new IPEndPoint(IPAddress.Any, 0);


                    try
                    {
                        s.ReceiveFrom(buffer, ref enemyClient);
                    }
                    catch (System.Exception)
                    {

                        continue;
                    }
                        



                        BinaryFormatter binaryFormatter = new BinaryFormatter();
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
                                        acknowledging.Remove(gpp.id.Substring(clientId.Length));
                                    }

                                    lock (sender)
                                    {
                                        sender.SendTo(buffer, OpponentEndPoint);
                                    }
                                }
                                else
                                {
                                    
                                    lock (acknowledging)
                                    {
                                        if (!acknowledging.ContainsKey(gpp.id.Substring(clientId.Length)))
                                        {
                                        gpp.acknowledged = true;
                                        newest = gpp;
                                        networkLeftClick = gpp.leftClick;
                                        networkRightClick = gpp.rightClick;
                                        enemyHoveredCol = gpp.col;
                                        enemyHoveredRow = gpp.row;
                                        networkLeftClick = gpp.leftClick;
                                        networkRightClick = gpp.rightClick;
                                        acknowledging.Add(gpp.id.Substring(clientId.Length), gpp);
                                        }
                                        else
                                        {
                                            acknowledging[gpp.id.Substring(clientId.Length)] = gpp;
                                        }
                                    }
                                }
                            }
                            else if (gpp.arbiterId == clientId)
                            {
                                if (gpp.acknowledged)
                                {
                                    lock (sender)
                                    {
                                        sender.SendTo(buffer, OpponentEndPoint);
                                    }
                                    lock (unacknowledged)
                                    {
                                        if (unacknowledged.ContainsKey(gpp.id.Substring(clientId.Length)))
                                        {
                                            unacknowledged.Remove(gpp.id.Substring(clientId.Length));
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
                                    for (int i = 0; i < pcl.cluster.Count; i++)
                                    {
                                        lock (unacknowledged)
                                        {
                                            if (unacknowledged.ContainsKey(pcl.cluster[i].id.Substring(clientId.Length)))
                                            {
                                                unacknowledged.Remove(pcl.cluster[i].id.Substring(clientId.Length));
                                            }
                                        }
                                    }
                                    pcl.acknowledged = true;
                                    MemoryStream t = new MemoryStream();
                                    binaryFormatter.Serialize(t, pcl);
                                    lock (sender)
                                    {
                                        sender.SendTo(t.GetBuffer(), OpponentEndPoint);
                                    }
                                }
                            }
                            else if (pcl.arbiterId == enemyId)
                            {
                                if (pcl.acknowledged)
                                {
                                    for (int i = 0; i < pcl.cluster.Count; i++)
                                    {
                                        gameplayPacket gpp = pcl.cluster[i];
                                        lock (acknowledging)
                                        {
                                            acknowledging.Remove(gpp.id.Substring(clientId.Length));
                                        }

                                        lock (sender)
                                        {
                                            sender.SendTo(buffer, OpponentEndPoint);
                                        }
                                    }
                                }
                                else
                                {
                                pcl.acknowledged = true;
                                MemoryStream t = new MemoryStream();
                                binaryFormatter.Serialize(t, pcl);
                                lock (sender)
                                {
                                    sender.SendTo(t.GetBuffer(), OpponentEndPoint);
                                }
                                for (int i = 0; i < pcl.cluster.Count; i++)
                                    {
                                        
                                        lock (acknowledging)
                                        {
                                            gameplayPacket gpp = pcl.cluster[i];
                                            if (!acknowledging.ContainsKey(gpp.id.Substring(clientId.Length)))
                                            {
                                                acknowledging.Add(gpp.id.Substring(clientId.Length), gpp);
                                               
                                                gpp.acknowledged = true;
                                                newest = gpp;
                                                networkLeftClick = gpp.leftClick;
                                                networkRightClick = gpp.rightClick;
                                                enemyHoveredCol = gpp.col;
                                                enemyHoveredRow = gpp.row;
                                                networkLeftClick = gpp.leftClick;
                                                networkRightClick = gpp.rightClick;
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
                listener.Close();
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
            prevLeftMouseState = Mouse.GetState().LeftButton;
            prevRightMouseState = Mouse.GetState().RightButton;
            // TODO: Add your update logic here
            //MainSceneLogic
            /*if (whosTurn == playerTeam||online == false)
            {
               
            }
            else
            {
                //network input sending/recieving
            }
            
            //click on piece
            if (Mouse.GetState().LeftButton == ButtonState.Released && prevLeftMouseState == ButtonState.Pressed && action == "")
            {
                if (hoveredSpace.piece is Brute && hoveredSpace.piece.team == whosTurn)
                {
                    actionHex = hoveredSpace;
                    action = "brute";
                    if (actionHex.piece.hasAbility)
                    {
                        #region add all adjacent spaces to ability hexes
                        abilityHexes.Add(actionHex.upLeftNeighbor);
                        abilityHexes.Add(actionHex.upNeighbor);
                        abilityHexes.Add(actionHex.upRightNeighbor);
                        abilityHexes.Add(actionHex.downLeftNeighbor);
                        abilityHexes.Add(actionHex.downNeighbor);
                        abilityHexes.Add(actionHex.downRightNeighbor);
                        #endregion
                    }
                    movementHexes = actionHex.getMoveArea(1);
                }
                else if (hoveredSpace.piece is Interceptor && hoveredSpace.piece.team == whosTurn)
                {
                    actionHex = hoveredSpace;
                    action = "interceptor";
                    #region add interceptor ability hexes
                    abilityHexes.Add(actionHex.upLeftNeighbor.upLeftNeighbor);
                    abilityHexes.Add(actionHex.upNeighbor.upNeighbor);
                    abilityHexes.Add(actionHex.upRightNeighbor.upRightNeighbor);
                    abilityHexes.Add(actionHex.downLeftNeighbor.downLeftNeighbor);
                    abilityHexes.Add(actionHex.downNeighbor.downNeighbor);
                    abilityHexes.Add(actionHex.downRightNeighbor.downRightNeighbor);
                    #endregion
                    movementHexes = actionHex.getMoveArea(4);

                }
                else if (hoveredSpace.piece is Cyborg && hoveredSpace.piece.team == whosTurn)
                {
                    actionHex = hoveredSpace;
                    action = "cyborg";
                    movementHexes = actionHex.getMoveArea(3);

                }
                else if (hoveredSpace.piece is Ball && ((Ball)(hoveredSpace.piece)).owner.team == whosTurn)
                {
                    actionHex = hoveredSpace;
                    action = "ballThrow";
                    #region add all adjacent spaces to ability hexes
                    abilityHexes.Add(actionHex.upLeftNeighbor);
                    abilityHexes.Add(actionHex.upNeighbor);
                    abilityHexes.Add(actionHex.upRightNeighbor);
                    abilityHexes.Add(actionHex.downLeftNeighbor);
                    abilityHexes.Add(actionHex.downNeighbor);
                    abilityHexes.Add(actionHex.downRightNeighbor);
                    #endregion
                }
            }
            else if (Mouse.GetState().LeftButton == ButtonState.Released && prevLeftMouseState == ButtonState.Pressed && action == "brute")
            {
                if (actions >= 1 &&actionHex.piece.canMove)
                {
                    actions--;
                    BoardGameHelpers.tryMovePiece(hoveredSpace);
                }
                actionHex = null;
                movementHexes = new List<Hex>();
                abilityHexes = new List<Hex>();
            }
            else if (Mouse.GetState().RightButton == ButtonState.Released && prevRightMouseState == ButtonState.Pressed && action == "brute")
            {
                //brute ability
                if (abilityHexes.Contains(hoveredSpace)&&actionHex.piece.hasAbility)
                {
                    actionHex.piece.hasAbility = false;
                    for (int i = 0; i < abilityHexes.Count; i++)
                    {
                        if (abilityHexes[i].piece.team != actionHex.piece.team)
                        {
                            abilityHexes[i].piece = null;
                        }
                    }
                }
                action = "";
                actionHex = null;
                movementHexes = new List<Hex>();
                abilityHexes = new List<Hex>();
            }
            else if (Mouse.GetState().LeftButton == ButtonState.Released && prevLeftMouseState == ButtonState.Pressed && action == "cyborg")
            {
                if (actions >= 1 && actionHex.piece.canMove)
                {
                    actions--;
                    BoardGameHelpers.tryMovePiece(hoveredSpace);
                }
                actionHex = null;
                movementHexes = new List<Hex>();
                abilityHexes = new List<Hex>();
            }
            else if (action == "placeBall")
            {
                if (Mouse.GetState().LeftButton == ButtonState.Released && prevLeftMouseState == ButtonState.Pressed)
                {
                    if (abilityHexes.Contains(hoveredSpace))
                    {
                        hoveredSpace.piece = new Ball(ballPlaceTeam);
                        ((Ball)hoveredSpace.piece).owner = actionHex.piece;
                        action = "";
                        movementHexes = new List<Hex>();
                        abilityHexes = new List<Hex>();
                        cyborgThrow = Team.Neutral;
                    }
                }
            }
            else if (action == "interceptor")
            {
                if (Mouse.GetState().LeftButton == ButtonState.Released && prevLeftMouseState == ButtonState.Pressed)
                {
                    if (actions >= 1 && actionHex.piece.canMove)
                    {
                        actions--;
                        BoardGameHelpers.tryMovePiece(hoveredSpace);
                    }
                    actionHex = null;
                    movementHexes = new List<Hex>();
                    abilityHexes = new List<Hex>();
                }
                else if (Mouse.GetState().RightButton == ButtonState.Released && prevRightMouseState == ButtonState.Pressed)
                {

                }
            }
            if (whosTurn == playerTeam||online == false)
            {
                prevLeftMouseState = Mouse.GetState().LeftButton;
                prevRightMouseState = Mouse.GetState().RightButton;
            }
            else
            {
                if (networkLeftClick)
                {
                    prevLeftMouseState = ButtonState.Pressed;
                }
                else
                {
                    prevLeftMouseState = ButtonState.Released;
                }
                if (networkRightClick)
                {
                    prevRightMouseState = ButtonState.Pressed;
                }
                else
                {
                    prevRightMouseState = ButtonState.Released;
                }
            }*/
            //MainSceneLogic
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
            foreach (Hex space in boardLocations)
            {
                spriteBatch.Draw(texture: hex, position: space.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2);
            }
            //spriteBatch.Draw(texture: hex, position: space.downNeighbor.location, scale: new Vector2(.1f, .1f), rotation: MathHelper.Pi / 2);

            if (hoveredSpace != null)
            {
                spriteBatch.Draw(texture: hex, position: hoveredSpace.location + (new Vector2(0, screenOffset)), scale: new Vector2(.1f, .1f), rotation: MathHelper.Pi / 2);
                if (prevLeftMouseState == ButtonState.Pressed)
                {
                    spriteBatch.Draw(texture: enemycursor, position: hoveredSpace.location + (new Vector2(0, screenOffset)), scale: new Vector2(.1f, .1f), rotation: MathHelper.Pi / 2);

                }
            }
            if (enemyHoveredSpace != null)
            {
                if (networkLeftClick)
                {
                    spriteBatch.Draw(texture: enemycursor, position: enemyHoveredSpace.location + (new Vector2(0, screenOffset)), scale: new Vector2(.1f, .1f), rotation: MathHelper.Pi / 2);

                }
                else
                {
                    spriteBatch.Draw(texture: enemycursor, position: enemyHoveredSpace.location + (new Vector2(0, screenOffset)), scale: new Vector2(.15f, .15f), rotation: MathHelper.Pi / 2);
                }
            }
            // TODO: Add your drawing code here
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
