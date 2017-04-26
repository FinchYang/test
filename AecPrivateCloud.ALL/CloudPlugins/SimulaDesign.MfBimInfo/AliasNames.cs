using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.MfBimInfo
{
    public static class OB
    {
        /// <summary>
        /// 材料
        /// </summary>
        public const string Material = "ObjMaterial"; //材料

        //public const string Parameter = "ObjParameter"; //参数
        /// <summary>
        /// 构件模型
        /// </summary>
        public const string Part = "ObjPart"; //构件模型
        /// <summary>
        /// 构件类别
        /// </summary>
        public const string Category = "ObjCategory"; //构件类别
        /// <summary>
        /// 构件类型
        /// </summary>
        public const string PartType = "ObjPartType"; //构件类型
        /// <summary>
        /// 构件族
        /// </summary>
        public const string Family = "ObjPartFamily";
        /// <summary>
        /// 楼层
        /// </summary>
        public const string Level = "ObjFloorLevel"; //楼层
        /// <summary>
        /// 视图
        /// </summary>
        public const string View = "ObjView"; //视图





        /// <summary>
        /// 单体
        /// </summary>
        public static readonly string ModelUnit ="ObjModelUnit";
        /// <summary>
        /// 楼层，父类型为：单体(ObjModelUnit)
        /// </summary>
        public static readonly string Floor = "ObjFloor";
        /// <summary>
        /// 模型专业，父类型为：楼层(ObjFloor)
        /// </summary>
        public static readonly string ModelDisc = "ObjModelDiscipline";
    }

    public static class CS
    {
        ///// <summary>
        ///// BIM模型
        ///// </summary>
        //public const string ModelFile = "ClassBimModel"; //BIM模型
        /// <summary>
        /// 材料
        /// </summary>
        public const string Material = "ClassMaterial"; //材料
        //public const string Parameter = "ClassParameter"; //参数
        /// <summary>
        /// 构件模型
        /// </summary>
        public const string Part = "ClassPart"; //构件模型
        /// <summary>
        /// 族构件模型
        /// </summary>
        public const string FamilyPart = "ClassFamPart"; //族构件模型
        /// <summary>
        /// 构件类别
        /// </summary>
        public const string Category = "ClassCategory"; //构件类别
        /// <summary>
        /// 构件类型
        /// </summary>
        public const string PartType = "ClassPartType"; //构件类型
        /// <summary>
        /// 构件族
        /// </summary>
        public const string Family = "ClassPartFamily";
        /// <summary>
        /// 楼层
        /// </summary>
        public const string Level = "ClassFloorLevel"; //楼层
        /// <summary>
        /// 视图
        /// </summary>
        public const string View = "ClassView"; //视图
        /// <summary>
        /// Ifc模型
        /// </summary>
        public const string IfcModel = "ClassPreviewModel"; //Ifc模型

        /// <summary>
        /// 单体
        /// </summary>
        public static string ModelUnit { get { return "ClassModelUnit"; } }
        /// <summary>
        /// 楼层
        /// </summary>
        public static string Floor { get { return "ClassFloor"; } }
        /// <summary>
        /// 模型专业
        /// </summary>
        public static string ModelDisc { get { return "ClassModelDiscipline"; } }
        /// <summary>
        /// BIM模型
        /// </summary>
        public static string DocBimModel { get { return "ClassBimModelDoc"; } }

    }

    public static class PD
    {
        public const string Name = "0";

        public const string Id = "PropID";

        public const string Guid = "PropGUID";
        /// <summary>
        /// 材料
        /// </summary>
        public const string Material = "PropMaterial";
        /// <summary>
        /// 构件类别
        /// </summary>
        public const string PartCategory = "PropPartCategory"; //构件类别
        /// <summary>
        /// 构件类型
        /// </summary>
        public const string PartType = "PropPartType";
        /// <summary>
        /// 楼层
        /// </summary>
        public const string Level = "PropFloorLevel";
        /// <summary>
        /// 视图类型
        /// </summary>
        public const string ViewType = "PropViewType";
        /// <summary>
        /// 视图专业
        /// </summary>
        public const string ViewDisc = "PropViewDisc";
        /// <summary>
        /// 所属族
        /// </summary>
        public const string OwnedFamily = "PropFamily";
        /// <summary>
        /// 标高
        /// </summary>
        public const string Elevation = "PropElevation";
        /// <summary>
        /// 参数列表
        /// </summary>
        public const string ParamList = "PropParamList";
        /// <summary>
        /// 所属模型
        /// </summary>
        public const string OwnedModel = "PropOwnedModel";
        /// <summary>
        /// 族参数列表
        /// </summary>
        public const string FamParamList = "PropFamParams";
        /// <summary>
        /// Ifc GUID
        /// </summary>
        public const string IfcId = "PropIfcId";
        /// <summary>
        /// 模型的浏览地址
        /// </summary>

        public const string ModelUrl = "PropModelUrl";

        /// <summary>
        /// 模型名称
        /// </summary>
        public static string ModelName { get { return "PropModelName"; } }
        /// <summary>
        /// 所在单体
        /// </summary>
        public static string ModelUnitAt { get { return "PropModelUnitAt"; } }
        /// <summary>
        /// 所在楼层
        /// </summary>
        public static string FloorAt { get { return "PropFloorAt"; } }
        /// <summary>
        /// 所在专业
        /// </summary>
        public static string DiscAt { get { return "PropDisciplineAt"; } }
    }
}
