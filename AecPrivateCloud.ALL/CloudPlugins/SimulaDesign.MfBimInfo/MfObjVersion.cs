using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.MfBimInfo
{
    public class MfObjVersion
    {
        public static readonly string Sep = "_";
        public int ObjType { get; set; }

        public int Id { get; set; }

        public int Version { get; set; }

        public override string ToString()
        {
            return ObjType + Sep + Id + Sep + Version;
        }

        public static readonly string ArraySep = ";";

        public static MfObjVersion Parse(string str)
        {
            var strs = str.Split(Sep.ToArray());
            if (strs.Length < 2 || strs.Length > 3) return null;
            var typeStr = strs[0];
            int type;
            var ok = int.TryParse(typeStr, out type);
            if (!ok) return null;
            var idStr = strs[1];
            int id;
            ok = int.TryParse(idStr, out id);
            if (!ok) return null;
            var version = -1;
            if (strs.Length == 3)
            {
                var versionStr = strs[2];
                int ver;
                ok = int.TryParse(versionStr, out ver);
                if (!ok) return null;
                version = ver;
            }
            return new MfObjVersion { ObjType = type, Id = id, Version = version };
        }
    }
}
