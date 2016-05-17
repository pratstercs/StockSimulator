using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace StockSimulator
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MonoGame : Game
    {
        GameLogic gl;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        //Texture2D t;
        SpriteFont f_120;

        static int WINDOW_HEIGHT = 1920;
        static int WINDOW_WIDTH = 1080;

        GameState state = GameState.MainMenu;

        float tickerScroll = 0;

        /// <summary>
        /// Basic constructor
        /// </summary>
        public MonoGame()
        {
            //gl = new GameLogic();
            //gl.getData("JPM", "20150101", "20160101");

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.Window.Title = "StockSimulator";

            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.IsFullScreen = false;

            graphics.PreferMultiSampling = true;

            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            //add other stocks to exchange
            //string[] symbols = { "MSFT", "AAPL", "TSLA", "GOOG", "AMZN", "FB", "HPQ", "SYMC" };
            //foreach (string s in symbols)
            //{
            //    gl.getData(s, "20160401", "20160501");
            //}

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            f_120 = Content.Load<SpriteFont>("f_120");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            tickerScroll = (tickerScroll + 1) % WINDOW_WIDTH;

            base.Update(gameTime);

            switch(state)
            {
                case GameState.MainMenu:
                    break;
                case GameState.Sandbox:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Helper method for DrawString, using degrees instead of radians and ignoring unneeded parameters.
        /// </summary>
        /// <param name="sb">The SpriteBatch to use</param>
        /// <param name="font">The predefined font to use</param>
        /// <param name="text">The text to draw</param>
        /// <param name="origin">The point to start the text</param>
        /// <param name="color">The colour of the text</param>
        /// <param name="scale">The percentage scaling of the text</param>
        /// <param name="degrees">The angle to rotate in degrees</param>
        public static void DrawString(SpriteBatch sb, SpriteFont font, string text, Vector2 origin, Color color, float scale, int degrees)
        {
            float radians = (float)degrees * ((float)System.Math.PI / 180f);
            sb.DrawString(font, text, origin, color, radians, new Vector2(0, 0), scale, SpriteEffects.None, 1f);
        }

        /// <summary>
        /// Method to draw a string in the ticker
        /// </summary>
        /// <param name="sb">SpriteBatch</param>
        /// <param name="font">Spritefont to use - must include ▲ and ▼ (&#x25B2; and &#x25BC;)</param>
        /// <param name="text">A string array containing 0 - the symbol, and 1 - the percentage change</param>
        /// <param name="origin">The point to draw the string from</param>
        /// <returns>A float of the width of the text</returns>
        public static float DrawTickerString(SpriteBatch sb, SpriteFont font, string[] text, Vector2 origin)
        {
            string symbol = text[0];
            double value = System.Double.Parse(text[1]);
            string change = value.ToString("N2") + "%";

            float scale = 0.15f;

            float width = font.MeasureString(symbol).X * scale; //calculate width of symbol text

            Vector2 changeStart = new Vector2(
                origin.X + width,
                origin.Y
            ); //declare start location of change text

            Color colour;
            if(value > 0)
            {
                colour = Color.Lime;
                change = " ▲" + change;
            }
            else if(value < 0)
            {
                colour = Color.Red;
                change = " ▼" + change;
            }
            else
            {
                colour = Color.White;
                change = " =" + change;
            }

            width += font.MeasureString(change).X * scale; //add change text to width

            DrawString(sb, font, symbol, origin, Color.White, scale, 0);
            DrawString(sb, font, change, changeStart, colour, scale, 0);

            return width;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.LightGray);
            //spriteBatch.Begin();

            //Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(0, WINDOW_HEIGHT * 0.08f), new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT * 0.08f), 35);
            //Graphing.drawLine(t, spriteBatch, Color.Black, new Vector2(0, WINDOW_HEIGHT * 0.875f), new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT * 0.875f), 5);

            //gl.getAllChanges();
            //float offset = 0;
            //foreach (StockChange sc in gl.changes.Values)
            //{
            //    offset += 1.5f * DrawTickerString(spriteBatch, f_120, new string[] { sc.symbol, sc.change.ToString() }, new Vector2(width - (tickerScroll + offset), height * 0.085f));
            //}

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

            //spriteBatch.End();

            base.Draw(gameTime);

            switch(state)
            {
                case GameState.MainMenu:
                    drawMainMenu();
                    break;
                case GameState.Sandbox:
                    break;
                default:
                    break;
            }
        }

        private void drawMainMenu()
        {
            ///TODO: Move this to own class
            ///Main menu class - take in spriteBatch, etc as paramters
            ///global vars such as Rectangle CampaignButton
            ///set up during initialize
            ///use same rectangle objects in draw and update to avoid duplication
            
            GraphicsDevice.Clear(Color.LightGray);
            spriteBatch.Begin();

            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData(new[] { Color.Maroon });

            float start = (WINDOW_WIDTH - f_120.MeasureString("StockSimulator").X) / 2;

            DrawString(spriteBatch, f_120, "StockSimulator", new Vector2(start, WINDOW_HEIGHT * 0.15f), Color.Maroon, 1f, 0);

            float startPoint = WINDOW_WIDTH * 0.2f;
            float buttonAreaWidth = WINDOW_WIDTH * 0.6f;
            int buttonWidth = (int)(buttonAreaWidth * 0.2f);
            int buttonHeight = (int)(WINDOW_HEIGHT * 0.05f);
            int heightPoint = (int)(WINDOW_HEIGHT * 0.6f);

            string[] labels = { "Campaign", "Sandbox", "Load Game", "Exit Game" };

            for(int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(t, new Rectangle((int)startPoint, heightPoint, buttonWidth, buttonHeight), Color.Maroon);
                Vector2 textSize = f_120.MeasureString(labels[i]) / 10f;
                Vector2 textStart = new Vector2(((buttonWidth - textSize.X) / 2) + startPoint, ((buttonHeight - textSize.Y) / 2) + heightPoint);

                DrawString(spriteBatch, f_120, labels[i], textStart, Color.White, 0.1f, 0);

                startPoint += buttonWidth * 1.25f;
            }

            spriteBatch.End();
        }
    }

    public static class Graphing
    {
        /// <summary>
        /// Method to draw a line between two points
        /// </summary>
        /// <param name="t">The texture object to use</param>
        /// <param name="sb">The SpriteBatch</param>
        /// <param name="color">The line colour</param>
        /// <param name="start">The co-ordinates of the starting point</param>
        /// <param name="end">The co-ordinates of the ending point</param>
        public static void drawLine(Texture2D t, SpriteBatch sb, Color color, Vector2 start, Vector2 end, int width)
        {
            Vector2 edge = end - start;
            // calculate angle to rotate line
            float angle =
                (float)System.Math.Atan2(edge.Y, edge.X);


            sb.Draw(t,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(), //sprite will strech the texture to fill this rectangle
                    width), //width of line
                null,
                color,
                angle,     //angle of corner (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);
        }

        /// <summary>
        /// Method to draw lines between a specified set of points, i.e. a graph
        /// </summary>
        /// <param name="t">The Texture to use</param>
        /// <param name="sb">The SpriteBatch</param>
        /// <param name="color">The Colour for the lines</param>
        /// <param name="data">The co-ordinates of the points to graph</param>
        public static void drawGraph(GraphicsDevice g, SpriteBatch sb, Color color, Vector2[] data)
        {
            Vector2 prevPoint = new Vector2(data[0].X, data[0].Y); //set initial "previous point" to the first point in the dataset

            foreach (Vector2 point in data)
            {
                Texture2D t = new Texture2D(g, 1, 1);
                t.SetData(new[] { color });

                drawLine(t, sb, color, prevPoint, point, 3);
                prevPoint = point;
            }
        }

        /// <summary>
        /// Method to transform decimal stock prices into co-ordinates for graphing
        /// </summary>
        /// <param name="data">The data array to graph</param>
        /// <param name="height">The height of the graph area</param>
        /// <param name="width">The width of the graph area</param>
        /// <param name="Xstart">The X co-ordinate of the graph origin</param>
        /// <param name="Ystart">The Y co-ordinate of the graph origin</param>
        /// <returns>An array of co-ordinates of the graph points</returns>
        public static Vector2[] pointMaker(decimal[] data, float height, float width, float Xstart, float Ystart, float max, float min)
        {
            Vector2[] toReturn = new Vector2[data.Length]; //create array of vectors to store points in

            float[] floats = System.Array.ConvertAll(data, x => (float)x); //convert decimals to floats

            float space = (float)(width / (double)data.Length); //get horizontal spacing between points

            for (int i = 0; i < floats.Length; i++)
            {
                floats[i] -= min; //remove minimum from all values
                floats[i] /= (max - min); //divide all by converted max to get percentage of graph height
                floats[i] *= height; //multiply by graph height to get relative heights
                floats[i] += Ystart;

                toReturn[i] = new Vector2(
                    Xstart + (space * i), //start of graph + spacing for each point
                    floats[i]); //the proper height
            }

            return toReturn;
        }

        /// <summary>
        /// Method to draw the basics of the graph, such as gridlines and axis labels. Also calculates offsets and graph area co-ordinates
        /// </summary>
        /// <param name="sb">The SpriteBatch to use</param>
        /// <param name="t">The Texture2D object to use to draw the lines</param>
        /// <param name="font">The predefined font to use</param>
        /// <param name="data">The 2D decimal array containing all the stock data to graph</param>
        /// <param name="dates">The DateTime array containing the dates to form the bottom axis labels</param>
        /// <param name="height">The graph area height</param>
        /// <param name="width">The graph area width</param>
        /// <param name="Xstart">The x co-ordinate to start from</param>
        /// <param name="Ystart">The y co-ordinate to start from</param>
        /// <returns>A float array containing the graph area height, width, x-offset, y-offset, data max point, data min point</returns>
        public static float[] initialiseGraph(SpriteBatch sb, Texture2D t, SpriteFont font, decimal[][] data, System.DateTime[] dates, float height, float width, float Xstart, float Ystart)
        {
            string[][] labels = Utilities.axisLabeller(data, dates);
            decimal[] extremes = Utilities.getExtremes(data);

            int maxLeftWidth = (int)labels[0].Max(x => (font.MeasureString(x).X / 10f)); //get length of longest string
            int maxBottomHeight = labels[1].Max(x => pythagoreanShite((font.MeasureString(x) / 10f))); //get length of longest string

            float graphWidth = width - maxLeftWidth; //width of plot area (minus left label width)
            float graphHeight = height - maxBottomHeight; //height of plot area (minus bottom label height)

            float[] toReturn = { graphHeight, graphWidth, (width - graphWidth + Xstart), (Ystart), (float)extremes[1], (float)extremes[0] };

            drawMainMargins(sb, t, height, width, Xstart, Ystart);
            drawMinorMargins(sb, t, graphHeight, graphWidth, Xstart + maxLeftWidth, Ystart);
            drawGridlines(sb, t, graphHeight, Xstart + maxLeftWidth, Ystart, Xstart + width);

            drawLeftAxisLabels(sb, font, labels[0], graphHeight, Xstart, height-maxBottomHeight+Ystart);
            drawBottomAxisLabels(sb, font, labels[1], graphWidth, height+Ystart, (Xstart+maxLeftWidth));

            return toReturn;
        }

        /// <summary>
        /// Method to draw the margins surrounding the whole graph area
        /// </summary>
        /// <param name="sb">The SpriteBatch to use</param>
        /// <param name="t">The Texture2D object to use to draw the lines</param>
        /// <param name="height">The graph area height</param>
        /// <param name="width">The graph area width</param>
        /// <param name="Xstart">The x co-ordinate to start from</param>
        /// <param name="Ystart">The y co-ordinate to start from</param>
        private static void drawMainMargins(SpriteBatch sb, Texture2D t, float height, float width, float Xstart, float Ystart)
        {
            Vector2 origin = new Vector2(Xstart, Ystart + height);
            Vector2 leftTop = new Vector2(Xstart, Ystart);
            Vector2 rightTop = new Vector2(Xstart + width, Ystart);
            Vector2 rightBottom = new Vector2(Xstart + width, Ystart + height);

            drawLine(t, sb, Color.Black, origin, leftTop,2);
            drawLine(t, sb, Color.Black, leftTop, rightTop, 2);
            drawLine(t, sb, Color.Black, rightTop, rightBottom, 2);
            drawLine(t, sb, Color.Black, rightBottom, origin, 2);
        }

        /// <summary>
        /// Method to draw the axis lines
        /// </summary>
        /// <param name="sb">The SpriteBatch to use</param>
        /// <param name="t">The Texture2D object to use to draw the lines</param>
        /// <param name="height">The graph area height</param>
        /// <param name="width">The graph area width</param>
        /// <param name="Xstart">The x co-ordinate to start from</param>
        /// <param name="Ystart">The y co-ordinate to start from</param>
        private static void drawMinorMargins(SpriteBatch sb, Texture2D t, float height, float width, float Xstart, float Ystart)
        {
            Vector2 origin = new Vector2(Xstart, Ystart + height);
            Vector2 leftTop = new Vector2(Xstart, Ystart);
            Vector2 rightTop = new Vector2(Xstart + width, Ystart);
            Vector2 rightBottom = new Vector2(Xstart + width, Ystart + height);

            drawLine(t, sb, Color.Black, origin, leftTop, 2);
            drawLine(t, sb, Color.Black, leftTop, rightTop, 2);
            drawLine(t, sb, Color.Black, rightTop, rightBottom, 2);
            drawLine(t, sb, Color.Black, rightBottom, origin, 2);
        }

        /// <summary>
        /// Method to draw the grid lines every 1/4 of the graph area
        /// </summary>
        /// <param name="sb">The SpriteBatch to use</param>
        /// <param name="t">The Texture2D object to use to draw the lines</param>
        /// <param name="height">The graph area height</param>
        /// <param name="Xstart">The x co-ordinate to start from</param>
        /// <param name="Ystart">The y co-ordinate to start from</param>
        /// <param name="rightEdge"></param>
        private static void drawGridlines(SpriteBatch sb, Texture2D t, float height, float Xstart, float Ystart, float rightEdge)
        {
            float spacing = height / 4;

            for(int i = 1; i < 4; i++)
            {
                float yPos = Ystart + (i * spacing);
                drawLine(t, sb, Color.Gray, new Vector2(Xstart, yPos), new Vector2(rightEdge, yPos), 2);
            }
        }

        /// <summary>
        /// Method to draw the axis labels on the left hand side of a graph
        /// </summary>
        /// <param name="sb">The SpriteBatch to use</param>
        /// <param name="font">The predefined font to use</param>
        /// <param name="labels">The string array of the label text</param>
        /// <param name="height">The graph area height</param>
        /// <param name="Xstart">The x co-ordinate to start from</param>
        /// <param name="Ystart">The y co-ordinate to start from</param>
        private static void drawLeftAxisLabels(SpriteBatch sb, SpriteFont font, string[] labels, float height, float Xstart, float Ystart)
        {
            float spacing = height / 10;

            for(int i = 0; i < 11; i++)
            {
                MonoGame.DrawString(sb, font, labels[i], new Vector2(Xstart, (Ystart - (spacing * i))), Color.Black, 0.1f, 0);
            }
        }

        /// <summary>
        /// Method to draw the axis labels on the bottom of a graph
        /// </summary>
        /// <param name="sb">The SpriteBatch to use</param>
        /// <param name="font">The predefined font to use</param>
        /// <param name="labels">The string array of the label text</param>
        /// <param name="width">The graph area width</param>
        /// <param name="height">The bottom of the graph area, aka, where to draw the labels</param>
        /// <param name="Xstart">The x (horizontal) co-ord to start from</param>
        private static void drawBottomAxisLabels(SpriteBatch sb, SpriteFont font, string[] labels, float width, float height, float Xstart)
        {
            float spacing = width / 10;

            for (int i = 0; i < 11; i++)
            {
                MonoGame.DrawString(sb, font, labels[i], new Vector2(Xstart + (spacing * i), height), Color.Black, 0.1f, -45);
            }
        }

        /// <summary>
        /// Text will be rotated -45°, so Pythagoras to the rescue to calculate the height!
        /// </summary>
        /// <param name="v">The text to calculate the rotated height of</param>
        /// <returns>An int representing the height of the text should it be rotated</returns>
        private static int pythagoreanShite(Vector2 v)
        {
            return (int)System.Math.Sqrt(System.Math.Pow(v.X,2) + System.Math.Pow(v.Y, 2));
        }
    }

    enum GameState
    {
        MainMenu,
        Sandbox
    }
}
