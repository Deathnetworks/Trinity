using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.Game.Internals.SNO;

namespace Trinity.LazyCache
{
    public partial class CacheMeta
    {
        public static ActorMeta GetOrCreateActorMeta(CacheBase cacheBase)
        {
            return GetOrCreateActorMeta(cacheBase.Object, cacheBase.Source, cacheBase.ActorSNO, cacheBase.ActorType);
        }

        public static ActorMeta GetOrCreateActorMeta(DiaObject diaObject, ACD acd, int actorSNO, ActorType actorType)
        {
            ActorMeta actorMeta;

            if (!ReferenceActorMeta.TryGetValue(actorSNO, out actorMeta))
            {
                actorMeta = CreateActorMeta(diaObject, acd, actorSNO, actorType);
            }

            return actorMeta;
        }

        private static ActorMeta CreateActorMeta(DiaObject diaObject, ACD acd, int actorSNO, ActorType actorType)
        {
            ActorMeta actorMeta;

            if (actorType == ActorType.Monster && diaObject is DiaUnit || actorType == ActorType.Gizmo && diaObject is DiaGizmo || actorType == ActorType.Player)
            {
                actorMeta = new ActorMeta(diaObject, acd, actorSNO, actorType);

                if (Trinity.Settings.Advanced.ExportNewActorMeta && actorMeta.IsValid && !actorMeta.IsPartial)
                {
                    Logger.Log("Exporting ActorMeta for {0} ({1})", acd.Name, actorSNO);
                    WriteToLog(actorMeta);
                }

                ReferenceActorMeta.Add(actorSNO, actorMeta);

            }
            else if (actorType == ActorType.Player)
            {
                actorMeta = new ActorMeta(diaObject, acd, actorSNO, actorType);
            }
            else
            {
                actorMeta = new ActorMeta();
            }

            return actorMeta;
        }

        private static ActorMeta CreateActorMeta(CacheBase cacheBase)
        {
            return CreateActorMeta(cacheBase.Object, cacheBase.Source, cacheBase.ActorSNO, cacheBase.ActorType);
        }

        /// <summary>
        /// Writes an ActorMeta record to the log file in the format for a Dictionary collection initializer
        /// </summary>
        public static void WriteToLog(ActorMeta meta)
        {
            if (ReferenceActorMeta.ContainsKey(meta.ActorSNO))
                return;

            var logStream = File.Open(Path.Combine(FileManager.LoggingPath, "MonsterInfo.log"), FileMode.Append, FileAccess.Write, FileShare.Read);

            using (var logWriter = new StreamWriter(logStream))
            {
                logWriter.WriteLine(meta.ToExportString());
            }

            logStream.Close();
        }

        /// <summary>
        /// Static actor information object; contains the bits we use from ActorInfo, MonsterInfo and a few extras.
        /// </summary>
        public class ActorMeta
        {
            public ActorMeta() {}

            public ActorMeta(CacheBase cacheBase) 
                : this(cacheBase.Object, cacheBase.Source, cacheBase.ActorSNO, cacheBase.ActorType) { }

