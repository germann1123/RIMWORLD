using MeteorIncidentGer;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncident
{
    public class TheGlobalInfo
    {
        public static bool IsEmemyForceDefenc = false;
        public static int NumberOfEmemyDefence = 0;
        public static int ChongFuJiCount = 0;

        /// <summary>
        /// 地图中是否会刷新拿枪的敌人
        /// </summary>
        public static bool IsEmemyHasGun = false;

        public static bool IsFriendlyGunSoiler = false;
        /// <summary>
        /// 敌方主力部队
        /// </summary>
        public static bool IsEmemyBigForce = false;

        public static bool IsYiliao = false;

        /// <summary>
        /// 部落偷袭时刷新的小人列表
        /// </summary>
        public static List<Pawn> _BuLuoTouXiGroup = new List<Pawn>();

        public static int MetorRandomCount { get; set; } = 0;

        /// <summary>
        /// 指定一个土匪营地是否处于资源匮乏状态
        /// </summary>
        public static bool IsNoResouce = false;

        public static bool IsMoreResource = false;

        /// <summary>
        /// 是否出现存放化和燃料的房间
        /// </summary>
        public static bool IsStoneChemfuelRoom = false;
        public static bool IsJieLueZhe = false;

        /// <summary>
        /// 刷新土匪地图的时候是否在玩家随机营地生成随机的部落士兵
        /// </summary>
        public static bool IsBuLuoYinCangBing = false;

        /// <summary>
        /// 是否在玩家进图的时候追加愉快旅行的BUFF
        /// </summary>
        public static bool IsYuKuaiLvXing = false;
        /// <summary>
        /// 当前地图是否贮存食物房间
        /// </summary>
        public static bool IsGanRouBing = false;

        /// <summary>
        /// 随机化各种地图状态
        /// </summary>
        public static void DoTheRandom() 
        {
            IsMoreResource = false;
            IsNoResouce = false;
            IsStoneChemfuelRoom = false;
            IsJieLueZhe = false;
            IsBuLuoYinCangBing = false;
            IsYuKuaiLvXing = false;
            IsEmemyHasGun = false;
            IsFriendlyGunSoiler = false;
            IsEmemyBigForce = false;
            IsGanRouBing = false;
            if (HotseatGameComponent.RG.Next(1, 2000) < 8) IsMoreResource = true;
            if (HotseatGameComponent.RG.Next(1, 2000) < 8 && !IsMoreResource) IsNoResouce = true;
            if (HotseatGameComponent.RG.Next(1, 2000) < 8) IsStoneChemfuelRoom = true;
            if (HotseatGameComponent.RG.Next(1, 2000) < 18) IsJieLueZhe = true;
            if (HotseatGameComponent.RG.Next(1, 2000) < 18) IsBuLuoYinCangBing = true;
            if (HotseatGameComponent.RG.Next(1, 2000) < 38) IsYuKuaiLvXing = true;

            if (HotseatGameComponent.RG.Next(1, 2000) < 238) IsEmemyHasGun = true;
            //敌方主力部队
            if (HotseatGameComponent.RG.Next(1, 2000) < 138) IsEmemyBigForce = true;
            //出现贮藏干肉饼的房间
            if (HotseatGameComponent.RG.Next(1, 2000) < 138) IsGanRouBing = true;
            if (HotseatGameComponent.RG.Next(1, 2000) < 238 && HotseatGameComponent.IsHasFriend) IsFriendlyGunSoiler=true;
        }

        public static string GetItemInfos() 
        {
            StringBuilder SB = new StringBuilder();
            ThingSetMakerDefOf.ResourcePod.root.Generate().ForEach(XX=>SB.Append($"{ XX.def.defName }<aa>{XX.stackCount}<item>"));
            return SB.ToString();
        }

        public static List<string> GetItemInfos1()
        {
            List<string> RR = new List<string>();
            ThingSetMakerDefOf.ResourcePod.root.Generate().ForEach(XX => RR.Add(XX.def.defName));
            return RR;
        }

        public static (string SteamPersonaName, string ClientID, int DaysPassed, float WealthTotal, 
            int FreeColonistsNoDowned, int FreeColonistsInBed,int CurrentMapId,bool IsBeiLuTeOpen, bool IsFeiBiOpen,string STName,string DianStates) PlayerState;
        public static object PlayerStateOB = new object();
    }

    public static class PlayerStateHelper 
    {
        public static string GetPlayerStr(this (string SteamPersonaName, string ClientID, 
            int DaysPassed, float WealthTotal, int FreeColonistsNoDowned, int FreeColonistsInBed, 
            int CurrentMapId, bool IsBeiLuTeOpen, bool IsFeiBiOpen,string STName, string DianStates) Data) 
        {
            var ThePlayerData = $"{Data.SteamPersonaName}##" +
            $"{Data.ClientID}##" +
            $"{ Data.DaysPassed }##" +
            $"{ Data.WealthTotal}##" +
            $"{Data.FreeColonistsNoDowned}##" +
            $"{Data.FreeColonistsInBed}##{ Data.CurrentMapId }##{Data.IsBeiLuTeOpen}##{Data.IsFeiBiOpen}##{Data.STName}##{Data.DianStates}##";

            return ThePlayerData;
        }
    }

    public static class MapHelper 
    {
        /// <summary>
        /// 燃烧火焰 - 失败
        /// </summary>
        //private static void StartRandomFireInRadius(Map TheMap,float radius)
        //{
        //    IntVec3 intVec = TheMap.Center + IntVec3Utility.ToIntVec3(Rand.InsideUnitCircleVec3 * radius);
        //    bool flag = !GenGrid.InBounds(intVec, TheMap);
        //    if (!flag)
        //    {
        //        FireUtility.TryStartFireIn(intVec, TheMap, Rand.Range(0.1f, 0.925f));
        //    }
        //}

        public static void CreateCustomMapDefence(this Map TheMap)
        {
            var RandomPoint = TheMap.GetRandomCell();
            CellRect cellRect = CellRect.CenteredOn(TheMap.Center, 40, 40).ClipInsideMap(TheMap);

            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.hivesCount = 3;
            resolveParams.rect = cellRect;

            BaseGen.globalSettings.map = TheMap;
            //BaseGen.symbolStack.Push("hives", resolveParams, null);
            BaseGen.symbolStack.Push("batteryRoom", resolveParams, null);
            BaseGen.Generate();
        }

        public static void CreateCustomMapDefence1(this Map TheMap)
        {
            var RandomPoint = TheMap.GetRandomCell();
            CellRect cellRect = CellRect.CenteredOn(TheMap.Center, 40, 40).ClipInsideMap(TheMap);

            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.edgeDefenseWidth = 30;
            resolveParams.settlementDontGeneratePawns = true;
            resolveParams.settlementPawnGroupSeed = 12;
            resolveParams.rect = cellRect;

            BaseGen.globalSettings.map = TheMap;
            //BaseGen.symbolStack.Push("hives", resolveParams, null);
            BaseGen.symbolStack.Push("settlement", resolveParams, null);
            BaseGen.Generate();
        }

        public static void CreateCustomMapDefence2(this Map TheMap)
        {
            var RandomPoint = TheMap.GetRandomCell();
            CellRect cellRect = CellRect.CenteredOn(RandomPoint, 20, 20).ClipInsideMap(TheMap);

            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.pawnGroupKindDef = PawnGroupKindDefOf.Settlement;
            resolveParams.rect = cellRect;

            BaseGen.globalSettings.map = TheMap;
            //BaseGen.symbolStack.Push("hives", resolveParams, null);
            BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);
            BaseGen.Generate();
        }

        public static void JieLueZheZhanJiBuff(this Map TheMap) 
        {
            TheMap.mapPawns.FreeColonists.ForEach(XX3 =>
            {
                //添加BUFF
                XX3.needs.mood.thoughts.memories.
                TryGainMemoryFast(ThoughtDef.Named("GerJielueZhe"));
            });            
        }

        public static void JiangGuoZhang(this Map TheMap)
        {
            bool TryFindRootCell(Map map112, out IntVec3 cell)
            {
                return CellFinderLoose.TryFindRandomNotEdgeCellWith(0, (IntVec3 x) => x.GetRoomOrAdjacent(map112, RegionType.Set_Passable).CellCount >= 20, map112, out cell);
            }

            bool CanSpawnAt(IntVec3 c, Map map1)
            {
                if (!c.Standable(map1) || c.Fogged(map1) || map1.fertilityGrid.FertilityAt(c) < ThingDefOf.Plant_Ambrosia.plant.fertilityMin || !c.GetRoomOrAdjacent(map1, RegionType.Set_Passable).PsychologicallyOutdoors || c.GetEdifice(map1) != null || !PlantUtility.GrowthSeasonNow(c, map1, false))
                {
                    return false;
                }

                return true;
            }

            Map map = TheMap;
            IntVec3 intVec;
            if (!TryFindRootCell(map, out intVec))
            {
                return;
            }

            Find.LetterStack.ReceiveLetter(
                    "植物生长",
                    "一些可采摘的植物在地图上生长",
                    LetterDefOf.PositiveEvent
                    );


            Thing thing = null;
            IntRange CountRange = new IntRange(10, 20);
            int randomInRange = CountRange.RandomInRange;

            //随机刷出一种植物
            List<string> _PlantRamdom = new List<string>
            {
                "Plant_Agave","Plant_Berry",
            };
            var CurrentRamdomP = _PlantRamdom.RandomElement();

            for (int i = 0; i < Rand.Range(8, 52); i++)
            {
                IntVec3 arg_71_0 = intVec;
                Map arg_71_1 = map;
                int arg_71_2 = 6;

                IntVec3 intVec2;
                if (!CellFinder.TryRandomClosewalkCellNear(arg_71_0, arg_71_1, arg_71_2, out intVec2, XX => CanSpawnAt(XX, map)))
                {
                    break;
                }

                //var TriggerEx = intVec2.get

                Plant plant = intVec2.GetPlant(map);
                if (plant != null)
                {
                    plant.Destroy(DestroyMode.Vanish);
                }

                Thing thing2 = GenSpawn.Spawn(ThingDef.Named(CurrentRamdomP), intVec2, map, WipeMode.Vanish);
                if (thing == null)
                {
                    thing = thing2;
                }
            }
            if (thing == null)
            {
                //return false;
            }
            //base.SendStandardLetter(parms, thing, Array.Empty<NamedArgument>());
            //return true;
        }

        //public static void StartRandomFire(this Map TheMap, float radius) => StartRandomFireInRadius(TheMap, radius);
    }
}
