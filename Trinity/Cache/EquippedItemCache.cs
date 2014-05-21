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
            get { return new HashSet<ACDItem>(_items.Where(i => i.IsValid)); }
            set { _items = value; }
        }
        public HashSet<int> ItemIds { get; set; }

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

            var itemsList = ZetaDia.Me.Inventory.Equipped.Where(i => i.IsValid).ToList();

            if (!itemsList.Any())
                return;

            Items = new HashSet<ACDItem>(itemsList);
            ItemIds = new HashSet<int>(itemsList.Select(i => i.ActorSNO));
        }

    }
}
