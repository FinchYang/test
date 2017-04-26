//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Drawing;
//using System.IO;
//using WORD = Microsoft.Office.Interop.Word;
//using System.Windows.Forms;
//namespace WindowsFormsApplication1
//{
//    public class WordHelper
//    {
//        private WORD.ApplicationClass oWordApplic;//a reference to word application
//        private WORD.Document oDoc;//a reference to the document
//        object missing = System.Reflection.Missing.Value;
//        public WORD.ApplicationClass WordApplication
//        {
//            get { return oWordApplic; }
//        }
//        ///
//        /// 无参数构造函数
//        ///
//        public WordHelper()
//        {
//            //activate the interface with the COM object of Microsoft Word
//            oWordApplic = new WORD.ApplicationClass();
//        }
//        ///
//        /// 带参数构造函数
//        ///
//        ///
//        public WordHelper(WORD.ApplicationClass wordapp)
//        {
//            oWordApplic = wordapp;
//        }
//        #region 文件操作
//        //Open a file (the file must exitsts) and activate it
//        ///
//        /// 打开一个已存在的文件
//        ///
//        /// 文件名
//        public void open(string strFileName)
//        {
//            object fileName = strFileName;
//            object readOnly = false;
//            object isVisible = true;
//            oDoc = oWordApplic.Documents.Open(ref fileName,ref missing,ref readOnly,ref missing,ref missing,
//                ref missing,ref missing,ref missing,ref missing,ref missing,,ref missing,ref missing,ref missing,ref missing,ref missing);
//            oDoc.Activate();
//        }
//        ///
//        /// 新建一个空白文档 open a new document
//        ///
//        public void open()
//        {
//            oDoc = oWordApplic.Documents.Add(ref missing, ref missing, ref missing, ref missing);
//            oDoc.Activate();
//        }
//        ///
//        /// 退出office
//        ///
//        public void Quit()
//        {
//            oWordApplic.Application.Quit(ref missing, ref missing, ref missing);
//            oWordApplic = null;
//        }
//        ///
//        /// 附加dot模板文件
//        ///
//        ///
//        public void LoadDotFile(string strDotFile)
//        {
//            if (!string.IsNullOrEmpty(strDotFile))
//            {
//                WORD.Document wDot = null;
//                if (oWordApplic != null)
//                {
//                    oDoc = oWordApplic.ActiveDocument;
//                    oWordApplic.Selection.WholeStory();
//                    //string strContent = oWordApplic.Selection.Text;
//                    oWordApplic.Selection.Copy();
//                    wDot = CreateWordDocument(strDotFile, true);
//                    object bkmC = "Content";
//                    if (oWordApplic.ActiveDocument.Bookmarks.Exists("Content") == true)
//                    {
//                        oWordApplic.ActiveDocument.Bookmarks.get_Item(ref bkmC).Select();
//                    }
//                    //对标签Content进行填充
//                    //直接写入内容不能识别表格什么的
//                    //oWordApplic.Selection.TypeText(strContent);
//                    oWordApplic.Selection.Paste();
//                    oWordApplic.Selection.WholeStory();
//                    oWordApplic.Selection.Copy();
//                    wDot.Close(ref missing, ref missing, ref missing);
//                    oDoc.Activate();
//                    oWordApplic.Selection.Paste();
//                }
//            }
//        }
//        ///
//        /// 打开word文档并且返回oDoc对象
//        ///
//        /// 完整的Word文件路径+名称
//        ///
//        /// 返回的oDoc对象
//        private WORD.Document CreateWordDocument(string strDotFile, bool p)
//        {
//            if (strDotFile == "") return null;
//            oWordApplic.Visible = p;
//            oWordApplic.Caption = "";
//            oWordApplic.Options.CheckSpellingAsYouType = false;
//            oWordApplic.Options.CheckGrammarAsYouType = false;
//            object fileName = strDotFile;
//            object confirmConversion = false;
//            object readOnly = true;
//            object addToRecentFiles = false;
//            object passwordDocument = System.Type.Missing;
//            object passwordTemplates = System.Type.Missing;
//            object revert = System.Type.Missing;
//            object writePasswordDocument = System.Type.Missing;
//            object writePasswordTemplates = System.Type.Missing;
//            object format = System.Type.Missing;
//            object encoding = System.Type.Missing;
//            object visible = System.Type.Missing;
//            object openAndRepair = System.Type.Missing;
//            object documentDirection = System.Type.Missing;
//            object noEncodingDialog = System.Type.Missing;
//            object xmlTransform = System.Type.Missing;
//            try
//            {
//                WORD.Document wordDoc = oWordApplic.Documents.Open(ref fileName, ref confirmConversion, ref readOnly, ref addToRecentFiles, ref passwordDocument, ref passwordTemplates, ref revert, ref writePasswordDocument, ref writePasswordTemplates, ref format, ref encoding, ref visible, ref openAndRepair, ref documentDirection, ref noEncodingDialog, ref xmlTransform);
//                return wordDoc;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//                return null;
//            }
//        }
//        ///
//        /// 保存文档文件
//        ///
//        /// 要保存的文档
//        /// 要保存的文件路径+文件名
//        public void SaveAs(WORD.Document oDoc, string strFileName)
//        {
//            object fileName = strFileName;
//            if (File.Exists(strFileName))
//            {
//                if (MessageBox.Show("文件'" + strFileName + "'+已经存在，选确定覆盖源文件，选取消退出操作！", "警告", MessageBoxButtons.OKCancel) == DialogResult.OK)
//                {
//                    oDoc.SaveAs(ref fileName, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
//                }
//                else
//                {
//                    Clipboard.Clear();
//                }
//            }
//            else
//            {
//                oDoc.SaveAs(ref fileName, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
//            }
//        }
//        ///
//        /// 保存成网页文件
//        ///
//        /// 文档对象
//        /// 文件名
//        public void SaveAsHtml(WORD.Document oDoc, string strFileName)
//        {
//            object fileName = strFileName;
//            //wdFormatWebArchive 保存为单个网页文件
//            //wdFormatFilteredHTML 保存为过滤掉word标签的 htm文件，缺点是有图片的话会产生网页文件夹
//            if (File.Exists(strFileName))
//            {
//                if (MessageBox.Show("文件'" + strFileName + "'+已经存在，选确定覆盖源文件，选取消退出操作！", "警告", MessageBoxButtons.OKCancel) == DialogResult.OK)
//                {
//                    object Format = (int)WORD.WdSaveFormat.wdFormatWebArchive;
//                    oDoc.SaveAs(ref fileName, ref Format, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
//                }
//                else
//                {
//                    Clipboard.Clear();
//                }
//            }
//            else
//            {
//                object Format = (int)WORD.WdSaveFormat.wdFormatWebArchive;
//                oDoc.SaveAs(ref fileName, ref Format, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
//            }
//        }

