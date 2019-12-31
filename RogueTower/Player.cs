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
        public bool AltAttackHeld;
        public bool Pickup;

        public bool ClimbUp;
        public bool ClimbDown;

        public bool IsAiming;
        public bool AimFire;
        public float AimAngle;

        public InputQueue(Player player)
        {
            Player = player;
        }

        public void Update(SceneGame game)
        {
            bool left = game.InputState.IsKeyDown(Keys.A) || (game.InputState.IsButtonDown(Buttons.LeftThumbstickLeft) || game.InputState.IsButtonDown(Buttons.DPadLeft));
            bool right = game.InputState.IsKeyDown(Keys.D) || (game.InputState.IsButtonDown(Buttons.LeftThumbstickRight) || game.InputState.IsButtonDown(Buttons.DPadRight));
            bool up = game.InputState.IsKeyDown(Keys.W) || (game.InputState.IsButtonDown(Buttons.LeftThumbstickUp) || game.InputState.IsButtonDown(Buttons.DPadUp));
            bool down = game.InputState.IsKeyDown(Keys.S) || (game.InputState.IsButtonDown(Buttons.LeftThumbstickDown) || game.InputState.IsButtonDown(Buttons.DPadDown));
            bool attack = game.InputState.IsKeyPressed(Keys.Space) || (game.InputState.IsButtonPressed(Buttons.X));
            bool altattack = game.InputState.IsKeyPressed(Keys.LeftAlt) || (game.InputState.IsButtonPressed(Buttons.B));
            bool pickup = game.InputState.IsKeyPressed(Keys.LeftControl) || (game.InputState.IsButtonPressed(Buttons.Y));
            bool forward = (Player.Facing == HorizontalFacing.Left && left) || (Player.Facing == HorizontalFacing.Right && right);
            bool back = (Player.Facing == HorizontalFacing.Left && right) || (Player.Facing == HorizontalFacing.Right && left);

            MoveLeft = left;
            MoveRight = right;

            if((game.InputState.IsKeyPressed(Keys.LeftShift)) || (game.InputState.IsButtonPressed(Buttons.A)))
                Jump = true;
            JumpHeld = game.InputState.IsKeyDown(Keys.LeftShift) || game.InputState.IsButtonDown(Buttons.A);
            
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
            AltAttackHeld = game.InputState.IsKeyDown(Keys.LeftAlt) || game.InputState.IsButtonDown(Buttons.B);

            if (pickup)
                Pickup = true;

            var aimVector = game.InputState.Next.GamePad.ThumbSticks.Right;
            aimVector.Y = -aimVector.Y;
            if (!IsAiming)
            {
                if(aimVector.Length() > 0.8f)
                {
                    IsAiming = true;
                }
            }
            else
            {
                if (aimVector.Length() < 0.8f)
                {
                    AimFire = true;
                    IsAiming = false;
                }
                else
                {
                    AimAngle = VectorToAngle(aimVector);
                }
            }

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
            AltAttackHeld = false;
            Pickup = false;

            ClimbUp = false;
            ClimbDown = false;

            AimFire = false;
        }
    }

    class Player : EnemyHuman
    {
        public InputQueue Controls;

        public override RectangleF ActivityZone => World.Bounds;

        //public Weapon Weapon = new WeaponKnife(15, 14, new Vector2(14 / 2, 14 * 2));
        //public Weapon Weapon = new WeaponKatana(15, 20, new Vector2(10, 40));
        //public Weapon Weapon = new WeaponRapier(15, 20, new Vector2(10, 40));
        //public Weapon Weapon = new WeaponWandOrange(10, 16, new Vector2(8, 32));
        //public Weapon Weapon = new WeaponUnarmed(10, 14, new Vector2(7, 28));
        //public Weapon Weapon = new WeaponLance(20, 38, new Vector2(19, 76));
        public double SwordSwingDamage = 15.0;
        public double SwordSwingDownDamage = 20.0;

        public PlayerInput PlayerInput;

        public List<DroppedItem> NearbyItems = new List<DroppedItem>();

        public List<Item> Inventory = new List<Item>();
        public bool InventoryChanged;

        public Player(GameWorld world, Vector2 position) : base(world, position)
        {
            InitHealth(100);
            Controls = new InputQueue(this);
            //Weapon = new WeaponKatana(15, 20, new Vector2(10, 40));
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.AddTags(CollisionTag.Character);
            Box.Data = this;

            foreach(var weapon in Weapon.PresetWeaponList)
            {
                Inventory.Add(weapon);
            }
        }

        public void SetControl(PlayerInput input)
        {
            PlayerInput = input;
        }

        public void Pickup(DroppedItem item)
        {
            if (!item.Destroyed)
            {
                //new DamagePopup(World, item.Position, $"+1 {item.Item.Name}", 30);
                new ItemPickup(World, item.Item, item.Position, new Vector2(24, 24), 50);
                Inventory.Add(item.Item);
                InventoryChanged = true;
                item.Destroy();
            }
        }

        protected override void HandleInput()
        {
            CurrentAction.OnInput();
            Controls.Reset();
        }

        public override void Update(float delta)
        {
            InventoryChanged = false;
            base.Update(delta);
            int removed = Inventory.RemoveAll(item => item.Destroyed);
            if (removed > 0)
                InventoryChanged = true;
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();

            RectangleF pickupArea = new RectangleF(Position + new Vector2(-12, 0), new Vector2(24, 8));
            NearbyItems = World.FindBoxes(pickupArea).Where(x => x.Data is DroppedItem).Select(x => (DroppedItem)x.Data).ToList();
        }

        public override PlayerState GetBasePose()
        {
            PlayerState pose = new PlayerState(
                HeadState.Forward,
                BodyState.Stand,
                ArmState.Neutral,
                ArmState.Neutral,
                WeaponState.None
            );
            Weapon.GetPose(pose);
            return pose;
        }

        public override void SetPhenoType(PlayerState pose)
        {
            //NOOP
        }

        public override void Death()
        {
            base.Death();
            if (!(CurrentAction is ActionPlayerDeath))
                CurrentAction = new ActionPlayerDeath(this, 100);
        }
    }
}
