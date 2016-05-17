using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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

        public static int WINDOW_HEIGHT = ScreenManager.WINDOW_HEIGHT;
        public static int WINDOW_WIDTH = ScreenManager.WINDOW_WIDTH;

        float tickerScroll = 0;

        public override void LoadAssets()
        {
            BackgroundColor = Color.LightGray;

            DateTime now = DateTime.Now.Date;
            DateTime old = now.AddYears(-1);

            gl = new GameLogic(old, 100000M);

            gl.getData("C", old.ToString("yyyyMMdd"), now.ToString("yyyyMMdd"));
            gl.getData("JPM", old.ToString("yyyyMMdd"), now.ToString("yyyyMMdd"));

            base.LoadAssets();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            drawWalletBox();

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

            //System.DateTime start = new System.DateTime(2015, 1, 1);
            //System.DateTime end = new System.DateTime(2016, 1, 1);
            //System.DateTime[] dates = gl.getStockDates("JPM", start, end);

            //decimal[][] data = gl.getStockDataByDay("JPM", start, end);
            //Color[] colours = { Color.PeachPuff, Color.Navy, Color.Green, Color.MonoGameOrange };

            //float[] values = Graphing.initialiseGraph(spriteBatch, t, f_120, data, dates, 780, 1620, 150, 150); //draw basics of graph and calculate actual plot area excluding margins, etc

            //for (int i = 0; i < 4; i++)
            //{
            //    Vector2[] points = Graphing.pointMaker(data[i], values[0], values[1], values[2], values[3], values[4], values[5]);
            //    Graphing.drawGraph(GraphicsDevice, spriteBatch, colours[i], points);
            //}

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void drawWalletBox()
        {
            //---Draw Box
            int heightStart = (int)(WINDOW_HEIGHT * 0.75f);
            int size = (int)(WINDOW_HEIGHT * 0.25f);

            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData<Color>(new Color[] { Color.White });

            spriteBatch.Draw(t, new Rectangle(0, heightStart, size, size), Color.White);


            //---Text
            string cash, value, change, changeStr;
            Color colour;

            decimal newVal = 0M;
            decimal purchaseVal = 0M;
            foreach(var x in gl.wallet)
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
            Vector2 cashStart = new Vector2((size - cashSize.X) / 2f, (walletStart.Y + walletSize.Y + (size*0.1f)));
            Vector2 valueStart = new Vector2((size - valueSize.X) / 2f, cashStart.Y + cashSize.Y);
            Vector2 changeStrStart = new Vector2((size - changeSize.X) / 2f, valueStart.Y + valueSize.Y);
            Vector2 changeStart = new Vector2(changeStrStart.X + changeStrSize.X, changeStrStart.Y);

            Graphing.DrawString(spriteBatch, f_120, "Wallet", walletStart, Color.Black, 0.2f, 0);
            Graphing.DrawString(spriteBatch, f_120, cash, cashStart, Color.Black, 0.125f, 0);
            Graphing.DrawString(spriteBatch, f_120, value, valueStart, Color.Black, 0.125f, 0);

            Graphing.DrawString(spriteBatch, f_120, changeStr, changeStrStart, Color.Black, 0.125f, 0);
            Graphing.DrawString(spriteBatch, f_120, change, changeStart, colour, 0.125f, 0);
        }

        public override void Update(GameTime gameTime)
        {
            tickerScroll = (tickerScroll + 1) % WINDOW_WIDTH;

            base.Update(gameTime);
        }
    }
}
