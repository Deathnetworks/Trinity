using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Trinity.Cache;

namespace Trinity.UI.UIComponents
{
    public class CacheUIDataModel : INotifyPropertyChanged
    {
        public CacheUIDataModel()
        {
            Cache = new ObservableCollection<CacheUIObject>(CacheUI.GetCacheActorList());
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set { SetField(ref _enabled, value, "Enabled"); }
        }

        private ObservableCollection<CacheUIObject> _cache = new ObservableCollection<CacheUIObject>();
        public ObservableCollection<CacheUIObject> Cache
        {
            get { return _cache; }
            set { SetField(ref _cache, value, "Cache"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
