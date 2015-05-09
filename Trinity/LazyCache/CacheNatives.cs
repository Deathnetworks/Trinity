using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Mozilla;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Trinity.Technicals;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Requesting ActorInfo and MonsterInfo from DB is very very slow.
    /// The data that we use (Size/Type/Race) doesn't change over time (except rare situations like jondar) 
    /// and is shared between monsters with the same ActorSNO. So, if we've seen it before the data is pulled from a HashTable.
    /// </summary>
    public class CacheNatives
    {
        /// <summary>
        /// Storage for cached ActorInfo/MonsterInfo
        /// </summary>
        public static Hashtable HashCachedNativesBySNO = new Hashtable();

        /// <summary>
        /// Actors Table contains ActorInfo indexed by ActorSNO
        /// </summary>
        private static SNOTable ActorSNOTable
        {
            get { return ZetaDia.SNO[ClientSNOTable.Actor]; }
        }

        /// <summary>
        /// Monsters Table contains MonsterInfo indexed by SNOMonster
        /// </summary>
        private static SNOTable MonsterSNOTable
        {
            get { return ZetaDia.SNO[ClientSNOTable.Monster]; }
        }

        /// <summary>
        /// Retrieves an ActorProperties object (one is cached for every ActorSNO seen)
        /// </summary>
        public static ActorProperties GetActorProperties(int actorSNO)
        {
            ActorProperties aprops;

            if (HashCachedNativesBySNO.ContainsKey(actorSNO))
            {
                aprops = (ActorProperties) HashCachedNativesBySNO[actorSNO];
            }
            else
            {
                aprops = GetActorPropertiesFromMemory(actorSNO);
                HashCachedNativesBySNO.Add(actorSNO, aprops);
            }

            return aprops;
        }

        /// <summary>
        /// Request new ActorInfo and MonsterInfo data from Memory
        /// </summary>
        public static ActorProperties GetActorPropertiesFromMemory(int actorSNO)
        {
            var aProps = new ActorProperties();

            var actorInfoPtr = ActorSNOTable.GetRecordPtr(actorSNO);
            aProps.CachedActorInfo = ZetaDia.Memory.Read<CachedActorInfo>(actorInfoPtr);
            //ActorSNOTable.PurgeRecordPtr(actorInfoPtr);

            if (aProps.CachedActorInfo.SNOMonster != -1)
            {
                if (MonsterSNOTable.IsValid && !MonsterSNOTable.IsDisposing && !MonsterSNOTable.IsDisposed)
                {
                    var monsterInfoPtr = MonsterSNOTable.GetRecordPtr(aProps.CachedActorInfo.SNOMonster);
                    aProps.CachedMonsterInfo = ZetaDia.Memory.Read<CachedMonsterInfo>(monsterInfoPtr);
                    //MonsterSNOTable.PurgeRecordPtr(monsterInfoPtr);
                }
            }
            
            aProps.IsLoaded = true;

            // There is a bug in DB where some Monsters have invalid MonsterInfo and DB serves them up as valid DiaUnits
            // for these objects all DiaUnit properties throw exceptions, so we need to flag them so they can be avoided.
            if (aProps.CachedMonsterInfo.ActorSNO <= 1)
            {
                aProps.CachedMonsterInfo = new CachedMonsterInfo();
                aProps.IsInvalidMonsterInfo = true;
            }
                
            return aProps;
        }

        /// <summary>
        /// Container for storing actor and monster information
        /// </summary>
        public struct ActorProperties
        {
            public bool IsLoaded;
            public CachedActorInfo CachedActorInfo;
            public CachedMonsterInfo CachedMonsterInfo;
            public bool IsInvalidMonsterInfo;
        }

        /// <summary>
        /// Test Method
        /// </summary>
        public static void RunPerformanceTest()
        {
            var diaUnit = ZetaDia.Actors.GetActorsOfType<DiaUnit>().FirstOrDefault();
            if (diaUnit == null)
                return;

            var trinityUnit = CacheFactory.CreateTypedTrinityObject(diaUnit.CommonData, diaUnit.ActorType, diaUnit.ACDGuid, diaUnit.ActorSNO);
            var monsterType = MonsterType.None;

            var stopwatch3 = Stopwatch.StartNew();
            monsterType = trinityUnit.MonsterInfo.MonsterType;
            stopwatch3.Stop();
            Logger.Log("Trinity Ver. took {0:00.0000}ms. MonsterType={1}", stopwatch3.Elapsed.TotalMilliseconds, monsterType);

            var stopwatch1 = Stopwatch.StartNew();
            monsterType = diaUnit.MonsterInfo.MonsterType;
            stopwatch1.Stop();
            Logger.Log("DB Ver. took {0:00.0000}ms. MonsterType={1}", stopwatch1.Elapsed.TotalMilliseconds, monsterType);
        }

        public struct CachedActorInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            private byte[] au0001;
            public int abu0001;
            public ActorType Type;
            public int au0003;
            public int SNOPhysMesh;
            public AxialCylinder AxialCylinder;
            public Sphere Sphere;
            public AABB Bounds;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] eu0001;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] fu0002;
            public int fu0005;
            public int SNOMonster;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] gu0003;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] gu0004;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] gu0005;
            public Vector3 hu0001;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public UnknownStructB[] astruct0001;
            public int gu0007;
            private int gu0008;
            private int gu000E;
            public float gu0001;
            public float gu0002;
            public float gau0003;
            public UnknownStructC BUnknownStructCstruct0001;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public UnknownStructE[] aasstruct0001;
            private int ua000F;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] u0006;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] u0007;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] u0008;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] u000E;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UnknownStructA
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] u0001;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UnknownStructB
        {
            public UnknownStructA u0012401;
            public int u0412001;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UnknownStructC
        {
            public UnknownStructD u1230345001;
            private int u230001;
            public AxialCylinder u0001231;
            public AABB u0012301;
            public float u1230001;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UnknownStructD
        {
            private int u0123001;
            private int u0101202;
            private int u0123003;
            private int u0123004;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UnknownStructE
        {
            private int u0001;
            private int u0002;
        }

        public struct CachedMonsterInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            private byte[] va111l1;
            public int va11l2;
            public int ActorSNO;
            public int v1al4;
            public MonsterType MonsterType;
            public MonsterRace MonsterRace;
            public MonsterSize MonsterSize;
            public float v1al1;
            public float v1al2;
            public float va1l3;
            public float v11al4;
            public int val7;
            public int val8;
            public int valE;
            public int valF;
            public int u0010;
            public int u0011;
            public int u0012;
            public int u0013;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 157)]
            public int[] val1;
            public int u0014;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] PropertySet1;
            public int u0015;
            public int u0016;
            public int u0017;
            public int u0018;
            public int u0019;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public int[] val2;
            public int u001A;
            public int u001B;
            public int u001C;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] val13;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public int[] val14;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] val15;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] val16;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] val17;
            public int val1D;
            public int val1E;
            public int va1l1F;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 44)]
            public int[] v1al18;
            public int va1l7F;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public int[] va1l1E;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] PropertySet2;
        }

    }
}
