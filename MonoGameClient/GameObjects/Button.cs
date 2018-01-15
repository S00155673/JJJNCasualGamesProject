using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MonoGameClient
{
    //normal
    class Button
    {
        Rectangle posSize;
        bool clicked;
        bool available;
        Texture2D image;

        public Button()
        {
            posSize = new Rectangle(100, 100, 100, 50);
            clicked = false;
            available = true;
        }

        public Button(Rectangle rec, bool avail)
        {
            posSize = rec;
            available = avail;
            clicked = false;
        }

        public void load(ContentManager content, string name)
        {
            image = content.Load<Texture2D>(name);
        }
        public bool update(Vector2 mouse)
        {
            if (mouse.X >= posSize.X && mouse.X <= posSize.X + posSize.Width && mouse.Y >= posSize.Y && mouse.Y <= posSize.Y + posSize.Height)
            {
                clicked = true;
            }

            else
            {
                clicked = false;
            }

            if (!available)
            {
                clicked = false;
            }

            return clicked;

        }
        public void draw(SpriteBatch sp)
        {

            Color col = Color.White;

            if (!available)
            {
                col = new Color(50, 50, 50);
            }

            if (clicked)
            {
                col = Color.Green;
            }

            sp.Draw(image, posSize, col);

        }

        public bool Clicked
        {

            get { return clicked; }

            set { clicked = value; }

        }

        public bool Available
        {

            get { return available; }

            set { available = value; }

        }

        public Rectangle PosSize
        {

            get { return posSize; }

            set { posSize = value; }

        }

    }

}
