using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Trinity.Cache;
using Trinity.LazyCache;

namespace Trinity.UI.UIComponents
{
    public class CacheUIDataModel : INotifyPropertyChanged
    {
        public CacheUIDataModel()
        {

        }

        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set { SetField(ref _enabled, value); }
        }

        private bool _isDefaultVisible = true;
        public bool IsDefaultVisible
        {
            get { return _isDefaultVisible; }
            set { SetField(ref _isDefaultVisible, value); }
        }

        private bool _isLazyCacheVisible;
        public bool IsLazyCacheVisible
        {
            get { return _isLazyCacheVisible; }
            set { SetField(ref _isLazyCacheVisible, value); }
        }

        private ObservableCollection<CacheUIObject> _cache = new ObservableCollection<CacheUIObject>();
        public ObservableCollection<CacheUIObject> Cache
        {
            get { return _cache; }
            set { SetField(ref _cache, value); }
        }

        private ObservableCollection<TrinityObject> _lazyCache = new ObservableCollection<TrinityObject>();
        public ObservableCollection<TrinityObject> LazyCache
        {
            get { return _lazyCache; }
            set { SetField(ref _lazyCache, value); }
        }

        private CollectionViewSource _collection = new CollectionViewSource();
        public CollectionViewSource Collection
        {
            get { return _collection; }
            set { SetField(ref _collection, value); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
