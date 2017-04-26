using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SimulaDesign.BimInfo
{
    /// <summary>
    /// 参数
    /// </summary>
    public class ElementParameter : IComparable<ElementParameter>, IComparable, IEquatable<ElementParameter>
    {
        public const string Sep = ": \t";
        public int Id { get; set; }
        /// <summary>
        /// 共享参数才会有GUID
        /// </summary>
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Name + Sep + Value;
        }

        public int CompareTo(ElementParameter other)
        {
            if (other == null) return 1;
            return String.Compare(Name, other.Name);
        }

        int IComparable.CompareTo(object obj)
        {
            var other = obj as ElementParameter;
            return CompareTo(other);
        }

        public bool Equals(ElementParameter other)
        {
            if (other == null) return false;
            return other.Name == Name && other.Value == Value;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Value.GetHashCode();
        }
    }
}
