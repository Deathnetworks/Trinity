using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Trinity.Reference;
using Trinity.Settings.Loot;
using Trinity.UI.UIComponents;

namespace Trinity.Settings.Mock
{
    public class ItemListMockData
    {
        public CollectionViewSource Collection { get; set; }   
        public List<SettingsItem> DisplayItems { get; set; }

        /// <summary>
        /// Mock Data for viewing ItemList controls in DesignTime
        /// </summary>
        public ItemListMockData()
        {
            DisplayItems = new List<SettingsItem>
            {
                new SettingsItem(Legendary.BombadiersRucksack)
                {
                    IsSelected = true
                },
                new SettingsItem(Legendary.BoardWalkers),
                new SettingsItem(Legendary.LutSocks),
                new SettingsItem(Legendary.BreastplateOfAkkhan)
                {
                    IsSelected = true
                },
                new SettingsItem(Legendary.Cindercoat),
                new SettingsItem(Legendary.FlyingDragon), 
            };

            Collection = new CollectionViewSource();
            Collection.Source = DisplayItems;
            Collection.GroupDescriptions.Add(new PropertyGroupDescription("ItemType"));
        }
    }
}
