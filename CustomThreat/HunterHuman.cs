using MeteorIncidentGer;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncident
{
    public class HunterHuman
    {
        static Thread _ThreadOB = null;
        //public static void StartHunterHumanTask()  //测试样例2 - 多线程刷新动物发狂事件
        //{
        //    if (_ThreadOB != null) return;
        //    //猎杀人类事件，定时触发
        //    _ThreadOB = new Thread(() =>
        //    {
        //        while (true)
        //        {
        //            try
        //            {
        //                if (HotseatGameComponent.TakeDownCount19 >= 0) HotseatGameComponent.TakeDownCount19--;

        //                //多线程猎杀人类必须在游戏处于非暂停状态下使用
        //                if (HotseatGameComponent.RG.Next(1, 10000) < 22 && HotseatGameComponent.TakeDownCount19 <= 0 && Find.TickManager.CurTimeSpeed != 0)
        //                {
        //                    HotseatGameComponent.TakeDownCount19 = HotseatGameComponent.RG.Next(22, 164);
        //                    //猎杀人类
        //                    var ThePoint = (float)(new IntRange(1, (int)(Find.RandomPlayerHomeMap.wealthWatcher.WealthTotal / 500)).RandomInRange);
        //                    if (Find.CurrentMap != null && ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(ThePoint, Find.CurrentMap.Tile, out var pawnKindDef) &&
        //                        RCellFinder.TryFindRandomPawnEntryCell(out var intVec, Find.CurrentMap, CellFinder.EdgeRoadChance_Animal, false, null))
        //                    {
        //                        //寻找猎杀人类的事件类
        //                        var allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName == "ManhunterPack");
        //                        if (allDefsListForReading?.Count() != 0)
        //                        {
        //                            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, Find.CurrentMap);
        //                            incidentParms.pawnKind = pawnKindDef;
        //                            incidentParms.questTag = "GerAnimal";
        //                            incidentParms.points = ThePoint;
        //                            incidentParms.target = Find.CurrentMap;
        //                            incidentParms.pawnCount = Find.CurrentMap.mapPawns.FreeColonists.Count;
        //                            allDefsListForReading.FirstOrDefault().Worker.TryExecute(incidentParms);
        //                        }
        //                    }
        //                }
        //            }
        //            catch { }
        //            finally
        //            {
        //                Thread.Sleep(1000);
        //            }
        //        }
        //    });
        //    _ThreadOB.IsBackground = true;
        //    _ThreadOB.Start();
        //}
    }

    public class CaravansHelper
    {
        public static (int TheTile, bool IsFindout) GetThePlayerCaravansTite()
        {
            List<Caravan> caravans = Find.WorldObjects.Caravans;
            for (int j = 0; j < caravans.Count; j++)
            {
                if (caravans[j].IsPlayerControlled)
                {
                    return (caravans[j].Tile, true);
                    //TileFinder.tmpPlayerTiles.Add(caravans[j].Tile);
                }
            }

            return (-100, false);
        }

        /// <summary>
        /// 寻找世界中玩家远行队
        /// </summary>
        public static Caravan FirstPlayerCaravan()=>Find.WorldObjects.Caravans.FirstOrDefault();
    }

    public static class CaravansHelperEx 
    {
        /// <summary>
        /// 在商队周围随机寻找一个随机的地图点编号
        /// </summary>
        public static int GetTheNearbyTite(this (int TheTile, bool IsFindout) Data) 
        {
            if (!Data.IsFindout) return -100;
            var RR = TileFinder.TryFindPassableTileWithTraversalDistance(Data.TheTile, 1, 2, out var TheResult);

            //如果是不可逾越地形，掠过
            if (Find.WorldGrid[TheResult].hilliness == Hilliness.Mountainous) return -100;
            if (RR) return TheResult; else return -100;
        }
    }

    /// <summary>
    /// 狂暴野人
    /// </summary>
    public static class TheManHunterWildMan 
    {
        public static bool IsCreateJingYing = false;

        public static bool TryExecuteWorker(Map TheMap)
        {
            Map map = TheMap;
            IntVec3 loc;
            if (!TryFindEntryCell(map, out loc))
            {
                return false;
            }
            Faction faction;
            if (!TryFindFormerFaction(out faction))
            {
                return false;
            }

            if (!IsCreateJingYing)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.WildMan, faction);

                var Skill8XiaoCount = pawn.skills.skills.Where(XX => XX.levelInt < 8);
                //统计随机出来的野人技能小于8的个数
                if (Skill8XiaoCount.Count() >= 5)
                {
                    //8次机会修改技能等级
                    for (int loopa = 0; loopa < 5; loopa++)
                    {
                        var TheSkill = Skill8XiaoCount.RandomElement();
                        if (TheSkill != null)
                        {
                            TheSkill.levelInt = Rand.Range(1, 16);
                            if (TheSkill.passion != Passion.Major)
                            {
                                //修改技能为双火
                                TheSkill.passion = Passion.Major;
                            }
                        }
                    }
                }

                pawn.SetFaction(null, null);
                GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
                //永久猎杀人类
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, forceWake: true);
                Find.LetterStack.ReceiveLetter("狂暴野人", "一个狂暴野人出现，他拥有极强属性", LetterDefOf.ThreatBig);
                return true;
            }
            else 
            {
                IsCreateJingYing = false;
                //当前地图为空的情况，玩家正在远行什么的
                if (TheMap == null) return true;

                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.WildMan, faction);

                var Skill8XiaoCount = pawn.skills.skills.Where(XX => XX.levelInt < 8);
                //统计随机出来的野人技能小于8的个数
                if (Skill8XiaoCount.Count() >= 5)
                {
                    //8次机会修改技能等级
                    for (int loopa = 0; loopa < 5; loopa++)
                    {
                        var TheSkill = Skill8XiaoCount.RandomElement();
                        if (TheSkill != null)
                        {
                            TheSkill.levelInt = Rand.Range(1, 16);
                            if (TheSkill.passion != Passion.Major)
                            {
                                //修改技能为双火
                                TheSkill.passion = Passion.Major;
                            }
                        }
                    }
                }

                pawn.SetFaction(Faction.OfPlayer, null);
                GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
                //永久猎杀人类
                //pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, forceWake: true);
                Find.LetterStack.ReceiveLetter("精英加入(野人)", "一个狂暴野人出现，他拥有极强属性", LetterDefOf.PositiveEvent);
                return true;
            }
        }

        private static bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out cell);
        }

        private static bool TryFindFormerFaction(out Faction formerFaction)
        {
            return Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out formerFaction, false, true, TechLevel.Undefined, false);
        }

        /// <summary>
        /// 立即在地图上刷新一个猎杀人类的野人
        /// </summary>
        public static bool ManHunterWildMan(this Map TheMap) 
        {
            return TryExecuteWorker(TheMap);
        }

        public static bool ManHunterPlayerMan(this Map TheMap)
        {
            IsCreateJingYing = true;
            return TryExecuteWorker(TheMap);
        }
    }
}
