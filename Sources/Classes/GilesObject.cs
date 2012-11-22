using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Profile.Composites;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // GilesObject type used to cache all data
        // Let's me create an object list with ONLY the data I need read from D3 memory, and then read from this while
        // Handling movement and interaction with the target - whether the target is a shrine, an item or a monster
        // Completely minimizing the D3 memory reads to the bare minimum
        public class GilesObject
        {
            // Generic stuff applicable to all objects
            public GilesObjectType GilesObjectType { get; set; }
            public double dWeight { get; set; }
            public Vector3 vPosition { get; set; }
            public float fCentreDist { get; set; }
            public float fRadiusDistance { get; set; }
            public string sInternalName { get; set; }
            public int iACDGuid { get; set; }
            public int iRActorGuid { get; set; }
            public int iDynamicID { get; set; }
            public int iBalanceID { get; set; }
            public int iActorSNO { get; set; }
            // Item/gold/other stuff
            public int iLevel { get; set; }
            public int iGoldAmount { get; set; }
            public bool bOneHanded { get; set; }
            public ItemQuality eItemQuality { get; set; }
            public ItemType eDBItemType { get; set; }
            public FollowerType eFollowerType { get; set; }
            public GilesItemType eGilesItemType { get; set; }
            // Monster/unit stuff
            public bool bIsElite { get; set; }
            public bool bIsRare { get; set; }
            public bool bIsUnique { get; set; }
            public bool bIsMinion { get; set; }
            public bool bIsTreasureGoblin { get; set; }
            public bool IsEliteRareUnique { get; set; }
            public bool IsBoss { get; set; }
            public bool bIsAttackable { get; set; }
            public double iHitPoints { get; set; }
            public float fRadius { get; set; }
            public bool bForceLeapAgainst { get; set; }
            public MonsterSize eMonsterStyle { get; set; }
            // A reference to the original object for fast updates
            public DiaUnit tUnit
            {
                get
                {
                    try
                    {
                        return (DiaUnit)tObject;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            public DiaObject tObject { get; set; }
            public GilesObject(Vector3 _position, GilesObjectType _objectType = GilesObjectType.Unknown, double _weight = 0, float _distance = 0f, float _radiusDistance = 0f,
                string _internalName = "", int _acdguid = -1, int _ractorGuid = -1, int _dynamicId = -1, int _balanceId = -1, int _actorsno = -1, int _level = -1, int _gold = -1,
                bool _onehand = true, ItemQuality _quality = ItemQuality.Invalid, ItemType _itemtype = ItemType.Unknown, FollowerType _followertype = FollowerType.None,
                GilesItemType _gilestype = GilesItemType.Unknown, bool _elite = false, bool _rare = false, bool _unique = false, bool _minion = false,
                bool _treasure = false, bool _boss = false, bool _attackable = false, double _hitpoints = 0d, float _radius = 0f, MonsterSize _monstersize = MonsterSize.Unknown,
                DiaObject _DiaObject = null, bool uniquerareelite = false, bool forceleap = false)
            {
                vPosition = _position;
                GilesObjectType = _objectType;
                dWeight = _weight;
                fCentreDist = _distance;
                fRadiusDistance = _radiusDistance;
                sInternalName = _internalName;
                iACDGuid = _acdguid;
                iRActorGuid = _ractorGuid;
                iDynamicID = _dynamicId;
                iBalanceID = _balanceId;
                iActorSNO = _actorsno;
                iLevel = _level;
                iGoldAmount = _gold;
                bOneHanded = _onehand;
                eItemQuality = _quality;
                eDBItemType = _itemtype;
                eFollowerType = _followertype;
                eGilesItemType = _gilestype;
                bIsElite = _elite;
                bIsRare = _rare;
                bIsUnique = _unique;
                bIsMinion = _minion;
                bIsTreasureGoblin = _treasure;
                IsEliteRareUnique = uniquerareelite;
                IsBoss = _boss;
                bIsAttackable = _attackable;
                iHitPoints = _hitpoints;
                fRadius = _radius;
                eMonsterStyle = _monstersize;
                tObject = _DiaObject;
                bForceLeapAgainst = forceleap;
            }
            // For cloning the object (and not simply referencing it)
            public GilesObject Clone()
            {
                GilesObject newGilesObject = new GilesObject(vPosition, GilesObjectType, dWeight, fCentreDist, fRadiusDistance, sInternalName, iACDGuid,
                    iRActorGuid, iDynamicID, iBalanceID, iActorSNO, iLevel, iGoldAmount, bOneHanded,
                    eItemQuality, eDBItemType, eFollowerType, eGilesItemType, bIsElite, bIsRare, bIsUnique,
                    bIsMinion, bIsTreasureGoblin, IsBoss, bIsAttackable, iHitPoints, fRadius, eMonsterStyle, tUnit, IsEliteRareUnique, bForceLeapAgainst);
                return newGilesObject;
            }
        }
    }
}
