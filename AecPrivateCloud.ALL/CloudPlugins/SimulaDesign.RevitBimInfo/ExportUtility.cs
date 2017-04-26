using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SimulaDesign.MfBimInfo;

namespace SimulaDesign.RevitBimInfo
{
    public class ExportUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="path"></param>
        /// <param name="useTrans">是否使用事务</param>
        public static void ExportIFC(Document doc, string path, bool useTrans)
        {
            Transaction ts = null;
            if (useTrans)
            {
                ts = new Transaction(doc, "ifcExport"+DateTime.Now.ToString("yyyyMMddHHmmss"));
            }
            try
            {
                if (ts != null) ts.Start();
                var options = new IFCExportOptions
                {
                    FileVersion = IFCVersion.IFC2x3,
                    SpaceBoundaryLevel = 2,
                    WallAndColumnSplitting = true,
                    ExportBaseQuantities = true,
                    FamilyMappingFile = String.Empty
                };
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var mappingFile = Path.Combine(location, "exportlayers-ifc-IAI-grid.txt");
                if (File.Exists(mappingFile))
                {
                    options.FamilyMappingFile = mappingFile;
                }

                doc.Export(Path.GetDirectoryName(path), Path.GetFileName(path), options);
                if (ts != null) ts.Commit();
            }
            finally
            {
                if (ts != null) ts.Dispose();
            }
        }

        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(45);

        private static HttpClientHandler Handler
        {
            get
            {
                return new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
        }

        public static Task<string> UploadXbim(string url, string filePath)
        {

            var fullUrl = url;
            var name = Path.GetFileName(filePath);

#if R2014
            using (var client = new HttpClient(Handler){Timeout = Timeout})
            {
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    //content.Add(new StringContent("F06470B4-070B-4582-9D95-9AE5EA4F5869"), "guid");
                    //content.Add(new StringContent("1"), "typeid");
                    //content.Add(new StringContent("1"), "id");
                    content.Add(new StreamContent(File.OpenRead(filePath)), "file", name);
                    using (var message = client.PostAsync(fullUrl, content).Result)
                    {
                        var input = message.Content.ReadAsStringAsync();


                        return input;//!string.IsNullOrWhiteSpace(input) ? Regex.Match(input, @"http://\w*\.directupload\.net/images/\d*/\w*\.[a-z]{3}").Value : null;
                    }
                }
            }
#else
            return Task.Run(async () =>
            {
                using (var client = new HttpClient(Handler) { Timeout = Timeout })
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    using (var content =
                        new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                    {
                        //content.Add(new StringContent("F06470B4-070B-4582-9D95-9AE5EA4F5869"), "guid");
                        //content.Add(new StringContent("1"), "typeid");
                        //content.Add(new StringContent("1"), "id");
                        content.Add(new StreamContent(File.OpenRead(filePath)), "file", name);

                        using (var message = await client.PostAsync(fullUrl, content))
                        {

                            var input = await message.Content.ReadAsStringAsync();


                            return input;//!string.IsNullOrWhiteSpace(input) ? Regex.Match(input, @"http://\w*\.directupload\.net/images/\d*/\w*\.[a-z]{3}").Value : null;
                        }
                    }
                }
            });

            
#endif
        }

    }
}
