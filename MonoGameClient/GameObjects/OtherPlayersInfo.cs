using CommonDataItems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameClient.GameObjects
{
    public class OtherPlayer : DrawableGameComponent
    {
        public Texture2D Image;
        public Point Position;
        public Rectangle BoundingRect;
        public bool Visible = true;
        public Color tint = Color.White;
        public PlayerData opData;

        // The constructor expects a loaded texture and a start pos...
        public OtherPlayer(
            Game game, PlayerData data, Texture2D spriteImage,
            Point startPosition) : base(game)
        {
            opData = data;
            game.Components.Add(this);
            // Copy the texture thats passed...
            Image = spriteImage;
            // Copy the start position...
            Position = startPosition;
            // Calculate a bounding rectangle...
            BoundingRect = new Rectangle(
                startPosition.X, 
                startPosition.Y, 
                Image.Width, 
                Image.Height); 
        }

        public override void Update(GameTime gameTime)
        {
            BoundingRect = new Rectangle(
                Position.X, 
                Position.Y,
                Image.Width, 
                Image.Height);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sp = Game.Services.GetService<SpriteBatch>();
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