//        ///
//        /// 保存文档
//        ///
//        public void Save()
//        {
//            oDoc.Save();
//        }
//        ///
//        /// 保存文档
//        ///
//        /// 保存文件名
//        public void SaveAs(string strFileName)
//        {
//            object fileName = strFileName;
//            oDoc.SaveAs(ref fileName, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
//        }
//        ///
//        /// Save the document in HTML format
//        ///
//        /// filename
//        public void SaveAs1(string strFileName)
//        {
//            object fileName = strFileName;
//            object format = (int)WORD.WdSaveFormat.wdFormatHTML;
//            oDoc.SaveAs(ref fileName, ref format, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
//        }
//        #endregion
//        #region 添加菜单(工具栏）项
//        ///
//        /// 添加单独的菜单项
//        ///
//        ///
//        public void AddMenu(Microsoft.Office.Core.CommandBarPopup popuBar)
//        {
//            Microsoft.Office.Core.CommandBar menuBar = null;
//            menuBar = this.oWordApplic.CommandBars["Menu Bar"];
//            popuBar = (Microsoft.Office.Core.CommandBarPopup)this.oWordApplic.CommandBars.FindControl(Microsoft.Office.Core.MsoControlType.msoControlPopup, missing, popuBar.Tag, true);
//            if (popuBar == null)
//            {
//                popuBar = (Microsoft.Office.Core.CommandBarPopup)menuBar.Controls.Add(Microsoft.Office.Core.MsoControlType.msoControlPopup, missing, missing, missing, missing);
//            }
//        }
//        ///
//        /// 添加单独的工具栏
//        ///
//        /// 工具栏名称
//        /// 菜单按钮名称
//        public void AddToolItem(string strBarName, string strBtnName)
//        {
//            Microsoft.Office.Core.CommandBar toolBar = null;
//            toolBar = (Microsoft.Office.Core.CommandBar)this.oWordApplic.CommandBars.FindControl(Microsoft.Office.Core.MsoControlType.msoControlButton, missing, strBarName, true);
//            if (toolBar == null)
//            {
//                toolBar = (Microsoft.Office.Core.CommandBar)this.oWordApplic.CommandBars.Add(Microsoft.Office.Core.MsoControlType.msoControlButton, missing, missing, missing);
//                toolBar.Name = strBarName;
//                toolBar.Visible = true;
//            }
//        }