            public ActorMeta(DiaObject diaObject, ACD acd, int actorSNO, ActorType actorType)
            {
                _isValid = false;
                _isPartial = false;

                if (diaObject == null || acd == null)
                    return;

                var actorInfo = acd.ActorInfo;
                var monsterInfo = acd.MonsterInfo;
                var unit = diaObject as DiaUnit;
                var gizmo = diaObject as DiaGizmo;

                ActorSNO = actorSNO;
                ActorType = actorType;

                try
                {
                    _isValid = acd.IsValid && !acd.IsDisposed && actorInfo != null && actorInfo.IsValid && !actorInfo.IsDisposed && (actorInfo.SNOMonster == -1 || unit != null && unit.IsValid && monsterInfo.IsValid && !monsterInfo.IsDisposed);
                }
                catch (Exception) { }

                try
                {
                    _isPartial = (gizmo != null && !gizmo.IsValid) || (unit != null && !unit.IsValid);
                }
                catch (Exception) { }

                if (!_isValid)
                    return;

                if (acd.IsValid && !acd.IsDisposed)
                {                    
                    InternalName = Trinity.NameNumberTrimRegex.Replace(acd.Name, "");
                }

                if (actorInfo != null && actorInfo.IsValid && !actorInfo.IsDisposed)
                {
                    MonsterSNO = actorInfo.SNOMonster;
                    Radius = acd.ActorInfo.Sphere.Radius;
                    PhysMeshSNO = actorInfo.SNOPhysMesh;
                    ApperanceSNO = actorInfo.SNOApperance;
                    AnimSetSNO = actorInfo.SNOAnimSet;
                    IsMerchant = actorInfo.IsMerchant;
                }

                if (MonsterSNO != -1 && unit != null && unit.IsValid && monsterInfo.IsValid && !monsterInfo.IsDisposed)
                {
                    IsMonster = true;
                    MonsterType = monsterInfo.MonsterType;
                    MonsterRace = monsterInfo.MonsterRace;
                    MonsterSize = monsterInfo.MonsterSize;
                }

                if (unit != null && unit.IsValid)
                {
                    IsUnit = true;
                    GizmoType = GizmoType.None;
                    try { IsHostile = unit.IsHostile; }
                    catch (Exception) { }
                    try { IsNPC = unit.IsNPC; }
                    catch (Exception) { }
                    try { PetType = unit.PetType; }
                    catch (Exception) { }
                    try { IsSalvageShortcut = unit.IsSalvageShortcut; }
                    catch (Exception) { }
                    try { HirelingType = unit.HirelingType; }
                    catch (Exception) { }
                    try { IsHelper = unit.IsHelper; }
                    catch (Exception) { }
                    try { IsDefaultHidden = unit.IsHidden; }
                    catch (Exception) { }
                    try { IsQuestGiver = unit.IsQuestGiver; }
                    catch (Exception) { }
                    try { IsSummoned = unit.Summoner != null || unit.SummonedBySNO != -1 || unit.SummonedByACDId != -1; }
                    catch (Exception) { }
                    try { IsSummoner = acd.Name.ToLower().Contains("summoner") || CacheManager.Units.Any(u => u.SummonedByACDId == acd.DynamicId); }
                    catch (Exception) { }
                }

                if (gizmo != null && gizmo.IsValid)
                {
                    IsGizmo = true;
                    try { GizmoType = acd.GizmoType; }
                    catch (Exception) { }
                    try { GizmoIsBarracade = gizmo.IsBarricade; }
                    catch (Exception) { }
                    try { GizmoIsDestructible = gizmo.IsDestructibleObject; }
                    catch (Exception) { }
                    try { GizmoIsDisabledByScript = gizmo.IsGizmoDisabledByScript; }
                    catch (Exception) { }
                    try { GizmoIsPortal = gizmo.IsPortal; }
                    catch (Exception) { }
                    try { GizmoIsTownPortal = gizmo.IsTownPortal; }
                    catch (Exception) { }
                    try { GizmoDefaultCharges = gizmo.GizmoCharges; }
                    catch (Exception) { }
                    try { GizmoDefaultState = gizmo.GizmoState; }
                    catch (Exception) { }
                    try { GizmoGrantsNoXp = gizmo.GrantsNoXp; }
                    catch (Exception) { }
                    try { GizmoDropNoLoot = gizmo.DropsNoLoot; }
                    catch (Exception) { }
                    try { GizmoIsOperatable = gizmo.Operatable; }
                    catch (Exception) { }
                }                
            }

            public int ActorSNO = -1;
            public int MonsterSNO = -1;
            public string InternalName = string.Empty;
            public ActorType ActorType = ActorType.Invalid;
            public double Radius = 5;
            public MonsterType MonsterType = MonsterType.None;
            public MonsterRace MonsterRace = MonsterRace.Unknown;
            public MonsterSize MonsterSize = MonsterSize.Unknown;
            public GizmoType GizmoType = GizmoType.None;
            public bool IsSummoner;
            public bool IsHostile;
            public bool IsNPC;
            public int PhysMeshSNO = -1;
            public int ApperanceSNO = -1;
            public int AnimSetSNO = -1;
            public bool IsMerchant;
            public bool IsBarracade;
            public bool GizmoIsTownPortal;
            public bool GizmoIsPortal;
            public bool GizmoIsDisabledByScript;
            public bool GizmoIsDestructible;
            public int GizmoGrantsNoXp = -1;
            public int GizmoDefaultCharges = -1;
            public int GizmoDefaultState = -1;
            public int GizmoDropNoLoot = -1;
            public bool GizmoIsOperatable;
            public bool GizmoIsBarracade;
            public int PetType = -1;
            public bool IsSummoned;
            public bool IsSalvageShortcut;
            public HirelingType HirelingType = HirelingType.None;
            public bool IsHelper;
            public bool IsDefaultHidden;
            public bool IsQuestGiver;
            public bool IsGizmo;
            public bool IsUnit;
            public bool IsMonster;

            private bool _isPartial;
            private bool _isValid;
            private static Type _type;
            private static List<FieldInfo> _fields;

