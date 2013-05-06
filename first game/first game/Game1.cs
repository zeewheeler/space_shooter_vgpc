using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace first_game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public struct Projectile
        {
            public Texture2D texture;
            public Vector2 position;
            public Vector2 velocity;
            public bool isAlive;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
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


        //Global Varibles

        Vector2 player_ship_position;
        Texture2D player_ship_texture;

        Vector2   enemy_ship_position;
        Texture2D enemy_ship_texture;
        Vector2   enemy_ship_velocity;
        bool      enemy_ship_IsAlive;

        Vector2 background_location;
        Texture2D background_texture;

        Projectile single_projectile;


        Rectangle viewportRect;

        SoundEffect fire_laser_sound;

        KeyboardState previousKeyboardState = Keyboard.GetState();

        const int maxProjectiles = 1000;

        //multiple projectiles
        Projectile[] projectile_array;
        


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //initialize player ship texture and location
            player_ship_texture = Content.Load<Texture2D>("terran_bc");

            fire_laser_sound = Content.Load<SoundEffect>("sounds\\ship bang");

            player_ship_position.X = graphics.GraphicsDevice.Viewport.Width / 2;
            player_ship_position.Y = graphics.GraphicsDevice.Viewport.Height / 2;

            //load enemy ship texture, set position and velocity

            enemy_ship_texture = Content.Load<Texture2D>("enemy");

            enemy_ship_position.X = graphics.GraphicsDevice.Viewport.Width;
            enemy_ship_position.Y = graphics.GraphicsDevice.Viewport.Height / 2;

            enemy_ship_IsAlive = true;

            enemy_ship_velocity.X = -5;
            enemy_ship_velocity.Y = 0;



            //load background texture and location
            background_texture = Content.Load<Texture2D>("space0000");
            background_location.X = 0;
            background_location.Y = 0;

            single_projectile.texture = Content.Load<Texture2D>("SuperShot");

           projectile_array = new Projectile[maxProjectiles];

           for (int i = 0; i < maxProjectiles; i++)
           {
               projectile_array[i].texture = Content.Load<Texture2D>("superFireBall");
               
               projectile_array[i].position.X = -10000;
               projectile_array[i].position.Y = -10000;

               projectile_array[i].velocity.Y = 0;
               projectile_array[i].velocity.X = 5;
               projectile_array[i].isAlive = false;
           }



           viewportRect = new Rectangle(0, 0,
                graphics.GraphicsDevice.Viewport.Width,
                graphics.GraphicsDevice.Viewport.Height);

            

            // TODO: use this.Content to load your game content here
        }

        public void Update_player_projectiles()
        {
            Rectangle enemy_bounding_box = new Rectangle((int)enemy_ship_position.X,
                (int)enemy_ship_position.Y, enemy_ship_texture.Width, enemy_ship_texture.Height);

            
            foreach(Projectile proj in projectile_array)
            {
                if (proj.isAlive)
                {
                    Rectangle proj_bounding_box = new Rectangle((int)proj.position.X, (int)proj.position.Y,
                        proj.texture.Width, proj.texture.Height);

                    if(enemy_bounding_box.Intersects(proj_bounding_box))
                    {
                        enemy_ship_IsAlive = false;
                       
                    }
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            KeyboardState keyboardstate = Keyboard.GetState();
            
            //exit game if  escape key is down
            if (keyboardstate.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            if (keyboardstate.IsKeyDown(Keys.Up))
            {
                player_ship_position.Y -= 5;
            }

            if (keyboardstate.IsKeyDown(Keys.Down))
            {
                player_ship_position.Y += 5;
            }

            if (keyboardstate.IsKeyDown(Keys.Left))
            {
                player_ship_position.X -= 5;
            }

            if (keyboardstate.IsKeyDown(Keys.Right))
            {
                player_ship_position.X += 5;
            }

            if (keyboardstate.IsKeyDown(Keys.Enter))
            {
                player_ship_position.X = graphics.GraphicsDevice.Viewport.Width / 2;
                player_ship_position.Y = graphics.GraphicsDevice.Viewport.Height / 2;
            }


            if (keyboardstate.IsKeyDown(Keys.Space) &&
                previousKeyboardState.IsKeyUp(Keys.Space) )
            {
                for (int i = 0; i < maxProjectiles; i++)
                {
                    if (!projectile_array[i].isAlive)
                    {
                        projectile_array[i].position.X = player_ship_position.X;
                        projectile_array[i].position.Y = player_ship_position.Y;
                        projectile_array[i].isAlive = true;
                        fire_laser_sound.Play();
                        break;
                    }
                }
            }




            //Update projectiles

            for (int i = 0; i < maxProjectiles; i++)
            {
                projectile_array[i].position.X += projectile_array[i].velocity.X;
                projectile_array[i].position.Y += projectile_array[i].velocity.Y;
            }

            //call projectile collision detection function
            Update_player_projectiles();

            //update enemy ship
            enemy_ship_position.X += enemy_ship_velocity.X;
            enemy_ship_position.Y += enemy_ship_velocity.Y;

            if (enemy_ship_position.X < 0)
            {
                enemy_ship_position.X = graphics.GraphicsDevice.Viewport.Width;
                enemy_ship_IsAlive = true;
            }

         

            


            previousKeyboardState = keyboardstate;

            

            // TODO: Add your update logic here

            base.Update(gameTime);
        }


         
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            spriteBatch.Draw(background_texture, viewportRect, Color.White);

            for (int i = 0; i < maxProjectiles; i++)
            {
                spriteBatch.Draw(projectile_array[i].texture, projectile_array[i].position, Color.White);
            }

            if (enemy_ship_IsAlive)
            {
                spriteBatch.Draw(enemy_ship_texture, enemy_ship_position, Color.White);
            }

            spriteBatch.Draw(player_ship_texture, player_ship_position, Color.White);

            spriteBatch.End();

            // TODO: Add your drawing code here
             
            base.Draw(gameTime);
        }
    }
}
