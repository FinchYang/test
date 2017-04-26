using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using MFilesAPI;

namespace AecCloud.MFilesCore.Metadata
{
    [XmlRoot("structure")]
    public class MetadataStructure
    {
        //private static Dictionary<int, string> _propNameDict = new Dictionary<int, string>
        //{
        //    {0, "MFBuiltInPropertyDefNameOrTitle"},
        //    {20, "MFBuiltInPropertyDefCreated"},
        //    {21, "MFBuiltInPropertyDefLastModified"},
        //    {22, "MFBuiltInPropertyDefSingleFileObject"},
        //    {23, "MFBuiltInPropertyDefLastModifiedBy"},
        //    {24, "MFBuiltInPropertyDefStatusChanged"},
        //    {25, "MFBuiltInPropertyDefCreatedBy"},
        //    {26, "MFBuiltInPropertyDefKeywords"},
        //    {27, "MFBuiltInPropertyDefDeleted"},
        //    {28, "MFBuiltInPropertyDefDeletedBy"},
        //    {29, "MFBuiltInPropertyDefVersionLabel"},
        //    {30, "MFBuiltInPropertyDefSizeOnServerThisVersion"},
        //    {31, "MFBuiltInPropertyDefSizeOnServerAllVersions"},
        //    {32, "MFBuiltInPropertyDefMarkedForArchiving"},
        //    {33, "MFBuiltInPropertyDefVersionComment"},
        //    {34, "MFBuiltInPropertyDefTraditionalFolder"},
        //    {35, "MFBuiltInPropertyDefCreatedFromExternalLocation"},
        //    {36, "MFBuiltInPropertyDefAdditionalClasses"},
        //    {37, "MFBuiltInPropertyDefIsTemplate"},
        //    {38, "MFBuiltInPropertyDefWorkflow"},
        //    {39, "MFBuiltInPropertyDefState"},
        //    {40, "MFBuiltInPropertyDefStateEntered"},
        //    {41, "MFBuiltInPropertyDefAssignmentDescription"},
        //    {42, "MFBuiltInPropertyDefDeadline"},
        //    {43, "MFBuiltInPropertyDefMonitoredBy"},
        //    {44, "MFBuiltInPropertyDefAssignedTo"},
        //    {45, "MFBuiltInPropertyDefCompletedBy"},
        //    {46, "MFBuiltInPropertyDefCollectionMemberDocuments"},
        //    {47, "MFBuiltInPropertyDefCollectionMemberCollections"},
        //    {48, "MFBuiltInPropertyDefConstituent"},
        //    {75, "MFBuiltInPropertyDefOriginalPath"},
        //    {76, "MFBuiltInPropertyDefReference"},
        //    {77, "MFBuiltInPropertyDefOriginalPath2"},
        //    {78, "MFBuiltInPropertyDefOriginalPath3"},
        //    {79, "MFBuiltInPropertyDefWorkflowAssignment"},
        //    {81, "MFBuiltInPropertyDefAccessedByMe"},
        //    {82, "MFBuiltInPropertyDefFavoriteView"},
        //    {83, "MFBuiltInPropertyDefMessageID"},
        //    {84, "MFBuiltInPropertyDefInReplyTo"},
        //    {85, "MFBuiltInPropertyDefInReplyToReference"},
        //    {86, "MFBuiltInPropertyDefSignatureManifestation"},
        //    {87, "MFBuiltInPropertyDefReportURL"},
        //    {88, "MFBuiltInPropertyDefReportPlacement"},
        //    {89, "MFBuiltInPropertyDefObjectChanged"},
        //    {90, "MFBuiltInPropertyDefACLChanged"},
        //    {91, "MFBuiltInPropertyDefVersionLabelChanged"},
        //    {92, "MFBuiltInPropertyDefVersionCommentChanged"},
        //    {93, "MFBuiltInPropertyDefDeletionStatusChanged"},
        //    {94, "MFBuiltInPropertyDefVaultGUID"},
        //    {95, "MFBuiltInPropertyDefSharedFiles"},
        //    {96, "MFBuiltInPropertyDefConflictResolved"},
        //    {100, "MFBuiltInPropertyDefClass"},
        //    {101, "MFBuiltInPropertyDefClassGroups"},
        //    {-102, "MFBuiltInPropertyDefObjectID"}
        //};

