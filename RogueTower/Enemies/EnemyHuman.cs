using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Actions.Death;
using RogueTower.Actions.Hurt;
using RogueTower.Actions.Movement;
using RogueTower.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower.Enemies
{
    abstract class EnemyHuman : EnemyGravity
    {
        public HorizontalFacing Facing;
        public PlayerState Pose;

        public override float Gravity => CurrentAction.HasGravity ? base.Gravity : 0;
        public override float Friction => CurrentAction.Friction;
        public override float Drag => CurrentAction.Drag;
        public virtual float Acceleration => 0.25f;
        public virtual float SpeedLimit => 2;
        public int ExtraJumps = 0;
        public int Invincibility = 0;
        public override bool CanDamage => true;

        public Weapon Weapon;

        public virtual bool Strafing => false;
        public override bool CanParry => CurrentAction.CanParry;
        public override bool Incorporeal => CurrentAction.Incorporeal;
        public override bool Dead => CurrentAction is ActionEnemyDeath;

        public override bool VisualInvisible => false;

        public ActionBase CurrentAction;

        public EnemyHuman(GameWorld world, Vector2 position) : base(world, position)
        {
            CurrentAction = new ActionIdle(this);
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.AddTags(CollisionTag.Character);
            Box.Data = this;
        }

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
        }

        public void ResetState()
        {
            if (OnGround)
            {
                CurrentAction = new ActionIdle(this);
            }
            else
            {
                CurrentAction = new ActionJump(this, true, true);
            }
        }

        public override void Update(float delta)
        {
            float modifier = 1.0f;
            if (CurrentAction is ActionEnemyDeath)
                modifier = 0.5f;
            base.Update(delta * modifier);
        }

        protected override void UpdateDelta(float delta)
        {
            Lifetime += delta;

            UpdatePose();

            HandleMovement(delta);

            Weapon.UpdateDelta(this, delta);
            CurrentAction.UpdateDelta(delta);
        }

        protected override void UpdateDiscrete()
        {
            if (OnGround)
            {
                ExtraJumps = 0;
            }

            HandlePhysicsEarly();

            var wallTiles = World.FindBoxes(Box.Bounds.Offset(GetFacingVector(Facing))).Where(box => box.Data is Tile && !IgnoresCollision(box));
            if (wallTiles.Any() && !Incorporeal)
            {
                Velocity.X = 0;
            }
            else
            {
                OnWall = false;
            }

            if (!Incorporeal)
            {
                var nearbies = World.FindBoxes(Box.Bounds).Where(x => x.Data != this);
                foreach (var nearby in nearbies)
                {
                    if (nearby.Data is Enemy enemy && !enemy.Incorporeal)
                    {
                        float dx = enemy.Position.X - Position.X;
                        if (Math.Abs(dx) < 3)
                            dx = -Velocity.X;
                        if (dx == 0)
                            dx = Random.NextDouble() < 0.5 ? 1 : -1;
                        if (dx > 0 && Velocity.X > -1)
                            Velocity.X = -1;
                        else if (dx < 0 && Velocity.X < 1)
                            Velocity.X = 1;
                    }
                }
            }

            HandleDamage();

            Weapon.UpdateDiscrete(this);
            if (!Stunned)
            {
                CurrentAction.UpdateDiscrete();
                HandleInput(); //For implementors
            }

            HandlePhysicsLate();

            if (OnGround) //Damage
            {
                var tiles = World.FindTiles(Box.Bounds.Offset(0, 1));
                if (tiles.Any())
                {
                    Tile steppedTile = tiles.WithMin(tile => Math.Abs(tile.X * 16 + 8 - Position.X));
                    steppedTile.StepOn(this);
                }
            }
        }

        public void UpdatePose()
        {
            Pose = GetBasePose();
            CurrentAction.GetPose(Pose);
            SetPhenoType(Pose);
        }

        public abstract PlayerState GetBasePose();

        public abstract void SetPhenoType(PlayerState pose);

        protected virtual void HandleInput()
        {
            //NOOP
        }

        /*public bool Parry(RectangleF hitmask)
        {
            //new RectangleDebug(World, hitmask, Color.Orange, 20);
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data is Enemy enemy && enemy.Attacking)
                {
                    if (Box.Data == this)
                        continue;
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
                    new ParryEffect(World, Vector2.Lerp(Box.Bounds.Center, Position, 0.5f), 0, 10);
                    return true;
                }
            }

            return false;
        }*/

        public override void ParryGive(IParryReceiver receiver, RectangleF box)
        {
            if (CurrentAction is ActionAttack attack)
                attack.ParryGive(receiver);
            Invincibility = 10;
            Hitstop = 20;
        }

        public override void ParryReceive(IParryGiver giver, RectangleF box)
        {
            if (CurrentAction is ActionAttack attack)
                attack.ParryReceive(giver);
            new ParryEffect(World, Vector2.Lerp(box.Center, Position, 0.3f), 0, 10);
            Invincibility = 10;
            Hitstop = 20;
        }

        public void SwingWeapon(RectangleF hitmask, double damageIn = 0)
        {
            if (SceneGame.DebugMasks)
                new RectangleDebug(World, hitmask, new Color(Color.Lime, 0.5f), 20);
            Weapon.OnAttack(CurrentAction, hitmask);
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data == this)
                    continue;
                if (Box.Data is Enemy enemy && enemy.CanHit)
                {
                    Weapon.OnHit(CurrentAction, enemy);
                }
                if (Box.Data is Tile tile)
                {
                    tile.HandleTileDamage(damageIn);
                }
            }

        }

        public float GetJumpVelocity(float height)
        {
            return (float)Math.Sqrt(2 * Gravity * height);
        }

        protected void HandleDamage()
        {
            //if (!(CurrentAction is ActionHit))
            Invincibility--;
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            if (Invincibility > 0 || Dead)
                return;
            Invincibility = invincibility + hurttime;
            if (Random.NextDouble() < 1.0) //Poise?
            {
                if (CurrentAction is ActionClimb)
                    Velocity = GetFacingVector(Facing) * -1 + new Vector2(0, 1);
                else
                    Velocity = velocity;
                OnWall = false;
                OnGround = false;
                CurrentAction = new ActionHit(this, hurttime);
            }
            PlaySFX(sfx_player_hurt, 1.0f, 0.1f, 0.3f);
            HandleDamage(damageIn);
            World.Hitstop = 6;
            Hitstop = 6;
            VisualOffset = OffsetHitStun(6);
            VisualFlash = Flash(Color.White, 4);
            for (int i = 0; i < 3; i++)
                new BloodSpatterEffect(World, GetRandomPosition(Box.Bounds, Random), Random.NextFloat() * MathHelper.TwoPi, 3 + Random.NextFloat() * 5);
            new ScreenShakeJerk(World, AngleToVector(Random.NextFloat() * MathHelper.TwoPi) * 4, 3);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            if (VisualInvisible)
                yield return DrawPass.Invisible;
            else
            {
                yield return DrawPass.Background;
                yield return DrawPass.Foreground;
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            scene.DrawHuman(this);
            scene.DrawWireCircle(Position, 16, 20, Color.Red);
        }
    }
}
