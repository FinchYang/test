using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace AecCloud.Client.Util
{
    public static class FindFrameworkElementUtil
    {
        /// <summary>
        /// 根据控件名称，查找父控件
        /// elementName为空时，查找指定类型的父控件
        /// </summary>
        public static T GetParentByName<T>(this DependencyObject obj, string elementName)
        where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if ((parent is T) && (((T)parent).Name == elementName || string.IsNullOrEmpty(elementName)))
                {
                    return (T)parent;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        /// <summary>
        /// 根据控件名称，查找子控件
        /// elementName为空时，查找指定类型的子控件
        /// </summary>
        public static T GetChildByName<T>(this DependencyObject obj, string elementName)
        where T : FrameworkElement
        {
            DependencyObject child = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (((T)child).Name == elementName) || (string.IsNullOrEmpty(elementName)))
                {
                    return (T)child;
                }
                else
                {
                    T grandChild = GetChildByName<T>(child, elementName);
                    if (grandChild != null)
                    {
                        return grandChild;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 根据控件名称，查找子控件集合
        /// elementName为空时，查找指定类型的所有子控件
        /// </summary>
        public static List<T> GetChildsByName<T>(this DependencyObject obj, string elementName)
        where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (((T)child).Name == elementName) || (string.IsNullOrEmpty(elementName)))
                {
                    childList.Add((T)child);
                }
                else
                {
                    List<T> grandChildList = GetChildsByName<T>(child, elementName);
                    if (grandChildList != null)
                    {
                        childList.AddRange(grandChildList);
                    }
                }
            }
            return childList;
        }
    }
}

