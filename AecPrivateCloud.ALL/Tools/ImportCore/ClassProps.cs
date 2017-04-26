using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using MFilesAPI;

namespace SimulaDesign.ImportCore
{
    public class ClassProps
    {
        private static readonly List<int> ExcludeProps = new List<int> { 20, 25, 23, 21, 24, 22, 30, 31, 37, 101, 32, 89 };

        private readonly static Dictionary<int, MfPropDef> PropDict = new Dictionary<int, MfPropDef>();

        public static SelectedFiles GetFiles(string dir)
        {
            var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            if (!dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                dir += Path.DirectorySeparatorChar;
            }
            
            var sf = new SelectedFiles {RootDir = dir};
            foreach (var f in files)
            {
                var rf = f.Substring(dir.Length);
                var selF = new SelectedFile {Filepath = rf, NewFilename = Path.GetFileName(f)};
                sf.Files.Add(selF);
            }
            return sf;
        }

        private static MfPropDef GetProp(Vault vault, int def)
        {
            if (!PropDict.ContainsKey(def))
            {
                var propDef = vault.PropertyDefOperations.GetPropertyDef(def);
                if (propDef.AutomaticValueType != MFAutomaticValueType.MFAutomaticValueTypeNone)
                {
                    return null;
                }
                var mp = new MfPropDef
                {
                    Def = def,
                    DataType = (int)propDef.DataType,
                    Name = propDef.Name,
                    ValueList = propDef.ValueList
                };
                PropDict.Add(def, mp);
            }
            return PropDict[def];
        }

        internal static List<MfClassPropDef> GetClassProps(Vault vault, int classId)
        {
            var oc = vault.ClassOperations.GetObjectClass(classId);
            var mc = new List<MfClassPropDef>();

            var namedPropDef = GetProp(vault, oc.NamePropertyDef);
            if (namedPropDef != null)
            {
                var np = new MfClassPropDef { PropDef = namedPropDef, NamedProp = true, Required = true, ClassId = classId };
                mc.Add(np);
            }

            foreach (AssociatedPropertyDef p in oc.AssociatedPropertyDefs)
            {
                var def = p.PropertyDef;
                if (def == oc.NamePropertyDef) continue;
                if (ExcludeProps.Contains(def)) continue;
                
                var mp = GetProp(vault, def);
                if (mp == null) continue;
                var required = p.Required;
                var np0 = new MfClassPropDef {PropDef = mp, Required = required, ClassId = classId};
                mc.Add(np0);
            }
            return mc;
        }
    }

    public abstract class MfType : IEquatable<MfType>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(MfType other)
        {
            if (other == null) return false;
            return Id == other.Id && GetType() == other.GetType();
        }