//        #endregion
//        #region 移动光标位置
//        ///
//        /// 定位到书签位置
//        /// go to the predefind bookmark,if the bookmark doesn't exists the application will raise an error
//        ///
//        ///
//        public void GotoBookMark(string strBookMarkName)
//        {
//            object bookMark = (int)WORD.WdGoToItem.wdGoToBookmark;
//            object nameBookMark = strBookMarkName;
//            oWordApplic.Selection.GoTo(ref bookMark, ref missing, ref missing, ref nameBookMark);
//        }
//        ///
//        /// 移动至文末
//        ///
//        public void GotoTheEnd()
//        {
//            object unit;
//            unit = WORD.WdUnits.wdStory;
//            oWordApplic.Selection.EndKey(ref unit, ref missing);
//        }
//        ///
//        /// 移动至行末
//        ///
//        public void GotoLineEnd()
//        {
//            object unit = WORD.WdUnits.wdLine;
//            object ext = WORD.WdMovementType.wdExtend;
//            oWordApplic.Selection.EndKey(ref unit, ref ext);
//        }
//        ///
//        /// 移动至文首
//        ///
//        public void GotoTheBeginning()
//        {
//            object unit = WORD.WdUnits.wdStory;
//            oWordApplic.Selection.HomeKey(ref unit, ref missing);
//        }
//        ///
//        /// 跳转到表格
//        ///
//        ///
//        public void GotoTheTable(int ntable)
//        {
//            //Selection.GoTo What:=wdGoToTable,Which:=wdGoToFirst,Count:=1,namespace:=""
//            //Selection.Find.ClearFormatting
//            //With Selection.Find
//            //  .Text =""
//            //  .Replacement.Text=""
//            //  .Forward = True
//            //  .Wrap = wdFindContinue
//            //  .Format = False
//            //  .MatchCase =False
//            //  .MatchWholeWord =False
//            //  .MatchWildcards = False
//            //  .MatchSoundsLike = False
//            //  .MatchAllWordForms = False
//            //End With
//            object what;
//            what = WORD.WdUnits.wdTable;
//            object which;
//            which = WORD.WdGoToDirection.wdGoToFirst;
//            object count;
//            count = 1;
//            oWordApplic.Selection.GoTo(ref what, ref which, ref count, ref missing);
//            oWordApplic.Selection.Find.ClearFormatting();
//            oWordApplic.Selection.Text = "";