        //private static Dictionary<int, string> _objTypeNameDict = new Dictionary<int, string>
        //{
        //    {0, "MFBuiltInObjectTypeDocument"},
        //    {9, "MFBuiltInObjectTypeDocumentCollection"},
        //    {10, "MFBuiltInObjectTypeAssignment"},
        //    {-102, "MFBuiltInObjectTypeDocumentOrDocumentCollection"}
        //};


        //private static Dictionary<int, string> _objClassNameDict = new Dictionary<int, string>
        //{
        //    {-100, "MFBuiltInObjectClassGenericAssignment"},
        //    {-3, "MFBuiltInObjectClassAny"},
        //    {-2, "MFBuiltInObjectClassNotSet"}
        //};

        private static string GetName(Dictionary<int, string> dict, int id)
        {
            if (dict.ContainsKey(id)) return dict[id];
            return null;
        }


        private static List<int> _ignorePropList = new List<int> { 20, 25, 21, 23, 24, 89, 22, 101, 30, 31, 32 };
        internal static bool IsTrue(string literal)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(literal, "TRUE");
        }

        private readonly List<MfObjType> _objTypes = new List<MfObjType>();

        [XmlArray("objtypes")]
        [XmlArrayItem(ElementName = "objtype", Type = typeof(MfObjType))]
        public List<MfObjType> ObjTypes
        {
            get { return _objTypes; }
        }

        private readonly List<MfPropertyDef> _propDefs = new List<MfPropertyDef>();
        [XmlArray("propertydefs")]
        [XmlArrayItem(elementName: "propertydef", type:typeof(MfPropertyDef))]
        public List<MfPropertyDef> PropertyDefs
        {
            get { return _propDefs; }
        }

        private readonly List<MfNamedAcl> _namedAcls = new List<MfNamedAcl>();
        [XmlArray("namedacls")]
        [XmlArrayItem(elementName: "namedacl", type: typeof(MfNamedAcl))]
        public List<MfNamedAcl> NamedAcls
        {
            get { return _namedAcls; }
        }

        private readonly List<MfClass> _classes = new List<MfClass>();
        [XmlArray("classes")]
        [XmlArrayItem(elementName: "class", type: typeof(MfClass))]
        public List<MfClass> Classes
        {
            get { return _classes; }
        }

        private readonly List<MfUserGroup> _groups = new List<MfUserGroup>();
        [XmlArray("usergroups")]
        [XmlArrayItem(elementName: "group", type: typeof(MfUserGroup))]
        public List<MfUserGroup> Groups
        {
            get { return _groups; }
        }

        private readonly List<MfView> _views = new List<MfView>();
        [XmlArray("viewdefs")]
        [XmlArrayItem(elementName: "view", type: typeof(MfView))]
        public List<MfView> Views
        {
            get { return _views; }
        }

        private readonly Dictionary<string, MfPropertyDef> _propDict = new Dictionary<string, MfPropertyDef>();

        public MfPropertyDef GetPropDefById(string id)
        {
            if (!_propDict.ContainsKey(id))
            {
                var p = _propDefs.FirstOrDefault(c => c.Id == id);
                _propDict.Add(id, p);
            }
            return _propDict[id];
        }

