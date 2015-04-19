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

        private ObservableCollection<CacheUIObject> _cache = new ObservableCollection<CacheUIObject>();
        public ObservableCollection<CacheUIObject> Cache
        {
            get { return _cache; }
            set
            {
                if (_cache != value)
                {
                    _cache = value;
                    OnPropertyChanged("Cache");
                }
            }
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
