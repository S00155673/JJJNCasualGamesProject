using System;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CommonDataItems;
using System.Collections.Generic;
using MonoGameClient.GameObjects;
using Engine.Engines;
using System.Linq;
using GameComponentNS;
using Microsoft.Xna.Framework.Media;

namespace MonoGameClient
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        string connectionMessage = string.Empty;
        SpriteFont font;

        // The Signalr Client objects
        HubConnection serverConnection;
        IHubProxy proxy;
        private PlayerData pdata;

        //Variables for background
        Texture2D bg;
        Texture2D bgNoFont;


#region button and menu
        InputEngine input;

        //Creating button
        Button playGameButton;
        Texture2D playgameText;
        MouseState mouseState, previousMouseState;

        public static string password = string.Empty;
        bool firstText = false;
        public bool Done = false;

        //Menu and Game Constants
        const byte MENU = 0, PLAYGAME = 1;

        //variable so current screen is set to menu 
        public int CurrentScreen = MENU;

        //Login Variables
        public static string name = string.Empty;

        public string Name
        {
            get { return name; }

            set { name = value; }
        }

        string output = "";

        public string Output
        {
            get { return output; }

            set { output = value; }
        }

        public void Clear()
        {
            firstText = false;
            Name = string.Empty;
            output = string.Empty;
            input.KeysPressedInLastFrame.Clear();
            InputEngine.ClearState();
            Done = false;
        }