        public static MetadataStructure GetFromFile(string metadataFile)
        {
            var ser = new XmlSerializer(typeof (MetadataStructure));
            using (var fs = new FileStream(metadataFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var obj = (MetadataStructure)ser.Deserialize(fs);
                return obj;
            }
        }

        public MetadataAliases GetAliases(bool withName=false)
        {
            var aliases = new MetadataAliases();
            var valueList = _objTypes.Where(c => !String.IsNullOrEmpty(c.Aliases) && !c.IsRealObj).ToArray();
            if (withName)
            {
                foreach (var v in valueList)
                {
                    aliases.ValueLists.Add(v.Aliases, v.Name);
                }
            }
            else
            {
                foreach (var v in valueList)
                {
                    aliases.ValueLists.Add(v.Aliases, v.Aliases);
                }
            }
            var groups = _groups.Where(c => !String.IsNullOrEmpty(c.Aliases)).ToArray();
            if (withName)
            {
                foreach (var g in groups)
                {
                    aliases.UserGroups.Add(g.Aliases, g.Name);
                }
            }
            else
            {
                foreach (var g in groups)
                {
                    aliases.UserGroups.Add(g.Aliases, g.Aliases);
                }
            }

            var objs = _objTypes.Where(c => c.IsRealObj);
            foreach (var o in objs)
            {
                var obj = o;
                var classes = _classes.Where(c => c.ObjTypeId == obj.Id).ToArray(); // && !String.IsNullOrEmpty(c.Aliases)
                if (classes.Length == 0) continue;
                var objName = obj.Aliases ??String.Empty;

                var objAlias = new MfObjectAliases();
                if (obj.Owner != null)
                {
                    var ownerId = obj.Owner.Id;
                    var oo = _objTypes.FirstOrDefault(c => c.Id == ownerId);
                    if (oo != null)
                    {
                        if (!String.IsNullOrEmpty(oo.Aliases))
                        {
                            objAlias.Owner = oo.Aliases;
                        }
                        else
                        {
                            objAlias.Owner = oo.Id;
                        }
                    }
                }
                if (!String.IsNullOrEmpty(obj.Aliases))
                {
                    objAlias.Alias = withName ? obj.Name : obj.Aliases;
                }
                else
                {
                    var id = int.Parse(obj.Id);
                    var idEnum = Enum.GetName(typeof(MFBuiltInObjectType), id);//GetName(_objTypeNameDict, id);//
                    objName = idEnum ?? obj.Id;
                    objAlias.Alias = withName ? obj.Name : obj.Id;
                }
                foreach (var c0 in classes)
                {
                    var cl = c0;
                    var classAlias = new MfClassAliases();
                    var hasAlias = false;
                    foreach (var pp in cl.Props)
                    {
                        var pId = int.Parse(pp.Id);
                        if (_ignorePropList.Contains(pId)) continue;
                        var propDef = GetPropDefById(pp.Id);
                        var a = propDef.Aliases;
                        if (String.IsNullOrEmpty(a))
                        {

                            var pIdEnum = Enum.GetName(typeof(MFBuiltInPropertyDef), pId);//GetName(_propNameDict, pId);//
                            classAlias.PropDict.Add(pIdEnum ?? pp.Id, withName ? propDef.Name : pp.Id);
                        }
                        else
                        {
                            hasAlias = true;
                            classAlias.PropDict.Add(a, withName ? propDef.Name : a);
                        }
                    }
                    var hasClassAlias = !String.IsNullOrEmpty(cl.Aliases);
                    var clAlias = cl.Aliases ?? String.Empty;
                    if (!String.IsNullOrEmpty(cl.Aliases))
                    {
                        classAlias.Alias = withName ? cl.Name : cl.Aliases;
                    }
                    else
                    {
                        classAlias.Alias = withName ? cl.Name : cl.Id;
                    }
                    if (!hasAlias && !hasClassAlias) continue;
                    if (String.IsNullOrEmpty(clAlias))
                    {
                        int cId;
                        var ok = int.TryParse(cl.Id, out cId);
                        if (ok)
                        {
                            var pIdEnum = Enum.GetName(typeof(MFBuiltInObjectClass), cId);//GetName(_objClassNameDict, cId);//
                            clAlias = pIdEnum ?? cl.Id;
                        }
                        else
                        {
                            clAlias = cl.Id;
                        }
                    }
                    objAlias.ClassDict.Add(clAlias, classAlias);
                }
                if (objAlias.ClassDict.Count > 0) aliases.Objects.Add(objName, objAlias);
            }
            foreach (var v in _views)
            {
                if (v.IsDeleted) continue;
                var vi = new MfViewAliases
                {
                    Alias = v.Aliases,
                    Id = int.Parse(v.Id),
                    Name = v.Name,
                    Common = v.IsCommon,
                    Deleted = v.IsDeleted,
                    Guid = v.Guid
                };
                aliases.Views.Add(v.Aliases,vi);
            }
            return aliases;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="withName">是否导出名称而不是别名</param>
        /// <param name="formatted">是否格式化输出文本，导出名称时建议格式化</param>
        public void ToFile(string configFile, bool withName, bool formatted)
        {
            var aliases = GetAliases(withName);
            var content = formatted ? JsonConvert.SerializeObject(aliases, Formatting.Indented) : JsonConvert.SerializeObject(aliases);

            using (var sw = new StreamWriter(configFile, false, Encoding.UTF8))
            {
                sw.Write(content);//sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
            }
        }

    }

    
}
