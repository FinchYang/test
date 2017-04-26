using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AecCloud.BaseCore;

namespace DBWorld.AecCloud.Web.Models
{
    public class FileDesc
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
    }
    public class FileMFModel
    {
        [Required]
        public string Guid { get; set; }

        public int ObjType { get; set; }
        [Range(1, Int32.MaxValue, ErrorMessage="必须大于0")]
        public int ObjId { get; set; }

        public int FileId { get; set; }

        public int ExpiredDays { get; set; }

        //public long UserId { get; set; }
    }

    public class FileShareLinkModel
    {
        public string Url { get; set; }

        public string Password { get; set; }
    }

    public class FileGetModel
    {
        public string Item { get; set; }
        public string Key { get; set; }
    }

    public class CloudAppFileModel
    {
        public string Domain { get; set; }

        public string ApplicationID { get; set; }

        public string AppName { get; set; }

        public string ProjectName { get; set; }

        public string FilePath { get; set; }
    }

    public class ModelFile
    {
        [Display(Name = "文档库GUID")]
        [Required]
        public string Guid { get; set; }
        [Range(0, int.MaxValue)]
        public int TypeId { get; set; }
        [Display(Name = "对象ID")]
        [Range(1, int.MaxValue)]
        public long ObjId { get; set; }
        public string IfcGuid { get; set; }

        public static ModelFile GetFromName(string name)
        {
            var index = name.LastIndexOf('-');
            var idStr = name.Substring(index + 1);
            var name1 = name.Substring(0, index);
            index = name1.LastIndexOf('-');
            var typeIdStr = name1.Substring(index + 1);
            var guid = name1.Substring(0, index);
            return new ModelFile { Guid = guid, TypeId = int.Parse(typeIdStr), ObjId = long.Parse(idStr) };
        }
    }

}