//        }
//        ///
//        /// 移动至右边单元格
//        ///
//        public void GotoRightCell()
//        {
//            //Selection.MoveRight Unit:= wdCell
//            object direction;
//            direction = WORD.WdUnits.wdCell;
//            oWordApplic.Selection.MoveRight(ref direction, ref missing, ref missing);
//        }
//        ///
//        /// 移动至左边单元格
//        ///
//        public void GotoLeftCell()
//        {
//            //Selection.MoveLeft Unit:= wdCell
//            object direction;
//            direction = WORD.WdUnits.wdCell;
//            oWordApplic.Selection.MoveLeft(ref direction, ref missing, ref missing);
//        }
//        ///
//        /// 移动至下边单元格
//        ///
//        public void GotoDownCell()
//        {
//            //Selection.MoveDown Unit:= wdCell
//            object direction;
//            direction = WORD.WdUnits.wdCell;
//            oWordApplic.Selection.MoveDown(ref direction, ref missing, ref missing);
//        }
//        ///
//        /// 移动至上边单元格
//        ///
//        public void GotoUpCell()
//        {
//            //Selection.MoveUp Unit:= wdCell
//            object direction;
//            direction = WORD.WdUnits.wdCell;
//            oWordApplic.Selection.MoveUp(ref direction, ref missing, ref missing);
//        }
//        #endregion
//        #region 插入操作
//        ///
//        /// 插入文字
//        ///
//        ///
//        public void InsertText(string strText)
//        {
//            oWordApplic.Selection.TypeText(strText);
//        }
//        ///
//        /// 插入空行
//        ///
//        public void InsertLineBreak()
//        {
//            oWordApplic.Selection.TypeParagraph();
//        }
//        ///
//        /// 插入多个空行
//        ///
//        /// 空行数
//        public void InsertLineBreak(int nLine)
//        {
//            for (int i = 0; i < nLine; i++)
//                oWordApplic.Selection.TypeParagraph();
//        }
//        ///
//        /// 插入空白页
//        ///
//        public void InsertPageBreak()
//        {
//            object pBreak = (int)WORD.WdBreakType.wdPageBreak;
//            oWordApplic.Selection.InsertBreak(ref pBreak);
//        }
//        ///
//        /// 插入页码
//        ///
//        public void InsertPageNumber()
//        {
//            object wdFieldPage = WORD.WdFieldType.wdFieldPage;
//            object preserveFormatting = true;
//            oWordApplic.Selection.Fields.Add(oWordApplic.Selection.Range, ref wdFieldPage, ref missing, ref preserveFormatting);
//        }
//        ///
//        /// 插入页码
//        ///
//        /// 页码样式
//        public void InsertPageNumber(string strAlign)
//        {
//            object wdFieldPage = WORD.WdFieldType.wdFieldPage;
//            object preserverFormatting = true;
//            oWordApplic.Selection.Fields.Add(oWordApplic.Selection.Range, ref wdFieldPage, ref missing, ref preserverFormatting);
//            SetAlignment(strAlign);
//        }
//        ///
//        /// 插入四面环绕型图片
//        ///
//        /// 文件路径+名称
//        /// 宽
//        /// 高
//        public void InsertImage(string strPicPath, float picWidth, float picHeight)
//        {
//            string fileName = strPicPath;
//            object linkToFile = false;
//            object saveWithDocument = true;
//            object anchor = oWordApplic.Selection.Range;
//            oWordApplic.ActiveDocument.InlineShapes.AddPicture(fileName, ref linkToFile, ref saveWithDocument, ref anchor).Select();
//            //设置图片宽高
//            oWordApplic.Selection.InlineShapes[1].Width = picWidth;
//            oWordApplic.Selection.InlineShapes[1].Height = picHeight;
//            //将图片设置为四面环绕型
//            WORD.Shape s = oWordApplic.Selection.InlineShapes[1].ConvertToShape();
//            s.WrapFormat.Type = WORD.WdWrapType.wdWrapSquare;
//        }
//        public void InsertLine(float left, float top, float width, float weight, int r, int g, int b)
//        {
//            //SetFontColor("red")
//            //SetAlignment("center");
//            object anchor = oWordApplic.Selection.Range;
//            int pLeft = 0, pTop = 0, pHeight = 0, pWidth = 0;
//            //oWordApplic.ActiveWindow.GetPoint(out pLeft, out pTop,out pWidth, out pHeight, missing);
//            //MessageBox.Show(pLeft + "," + pTop + "," + pWidth + "," + pHeight);
//            object rep = false;
//            //left += oWordApplic.ActiveDocument.PageSetup.LeftMargin;
//            left = oWordApplic.CentimetersToPoints(left);
//            top = oWordApplic.CentimetersToPoints(top);
//            width = oWordApplic.CentimetersToPoints(width);
//            WORD.Shape s = oWordApplic.ActiveDocument.Shapes.AddLine(0, top, width, top, ref anchor);
//            s.Line.ForeColor.RGB = RGB(r, g, b);
//            s.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
//            s.Line.Style = Microsoft.Office.Core.MsoLineStyle.msoLineSingle;
//            s.Line.Weight = weight;
//        }

