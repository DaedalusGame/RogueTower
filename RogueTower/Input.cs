using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    public struct InputState
    {
        public InputState(KeyboardState keyboard, MouseState mouse, GamePadState gamepad, string inputText)
        {
            Keyboard = keyboard;
            Mouse = mouse;
            GamePad = gamepad;
            InputText = inputText;
        }

        public KeyboardState Keyboard;
        public MouseState Mouse;
        public GamePadState GamePad;
        public string InputText;
    }

    public class InputTwinState
    {
        class KeyRepeat
        {
            public int TimePressed;
            public int RepeatCount;
        }

        public InputTwinState(InputState previous, InputState next, InputTwinState old)
        {
            Previous = previous;
            Next = next;
            if (old != null)
            {
                KeyRepeats = old.KeyRepeats;
                ButtonRepeats = old.ButtonRepeats;
            }
            else
            {
                KeyRepeats = new Dictionary<Keys, KeyRepeat>();
                ButtonRepeats = new Dictionary<Buttons, KeyRepeat>();
            }

        }

        public InputState Previous;
        public InputState Next;
        Dictionary<Keys, KeyRepeat> KeyRepeats;
        Dictionary<Buttons, KeyRepeat> ButtonRepeats;

        public void HandleRepeats()
        {
            foreach (var keyRepeat in KeyRepeats)
            {
                if (IsKeyDown(keyRepeat.Key))
                    keyRepeat.Value.TimePressed++;
                else
                    keyRepeat.Value.TimePressed = 0;
            }

            foreach (var buttonRepeat in ButtonRepeats)
            {
                if (IsButtonDown(buttonRepeat.Key))
                    buttonRepeat.Value.TimePressed++;
                else
                    buttonRepeat.Value.TimePressed = 0;
            }
        }

        public bool IsKeyDown(Keys key)
        {
            return Next.Keyboard.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return Next.Keyboard.IsKeyUp(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return Next.Keyboard.IsKeyDown(key) && Previous.Keyboard.IsKeyUp(key);
        }

        public bool IsKeyPressed(Keys key, int repeatStart, int repeatStep)
        {
            KeyRepeat repeat = new KeyRepeat();
            if (!KeyRepeats.ContainsKey(key))
                KeyRepeats.Add(key, new KeyRepeat());
            else
                repeat = KeyRepeats[key];
            return IsKeyPressed(key) || (repeat.TimePressed > repeatStart && (repeat.TimePressed - repeatStart) % repeatStep == 0);
        }

        public bool IsKeyPressed(Keys key, int delay)
        {
            KeyRepeat repeat = new KeyRepeat();
            if (!KeyRepeats.ContainsKey(key))
                KeyRepeats.Add(key, new KeyRepeat());
            else
                repeat = KeyRepeats[key];
            return repeat.TimePressed == delay;
        }

        public bool IsKeyReleased(Keys key)
        {
            return Next.Keyboard.IsKeyUp(key) && Previous.Keyboard.IsKeyDown(key);
        }

        public bool IsButtonDown(Buttons button)
        {
            return Next.GamePad.IsButtonDown(button);
        }

        public bool IsButtonUp(Buttons button)
        {
            return Next.GamePad.IsButtonUp(button);
        }

        public bool IsButtonPressed(Buttons button)
        {
            return Next.GamePad.IsButtonDown(button) && Previous.GamePad.IsButtonUp(button);
        }

        public bool IsButtonPressed(Buttons button, int repeatStart, int repeatStep)
        {
            KeyRepeat repeat = new KeyRepeat();
            if (!ButtonRepeats.ContainsKey(button))
                ButtonRepeats.Add(button, new KeyRepeat());
            else
                repeat = ButtonRepeats[button];
            return IsButtonPressed(button) || (repeat.TimePressed > repeatStart && (repeat.TimePressed - repeatStart) % repeatStep == 0);
        }

        public bool IsButtonPressed(Buttons button, int delay)
        {
            KeyRepeat repeat = new KeyRepeat();
            if (!ButtonRepeats.ContainsKey(button))
                ButtonRepeats.Add(button, new KeyRepeat());
            else
                repeat = ButtonRepeats[button];
            return repeat.TimePressed == delay;
        }

        public bool IsButtonReleased(Buttons button)
        {
            return Next.GamePad.IsButtonUp(button) && Previous.GamePad.IsButtonDown(button);
        }
    }
}
