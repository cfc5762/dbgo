using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;



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
        public int hoveredRow;
        public int hoveredCol;
        public Hex hoveredSpace;
        public List<Hex> boardLocations;
        Texture2D hex;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
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
            // TODO: Add your initialization logic here
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
            boardLocations = new List<Hex>();
            for (int y = 0; y < 25; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    Vector2 location = Hex.HexToPoints(hex.Width * .15f, y, x);
                    boardLocations.Add(new Hex(location + new Vector2(hex.Width*.075f,0),y,x));
                }
            }
            for (int i = 0; i < 20; i++)
            {
                boardLocations[i].PopulateNeighbors(boardLocations);
            }
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
        
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            //MainSceneLogic
            if (whosTurn == playerTeam||online == false)
            {
                Hex.PointToHex(Mouse.GetState().Position.X, Mouse.GetState().Position.Y, hex.Width * .15f, out hoveredRow, out hoveredCol);
            }
            else
            {
                //network input sending/recieving
            }
            for (int i = 0; i < boardLocations.Count; i++)
            {
                if (boardLocations[i].gridLocation[0] == hoveredRow && boardLocations[i].gridLocation[1] == hoveredCol)
                {
                    hoveredSpace = boardLocations[i];
                }
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
            }
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
                spriteBatch.Draw(texture: hex, position: space.location, scale: new Vector2(.15f,.15f), rotation:MathHelper.Pi / 2);
            }
            //spriteBatch.Draw(texture: hex, position: space.downNeighbor.location, scale: new Vector2(.1f, .1f), rotation: MathHelper.Pi / 2);

            if (hoveredSpace != null)
            {
                spriteBatch.Draw(texture: hex, position: hoveredSpace.location, scale: new Vector2(.1f, .1f), rotation: MathHelper.Pi / 2);
            }
            // TODO: Add your drawing code here
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
