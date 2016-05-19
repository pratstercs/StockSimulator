using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace StockSimulator
{
    class WalletScreen : GameScreen
    {
        SpriteBatch spriteBatch = ScreenManager.spriteBatch;
        SpriteFont f_120 = ScreenManager.f_120;
        SpriteFont f_30 = ScreenManager.f_30;
        GraphicsDevice GraphicsDevice = ScreenManager.graphicsDevice;
        MouseState mouseState;

        public static int WINDOW_HEIGHT = ScreenManager.WINDOW_HEIGHT;
        public static int WINDOW_WIDTH = ScreenManager.WINDOW_WIDTH;

        float dateCol, nameCol, priceCol, amtCol, valCol;
        float dateStart, nameStart, priceStart, amtStart, valStart;
        float textHeight;

        Rectangle exit;

        GameLogic gl;

        public WalletScreen(GameLogic g)
        {
            gl = g;
        }

        //longest name: Carnival Corp. Paired Ctf 1 Com Carnival Corp & 1 Tr Sh

        public override void LoadAssets()
        {
            BackgroundColor = Color.White;

            textHeight = f_30.MeasureString("A").Y * 0.75f;

            dateCol = WINDOW_WIDTH * 0.2f;
            nameCol = WINDOW_WIDTH * 0.3f;
            priceCol = WINDOW_WIDTH * 0.15f;
            amtCol = WINDOW_WIDTH * 0.1f;
            valCol = WINDOW_WIDTH * 0.25f;

            dateStart = 0;
            nameStart = dateCol;
            priceStart = nameStart + nameCol;
            amtStart = priceStart + priceCol;
            valStart = amtStart + amtCol;

            base.LoadAssets();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            Texture2D t = new Texture2D(ScreenManager.graphicsDevice, 1, 1);
            t.SetData(new Color[] { Color.Black });
            Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(dateStart, 0), new Vector2(dateStart, WINDOW_HEIGHT), 1);
            Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(nameStart, 0), new Vector2(nameStart, WINDOW_HEIGHT), 1);
            Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(priceStart, 0), new Vector2(priceStart, WINDOW_HEIGHT), 1);
            Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(amtStart, 0), new Vector2(amtStart, WINDOW_HEIGHT), 1);
            Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(valStart, 0), new Vector2(valStart, WINDOW_HEIGHT), 1);

            //Headers
            //offset + ((width - textWidth.X)/2)
            Vector2 dateHead = new Vector2(dateStart + ((dateCol - f_30.MeasureString("Date").X) /2), 0);
            Vector2 nameHead = new Vector2(nameStart + ((nameCol - f_30.MeasureString("Company").X) / 2), 0);
            Vector2 priceHead = new Vector2(priceStart + ((priceCol - f_30.MeasureString("Price").X) / 2), 0);
            Vector2 amountHead = new Vector2(amtStart + ((amtCol - f_30.MeasureString("Amount").X) / 2), 0);
            Vector2 valueHead = new Vector2(valStart + ((valCol - f_30.MeasureString("Total").X) / 2), 0);

            Graphing.DrawString(spriteBatch, f_30, "Date", dateHead, Color.Navy, 0.75f, 0);
            Graphing.DrawString(spriteBatch, f_30, "Name", nameHead, Color.Navy, 0.75f, 0);
            Graphing.DrawString(spriteBatch, f_30, "Price", priceHead, Color.Navy, 0.75f, 0);
            Graphing.DrawString(spriteBatch, f_30, "Amount", amountHead, Color.Navy, 0.75f, 0);
            Graphing.DrawString(spriteBatch, f_30, "Total", valueHead, Color.Navy, 0.75f, 0);

            float currentHeight = textHeight * 1.1f;
            //Elements
            foreach(Stock x in gl.wallet)
            {
                string date = x.purchaseDate.ToString("dd/MM/yyyy");
                decimal price = x.purchasePrice;
                decimal amount = x.amount;

                string fullName = gl.getName(x.symbol) + " (" + x.symbol + ")";
                string priceStr = "$" + price.ToString("N2");
                string amtStr = amount.ToString("N0");
                string valStr = "$" + (price * amount).ToString("N2");

                float fullNameScale = 0.75f;
                float fullNameWidth = f_30.MeasureString(fullName).X * 0.75f;
                if(fullNameWidth > nameCol) //handling too long company names
                {
                    fullNameScale = nameCol / fullNameWidth * 0.75f;
                }

                Vector2 dateS = new Vector2(dateStart + ((dateCol - f_30.MeasureString(date).X) / 2), currentHeight);
                Vector2 nameS = new Vector2(nameStart + ((nameCol - f_30.MeasureString(fullName).X) / 2), currentHeight);
                Vector2 priceS = new Vector2(priceStart + ((priceCol - f_30.MeasureString(priceStr).X) / 2), currentHeight);
                Vector2 amountS = new Vector2(amtStart + ((amtCol - f_30.MeasureString(amtStr).X) / 2), currentHeight);
                Vector2 valueS = new Vector2(valStart + ((valCol - f_30.MeasureString(valStr).X) / 2), currentHeight);

                Graphing.DrawString(spriteBatch, f_30, date, dateS, Color.Black, 0.75f, 0);
                Graphing.DrawString(spriteBatch, f_30, fullName, nameS, Color.Black, fullNameScale, 0);
                Graphing.DrawString(spriteBatch, f_30, priceStr, priceS, Color.Black, 0.75f, 0);
                Graphing.DrawString(spriteBatch, f_30, amtStr, amountS, Color.Black, 0.75f, 0);
                Graphing.DrawString(spriteBatch, f_30, valStr, valueS, Color.Black, 0.75f, 0);

                currentHeight += textHeight * 1.1f;
            }

            //Close
            Vector2 exitSize = f_30.MeasureString("X");
            exit = new Rectangle((int)(WINDOW_WIDTH - exitSize.X), 0, (int)exitSize.X, (int)exitSize.Y);
            Graphing.DrawString(spriteBatch, f_30, "X", new Vector2(WINDOW_WIDTH - exitSize.X, 0), Color.Red, 1f, 0);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed) //check if mouse is pressed and is inside a button
            {
                if(exit.Contains(mouseState.Position))
                {
                    ScreenManager.RemoveScreen(this);   
                }
            }

                base.Update(gameTime);
        }
    }
}
