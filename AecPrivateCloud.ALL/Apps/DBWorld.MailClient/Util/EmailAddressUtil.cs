using System;
using System.Collections.Generic;
using System.Linq;

namespace DBWorld.MailClient.Util
{
    public class EmailAddressUtil
    {
        private readonly string _content;

        private readonly int _position;

        private readonly char _flag;


        public class SubAddress
        {
            public string Str { get; set; }
            public int Pos { get; set; }
        }

        public EmailAddressUtil(string text, char flag, int position)
        {
            _content = text;
            _position = position;
            _flag = flag;

            if (!_content.EndsWith(";"))
            {
                _content += ";";
            }
        }

        /// <summary>
        /// 获取选择段的序号
        /// </summary>
        /// <returns></returns>
        public int GetSelectedItemIndex()
        {
            var list = GetFlagsIndex(_content, _flag);

            if (list.Count == 1)
            {
                return 0;
            }
            else
            {
                var pos = 0;
                for (int i = 0; i < list.Count - 1; i++) // p1 < p < p2 ...
                {
                    if (_position <= list[0])
                    {
                        pos = 0;
                        break;
                    }

                    if (list[i] + 1 < _position && _position <= list[i + 1] + 1)
                    {
                        pos = i + 1;
                        break;
                    }
                }

                return pos;
            }
        }

        /// <summary>
        /// 获取选择段的字符串
        /// </summary>
        /// <returns></returns>
        public string GetSelectedItem()
        {
            var index = GetSelectedItemIndex();

            var items = _content.Split(_flag);
            return items[index];
        }

        /// <summary>
        /// 按“Backspace”删除指定段序号的字符串
        /// </summary>
        /// <param name="index">段序号</param>
        /// <returns></returns>
        public SubAddress BackSelectedItemAt(int index)
        {
            
            var array = _content.Split(_flag);
            var items = array.ToList();
            items.Remove("");
      
            if (index < 0 || items.Count() - 1 < index)
            {
                //异常处理
                return new SubAddress
                {
                    Str = _content,
                    Pos = _content.Length
                };
            }

            //删除指定的项
            items.RemoveAt(index);

            //删除后则字符串
            var str = String.Empty;
            foreach (var item in items)
            {
                str += item;
                str += ";";
            }

            //删除后光标位置
            var pos = 0;
            var list = GetFlagsIndex(str, _flag);
            if (index == 0)
            {
                pos = 0;
            }
            else
            {
                pos = list[index - 1] + 1;
            }

            return new SubAddress
            {
                Str = str,
                Pos = pos
            };
        }

        /// <summary>
        /// 按“Delete”删除指定段序号的字符串
        /// </summary>
        /// <param name="index">段序号</param>
        /// <returns></returns>
        public SubAddress DeleteSelectedItemAt(int index)
        {
            var array = _content.Split(_flag);
            var items = array.ToList();
            items.Remove("");

            if (index < 0 || items.Count() - 1 < index)
            {
                //异常处理
                return new SubAddress
                {
                    Str = _content,
                    Pos = _content.Length
                };
            }

            //删除指定的项
            if (_position == 0)
            {
                items.RemoveAt(index);
            }
            else
            {
                items.RemoveAt(index + 1);
            }

            //删除后则字符串
            var str = String.Empty;
            foreach (var item in items)
            {
                str += item;
                str += ";";
            }

            return new SubAddress
            {
                Str = str,
                Pos = _position
            };
        }

        /// <summary>
        /// 获取选择的字符串分段
        /// </summary>
        /// <param name="start">光标开始的位置</param>
        /// <param name="length">选择的字符长度</param>
        public void GetSelectedItemSection(ref int start, ref int length)
        {
            var list = GetFlagsIndex(_content, _flag);

            if (list.Count == 1)
            {
                start = 0;
                length = _content.Length;
            }
            else //count > 1
            {
                if (_position == 0) //p=0
                {
                    start = 0;
                    length = list[0] + 1;
                    return;
                }

                for (int i = 0; i < list.Count; i++) //p = (p1 || p2 || p3 ...)
                {
                    if (_position == (list[i] + 1) || _position == list[i])
                    {
                        if (i != 0)
                        {
                            start = list[i - 1] + 1;
                        }
                        length = list[i] - start + 1;
                        return;
                    }
                }

                for (int i = 0; i < list.Count - 1; i++) // p1 < p < p2 ...
                {
                    if (_position < list[0])
                    {
                        start = 0;
                        length = list[0] + 1;
                        return;
                    }

                    if (list[i] < _position && _position < list[i+1])
                    {
                        start = list[i] + 1;
                        length = list[i + 1] - start + 1;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 获取标记字符所有位置
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="flag">标记字符（分隔字符）</param>
        /// <returns></returns>
        private List<int> GetFlagsIndex(string str, char flag)
        {
            var list = new List<int>();
            var array = str.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == flag)
                {
                    list.Add(i);
                }
            }

            return list;
        }
    }
}
