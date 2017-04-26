//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
//using Microsoft.Office.Interop;
//using Microsoft.Vbe.Interop;
//using Word = Microsoft.Office.Interop.Word;
//using System.Runtime.InteropServices;

//namespace WindowsFormsApplication1
//{
//    public partial class Form1 : System.Windows.Forms.Form
//    {

//        [DllImport("shell32.dll ")]
//        public static extern int ShellExecute(IntPtr hwnd, String lpszOp, String lpszFile, String lpszParams, String lpszDir, int FsShowCmd);
//        public Form1()
//        {
//            InitializeComponent();
//        }

//        private void button1_Click(object sender, EventArgs e)
//        {
//            //新建文档
//            // Word.Application newapp = new Word.Application();//用这句也能初始化
//            Word.Application newapp = new Word.Application();
//            Word.Document newdoc;
//            object nothing = System.Reflection.Missing.Value;//用于作为函数的默认参数
//            newdoc = newapp.Documents.Add(ref nothing, ref nothing, ref nothing, ref nothing);//生成一个word文档
//            newapp.Visible = true;//是否显示word程序界面

//            //页面设置
//            //newdoc.PageSetup.Orientation = Word.WdOrientation.wdOrientLandscape ;
//            //newdoc.PageSetup.PageWidth = newapp.CentimetersToPoints(21.0f);
//            //newdoc.PageSetup.PageHeight = newapp.CentimetersToPoints(29.7f);
//            newdoc.PageSetup.PaperSize = Word.WdPaperSize.wdPaperA4;
//            newdoc.PageSetup.Orientation = Word.WdOrientation.wdOrientPortrait;
//            newdoc.PageSetup.TopMargin = 57.0f;
//            newdoc.PageSetup.BottomMargin = 57.0f;
//            newdoc.PageSetup.LeftMargin = 57.0f;
//            newdoc.PageSetup.RightMargin = 57.0f;
//            newdoc.PageSetup.HeaderDistance = 30.0f;//页眉位置

//            //设置页眉
//            newapp.ActiveWindow.View.Type = Word.WdViewType.wdOutlineView;//视图样式。
//            newapp.ActiveWindow.View.SeekView = Word.WdSeekView.wdSeekPrimaryHeader;//进入页眉设置，其中页眉边距在页面设置中已完成
//            newapp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
//            //插入页眉图片
//            string headerfile = "d:\\header.jpg";
//            this.outpicture(headerfile, Properties.Resources.header);
//            Word.InlineShape shape1 = newapp.ActiveWindow.ActivePane.Selection.InlineShapes.AddPicture(headerfile, ref nothing, ref nothing, ref nothing);
//            shape1.Height = 30;
//            shape1.Width = 80;
//            newapp.ActiveWindow.ActivePane.Selection.InsertAfter("中建东北院");
//            //去掉页眉的那条横线
//            newapp.ActiveWindow.ActivePane.Selection.ParagraphFormat.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineStyle = Word.WdLineStyle.wdLineStyleNone;
//            newapp.ActiveWindow.ActivePane.Selection.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].Visible = false;
//            newapp.ActiveWindow.ActivePane.View.SeekView = Word.WdSeekView.wdSeekMainDocument;//退出页眉设置

//            //添加页码
//            Word.PageNumbers pns = newapp.Selection.Sections[1].Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterEvenPages].PageNumbers;
//            pns.NumberStyle = Word.WdPageNumberStyle.wdPageNumberStyleNumberInDash;
//            pns.HeadingLevelForChapter = 0;
//            pns.IncludeChapterNumber = false;
//            pns.ChapterPageSeparator = Word.WdSeparatorType.wdSeparatorHyphen;
//            pns.RestartNumberingAtSection = false;
//            pns.StartingNumber = 0;
//            object pagenmbetal = Word.WdPageNumberAlignment.wdAlignPageNumberCenter;
//            object first = true;
//            newapp.Selection.Sections[1].Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterEvenPages].PageNumbers.Add(ref pagenmbetal, ref first);

//            //文字设置(Selection表示当前选择集，如果当前没有选择对像，则指对光标所在处进行设置)
//            newapp.Selection.Font.Size = 14;
//            newapp.Selection.Font.Bold = 0;
//            newapp.Selection.Font.Color = Word.WdColor.wdColorBlack;
//            newapp.Selection.Font.Name = "宋体";

