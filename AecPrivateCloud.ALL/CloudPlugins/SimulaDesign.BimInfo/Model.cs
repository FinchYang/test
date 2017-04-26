using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.BimInfo
{
    /// <summary>
    /// 表示一个完整的模型
    /// </summary>
    public class ProjectModel : Model
    {
        public ProjectInfo Project { get; set; }

        private readonly List<LevelElement> _levels = new List<LevelElement>();
        /// <summary>
        /// 楼层
        /// </summary>
        public List<LevelElement> Levels
        {
            get { return _levels; }
        }

        private readonly List<ViewElement> _views = new List<ViewElement>();
        /// <summary>
        /// 视图
        /// </summary>
        public List<ViewElement> Views
        {
            get { return _views; }
        }

        private readonly List<MaterialElement> _mats = new List<MaterialElement>();
        /// <summary>
        /// 材料
        /// </summary>
        public List<MaterialElement> Materials
        {
            get { return _mats; }
        }

        private readonly List<ElementCategory> _cates = new List<ElementCategory>();
        /// <summary>
        /// 类别
        /// </summary>
        public List<ElementCategory> Categories
        {
            get { return _cates; }
        }

        private readonly List<ElementType> _types = new List<ElementType>();
        /// <summary>
        /// 构件类型
        /// </summary>
        public List<ElementType> Types
        {
            get { return _types; }
        }

        private readonly List<ElementFamily> _fams = new List<ElementFamily>();
        /// <summary>
        /// 构件族
        /// </summary>
        public List<ElementFamily> Families
        {
            get { return _fams; }
        }

        private readonly List<Element> _elems = new List<Element>();

        public List<Element> Elements
        {
            get { return _elems; }
        }

        public override bool IsProject
        {
            get { return true; }
        }

        public override string GetErr()
        {
            foreach (var p in this.Elements)
            {
                var cate = Categories.FirstOrDefault(c => c.Id == p.Category.Id);
                if (cate == null)
                {
                    return "缺少类别";
                }
                var type = Types.FirstOrDefault(c => c.GetKey() == p.ElemType);
                if (type == null)
                {
                    return "缺少类型";
                }
                if (p.Material != null)
                {
                    var mat = Materials.FirstOrDefault(c => c.GetKey() == p.Material);
                    if (mat == null)
                    {
                        return "缺少材料";
                    }
                }
                if (p.Level != null)
                {
                    var l = Levels.FirstOrDefault(c => c.GetKey() == p.Level);
                    if (l == null)
                    {
                        return "缺少楼层";
                    }
                }
                if (p.Family != null)
                {
                    var f = Families.FirstOrDefault(c => c.GetKey() == p.Family);
                    if (f == null)
                    {
                        return "缺少族";
                    }
                }
            }
            return String.Empty;
        }


        public ModelLists GetLists()
        {
            var list = new ModelLists();
            foreach (var c in Categories)
            {
                var key = c.GetKey();
                if (list.Cates.Contains(key))
                {
                    throw new Exception("类别已存在此Key：" + key);
                }
                list.Cates.Add(key);
            }
            foreach (var c in Elements)
            {
                var key = c.GetKey();
                if (list.Elems.Contains(key))
                {
                    throw new Exception("构件已存在此Key：" + key);
                }
                list.Elems.Add(key);
            }
            foreach (var c in Families)
            {
                list.Fams.Add(c.GetKey());
            }
            foreach (var c in Levels)
            {
                list.Floors.Add(c.GetKey());
            }
            foreach (var c in Materials)
            {
                list.Mats.Add(c.GetKey());
            }
            foreach (var c in Types)
            {
                list.Types.Add(c.GetKey());
            }
            foreach (var c in Views)
            {
                list.Views.Add(c.GetKey());
            }
            return list;
        }
    }
    /// <summary>
    /// 族文件模型
    /// </summary>
    public class FamilyModel : Model
    {
        public ElementFamily Family { get; set; }

        private readonly List<ElementParameter> _params = new List<ElementParameter>();

        public List<ElementParameter> Parameters
        {
            get { return _params; }
        }

        private readonly List<ElementParameter> _famParams = new List<ElementParameter>();

        public List<ElementParameter> FamParameters
        {
            get { return _famParams; }
        }

        public ElementCategory Category { get; set; }

        public override bool IsProject
        {
            get { return false; }
        }
    }
    public abstract class Model
    {
        public int Id { get; set; }
        public string Name { set; get; }

        public string Filepath { get; set; }

        public abstract bool IsProject { get; }


        public virtual string GetErr()
        {
            return String.Empty;
        }

    }
    /// <summary>
    /// 模型中元素的唯一标识列表
    /// </summary>
    public class ModelLists
    {
        public ModelLists()
        {
            Cates = new List<string>();
            Fams = new List<string>();
            Floors = new List<string>();
            Mats = new List<string>();
            Types = new List<string>();
            Views = new List<string>();
            Elems = new List<string>();
        }
        public List<string> Cates { get; set; }

        public List<string> Fams { get; set; }

        public List<string> Floors { get; set; }

        public List<string> Mats { get; set; }

        public List<string> Types { get; set; }

        public List<string> Views { get; set; }

        public List<string> Elems { get; set; }
    }
}