//        #endregion
//        #region 设置样式
//        ///
//        /// 段落位置设置
//        ///
//        /// 居中、左、右
//        private void SetAlignment(string strAlign)
//        {
//            switch (strAlign.ToLower())
//            {
//                case "center":
//                    oWordApplic.Selection.ParagraphFormat.Alignment = WORD.WdParagraphAlignment.wdAlignParagraphCenter;
//                    break;
//                case "left":
//                    oWordApplic.Selection.ParagraphFormat.Alignment = WORD.WdParagraphAlignment.wdAlignParagraphLeft;
//                    break;
//                case "right":
//                    oWordApplic.Selection.ParagraphFormat.Alignment = WORD.WdParagraphAlignment.wdAlignParagraphRight;
//                    break;
//                case "justify":
//                    oWordApplic.Selection.ParagraphFormat.Alignment = WORD.WdParagraphAlignment.wdAlignParagraphJustify;
//                    break;
//            }
//        }
//        ///
//        /// if you use this function to change the font you should call it
//        /// again with no parameter in order to set the font without a particular
//        /// format
//        ///
//        ///
//        public void SetFont(string strType)
//        {
//            switch (strType)
//            {
//                case "Bold":
//                    oWordApplic.Selection.Font.Bold = 1;
//                    break;
//                case "Italic":
//                    oWordApplic.Selection.Font.Italic = 1;
//                    break;
//                case "Underlined":
//                    oWordApplic.Selection.Font.Subscript = 0;
//                    break;
//            }
//        }
//        ///
//        /// disable all the style
//        ///
//        public void SetFont()
//        {
//            oWordApplic.Selection.Font.Bold = 0;
//            oWordApplic.Selection.Font.Italic = 0;
//            oWordApplic.Selection.Font.Subscript = 0;
//        }
//        public void SetFontSize(float nSize)
//        {
//            SetFontSize(nSize, 100);
//        }
//        ///
//        /// 设置字体大小
//        ///
//        ///
//        ///
//        private void SetFontSize(float nSize, int p)
//        {
//            if (nSize > 0f)
//                oWordApplic.Selection.Font.Size = nSize;
//            if (p > 0)
//                oWordApplic.Selection.Font.Scaling = p;
//        }
//        ///
//        /// 设置字体颜色
//        ///
//        ///
//        public void SetFontColor(string strFontColor)
//        {
//            switch (strFontColor.ToLower())
//            {
//                case "blue":
//                    oWordApplic.Selection.Font.Color = WORD.WdColor.wdColorBlue;
//                    break;
//                case "gold":
//                    oWordApplic.Selection.Font.Color = WORD.WdColor.wdColorGold;
//                    break;
//                case "gray":
//                    oWordApplic.Selection.Font.Color = WORD.WdColor.wdColorGray875;
//                    break;
//                case "green":
//                    oWordApplic.Selection.Font.Color = WORD.WdColor.wdColorGreen;
//                    break;
//                case "lightblue":
//                    oWordApplic.Selection.Font.Color = WORD.WdColor.wdColorLightBlue;
//                    break;
//                case "orange":
//                    oWordApplic.Selection.Font.Color = WORD.WdColor.wdColorOrange;
//                    break;
//                case "red":
//                    oWordApplic.Selection.Font.Color = WORD.WdColor.wdColorRed;
//                    break;
//                case "yellow":
//                    oWordApplic.Selection.Font.Color = WORD.WdColor.wdColorYellow;
//                    break;
//            }
//        }
//        ///
//        /// 设置页码位置
//        ///
//        ///
//        ///
//        public void SetPageNumberAlign(string strType, bool bHeader)
//        {
//            object alignment;
//            object bFirstPage = false;
//            object bF = true;
//            //if (bHeader == true)
//            //{
//            //    oWordApplic.Selection.HeaderFooter.PageNumbers.ShowFirstPageNumber = bF;
//            //}
//            switch (strType)
//            {
//                case "Center":
//                    alignment = WORD.WdPageNumberAlignment.wdAlignPageNumberCenter;
//                    //oWordApplic.Selection.HeaderFooter.PageNumbers.Add(ref alignment, ref bFirstPage);
//                    //WORD.Selection.objSelection = oWordApplic.pSelection;
//                    oWordApplic.Selection.HeaderFooter.PageNumbers[1].Alignment = WORD.WdPageNumberAlignment.wdAlignPageNumberCenter;
//                    break;
//                case "Right":
//                    alignment = WORD.WdPageNumberAlignment.wdAlignPageNumberRight;
//                    oWordApplic.Selection.HeaderFooter.PageNumbers[1].Alignment = WORD.WdPageNumberAlignment.wdAlignPageNumberRight;
//                    break;
//                case "left":
//                    alignment = WORD.WdPageNumberAlignment.wdAlignPageNumberLeft;
//                    oWordApplic.Selection.HeaderFooter.PageNumbers.Add(ref alignment, ref bFirstPage);
//                    break;
//            }
//        }

