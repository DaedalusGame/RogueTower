using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChaiFoxes.FMODAudio;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower
{
    enum HorizontalFacing
    {
        Left,
        Right,
    }

    enum CollisionType
    {
        Air,
        Floor,
        Wall,
        Ceiling,
    }

    class InputQueue
    {
        Player Player;

        public bool MoveLeft;
        public bool MoveRight;
        public bool Jump;
        public bool JumpHeld;

        public bool Attack;
        public bool ForwardAttack;
        public bool BackAttack;
        public bool UpAttack;
        public bool DownAttack;
        public bool AltAttack;

        public bool ClimbUp;
        public bool ClimbDown;

        KeyboardState LastState;
        GamePadState LastGPState;

        public InputQueue(Player player)
        {
            Player = player;
        }

        public void Update(SceneGame game)
        {
            bool left = game.KeyState.IsKeyDown(Keys.A) || (game.PadState.IsButtonDown(Buttons.LeftThumbstickLeft) || game.PadState.IsButtonDown(Buttons.DPadLeft));
            bool right = game.KeyState.IsKeyDown(Keys.D) || (game.PadState.IsButtonDown(Buttons.LeftThumbstickRight) || game.PadState.IsButtonDown(Buttons.DPadRight));
            bool up = game.KeyState.IsKeyDown(Keys.W) || (game.PadState.IsButtonDown(Buttons.LeftThumbstickUp) || game.PadState.IsButtonDown(Buttons.DPadUp));
            bool down = game.KeyState.IsKeyDown(Keys.S) || (game.PadState.IsButtonDown(Buttons.LeftThumbstickDown) || game.PadState.IsButtonDown(Buttons.DPadDown));
            bool attack = game.KeyState.IsKeyDown(Keys.Space) && LastState.IsKeyUp(Keys.Space) || (game.PadState.IsButtonDown(Buttons.X) && LastGPState.IsButtonUp(Buttons.X));
            bool altattack = game.KeyState.IsKeyDown(Keys.LeftAlt) && LastState.IsKeyUp(Keys.LeftAlt) || (game.PadState.IsButtonDown(Buttons.B) && LastGPState.IsButtonUp(Buttons.B));
            bool forward = (Player.Facing == HorizontalFacing.Left && left) || (Player.Facing == HorizontalFacing.Right && right);
            bool back = (Player.Facing == HorizontalFacing.Left && right) || (Player.Facing == HorizontalFacing.Right && left);

            MoveLeft = left;
            MoveRight = right;

            if((game.KeyState.IsKeyDown(Keys.LeftShift) && LastState.IsKeyUp(Keys.LeftShift)) || (game.PadState.IsButtonDown(Buttons.A) && LastGPState.IsButtonUp(Buttons.A)))
                Jump = true;
            JumpHeld = game.KeyState.IsKeyDown(Keys.LeftShift) || game.PadState.IsButtonDown(Buttons.A);
            
            ClimbUp = up;
            ClimbDown = down;

            if (attack)
                Attack = true;
            if (attack && up)
                UpAttack = true;
            if (attack && down)
                DownAttack = true;
            if (forward && attack)
                ForwardAttack = true;
            if (back && attack)
                BackAttack = true;

            if(altattack)
                AltAttack = true;
            LastState = game.KeyState;
            LastGPState = game.PadState;
        }

        public void Reset()
        {
            MoveLeft = false;
            MoveRight = false;
            Jump = false;
            JumpHeld = false;

            Attack = false;
            ForwardAttack = false;
            BackAttack = false;
            DownAttack = false;
            UpAttack = false;
            AltAttack = false;

            ClimbUp = false;
            ClimbDown = false;
        }
    }

    class Player : GameObject
    {
        public InputQueue Controls;

        public override RectangleF ActivityZone => World.Bounds;

        public IBox Box;
        public Vector2 Position
        {
            get
            {
                return Box.Bounds.Center;
            }
            set
            {
                var pos = value + Box.Bounds.Center - Box.Bounds.Location;
                Box.Teleport(pos.X, pos.Y);
            }
        }
        public Vector2 Velocity;

        private Vector2 VelocityLeftover;
        //public Weapon Weapon = new WeaponKnife(15, 14, new Vector2(14 / 2, 14 * 2));
        public Weapon Weapon = new WeaponKatana(15, 20, new Vector2(10, 40));

        public float Gravity = 0.2f;
        public float GravityLimit = 10f;
        public float Acceleration = 0.25f;
        public float SpeedLimit = 2;
        public bool OnGround;
        public bool InAir => !OnGround;
        public bool OnWall;
        public bool OnCeiling;
        public float GroundFriction = 1.0f;
        public float AppliedFriction;

        public int ExtraJumps = 0;

        public HorizontalFacing Facing;
        public Action CurrentAction;

        public int Invincibility = 0;

        public bool Attacking => CurrentAction.Attacking;

        public double SwordSwingDamage = 15.0;
        public double SwordSwingDownDamage = 20.0;

        public float Lifetime;

        KeyboardState LastState;
        SceneGame SceneGame;

        public Player(GameWorld world, Vector2 position) : base(world)
        {
            Controls = new InputQueue(this);
            CurrentAction = new ActionIdle(this);
            Create(position.X, position.Y);
        }

        public void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.AddTags(CollisionTag.Character);
            Box.Data = this;
        }

        public void SetControl(SceneGame game)
        {
            SceneGame = game;
        }

        public void ResetState()
        {
            if(OnGround)
            {
                CurrentAction = new ActionIdle(this);
            }
            else
            {
                CurrentAction = new ActionJump(this, true, true);
            }
        }

        private Vector2 CalculateMovement(float delta)
        {
            var velocity = Velocity * delta + VelocityLeftover;
            var movement = new Vector2((int)velocity.X, (int)velocity.Y);

            VelocityLeftover = velocity - movement;

            return movement;
        }

        public bool Parry(RectangleF hitmask)
        {
            //new RectangleDebug(World, hitmask, Color.Orange, 20);
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data is Enemy enemy && enemy.Attacking)
                {
                    PlaySFX(sfx_sword_bink, 1.0f, -0.3f, -0.5f);
                    World.Hitstop = 15;
                    Invincibility = 10;
                    if (OnGround)
                    {
                        Velocity += GetFacingVector(Facing) * -2;
                    }
                    else
                    {
                        ExtraJumps = Math.Max(ExtraJumps, 1);
                        Velocity.Y = 0;
                    }
                    new ParryEffect(World, Vector2.Lerp(Box.Bounds.Center,Position,0.5f), 0, 10);
                    return true;
                }
            }

            return false;
        }

        public void SwingWeapon(RectangleF hitmask, double damageIn = 0)
        {
            //new RectangleDebug(World, hitmask, Color.Lime, 20);
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data is Enemy enemy)
                {
                    enemy.Hit(Util.GetFacingVector(Facing) + new Vector2(0, -2), 20, 50, damageIn);
                }
                if (Box.Data is Tile tile)
                {
                    tile.HandleTileDamage(damageIn);
                }
            }

        }

        protected override void UpdateDelta(float delta)
        {
            Lifetime += delta;

            CurrentAction.UpdateDelta(delta);

            Controls.Update(SceneGame);

            var movement = CalculateMovement(delta);

            bool IsMovingVertically = Math.Abs(movement.Y) > 0.1;
            bool IsMovingHorizontally = Math.Abs(movement.X) > 0.1;

            IMovement move = Move(movement);

            var hits = move.Hits.Where(c => c.Normal != Vector2.Zero && !IgnoresCollision(c.Box));

            /*if (move.Hits.Any() && !hits.Any())
            {
                IsMovingHorizontally = false;
                IsMovingVertically = false;
            }*/

            var nearbies = World.FindBoxes(Box.Bounds).Where(x => x.Data != this);
            foreach (var nearby in nearbies)
            {
                if (nearby.Data is Enemy enemy)
                {
                    float dx = enemy.Position.X - Position.X;
                    if (Math.Abs(dx) < 1)
                        dx = Random.NextFloat() - 0.5f;
                    if (dx > 0 && Velocity.X > -1)
                        Velocity.X = -1;
                    else if (dx < 0 && Velocity.X < 1)
                        Velocity.X = 1;
                }
                if (nearby.Data is Player player)
                {
                    float dx = player.Position.X - Position.X;
                    if (Math.Abs(dx) < 1)
                        dx = Random.NextFloat() - 0.5f;
                    if (dx > 0 && Velocity.X > -1)
                        Velocity.X = -1;
                    else if (dx < 0 && Velocity.X < 1)
                        Velocity.X = 1;
                }
            }

            if (IsMovingVertically)
            {
                if (hits.Any((c) => c.Normal.Y < 0))
                {
                    OnGround = true;
                }
                else
                {
                    OnGround = false;
                }

                if (hits.Any((c) => c.Normal.Y > 0))
                {
                    OnCeiling = true;
                }
                else
                {
                    OnCeiling = false;
                }
            }

            if (IsMovingHorizontally)
            {
                if (hits.Any((c) => c.Normal.X != 0))
                {
                    OnWall = true;
                }
                else
                {
                    OnWall = false;
                }
            }

            RectangleF panicBox = new RectangleF(move.Destination.X + 2, move.Destination.Y + 2, move.Destination.Width - 4, move.Destination.Height - 4);
            var found = World.FindBoxes(panicBox);
            if (found.Any() && found.Any(x => x != Box && !IgnoresCollision(x)))
            {
                Box.Teleport(move.Origin.X, move.Origin.Y);
            }
        }

        /*private void DownSwing()
        {
            var facingLength = 14;
            if (CurrentAction == Action.Slash || CurrentAction == Action.SlashUp)
            {
            }
            SlashAction = SwordAction.DownSwing;
            switch (CurrentAction)
            {
                case (Action.Slash):

                    break;
                case (Action.SlashUp):
                    break;
            }

            if (CurrentAction == Action.SlashKnife)
            {

                Vector2 facing = GetFacingVector(Facing);
                Knife bullet = new Knife(World, Position + facing * 5);
                bullet.Velocity = facing * 8;
                bullet.LifeTime = 20;
                bullet.Shooter = this;
                PlaySFX(sfx_knife_throw, 1.0f, 0.4f, 0.7f);
            }
        }*/

        private IMovement Move(Vector2 movement)
        {
            return Box.Move(Box.X + movement.X, Box.Y + movement.Y, collision =>
            {
                if (IgnoresCollision(collision.Hit.Box))
                    return new CrossResponse(collision);
                return new SlideAdvancedResponse(collision);
            });
        }

        private bool IgnoresCollision(IBox box)
        {
            return box.HasTag(CollisionTag.NoCollision) || box.HasTag(CollisionTag.Character);
        }

        protected override void UpdateDiscrete()
        {
            if (OnCeiling)
            {
                Velocity.Y = 1;
                AppliedFriction = 1;
            }
            else if (OnGround) //Friction
            {
                UpdateGroundFriction();
                if(Velocity.Y < 0)
                {

                }
                Velocity.Y = 0;
                AppliedFriction = CurrentAction.Friction;
                ExtraJumps = 0;
            }
            else //Drag
            {
                AppliedFriction = CurrentAction.Drag;
            }

            if (OnWall)
            {
                var wallTiles = World.FindTiles(Box.Bounds.Offset(GetFacingVector(Facing)));
                if (wallTiles.Any())
                {
                    Velocity.X = 0;
                }
                else
                {
                    OnWall = false;
                }
            }

            HandleDamage();

            CurrentAction.UpdateDiscrete();

            CurrentAction.OnInput();
            Controls.Reset();

            Velocity.X *= AppliedFriction;

            if (CurrentAction.HasGravity && Velocity.Y < GravityLimit)
                Velocity.Y = Math.Min(GravityLimit, Velocity.Y + Gravity); //Gravity

            if (OnGround) //Damage
            {
                var tiles = World.FindTiles(Box.Bounds.Offset(0, 1)).Where(tile => tile.Damage > 0);
                if (tiles.Any())
                {
                    Hit(-GetFacingVector(Facing) * 1 + new Vector2(0, -2), 20, 50, tiles.First().Damage);
                }
            }

            LastState = SceneGame.KeyState;
        }

        private void UpdateGroundFriction()
        {
            var tiles = World.FindTiles(Box.Bounds.Offset(0, 1));
            if (tiles.Any())
                GroundFriction = tiles.Max(tile => tile.Friction);
            else
                GroundFriction = 1.0f;
        }

        private void HandleDamage()
        {
            if (!(CurrentAction is ActionHit))
                Invincibility--;
        }

        public float GetJumpVelocity(float height)
        {
            return (float)Math.Sqrt(2 * Gravity * height);
        }

        public void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            if (CurrentAction is ActionSlash slash && slash.IsUpSwing)
            {
                //Parry
                slash.Swing();
                return;
            }
            if (Invincibility > 0)
                return;
            if(CurrentAction is ActionClimb)
                Velocity = GetFacingVector(Facing) * -1 + new Vector2(0,1);
            else 
                Velocity = velocity;
            OnWall = false;
            OnGround = false;
            Invincibility = invincibility;
            CurrentAction = new ActionHit(this, hurttime);
            PlaySFX(sfx_player_hurt, 1.0f, 0.1f, 0.3f);
            HandleDamage(damageIn);
            World.Hitstop = 6;
        }

        public override void ShowDamage(double damage)
        {
            new DamagePopup(World, Position + new Vector2(0, -16), damage.ToString(), 30);
        }
    }
}
