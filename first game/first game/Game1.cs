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
            public      Texture2D   texture;
            public      Vector2     position;
            public      Vector2     velocity;
            public      bool        is_alive;
        }

        public struct Enemy_ship
        {
            public      Texture2D   texture;
            public      Vector2     position;
            public      Vector2     velocity;
            public      bool        is_alive;
            public      int         health;
            public      int         cash_value;
        }

        public struct player_state_vars
        {
            //player variables
            public int      player_health;
            public int      player_lives;
            public int      player_cash;

            public int      standard_weapon_damage;
            public bool     fire_spread;
        }

        public struct game_state_vars
        {
            public      int         game_level;
            public      bool        is_paused;
            public      int         enemies_blown_up;
            public      bool        game_OVER;
            

        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //define what screen resolution the game should run in
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            explosion = new ExplosionParticleSystem(this, 3);
            Components.Add(explosion);

            // but the smoke from the explosion lingers a while.
            smoke = new ExplosionSmokeParticleSystem(this, 1);
            Components.Add(smoke);

            // we'll see lots of these effects at once; this is ok
            // because they have a fairly small number of particles per effect.
            smokePlume = new SmokePlumeParticleSystem(this, 4);
            Components.Add(smokePlume);
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

        //support for particle system objects
        ExplosionParticleSystem explosion;
        ExplosionSmokeParticleSystem smoke;
        SmokePlumeParticleSystem smokePlume;

       

       

      


        Vector2     player_ship_position;
        Texture2D   player_ship_texture;

        Vector2     background_location;
        Texture2D   background_texture;

        Rectangle   viewportRect;
        SoundEffect fire_laser_sound;
        SoundEffect ship_bang;


        player_state_vars player_state;
        game_state_vars game_state;

        KeyboardState previousKeyboardState = Keyboard.GetState();


        

        const int   max_projectiles                 = 1000;
        const int   max_enemy_ships                 = 100;
        const int   max_enemies_on_screen           = 3;
        int         current_enemy_ships_on_screen   = 0;
        int         damage_upgrade_levels           = 0;

        //Game "tweaking" varibles. These are important, by changing their variables around you can
        //"tune" the game to make it more or less challenging. Find a good balance, and your game will be fun!
        const int   enemy_health_per_level          = 10;
        const int   base_kills_per_level            = 5;
        const float cash_worth_multipler            = 1.0f;

        const int   starting_player_health          = 100;
        const int   starting_player_lifes           = 3;
        const int   starting_player_standard_damage = 10;
        const float enemy_damage_multiplier         = 1.0f;
        
        const int fire_spread_upgrade_cost          = 50;
        const int fire_damage_upgrade_cost          = 10;
       

       
       

        //multiple projectiles
        Projectile[] projectile_array;
        Enemy_ship[] enemy_ship_array;

        //create a random class instantiation so we can create random numbers
        private static Random random = new Random();
        public static Random Random
        {
            get { return random; }
        }

        public static float RandomBetween(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        SpriteFont arialFont;

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }
        


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //load font
            arialFont = Content.Load<SpriteFont>("Arial");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //initialize player ship texture and location
            player_ship_texture = Content.Load<Texture2D>("terran_bc");

            fire_laser_sound    = Content.Load<SoundEffect>("sounds\\lazer");
            ship_bang           = Content.Load<SoundEffect>("sounds\\ship bang");

            player_ship_position.X = graphics.GraphicsDevice.Viewport.Width / 2;
            player_ship_position.Y = graphics.GraphicsDevice.Viewport.Height / 2;


            //load background texture and location
            background_texture = Content.Load<Texture2D>("space0000");
            background_location.X = 0;
            background_location.Y = 0;

            //initialize player state
            player_state.player_cash                = 0;
            player_state.player_health              = starting_player_health;
            player_state.player_lives               = starting_player_lifes;
            player_state.standard_weapon_damage     = starting_player_standard_damage;
            player_state.fire_spread                = false;

            //initialize game state
            game_state.is_paused                    = true;
            game_state.game_level                   = 1;
            game_state.game_OVER                    = false;

            
            //intialize projectile array
            projectile_array = new Projectile[max_projectiles];
            for (int i = 0; i < max_projectiles; i++)
            {
                projectile_array[i].texture = Content.Load<Texture2D>("cannonball");
               
                projectile_array[i].position.X = -10000;
                projectile_array[i].position.Y = -10000;

                projectile_array[i].velocity.Y = 0;
                projectile_array[i].velocity.X = 30;
                projectile_array[i].is_alive = false;
            }

            enemy_ship_array = new Enemy_ship[max_enemy_ships];
            for (int i = 0; i < max_enemy_ships; i++)
            {
                //assign texture 
                enemy_ship_array[i].texture = Content.Load<Texture2D>("enemy");
                
                //assign is_alive = false. Enemies will start off "dead", the "spawn" function will set them to alive
                enemy_ship_array[i].is_alive = false;

            

            }



            //creates a rectangle exactly the size of the screen. This is useful if you don't want to have to type out
            // "graphics.GraphicsDevice.Viewport." every time you want to access the viewport width or height.
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
                    if (enemy_bounding_box.Intersects(proj_bounding_box) &&
                        enemy_ship_array[i].is_alive)
                    {
                        //remove projectile, it's used up
                        projectile_array[j].is_alive = false;

                        //decrement enemy life by the amount the weapon does, in the case, it is the "standard_weapon
                        //damage
                        enemy_ship_array[i].health -= player_state.standard_weapon_damage +
                            (damage_upgrade_levels * player_state.standard_weapon_damage);

                       
                      
                       
                        //enemy_ships_blown_up++;
                        //explosion.AddParticles(enemy_ship_array[i].position);
                        //smoke.AddParticles(enemy_ship_array[i].position);
                        
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
                        enemy_ship_array[i].position.Y = MathHelper.Lerp((float)viewportRect.Top + 50,
                            (float)viewportRect.Bottom -50, (float)random.NextDouble());

                        //don't forget to set enemy ship is_alive to true now!
                        enemy_ship_array[i].is_alive = true;

                        enemy_ship_array[i].health = enemy_health_per_level * game_state.game_level;

                        enemy_ship_array[i].velocity.X = -5;
                        enemy_ship_array[i].cash_value = (int)(cash_worth_multipler * game_state.game_level);
                        current_enemy_ships_on_screen++;
                        

                        
                        //break out of loop since we only want to create 1 bad guy per call to this function
                        break;
                    }
                }
            }
        }

       

        public void fire_projectile_forward()
        {
            for (int i = 0; i < max_projectiles; i++)
            {
                if (!projectile_array[i].is_alive)
                {
                    projectile_array[i].position.X = player_ship_position.X + (player_ship_texture.Width);
                    projectile_array[i].position.Y = player_ship_position.Y + (0.5f * player_ship_texture.Height);
                    projectile_array[i].velocity.X = 10;
                    projectile_array[i].velocity.Y = 0;

                    projectile_array[i].is_alive = true;
                    break;
                }
            }
        }

        public void fire_projectile_down_forward()
        {
            for (int i = 0; i < max_projectiles; i++)
            {
                if (!projectile_array[i].is_alive)
                {
                    projectile_array[i].position.X = player_ship_position.X + (player_ship_texture.Width);
                    projectile_array[i].position.Y = player_ship_position.Y + (0.5f * player_ship_texture.Height);
                    projectile_array[i].velocity.X = 10;
                    projectile_array[i].velocity.Y = -5;

                    projectile_array[i].is_alive = true;
                    break;
                }
            }
        }

        public void fire_projectile_up_forward()
        {
            for (int i = 0; i < max_projectiles; i++)
            {
                if (!projectile_array[i].is_alive)
                {
                    projectile_array[i].position.X = player_ship_position.X + (player_ship_texture.Width);
                    projectile_array[i].position.Y = player_ship_position.Y + (0.5f * player_ship_texture.Height);
                    projectile_array[i].velocity.X = 10;
                    projectile_array[i].velocity.Y = 5;

                    projectile_array[i].is_alive = true;
                    break;
                }
            }
        }


        public void handle_menu_input()
        {
            KeyboardState keyboardstate = Keyboard.GetState();

            //exit game if  escape key is down
            if (keyboardstate.IsKeyDown(Keys.Escape) &&
                previousKeyboardState.IsKeyUp(Keys.Escape) )
            {
                this.Exit();
            }

            //buy spread
            if (keyboardstate.IsKeyDown(Keys.S) &&
               previousKeyboardState.IsKeyUp(Keys.S))
            {
                if (player_state.player_cash >= fire_spread_upgrade_cost)
                {
                    player_state.player_cash -= fire_spread_upgrade_cost;
                    player_state.fire_spread = true;
                }
            }
            //buy damage up
            if (keyboardstate.IsKeyDown(Keys.D) &&
              previousKeyboardState.IsKeyUp(Keys.D))
            {
                if (player_state.player_cash >= (fire_damage_upgrade_cost) + (fire_damage_upgrade_cost * damage_upgrade_levels))
                {
                    player_state.player_cash -= (fire_damage_upgrade_cost) + (fire_damage_upgrade_cost * damage_upgrade_levels);
                    damage_upgrade_levels++;
                }
            }



            if (keyboardstate.IsKeyDown(Keys.Space) &&
                previousKeyboardState.IsKeyUp(Keys.Space))
            {
                game_state.is_paused = false;
            }

            previousKeyboardState = keyboardstate;
        }
        public void handle_input()
        {
            KeyboardState keyboardstate = Keyboard.GetState();

            //exit game if  escape key is down
            if (keyboardstate.IsKeyDown(Keys.Escape))
            {
                game_state.is_paused = true;
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
                fire_laser_sound.Play();

                if (player_state.fire_spread)
                {
                    fire_projectile_forward();
                    fire_projectile_up_forward();
                    fire_projectile_down_forward();
                }
                else
                {
                    fire_projectile_forward();
                }
               
            }

            previousKeyboardState = keyboardstate;
        }

        //update player checks and updates various player related game state information
        public void update_player()
        {
            //check if player has no health and no lives, if so, GAME OVER
            if( player_state.player_health <= 0)
            {
                game_state.game_OVER = true;
            }
        }

        public void update_level()
        {
            if (game_state.enemies_blown_up > (base_kills_per_level * game_state.game_level))
            {
                game_state.enemies_blown_up = 0;
                game_state.game_level++;
            }
        }

        public void update_enemy_ships()
        {
           
            Rectangle player_ship_bounding_box = new Rectangle((int)player_ship_position.X,
                 (int)player_ship_position.Y, player_ship_texture.Width, player_ship_texture.Height);

           
            
            //first check if enemy ship has negative health, if so destroy it
            for (int i = 0; i < max_enemy_ships; i++)
            {

                if ( (enemy_ship_array[i].health <= 0 ) && enemy_ship_array[i].is_alive)
                {
                    enemy_ship_array[i].is_alive = false;
                    explosion.AddParticles(enemy_ship_array[i].position);
                    smoke.AddParticles(enemy_ship_array[i].position);
                    player_state.player_cash += enemy_ship_array[i].cash_value;
                    current_enemy_ships_on_screen--;
                    game_state.enemies_blown_up++;
                    ship_bang.Play();
                    
                }
                
              
                //create a bounding box around this current enemy in the loop
                Rectangle enemy_bounding_box = new Rectangle((int)enemy_ship_array[i].position.X,
                    (int)enemy_ship_array[i].position.Y, enemy_ship_array[i].texture.Width, enemy_ship_array[i].texture.Height);

                //see if this enemy collides with player ship
                if (enemy_bounding_box.Intersects(player_ship_bounding_box))
                {
                    //collision detected, ship enemy health to zero and remove health from player 
                    enemy_ship_array[i].health = 0;
                    player_state.player_health -= (int)(enemy_damage_multiplier * game_state.game_level);
                }

                if (enemy_ship_array[i].is_alive)
                {


                    enemy_ship_array[i].position.X += enemy_ship_array[i].velocity.X;
                    enemy_ship_array[i].position.X += enemy_ship_array[i].velocity.Y;

                    //if the enemy ship is off the screen to the left, set alive = false
                    if (enemy_ship_array[i].position.X < 0 && enemy_ship_array[i].is_alive == true)
                    {
                        enemy_ship_array[i].is_alive = false;
                        current_enemy_ships_on_screen--;
                    }
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

            if (game_state.is_paused || game_state.game_OVER)
            {
                //handle menu input
                handle_menu_input();
            }
            else if (!game_state.game_OVER)
            {
                //Update projectiles
                Update_player_projectiles();

                //scan and act upon keyboard input
                handle_input();

                //update enemy ship
                update_enemy_ships();

                //call our custom spawn enemy function, it will keep max_enemies_on_screen worth of enemies on the screen
                spawn_enemy_ship();

                //update game level state
                update_level();

                //update player state
                update_player();

               
            }
      
            base.Update(gameTime);
        }


         
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
    
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            //if game is paused, draw menu and options, else draw game
            if (game_state.is_paused && !game_state.game_OVER)
            {

                //draw cash and player health:
                spriteBatch.DrawString(
                 arialFont,
                 "Health: " + player_state.player_health,
                 new Vector2((viewportRect.Width / 20),
                 40),
                 Color.White);

               
                spriteBatch.DrawString(
               arialFont,
               "cash: " + player_state.player_cash,
               new Vector2((viewportRect.Width / 20),
               80),
               Color.White);

                spriteBatch.DrawString(
              arialFont,
              "damage: " + (starting_player_standard_damage + (starting_player_standard_damage * damage_upgrade_levels)),
              new Vector2((viewportRect.Width / 20),
              120),
              Color.White);

                if (player_state.fire_spread)
                {
                    spriteBatch.DrawString(
                        arialFont,
                        "fire spread: Yes",
                        new Vector2((viewportRect.Width / 20),
                        160),
                        Color.White);
                }
                else
                {
                    spriteBatch.DrawString(
                        arialFont,
                        "fire spread: No",
                        new Vector2((viewportRect.Width / 20),
                        160),
                        Color.White);
                }



                //draw menu
                spriteBatch.DrawString(
                  arialFont,
                  "Menu options: ",
                  new Vector2((viewportRect.Width / 2) - 80,
                  40),
                  Color.White);

                spriteBatch.DrawString(
                 arialFont,
                 "Exit game                    : Escape",
                 new Vector2((viewportRect.Width / 2) - 80,
                 80),
                 Color.White);

                spriteBatch.DrawString(
                arialFont,
                "Unpause or start game       : Space ",
                new Vector2((viewportRect.Width / 2) - 80,
                100),
                Color.White);

                spriteBatch.DrawString(
               arialFont,
               "Buy options:",
               new Vector2((viewportRect.Width / 2) - 80,
               190),
               Color.White);

                spriteBatch.DrawString(
               arialFont,
               "Buy spread fire for $" + fire_spread_upgrade_cost.ToString() + "     : S",
               new Vector2((viewportRect.Width / 2) - 80,
               220),
               Color.White);

                spriteBatch.DrawString(
              arialFont,
              "Buy damage upgrade for $" +
              ((fire_damage_upgrade_cost) + (fire_damage_upgrade_cost * damage_upgrade_levels)) + " : D",
              new Vector2((viewportRect.Width / 2) - 80,
              240),
              Color.White);



            }
            else if (!game_state.game_OVER)
            {
                //draw game

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


                spriteBatch.DrawString(
                    arialFont,
                    "cash: " + player_state.player_cash.ToString(),
                    new Vector2(100,
                    40),
                    Color.White);

                spriteBatch.DrawString(
                   arialFont,
                   "Health: " + player_state.player_health.ToString(),
                   new Vector2(300,
                   40),
                   Color.White);

                spriteBatch.DrawString(
                  arialFont,
                  "level: " + game_state.game_level.ToString(),
                  new Vector2(500,
                  40),
                  Color.White);


            }
            else
            {
                spriteBatch.DrawString(
                 arialFont,
                 "Game Over! ",
                 new Vector2(viewportRect.Width / 2,
                 viewportRect.Height /2 ),
                 Color.Red);
            }

            spriteBatch.End();

            // TODO: Add your drawing code here
             
            base.Draw(gameTime);
        }
    }
}