            /// <summary>
            /// Constructor used by ReferenceActorMeta collection initializer records.
            /// This imports a previously exported set of data.
            /// </summary>
            public ActorMeta(int actorSNO, int monsterSNO, string internalName, int actorType, double radius, int monsterType, int monsterRace, int monsterSize,
                int gizmoType, bool isSummoner, bool isHostile, bool isNPC, int physMeshSNO, int apperanceSNO, int animSetSNO, bool isMerchant, bool isBarracade,
                bool gizmoIsTownPortal, bool gizmoIsPortal, bool gizmoIsDisabledByScript, bool gizmoIsDestructible, int gizmoGrantsNoXp, int gizmoDefaultCharges,
                int gizmoDefaultState, int gizmoDropNoLoot, bool gizmoIsOperatable, bool gizmoIsBarracade, int petType, bool isSummoned, bool isSalvageShortcut,
                int hirelingType, bool isHelper, bool isDefaultHidden, bool isQuestGiver, bool isGizmo, bool isUnit, bool isMonster)
            {
                ActorSNO = actorSNO;
                MonsterSNO = monsterSNO;
                InternalName = internalName;
                ActorType = (ActorType)actorType;
                Radius = radius;
                MonsterType = (MonsterType)monsterType;
                MonsterRace = (MonsterRace)monsterRace;
                MonsterSize = (MonsterSize)monsterSize;
                GizmoType = (GizmoType)gizmoType;
                IsSummoner = isSummoner;
                IsHostile = isHostile;
                IsNPC = isNPC;
                PhysMeshSNO = physMeshSNO;
                ApperanceSNO = apperanceSNO;
                AnimSetSNO = animSetSNO;
                IsMerchant = isMerchant;
                IsBarracade = isBarracade;
                GizmoIsTownPortal = gizmoIsTownPortal;
                GizmoIsPortal = gizmoIsPortal;
                GizmoIsDisabledByScript = gizmoIsDisabledByScript;
                GizmoIsDestructible = gizmoIsDestructible;
                GizmoGrantsNoXp = gizmoGrantsNoXp;
                GizmoDefaultCharges = gizmoDefaultCharges;
                GizmoDefaultState = gizmoDefaultState;
                GizmoDropNoLoot = gizmoDropNoLoot;
                GizmoIsOperatable = gizmoIsOperatable;
                GizmoIsBarracade = gizmoIsBarracade;
                PetType = petType;
                IsSummoned = isSummoned;
                IsSalvageShortcut = isSalvageShortcut;
                HirelingType = (HirelingType) hirelingType;
                IsHelper = isHelper;
                IsDefaultHidden = isDefaultHidden;
                IsQuestGiver = isQuestGiver;
                IsGizmo = isGizmo;
                IsUnit = isUnit;
                IsMonster = isMonster;

                _isPartial = false;
                _isValid = true;
            }

            public bool IsPartial
            {
                get { return _isPartial; }
            }

            public bool IsValid
            {
                get { return _isValid; }
            }

            private static Type Type
            {
                get { return _type ?? (_type = typeof(ActorMeta)); }
            }

            private static List<FieldInfo> Fields
            {
                get { return _fields ?? (_fields = Type.GetFields(BindingFlags.Public | BindingFlags.Instance).ToList()); }
            }

            /// <summary>
            /// Creates a string in the format for ReferenceActorMeta Dictionary
            /// todo: 
            /// Add a way to update existing records in the event we add/remove/change ActorMeta fields, 
            /// you would have to loop through ReferenceActorMeta and re-export each one in the new format. 
            /// Then update the constructor (which doesn't use reflection for performance reasons)
            /// </summary>
            public string ToExportString()
            {
                string output = "";

                var fieldCount = Fields.Count;
                var fi = 0;

                foreach (var field in Fields)
                {
                    fi++;
                    var val = field.GetValue(this);
                    string exportVal;

                    if (field.FieldType.IsEnum)
                    {
                        exportVal = Convert.ToInt32(val).ToString();
                    }
                    else if (field.FieldType == typeof (string))
                    {
                        exportVal = string.Format("\"{0}\"", val);
                    }
                    else if (field.FieldType == typeof(double))
                    {
                        exportVal = string.Format("{0:0.00}", val);
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        exportVal = val.ToString().ToLower();
                    }
                    else
                    {
                        exportVal = val.ToString();                        
                    }
                    output += exportVal + (fieldCount != fi ? "," : string.Empty);
                }
                return string.Format("{{ {0}, new ActorMeta ( {1} ) }},", ActorSNO, output);

            }
        }

    }
}