//            //段落设置
//            newapp.Selection.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceExactly;
//            newapp.Selection.ParagraphFormat.LineSpacing = 20;
//            newapp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
//            newapp.Selection.ParagraphFormat.FirstLineIndent = 30;
//            newdoc.Content.InsertAfter(WindowsFormsApplication1.Properties.Resources.PreViewWords);



//            //插入公式
//            object oEndOfDoc = "\\endofdoc";
//            Word.Range rang1 = newdoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
//            object fieldType = Word.WdFieldType.wdFieldEmpty;
//            object formula = @"eq \i(a,b,ξxdx)";
//            object presrveFormatting = false;
//            rang1.Text = formula.ToString();
//            rang1.Font.Size = 14;
//            rang1.Font.Bold = 0;
//            rang1.Font.Subscript = 0;
//            rang1.Font.Color = Word.WdColor.wdColorBlue;
//            rang1.Font.Name = "宋体";
//            rang1.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;
//            rang1.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
//            newdoc.Fields.Add(rang1, ref fieldType, ref formula, ref presrveFormatting);

//            //将文档的前三个字替换成"asdfasdf"，并将其颜色设为蓝色
//            object start = 0;
//            object end = 3;
//            Word.Range rang2 = newdoc.Range(ref start, ref end);
//            rang2.Font.Color = Word.WdColor.wdColorBlue;
//            rang2.Text = "as签";

//            //将文档开头的"as"替换成"袁波"
//            rang1.Start = 0;
//            rang1.End = 2;
//            rang1.Text = "这是一个";
//            rang1.InsertAfter("书");
//            //rang1.Select();
//            object codirection = Word.WdCollapseDirection.wdCollapseStart;
//            rang1.Collapse(ref codirection);//将rang1的起点和终点都定于起点或终点

//            //对前三个字符进行加粗
//            newdoc.Range(ref start, ref end).Bold = 1;
//            object rang = rang2;
//            newdoc.Bookmarks.Add("yb", ref rang);


//            object unite = Word.WdUnits.wdStory;
//            newapp.Selection.EndKey(ref unite, ref nothing);//将光标移至文末
//            newapp.Selection.Font.Size = 10;
//            newapp.Selection.TypeText("...............................(式1)\n");


//            //插入图片
//            newapp.Selection.EndKey(ref unite, ref nothing);//将光标移至文末
//            //newapp.Selection.HomeKey(ref unite, ref nothing);//将光标移至文开头
//            newapp.Selection.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;
//            newapp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
//            object LinkToFile = false;
//            object SaveWithDocument = true;
//            object Anchor = newapp.Selection.Range;
//            string picname = "d:\\kk.jpg";
//            this.outpicture(picname, Properties.Resources.IMG_2169);
//            newdoc.InlineShapes.AddPicture(picname, ref LinkToFile, ref SaveWithDocument, ref Anchor);
//            newdoc.InlineShapes[1].Height = 200;
//            newdoc.InlineShapes[1].Width = 200;
//            newdoc.Content.InsertAfter("\n");
//            newapp.Selection.EndKey(ref unite, ref nothing);//将光标移至文末
//            newapp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
//            newapp.Selection.Font.Size = 10;
//            newapp.Selection.TypeText("图1 袁冶\n");
//            newapp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
//            newdoc.Content.InsertAfter("\n");
//            newdoc.Content.InsertAfter("\n");

//            //用这种方式也可以插入公式，并且这种方法更简单
//            newapp.Selection.Font.Size = 14;
//            newapp.Selection.InsertFormula(ref formula, ref nothing);
//            newapp.Selection.Font.Size = 10;
//            newapp.Selection.TypeText("..............................(式2)\n");
//            newapp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
//            newapp.Selection.TypeText("表1 电子产品\n");

