using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockSimulator
{
    class PlayScreen : GameScreen
    {
        SpriteBatch spriteBatch = ScreenManager.spriteBatch;
        SpriteFont f_120 = ScreenManager.f_120;
        GraphicsDevice GraphicsDevice = ScreenManager.graphicsDevice;
        MouseState mouseState;

        GameLogic gl;

        Rectangle walletBox;

        DateTime now;
        DateTime old;

        LinkedList<KeyValuePair<string, string>> ticker = new LinkedList<KeyValuePair<string, string>>();

        public static int WINDOW_HEIGHT = ScreenManager.WINDOW_HEIGHT;
        public static int WINDOW_WIDTH = ScreenManager.WINDOW_WIDTH;

        float tickerScroll = 0;

        /// <summary>
        /// Load what is required for PlayScreen
        /// Sets background color and configures the GameLogic object with the stock data required
        /// </summary>
        public override void LoadAssets()
        {
            BackgroundColor = Color.LightGray;

            now = new DateTime(2016, 6, 1);
            old = new DateTime(2016, 5, 1);
            //old = now.AddYears(-1);

            gl = new GameLogic(old, 100000M);

            gl.getData("C", old.ToString("yyyyMMdd"), now.ToString("yyyyMMdd"));
            gl.getData("JPM", old.ToString("yyyyMMdd"), now.ToString("yyyyMMdd"));

            base.LoadAssets();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            drawWalletBox();
            drawDateBox();
            DrawTicker();
            DrawGraph();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the wallet box and stats in the bottom left corner
        /// </summary>
        private void drawWalletBox()
        {
            //---Draw Box
            int heightStart = (int)(WINDOW_HEIGHT * 0.75f);
            int size = (int)(WINDOW_HEIGHT * 0.25f);

            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData<Color>(new Color[] { Color.White });

            walletBox = new Rectangle(0, heightStart, size, size);

            spriteBatch.Draw(t, walletBox, Color.White);


            //---Text
            string cash, value, change, changeStr;
            Color colour;

            decimal newVal = 0M;
            decimal purchaseVal = 0M;
            foreach (var x in gl.wallet)
            {
                newVal += (x.amount * gl.ex[x.symbol].Last().Value.close);
                purchaseVal += (x.amount * x.purchasePrice);
            }

            cash = "Cash: $" + gl.cash.ToString("N2");
            value = "Value: $" + newVal.ToString("N2");
            changeStr = "Change: ";

            decimal percChange;
            if (purchaseVal == 0)
            {
                percChange = 0;
            }
            else
            {
                percChange = (newVal / purchaseVal) * 100;
            }

            if (percChange > 0)
            {
                colour = Color.Green;
                change = "▲ ";
            }
            else if (percChange < 0)
            {
                colour = Color.Red;
                change = "▼ ";
            }
            else
            {
                colour = Color.SlateBlue;
                change = "= ";
            }

            change += percChange.ToString("N2") + "%";

            //cash, value, change
            Vector2 walletSize = (f_120.MeasureString("Wallet") * 0.2f);
            Vector2 cashSize = (f_120.MeasureString(cash) * 0.125f);
            Vector2 valueSize = (f_120.MeasureString(value) * 0.125f);
            Vector2 changeStrSize = (f_120.MeasureString(changeStr) * 0.125f);
            Vector2 changeSize = (f_120.MeasureString(changeStr + change) * 0.125f);

            Vector2 walletStart = new Vector2((size - walletSize.X) / 2f, heightStart + (size * 0.1f));
            Vector2 cashStart = new Vector2((size - cashSize.X) / 2f, (walletStart.Y + walletSize.Y + (size * 0.1f)));
            Vector2 valueStart = new Vector2((size - valueSize.X) / 2f, cashStart.Y + cashSize.Y);
            Vector2 changeStrStart = new Vector2((size - changeSize.X) / 2f, valueStart.Y + valueSize.Y);
            Vector2 changeStart = new Vector2(changeStrStart.X + changeStrSize.X, changeStrStart.Y);

            Graphing.DrawString(spriteBatch, f_120, "Wallet", walletStart, Color.Black, 0.2f, 0);
            Graphing.DrawString(spriteBatch, f_120, cash, cashStart, Color.Black, 0.125f, 0);
            Graphing.DrawString(spriteBatch, f_120, value, valueStart, Color.Black, 0.125f, 0);

            Graphing.DrawString(spriteBatch, f_120, changeStr, changeStrStart, Color.Black, 0.125f, 0);
            Graphing.DrawString(spriteBatch, f_120, change, changeStart, colour, 0.125f, 0);
        }

        /// <summary>
        /// Method to draw the date box in the bottom left corner
        /// </summary>
        private void drawDateBox()
        {
            int size = (int)(WINDOW_HEIGHT * 0.25f);
            int heightStart = (int)(WINDOW_HEIGHT * 0.75f);
            int widthStart = WINDOW_WIDTH - size;

            //White Box
            Rectangle dateBox = new Rectangle(widthStart, heightStart, size, size);

            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData<Color>(new Color[] { Color.White });

            spriteBatch.Draw(t, dateBox, Color.White);

            //Date text
            string date = gl.currentDate.ToString("dd/MM/yyyy");
            Vector2 dateSize = f_120.MeasureString(date) * 0.2f;
            Vector2 dateStart = new Vector2(widthStart + ((size - dateSize.X) / 2f), heightStart + (size * 0.1f));
            Graphing.DrawString(spriteBatch, f_120, date, dateStart, Color.Black, 0.2f, 0);

            //pause 140-100-140
            Texture2D tx = new Texture2D(GraphicsDevice, 1, 1);
            tx.SetData<Color>(new Color[] { Color.Black });

            Vector2 pos = new Vector2(widthStart + (size / 3f), (heightStart + (size * (2f / 3f))));
            float length = 200f;
            float black = length * (140 / 380);
            float white = length * (100 / 380);

            Rectangle left = new Rectangle((int)pos.X, (int)pos.Y, (int)black, (int)length);
            Rectangle right = new Rectangle((int)(pos.X + black + white), (int)pos.Y, (int)black, (int)length);

            spriteBatch.Draw(tx, left, Color.Black);
            spriteBatch.Draw(tx, right, Color.Black);
        }

        /// <summary>
        /// Method to draw the ticker scrolling along the top of the screen
        /// </summary>
        private void DrawTicker()
        {
            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData(new Color[] { Color.Black });

            Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(0, WINDOW_HEIGHT * 0.08f), new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT * 0.08f), 35);

            gl.getAllChanges();
            float offset = 0;
            float FIXED_WIDTH = f_120.MeasureString("AAPL: ▲ -999.99").X * 0.17f;
            foreach (StockChange sc in gl.changes.Values)
            {
                Graphing.DrawTickerString(spriteBatch, f_120, new string[] { sc.symbol, sc.change.ToString() }, new Vector2(WINDOW_WIDTH - (tickerScroll + offset), WINDOW_HEIGHT * 0.085f));
                offset += FIXED_WIDTH;
            }
        }

        /// <summary>
        /// Method to draw the a stock graph
        /// </summary>
        private void DrawGraph()
        {
            DateTime[] dates = gl.getStockDates("JPM", old, now);

            decimal[][] data = gl.getStockDataByDay("JPM", old, now);
            Color[] colours = { Color.PeachPuff, Color.Navy, Color.Green, Color.MonoGameOrange };
            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            float[] values = Graphing.initialiseGraph(spriteBatch, t, f_120, data, dates, WINDOW_HEIGHT * 0.5f, WINDOW_WIDTH * 0.5f, WINDOW_WIDTH * 0.25f, WINDOW_HEIGHT * 0.25f); //draw basics of graph and calculate actual plot area excluding margins, etc

            for (int i = 0; i < 4; i++)
            {
                Vector2[] points = Graphing.pointMaker(data[i], values[0], values[1], values[2], values[3], values[4], values[5]);
                Graphing.drawGraph(GraphicsDevice, spriteBatch, colours[i], points);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            tickerScroll = (tickerScroll + 1) % WINDOW_WIDTH;

            base.Update(gameTime);
        }
    }
}