//        ///
//        /// 设置页面为标准A4公文样式
//        ///
//        public void SetA4PageSetup()
//        {
//            oWordApplic.ActiveDocument.PageSetup.TopMargin = oWordApplic.CentimetersToPoints(3.7f);
//            oWordApplic.ActiveDocument.PageSetup.LeftMargin = oWordApplic.CentimetersToPoints(2.8f);
//            oWordApplic.ActiveDocument.PageSetup.RightMargin = oWordApplic.CentimetersToPoints(2.8f);
//            oWordApplic.ActiveDocument.PageSetup.PageWidth = oWordApplic.CentimetersToPoints(21f);
//            oWordApplic.ActiveDocument.PageSetup.PageHeight = oWordApplic.CentimetersToPoints(29.7f);
//            //oWordApplic.ActiveDocument.PageSetup.HeaderDistance = oWordApplic.CentimetersToPoints(2.5f);
//            //oWordApplic.ActiveDocument.PageSetup.FooterDistance = oWordApplic.CentimetersToPoints(1f);

//        }
//        #endregion
//        #region  其他
//        ///
//        /// RGB转换函数
//        ///
//        ///
//        ///
//        ///
//        ///
//        private int RGB(int r, int g, int b)
//        {
//            return ((b << 16) | (ushort)(((ushort)g << 8) | r));
//        }
//        Color RGBToColor(int color)
//        {
//            int r = 0xFF & color;
//            int g = 0xFF00 & color;
//            g >>= 8;
//            int b = 0xFF0000 & color;
//            b >>= 16;
//            return Color.FromArgb(r, g, b);
//        }
//        #endregion
//        public bool Repalce(string strOldText, string strNewText)
//        {
//            if (oDoc == null)
//                oDoc = oWordApplic.ActiveDocument;
//            this.oDoc.Content.Find.Text = strOldText;
//            object FindText, ReplaceWith, Repalce;
//            FindText = strOldText;
//            Repalce = strNewText;
//            ReplaceWith = strNewText;
//            //wdReplaceAll 替换找到的所有项
//            //wdReplaceNone 不替换找到的任何项
//            //wdRepalceOne 替换找到的第一项
//            Repalce = WORD.WdReplace.wdReplaceAll;
//            oDoc.Content.Find.ClearFormatting();//移除Find 的搜索文本和段落格式设置
//            if (oDoc.Content.Find.Execute(
//                ref FindText, ref missing,
//                ref missing, ref missing,
//                ref missing, ref missing,
//                ref missing, ref missing, ref missing,
//                ref ReplaceWith, ref Repalce,
//                ref missing, ref missing,
//                ref missing, ref missing))
//            {
//                return true;
//            }
//            return false;
//        }
//        public bool SearchReplace(string strOldText, string strNewText)
//        {
//            object replaceAll = WORD.WdReplace.wdReplaceAll;
//            //首先清除任何现有的格式设置选项，然后设置搜索的字符串strOldText
//            oWordApplic.Selection.Find.ClearFormatting();
//            oWordApplic.Selection.Find.Text = strOldText;
//            oWordApplic.Selection.Find.Replacement.Text = strNewText;
//            if (oWordApplic.Selection.Find.Execute(
//                ref missing, ref missing, ref missing, ref missing, ref missing,
//                ref missing, ref missing, ref missing, ref missing, ref missing,
//                ref replaceAll, ref missing, ref missing, ref missing, ref missing))
//            {
//                return true;
//            }
//            return false;
//        }
//    }
//}