using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class Healthbar
    {
        Func<double> Binding;
        LerpHelper.Delegate Lerp;
        double Speed;

        double LastValue;
        double Value;
        double Slide => FrameEnd == 0 ? 1 : Math.Max(Math.Min(Frame / FrameEnd, 1), 0);

        double Frame, FrameEnd;

        public double CurrentValue => Lerp(LastValue, Value,Math.Max(Math.Min(Slide,1),0));

        public Healthbar(Func<double> binding, LerpHelper.Delegate lerp, double speed)
        {
            Binding = binding;
            Lerp = lerp;
            Speed = speed;
        }

        public void Update(float delta)
        {
            Frame += delta;

            var newValue = Binding();
            if (Value != newValue)
            {
                SetValue(newValue);
            }
        }

        public void SetValue(double value)
        {
            LastValue = CurrentValue;
            Value = value;
            Frame = 0;
            FrameEnd = Math.Abs(Value - LastValue) / Speed;
        }
    }
}
