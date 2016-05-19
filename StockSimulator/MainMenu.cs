using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StockSimulator
{
    class MainMenu : GameScreen
    {
        SpriteBatch spriteBatch = ScreenManager.spriteBatch;
        SpriteFont f_120 = ScreenManager.f_120;
        GraphicsDevice GraphicsDevice = ScreenManager.graphicsDevice;
        MouseState mouseState;

        public static int WINDOW_HEIGHT = ScreenManager.WINDOW_HEIGHT;
        public static int WINDOW_WIDTH = ScreenManager.WINDOW_WIDTH;

        private readonly Rectangle[] buttons = new Rectangle[4];

        readonly float startPoint = WINDOW_WIDTH * 0.2f;
        readonly static float buttonAreaWidth = WINDOW_WIDTH * 0.6f;
        readonly int heightPoint = (int)(WINDOW_HEIGHT * 0.6f);

        readonly int buttonWidth = (int)(buttonAreaWidth * 0.2f);
        readonly int buttonHeight = (int)(WINDOW_HEIGHT * 0.05f);

        public override void LoadAssets()
        {
            BackgroundColor = Color.LightGray;

            float start = startPoint;

            for (int i = 0; i < 4; i++)
            {
                buttons[i] = new Rectangle((int)start, heightPoint, buttonWidth, buttonHeight);

                start += (int)(buttonWidth * 1.25f);
            }

            base.LoadAssets();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            int choice = -1;

            if (mouseState.LeftButton == ButtonState.Pressed) //check if mouse is pressed and is inside a button
            {
                float start = startPoint;
                for (int i = 0; i < 4; i++)
                {
                    Rectangle area = new Rectangle((int)start, heightPoint, buttonWidth, buttonHeight);
                    start += (int)(buttonWidth * 1.25f);

                    if (area.Contains(mouseState.Position))
                    {
                        choice = i;
                    }
                }
                if (choice != -1)
                {
                    switch (choice)
                    {
                        case 0: //campaign
                            ScreenManager.RemoveScreen(this);
                            break;
                        case 1: //sandbox
                            //loadingString();
                            ScreenManager.AddScreen(new PlayScreen());
                            break;
                        case 2: //load game
                            ScreenManager.RemoveScreen(this);
                            //OpenFile();
                            break;
                        case 3: //exit
                            ScreenManager.RemoveScreen(this);
                            break;
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData(new[] { Color.Maroon });

            DrawTitle();
            DrawButton();
            DrawButtonText();

            //loadingString();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the StockSimulator title
        /// </summary>
        private void DrawTitle()
        {
            float start = (WINDOW_WIDTH - f_120.MeasureString("StockSimulator").X) / 2;
            Graphing.DrawString(spriteBatch, f_120, "StockSimulator", new Vector2(start, WINDOW_HEIGHT * 0.15f), Color.Maroon, 1f, 0);
        }

        /// <summary>
        /// Draws the background of the menu buttons
        /// </summary>
        private void DrawButton()
        {
            Color[] colours = { Color.DarkGray, Color.Maroon };
            int j = 0;

            foreach (Rectangle rectangle in buttons)
            {
                Color[] data = new Color[rectangle.Width * rectangle.Height];
                Texture2D rectTexture = new Texture2D(GraphicsDevice, rectangle.Width, rectangle.Height);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = colours[j % colours.Length];
                }

                rectTexture.SetData(data);
                var position = new Vector2(rectangle.Left, rectangle.Top);

                spriteBatch.Draw(rectTexture, position, colours[j % colours.Length]);
                j++;
            }
        }

        /// <summary>
        /// Draws the button text
        /// </summary>
        private void DrawButtonText()
        {
            float start = startPoint;

            string[] labels = { "Campaign", "Sandbox", "Load Game", "Exit Game" };
            for (int i = 0; i < 4; i++)
            {
                Vector2 textSize = f_120.MeasureString(labels[i]) / 10f;
                float x = ((buttonWidth - textSize.X) / 2) + start;
                float y = ((buttonHeight - textSize.Y) / 2) + heightPoint;
                Vector2 textStart = new Vector2(x, y);

                Graphing.DrawString(spriteBatch, f_120, labels[i], textStart, Color.White, 0.1f, 0);

                start += buttonWidth * 1.25f;
            }
        }

        private void loadingString()
        {
            string str = "Loading...";
            Vector2 size = f_120.MeasureString(str);
            Vector2 start = new Vector2((WINDOW_WIDTH - size.X) / 2, (WINDOW_HEIGHT - size.Y) / 2);

            Graphing.DrawString(spriteBatch, f_120, str, start, Color.Black, 1f, 0);
        }

        private void OpenFile()
        {
            //nothing yet
        }
    }
}
