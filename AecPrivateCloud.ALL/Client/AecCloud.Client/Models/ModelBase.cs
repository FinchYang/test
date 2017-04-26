using System;
using System.ComponentModel;

namespace AecCloud.Client.Models
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string GetProperyName(string methodName)
        {
            if (methodName.StartsWith("get_")
                || methodName.StartsWith("set_")
                || methodName.StartsWith("put_"))
            {
                return methodName.Substring("get_".Length);
            }

            throw new Exception(methodName + " not a method of property.");
        }

        protected bool SetProperty<T>(ref T storage, T value)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            string propertyName = GetProperyName(new System.Diagnostics.StackTrace(true).GetFrame(1).GetMethod().Name);
            this.OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
