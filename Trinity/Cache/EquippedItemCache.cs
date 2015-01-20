using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Cache
{
    public class EquippedItemCache
    {
        private static EquippedItemCache _instance;
        public static EquippedItemCache Instance { get { return _instance ?? (_instance = new EquippedItemCache()); } }

        private HashSet<ACDItem> _items;
        public HashSet<ACDItem> Items
        {
            get
            {
                if (ShouldUpdate)
                    Update();

                return new HashSet<ACDItem>(_items.Where(i => i.IsValid));                 
            }
            set { _items = value; }
        }

        private HashSet<int> _itemIds;
        public HashSet<int> ItemIds
        {
            get
            {
                return new HashSet<int>(Items.Select(i => i.ActorSNO));
            }
            set { _itemIds = value; }
        }

        public EquippedItemCache()
        {
            _items = new HashSet<ACDItem>();
            Update();
        }

        public void Update()
        {
            if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld || ZetaDia.Me == null)
                return;

            if (!ZetaDia.Me.IsValid)
                return;

            var itemsList = ZetaDia.Me.Inventory.Equipped.Where(i => i.IsValid && i.ACDGuid > 0).ToList();
            var gemsList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).Where(i => i.IsValid && i.ACDGuid > 0 && i.InventorySlot == InventorySlot.Socket);
            itemsList.AddRange(gemsList);
            _lastUpdate = DateTime.UtcNow;

            if (!itemsList.Any())
                return;

            Items = new HashSet<ACDItem>(itemsList);
            ItemIds = new HashSet<int>(itemsList.Select(i => i.ActorSNO));
        }

        private DateTime _lastUpdate = DateTime.MinValue;
        private bool ShouldUpdate
        {
            get { return DateTime.UtcNow.Subtract(_lastUpdate) > TimeSpan.FromSeconds(5); }
        } 

    }
}
