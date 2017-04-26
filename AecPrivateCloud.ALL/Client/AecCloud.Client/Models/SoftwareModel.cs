using System;
using System.Collections.Generic;

namespace AecCloud.Client.Models
{
    public class SoftwareModel
    {
        public static SoftwareModel CreateNewSoftwareModel()
        {
            return new SoftwareModel();
        }

        public static SoftwareModel CreateSoftwareModel(
           int id,
           string type,
           string title,
           string version,
           string summary,
           string detail,
           string img)
        {
            return new SoftwareModel
            {
                SoftId = id,
                SoftTypeName = type,
                SoftTitle = title,
                SoftVersion = version,
                SoftSummary = summary,
                SoftDetail = detail,
                SoftImgName = img
            };
        }

        protected SoftwareModel()
        {
            
        }

        /// <summary>
        /// 软件编号
        /// </summary>
        public int SoftId { get; set; }
  
        /// <summary>
        /// 软件类别
        /// </summary>
        public string SoftTypeName { get; set; }

        /// <summary>
        /// 软件名称
        /// </summary>
        public string SoftTitle { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        public string SoftVersion { get; set; }

        /// <summary>
        /// 软件简介
        /// </summary>
        public string SoftSummary { get; set; }

        /// <summary>
        /// 软件详情
        /// </summary>
        public string SoftDetail { get; set; }

        /// <summary>
        /// 软件图标名称
        /// </summary>
        public string SoftImgName { get; set; }
    }
}
