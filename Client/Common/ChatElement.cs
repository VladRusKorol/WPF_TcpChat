using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Client.Common
{
    public class ChatElement
    {
        public string Name;
        public string Text;
        public ChatElement(string name, string text) {
            Name = name;
            Text = text;
        }
    }
}