        public override bool Equals(object obj)
        {
            var other = obj as MfType;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
    /// <summary>
    /// 对象类别
    /// </summary>
    public class MfClass : MfType
    {
        
    }
    /// <summary>
    /// 对象类型
    /// </summary>
    public class MfObjType : MfType
    {
        public bool IsDocType()
        {
            return Id == (int)(MFBuiltInObjectType.MFBuiltInObjectTypeDocument);
        }
        
    }

    public class ClassPropSets
    {
        /// <summary>
        /// 是否为上次更新
        /// </summary>
        public bool LastUpdated { get; set; }

        public MfObjType ObjType { get; set; }
        public MfClass ObjectClass { get; set; }

        private List<MfClassPropDef> _props = new List<MfClassPropDef>();

        public List<MfClassPropDef> Props
        {
            get { return _props; }
        }
    }

    public class MfPropDef : IEquatable<MfPropDef>
    {
        /// <summary>
        /// 属性定义ID
        /// </summary>
        public int Def { get; set; }
        /// <summary>
        /// 属性定义名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 属性定义的数据类型
        /// </summary>
        public int DataType { get; set; }
        /// <summary>
        /// 属性定义的值列表ID
        /// </summary>
        public int ValueList { get; set; }

        public bool IsTextProp()
        {
            return DataType == (int) MFDataType.MFDatatypeText || DataType == (int) MFDataType.MFDatatypeMultiLineText;
        }

        public bool Equals(MfPropDef other)
        {
            if (other == null) return false;
            return Def == other.Def;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MfPropDef;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Def;
        }

        public override string ToString()
        {
            return Name;
        }

        private static string TypeDesc(MFDataType type)
        {
            switch (type)
            {
                case MFDataType.MFDatatypeBoolean:
                    return "布尔";
                case MFDataType.MFDatatypeDate:
                case MFDataType.MFDatatypeTime:
                case MFDataType.MFDatatypeTimestamp:
                    return "日期";
                case MFDataType.MFDatatypeFloating:
                case MFDataType.MFDatatypeInteger:
                case MFDataType.MFDatatypeInteger64:
                    return "数字";
            }
            return "字符串";
        }

        public string GetDesc()
        {
            return "名称: " + Name + "; 类型: " + TypeDesc((MFDataType)DataType);
        }
    }
    public class MfClassPropDef : IEquatable<MfClassPropDef>
    {
        /// <summary>
        /// 类别ID
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// 属性定义名称
        /// </summary>
        [XmlIgnore]
        public string Name { get { return PropDef.Name; } }
        /// <summary>
        /// 属性定义ID
        /// </summary>
        [XmlIgnore]
        public int Def { get { return PropDef.Def; } }
        /// <summary>
        /// 属性定义的数据类型
        /// </summary>
        [XmlIgnore]
        public int DataType { get { return PropDef.DataType; } }
        /// <summary>
        /// 对应的值列表ID
        /// </summary>
        [XmlIgnore]
        public int ValueList { get { return PropDef.ValueList; } }

        public MfPropDef PropDef { get; set; }
        /// <summary>
        /// 是否是必填属性
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// 是否是类别的命名属性
        /// </summary>
        public bool NamedProp { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(MfClassPropDef other)
        {
            if (other == null) return false;
            return ClassId == other.ClassId && Def == other.Def;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MfClassPropDef;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return ClassId ^ Def;
        }
    }

    public class SelectedFile
    {
        public string NewFilename { get; set; }

        public string Filepath { get; set; }
    }

    public class SelectedFiles
    {
        /// <summary>
        /// 最大层数
        /// </summary>
        public int LayerCount {
            get
            {
                var layer = 0;
                foreach (var rf in Files)
                {
                    var len = rf.Filepath.Split(Path.DirectorySeparatorChar).Length;
                    if (len > layer)
                    {
                        layer = len;
                    }
                }
                return layer;
            } 
        }
        public SelectedFiles()
        {
            Files = new List<SelectedFile>();
        }
        /// <summary>
        /// 选择的根目录
        /// </summary>
        public string RootDir { get; set; }
        /// <summary>
        /// 相对于根目录的路径名称
        /// </summary>
        public List<SelectedFile> Files { get; private set; }

        /// <summary>
        /// 获得完整路径
        /// </summary>
        /// <param name="file">相对于根的文件路径</param>
        /// <returns></returns>
        public string GetFullpath(string file)
        {
            return Path.Combine(RootDir, file);
        }
        /// <summary>
        /// 具有最大层级的文件
        /// </summary>
        public SelectedFile MaxLayerFile
        {
            get
            {
                var layerCount = LayerCount;
                return Files.FirstOrDefault(c => c.Filepath.Split(Path.DirectorySeparatorChar).Length == layerCount);
            }
        }
        /// <summary>
        /// 获取文件夹层次列表，文件名或放在第一位，后位补空字符串
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<string> GetProps(SelectedFile file)
        {
            return GetProps(file, LayerCount);
        }

        private static List<string> GetProps(SelectedFile file, int propCount)
        {
            var filePath = file.Filepath;
            var strs = filePath.Split(Path.DirectorySeparatorChar);
            var emptyPropCount = propCount - strs.Length;
            var list = new List<string>();
            var fileName = file.NewFilename;
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = strs[strs.Length - 1];
            }
            list.Add(fileName);
            list.AddRange(strs.Take(strs.Length - 1));
            if (emptyPropCount > 0)
            {
                list.AddRange(Enumerable.Range(0, emptyPropCount).Select(c => ""));
            }
            return list;
        }

    }
}
