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
        SpriteFont font;

        public MonoGame()
        {
            gl = new GameLogic();
            gl.getData("JPM", "20150101", "20160101");

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 800;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 600;   // set this value to the desired height of your window
            this.Window.Title = "StockSimulator";

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
            // TODO: Add your initialization logic here

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
            font = Content.Load<SpriteFont>("font");

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

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// Helper method for DrawString, using degrees instead of radians and ignoring unneeded parameters.
        /// </summary>
        /// <param name="sb">The SpriteBatch to use</param>
        /// <param name="font">The predefined font to use</param>
        /// <param name="text">The text to draw</param>
        /// <param name="origin">The point to start the text</param>
        /// <param name="color">The colour of the text</param>
        /// <param name="degrees">The angle to rotate in degrees</param>
        public static void DrawString(SpriteBatch sb, SpriteFont font, string text, Vector2 origin, Color color, int degrees)
        {
            float radians = (float)degrees * ((float)System.Math.PI / 180f);
            sb.DrawString(font, text, origin, color, radians, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            decimal[][] data = gl.getStockDataByDay("JPM", new System.DateTime(2015, 1, 1), new System.DateTime(2016, 1, 1));
            Color[] colours = { Color.Yellow, Color.Blue, Color.Lime, Color.OrangeRed };

            float[] values = Graphing.initialiseGraph(spriteBatch, data, 600, 800, 0, 0); //draw basics of graph and calculate actual plot area excluding margins, etc

            for(int i = 0; i < 4; i++)
            {
                Vector2[] points = Graphing.pointMaker(data[i], values[0], values[1], (int)values[2], (int)values[3], values[4], values[5]);
                //Vector2[] points = Graphing.pointMaker(data[i], 600, 800, 0, 0);
                Graphing.drawGraph(new Texture2D(GraphicsDevice, 1, 1), spriteBatch, colours[i], points);
            }

            DrawString(spriteBatch, font, "Test", new Vector2(100, 100), Color.Black, -45);

            spriteBatch.End();

            base.Draw(gameTime);
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
            t.SetData<Color>(new Color[] { color });

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
        public static void drawGraph(Texture2D t, SpriteBatch sb, Color color, Vector2[] data)
        {
            Vector2 prevPoint = new Vector2(data[0].X, data[0].Y); //set initial "previous point" to the first point in the dataset


            //draw each data line
            foreach (Vector2 point in data)
            {
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
        public static Vector2[] pointMaker(decimal[] data, float height, float width, int Xstart, int Ystart, float max, float min)
        {
            Vector2[] toReturn = new Vector2[data.Length]; //create array of vectors to store points in

            float[] floats = System.Array.ConvertAll(data, x => (float)x); //convert decimals to floats

            //float max = floats.Max(); //get the maximum value
            //float min = floats.Min(); //get the minimum value

            height *= 0.95f; //set graph to use 95% of the vertical space to add padding

            float space = (float)(width / (double)data.Length); //get horizontal spacing between points

            for (int i = 0; i < floats.Length; i++)
            {
                floats[i] -= min; //remove minimum from all values
                floats[i] /= (max - min); //divide all by converted max to get percentage of graph height
                floats[i] *= height; //multiply by graph height to get relative heights
                floats[i] = height - floats[i]; //flip graph (pixel numbering 0,0 is top left)
                floats[i] += Xstart + (height * 0.025f); //add graph start offset and add vertical padding

                toReturn[i] = new Vector2(
                    Ystart + (space * i), //start of graph + spacing for each point
                    floats[i]); //the proper height
            }

            return toReturn;
        }

        public static float[] initialiseGraph(SpriteBatch sb, decimal[][] data, float height, float width, int Xstart, int Ystart)
        {
            //vertical axis labels

            //horizontal axis labels

            //draw horizontal margins

            //draw vertical margins
            //draw midpoints
            //float[] toReturn = new float[5];
            float[] toReturn = { 600f, 800f, 0f, 0f, 0f, 0f};
            return toReturn;
        }
    }
}
