using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    //TODO: this could be a struct
    class Slider
    {
        public float Time;
        public float EndTime;
        public float Slide => Time / EndTime;
        public bool Done => Time >= EndTime;

        public Slider(float time, float endTime)
        {
            Time = time;
            EndTime = endTime;
        }

        public Slider(float time) : this(0, time)
        {

        }

        public static Slider operator +(Slider slider, float i)
        {
            slider.Time = MathHelper.Clamp(slider.Time + i, 0, slider.EndTime);
            return slider;
        }

        public static Slider operator -(Slider slider, float i)
        {
            slider.Time = MathHelper.Clamp(slider.Time - i, 0, slider.EndTime);
            return slider;
        }

        public static bool operator <(Slider slider, float i)
        {
            return slider.Time < i;
        }

        public static bool operator >(Slider slider, float i)
        {
            return slider.Time > i;
        }

        public static bool operator <=(Slider slider, float i)
        {
            return slider.Time <= i;
        }

        public static bool operator >=(Slider slider, float i)
        {
            return slider.Time >= i;
        }

        public override string ToString()
        {
            return $"{Time} ({Slide})";
        }
    }
}
