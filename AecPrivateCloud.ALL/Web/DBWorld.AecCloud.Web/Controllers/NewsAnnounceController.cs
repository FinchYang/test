using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace DBWorld.AecCloud.Web.Controllers
{
    public class NewsAnnounceController : Controller
    {
        // GET: News
        public ViewResult NewsIndex()
        {
            return View();
        }

        // GET: News
        public ViewResult AnnounceIndex()
        {
            return View();
        }

        public ViewResult Operation()
        {
            return View();
        }

        //添加新闻或者公告
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Operation(string content, string title, string date, string type, string operType, string id)
        {
            string returnStr = "";
            try
            {
                switch (operType)
                {
                    case "modify":
                        returnStr = Modify(content, title, date, type, id);
                        break;
                    case "add":
                        returnStr = Add(content, title, date, type);
                        break;
                    default:
                        break;
                }
                return Content(returnStr);
            }
            catch (Exception)
            {
                return Content("fail");
                throw;
            }
        }

        private string Modify(string content, string title, string date, string type, string id)
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                switch (type)
                {
                    case "news":
                        xmldoc.Load(Server.MapPath("~/Installer/News.xml"));
                        break;
                    case "announce":
                        xmldoc.Load(Server.MapPath("~/Installer/Announce.xml"));
                        break;
                    default:
                        break;
                }
                XmlNodeList nodeList = xmldoc.SelectSingleNode("List").ChildNodes;
                foreach (XmlNode xnode in nodeList)//遍历所节点 
                {
                    if (xnode.Attributes["Id"].InnerText == id)
                    {
                        foreach (XmlNode xnode2 in xnode.ChildNodes)
                        {
                            XmlElement my_parm = (XmlElement)xnode2;//节点类型转换XmlElement类型
                            string name = my_parm.Name;
                            switch (name)
                            {
                                case "title":
                                    my_parm.InnerText = title;
                                    break;
                                case "content":
                                    my_parm.InnerText = content;
                                    break;
                                case "Date":
                                    my_parm.InnerText = date;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                switch (type)
                {
                    case "news":
                        xmldoc.Save(Server.MapPath("~/Installer/News.xml"));
                        break;
                    case "announce":
                        xmldoc.Save(Server.MapPath("~/Installer/Announce.xml"));
                        break;
                    default:
                        break;
                }
                return "success";

            }
            catch (Exception)
            {
                return "fail";
                throw;
            }
        }

        private string Add(string content, string title, string date, string type)
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                switch (type)
                {
                    case "news":
                        xmldoc.Load(Server.MapPath("~/Installer/News.xml"));
                        break;
                    case "announce":
                        xmldoc.Load(Server.MapPath("~/Installer/Announce.xml"));
                        break;
                    default:
                        break;
                }
                XmlNode root = xmldoc.SelectSingleNode("List");
                XmlElement xe1 = xmldoc.CreateElement("Element");
                if (root.ChildNodes.Count == 0)
                {
                    xe1.SetAttribute("Id", "1");
                }
                else
                {
                    xe1.SetAttribute("Id", (int.Parse(root.LastChild.Attributes[0].Value) + 1).ToString());  //获取最后一个新闻的 ID，然后加一作为新的新闻的ID
                }
                xe1.SetAttribute("state", "1");//为节点添加属性state标注为删除状态，默认为1表示未删除。0表示已删除
                XmlElement xesub1 = xmldoc.CreateElement("title");
                xesub1.InnerText = title.Trim();//设置文本节点
                xe1.AppendChild(xesub1);//添加到<Node>节点中
                XmlElement xesub2 = xmldoc.CreateElement("content");
                xesub2.InnerText = content.Trim();
                xe1.AppendChild(xesub2);
                XmlElement xesubDate = xmldoc.CreateElement("Date");
                xesubDate.InnerText = date.Trim();//设置文本节点
                xe1.AppendChild(xesubDate);//添加到<Node>节点中

                root.AppendChild(xe1);//添加到<Employees>节点中
                switch (type)
                {
                    case "news":
                        xmldoc.Save(Server.MapPath("~/Installer/News.xml"));
                        break;
                    case "announce":
                        xmldoc.Save(Server.MapPath("~/Installer/Announce.xml"));
                        break;
                    default:
                        break;
                }
                return "success";
            }
            catch (Exception)
            {
                return "fail";
                throw;
            }

        }

        //新闻列表
        public ActionResult Titles(int pageSize, int pageIndex, string type)
        {
            XmlDocument XmlDoc = new XmlDocument();
            switch (type)
            {
                case "news":
                    XmlDoc.Load(Server.MapPath("~/Installer/News.xml"));
                    break;
                case "announce":
                    XmlDoc.Load(Server.MapPath("~/Installer/Announce.xml"));
                    break;
                default:
                    break;
            }
            XmlNodeList nodeList = XmlDoc.SelectSingleNode("List").ChildNodes;
            int count = nodeList.Count;
            int pages = (int)(Math.Ceiling((double)nodeList.Count / pageSize)); //算出总页数
            string tempStr = "{'count':'" + nodeList.Count + "','titles':[";
            if (pageIndex == -1 && pageSize == -1)  //如果pagendex和pageSize都为-1 则表示查询 最新5条新闻，用于首页显示
            {
                for (int i = nodeList.Count; i >= nodeList.Count - 7; i--)
                //for (int i =nodeList.Count - 1 ; i >= nodeList.Count - 7; i--)  //新闻新发布的在上边，所以从后向前读取
                {
                    if (nodeList[i] == null)
                    {
                        continue;
                    }
                    else
                    {
                        string id = nodeList[i].Attributes["Id"].Value;
                        string state = nodeList[i].Attributes["state"].Value;
                        tempStr += "{'Id':'" + id + "'," + "'state':'" + state + "',";
                        foreach (XmlNode xnode in nodeList[i])//遍历所节点 
                        {
                            XmlElement my_parm = (XmlElement)xnode;//节点类型转换XmlElement类型
                            string name = my_parm.Name;
                            switch (name)
                            {
                                case "title":
                                    tempStr += "'title':'" + my_parm.InnerText + "',";
                                    break;
                                case "Date":
                                    tempStr += "'Date':'" + my_parm.InnerText + "'},";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = count - pageSize * (pageIndex - 1) - 1; i > count - pageSize * pageIndex - 1; i--)//采用倒叙排序方式查询
                {
                    if (nodeList[i] == null)
                    {
                        continue;
                    }
                    else
                    {
                        string id = nodeList[i].Attributes[0].Value;
                        string state = nodeList[i].Attributes["state"].Value;
                        tempStr += "{'Id':'" + id + "'," + "'state':'" + state + "',";
                        foreach (XmlNode xnode in nodeList[i])//遍历所节点 
                        {
                            XmlElement my_parm = (XmlElement)xnode;//节点类型转换XmlElement类型
                            string name = my_parm.Name;
                            switch (name)
                            {
                                case "title":
                                    tempStr += "'title':'" + my_parm.InnerText + "',";
                                    break;
                                case "Date":
                                    tempStr += "'Date':'" + my_parm.InnerText + "'},";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            tempStr = tempStr.Substring(0, tempStr.Length - 1);
            tempStr += "]}";
            return Content(tempStr);
        }

        //新闻详情页面
        public ViewResult NewsDetails()
        {
            return View();
        }

        //公告详情页面
        public ViewResult AnnounceDetails()
        {
            return View();
        }

        [HttpPost]
        //获取新闻详细内容
        public ActionResult GetDetails(string id, string type)
        {
            XmlDocument XmlDoc = new XmlDocument();
            switch (type)
            {
                case "news":
                    XmlDoc.Load(Server.MapPath("~/Installer/News.xml"));
                    break;
                case "announce":
                    XmlDoc.Load(Server.MapPath("~/Installer/Announce.xml"));
                    break;
                default:
                    break;
            }
            string jsonStr = "";
            ////获取<data>节点所节点
            XmlNodeList nodeList = XmlDoc.SelectSingleNode("List").ChildNodes;
            foreach (XmlNode xnode in nodeList)//遍历所节点 
            {
                if (xnode.Attributes[0].InnerText == id)
                {
                    jsonStr = "{";
                    foreach (XmlNode xnode2 in xnode.ChildNodes)
                    {
                        XmlElement my_parm = (XmlElement)xnode2;//节点类型转换XmlElement类型
                        string name = my_parm.Name;
                        switch (name)
                        {
                            case "title":
                                jsonStr += "'title':'" + my_parm.InnerText + "',";
                                break;
                            case "content":
                                string temp = my_parm.InnerText.Replace("\n", "");//替换掉换行符
                                jsonStr += "'content':'" + temp + "',";
                                break;
                            case "Date":
                                jsonStr += "'date':'" + my_parm.InnerText + "'";
                                break;
                            default:
                                break;
                        }
                    }
                    jsonStr += "}";
                }
            }
            return Content(jsonStr);
        }

        //获取新闻或者公告的列表
        public ViewResult Manage(string type)
        {
            return View();
        }

        //删除或恢复某个新闻或公告
        public ActionResult Delete(string type, string id, string operType)
        {
            try
            {
                XmlDocument XmlDoc = new XmlDocument();
                switch (type)
                {
                    case "news":
                        XmlDoc.Load(Server.MapPath("~/Installer/News.xml"));
                        break;
                    case "announce":
                        XmlDoc.Load(Server.MapPath("~/Installer/Announce.xml"));
                        break;
                    default:
                        break;
                }
                XmlNodeList nodeList = XmlDoc.SelectSingleNode("List").ChildNodes;
                foreach (XmlNode xnode in nodeList)//遍历所节点 
                {
                    if (xnode.Attributes["Id"].InnerText == id)
                    {
                        if (operType == "delete")
                        {
                            xnode.Attributes["state"].InnerText = "0"; //删除或恢复某个新闻或公告，只是改掉其状态就可以。不用真的删除
                        }
                        else
                        {
                            xnode.Attributes["state"].InnerText = "1";
                        }
                    }
                }
                switch (type)
                {
                    case "news":
                        XmlDoc.Save(Server.MapPath("~/Installer/News.xml"));
                        break;
                    case "announce":
                        XmlDoc.Save(Server.MapPath("~/Installer/Announce.xml"));
                        break;
                    default:
                        break;
                }
                return Content("success");
            }
            catch (Exception)
            {
                return Content("fail");
                throw;
            }
        }
    }
}