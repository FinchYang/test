using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SimulaDesign.ImportCore
{
    public class ExcelUtility
    {
        private static List<string> GetRowValues(IRow row, int count)
        {
            var list = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var cell = row.GetCell(i);
                var val = String.Empty;
                if (cell != null)
                {
                    val = GetStringValue(cell);
                }
                list.Add(val);
            }
            return list;
        }
        /// <summary>
        /// 从Excel中获取字符串的值
        /// </summary>
        /// <param name="excelFilepath"></param>
        /// <returns></returns>
        public static List<List<string>> GetFromExcel(string excelFilepath)
        {
            var list = new List<List<string>>();
            using (var file1 = new FileStream(excelFilepath, FileMode.Open, FileAccess.Read))
            {
                var hssfworkbook = new XSSFWorkbook(file1);
                ISheet sheet1 = hssfworkbook.GetSheetAt(0);
                var nameRow = sheet1.GetRow(0);
                var index = 0;
                ICell nameCell = null;
                var nameList = new List<string>();
                while ((nameCell = nameRow.GetCell(index))!=null)
                {
                    nameList.Add(GetStringValue(nameCell));
                    index++;
                }
                list.Add(nameList);
                var rowIndex = 1;
                var row = sheet1.GetRow(rowIndex);
                while (row != null)
                {
                    var rList = GetRowValues(row, index);
                    if (!rList.All(String.IsNullOrEmpty))
                    {
                        list.Add(rList);
                    }
                    else
                    {
                        break;
                    }
                    rowIndex++;
                    row = sheet1.GetRow(rowIndex);
                }

                file1.Close();
            }
            return list;
        }
        private static List<List<string>> GetFromExcel(string excelFilepath, ImportFiles obj)
        {
            var list = new List<List<string>>();
            using (var file1 = new FileStream(excelFilepath, FileMode.Open, FileAccess.Read))
            {
                var hssfworkbook = new XSSFWorkbook(file1);
                ISheet sheet1 = hssfworkbook.GetSheetAt(0);
                for (var i = 0; i < obj.Files.Files.Count+1; i++)
                {
                    var row0 = sheet1.GetRow(i);
                    if (row0 == null)
                    {
                        throw new Exception("行数不对！");
                    }
                    var vals = new List<string>();
                    for (var j = 0; j < obj.Props.Count; j++)
                    {
                        var cell0 = row0.GetCell(j);
                        if (cell0 == null)
                        {
                            vals.Add(String.Empty);
                        }
                        else
                        {
                            var cellVal = GetStringValue(cell0);
                            vals.Add(cellVal);
                        }
                    }
                    list.Add(vals);
                }
                file1.Close();
            }
            return list;
        }

        private static string GetStringValue(ICell cell0)
        {
            switch (cell0.CellType)
            {
                case CellType.Blank:
                    return String.Empty;
                case CellType.Boolean:
                    return cell0.BooleanCellValue.ToString();
                case CellType.Error:
                    return Convert.ToInt32(cell0.ErrorCellValue).ToString();
                case CellType.Formula:
                    return cell0.CellFormula;
                case CellType.Numeric:
                    return cell0.NumericCellValue.ToString();
                case CellType.String:
                    return cell0.StringCellValue;
                case CellType.Unknown:
                default:
                    return String.Empty;
            }
        }
        public static ImportFiles Read(string excelFilepath)
        {
            var dotIndex = excelFilepath.LastIndexOf('.');
            var rootPath = excelFilepath.Substring(0, dotIndex);

            var jsonFilepath = Path.ChangeExtension(excelFilepath, "json");
            if (!File.Exists(jsonFilepath))
            {
                throw new Exception("缺少Json数据文件：" + jsonFilepath);
            }
            var obj = JsonConvert.DeserializeObject<ImportFiles>(File.ReadAllText(jsonFilepath));
            obj.Files.RootDir = rootPath;

            var data = GetFromExcel(excelFilepath, obj);

            for (var i = 0; i<obj.Files.Files.Count; i++)
            {
                obj.Files.Files[i].NewFilename = data[i + 1][0];
                if (obj.AddedPropValues.Count > 0)
                {
                    obj.AddedPropValues[i].Clear();
                    for (var j = obj.Files.LayerCount; j < obj.Props.Count; j++)
                    {
                        obj.AddedPropValues[i].Add(data[i + 1][j]);
                    }
                }
            }
            return obj;
        }
        public static string Write(ImportFiles obj)
        {
            var rootPath = obj.Files.RootDir;
            var excelFilepath = rootPath.TrimEnd('\\') + ".xlsx";
            var jsonFilepath = rootPath.TrimEnd('\\') + ".json";
            var list = new List<List<string>>();
            var headers = GetHeaders(obj.Props);
            list.Add(headers);
            for (var i = 0; i < obj.Files.Files.Count; i++)
            {
                var vals = obj.Files.GetProps(obj.Files.Files[i]);
                if (obj.AddedPropValues.Count > 0)
                {
                    vals.AddRange(obj.AddedPropValues[i]);
                }
                list.Add(vals);
            }
            try
            {
                To(excelFilepath, list);
            }
            catch (Exception)
            {
                throw new Exception("Excel文件被占用，请关闭：" + excelFilepath);
            }
            var json = JsonConvert.SerializeObject(obj, Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            File.WriteAllText(jsonFilepath, json);
            return excelFilepath;
        }

        private static void To(string excelFilepath, List<List<string>> values)
        {
            using (var file1 = new FileStream(excelFilepath, FileMode.Create, FileAccess.Write))
            {
                var hssfworkbook = new XSSFWorkbook();
                ISheet sheet1 = hssfworkbook.CreateSheet("Sheet1");
                for (var i = 0; i < values.Count; i++)
                {
                    var row0 = sheet1.CreateRow(i);
                    for (var j = 0; j < values[i].Count; j++)
                    {
                        var cell0 = row0.CreateCell(j);
                        cell0.SetCellValue(values[i][j]);
                    }
                }
                //var file = new FileStream(@"test2.xlsx", FileMode.Create, FileAccess.Write, FileShare.ReadWrite); //
                hssfworkbook.Write(file1);
                file1.Close();
            }
        }

        private static List<string> GetHeaders(List<MfClassPropDef> props)
        {
            return props.Select(c => c.Name).ToList();
        }
        
    }

    public class ImportFiles
    {
        public List<MfClassPropDef> Props { get; set; }

        public MfClass ObjClass { get; set; }

        public SelectedFiles Files { get; set; }

        public List<List<string>> AddedPropValues { get; set; }
    }

    public class ImportObjects
    {
        public List<MfClassPropDef> Props { get; set; }

        public MfClass ObjClass { get; set; }

        public List<List<string>> PropValues { get; set; }
    }
}
