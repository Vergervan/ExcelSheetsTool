using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VladimirsTool.Models
{
    public class Header
    {
        public string Value { get; set; }

        public static implicit operator string(Header obj)
        {
            return obj.Value;
        }
        public static implicit operator Header(string str)
        {
            return new Header(str);
        }
        public Header(string value)
        {
            this.Value = value;
        }
        public Header() { }
    }
}
