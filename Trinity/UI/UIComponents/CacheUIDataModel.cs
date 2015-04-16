using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Trinity.UI.UIComponents
{
    public class CacheUIDataModel : INotifyPropertyChanged
    {
        internal List<TrinityCacheObject> SourceCacheObjects = new List<TrinityCacheObject>();

        private ObservableCollection<TrinityCacheObject> _observableCache = new ObservableCollection<TrinityCacheObject>();
        internal ObservableCollection<TrinityCacheObject> ObservableCache
        {
            get { return _observableCache; }
            set { SetField(ref _observableCache, value, "ObservableCache"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
