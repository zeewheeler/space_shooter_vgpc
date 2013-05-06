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
            public Texture2D    texture;
            public Vector2      position;
            public Vector2      velocity;
            public bool         is_alive;
        }

        public struct Enemy_ship
        {
            public Texture2D  texture;
            public Vector2    position;
            public Vector2    velocity;
            public bool       is_alive;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //define what screen resolution the game should run in
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

        Vector2     player_ship_position;
        Texture2D   player_ship_texture;

        Vector2     background_location;
        Texture2D   background_texture;

        Rectangle   viewportRect;
        SoundEffect fire_laser_sound;

        KeyboardState previousKeyboardState = Keyboard.GetState();

        const int   max_projectiles                 = 1000;
        const int   max_enemy_ships                 = 100;
        const int   max_enemies_on_screen           = 3;
        int         current_enemy_ships_on_screen   = 0;

        //multiple projectiles
        Projectile[] projectile_array;
        Enemy_ship[] enemy_ship_array;

        //create a random class instantiation so we can create random numbers
        static Random random = new Random();
        


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

            fire_laser_sound = Content.Load<SoundEffect>("sounds\\lazer");

            player_ship_position.X = graphics.GraphicsDevice.Viewport.Width / 2;
            player_ship_position.Y = graphics.GraphicsDevice.Viewport.Height / 2;


            //load background texture and location
            background_texture = Content.Load<Texture2D>("space0000");
            background_location.X = 0;
            background_location.Y = 0;

            
            //intialize projectile array
            projectile_array = new Projectile[max_projectiles];
            for (int i = 0; i < max_projectiles; i++)
            {
                projectile_array[i].texture = Content.Load<Texture2D>("cannonball");
               
                projectile_array[i].position.X = -10000;
                projectile_array[i].position.Y = -10000;

                projectile_array[i].velocity.Y = 0;
                projectile_array[i].velocity.X = 15;
                projectile_array[i].is_alive = false;
            }

            enemy_ship_array = new Enemy_ship[max_enemy_ships];
            for (int i = 0; i < max_enemy_ships; i++)
            {
                //assign texture 
                enemy_ship_array[i].texture = Content.Load<Texture2D>("enemy");

                /*assign position, it does not matter what it is because it will start off with alive = false
                 * and the position will be set in the function that "spawns" new enemies*/
                enemy_ship_array[i].position.X = 0;
                enemy_ship_array[i].position.Y = 0;

                //assign is_alive = false. Enemies will start off "dead", the "spawn" function will set them to alive
                enemy_ship_array[i].is_alive = false;

                //assign velocity
                enemy_ship_array[i].velocity.Y = 0;
                enemy_ship_array[i].velocity.X = -5;

            }




           viewportRect = new Rectangle(0, 0,
                graphics.GraphicsDevice.Viewport.Width,
                graphics.GraphicsDevice.Viewport.Height);

            

            // TODO: use this.Content to load your game content here
        }

        public void Update_player_projectiles()
        {
           
            //loop through all enemy ships
            for (int i = 0; i < max_enemy_ships; i++)
            {

              
                //create a bounding_box(hitbox) around this particular enemy ship in the array([i])
                Rectangle enemy_bounding_box = new Rectangle((int)enemy_ship_array[i].position.X,
                    (int)enemy_ship_array[i].position.Y, enemy_ship_array[i].texture.Width, enemy_ship_array[i].texture.Height);

                //loop through all player projectiles
                for (int j = 0; j < max_projectiles; j++)
                {
                    //create a bounding_Box(hitbox) around this particlar player projectile in the array([j])
                    Rectangle proj_bounding_box = new Rectangle((int)projectile_array[j].position.X, (int)projectile_array[j].position.Y,
                            projectile_array[j].texture.Width, projectile_array[j].texture.Height);

                    //check if the two hitboxes intersect each other, if so, it's a hit
                    if (enemy_bounding_box.Intersects(proj_bounding_box))
                    {
                        //set enemy ship[i] and project[j] is_alive to false
                        enemy_ship_array[i].is_alive = false;
                        projectile_array[j].is_alive = false;
                        current_enemy_ships_on_screen--;
                        
                    }
                }
            }

            for (int i = 0; i < max_projectiles; i++)
            {
                projectile_array[i].position.X += projectile_array[i].velocity.X;
                projectile_array[i].position.Y += projectile_array[i].velocity.Y;
            }
        }

        public void spawn_enemy_ship()
        {
            
            
            if (current_enemy_ships_on_screen < max_enemies_on_screen)
            {
                //find a dead enemy ship in the array, then set it's is alive to true and set an random position
                for (int i = 0; i < max_enemy_ships; i++)
                {
                    if (enemy_ship_array[i].is_alive == false)
                    {
                        //set X position to the right of the screen
                        enemy_ship_array[i].position.X = viewportRect.Width;
                        
                        /* set Y position to a random position that is still on the screen. Lerp stands for "Linear Interpolation"
                         * it just takes the random number and converts it into a number that is between the top and bottom of the y-axis */
                        enemy_ship_array[i].position.Y = MathHelper.Lerp((float)viewportRect.Top - 20,
                            (float)viewportRect.Bottom -50, (float)random.NextDouble());

                        //don't forget to set enemy ship is_alive to true now!
                        enemy_ship_array[i].is_alive = true;
                        

                        
                        //break out of loop since we only want to create 1 bad guy per call to this function
                        break;
                    }
                }
            }
        }

        public void update_enemy_ship_count()
        {
            current_enemy_ships_on_screen = 0;
            for(int i = 0; i < max_enemy_ships; i++)
            {
                 Rectangle enemy_bounding_box = new Rectangle((int)enemy_ship_array[i].position.X,
                    (int)enemy_ship_array[i].position.Y, enemy_ship_array[i].texture.Width, enemy_ship_array[i].texture.Height);

                 if (viewportRect.Intersects(enemy_bounding_box) && enemy_ship_array[i].is_alive)
                 {
                     current_enemy_ships_on_screen++;
                 }
            }
        }

        public void handle_input()
        {
            KeyboardState keyboardstate = Keyboard.GetState();

            //exit game if  escape key is down
            if (keyboardstate.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            if (keyboardstate.IsKeyDown(Keys.Up))
            {
                if (player_ship_position.Y - 5 > viewportRect.Top)
                {
                    player_ship_position.Y -= 10;
                }
            }

            if (keyboardstate.IsKeyDown(Keys.Down))
            {
                if (player_ship_position.Y + 5 < (viewportRect.Bottom - player_ship_texture.Height ))
                {
                    player_ship_position.Y += 10;
                }
            }

            if (keyboardstate.IsKeyDown(Keys.Left))
            {
                if (player_ship_position.X - 5 > 0)
                {
                    player_ship_position.X -= 10;
                }
            }

            if (keyboardstate.IsKeyDown(Keys.Right))
            {
                if (player_ship_position.X + 5 < viewportRect.Right - player_ship_texture.Width)
                {
                    player_ship_position.X += 10;
                }
            }

            if (keyboardstate.IsKeyDown(Keys.Enter))
            {
                player_ship_position.X = graphics.GraphicsDevice.Viewport.Width / 2;
                player_ship_position.Y = graphics.GraphicsDevice.Viewport.Height / 2;
            }


            if (keyboardstate.IsKeyDown(Keys.Space) &&
                previousKeyboardState.IsKeyUp(Keys.Space))
            {
                for (int i = 0; i < max_projectiles; i++)
                {
                    if (!projectile_array[i].is_alive)
                    {
                        projectile_array[i].position.X = player_ship_position.X + (player_ship_texture.Width);
                        projectile_array[i].position.Y = player_ship_position.Y +(0.5f * player_ship_texture.Height);
                        projectile_array[i].is_alive = true;
                        fire_laser_sound.Play();
                        break;
                    }
                }
            }

            previousKeyboardState = keyboardstate;
        }

        public void update_enemy_ship()
        {
            for (int i = 0; i < max_enemy_ships; i++)
            {
                enemy_ship_array[i].position.X += enemy_ship_array[i].velocity.X;
                enemy_ship_array[i].position.X += enemy_ship_array[i].velocity.Y;

                //if the enemy ship is off the screen to the left, set alive = false
                if (enemy_ship_array[i].position.X < 0 && enemy_ship_array[i].is_alive == true)
                {
                    enemy_ship_array[i].is_alive = false;
                }
            }
        }
        
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

       
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
       
        protected override void Update(GameTime gameTime)
        {
            //Update projectiles
            Update_player_projectiles();

            //scan and act upon keyboard input
            handle_input();

            //update enemy ship
            update_enemy_ship();
         
            //call our custom spawn enemy function, it will keep max_enemies_on_screen worth of enemies on the screen
            spawn_enemy_ship();
         
            //update the current number of enemyes on the screen using our custom function
            update_enemy_ship_count();
      
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

            for (int i = 0; i < max_projectiles; i++)
            {
                if (projectile_array[i].is_alive)
                {
                    spriteBatch.Draw(projectile_array[i].texture, projectile_array[i].position, Color.White);
                }
            }

            for (int i = 0; i < max_enemy_ships; i++)
            {
                if (enemy_ship_array[i].is_alive)
                {
                    spriteBatch.Draw(enemy_ship_array[i].texture, enemy_ship_array[i].position, Color.White);
                }
            }


            spriteBatch.Draw(player_ship_texture, player_ship_position, Color.White);

            spriteBatch.End();

            // TODO: Add your drawing code here
             
            base.Draw(gameTime);
        }
    }
}
