using CommonDataItems;
using Engine.Engines;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonoGameClient.GameObjects
{
    public class PlayerSprite : DrawableGameComponent
    {
        public Texture2D Image;
        public Point Position;
        public Rectangle BoundingRect;
        public bool Visible = true;
        public int speed = 5;
        public PlayerData pData;
        public Point previousPosition;
        public Color tint = Color.White;
        public TimeSpan delay = new TimeSpan(0, 0, 1);

        // The constructor expects a loaded texture and a start pos...
        public PlayerSprite(Game game, PlayerData data, Texture2D spriteImage,Point startPosition) :base(game)
        {
            pData = data;
            DrawOrder = 1;
            game.Components.Add(this);
            // Copy the texture thats passed...
            Image = spriteImage;
            // Copy the start position...
            previousPosition = Position = startPosition;
            // Calculate the bounding rectangle
            BoundingRect = new Rectangle(
                Position.X, 
                Position.Y, 
                Image.Width, 
                Image.Height);
        }

        public override void Update(GameTime gameTime)
        {
            previousPosition = Position;
            if (InputEngine.IsKeyHeld(Keys.Up))
                Position += new Point(0, -speed);
            if (InputEngine.IsKeyHeld(Keys.Down))
                Position += new Point(0, speed);
            if (InputEngine.IsKeyHeld(Keys.Left))
                Position += new Point(-speed, 0);
            if (InputEngine.IsKeyHeld(Keys.Right))
                Position += new Point(speed, 0);

            //Prevents traffic going up to the server, better for performance...
            delay -= gameTime.ElapsedGameTime;
            // If Player moves pull back the proxy reference and send a message to the Gamehub...
            if (Position != previousPosition)
            {
                delay = new TimeSpan(0, 0, 1);
                pData.playerPosition = new Position { X = Position.X, Y = Position.Y };
                IHubProxy proxy = Game.Services.GetService<IHubProxy>();
                proxy.Invoke("Moved", new Object[]
                {
                    pData.playerID,
                    pData.playerPosition});
            }

            // Calculate a bounding rectangle...
            BoundingRect = new Rectangle(
                Position.X, 
                Position.Y, 
                Image.Width, 
                Image.Height);

            Collectable[] collisonArray = Game1.collectableArray;
            if (collisonArray.Length > 0)
            {
                foreach (Collectable c in collisonArray)
                {
                    if (c.BoundingRect.Intersects(BoundingRect))
                    {
                        c.Visible = false;
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sp = Game.Services.GetService<SpriteBatch>();
            if (sp == null) return;
            if (Image != null && Visible)
            {
                sp.Begin();
                sp.Draw(Image, BoundingRect, tint);
                sp.End();
            }
            base.Draw(gameTime);
        }
    }
}
