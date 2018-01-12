using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameClient.GameObjects
{
    class Collectable : DrawableGameComponent
    {
        public Texture2D Image;
        public Point Position;
        public Rectangle BoundingRect;
        public bool Visible = true;

        public Collectable(Game game, Texture2D image, Point position) : base(game)
        {
            Image = image;
            Position = position;
            BoundingRect = new Rectangle(
                Position.X,
                Position.Y,
                Image.Width,
                Image.Height);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sp = Game.Services.GetService<SpriteBatch>();
            if (sp == null) return;
            if (Image != null && Visible)
            {
                sp.Begin();
                sp.Draw(Image, BoundingRect, Color.White);
                sp.End();
            }
            base.Draw(gameTime);
        }
    }
}
