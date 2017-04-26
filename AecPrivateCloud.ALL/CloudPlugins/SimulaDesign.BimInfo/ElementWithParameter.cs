using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.BimInfo
{
    public abstract class ElementWithParameter : ElementWithGuid
    {
        private readonly List<ElementParameter> _params = new List<ElementParameter>();

        public List<ElementParameter> Parameters
        {
            get { return _params; }
        }
    }

    public abstract class ElementWithGuid : BaseElement, IEquatable<ElementWithGuid>
    {
        public string Guid { get; set; }

        public override string GetKey()
        {
            return Guid;
        }

        public bool Equals(ElementWithGuid other)
        {
            if (other == null) return false;
            return Guid == other.Guid;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ElementWithGuid;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}
