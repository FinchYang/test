using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.BimInfo
{
    public abstract class BaseElement : IEquatable<BaseElement>
    {
        //public string Url { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }

        public virtual string GetKey()
        {
            return Id.ToString();
        }

        public override bool Equals(object obj)
        {
            var other = obj as BaseElement;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(BaseElement other)
        {
            if (other == null) return false;
            return Id == other.Id;
        }
    }
}
