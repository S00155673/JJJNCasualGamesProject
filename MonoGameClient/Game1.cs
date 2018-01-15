using System;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CommonDataItems;
using System.Collections.Generic;
using MonoGameClient.GameObjects;
using Engine.Engines;
using GameComponentNS;

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

        public bool Connected { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            new InputEngine(this);
            new FadeTextManager(this);

            serverConnection = new HubConnection("https://casualgamesjjjn.azurewebsites.net");
            //Use this if you want to test Locally...
            // serverConnection = new HubConnection ("http://localhost:12719/");
            serverConnection.StateChanged += ServerConnection_StateChanged;
            proxy = serverConnection.CreateHubProxy("GameHub");
            serverConnection.Start();

            Action<PlayerData> joined = clientJoined;
            proxy.On<PlayerData>("Joined", joined);

            Action<List<PlayerData>> currentPlayers = clientPlayers;
            proxy.On<List<PlayerData>>("CurrentPlayers", currentPlayers);

            Action<string, Position> otherMove = otherMovedClient;
            proxy.On<string, Position>("OtherMove", otherMove);

            // Add the proxy client as a Game service so components can send messages 
            Services.AddService<IHubProxy>(proxy);

            base.Initialize();
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
                    //Break out when only one player position is updated and its found...                    
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
                    startGame();
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
            proxy.Invoke<PlayerData>("Left", new object[] {pdata.GamerTag});
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
            }
            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            //Draw the Connection Message...
            spriteBatch.DrawString(font, connectionMessage, new Vector2(10, 10), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
