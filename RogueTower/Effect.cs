﻿using ChaiFoxes.FMODAudio;
using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    abstract class VisualEffect : GameObject
    {
        public float Frame;

        public override RectangleF ActivityZone => World.Bounds;

        public VisualEffect(GameWorld world) : base(world)
        {
        }

        protected override void UpdateDelta(float delta)
        {
            Frame += delta;
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Effect;
        }
    }

    class ScreenShake : VisualEffect
    {
        public Vector2 Offset;
        public float FrameEnd;

        public ScreenShake(GameWorld world, float time) : base(world)
        {
            FrameEnd = time;
        }

        public override void Draw(SceneGame scene)
        {
            //NOOP
        }
    }

    class ScreenShakeRandom : ScreenShake
    {
        float Amount;

        public ScreenShakeRandom(GameWorld world, float amount, float time) : base(world, time)
        {
            Amount = amount;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);

            double amount = Amount * (1 - Frame / FrameEnd);
            double shakeAngle = Random.NextDouble() * Math.PI * 2;
            int x = (int)Math.Round(Math.Cos(shakeAngle) * amount);
            int y = (int)Math.Round(Math.Sin(shakeAngle) * amount);
            Offset = new Vector2(x, y);
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }
    }

    class ScreenShakeJerk : ScreenShake
    {
        Vector2 Jerk;

        public ScreenShakeJerk(GameWorld world, Vector2 jerk, float time) : base(world, time)
        {
            Jerk = jerk;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);

            float amount = (1 - Frame / FrameEnd);
            Offset = Jerk * amount;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }
    }

    class SlashEffectRound : Particle
    {
        public Func<Vector2> Anchor;
        public float Angle;
        public SpriteEffects Mirror;
        public float FrameEnd;
        public float Size;

        public override Vector2 Position
        {
            get
            {
                return Anchor();
            }
            set
            {
                //NOOP
            }
        }

        public SlashEffectRound(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, Vector2.Zero)
        {
            Anchor = anchor;
            Angle = angle;
            Size = size;
            Mirror = mirror;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if(Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var slash = SpriteLoader.Instance.AddSprite("content/slash_round");
            var slashAngle = Angle;
            if (Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                slashAngle = -slashAngle;
            if (Size > 0.2)
                scene.SpriteBatch.Draw(slash.Texture, Position + new Vector2(8, 8) - new Vector2(8, 8), slash.GetFrameRect(Math.Min(slash.SubImageCount - 1, (int)(slash.SubImageCount * Frame / FrameEnd) - 1)), Color.LightGray, slashAngle, slash.Middle, Size - 0.2f, Mirror, 0);
            scene.SpriteBatch.Draw(slash.Texture, Position + new Vector2(8, 8) - new Vector2(8, 8), slash.GetFrameRect(Math.Min(slash.SubImageCount - 1, (int)(slash.SubImageCount * Frame / FrameEnd))), Color.White, slashAngle, slash.Middle, Size, Mirror, 0);
        }
    }

    class SlashEffectStraight : SlashEffectRound
    {
        public SlashEffectStraight(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, anchor, size, angle, mirror, time)
        {
        }

        public override void Draw(SceneGame scene)
        {
            var stab = SpriteLoader.Instance.AddSprite("content/slash_straight");
            var slashAngle = Angle;
            if (Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                slashAngle = -slashAngle;
            scene.SpriteBatch.Draw(stab.Texture, Position + new Vector2(8, 8) - new Vector2(8, 8), stab.GetFrameRect(Math.Min(stab.SubImageCount - 1, (int)(stab.SubImageCount * Frame / FrameEnd))), Color.White, slashAngle, stab.Middle, Size, Mirror, 0);
        }
    }

    class PunchEffectStraight : SlashEffectRound
    {
        public PunchEffectStraight(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, anchor, size, angle, mirror, time)
        {
        }

        public override void Draw(SceneGame scene)
        {
            var punchStraight = SpriteLoader.Instance.AddSprite("content/punch");
            var punchAngle = Angle;
            if (Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                punchAngle = -punchAngle;
            scene.DrawSpriteExt(punchStraight, scene.AnimationFrame(punchStraight, Frame, FrameEnd), Position - punchStraight.Middle, punchStraight.Middle, punchAngle, Mirror, 0);
        }
    }

    abstract class Particle : VisualEffect
    {
        public virtual Vector2 Position
        {
            get;
            set;
        }

        public Particle(GameWorld world, Vector2 position) : base(world)
        {
            Position = position;
        }
    }

    class ParryEffect : Particle
    {
        public float Angle;
        public float FrameEnd;

        public ParryEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position)
        {
            Angle = angle;
            FrameEnd = time;
        }

        public override void Update(float delta)
        {
            base.Update(1.0f);
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var crit = SpriteLoader.Instance.AddSprite("content/crit");
            scene.DrawSpriteExt(crit, scene.AnimationFrame(crit, Frame, FrameEnd), Position - crit.Middle, crit.Middle, Angle, SpriteEffects.None, 0);
        }
    }

    class FireEffect : Particle
    {
        public float Angle;
        public float FrameEnd;

        public FireEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position)
        {
            Angle = angle;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var fire = SpriteLoader.Instance.AddSprite("content/fire_small");
            var middle = new Vector2(8, 12);
            scene.DrawSpriteExt(fire, scene.AnimationFrame(fire, Frame, FrameEnd), Position - middle, middle, Angle, SpriteEffects.None, 0);
        }
    }

    class BigFireEffect : FireEffect
    {
        public BigFireEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position, angle, time)
        {
        }

        public override void Draw(SceneGame scene)
        {
            var fireBig = SpriteLoader.Instance.AddSprite("content/fire_big");
            var middle = new Vector2(8, 12);
            scene.DrawSpriteExt(fireBig, scene.AnimationFrame(fireBig, Frame, FrameEnd), Position - middle, middle, Angle, SpriteEffects.None, 0);
        }
    }

    class BloodSpatterEffect : FireEffect
    {
        public BloodSpatterEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position, angle, time)
        {
        }

        public override void Update(float delta)
        {
            base.Update(1.0f);
        }

        public override void Draw(SceneGame scene)
        {
            var bloodSpatter = SpriteLoader.Instance.AddSprite("content/blood_spatter");
            scene.DrawSpriteExt(bloodSpatter, scene.AnimationFrame(bloodSpatter, Frame, FrameEnd), Position - bloodSpatter.Middle, bloodSpatter.Middle, Angle, SpriteEffects.None, 0);
        }
    }

    class BloodDrop : Particle
    {
        public Vector2 Velocity;
        public float FrameEnd;
        public float Rotation;

        public BloodDrop(GameWorld world, Vector2 position, Vector2 velocity, float rotation, float time) : base(world, position)
        {
            Velocity = velocity;
            Rotation = rotation;
            FrameEnd = time;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Position += Velocity * delta;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
            Velocity += new Vector2(0, 0.4f);
        }

        public override void Draw(SceneGame scene)
        {
            throw new NotImplementedException();
        }
    }

    class KnifeBounced : Particle
    {
        public Vector2 Velocity;
        public float FrameEnd;
        public float Rotation;

        public KnifeBounced(GameWorld world, Vector2 position, Vector2 velocity, float rotation, float time) : base(world, position)
        {
            Velocity = velocity;
            Rotation = rotation;
            FrameEnd = time;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Position += Velocity * delta;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
            Velocity += new Vector2(0, 0.4f);
        }

        public override void Draw(SceneGame scene)
        {
            var knife = SpriteLoader.Instance.AddSprite("content/knife");
            scene.DrawSpriteExt(knife, 0, Position - knife.Middle, knife.Middle, Rotation * Frame, SpriteEffects.None, 0);
        }
    }

    class SnakeHead : Particle
    {
        public Vector2 Velocity;
        public SpriteEffects Mirror;
        public float Rotation;
        public float FrameEnd;

        public SnakeHead(GameWorld world, Vector2 position, Vector2 velocity, SpriteEffects mirror, float rotation, float time) : base(world, position)
        {
            Velocity = velocity;
            FrameEnd = time;
            Mirror = mirror;
            Rotation = rotation;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Position += Velocity * delta;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
            Velocity += new Vector2(0, 0.4f);
        }

        public override void Draw(SceneGame scene)
        {
            var snakeHeadOpen = SpriteLoader.Instance.AddSprite("content/snake_open");
            scene.DrawSpriteExt(snakeHeadOpen, 0, Position - snakeHeadOpen.Middle, snakeHeadOpen.Middle, Rotation * Frame, Mirror, 0);
        }
    }

    class DamagePopup : Particle
    {
        public float FrameEnd;
        public string Text;
        public Color FontColor;
        public Color BorderColor;
        public Vector2 Offset => new Vector2(0,-16) * (float)LerpHelper.QuadraticOut(0,1,Frame/FrameEnd);

        public DamagePopup(GameWorld world, Vector2 position, string text, float time, Color? fontColor = null, Color? borderColor = null) : base(world, position)
        {
            Text = text;
            FrameEnd = time;
            FontColor = fontColor ?? Color.White;
            BorderColor = borderColor ?? Color.Black;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var calcParams = new TextParameters().SetColor(FontColor, BorderColor).SetConstraints(128, 64);
            string fit = FontUtil.FitString(Game.ConvertToSmallPixelText(Text), calcParams);
            var width = FontUtil.GetStringWidth(fit, calcParams);
            var height = FontUtil.GetStringHeight(fit);
            scene.DrawText(fit, Position + Offset - new Vector2(128, height) / 2, Alignment.Center, new TextParameters().SetColor(FontColor, BorderColor).SetConstraints(128, height + 64));
        }
    }

    class ItemPickup : VisualEffect
    {
        Item Item;
        Vector2 PositionWorld;
        Vector2 PositionBag;
        float FrameEnd;
        Vector2 Offset;

        public ItemPickup(GameWorld world, Item item, Vector2 position, Vector2 positionBag, float time) : base(world)
        {
            Item = item;
            PositionWorld = position;
            PositionBag = positionBag;
            FrameEnd = time;
            Offset = Util.AngleToVector(Random.NextFloat() * MathHelper.TwoPi) * 40;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var posBag = Vector2.Transform(PositionBag, Matrix.Invert(scene.WorldTransform));
            var offset = Offset * (float)Math.Sin(Frame / FrameEnd * Math.PI);
            Item.DrawIcon(scene, Vector2.Lerp(PositionWorld, posBag, (float)LerpHelper.CircularIn(0, 1, Frame / FrameEnd)) + offset);
        }
    }

    class RectangleDebug : VisualEffect
    {
        public RectangleF Rectangle;
        public Color Color;
        public int FrameEnd;
        
        public RectangleDebug(GameWorld world, RectangleF rect, Color color, int time) : base(world)
        {
            Rectangle = rect;
            Color = color;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            scene.SpriteBatch.Draw(scene.Pixel, Rectangle.ToRectangle(), Color);
        }
    }

    class ChargeEffect : Particle
    {
        public override Vector2 Position
        {
            get
            {
                return Human.Position;
            }
            set
            {
                //NOOP
            }
        }

        public float Angle;
        public float FrameEnd;
        public EnemyHuman Human;
        SoundChannel Sound;

        public ChargeEffect(GameWorld world, Vector2 position, float angle, float time, EnemyHuman human) : base(world, position)
        {
            Angle = angle;
            FrameEnd = time;
            Human = human;
            Sound = Game.sfx_player_charging.Play();
            Sound.Pitch = 0;
            Sound.Looping = true;
        }

        public override void Destroy()
        {
            base.Destroy();
            StopSound();
        }

        private void StopSound()
        {
            Sound.Looping = false;
            Sound.Stop();
        }

        public override void Update(float delta)
        {
            if (!(Human.CurrentAction is ActionCharge))
            {
                Destroy();
            }
            base.Update(1.0f);
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Sound.Pitch = (float)LerpHelper.CircularOut(0f, 2f, Frame / FrameEnd);
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var charge = SpriteLoader.Instance.AddSprite("content/charge");
            scene.DrawSpriteExt(charge, (int)-Frame, Position - charge.Middle, charge.Middle, Angle, SpriteEffects.None, 0);
        }
    }

    class AimingReticule : Particle
    {
        public float FrameEnd;
        public EnemyHuman Player;
        public AimingReticule(GameWorld world, Vector2 position, EnemyHuman human) : base(world, position)
        {
            FrameEnd = float.PositiveInfinity;
            Player = human;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            if(!(Player.CurrentAction is ActionAiming))
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var aimingreticule = SpriteLoader.Instance.AddSprite("content/aim_reticule");
            scene.DrawSpriteExt(aimingreticule, 0, Position - aimingreticule.Middle, aimingreticule.Middle, 0, SpriteEffects.None, 0);
        }
    }

    abstract class StatusEffectVisual<T> : VisualEffect where T : StatusEffect
    {
        protected T Effect;
        protected Enemy Enemy => Effect.Enemy;
        protected Vector2 Position
        {
            get
            {
                if (Enemy is EnemyHuman enemy)
                    return new Vector2(enemy.Box.Bounds.Center.X, enemy.Box.Bounds.Top - 16);
                else
                    return Enemy.Position - new Vector2(0, 16);
            }
        }

        public StatusEffectVisual(GameWorld world, T effect) : base(world)
        {
            Effect = effect;
        }

        protected override void UpdateDiscrete()
        {
            if (Effect.Removed || Effect.Enemy.Destroyed)
            {
                Destroy();
            }
        }
    }

    class StatusPoisonEffect : StatusEffectVisual<Poison>
    {
        public StatusPoisonEffect(GameWorld world, Poison effect) : base(world, effect)
        {
        }

        public override void Draw(SceneGame scene)
        {
            var statusPoisoned = SpriteLoader.Instance.AddSprite("content/status_poisoned");
            scene.DrawSpriteExt(statusPoisoned, (int)(Frame * 0.25f), Position - statusPoisoned.Middle, statusPoisoned.Middle, 0, SpriteEffects.None, 0);
        }
    }

    class StatusSlowEffect : StatusEffectVisual<Slow>
    {
        public StatusSlowEffect(GameWorld world, Slow effect) : base(world, effect)
        {
        }

        public override void Draw(SceneGame scene)
        {
            var statusSlowed = SpriteLoader.Instance.AddSprite("content/status_slowed");
            float slide = (Frame * 0.01f) % 1;
            float angle = 0;
            if (slide < 0.1f)
            {
                angle = MathHelper.Lerp(0, MathHelper.Pi, slide / 0.1f);
            }
            scene.DrawSpriteExt(statusSlowed, 0, Position - statusSlowed.Middle, statusSlowed.Middle, angle, SpriteEffects.None, 0);
        }
    }

   class StatusStunEffect : StatusEffectVisual<Stun>
    {
        public StatusStunEffect(GameWorld world, Stun effect) : base(world, effect)
        {
        }

        public override void Draw(SceneGame scene)
        {
            var statusStunned = SpriteLoader.Instance.AddSprite("content/status_stunned");
            float radius = 8;
            float circleSpeed = 0.15f;
            var offset = new Vector2(radius * (float)Math.Sin(Frame * Math.PI * circleSpeed), (radius / 2) * (float)Math.Cos(Frame * Math.PI * circleSpeed));
            scene.DrawSpriteExt(statusStunned, (int)(Frame * 0.3f), Position + offset - statusStunned.Middle, statusStunned.Middle, 0, SpriteEffects.None, 0);
        }
    }
}
