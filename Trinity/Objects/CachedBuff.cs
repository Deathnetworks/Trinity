using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Helpers;
using Zeta.Game.Internals.Actors;
using Trinity.Technicals;

namespace Trinity.Objects
{
    public class CachedBuff
    {
        public CachedBuff() { }

        public CachedBuff(Buff buff)
        {
            _buff = buff;
            InternalName = buff.InternalName;
            IsCancellable = buff.IsCancelable;
            StackCount = buff.StackCount;
            Id = buff.SNOId;
        }

        private readonly Buff _buff;
        public string InternalName { get; set; }
        public bool IsCancellable { get; set; }
        public int StackCount { get; set; }
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; }

        public void Cancel()
        {
            if (IsCancellable && _buff.IsValid)
                _buff.Cancel();
        }

        public override string ToString()
        {
            return ToStringReflector.GetObjectString(this);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        
    }
}
