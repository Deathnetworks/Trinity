using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    public class CombatContext
    {
        public CombatContext()
        {
            // don't set anything by default
        }

        public bool UseOutOfCombatBuff
        {
            get
            {
                if (CurrentTarget == null)
                    return true;
                else
                    return false;
            }
        }
        public bool IsCurrentlyAvoiding
        {
            get
            {
                if (CurrentTarget.Type == GObjectType.Avoidance)
                    return true;
                else
                    return false;
            }
        }
        public bool UseDestructiblePower
        {
            get
            {
                switch (CurrentTarget.Type)
                {
                    case GObjectType.Destructible:
                    case GObjectType.Barricade:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public HashSet<SNOPower> Hotbar
        {
            get
            {
                return Trinity.Hotbar;
            }
        }
        public PlayerInfoCache PlayerStatus
        {
            get
            {
                return Trinity.PlayerStatus;
            }
        }

        public TrinityCacheObject CurrentTarget
        {
            get
            {
                return Trinity.CurrentTarget;
            }
        }
    }
}
