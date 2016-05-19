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
        SpriteFont f_30 = ScreenManager.f_30;
        GraphicsDevice GraphicsDevice = ScreenManager.graphicsDevice;
        MouseState mouseState;
        MouseState oldMouseState;

        GameLogic gl;

        Rectangle walletBox;
        Rectangle leftArrow, rightArrow;
        Rectangle quantLeftArrow, quantRightArrow;
        Rectangle buyButton, sellButton;
        Rectangle nextDay;

        int stockNumber = 0;
        string[] stocks;
        string selectedStock;
        decimal price = 0M;
        string date = "";

        int quantity = 0;

        float graphXStart = 0;
        float graphYStart = 0;
        float graphHeight = 0;
        float graphWidth = 0;

        bool input = false;

        DateTime start, end;

        LinkedList<KeyValuePair<string, string>> ticker = new LinkedList<KeyValuePair<string, string>>();

        public static int WINDOW_HEIGHT = ScreenManager.WINDOW_HEIGHT;
        public static int WINDOW_WIDTH = ScreenManager.WINDOW_WIDTH;

        float tickerScroll = 0;
        int graphWeeks = 52;

        public PlayScreen()
        {
        }

        public PlayScreen(GameLogic g)
        {
            gl = g;
        }

        /// <summary>
        /// Load what is required for PlayScreen
        /// Sets background color and configures the GameLogic object with the stock data required
        /// </summary>
        public override void LoadAssets()
        {
            BackgroundColor = Color.LightGray;

            start = new DateTime(2014, 5, 5);
            DateTime dateToStart = new DateTime(2015, 5, 5);
            end = new DateTime(2016, 5, 2);

            gl = new GameLogic(dateToStart, 100000M);

            gl.getData("C", start.ToString("yyyyMMdd"), end.ToString("yyyyMMdd"));
            gl.getData("JPM", start.ToString("yyyyMMdd"), end.ToString("yyyyMMdd"));

            var symbols = from entry in gl.ex.Keys orderby entry ascending select entry;
            stocks = new string[symbols.Count()];
            int i = 0;
            foreach(string x in symbols)
            {
                stocks[i] = x;
                i++;
            }
            selectedStock = stocks[stockNumber];

            date = gl.currentDate.ToString("dd/MM/yyyy");

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
            drawPlayBox();
            DrawGraph(graphWeeks);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Method to draw the bottom left box
        /// </summary>
        private void drawPlayBox()
        {
            float sides = WINDOW_HEIGHT * (0.25f * 1.25f);
            float height = 0.15f * WINDOW_HEIGHT;
            float boxWidth = WINDOW_WIDTH - (2 * sides);
            float boxHeight = 0.75f * WINDOW_HEIGHT;

            StockRow stock = gl.ex[selectedStock][gl.currentDate.Date];

            graphWidth = boxWidth * 0.5f;
            graphXStart = sides + graphWidth;

            //Background
            Color col = Color.Cornsilk;
            Rectangle box = new Rectangle((int)sides, (int)height, (int)boxWidth, (int)boxHeight);
            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData<Color>(new Color[] { col });
            spriteBatch.Draw(t, box, col);

            float titleHeight = f_120.MeasureString("A").Y / 2f;
            graphYStart = height + titleHeight;
            graphHeight = boxHeight - titleHeight;

            string name = gl.getName(selectedStock);
            Graphing.DrawString(spriteBatch, f_120, name, new Vector2(sides, height), Color.Black, 0.5f, 0);

            //left 2/3 text etc
            Vector2 searchTextSize = f_30.MeasureString("Symbol: ");
            Vector2 searchTextStart = new Vector2(sides, height + titleHeight);
            Graphing.DrawString(spriteBatch, f_30, "Symbol: ", searchTextStart, Color.Black, 1f, 0);

            Vector2 searchBoxSize = f_30.MeasureString("AAAAA");
            Vector2 arrowSize = f_30.MeasureString("▶");
            Vector2 arrowStart = new Vector2(searchTextStart.X + searchTextSize.X, searchTextStart.Y);
            Vector2 rightArrowStart = new Vector2(arrowStart.X + arrowSize.X + searchTextSize.X, arrowStart.Y);
            leftArrow = new Rectangle((int)arrowStart.X, (int)arrowStart.Y, (int)arrowSize.X, (int)arrowSize.Y);
            rightArrow = new Rectangle((int)rightArrowStart.X, (int)rightArrowStart.Y, (int)arrowSize.X, (int)arrowSize.Y);
            Rectangle stockBox = new Rectangle((int)(arrowStart.X + arrowSize.X), (int)arrowStart.Y, (int)searchBoxSize.X, (int)searchBoxSize.Y);

            t.SetData(new Color[] { Color.White });
            spriteBatch.Draw(t, stockBox, Color.White);

            float symbolWidth = f_30.MeasureString(selectedStock).X;
            Vector2 symbolStart = new Vector2(arrowStart.X + arrowSize.X + (searchBoxSize.X - symbolWidth) / 2, arrowStart.Y);
            Graphing.DrawString(spriteBatch, f_30, "<", arrowStart, Color.Black, 1f, 0);
            Graphing.DrawString(spriteBatch, f_30, ">", rightArrowStart, Color.Black, 1f, 0);

            Graphing.DrawString(spriteBatch, f_30, selectedStock, symbolStart, Color.Black, 1f, 0);

            //High/Low/Open/Close
            string high = "High: $" + stock.high.ToString("N2");
            string low = "Low: $" + stock.low.ToString("N2");
            string open = "Open: $" + stock.open.ToString("N2");
            string close = "Close: $" + stock.close.ToString("N2");

            Vector2 highStart = searchTextStart;
            highStart.Y += searchTextSize.Y * 1.35f;
            Graphing.DrawString(spriteBatch, f_30, high, highStart, Color.Black, 1f, 0);
            highStart.Y += searchTextSize.Y * 1.35f;
            Graphing.DrawString(spriteBatch, f_30, low, highStart, Color.Black, 1f, 0);
            highStart.Y += searchTextSize.Y * 1.35f;
            Graphing.DrawString(spriteBatch, f_30, open, highStart, Color.Black, 1f, 0);
            highStart.Y += searchTextSize.Y * 1.35f;
            Graphing.DrawString(spriteBatch, f_30, close, highStart, Color.Black, 1f, 0);
            highStart.Y += searchTextSize.Y * 1.35f;

            //Quantity
            Vector2 quantityTextSize = f_30.MeasureString("Quantity: ");
            Graphing.DrawString(spriteBatch, f_30, "Quantity: ", highStart, Color.Black, 1f, 0);

            Vector2 QLarrowStart = new Vector2(highStart.X + quantityTextSize.X, highStart.Y);
            Vector2 QRarrowStart = new Vector2(QLarrowStart.X + arrowSize.X + quantityTextSize.X, highStart.Y);
            quantLeftArrow = new Rectangle((int)QLarrowStart.X, (int)QLarrowStart.Y, (int)arrowSize.X, (int)arrowSize.Y);
            quantRightArrow = new Rectangle((int)QRarrowStart.X, (int)QRarrowStart.Y, (int)arrowSize.X, (int)arrowSize.Y);
            Graphing.DrawString(spriteBatch, f_30, "<", QLarrowStart, Color.Black, 1f, 0);
            Graphing.DrawString(spriteBatch, f_30, ">", QRarrowStart, Color.Black, 1f, 0);

            Rectangle quantBox = new Rectangle((int)(QLarrowStart.X + arrowSize.X), (int)QLarrowStart.Y, (int)searchBoxSize.X, (int)searchBoxSize.Y);
            spriteBatch.Draw(t, quantBox, Color.White);

            Vector2 quantitySize = f_30.MeasureString(quantity.ToString());
            Vector2 quantityStart = new Vector2(quantBox.Left + ((quantBox.Width - quantitySize.X) / 2), quantBox.Top);
            Graphing.DrawString(spriteBatch, f_30, quantity.ToString(), quantityStart, Color.Black, 1f, 0);

            highStart.Y += searchTextSize.Y * 1.25f;

            decimal cost = quantity * stock.close;
            string costStr = "Total: $" + cost.ToString("N2");
            Graphing.DrawString(spriteBatch, f_30, costStr, highStart, Color.Black, 1f, 0);

            //Buy/Sell Buttons
            highStart.Y += searchTextSize.Y * 1.25f;

            Vector2 buttonAreaStart = new Vector2(highStart.X, highStart.Y);
            float buttonSpacing = boxWidth / 14f;
            float buttonWidth = buttonSpacing * 2f;
            float buttonHeight = searchTextSize.Y;

            Vector2 buyButtonStart = new Vector2(buttonAreaStart.X + buttonSpacing, buttonAreaStart.Y);
            Vector2 sellButtonStart = new Vector2((buttonAreaStart.X + buttonWidth + (2 * buttonSpacing)), buttonAreaStart.Y);

            float buyOffset = (buttonWidth - f_30.MeasureString("Buy").X) /2f;
            float sellOffset = (buttonWidth - f_30.MeasureString("Sell").X) /2f;

            Vector2 buyTextStart = new Vector2(buttonAreaStart.X + buttonSpacing + buyOffset, buttonAreaStart.Y);
            Vector2 sellTextStart = new Vector2((buttonAreaStart.X + buttonWidth + (2 * buttonSpacing)) + sellOffset, buttonAreaStart.Y);

            buyButton = new Rectangle((int)buyButtonStart.X, (int)buyButtonStart.Y, (int)buttonWidth, (int)buttonHeight); //x y width height
            sellButton = new Rectangle((int)sellButtonStart.X, (int)sellButtonStart.Y, (int)buttonWidth, (int)buttonHeight);

            spriteBatch.Draw(t, buyButton, Color.White);
            spriteBatch.Draw(t, sellButton, Color.White);

            Graphing.DrawString(spriteBatch, f_30, "Buy", buyTextStart, Color.Black, 1f, 0);
            Graphing.DrawString(spriteBatch, f_30, "Sell", sellTextStart, Color.Black, 1f, 0);
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
                newVal += (x.amount * gl.ex[x.symbol][gl.currentDate.Date].close);
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
                percChange = ((newVal -purchaseVal) / purchaseVal) * 100;
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

            Vector2 nextSize = f_30.MeasureString("Next Day");
            Vector2 nextStart = new Vector2((widthStart + (size - nextSize.X) / 2), dateStart.Y + (dateSize.Y * 1.5f));
            Graphing.DrawString(spriteBatch, f_30, "Next Day", nextStart, Color.DarkBlue, 1f, 0);
            nextDay = new Rectangle((int)nextStart.X, (int)nextStart.Y, (int)nextSize.X, (int)nextSize.Y);
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
        private void DrawGraph(int weeks)
        {
            DateTime endDate = gl.CurrentDate;
            DateTime[] dates = gl.getStockDates(selectedStock, endDate.AddDays(-7*weeks), endDate);

            decimal[][] data = gl.getStockDataByDay(selectedStock, endDate.AddDays(-7 * weeks), endDate);
            Color[] colours = { Color.PeachPuff, Color.Navy, Color.Green, Color.MonoGameOrange };
            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            float[] values = Graphing.initialiseGraph(spriteBatch, t, f_120, data, dates, graphHeight, graphWidth, graphXStart, graphYStart); //draw basics of graph and calculate actual plot area excluding margins, etc

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

            mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released) //check if mouse is pressed and is inside a button
            {
                if(leftArrow.Contains(mouseState.Position))
                {
                    if (!input)
                    {
                        stockNumber = (stockNumber + 1) % stocks.Count();
                    }
                    input = true;
                }
                else if (rightArrow.Contains(mouseState.Position))
                {
                    if (!input)
                    {
                        stockNumber = (stockNumber - 1 + stocks.Count()) % stocks.Count();
                    }
                    input = true;
                }
                else if(quantLeftArrow.Contains(mouseState.Position))
                {
                    if (!input)
                    {
                        if (quantity < 1)
                        {
                            quantity = 0;
                        }
                        else
                        {
                            quantity -= 1;
                        }
                    }
                    input = true;
                }
                else if(quantRightArrow.Contains(mouseState.Position))
                {
                    quantity += 1;
                }
                else if(buyButton.Contains(mouseState.Position))
                {
                    if (!input)
                    {
                        gl.buyStock(gl.currentDate, selectedStock, quantity);
                        quantity = 0;
                    }
                    input = true;
                }
                else if(sellButton.Contains(mouseState.Position))
                {
                    if (!input)
                    {
                        gl.sellStock(gl.currentDate, selectedStock, quantity);
                        quantity = 0;
                    }
                    input = true;
                }
                else if(nextDay.Contains(mouseState.Position))
                {
                    if (!input)
                    {
                        gl.incrementTime();
                        date = gl.currentDate.ToString("dd/MM/yyyy");
                    }
                    input = true;
                    
                }
                else if(walletBox.Contains(mouseState.Position))
                {
                    if (!input)
                    {
                        ScreenManager.AddScreen(new WalletScreen(gl));
                    }
                    input = true;
                }
                else
                {
                    input = false;
                }
            }
            else
            {
                input = false;
            }

            selectedStock = stocks[stockNumber];
            price = gl.getValues(selectedStock, gl.CurrentDate).close;

            base.Update(gameTime);
        }
    }
}
