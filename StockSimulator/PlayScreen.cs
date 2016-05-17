using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StockSimulator
{
    class PlayScreen : GameScreen
    {
        SpriteBatch spriteBatch = ScreenManager.spriteBatch;
        SpriteFont f_120 = ScreenManager.f_120;
        GraphicsDevice GraphicsDevice = ScreenManager.graphicsDevice;
        MouseState mouseState;

        GameLogic gl;

        public static int WINDOW_HEIGHT = ScreenManager.WINDOW_HEIGHT;
        public static int WINDOW_WIDTH = ScreenManager.WINDOW_WIDTH;

        float tickerScroll = 0;

        public override void LoadAssets()
        {
            gl = new GameLogic();

            base.LoadAssets();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);

            Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(0, WINDOW_HEIGHT * 0.08f), new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT * 0.08f), 35);
            Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(0, WINDOW_HEIGHT * 0.875f), new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT * 0.875f), 5);

            gl.getAllChanges();
            float offset = 0;
            foreach (StockChange sc in gl.changes.Values)
            {
                offset += 1.5f * Graphing.DrawTickerString(spriteBatch, f_120, new string[] { sc.symbol, sc.change.ToString() }, new Vector2(WINDOW_WIDTH - (tickerScroll + offset), WINDOW_HEIGHT * 0.085f));
            }

            System.DateTime start = new System.DateTime(2015, 1, 1);
            System.DateTime end = new System.DateTime(2016, 1, 1);
            System.DateTime[] dates = gl.getStockDates("JPM", start, end);

            decimal[][] data = gl.getStockDataByDay("JPM", start, end);
            Color[] colours = { Color.PeachPuff, Color.Navy, Color.Green, Color.MonoGameOrange };

            float[] values = Graphing.initialiseGraph(spriteBatch, t, f_120, data, dates, 780, 1620, 150, 150); //draw basics of graph and calculate actual plot area excluding margins, etc

            for (int i = 0; i < 4; i++)
            {
                Vector2[] points = Graphing.pointMaker(data[i], values[0], values[1], values[2], values[3], values[4], values[5]);
                Graphing.drawGraph(GraphicsDevice, spriteBatch, colours[i], points);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            tickerScroll = (tickerScroll + 1) % WINDOW_WIDTH;

            base.Update(gameTime);
        }
    }
}
