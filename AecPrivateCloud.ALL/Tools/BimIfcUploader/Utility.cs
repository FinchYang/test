using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MFilesAPI;

namespace BimIfcUploader
{
    public class Utility
    {
        public static List<string> GetVaultList()
        {
            return new MFilesClientApplication().GetVaultConnections().OfType<VaultConnection>().Select(c => c.Name).ToList();
        }
        public static async Task<int> UploadIfcsAsync(Vault vault, string guid, ObjectVersions ifcs, IProgress<int> progress,
                                CancellationToken ct)
        {
            int totalCount = ifcs.Count;
            int processCount = await Task.Run(async () =>
            {
                int tempCount = 1;
                foreach (ObjectVersion ifcObj in ifcs)
                {
                    //await the processing and uploading logic here
                    var processed = await UploadIfcAsync(vault, guid, ifcObj);
                    if (progress != null)
                    {
                        await Task.Delay(500, ct);
                        var count = (tempCount*100/totalCount);
                        progress.Report(count);
                        ct.ThrowIfCancellationRequested();
                    }
                    tempCount++;
                }

                return tempCount;
            }, ct);
            return processCount;
        }

        internal static int GetModelPropDef(Vault vault)
        {
            return vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropOwnedModel");
        }

        private static string Host = ConfigurationManager.AppSettings["host"];
        internal static async Task<Result> UploadIfcAsync(Vault vault, string guid, ObjectVersion ifcObj)
        {
            return await Task.Run(async() =>
            {
                var objVer = ifcObj.ObjVer;
                var pvs = vault.ObjectPropertyOperations.GetProperties(objVer);
                var modelId = GetModel(vault, pvs);
                if (modelId == null)
                {
                    return new Result {Msg = "没有找到模型！", OK = false};
                }
                var url = Host + String.Format("Model/Upload?Guid={0}&TypeId={1}&ObjId={2}", guid, 0, modelId.Value);
                var file = ifcObj.Files[1].FileVer;
                var filePath = vault.ObjectFileOperations.GetPathInDefaultView(objVer.ObjID, objVer.Version, file.ID,
                    file.Version);

                var fullUrl = url;
                var name = Path.GetFileName(filePath);

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
                            var input = String.Empty;
                            try
                            {
                                input = await message.Content.ReadAsStringAsync();
                            }
                            catch
                            {
                            }



                            return new Result { OK = message.IsSuccessStatusCode, Msg = input };//!string.IsNullOrWhiteSpace(input) ? Regex.Match(input, @"http://\w*\.directupload\.net/images/\d*/\w*\.[a-z]{3}").Value : null;

                        }
                    }
                }
            });
        }
        public static List<string> UploadIfcs(string vaultName)
        {
            var vault = GetVault(vaultName);
            if (vault == null)
            {
                MessageBox.Show("无法连接到文档库：" + vaultName);
                return new List<string>();
            }
            var files = SearchFiles(vault, "ifc");
            if (files.Count == 0)
            {
                MessageBox.Show("没有IFC文件！");
                return new List<string>();
            }
            var objVers = new ObjVers();
            foreach (ObjectVersion ov in files)
            {
                objVers.Add(01, ov.ObjVer);
            }
            var props = vault.ObjectPropertyOperations.GetPropertiesOfMultipleObjects(objVers);
            var list = new List<string>();
            var errList = new List<string>();
            var guid = vault.GetGUID().TrimStart(new[] {'{'}).TrimEnd(new[] {'}'});
            for (var i = 1; i <= objVers.Count; i++)
            {
                var objVer = objVers[i];
                var pvs = props[i];
                var modelId = GetModel(vault, pvs);
                if (modelId == null)
                {
                    errList.Add(files[i].Title);
                }
                else
                {
                    var url = Host + String.Format("Model/Upload?Guid={0}&TypeId={1}&ObjId={2}", guid, 0, modelId.Value);
                    var file = files[i].Files[1].FileVer;
                    var filePath = vault.ObjectFileOperations.GetPathInDefaultView(objVer.ObjID, objVer.Version, file.ID,
                        file.Version);
                    var res = UploadXbim(url, filePath).Result;
                    if (res.OK)
                    {
                        list.Add(files[i].Title);
                    }
                    else
                    {
                        errList.Add(files[i].Title);
                    }
                }
            }
            if (errList.Count > 0)
            {
                MessageBox.Show("未能找到模型或出错的IFC文件列表：\r\n" + String.Join("\r\n", errList));
            }
            return list;
        }

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

        public class Result
        {
            internal bool OK { get; set; }

            internal string Msg { get; set; }
        }

        private static readonly TimeSpan Timeout = TimeSpan.FromHours(2);

        public static Task<Result> UploadXbim(string url, string filePath)
        {

            var fullUrl = url;
            var name = Path.GetFileName(filePath);

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
                            var ok = message.IsSuccessStatusCode;
                            string input = String.Empty;
                            if (!ok)
                            {
                                input = await message.Content.ReadAsStringAsync();
                            }
                            return new Result { OK = ok, Msg = input };//!string.IsNullOrWhiteSpace(input) ? Regex.Match(input, @"http://\w*\.directupload\.net/images/\d*/\w*\.[a-z]{3}").Value : null;

                        }
                    }
                }
            });


        }
        internal static Vault GetVault(string vaultName)
        {
            return new MFilesClientApplication().GetVaultConnection(vaultName).BindToVault(IntPtr.Zero, true, true);
        }

        public static void UpdatePartPaths(Vault vault, string wrongUrl = "http://139.196.154.231:8000/")
        {
            var partAlias = "ObjPart";
            var objType = vault.ObjectTypeOperations.GetObjectTypeIDByAlias(partAlias);

            var addressPD = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropModelUrl");
            var url = ConfigurationManager.AppSettings["host"];

            var objs = SearchObjects(vault, objType, addressPD, wrongUrl);
            var objCount = objs.Count;
            var max = 500;
            var count = objCount / max;
            var residue = objCount % max;
            if (residue != 0)
            {
                count++;
            }
            for (var i = 0; i < count-1; i++)
            {
                var j = i * max;
                var end = (i + 1) * max;
                var objVers = new ObjVers();
                for (; j < end; j++)
                {
                    if (j == objCount) break;
                    objVers.Add(-1, objs[j + 1].ObjVer);
                }
                var newObjVers = new ObjVers();
                var objsAndProps = vault.ObjectPropertyOperations.GetPropertiesOfMultipleObjects(objVers);
                for (var k = 1; k <= objVers.Count; k++)
                {
                    var pv = objsAndProps[k].SearchForPropertyEx(addressPD, true);
                    if (pv == null) continue;
                    var adStr = pv.GetValueAsLocalizedText();
                    if (adStr.Contains(wrongUrl))
                    {
                        var str = adStr.Replace(wrongUrl, url);
                        var newPV = pv.Clone();
                        newPV.Value.SetValue(pv.Value.DataType, str);
                        var objVer = vault.ObjectOperations.CheckOut(objVers[k].ObjID);
                        var obj = vault.ObjectPropertyOperations.SetProperty(objVer.ObjVer, newPV);
                        newObjVers.Add(-1, obj.ObjVer);
                    }
                }
                if (newObjVers.Count > 0)
                {
                    vault.ObjectOperations.CheckInMultipleObjects(newObjVers);
                }
            }
        }

        internal static ObjectVersions SearchObjects(Vault vault, int objType, int urlDef, string wrongUrl)
        {
            var scs = new SearchConditions();

            var typeSc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            typeSc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
            typeSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, objType);
            scs.Add(-1, typeSc);

            var urlSc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeContains };
            urlSc.Expression.DataPropertyValuePropertyDef = urlDef;
            urlSc.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, wrongUrl);
            scs.Add(-1, urlSc);

            var delSc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            delSc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delSc.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, delSc);

            return
                vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0)
                    .GetAsObjectVersions();
        }

        internal static ObjectVersions SearchFiles(Vault vault, string extension)
        {
            var scs = new SearchConditions();

            var typeSc = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            typeSc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
            typeSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup,
                (int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument);
            scs.Add(-1, typeSc);

            var modelSc = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeNotEqual};
            modelSc.Expression.DataPropertyValuePropertyDef = GetModelPropDef(vault);
            modelSc.TypedValue.SetValueToNULL(MFDataType.MFDatatypeLookup);
            scs.Add(-1, modelSc);

            var fileNameSc = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeMatchesWildcardPattern};
            fileNameSc.Expression.DataFileValueType = MFFileValueType.MFFileValueTypeFileName;
            fileNameSc.TypedValue.SetValue(MFDataType.MFDatatypeText, "*." + extension);
            scs.Add(-1, fileNameSc);

            var delSc = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            delSc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delSc.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, delSc);

            return
                vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone, false)
                    .GetAsObjectVersions();
        }

        private static int? GetModel(Vault vault, PropertyValues pvs)
        {
            var modelPV = pvs.SearchForPropertyByAlias(vault, "PropOwnedModel", true);
            if (modelPV == null) return null;
            if (modelPV.Value.IsNULL()) return null;
            return modelPV.Value.GetLookupID();
        }
    }
}
