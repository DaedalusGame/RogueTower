using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class IconRender
    {
        SceneGame Scene;
        Action<SceneGame, Vector2> Render;

        public IconRender(Action<SceneGame,Vector2> render)
        {
            Render = render;
        }

        public void SetScene(SceneGame scene)
        {
            Scene = scene;
        }

        public void Draw(Vector2 pos)
        {
            Render(Scene, pos);
        }
    }

    abstract class Message
    {
        public abstract string RenderText(Enemy owner);
        public abstract IconRender[] Icons
        {
            get;
        }

        public virtual bool CanCombine(Message other)
        {
            return false;
        }

        public virtual Message Combine(Message other)
        {
            return this;
        }

        protected string Macro(Item item, Enemy owner) => $"{Game.FORMAT_ICON}{item.GetName(owner)}";
    }

    class MessageText : Message
    {
        string Text;

        public override string RenderText(Enemy owner) => Text;
        public override IconRender[] Icons => new IconRender[0];

        public MessageText(string text)
        {
            Text = text;
        }
    }

    class MessageItemTransform : Message
    {
        Item PreviousItem;
        Item CurrentItem;

        public override string RenderText(Enemy owner) => $"{Macro(PreviousItem,owner)} became {Macro(CurrentItem, owner)}.";
        public override IconRender[] Icons => new[]
        {
            new IconRender(PreviousItem.DrawIcon),
            new IconRender(CurrentItem.DrawIcon),
        };

        public MessageItemTransform(Item previous, Item current)
        {
            PreviousItem = previous;
            CurrentItem = current;
        }
    }

    class MessageItem : Message
    {
        Item Item;
        string Text;

        public override string RenderText(Enemy owner) => string.Format(Text, Macro(Item, owner));
        public override IconRender[] Icons => new[]
        {
            new IconRender(Item.DrawIcon),
        };

        public MessageItem(Item item, string text)
        {
            Text = text;
            Item = item;
        }
    }

    class MessageHistory
    {
        public List<Message> Messages = new List<Message>();

        public void Add(Message next)
        {
            var previous = Messages.LastOrDefault();
            if(previous != null && previous.CanCombine(next))
            {
                Messages.RemoveAt(Messages.Count - 1);
                Messages.Add(previous.Combine(next));
            }
            else
            {
                Messages.Add(next);
            }
        }
    }
}
