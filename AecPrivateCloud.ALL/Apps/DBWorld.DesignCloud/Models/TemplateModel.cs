using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBWorld.DesignCloud.Models
{
    [Serializable]
    public class TemplateModel :ICloneable
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// 模板描述
        /// </summary>
        public string TemplateDescription { get; set; }

        /// <summary>
        /// 是否有参与方
        /// </summary>
        public bool HasParty { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