#endregion
        public bool Connected { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            //new InputEngine(this);
            new FadeTextManager(this);
            input = new InputEngine(this);
            IsMouseVisible = true;

            serverConnection = new HubConnection("https://casualgamesjjjn.azurewebsites.net");
            //Use this if you want to test Locally...
            //serverConnection = new HubConnection("http://localhost:12719/");

            serverConnection.StateChanged += ServerConnection_StateChanged;
            proxy = serverConnection.CreateHubProxy("GameHub");
            serverConnection.Start();

            Action<PlayerData> joined = clientJoined;
            proxy.On<PlayerData>("Joined", joined);

            Action<List<PlayerData>> currentPlayers = clientPlayers;
            proxy.On<List<PlayerData>>("CurrentPlayers", currentPlayers);

            Action<PlayerData> playerHasLeft = otherLeft;
            proxy.On<PlayerData>("Leaving", playerHasLeft);

            Action<string, Position> otherMove = otherMovedClient;
            proxy.On<string, Position>("OtherMove", otherMove);

            // Add the proxy client as a Game service so components can send messages 
            Services.AddService<IHubProxy>(proxy);

            base.Initialize();
        }

        private void otherLeft(PlayerData obj)
        {
            //List<OtherPlayer> others = this.Components
            //OtherPlayer playerGone = (OtherPlayer)this.Components.FirstOrDefault(c => c.GetType() == typeof(OtherPlayer));
            OtherPlayer playerGone = (OtherPlayer)this.Components.FirstOrDefault(c => c.GetType() == typeof(OtherPlayer));
            if (playerGone != null)
            {
               this.Components.Remove(playerGone);
            }
        }

        private void otherMovedClient(string playerID, Position newPosition)
        {
            //Iterate through all the other player components, then check to see the correct id and type...       
            foreach (var player in Components)
            {
                if (player.GetType() == typeof(OtherPlayer) && ((OtherPlayer)player).opData.playerID == playerID)
                {
                    OtherPlayer p = ((OtherPlayer)player);
                    p.opData.playerPosition = newPosition;
                    p.Position = new Point(p.opData.playerPosition.X, p.opData.playerPosition.Y);
                    break;
                    //Break out of the only one player position is updated and its found...                    
                }
            }
        }

        //This is only called when another client joins the game...
        private void clientPlayers(List<PlayerData> otherPlayers)
        {
            foreach (PlayerData player in otherPlayers)
            {
                // Create other player sprites in the client...
                new OtherPlayer(
                    this, player, Content.Load<Texture2D>(player.imageName),
                    new Point(player.playerPosition.X, player.playerPosition.Y));
                connectionMessage = player.playerID + " delivered ";
            }
        }

        private void clientJoined(PlayerData otherPlayerData)
        {
            // Create the incoming players sprite...
            new OtherPlayer(this, otherPlayerData, Content.Load<Texture2D>(otherPlayerData.imageName),
            new Point(otherPlayerData.playerPosition.X, otherPlayerData.playerPosition.Y));
            new FadeText(this, Vector2.Zero, otherPlayerData.GamerTag + " has joined the game");
        }

        private void ServerConnection_StateChanged(StateChange State)
        {
            switch (State.NewState)
            {
                case ConnectionState.Connected:
                    //connectionMessage = "Connected...";
                    new FadeText(this,Vector2.Zero, "Connected...");
                    Connected = true;
                    //startGame();
                    break;
                case ConnectionState.Disconnected:
                    connectionMessage = "Disconnected...";
                    if (State.OldState == ConnectionState.Connected)
                        connectionMessage = "Lost connection...";
                    Connected = false;
                    break;
                case ConnectionState.Connecting:
                    connectionMessage = "Connecting...";
                    Connected = false;
                    break;
            }
        }

        private void startGame()
        {
            // Immediate Pattern...
            proxy.Invoke<PlayerData>("Join")
                .ContinueWith( // This processes the message, it returns the async invoke call...
                (p) =>
                { // With p do...
                    if (p.Result == null)
                        connectionMessage = "No player data returned";
                    else
                    {
                        CreatePlayer(p.Result);
                        // This means we are creating our player using the image name in PlayerData...
                        // uses the PlayerData packet to choose the image...
                        // we use a simpleSpritePlayer...
                    }
                });
        }

        private void LeaveGame()
        {
            proxy.Invoke<PlayerData>("Leaving", new object[] {pdata});
        }

        private void CreatePlayer(PlayerData player)
        {
            pdata = player;
            // Create an other player sprites in this client afte
            new PlayerSprite(this, player, Content.Load<Texture2D>(player.imageName),new Point(player.playerPosition.X, player.playerPosition.Y));
            new FadeText(this, Vector2.Zero, "Welcome " + player.GamerTag + " your assigned to " + player.imageName);
            //connectionMessage = player.playerID + " created ";
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(spriteBatch);
            font = Content.Load<SpriteFont>("Message");
            Services.AddService<SpriteFont>(font);

            #region Load for background
            bg = Content.Load<Texture2D>("AsteroidBlaster");
            bgNoFont = Content.Load<Texture2D>("bgnofont");
#endregion

            #region Load for button
            playgameText = Content.Load<Texture2D>("PlayGame");
            playGameButton = new Button(new Rectangle(280, 300, playgameText.Width, playgameText.Height), true);
            playGameButton.load(Content, "PlayGame");
            #endregion

            #region Load for music
            Song bgMusic = Content.Load<Song>("GameMusic");
            MediaPlayer.Play(bgMusic);
            #endregion
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                LeaveGame();
                Exit();

                #region starting in menu and switching to game
                //sets mouse state
                mouseState = Mouse.GetState();

                //switch for setting the scene
                switch (CurrentScreen)
                {
                    case MENU:
                        //What we want to happen in the MENU screen goes in here.
                        //GO TO PLAYGAME SCREEN

                        if (playGameButton.update(new Vector2(mouseState.X, mouseState.Y)) == true && mouseState != previousMouseState && mouseState.LeftButton == ButtonState.Pressed && !firstText && !Done)
                        {

                            Name = Output;
                            Output = string.Empty;
                            firstText = true;
                            InputEngine.ClearState();
                            CurrentScreen = PLAYGAME;
                        }

                        if (InputEngine.IsKeyPressed(Keys.Enter) && firstText && !Done)
                        {
                            Output = string.Empty;
                            Done = true;
                        }
                        if (InputEngine.IsKeyPressed(Keys.Back))
                            if (Output.Length > 0)
                                Output = Output.Remove(Output.Length - 1);
                        if (InputEngine.IsKeyPressed(Keys.Space))
                            Output += " ";
                        break;

                    case PLAYGAME:
                        //What we want to happen when we play our GAME goes in here.               
                        startGame();
                        break;
                }
            }
            // TODO: Add your update logic here

            previousMouseState = mouseState;
#endregion
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (!firstText)
                foreach (var s in input.KeysPressedInLastFrame)
                    Output += s;
            else
                foreach (var s in input.KeysPressedInLastFrame)
                {
                    Output += "*";
                }
            if (Done) return;
            spriteBatch.Begin();
            //Draw the Connection Message...

#region switch for what we want to see
            switch (CurrentScreen)
            {
                case MENU:
                    //What we want to happen in the MENU screen goes in here.
                    spriteBatch.Draw(bg, new Rectangle(0, 0, 800, 480), Color.White);
                    spriteBatch.DrawString(font, "   " + "User Name \n \n" + Output, new Vector2(310, GraphicsDevice.Viewport.Height - 350), Color.White);
                    spriteBatch.Draw(playgameText, new Rectangle(280, 300, playgameText.Width, playgameText.Height), Color.White);

                    break;

                case PLAYGAME:
                    //What we want to happen when we play our GAME goes in here.
                    spriteBatch.Draw(bgNoFont, new Rectangle(0, 0, 800, 480), Color.White);
                    spriteBatch.DrawString(font, connectionMessage, new Vector2(10, 10), Color.White);
                    break;

            }
              spriteBatch.DrawString(font, connectionMessage, new Vector2(10, 10), Color.White);
#endregion
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
