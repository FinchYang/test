using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimulaDesign.WPFPluginCore
{
    /// <summary>
    /// 插入插件（必须实现）
    /// </summary>
    public interface IPlugin
    {
        UIElement InsertPlugin();
    }

    /// <summary>
    /// 元数据（必须导出元数据）
    /// </summary>
    public interface IMetadata
    {
        string Key { get; }
        string Value { get; }
    }
    /// <summary>
    /// 界面(页面)接收数据接口
    /// </summary>
    public interface INavigationSetter
    {
        void SetModelContext(object context);
    }

    public interface INavigationSetter<in T> : INavigationSetter
    {
        void SetModelContext(T context);
    }
}
