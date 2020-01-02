using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class Message
    {
        public string Text;

        public Message(string text)
        {
            Text = text;
        }

        public virtual bool CanCombine(Message other)
        {
            return false;
        }

        public virtual Message Combine(Message other)
        {
            return this;
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