//            //插入表格
//            Word.Table table1 = newdoc.Tables.Add(newapp.Selection.Range, 4, 3, ref nothing, ref nothing);
//            newdoc.Tables[1].Cell(1, 1).Range.Text = "产品\n项目";
//            newdoc.Tables[1].Cell(1, 2).Range.Text = "电脑";
//            newdoc.Tables[1].Cell(1, 3).Range.Text = "手机";
//            newdoc.Tables[1].Cell(2, 1).Range.Text = "重量(kg)";
//            newdoc.Tables[1].Cell(3, 1).Range.Text = "价格(元)";
//            newdoc.Tables[1].Cell(4, 1).Range.Text = "共同信息";
//            newdoc.Tables[1].Cell(4, 2).Range.Text = "信息A";
//            newdoc.Tables[1].Cell(4, 3).Range.Text = "信息B";


//            table1.Select();
//            table1.Rows.Alignment = Word.WdRowAlignment.wdAlignRowCenter;//整个表格居中
//            newapp.Selection.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
//            newapp.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
//            newapp.Selection.Cells.HeightRule = Word.WdRowHeightRule.wdRowHeightExactly;
//            newapp.Selection.Cells.Height = 40;
//            table1.Rows[2].Height = 20;
//            table1.Rows[3].Height = 20;
//            table1.Rows[4].Height = 20;
//            table1.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
//            newapp.Selection.Cells.Width = 150;
//            table1.Columns[1].Width = 75;
//            table1.Cell(1, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
//            table1.Cell(1, 1).Range.Paragraphs[2].Format.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;




//            //表头斜线
//            table1.Cell(1, 1).Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderDiagonalDown].Visible = true;
//            table1.Cell(1, 1).Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderDiagonalDown].Color = Word.WdColor.wdColorGreen;
//            table1.Cell(1, 1).Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderDiagonalDown].LineWidth = Word.WdLineWidth.wdLineWidth050pt;

//            //表格边框
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderHorizontal].Visible = true;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderHorizontal].Color = Word.WdColor.wdColorGreen;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderHorizontal].LineWidth = Word.WdLineWidth.wdLineWidth050pt;

//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderVertical].Visible = true;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderVertical].Color = Word.WdColor.wdColorGreen;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderVertical].LineWidth = Word.WdLineWidth.wdLineWidth050pt;

//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderLeft].Visible = true;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderLeft].Color = Word.WdColor.wdColorGreen;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderLeft].LineWidth = Word.WdLineWidth.wdLineWidth050pt;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderLeft].LineStyle = Word.WdLineStyle.wdLineStyleDoubleWavy;

//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderRight].Visible = true;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderRight].Color = Word.WdColor.wdColorGreen;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderRight].LineWidth = Word.WdLineWidth.wdLineWidth050pt;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderRight].LineStyle = Word.WdLineStyle.wdLineStyleDoubleWavy;

//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].Visible = true;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].Color = Word.WdColor.wdColorGreen;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineWidth = Word.WdLineWidth.wdLineWidth050pt;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderBottom].LineStyle = Word.WdLineStyle.wdLineStyleDouble;

//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderTop].Visible = true;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderTop].Color = Word.WdColor.wdColorGreen;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderTop].LineWidth = Word.WdLineWidth.wdLineWidth050pt;
//            table1.Borders[Microsoft.Office.Interop.Word.WdBorderType.wdBorderTop].LineStyle = Word.WdLineStyle.wdLineStyleDouble;

//            //合并单元格
//            newdoc.Tables[1].Cell(4, 2).Merge(table1.Cell(4, 3));

//            //删除图片
//            this.delpictfile(headerfile);
//            this.delpictfile(picname);


//            //保存文档
//            object name = "c:\\yb3.doc";
//            newdoc.SaveAs(ref name, ref nothing, ref nothing, ref nothing, ref nothing, ref nothing, ref nothing,
//                   ref nothing, ref nothing, ref nothing, ref nothing, ref nothing, ref nothing, ref nothing,
//                   ref nothing, ref nothing);

//            //关闭文档
//            object saveOption = Word.WdSaveOptions.wdDoNotSaveChanges;
//            newdoc.Close(ref nothing, ref nothing, ref nothing);
//            newapp.Application.Quit(ref saveOption, ref nothing, ref nothing);
//            newdoc = null;
//            newapp = null;
//            ShellExecute(IntPtr.Zero, "open", "c:\\yb3.doc", "", "", 3);
//        }

//        private void outpicture(string filename, System.Drawing.Bitmap bmap)
//        {
//            bmap.Save(filename);
//        }

//        private void delpictfile(string filename)
//        {
//            System.IO.File.Delete(filename);
//        }

//    }

//}