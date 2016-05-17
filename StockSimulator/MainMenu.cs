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

        public static int WINDOW_HEIGHT = ScreenManager.WINDOW_HEIGHT;
        public static int WINDOW_WIDTH = ScreenManager.WINDOW_WIDTH;

        private static Rectangle[] buttons = new Rectangle[4];

        static float startPoint = WINDOW_WIDTH * 0.2f;
        static float buttonAreaWidth = WINDOW_WIDTH * 0.6f;
        static int buttonWidth = (int)(buttonAreaWidth * 0.2f);

        static int buttonHeight = (int)(WINDOW_HEIGHT * 0.05f);
        static int heightPoint = (int)(WINDOW_HEIGHT * 0.6f);

        public override void LoadAssets()
        {
            BackgroundColor = Color.LightGray;

            float start = startPoint;

            for (int i = 0; i < 4; i++)
            {
                buttons[i] = new Rectangle((int)start, heightPoint, buttonWidth, buttonHeight);

                start += buttonWidth * 1.25f;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //input handling
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData(new[] { Color.Maroon });

            DrawTitle();
            DrawButtons(t);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawTitle()
        {
            float start = (WINDOW_WIDTH - f_120.MeasureString("StockSimulator").X) / 2;
            Graphing.DrawString(spriteBatch, f_120, "StockSimulator", new Vector2(start, WINDOW_HEIGHT * 0.15f), Color.Maroon, 1f, 0);
        }

        private void DrawButtons(Texture2D t)
        {
            string[] labels = { "Campaign", "Sandbox", "Load Game", "Exit Game" };
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(t, buttons[i], Color.Maroon);
                Vector2 textSize = f_120.MeasureString(labels[i]) / 10f;
                Vector2 textStart = new Vector2(((buttonWidth - textSize.X) / 2) + startPoint, ((buttonHeight - textSize.Y) / 2) + heightPoint);

                Graphing.DrawString(spriteBatch, f_120, labels[i], textStart, Color.White, 0.1f, 0);

                startPoint += buttonWidth * 1.25f;
            }
        }
    }
}
