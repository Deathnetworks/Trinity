using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Trinity.Technicals;
using Trinity.UIComponents;

namespace Trinity.UI.UIComponents
{
    public class CacheUIDataModel
    {
        internal ObservableCollection<TrinityCacheObject> ObservableCache = new ObservableCollection<TrinityCacheObject>();
        internal List<TrinityCacheObject> SourceCacheObjects = new List<TrinityCacheObject>(); 

        public CacheUIDataModel()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }
    }
}
