using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Trinity.Reference;
using Trinity.Settings.Loot;
using Trinity.UI.UIComponents;
using Trinity.UIComponents;
using Zeta.Game.Internals.Actors;

namespace Trinity.Settings.Mock
{
    public class ItemListMockData
    {
        public CollectionViewSource Collection { get; set; }   
        public List<ItemListItem> DisplayItems { get; set; }

        /// <summary>
        /// Mock Data for viewing ItemList controls in DesignTime
        /// </summary>
        public ItemListMockData()
        {
            DisplayItems = new List<ItemListItem>
            {
                new ItemListItem(Legendary.BombadiersRucksack)
                {
                    IsSelected = true,
                    Rules = new ObservableCollection<ItemRule>
                    {
                        new ItemRule
                        {
                            Name = "Ancient really long name",
                            ItemPropertyId = (int)ItemProperty.Ancient                            
                        },
                        new ItemRule
                        {
                            Name = "Armor",
                            ItemPropertyId = (int)ItemProperty.Armor                            
                        }
                    }
                },
                new ItemListItem(Legendary.BoardWalkers),
                new ItemListItem(Legendary.LutSocks),
                new ItemListItem(Legendary.BreastplateOfAkkhan)
                {
                    IsSelected = true
                },
                new ItemListItem(Legendary.Cindercoat),
                new ItemListItem(Legendary.FlyingDragon), 
            };

            Collection = new CollectionViewSource();
            Collection.Source = DisplayItems;
            Collection.GroupDescriptions.Add(new PropertyGroupDescription("ItemType"));
        }
    }
}
