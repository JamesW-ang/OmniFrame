using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OmniFrame.Wpf.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            try { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
            catch (Exception ex) { OmniFrame.Common.Logger.Error($"ViewModel 属性通知异常 [{GetType().Name}.{name}]", ex); }
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
