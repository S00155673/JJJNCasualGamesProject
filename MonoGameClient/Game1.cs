using System;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CommonDataItems;

namespace MonoGameClient
{ 
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        string connectionMessage = string.Empty;

        // The Signalr Client objects
        HubConnection serverConnection;
        IHubProxy proxy;

        public bool Connected { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            serverConnection = new HubConnection("https://casualgamesjjjn.azurewebsites.net"); //"http://localhost:12719/");
            serverConnection.StateChanged += ServerConnection_StateChanged;
            proxy = serverConnection.CreateHubProxy("GameHub");
            serverConnection.Start();
            base.Initialize();
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        private void ServerConnection_StateChanged(StateChange State)
        {
            switch (State.NewState)
            {
                case ConnectionState.Connected:
                    connectionMessage = "Connected...";
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

        private void CreatePlayer(PlayerData result)
        {

        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
