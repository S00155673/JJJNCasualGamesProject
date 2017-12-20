using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameComponentNS
{
    class FadeTextManager : DrawableGameComponent
    {
        Vector2 basePosition;

        public FadeTextManager(Game game) : base(game)
        {
            game.Components.Add(this);
            basePosition = new Vector2(10, game.GraphicsDevice.Viewport.Height - 20);
        }
        protected override void LoadContent()
        {
            
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            var faders = Game.Components.Where(
                            t => t.GetType() == typeof(FadeText));
            if(faders.Count() > 0)
            {
                Vector2 b = basePosition;
                var font = Game.Services.GetService<SpriteFont>();
                Vector2 fontsize = font.MeasureString("Y");
                foreach (FadeText ft in faders)
                {
                    ft.Position = b;
                    b -= new Vector2(0, fontsize.Y - 10);
                }
            }
            base.Update(gameTime);
        }


    }


    class FadeText : DrawableGameComponent
    {
        string text;
        Vector2 position;
        byte blend = 255;

        public Vector2 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public FadeText(Game game, Vector2 Position, string Text) :base(game)
        {
            game.Components.Add(this);
            text = Text;
            this.Position = Position;
        }

        public override void Update(GameTime gameTime)
        {
            if(blend > 0)
                blend--;
            else { Game.Components.Remove(this); }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var sp = Game.Services.GetService<SpriteBatch>();
            var font = Game.Services.GetService<SpriteFont>();
            Color myColor = new Color((byte)0, (byte)0, (byte)0, blend);
            sp.Begin(SpriteSortMode.Immediate,BlendState.Additive);
            sp.DrawString(font, text, Position, new Color((byte)255, (byte)255, (byte)255, blend));
            sp.End();
            base.Draw(gameTime);
        }

    }
}
