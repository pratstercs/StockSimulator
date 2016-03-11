using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StockSimulator
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MonoGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D t;

        public MonoGame()
        {
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
            t = new Texture2D(GraphicsDevice, 1, 1);
            //t.SetData<Color>(new Color[] { Color.Red });

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
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            decimal[] data = { 53.91M, 57.57M, 58.6M, 59.25M, 58.86M, 58.17M, 58.88M, 58.09M, 56.21M, 57.03M, 58.11M, 57.605M, 59.2M, 59.9M, 59.96M, 60.55M, 60.05M, 59.5M, 59.54M, 59.67M };
            Vector2[] points = Utilities.pointMaker(data, 600, 800, 0, 0);
            Graphing.drawGraph(t, spriteBatch, Color.Lime, points);
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
        public static void drawLine(Texture2D t, SpriteBatch sb, Color color, Vector2 start, Vector2 end)
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
                    (int)edge.Length(), //sb will strech the texture to fill this rectangle
                    3), //width of line, change this to make thicker line
                null,
                color,
                angle,     //angle of line (calulated above)
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
            Vector2 prevPoint = new Vector2(data[0].X, data[0].Y);

            foreach (Vector2 point in data)
            {
                drawLine(t, sb, color, prevPoint, point);
                prevPoint = point;
            }
        }
    }
}
