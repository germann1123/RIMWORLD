using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MeteorIncident;
using MeteorIncident.Common;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;
using Verse.Sound;
using WebSocketSharp;

namespace MeteorIncidentGer
{
    public class HotseatGameComponent : GameComponent
    {
        public HotseatGameComponent()
        {
        }

        public HotseatGameComponent(Game game)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //事件时间参数存档
            Scribe_Values.Look(ref TakeDownCount, "TakeDownCount", 0, false);
            Scribe_Values.Look(ref TakeDownCount1, "TakeDownCount1", 0, false);
            Scribe_Values.Look(ref TakeDownCount2, "TakeDownCount2", 0, false);
            Scribe_Values.Look(ref TakeDownCount3, "TakeDownCount3", 0, false);
            Scribe_Values.Look(ref TakeDownCount4, "TakeDownCount4", 0, false);
            Scribe_Values.Look(ref TakeDownCount5, "TakeDownCount5", 0, false);
            Scribe_Values.Look(ref TakeDownCount6, "TakeDownCount6", 0, false);
            Scribe_Values.Look(ref TakeDownCount7, "TakeDownCount7", 0, false);
            Scribe_Values.Look(ref TakeDownCount8, "TakeDownCount8", 0, false);
            Scribe_Values.Look(ref TakeDownCount9, "TakeDownCount9", 0, false);
            Scribe_Values.Look(ref TakeDownCount10, "TakeDownCount10", 0, false);
            Scribe_Values.Look(ref TakeDownCount11, "TakeDownCount11", 0, false);
            Scribe_Values.Look(ref TakeDownCount12, "TakeDownCount12", 0, false);
            Scribe_Values.Look(ref TakeDownCount13, "TakeDownCount13", 0, false);
            Scribe_Values.Look(ref TakeDownCount14, "TakeDownCount14", 0, false);
            Scribe_Values.Look(ref TakeDownCount15, "TakeDownCount15", 0, false);
            Scribe_Values.Look(ref TakeDownCount16, "TakeDownCount16", 0, false);
            Scribe_Values.Look(ref TakeDownCount17, "TakeDownCount17", 0, false);
            //雷暴参数存盘
            Scribe_Values.Look(ref TheFlash.KeepOnCount, "KeepOnCount", 0, false);
            Scribe_Values.Look(ref TheFlash.InitDelay, "InitDelay", 0, false);

            Scribe_Values.Look(ref FindTheKey, "FindTheKey", false, false);
            //生成新的客户端唯一标识符
            Scribe_Values.Look(ref ClientID, "ClientID", Guid.NewGuid().ToString("N"), false);
            //土匪营地的探索次数存盘
            Scribe_Values.Look(ref TuFeiTanCount, "TuFeiTanCount", 0, false);
            Scribe_Values.Look(ref KongTouTanCiShu, "KongTouTanCiShu", 0, false);

            //有毒尘埃抑制
            Scribe_Values.Look(ref WuYouDuChengAi_TimeOut, nameof(WuYouDuChengAi_TimeOut), 0, false);
            //有毒尘埃变量
            Scribe_Values.Look(ref FindTheKeyTo_WuYouDuChengAi, "FindTheKeyTo_WuYouDuChengAi", false, false);

            //缓存小人信息在本地存档
            //Scribe_Collections.Look(ref TheGlobalInfo._BuLuoTouXiGroup, nameof(TheGlobalInfo._BuLuoTouXiGroup), lookMode: LookMode.Deep);
            //存贮伏击点
            //Scribe_Collections.Look(ref _RaidPoint, "_RaidPoint", LookMode.Deep);
        }

        /// <summary>
        /// 土匪营地探索次数
        /// </summary>
        public static int TuFeiTanCount = 0;

        public static int CurrentTick1 { get; set; } = 0;
        /// <summary>
        /// 用于显示神秘空投漂浮文字计时器
        /// </summary>
        public static int CurrentTick2 { get; set; } = 0;
        public static int CurrentTick3 { get; set; } = 0;
        public static bool IsCreateHive = false; //空投箱是否会在最后爆炸

        /// <summary>
        /// 客户端GUID
        /// </summary>
        public static string ClientID = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 指示货仓密钥
        /// </summary>
        public static bool FindTheKey = false;
        /// <summary>
        /// 持有此密钥可以直接解救偷渡客
        /// </summary>
        public static bool FindTheKeyToRescure = false;
        /// <summary>
        /// 神秘空投搜出来的钥匙能直接招募一个随机的友军基地小人
        /// </summary>
        public static bool FindTheKeyToFriendBaseAdd = false;

        /// <summary>
        /// 找到避开有毒尘埃关键技术
        /// </summary>
        public static bool FindTheKeyTo_WuYouDuChengAi = false;
        /// <summary>
        /// 在这个计时器到时之前，不会触发有毒尘埃
        /// </summary>
        public static int WuYouDuChengAi_TimeOut = 0;

        public static bool IsShang = false; //小人会在剧烈爆炸中受重伤

        public static bool TryFindCell(out IntVec3 cell, Map map)
        {
            int maxMineables = ThingSetMaker_Meteorite.MineablesCountRange.max;
            bool flag = false;

            if (Rand.Chance(0.35f))
            {
                // colonist target

                List<Pawn> ar_pawn = new List<Pawn>();
                List<Pawn> ar_pawn_all = map.mapPawns.FreeColonists.ToList<Pawn>();

                if (Rand.Chance(0.7f))
                {
                    // out door target
                    foreach (Pawn p in ar_pawn_all)
                    {
                        Room tmp_room = p.GetRoom(RegionType.Set_Passable);
                        if (tmp_room != null && !tmp_room.PsychologicallyOutdoors)
                        {
                            // indoor pawn

                        }
                        else
                        {
                            // outdoor pawn
                            ar_pawn.Add(p);
                        }

                    }

                    if (ar_pawn.Count == 0)
                    {
                        flag = CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 0, default(IntVec3), -1, true, true, true, true, false, false, null);
                        return flag;
                    }
                }
                else
                {
                    // any target
                    ar_pawn = ar_pawn_all;
                }



                IntVec3 tpoint = ar_pawn[Rand.Range(0, ar_pawn.Count)].Position;

                flag = CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 0, tpoint, 30, true, true, true, true, false, false, null);

            }
            else
            {
                // random area target
                flag = CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 0, default(IntVec3), -1, true, true, true, true, false, false, null);
            }

            return flag;

        }

        public static void CreateFriend(IntVec3 ThePos,bool IsLeaveMapAuto=false,bool IsGiveToPlayer=false)
        {
            var TheSoilers = new List<Pawn>();

            List<PawnKindDef> _TheFriendType = new List<PawnKindDef> 
            {
                PawnKindDefOf.Empire_Fighter_Cataphract,
                PawnKindDefOf.Empire_Fighter_Trooper,
                PawnKindDefOf.Empire_Fighter_Janissary,
            };

            //IntVec3 intVec = CellFinder.RandomClosewalkCellNear(ThePos, Find.CurrentMap, 10, null); Thrumbo

            //在集合中随机挑选一种类型刷新
            Pawn pawn = PawnGenerator.GeneratePawn(RG.Next(1,10000) < 2333 ? _TheFriendType.RandomElement() : PawnKindDefOf.Colonist,
                IsGiveToPlayer ? Faction.OfPlayer : Find.FactionManager.RandomAlliedFaction());
            //刷新给玩家的小人默认不会离开地图
            if (IsLeaveMapAuto && !IsGiveToPlayer) 
            {
                //自动离开地图设定
                TheTimerManager.AddTimer($"RemoveTimer_{Guid.NewGuid().GetHashCode()}", new TimerInfo
                {
                    TTL = RG.Next(10, 40),
                    _TheData = pawn,
                    _OnTimerDown_with_OB = DataSave => 
                    {
                        //到时间直接删除这个PAWN
                        if (DataSave is Pawn ThePawn) ThePawn.Destroy();
                    },
                });
            }

            QuestUtility.AddQuestTag(GenSpawn.Spawn(pawn, ThePos, Find.CurrentMap, 0), "");
            TheSoilers.Add(pawn);

            if (!IsGiveToPlayer)
            {
                //只有非玩家单位，才会给予AI设定
                LordJob_DefendPoint lordJob = new LordJob_DefendPoint(ThePos, 5f);
                var TL1123 = LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, Find.CurrentMap, TheSoilers);
            }
        }

        public static List<Pawn> CreateEnemy(IntVec3 ThePos,Map TheMap,PawnKindDef pawnKindDef,Faction TheFac=null)
        {
            var TheSoilers = new List<Pawn>();
            //IntVec3 intVec = CellFinder.RandomClosewalkCellNear(ThePos, Find.CurrentMap, 10, null); Thrumbo PawnGroupKindDefOf.Settlement
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, TheFac ?? Find.FactionManager.OfMechanoids);

            //非玩家单位才会去下达防守指令
            if (TheFac != Faction.OfPlayer)
            {
                //多少游戏周期后离开地图
                //pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(10000, 12000);
                //pawn.SetFaction(null, null);
                QuestUtility.AddQuestTag(GenSpawn.Spawn(pawn, ThePos, TheMap, 0), "");
                TheSoilers.Add(pawn);

                LordJob_DefendPoint lordJob = new LordJob_DefendPoint(ThePos, 5f);
                var TL1123 = LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, TheMap, TheSoilers);
            }
            else 
            {
                //创建玩家专属小人
                GenSpawn.Spawn(pawn, ThePos, TheMap, 0);
                TheSoilers.Add(pawn);
            }

            return TheSoilers;
        }

        /// <summary>
        /// 在指定点刷新一个小人
        /// </summary>
        static void CreateEnemyByPawn(IntVec3 ThePos, Map TheMap, Pawn ThePawn)
        {
            var TheSoilers = new List<Pawn>();
            QuestUtility.AddQuestTag(GenSpawn.Spawn(ThePawn, ThePos, TheMap, 0), "");
            TheSoilers.Add(ThePawn);
            LordJob_DefendPoint lordJob = new LordJob_DefendPoint(ThePos, 5f);
            var TL1123 = LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, TheMap, TheSoilers);
        }

        /// <summary>
        /// 神秘房间虫族伏击
        /// </summary>
        public static void ChongGong()
        {
            bool ChongCreate = false;
            CurrentTick3 = Find.TickManager.TicksGame;

            _RaidPoint.Keys.ToList().ForEach(XX => 
            {
                if (XX.uniqueID == Find.CurrentMap.uniqueID) 
                {
                    _RaidPoint[XX].ToList().ForEach(XX12 =>
                    {
                        Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX3 =>
                        {
                            //判断附近是否有玩家控制的殖民者
                            if (XX3.Position.DistanceTo(XX12) < 6 && !ChongCreate)
                            {
                                ChongCreate = true;
                                //移除地图突袭点
                                _RaidPoint[XX].Remove(XX12);
                                if (RG.Next(1, 1000) < 500)
                                {
                                    Find.LetterStack.ReceiveLetter("伏击", "埋伏在搜刮点附近的虫族开始进攻了", LetterDefOf.ThreatBig);
                                    //随机创建一些虫子
                                    for (int i = 0; i < Rand.Range(2, 4); i++)
                                    {
                                        IntVec3 intVec = CellFinder.RandomClosewalkCellNear(XX12, Find.CurrentMap, 20, XX4 => XX4.Walkable(Find.CurrentMap));
                                        if (intVec != null)
                                        {
                                            Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, Faction.OfInsects);
                                            if (pawn != null)
                                            {
                                                GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, 0);
                                                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                                            }
                                            //IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                                        }
                                    }
                                }
                                else
                                {
                                    //Find.LetterStack.ReceiveLetter("搜刮点", "你发现了一个隐藏的物资搜刮点", LetterDefOf.PositiveEvent);

                                    Find.LetterStack.ReceiveLetter("成功取悦邪神", "这就是死神给与你的最终宿命", LetterDefOf.PositiveEvent);
                                }
                            }
                        });
                    });
                }
            });
        }

        public static void DoSpawnKongTou()
        {
            var TheList = Find.CurrentMap.listerThings.AllThings.Where(XX => XX.questTags != null && XX.questTags.Contains("Boomer"));
            //不重复刷新空投箱，必须等待前一个爆炸后
            if (TheList.Count() != 0) return;
            //int duration = Mathf.RoundToInt((60000f * Rand.Range(0.1f, 0.8f)));
            //GameCondition_GerTheBoom.IsCreateHive = false; //允许刷新一次触发试虫子
            //GameCondition_GerTheBoom.IsShang = false; //允许给予一次重伤机会
            //GameCondition_GerTheBoom conditionMeteor2 = (GameCondition_GerTheBoom)GameConditionMaker.MakeCondition(GameConditionDef.Named("AirDropKABOOM"), duration);
            ////GameCondition_GerTheBoom conditionMeteor2 = (GameCondition_GerTheBoom)GameConditionMaker
            ////.MakeCondition(new GameConditionDef { endMessage = "距离空袭结束", label = "距离空袭结束", description = "叛军的军事行动将会给这片区域造成破坏，那些该死的叛徒！", conditionClass = typeof(GameCondition_GerTheBoom) }, duration);
            //Find.CurrentMap.gameConditionManager.RegisterCondition(conditionMeteor2);

            //在未来一个随机时间爆炸
            TheTimerManager.AddTimer(("ShenMiKongTou_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
            {
                TTL = Rand.Range(16, 45),
                _OnTimerDown = new System.Action(() =>
                {
                    IsTimerDown = true;
                    //如果没有指定物件，则不执行
                    if (Find.CurrentMap.listerThings.AllThings.Count(XX => XX.questTags != null && XX.questTags.Contains("Boomer")) == 0) return;
                    //所有神秘空投爆炸
                    Find.CurrentMap.listerThings.AllThings.Where(XX => XX.questTags != null && XX.questTags.Contains("Boomer")).ToList().ForEach(XX1 =>
                    {
                        if (XX1.Destroyed) return;
                        //指定物品是否被破坏
                        Find.LetterStack.ReceiveLetter("空投箱爆炸","由于没有被技术干预，残骸内部的自动引爆装置爆炸",LetterDefOf.ThreatSmall, new TargetInfo(XX1.Position, Find.CurrentMap, false));
                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, Rand.Range(0.5f, 5.5f), DamageDefOf.Flame, null, 8, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);
                        XX1.Destroy();
                    });
                })
            });

            IsTimerDown = false;

            
            TryFindCell(out var TheCell, Find.CurrentMap);
            Find.LetterStack.ReceiveLetter("神秘空投被发现", "一个神秘残骸在附近被发现，殖民者能自动触发空投箱内部程序", LetterDefOf.NegativeEvent, new TargetInfo(TheCell, Find.CurrentMap, false));

            SoundDefOf.ShipTakeoff.PlayOneShot(new TargetInfo(TheCell, Find.CurrentMap, false));
            var rr = ThingMaker.MakeThing(ThingDef.Named("ShipChunk_Ger"));
            var TheThing12 = GenSpawn.Spawn(rr, TheCell, Find.CurrentMap, 0);
            TheThing12.questTags = new List<string>();
            TheThing12.questTags.Add("Boomer");
        }

        public void TheResoucePodCrashed()
        {
            Map map = Find.CurrentMap;
            List<Thing> things = ThingSetMakerDefOf.ResourcePod.root.Generate();
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true, true);

            Find.LetterStack.ReceiveLetter(
                        "补给仓坠毁",
                        "a Airdrop has be founded in this map. you can serch anything if you take a Colonists near the Airdrop",
                        LetterDefOf.PositiveEvent
                        );

            //base.SendStandardLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent, parms, new TargetInfo(intVec, map, false), Array.Empty<NamedArgument>());
        }

        public void TheResoucePodCrashedMore()
        {
            for (int loopa = 0; loopa < 3; loopa++)
            {
                Map map = Find.CurrentMap;
                List<Thing> things = ThingSetMakerDefOf.ResourcePod.root.Generate();
                IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
                DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true, true);
            }

            Find.LetterStack.ReceiveLetter(
                            "大量货仓坠毁",
                            "a Airdrop has be founded in this map. you can serch anything if you take a Colonists near the Airdrop",
                            LetterDefOf.PositiveEvent
                            );

            //base.SendStandardLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent, parms, new TargetInfo(intVec, map, false), Array.Empty<NamedArgument>());
        }

        public static void DoZergAttack()
        {
            IEnumerable<PawnKindDef> enumerable = new List<PawnKindDef>
            {
                PawnKindDefOf.Megascarab,
                PawnKindDefOf.Spelopede,
                PawnKindDefOf.Megaspider
            };
            PawnKindDef pawnKindDef;

            Find.LetterStack.ReceiveLetter(
                "虫族进攻",
                "虫族顺着人类的气味一路追杀，它们不会放过任何一个人类",
                LetterDefOf.ThreatBig
                );

            bool flag = GenCollection.TryRandomElement<PawnKindDef>(enumerable, out var pawnKindDefO);
            if (RCellFinder.TryFindRandomPawnEntryCell(out var spawnCenter112, Find.CurrentMap, CellFinder.EdgeRoadChance_Animal, false, null))
            {
                for (int i = 0; i < Find.CurrentMap.mapPawns.FreeColonists.Count; i++)
                {
                    //Meteor_Object.TryFindCell(out var spawnCenter, Find.CurrentMap);
                    //List<Pawn> list = IncidentWorker_GerFaction.GenerateInsects(Find.CurrentMap.Tile, 0f);
                    //list.ForEach(XX => GenSpawn.Spawn(XX, spawnCenter, Find.CurrentMap, WipeMode.Vanish));
                    IntVec3 intVec = CellFinder.RandomClosewalkCellNear(spawnCenter112, Find.CurrentMap, 10, null);
                    Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDefO, Find.FactionManager.OfInsects);
                    GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, 0);
                    IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                }
            }

            //LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, 2);
            //LessonAutoActivator
        }

        public static void DoZergAttackWithGroupCount(int Count = 0)
        {
            IEnumerable<PawnKindDef> enumerable = new List<PawnKindDef>
            {
                PawnKindDefOf.Megascarab,
                PawnKindDefOf.Spelopede,
                PawnKindDefOf.Megaspider
            };
            PawnKindDef pawnKindDef;

            Find.LetterStack.ReceiveLetter(
                "虫族进攻(来自其他星系)",
                "虫族顺着人类的气味一路追杀，它们不会放过任何一个人类",
                LetterDefOf.ThreatBig
                );
            var TriggerMap = Find.RandomPlayerHomeMap;
            if (TriggerMap == null) return;

            if (Count != 0)
            {
                for (int loopa = 0; loopa < Count; loopa++)
                {
                    bool flag1 = GenCollection.TryRandomElement<PawnKindDef>(enumerable, out var pawnKindDefO1);
                    if (RCellFinder.TryFindRandomPawnEntryCell(out var spawnCenter1121, Find.CurrentMap, CellFinder.EdgeRoadChance_Animal, false, null))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            //Meteor_Object.TryFindCell(out var spawnCenter, Find.CurrentMap);
                            //List<Pawn> list = IncidentWorker_GerFaction.GenerateInsects(Find.CurrentMap.Tile, 0f);
                            //list.ForEach(XX => GenSpawn.Spawn(XX, spawnCenter, Find.CurrentMap, WipeMode.Vanish));
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(spawnCenter1121, TriggerMap, 10, null);
                            Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDefO1, Find.FactionManager.OfInsects);
                            GenSpawn.Spawn(pawn, intVec, TriggerMap, 0);
                            IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                        }
                    }
                }
                return;
            }

            bool flag = GenCollection.TryRandomElement<PawnKindDef>(enumerable, out var pawnKindDefO);
            if (RCellFinder.TryFindRandomPawnEntryCell(out var spawnCenter112, TriggerMap, CellFinder.EdgeRoadChance_Animal, false, null))
            {
                for (int i = 0; i < TriggerMap.mapPawns.FreeColonists.Count; i++)
                {
                    //Meteor_Object.TryFindCell(out var spawnCenter, Find.CurrentMap);
                    //List<Pawn> list = IncidentWorker_GerFaction.GenerateInsects(Find.CurrentMap.Tile, 0f);
                    //list.ForEach(XX => GenSpawn.Spawn(XX, spawnCenter, Find.CurrentMap, WipeMode.Vanish));
                    IntVec3 intVec = CellFinder.RandomClosewalkCellNear(spawnCenter112, TriggerMap, 10, null);
                    Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDefO, Find.FactionManager.OfInsects);
                    GenSpawn.Spawn(pawn, intVec, TriggerMap, 0);
                    IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                }
            }

            //LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, 2);
            //LessonAutoActivator
        }

        public int TheWaveCount = 0;
        public void DoZergAttack(int TheWave = 0)
        {
            IEnumerable<PawnKindDef> enumerable = new List<PawnKindDef>
            {
                PawnKindDefOf.Megascarab,
                PawnKindDefOf.Spelopede,
                PawnKindDefOf.Megaspider
            };
            PawnKindDef pawnKindDef;
            bool flag = GenCollection.TryRandomElement<PawnKindDef>(enumerable, out var pawnKindDefO);

            if (RCellFinder.TryFindRandomPawnEntryCell(out var spawnCenter112, Find.CurrentMap, CellFinder.EdgeRoadChance_Animal, false, null))
            {
                Find.LetterStack.ReceiveLetter(
                    $"虫族进攻 第{ ++TheWaveCount }波!",
                    "虫族顺着人类的气味一路追杀，它们不会放过任何一个人类",
                    LetterDefOf.ThreatBig
                    );

                for (int i = 0; i < Find.CurrentMap.mapPawns.FreeColonists.Count; i++)
                {
                    //Meteor_Object.TryFindCell(out var spawnCenter, Find.CurrentMap);
                    //List<Pawn> list = IncidentWorker_GerFaction.GenerateInsects(Find.CurrentMap.Tile, 0f);
                    //list.ForEach(XX => GenSpawn.Spawn(XX, spawnCenter, Find.CurrentMap, WipeMode.Vanish));
                    IntVec3 intVec = CellFinder.RandomClosewalkCellNear(spawnCenter112, Find.CurrentMap, 10, null);
                    Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDefO, Find.FactionManager.OfInsects);
                    GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, 0);
                    IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                }
            }
        }

        public static void DoZergAttackNormal()
        {
            IEnumerable<PawnKindDef> enumerable = new List<PawnKindDef>
            {
                PawnKindDefOf.Megascarab,
                PawnKindDefOf.Spelopede,
                PawnKindDefOf.Megaspider
            };
            PawnKindDef pawnKindDef;
            bool flag = GenCollection.TryRandomElement<PawnKindDef>(enumerable, out var pawnKindDefO);

            if (RCellFinder.TryFindRandomPawnEntryCell(out var spawnCenter112, Find.CurrentMap, CellFinder.EdgeRoadChance_Animal, false, null))
            {
                Find.LetterStack.ReceiveLetter(
                    $"虫族袭来",
                    "虫族顺着人类的气味一路追杀，它们不会放过任何一个人类，这些昆虫似乎被垃圾毒物所吸引而来。",
                    LetterDefOf.ThreatBig
                    );

                for (int i = 0; i < Find.CurrentMap.mapPawns.FreeColonists.Count; i++)
                {
                    //Meteor_Object.TryFindCell(out var spawnCenter, Find.CurrentMap);
                    //List<Pawn> list = IncidentWorker_GerFaction.GenerateInsects(Find.CurrentMap.Tile, 0f);
                    //list.ForEach(XX => GenSpawn.Spawn(XX, spawnCenter, Find.CurrentMap, WipeMode.Vanish));
                    IntVec3 intVec = CellFinder.RandomClosewalkCellNear(spawnCenter112, Find.CurrentMap, 10, null);
                    Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDefO, Find.FactionManager.OfInsects);
                    GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, 0);
                    IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                }
            }
        }

        /// <summary>
        /// 虫族伏击
        /// </summary>
        public void DoZergAttackFJ()
        {
            IEnumerable<PawnKindDef> enumerable = new List<PawnKindDef>
            {
                PawnKindDefOf.Megascarab,
                PawnKindDefOf.Spelopede,
                PawnKindDefOf.Megaspider
            };
            bool flag = GenCollection.TryRandomElement<PawnKindDef>(enumerable, out var pawnKindDefO);

            bool IsShow = false;
            Find.CurrentMap.mapPawns.FreeColonists.ForEach(XX =>
            {
                if (!XX.Dead)
                {
                    IntVec3 intVec = CellFinder.RandomClosewalkCellNear(XX.Position, Find.CurrentMap, 5, null);
                    if (intVec != null)
                    {
                        if (!IsShow)
                        {
                            IsShow = true;
                            Find.LetterStack.ReceiveLetter(
                            "虫族伏击",
                            "虫族顺着人类的气味一路追杀，它们不会放过任何一个人类",
                            LetterDefOf.ThreatBig
                            );
                        }
                        Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDefO, Find.FactionManager.OfInsects);
                        GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, 0);
                        IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                        if (Rand.Chance(0.3f))
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX.Position, Find.CurrentMap, 5, null);
                            Pawn pawn1 = PawnGenerator.GeneratePawn(pawnKindDefO, Find.FactionManager.OfInsects);
                            GenSpawn.Spawn(pawn1, intVec1, Find.CurrentMap, 0);
                            IncidentWorker_AnimalInsanityMass.DriveInsane(pawn1);
                        }
                    }
                }
            });

        }

        public static bool IsTimerDown { get; set; } = true;
        public static bool IsAttackOver { get; set; } = true;
        public static System.Random RG { get; set; } = new System.Random(Guid.NewGuid().GetHashCode());

        public static int TakeDownCount = 0;
        public static int TakeDownCount1 = 0;
        public static int TakeDownCount2 = 0;
        public static int TakeDownCount3 = 0;
        public static int TakeDownCount4 = 0;
        public static int TakeDownCount5 = 0;
        public static int TakeDownCount6 = 0;
        public static int TakeDownCount7 = 0;
        public static int TakeDownCount8 = 0;
        public static int TakeDownCount9 = 0;
        public static int TakeDownCount10 = 0;
        public static int TakeDownCount11 = 0;
        public static int TakeDownCount12 = 24 * 3; //超级流星时间并不会马上发生
        public static int TakeDownCount13 = 0; //超级流星时间并不会马上发生
        public static int TakeDownCount14 = 0;
        public static int TakeDownCount15 = 0;
        public static int TakeDownCount16 = 0;
        public static int TakeDownCount17 = 0;

        public static int TakeDownCount18 = 0;
        public static int TakeDownCount19 = 0;

        public static int KongTouTanCiShu = 0;

        /// <summary>
        /// 随机改变一个殖民者的技能等级
        /// </summary>
        public void ChangeTheSkillRandom()
        {
            var TheCI = Find.CurrentMap.mapPawns.FreeColonists.RandomElement();
            if (TheCI != null)
            {
                //WTheCI.needs.rest.TickResting();
                //TheCI.needs.food.CurLevelPercentage
                Find.LetterStack.ReceiveLetter(
                    "异变",
                    "你的殖民者似乎发生了一些潜在的变化",
                    LetterDefOf.ThreatBig
                    );
                //TheCI.skills.GetSkill(SkillDefOf.Construction).levelInt = Rand.Range(1,19);
                //随机变化殖民者技能等级
                TheCI.skills.skills.RandomElement().levelInt = Rand.Range(1, 8);

                //技能感兴趣程度随机改变
                if (Rand.Range(1, 1000) < 300)
                {
                    switch (Rand.Range(1, 3))
                    {
                        case 1:
                            TheCI.skills.skills.RandomElement().passion = Passion.None;
                            break;
                        case 2:
                            TheCI.skills.skills.RandomElement().passion = Passion.Minor;
                            break;
                        case 3:
                            TheCI.skills.skills.RandomElement().passion = Passion.Major;
                            break;
                    }
                }
            }
        }

        public static void RandomBuildingDestory()
        {
            Find.LetterStack.ReceiveLetter(
                    "建筑被毁",
                    "虫族的一系列动作导致一些建筑物受损",
                    LetterDefOf.NegativeEvent
                    );

            //最多摧毁8个建筑物
            for (int i = 0; i < Rand.Range(1, 8); i++)
            {
                var TheTriggerBuilding = Find.CurrentMap.listerBuildings.allBuildingsColonist.RandomElement();
                GenExplosion.DoExplosion(TheTriggerBuilding.Position, Find.CurrentMap, Rand.Range(0.1f, 0.3f), DamageDefOf.Flame, null, 10, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);
                TheTriggerBuilding?.Destroy();
            }
        }

        /// <summary>
        /// 执行流星坠落
        /// </summary>
        public bool DoTheMetorCreate()
        {
            bool TryFindCell(out IntVec3 cell, Map map)
            {
                //计算随机大小的流星
                int maxMineables = ThingSetMaker_Meteorite.MineablesCountRange.max * (Rand.Range(1, 2));
                if (Rand.Range(1, 1000) < 300) maxMineables /= 2;

                return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 0, default(IntVec3), -1, true, false, false, false, true, true, delegate (IntVec3 x)
                {
                    int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
                    CellRect cellRect = CellRect.CenteredOn(x, num, num);
                    int num2 = 0;
                    foreach (IntVec3 current in cellRect)
                    {
                        if (current.InBounds(map) && current.Standable(map))
                        {
                            num2++;
                        }
                    }
                    return num2 >= maxMineables;
                });
            }

            if (!TryFindCell(out var intVec, Find.CurrentMap))
            {
                return false;
            }
            List<Thing> list = ThingSetMakerDefOf.Meteorite.root.Generate();
            SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, intVec, Find.CurrentMap);
            //LetterDef baseLetterDef = list[0].def.building.isResourceRock ? LetterDefOf.PositiveEvent : LetterDefOf.NeutralEvent;
            //string str = string.Format(this.def.letterText, list[0].def.label).CapitalizeFirst();
            //base.SendStandardLetter(this.def.letterLabel + ": " + list[0].def.LabelCap, str, baseLetterDef, parms, new TargetInfo(intVec, map, false), Array.Empty<NamedArgument>());
            return true;
        }

        /// <summary>
        /// 随机选择一个有房顶的区域
        /// </summary>
        public static IntVec3 RandomDropSpot(Map map) => CellFinderLoose.RandomCellWith((IntVec3 c) => c.Roofed(map), map, 1000);

        /// <summary>
        /// 投放食物补给
        /// </summary>
        public static void DoTheGerPod(string SpecialBank = "")
        {

            if (Find.CurrentMap == null) return; //如果玩家现在正在远行，或是没有切换在地图页面，停止空投
            if (SpecialBank.Length != 0) goto SB;
            if (Rand.Chance(0.1f))
            {
                List<Thing> contents = new List<Thing>();
                contents.Add(ThingMaker.MakeThing(ThingDefOf.Chocolate));
                contents.Add(ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack));
                if (contents[0] != null) contents[0].stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.ColonistCount * 40);
                if (contents[1] != null) contents[1].stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.ColonistCount * 3);

                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                var refugee = ThingUtility.FindPawn(contents);
                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                Find.LetterStack.ReceiveLetter(
                                "食品补给舱",
                                "包含巧克力和压缩食品的空投补给舱",
                                LetterDefOf.PositiveEvent,
                                new TargetInfo(dropSpot, Find.CurrentMap, false)
                                );

                var podInfo = new ActiveDropPodInfo();
                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                podInfo.openDelay = 100;
                podInfo.leaveSlag = false;
                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
            }
            else if (Rand.Chance(0.2f))
            {
                List<Thing> contents = new List<Thing>();
                contents.Add(ThingMaker.MakeThing(ThingDefOf.Chocolate));
                if (contents[0] != null) contents[0].stackCount = Rand.Range(49, Find.CurrentMap.mapPawns.ColonistCount * 20);

                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                var refugee = ThingUtility.FindPawn(contents);
                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                Find.LetterStack.ReceiveLetter(
                                "巧克力补给舱",
                                "运载着大量巧克力的货仓",
                                LetterDefOf.PositiveEvent,
                                new TargetInfo(dropSpot, Find.CurrentMap, false)
                                );

                var podInfo = new ActiveDropPodInfo();
                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                podInfo.openDelay = 100;
                podInfo.leaveSlag = false;
                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
            }
            else if (Rand.Chance(0.2f))
            {
                List<Thing> contents = new List<Thing>();
                contents.Add(ThingMaker.MakeThing(ThingDefOf.Steel));
                if (contents[0] != null) contents[0].stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.ColonistCount * 50);

                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                var refugee = ThingUtility.FindPawn(contents);
                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                Find.LetterStack.ReceiveLetter(
                                "钢铁补给舱",
                                "运送大量钢铁补给仓",
                                LetterDefOf.PositiveEvent, new TargetInfo(dropSpot, Find.CurrentMap, false)
                                );

                var podInfo = new ActiveDropPodInfo();
                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                podInfo.openDelay = 100;
                podInfo.leaveSlag = false;
                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
            }
            else if (Rand.Chance(0.2f))
            {
                List<Thing> contents = new List<Thing>();
                contents.Add(ThingMaker.MakeThing(ThingDefOf.WoodLog));
                if (contents[0] != null) contents[0].stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.ColonistCount * 40);

                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                var refugee = ThingUtility.FindPawn(contents);
                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                Find.LetterStack.ReceiveLetter(
                                $"木材补给舱  { contents[0].stackCount } 个",
                                "运载着大量巧克力的货仓坠毁了",
                                LetterDefOf.PositiveEvent, new TargetInfo(dropSpot, Find.CurrentMap, false)
                                );

                var podInfo = new ActiveDropPodInfo();
                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                podInfo.openDelay = 100;
                podInfo.leaveSlag = false;
                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
            }
            else if (Rand.Chance(0.1f))
            {
                List<Thing> contents = new List<Thing>();
                contents.Add(ThingMaker.MakeThing(ThingDefOf.Wastepack));
                if (contents[0] != null) contents[0].stackCount = Rand.Range(1, 35);

                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                var refugee = ThingUtility.FindPawn(contents);
                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                Find.LetterStack.ReceiveLetter(
                                $"毒物空投  { contents[0].stackCount } 个有毒垃圾",
                                "运载着有毒垃圾的空投袭来，它们似乎来自敌对派系或者机械族。",
                                LetterDefOf.ThreatBig, new TargetInfo(dropSpot, Find.CurrentMap, false)
                                );

                var podInfo = new ActiveDropPodInfo();
                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                podInfo.openDelay = 100;
                podInfo.leaveSlag = false;
                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
                if (RG.Next(1, 10000) < 5000) 
                {
                    TheTimerManager.AddTimer(("ZergAttack_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
                    {
                        TTL = Rand.Range(1, 3),
                        _OnTimerDown = new System.Action(() =>
                        {
                            //随机进过1-3小时后，虫族袭来
                            DoZergAttackNormal();
                        })
                    });
                }
            }
            else
            {
                List<Thing> contents = new List<Thing>();
                contents.Add(ThingMaker.MakeThing(ThingDefOf.Pemmican));
                if (contents[0] != null) contents[0].stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.ColonistCount * 40);

                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                var refugee = ThingUtility.FindPawn(contents);
                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                Find.LetterStack.ReceiveLetter(
                                $"干肉饼补给舱{ contents[0].stackCount } 个",
                                "运载着大量干肉饼的货仓坠毁了",
                                LetterDefOf.PositiveEvent, new TargetInfo(dropSpot, Find.CurrentMap, false)
                                );

                var podInfo = new ActiveDropPodInfo();
                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                podInfo.openDelay = 100;
                podInfo.leaveSlag = false;
                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
            }
            return;
        SB:;
            {
                //空投箱掉落指定大量物资
                List<Thing> contents = new List<Thing>();
                contents.Add(ThingMaker.MakeThing(ThingDefOf.Beer));
                if (contents[0] != null) contents[0].stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.ColonistCount * 100);

                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                var refugee = ThingUtility.FindPawn(contents);
                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                Find.LetterStack.ReceiveLetter(
                                $"啤酒货仓坠毁 { contents[0].stackCount } 个",
                                "啤酒货仓坠毁",
                                LetterDefOf.PositiveEvent, new TargetInfo(dropSpot, Find.CurrentMap, false)
                                );

                var podInfo = new ActiveDropPodInfo();
                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                podInfo.openDelay = 100;
                podInfo.leaveSlag = false;
                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
            }
            return;
        }

        /// <summary>
        /// 全员感染流感
        /// </summary>
        //public void TakeAllFlu()
        //{
        //    Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
        //    {
        //        var plagueOnPawn = XX.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Flu);
        //        if (plagueOnPawn != null)
        //        {
        //            //如果已经感染流感，则不作处理
        //            //plagueOnPawn.Severity += randomSeverity;
        //        }
        //        else
        //        {
        //            //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
        //            Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Flu, XX);
        //            //设置这个状态的严重程度
        //            hediff.Severity = 0f;
        //            //把状态添加到被击中的目标身上
        //            XX.health.AddHediff(hediff);

        //            MoteMaker.ThrowText(XX.PositionHeld.ToVector3(), XX.MapHeld, "流感感染", Color.red, 12f);
        //        }
        //    });
        //    //TheReso
        //}

        /// <summary>
        /// 全员败血症
        /// </summary>
        public static void TakeAllBloodLess()
        {
            var IsLossBlood = false;
            Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
            {
                //GerBaiXieZheng

                var plagueOnPawn = XX.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                if (plagueOnPawn != null)
                {
                    //如果已经感染流感，则不作处理
                    //plagueOnPawn.Severity += randomSeverity;
                }
                else
                {
                    //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, XX);
                    //设置这个状态的严重程度
                    hediff.Severity = 0.6f;
                    //把状态添加到被击中的目标身上
                    XX.health.AddHediff(hediff);
                    IsLossBlood = true;

                    MoteMaker.ThrowText(XX.PositionHeld.ToVector3(), XX.MapHeld, "败血症感染", Color.red, 2f);
                }
            });

            //是否有人感染失血症状
            if (IsLossBlood)
            {
                Find.LetterStack.ReceiveLetter("失血症感染", "失血症状正在你的营地中传播", LetterDefOf.ThreatSmall);
            }
            //TheReso
        }

        public void TakeAllXingFen()
        {
            var IsLossBlood = false;
            Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
            {
                var plagueOnPawn = XX.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerSpeedHigh"));
                if (plagueOnPawn != null)
                {
                    //如果已经感染流感，则不作处理
                    //plagueOnPawn.Severity += randomSeverity;
                }
                else
                {
                    //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                    Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerSpeedHigh"), XX);
                    //把状态添加到被击中的目标身上
                    XX.health.AddHediff(hediff);


                    MoteMaker.ThrowText(XX.PositionHeld.ToVector3(), XX.MapHeld, "狂暴", Color.green, 3f);
                    IsLossBlood = true;
                }
            });

            //是否有人感染失血症状
            if (IsLossBlood)
            {
                Find.LetterStack.ReceiveLetter("疾风祝福", "所有殖民者的移动速度和操作速度将会在短时间内提高", LetterDefOf.PositiveEvent);
            }
            //TheReso
        }

        //TheCI.needs.food.CurLevelPercentage

        /// <summary>
        /// 随机一个小人感染上厌食症
        /// </summary>
        public void RandomColonistFoodMan()
        {
            var IsLossBlood = false;
            Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
            {
                var plagueOnPawn = XX.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerFoodMan"));
                if (plagueOnPawn != null)
                {
                    //如果已经感染流感，则不作处理
                    //plagueOnPawn.Severity += randomSeverity;
                }
                else
                {
                    //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                    Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerFoodMan"), XX);
                    //把状态添加到被击中的目标身上
                    XX.health.AddHediff(hediff);

                    MoteMaker.ThrowText(XX.PositionHeld.ToVector3(), XX.MapHeld, "狂暴", Color.green, 2f);
                    IsLossBlood = true;
                }
            });

            //是否所有小人都感染轻食症
            if (IsLossBlood)
            {
                Find.LetterStack.ReceiveLetter("食物风暴", "全员进食欲望下降", LetterDefOf.PositiveEvent);
            }
        }

        /// <summary>
        /// 保持所有殖民者饱食度
        /// </summary>
        public void UpdateColonistFoodMan()
        {
            bool IsOutDoor(Pawn XX)
            {
                Room tmp_room = XX.GetRoom(RegionType.Set_Passable);
                if (tmp_room != null && !tmp_room.PsychologicallyOutdoors)
                {
                    // indoor pawn
                    return true;
                }
                else
                {
                    // outdoor pawn
                    return false;
                }
            }

            Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
            {
                //XX.mindState.duty = new PawnDuty(DutyDefOf.SleepForever);
                //如果感染了轻食症状
                var plagueOnPawn = XX.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerFoodMan"));
                if (plagueOnPawn != null)
                {
                    //小人在一段时间内不需要进食
                    if (RG.Next(1, 1000) < 120) XX.needs.food.CurLevelPercentage = 1f;
                    //if (RG.Next(1, 1000) < 200) XX.needs.rest.CurLevelPercentage = 1f;
                }
                else
                {
                    ////我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                    //Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerFoodMan"), XX);
                    ////把状态添加到被击中的目标身上
                    //XX.health.AddHediff(hediff);

                    //MoteMaker.ThrowText(XX.PositionHeld.ToVector3(), XX.MapHeld, "狂暴", Color.green, 12f);
                    //IsLossBlood = true;
                }

                //MoteMaker.ThrowText(XX.PositionHeld.ToVector3(), XX.MapHeld, IsOutDoor(XX) ? "室内" : "户外", Color.red, 2f);
                //plagueOnPawn = XX.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerXingHongZuZhou"));
                //if (plagueOnPawn != null)
                //{
                //	//如果已经感染流感，则不作处理
                //	//plagueOnPawn.Severity += randomSeverity;
                //}
                //else
                //{
                //	//我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                //	Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerXingHongZuZhou"), XX);
                //	//把状态添加到被击中的目标身上
                //	XX.health.AddHediff(hediff);

                //	MoteMaker.ThrowText(XX.PositionHeld.ToVector3(), XX.MapHeld, "猩红诅咒", Color.green, 2f);
                //	//IsLossBlood = true;
                //}
            });
            //Room.MakeNew(Find.CurrentMap);

            //GerXingHongZuZhou

        }

        /// <summary>
        /// 永久睡眠
        /// </summary>
        /// <param name="pawn"></param>
        private void SleepShock(Pawn pawn)
        {
            if (!pawn.Dead)
            {
                if (pawn.needs.rest != null)
                {
                    pawn.needs.rest.CurLevel = 0f;
                }
                if (pawn.CurJob != null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
                pawn.mindState.duty = new PawnDuty(DutyDefOf.SleepForever);
            }
        }
        /// <summary>
        /// 判断一个物品是否可修理
        /// </summary>
        private bool CanRepair(Thing thing) => thing.def.useHitPoints && thing.HitPoints < thing.MaxHitPoints;
        /// <summary>
        /// 维修
        /// </summary>
        /// <param name="p"></param>
        public static void RepairAllApparelsInOnePawn(Pawn p)
        {
            var Factor = 1;
            foreach (Apparel a in p.apparel.WornApparel)
            {
                if (a.def.useHitPoints && a.HitPoints < Factor * a.MaxHitPoints)
                {
                    a.HitPoints = (int)(Factor * a.MaxHitPoints);
                }
            }
            foreach (ThingWithComps t in p.equipment.AllEquipmentListForReading)
            {
                if (t.def.useHitPoints && t.HitPoints < Factor * t.MaxHitPoints)
                {
                    t.HitPoints = (int)(Factor * t.MaxHitPoints);
                }
            }
            foreach (Thing t in p.inventory.GetDirectlyHeldThings())
            {
                if (t.def.useHitPoints && t.HitPoints < Factor * t.MaxHitPoints)
                {
                    t.HitPoints = (int)(Factor * t.MaxHitPoints);
                }
            }
        }
        /// <summary>
        /// 修复当前地图上所有殖民者的装备
        /// </summary>
        public static void RepairAllApparelsInPawn()
        {
            //foreach (Pawn p in Find.CurrentMap.mapPawns.FreeColonists)
            //{
            //    RepairAllApparelsInOnePawn(p);
            //}

            try
            {
                var RR = Find.CurrentMap.mapPawns.FreeColonists.RandomElement();
                if (RR != null && Find.CurrentMap != null)
                {
                    //自动维修随机小人身上的装备
                    RepairAllApparelsInOnePawn(RR);

                    Find.LetterStack.ReceiveLetter(
                                $"众神眷顾({ RR.Name }) 装备自动修理",
                                "由于众神的眷顾，随机被选中的小人身上的装备还原耐久度",
                                LetterDefOf.PositiveEvent
                                );
                }
            }
            catch { }
        }

        /// <summary>
        /// 狂风呼啸同时带来资源，清理灰尘
        /// </summary>
        public static (int,int ResouceCount) RemoveAllFilth()
        {
            int num = 0;

            Map TriggerMap = Find.RandomPlayerHomeMap;
            foreach (IntVec3 c in TriggerMap.AllCells)
            {
                List<Thing> Things = c.GetThingList(TriggerMap);
                for (int k = Things.Count - 1; k >= 0; k--)
                {
                    if (Things[k] is Filth)
                    {
                        Things[k].Destroy(DestroyMode.Vanish);
                        num++;
                    }
                }
            }

            //吹风差异化
            if (!SteamUtility.SteamPersonaName.Contains("神经病有所"))
            {
                var NumberOfResouce = RG.Next(1, num / 20);

                //狂风呼啸同时带来资源
                for (int i = 0; i < NumberOfResouce; i++)
                {
                    var TheTriggerCell = CellHelper.GetRandomCell();
                    var TriggerThing = Rand.Chance(0.5f) ? ThingDefOf.Beer : ThingDefOf.WoodLog;
                    if (Rand.Chance(0.1f)) TriggerThing = ThingDefOf.MedicineIndustrial;
                    if (Rand.Chance(0.1f)) TriggerThing = ThingDefOf.InsectJelly;
                    if (Rand.Chance(0.1f)) TriggerThing = ThingDefOf.MealFine;

                    //根据不同东西刷新不同的数量
                    Thing thing2 = GenSpawn.Spawn(TriggerThing, TheTriggerCell, TriggerMap, WipeMode.Vanish);
                    if (thing2 != null && thing2.def == ThingDefOf.Beer) thing2.stackCount = Rand.Range(1, 1);
                    if (thing2 != null && thing2.def == ThingDefOf.WoodLog) thing2.stackCount = Rand.Range(1, 3);
                    if (thing2 != null && thing2.def == ThingDefOf.MedicineIndustrial) thing2.stackCount = Rand.Range(1, 1);
                    if (thing2 != null && thing2.def == ThingDefOf.InsectJelly) thing2.stackCount = Rand.Range(1, 20);
                    if (thing2 != null && thing2.def == ThingDefOf.MealFine) thing2.stackCount = Rand.Range(1, 1);
                }
                return (num, NumberOfResouce);
            }
            else 
            {
                return (0, 0);
            }
        }

        public override void StartedNewGame()
        {
            Task.Run(() => 
            {
                try
                {
                    WebSocketHelper.DoWebSocketSend("StartedNewGame");
                }
                catch { }
            });
           
            base.StartedNewGame();
        }

        public override void LoadedGame()
        {
            Task.Run(() =>
            {
                try
                {
                    WebSocketHelper.DoWebSocketSend("LoadedGame");
                }
                catch { }
            });
            base.LoadedGame();
        }

        /// <summary>
        /// 虫族在地图营地中的突袭点
        /// </summary>
        static Dictionary<Map, List<IntVec3>> _RaidPoint { get; set; } = new Dictionary<Map, List<IntVec3>>();
        public static Faction _ChoiceFaction = null;

        public static Map _TheHasFriendMap = null;

        /// <summary>
        /// 刷新土匪营地建筑
        /// </summary>
        public static void CreateRandomRoom_HasFriend(Map TheMap) 
        {
            //想服务器发送探索土匪营地的信息 - 探索友军基地
            WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#{ SteamUtility.SteamPersonaName }#PengYouJi#");

            //进入地图随机决定此次的盟友派系
            _ChoiceFaction = Find.FactionManager.RandomAlliedFaction();

            //土匪刷新遗迹
            if (SteamUtility.SteamPersonaName.Contains("神经病有所"))
            {
                TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                {
                    TTL = 1,
                    _Data = TheMap,
                    _OnTimerDownWithOB = new System.Action<object>(TheMap1 =>
                    {
                        if (TheMap1 != null && TheMap1 is Map TheMapEx)
                        {
                            for (int loopa = 0; loopa < 4; loopa++)
                            {
                                //随机点生成资源房间
                                CellRect cellRect = CellRect.CenteredOn(TheMapEx.GetRandomCell(), 25, 25).ClipInsideMap(TheMapEx);
                                ResolveParams resolveParams = default(ResolveParams);
                                resolveParams.rect = cellRect;
                                resolveParams.disableSinglePawn = new bool?(false);
                                resolveParams.disableHives = new bool?(false);
                                resolveParams.makeWarningLetter = new bool?(true);
                                if (Find.Storyteller.difficulty.peacefulTemples)
                                {
                                    resolveParams.podContentsType = new PodContentsType?(PodContentsType.AncientFriendly);
                                }
                                BaseGen.globalSettings.map = TheMapEx;
                                BaseGen.symbolStack.Push("ancientTemple", resolveParams, null);
                                BaseGen.Generate();
                            }
                        }
                    })
                });
            }

            //Find.LetterStack.ReceiveLetter("友军控制区", "您拜访了地图上一个友军控制的基地，在这里您的部队会感到安全，并且能够接受友军的作战训练。", LetterDefOf.PositiveEvent);

            TheTimerManager.AddTimer(("FriendMap_GetOkayBuffer" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
            {
                TTL = Rand.Range(1, 2),
                _TheData = TheMap,
                _OnTimerDown_with_OB = new System.Action<object>(OB =>
                {
                    try
                    {
                        var Fmap = OB as Map;

                        //添加驻扎友军基地BUFF
                        Fmap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                        {
                            if (!XX.HasTrait(TraitDef.Named("FLearner")))
                            {
                                var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerFriendBase"));
                                if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerFriendBase"));
                            }
                            else 
                            {
                                var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerFriendBase_LearnBig"));
                                if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerFriendBase_LearnBig"));
                            }
                        });

                        var TheRandomLearnPawn = Fmap.mapPawns.FreeColonists.RandomElement();
                        var RandomSkill = TheRandomLearnPawn?.skills.skills.RandomElement();


                        if (RG.Next(1, 1000) < 700)
                        {
                            if (TheRandomLearnPawn != null)
                            {
                                var LearnPoint = (float)RG.Next(1, 5000);
                                if (SteamUtility.SteamPersonaName.Contains("神经病有所"))
                                {
                                    LearnPoint *= 20;
                                }
                                else 
                                {
                                    //拥有友军学习者的小人经验获取20倍
                                    if (TheRandomLearnPawn.HasTrait(TraitDef.Named("FLearner"))) LearnPoint *= 20;
                                }
                                //一个随机小人学习一样随机技能
                                RandomSkill?.Learn(LearnPoint, true);
                                Find.LetterStack.ReceiveLetter($"技能学习{TheRandomLearnPawn.Name} 技能 {RandomSkill.def.defName} 提高 {LearnPoint}", "您的小人在友军基地接受技能培训，随机一项技能能力被加强", LetterDefOf.RitualOutcomePositive);

                                WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#XiaoRenXueXi#{SteamUtility.SteamPersonaName}#");
                            }
                        }
                        else
                        {
                            if (RandomSkill.passion == Passion.Minor)
                            {
                                RandomSkill.passion = Passion.Major;
                                Find.LetterStack.ReceiveLetter($"技能兴趣培养 {TheRandomLearnPawn.Name} 技能 {RandomSkill.def.defName} 兴趣提高", "您的小人在友军基地接受技能培训，随机一项技能能力被加强", LetterDefOf.RitualOutcomePositive);

                                WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#XiaoRenXueXi-SkillUp#{SteamUtility.SteamPersonaName}#");
                            }

                            if (RandomSkill.passion == Passion.None)
                            {
                                RandomSkill.passion = Passion.Minor;
                                Find.LetterStack.ReceiveLetter($"技能兴趣培养 {TheRandomLearnPawn.Name} 技能 {RandomSkill.def.defName} 兴趣提高", "您的小人在友军基地接受技能培训，随机一项技能能力被加强", LetterDefOf.RitualOutcomePositive);

                                WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#XiaoRenXueXi-SkillUp#{SteamUtility.SteamPersonaName}#");
                            }
                        }
                    }
                    catch { }
                }),
            });

            CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier, _ChoiceFaction);
            CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier, _ChoiceFaction);
            CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier, _ChoiceFaction);
            CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier, _ChoiceFaction);
            if (HotseatGameComponent.FindTheKeyToRescure == true) 
            {
                HotseatGameComponent.FindTheKeyToRescure = false;
                CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier, Faction.OfPlayer);
                Find.LetterStack.ReceiveLetter("友军加入", "由于你持有的帝国征召令，盟友士兵直接加入。", LetterDefOf.RitualOutcomePositive);
            }

            var LoopIndex = RG.Next(1, (2 + (GenDate.DaysPassed / 50)));
            for (int loopa = 0; loopa < LoopIndex*3; loopa++)
            {
                CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier, _ChoiceFaction);
            }

            //WaponCommon
            if (RG.Next(1, 1000) < 120)
            {
                Find.LetterStack.ReceiveLetter("发现一批武器装备", "您在友军控制区发现了被一遗弃的武器装备弹药库。", LetterDefOf.RitualOutcomePositive);

                WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#YouJunJiDe-ZhunagBei#{ SteamUtility.SteamPersonaName }#");

                //随机点生成资源房间
                CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 15, 15).ClipInsideMap(TheMap);
                if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                {
                    list = new List<CellRect>();
                    MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                }
                ResolveParams resolveParams = default(ResolveParams);
                resolveParams.edgeDefenseTurretsCount = 5;
                resolveParams.mechanoidsCount = 6;
                resolveParams.rect = cellRect;
                resolveParams.faction = Faction.OfPlayer;

                //var LoopA = RG.Next(1, 4);
                // 一些巧克力资源
                resolveParams.stockpileConcreteContents = WaponCommon.GenerateWeapons(RG.Next(1, 8));

                BaseGen.globalSettings.map = TheMap;
                BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                BaseGen.globalSettings.minThroneRooms = 1;
                BaseGen.globalSettings.minBuildings = 3;

                BaseGen.symbolStack.Push("storage", resolveParams, null);
                BaseGen.Generate();
                MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                //随机玩家进入地图位置
                CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                MapGenerator.PlayerStartSpot = playerStartSpot;

                list.Add(cellRect);
            }
            else if (RG.Next(1, 10000) < 5444)
            {

                WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#YouJunJiDe-KongTou#{ SteamUtility.SteamPersonaName }#");
                //TryFindCell(out var TheCell, TheMap);
                //Find.LetterStack.ReceiveLetter("发现神秘空投", "一个神秘残骸在附近被发现，殖民者能自动触发空投箱内部程序", LetterDefOf.NeutralEvent, new TargetInfo(TheCell, Find.CurrentMap, false));

                //SoundDefOf.ShipTakeoff.PlayOneShot(new TargetInfo(TheCell, TheMap, false));
                //var rr = ThingMaker.MakeThing(ThingDef.Named("ShipChunk_Ger"));
                //var TheThing12 = GenSpawn.Spawn(rr, TheCell, TheMap, 0);

                //TheThing12.questTags = new List<string>();
                //TheThing12.questTags.Add("Boomer");

                //TheThing12.questTags.Add("TanSuo");

                //随机点生成资源房间
                CellRect cellRect = CellRect.CenteredOn(TheMap.Center, 25, 25).ClipInsideMap(TheMap);
                if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                {
                    list = new List<CellRect>();
                    MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                }
                ResolveParams resolveParams = default(ResolveParams);
                resolveParams.edgeDefenseTurretsCount = 5;
                resolveParams.mechanoidsCount = 6;
                resolveParams.rect = cellRect;
                resolveParams.faction = Faction.OfPlayer;

                //var LoopA = RG.Next(1, 4);
                // 一些巧克力资源[([]
                resolveParams.stockpileConcreteContents = new List<Thing> { ThingMaker.MakeThing(ThingDef.Named("ShipChunk_Ger")), ThingMaker.MakeThing(ThingDef.Named("ShipChunk_Ger")) };
                resolveParams.stockpileConcreteContents[0].questTags = new List<string>();
                resolveParams.stockpileConcreteContents[0].questTags.Add("Boomer");
                resolveParams.stockpileConcreteContents[0].questTags.Add("TanSuo");

                resolveParams.stockpileConcreteContents[1].questTags = new List<string>();
                resolveParams.stockpileConcreteContents[1].questTags.Add("Boomer");
                resolveParams.stockpileConcreteContents[1].questTags.Add("TanSuo");

                BaseGen.globalSettings.map = TheMap;
                BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                BaseGen.globalSettings.minThroneRooms = 1;
                BaseGen.globalSettings.minBuildings = 3;

                BaseGen.symbolStack.Push("storage", resolveParams, null);
                BaseGen.Generate();
                MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                //随机玩家进入地图位置
                CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                MapGenerator.PlayerStartSpot = playerStartSpot;
            }
            else if (RG.Next(1, 10000) < 144) 
            {
                Find.LetterStack.ReceiveLetter("发现神秘空投与武器装备", "您在友军控制区发现了被一遗弃的武器装备弹药库与神秘空投。", LetterDefOf.RitualOutcomePositive);

                WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#YouJunJiDe-KongTouZhuangBei#{ SteamUtility.SteamPersonaName }#");
                //随机点生成资源房间
                CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                {
                    list = new List<CellRect>();
                    MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                }
                ResolveParams resolveParams = default(ResolveParams);
                resolveParams.edgeDefenseTurretsCount = 5;
                resolveParams.mechanoidsCount = 6;
                resolveParams.rect = cellRect;
                resolveParams.faction = Faction.OfPlayer;

                //var LoopA = RG.Next(1, 4);
                // 一些巧克力资源[([]
                resolveParams.stockpileConcreteContents = new List<Thing> { ThingMaker.MakeThing(ThingDef.Named("ShipChunk_Ger")), 
                    ThingMaker.MakeThing(ThingDef.Named("ShipChunk_Ger")) };

                var RandomKongTouCount = RG.Next(2, 5);
                for (int loopa = 0; loopa < RandomKongTouCount; loopa++)
                {
                    resolveParams.stockpileConcreteContents[loopa].questTags = new List<string>
                    {
                        "Boomer",
                        "TanSuo"
                    };
                }

                var Wapons = WaponCommon.GenerateWeapons(RG.Next(1, 5));
                foreach (var item in Wapons)
                {
                    resolveParams.stockpileConcreteContents.Add(item);
                }

                BaseGen.globalSettings.map = TheMap;
                BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                BaseGen.globalSettings.minThroneRooms = 1;
                BaseGen.globalSettings.minBuildings = 3;

                BaseGen.symbolStack.Push("storage", resolveParams, null);
                BaseGen.Generate();
                MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                //随机玩家进入地图位置
                CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                MapGenerator.PlayerStartSpot = playerStartSpot;

                list.Add(cellRect);
            }
        }


        public static bool IsHasFriend=> Find.FactionManager.RandomAlliedFaction() != null;
        public static int RetryCount_ShangDui = 0;
        /// <summary>
        /// 创建随机资源房间
        /// </summary>
        public static void CreateRandomRoom(Map TheMap)
        {
            //想服务器发送探索土匪营地的信息
            WebSocketHelper.DoWebSocketSendSync($"ClientLog##{ SteamUtility.SteamPersonaName }##TuFei##");

            void DoTheWork(IncidentDef ID,IncidentParms IP,Map _TheMap)
            {
                //最多尝试20次
                if (RetryCount_ShangDui++ >= 20) return;
                var TheRR = ID.Worker.TryExecute(IP);
                if (!TheRR)
                {
                    //Find.LetterStack.ReceiveLetter("刷新商队失败", "您的盟友派出援军支援你的突袭行动", LetterDefOf.PositiveEvent);
                    TheTimerManager.AddTimerSmall(("ItemStash_DoTheWork" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = 1,
                        _Data = new List<object> { ID, IP, TheMap },
                        _OnTimerDownWithOB = new System.Action<object>(OB =>
                        {
                            List<object> _TheMap1 = OB as List<object>;
                            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, _TheMap);
                            incidentParms.target = _TheMap;
                            //incidentParms.faction = _ChoiceFaction; 
                            incidentParms.faction = _ChoiceFaction;
                            DoTheWork(_TheMap1[0] as IncidentDef, incidentParms, _TheMap1[2] as Map);
                        }),
                    });
                }
            }

            //进入地图随机决定此次的盟友派系
            _ChoiceFaction = Find.FactionManager.RandomNonHostileFaction(false, true);
            //土匪营地随机商队
            if (RG.Next(1, 10000) < 551)  //已屏蔽
            {
                RetryCount_ShangDui = 0;
                TheTimerManager.AddTimer(("ItemStash_TraderCaravanArrival" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
                {
                    TTL = Rand.Range(1, 1),
                    _TheData = TheMap,
                    _OnTimerDown_with_OB = new System.Action<object>(OB => 
                    {
                        if (OB == null) return;
                        Map _TheMap = OB as Map;

                        //随机点生成资源房间
                        CellRect cellRect = CellRect.CenteredOn(_TheMap.GetRandomCell(), 5, 5).ClipInsideMap(_TheMap);
                        if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                        {
                            list = new List<CellRect>();
                            MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                        }
                        ResolveParams resolveParams = default(ResolveParams);
                        resolveParams.edgeDefenseTurretsCount = 5;
                        resolveParams.mechanoidsCount = 6;
                        resolveParams.rect = cellRect;
                        resolveParams.faction = Faction.OfPlayer;

                        //刷新银币
                        var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.Silver) };
                        TheDongXi.FirstOrDefault().stackCount = RG.Next(1, 100);
                        //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                        resolveParams.stockpileConcreteContents = TheDongXi;

                        BaseGen.globalSettings.map = _TheMap;
                        BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                        BaseGen.globalSettings.minThroneRooms = 1;
                        BaseGen.globalSettings.minBuildings = 3;

                        BaseGen.symbolStack.Push("storage", resolveParams, null);
                        BaseGen.Generate();
                        MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                        //随机玩家进入地图位置
                        CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, _TheMap), _TheMap, 0f, out var playerStartSpot);
                        MapGenerator.PlayerStartSpot = playerStartSpot;

                        list.Add(cellRect);

                        var allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName == "TraderCaravanArrival");
                        //判断玩家有没有盟友派系
                        //if (allDefsListForReading.Count() != 0 && _ChoiceFaction != null)
                        //{
                            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, _TheMap);
                            incidentParms.target = _TheMap;
                            //incidentParms.faction = _ChoiceFaction; 
                            incidentParms.faction = _ChoiceFaction;
                            var TheRR = allDefsListForReading.FirstOrDefault().Worker.TryExecute(incidentParms);
                        if (!TheRR)
                        {
                            //Find.LetterStack.ReceiveLetter("刷新商队失败", "您的盟友派出援军支援你的突袭行动", LetterDefOf.PositiveEvent);
                            DoTheWork(allDefsListForReading.FirstOrDefault(), incidentParms, _TheMap);
                        }
                        else 
                        {
                            Find.LetterStack.ReceiveLetter("盟友商队", "您的盟友派出援军支援你的突袭行动", LetterDefOf.PositiveEvent);
                        }
                        //}
                        //else
                        //{
                        //    //如果玩家没有盟友派系，则立即改变一个敌人成为盟友
                        //    StationHelper.MakeFriends();
                        //    _ChoiceFaction = Find.FactionManager.RandomAlliedFaction();
                        //    //立即刷新商队
                        //    IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, _TheMap);
                        //    incidentParms.target = _TheMap;
                        //    //incidentParms.faction = _ChoiceFaction;
                        //    incidentParms.faction = Find.FactionManager.RandomAlliedFaction(true);
                        //    allDefsListForReading.FirstOrDefault().Worker.TryExecute(incidentParms);
                        //}
                    }),
                });
            }

            //处理土匪营怪癖探索
            TheTimerManager.AddTimer(("Player_QiangJieAiHaoZhe" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
            {
                TTL = Rand.Range(1, 4),
                _TheData = TheMap,
                _OnTimerDown_with_OB = new System.Action<object>(OB =>
                {
                    Map _TheMap = OB as Map;
                    //如果玩家快速推出地图，则不处理
                    if (_TheMap == null) return;

                    //QiangJieAiHao
                    _TheMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                    {
                        //所有持有土匪营地探索怪癖的小人，概率追加疾病
                        if (XX.HasTrait(TraitDef.Named("QiangJieAiHao")) && RG.Next(1,10000) < 2333)
                        {
                            //GerStart1143

                            var plagueOnPawn = XX.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerXingHongZuZhou2"));
                            if (plagueOnPawn != null)
                            {
                                //如果已经感染流感，则不作处理
                                //plagueOnPawn.Severity += randomSeverity;
                            }
                            else
                            {
                                //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                                Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerXingHongZuZhou2"), XX);
                                //把状态添加到被击中的目标身上
                                XX.health.AddHediff(hediff);

                                MoteMaker.ThrowText(XX.PositionHeld.ToVector3(), XX.MapHeld, "狂暴", Color.green, 2f);
                            }
                        }  
                    });

                    _TheMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                    {
                        if (XX.HasTrait(TraitDef.Named("QiangJieAiHao")) && RG.Next(1, 10000) < 6333)
                        {
                            var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart1143"));
                            if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart1143"));
                        }
                    });
                }),
            });

            //劫掠者战机音效
            if (TheGlobalInfo.IsJieLueZhe)
            {
                TheGlobalInfo.IsJieLueZhe = false;
                TheTimerManager._TriggerMap = TheMap;
                //SoundDef.Named("Shuttle_Landing").PlayOneShot(new TargetInfo(TheTimerManager._TriggerMap.GetRandomCell(), Find.CurrentMap, false));

                //每次播放50次战机呼啸声音
                for (int i = 0; i < 50; i++)
                {
                    TheTimerManager.AddTimerSmall(("ZergAttack_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = i,
                        _OnTimerDown = new System.Action(() =>
                        {
                            SoundDef.Named("Shuttle_Landing").PlayOneShot(new TargetInfo(TheTimerManager._TriggerMap.GetRandomCell(), Find.CurrentMap, false));
                        })
                    });
                }
                TheMap.JieLueZheZhanJiBuff();
            }

            //if (TheGlobalInfo.IsBuLuoYinCangBing)
            //{
            //    //踏马随机选择一个玩家基地
            //    var PlayerRandomHome = Find.RandomPlayerHomeMap;
            //    //踏马删除上次所有小人
            //    TheGlobalInfo._BuLuoTouXiGroup.ForEach(XX => XX.Destroy());

            //    //部落人数因生存天数而定 - 直接刷新在玩家基地随机点
            //    for (int loopa = 0; loopa < RG.Next(1, (2 + (GenDate.DaysPassed / 50))); loopa++)
            //    {
            //        var ThePawn = CreateEnemy(CellHelper.GetRandomCell(), PlayerRandomHome, PawnKindDefOf.Tribal_Warrior);

            //        //踏马缓存刷新部落小人
            //        TheGlobalInfo._BuLuoTouXiGroup.AddRange(ThePawn);
            //    }
            //}

            //每次土匪营地探索后11.2%概率触发报复袭击
            //if (RG.Next(1, 1000) < 112) 
            //{
            //    var AfterHourAttack = RG.Next(24, 62);
            //    Find.LetterStack.ReceiveLetter("报复", $"土匪将会以眼还眼,土匪袭击将会在 { AfterHourAttack }小时 后到达你的一个随机殖民地", LetterDefOf.ThreatBig);
            //    TheTimerManager.AddTimer(("HumanAttack_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
            //    {
            //        TTL = AfterHourAttack,  //未来24至62小时开始袭击
            //        _OnTimerDown = new System.Action(() =>
            //        {
            //            //优先选择财富最高的玩家老家进攻
            //            var TotalW = Find.Maps.Where(XX => XX.IsPlayerHome).Sum(XX => XX.wealthWatcher.WealthTotal); //计算档期那玩家总财富
            //            var TriggerMapX = Find.RandomPlayerHomeMap;
            //            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, TriggerMapX);
            //            incidentParms.forced = true;
            //            incidentParms.faction = Find.FactionManager.RandomEnemyFaction();
            //            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            //            incidentParms.points = TotalW / RG.Next(200,1000);
            //            incidentParms.target = TriggerMapX;
            //            IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
            //        })
            //    });
            //}

            //    Task.Run(() => 
            //{
            //    try
            //    {
            //        int aaa = 0;
            //        while (aaa++ <= 100)
            //        {
            //            //Find.LetterStack.ReceiveLetter("遭遇敌军", "在此图上存在装备精良敌人士兵，数量众多并且会对你抱有强烈敌意", LetterDefOf.PositiveEvent);
            //            if (TheTimerManager._TriggerMap != null)
            //            {
            //                //Find.LetterStack.ReceiveLetter("遭遇敌军", "在此图上存在装备精良敌人士兵，数量众多并且会对你抱有强烈敌意", LetterDefOf.PositiveEvent);
            //                SoundDef.Named("Shuttle_Landing").PlayOneShot(new TargetInfo(TheTimerManager._TriggerMap.GetRandomCell(), Find.CurrentMap, false));
            //            }
            //            Thread.Sleep(500);
            //        }
            //    }
            //    catch { };
            //});

            //try
            //{
            //    //刷新土匪士兵
            //    if (GenDate.DaysPassed >= 200 && RG.Next(1, 1000) < 100) TheMap.CreateCustomMapDefence2();
            //}
            //catch { }

            try
            {
                //小概率在土匪图出现古代士兵
                if (TheGlobalInfo.IsEmemyHasGun)
                {
                    //经过天数越多，敌方古代士兵出现的数量越多
                    var LoopIndex = RG.Next(2, (4 + (GenDate.DaysPassed / 50)));
                    for (int loopa = 0; loopa < LoopIndex; loopa++)
                    {
                        CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier);
                    }
                }

                if (TheGlobalInfo.IsEmemyBigForce) 
                {
                    TheTimerManager.AddTimer(("ItemStash_TraderCaravanArrival" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
                    {
                        TTL = Rand.Range(4, 8),
                        _TheData = TheMap,
                        _OnTimerDown_with_OB = new System.Action<object>(OB =>
                        {
                            if (OB!=null && OB is Map TheMap1 && Find.CurrentMap == TheMap1)
                            {
                                //经过天数越多，敌方古代士兵出现的数量越多
                                var LoopIndex = RG.Next(2, (4 + (GenDate.DaysPassed / 50)));
                                for (int loopa = 0; loopa < LoopIndex; loopa++)
                                {
                                    CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier);
                                }

                                Find.LetterStack.ReceiveLetter("土匪主力部队", "您的进攻行为激怒土匪势力，现在他们正在朝你疯狂报复！", LetterDefOf.NegativeEvent);
                            }
                        }),
                    });
                }

                //刷新盟友势力士兵
                if (_ChoiceFaction != null && TheGlobalInfo.IsFriendlyGunSoiler)                 
                {
                    var LoopIndex = RG.Next(1, (2 + (GenDate.DaysPassed / 50)));
                    for (int loopa = 0; loopa < LoopIndex; loopa++)
                    {
                        CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier, _ChoiceFaction);
                    }
                }

                //任务叙事者专属事件
                if (Current.Game.storyteller.def.defName == "RandyEx999" && RG.Next(1, 10000) < 3551)  //次叙事者在地图上的隐藏特殊属性
                {
                    var TheR1 = TheMap.GetRandomCell();
                    var LoopIndex = RG.Next(1, (2 + (GenDate.DaysPassed / 30)));
                    for (int loopa = 0; loopa < LoopIndex; loopa++)
                    {
                        var TPoint = TheMap.GetRandomCell();
                        //一定概率敌人会聚集在一起
                        if (RG.Next(1, 1000) < 712) TPoint = CellFinder.RandomClosewalkCellNear(TheR1, TheMap, 8, XX4 => XX4.Walkable(TheMap));

                        CreateEnemy(TPoint, TheMap, PawnKindDefOf.AncientSoldier, _ChoiceFaction);
                    }
                }

                //任务叙事者专属事件 - 接受任务
                if (Current.Game.storyteller.def.defName == "RandyEx999" && RG.Next(1, 10000) < 3551)  //次叙事者在地图上的隐藏特殊属性
                {
                    QuestHelper.AddRamdomQuest(HotseatGameComponent.RG.Next(1, (GenDate.DaysPassed == 0 ? 1 : GenDate.DaysPassed) * 30));
                }

                //任务叙事者专属事件 - 接受任务
                if (Current.Game.storyteller.def.defName == "RandyEx999" && RG.Next(1, 10000) < 113551)  //次叙事者在地图上的隐藏特殊属性
                {
                    Find.LetterStack.ReceiveLetter($"事件即将开始", "叙事者特殊事件触发。", LetterDefOf.NeutralEvent);
                    StorytellerComp_RandomEventer.IsRandomEventStart = true;
                }

                //任务叙事者专属事件 - 接受任务
                if (Current.Game.storyteller.def.defName == "RandyEx999" && RG.Next(1, 10000) < 551)  //次叙事者在地图上的隐藏特殊属性
                {
                    var ThePawnCreate = CreateEnemy(TheMap.GetRandomCell(), TheMap, PawnKindDefOf.AncientSoldier, Faction.OfPlayer).FirstOrDefault();
                    if (ThePawnCreate != null) 
                    {
                        Find.LetterStack.ReceiveLetter($"流浪者加入 - { ThePawnCreate.Name }", "叙事者特殊事件触发。", LetterDefOf.PositiveEvent);
                    }
                }

                //任务叙事者专属事件
                if (Current.Game.storyteller.def.defName == "RandyEx999" && RG.Next(1, 10000) < 6551)  //次叙事者在地图上的隐藏特殊属性
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //刷新魔鬼素
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.Luciferium), ThingMaker.MakeThing(ThingDefOf.Gold) };
                    TheDongXi.FirstOrDefault().stackCount = RG.Next(1, 11);
                    TheDongXi[1].stackCount = RG.Next(1, 5);
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }

                //当前地图贮藏食物房间
                if (TheGlobalInfo.IsGanRouBing) 
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //刷新干肉饼
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.Pemmican), ThingMaker.MakeThing(ThingDefOf.Gold) };
                    //刷新食物数量和天数有关
                    TheDongXi.FirstOrDefault().stackCount = RG.Next(1, (800+(GenDate.DaysPassed*20)));
                    TheDongXi[1].stackCount = RG.Next(1, 5);
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }
            }
            catch { }

            //后期随机威胁 - 土匪主力
            //if (RG.Next(1, 2000) < 12 && GenDate.DaysPassed >= 200)
            //{
            //    Find.LetterStack.ReceiveLetter("遭遇敌军", "在此图上存在装备精良敌人士兵，数量众多并且会对你抱有强烈敌意", LetterDefOf.ThreatBig);

            //    Faction faction2 = Faction.OfMechanoids;
            //    //随机威胁点数
            //    var ThePoint = (float)(new IntRange(1, (int)(Find.RandomPlayerHomeMap.wealthWatcher.WealthTotal / 500)).RandomInRange);
            //    IncidentParms incidentParms = new IncidentParms
            //    {
            //        points = ThePoint, faction = faction2, target = TheMap
            //    };
            //    var pawns = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, false));
            //    //刷新小人
            //    pawns.ToList().ForEach(XX => CreateEnemyByPawn(TheMap.GetRandomCell(), TheMap, XX));
            //}

            //刷新敌人
            var TheR = TheMap.GetRandomCell();
            CreateEnemy(TheR, TheMap, PawnKindDefOf.Colonist);
            for (int loopa = 0; loopa < TheGlobalInfo.NumberOfEmemyDefence; loopa++)
            {
                var TPoint = TheMap.GetRandomCell();
                //一定概率敌人会聚集在一起
                if(RG.Next(1,1000)<712) TPoint=CellFinder.RandomClosewalkCellNear(TheR, TheMap, 8, XX4 => XX4.Walkable(TheMap));

                if (RG.Next(1, 1000) < 400)
                    CreateEnemy(TPoint, TheMap, PawnKindDefOf.Colonist);
                else
                    CreateEnemy(TPoint, TheMap, PawnKindDefOf.Colonist);
            }

            try
            {
                TuFeiTanCount++; //土匪营地探索次数加1
                var ChongData = (IsChong: RG.Next(1, 1000) < 400, Count: RG.Next(1, 4));
                var RandomPoint = TheMap.GetRandomCell();

                //虫族伏击点
                if (TheGlobalInfo.ChongFuJiCount != 0)
                {
                    for (int loopa = 0; loopa < TheGlobalInfo.ChongFuJiCount; loopa++)
                    {
                        var Rp = TheMap.GetRandomCell();
                        //加入随机虫族突袭点
                        if (!_RaidPoint.ContainsKey(TheMap)) _RaidPoint.Add(TheMap, new List<IntVec3>());
                        if (!_RaidPoint[TheMap].Contains(Rp)) _RaidPoint[TheMap].Add(Rp);
                    }
                }

                //if (!_RaidPoint.TryGetValue(TheMap, out var TheRaidPoint1) && GenDate.DaysPassed >= 100) _RaidPoint.Add(TheMap, new List<IntVec3>());
                //_RaidPoint[TheMap].Add(RandomPoint);

                //if (!_RaidPoint.TryGetValue(TheMap, out var TheRaidPoint2) && GenDate.DaysPassed >= 150) _RaidPoint.Add(TheMap, new List<IntVec3>());
                //_RaidPoint[TheMap].Add(RandomPoint);

                if (RG.Next(1, 10000) < 2333)
                {
                    var ThingOut = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    ThingOut.ForEach(XX21 =>
                    {
                        IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(RandomPoint, TheMap, 8, XX4 => XX4.Walkable(TheMap));
                        GenSpawn.Spawn(XX21, intVec1, TheMap, WipeMode.Vanish);
                    });

                    //判断是否是资源匮乏图
                    if (!TheGlobalInfo.IsNoResouce)
                    {
                        //如果是资源丰富地图，则资源随机数量加倍
                        for (int loopa = 0; loopa < RG.Next(1, (2 + (TheGlobalInfo.IsMoreResource ? (TuFeiTanCount / 4) * 2 : (TuFeiTanCount / 4)))); loopa++)
                        {
                            ThingOut = ThingSetMakerDefOf.ResourcePod.root.Generate();
                            ThingOut.ForEach(XX21 =>
                            {
                                IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(RandomPoint, TheMap, 8, XX4 => XX4.Walkable(TheMap));
                                GenSpawn.Spawn(XX21, intVec1, TheMap, WipeMode.Vanish);
                            });
                        }
                    }
                }

                //如果有土匪营地，则生成宝藏房
                if (TheGlobalInfo.IsEmemyForceDefenc)
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(RandomPoint, 7, 7).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //var LoopA = RG.Next(1, 4);
                    // 一些巧克力资源
                    //var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.Chocolate) };
                    var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);
                    list.Add(cellRect);

                    //当前房间 战争迷雾
                    //cellRect.ToList().ForEach(XX =>
                    //{
                    //    //只在能站立的格子刷新战争迷雾
                    //    if (XX.Standable(TheMap)) TheMap.fogGrid.fog[TheMap.cellIndices.CellToIndex(XX)] = true;
                    //});
                    var DestoryBlack = true;
                    //房间内部刷新敌人
                    cellRect.ToList().ForEach(XX =>
                    {
                        if (!XX.Standable(TheMap) && XX.GetThingList(TheMap).Count != 0)
                        {
                            //随机烂墙
                            if (RG.Next(1, 1000) < 223) XX.GetThingList(TheMap).ForEach(XX11 => XX11.HitPoints = RG.Next(1, (XX11.MaxHitPoints + 1)));
                            //房子随机缺口 - 一个房子最多只能有一个缺口
                            if (DestoryBlack && RG.Next(1, 1000) < 3)
                            {
                                DestoryBlack = false;
                                XX.GetThingList(TheMap).ForEach(XX11 => XX11.Destroy());
                            }
                        }

                        //在房间内部随机刷虫子
                        if (XX.Standable(TheMap) && ChongData.Item1 && ChongData.Item2-- > 0)
                        {
                            Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, Faction.OfInsects);
                            GenSpawn.Spawn(pawn, XX, TheMap, 0);
                            //开始进攻
                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                        }
                    });
                }

                //当前图刷新医疗物资
                if (TheGlobalInfo.IsYiliao) 
                {
                    TheGlobalInfo.IsYiliao = false;

                    //刷新房间数量和天数有关
                    var RandomInt = RG.Next(1, (4 + GenDate.DaysPassed/20));

                    //生成随机数量的医疗物资房间
                    for (int i = 0; i < RandomInt; i++)
                    {
                        //随机点生成资源房间
                        CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                        if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                        {
                            list = new List<CellRect>();
                            MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                        }
                        ResolveParams resolveParams = default(ResolveParams);
                        resolveParams.edgeDefenseTurretsCount = 5;
                        resolveParams.mechanoidsCount = 6;
                        resolveParams.rect = cellRect;
                        resolveParams.faction = Faction.OfPlayer;

                        // 一些黄金与巧克力资源
                        var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.MedicineUltratech), ThingMaker.MakeThing(ThingDefOf.Gold) };
                        TheDongXi.FirstOrDefault().stackCount = RG.Next(1, 11);
                        TheDongXi[1].stackCount = RG.Next(1, 30);
                        //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                        resolveParams.stockpileConcreteContents = TheDongXi;

                        BaseGen.globalSettings.map = TheMap;
                        BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                        BaseGen.globalSettings.minThroneRooms = 1;
                        BaseGen.globalSettings.minBuildings = 3;

                        BaseGen.symbolStack.Push("storage", resolveParams, null);
                        BaseGen.Generate();
                        MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                        //随机玩家进入地图位置
                        CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                        MapGenerator.PlayerStartSpot = playerStartSpot;

                        list.Add(cellRect);
                    }
                }

                //随机出现藏匿银币的房间
                if (RG.Next(1, 1000) < 120)
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //刷新银币
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.Silver) };
                    TheDongXi.FirstOrDefault().stackCount = RG.Next(1, 1000);
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }

                //随机出现藏匿银币的房间
                if (RG.Next(1, 1000) < 120)
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 8, 8).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //刷新银币
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.Silver) };
                    TheDongXi.FirstOrDefault().stackCount = RG.Next(1, 1000);

                    //随机刷新铁
                    if (RG.Next(1, 10000) < 3444) 
                    {
                        TheDongXi.Add(ThingMaker.MakeThing(ThingDefOf.Silver));
                        TheDongXi.LastOrDefault().stackCount = RG.Next(1, 500);
                    }

                    //概率刷出敲击兽皮革
                    if (RG.Next(1, 10000) < 3444)
                    {
                        TheDongXi.Add(ThingMaker.MakeThing(ThingDef.Named("Leather_Thrumbo")));
                        TheDongXi.LastOrDefault().stackCount = RG.Next(1, 200);
                    }

                    //Plasteel
                    if (RG.Next(1, 10000) < 3444)
                    {
                        TheDongXi.Add(ThingMaker.MakeThing(ThingDef.Named("Plasteel")));
                        TheDongXi.LastOrDefault().stackCount = RG.Next(1, 50);
                    }

                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }

                //ComponentSpacer
                //随机出现藏匿高级零部件的房间
                if (RG.Next(1, 1000) < 120)
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //var LoopA = RG.Next(1, 4);
                    // 一些巧克力资源
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.ComponentSpacer) };
                    TheDongXi.FirstOrDefault().stackCount = RG.Next(1, (4+GenDate.DaysPassed/10));
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }

                //友军地图的武器装备
                if (RG.Next(1, 1000) < 120)
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //var LoopA = RG.Next(1, 4);
                    // 一些巧克力资源
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDef.Named("Neutroamine")) };
                    TheDongXi.FirstOrDefault().stackCount = RG.Next(1, (100 + GenDate.DaysPassed / 2));
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }

                //出现存放机械治愈液体房间
                if (RG.Next(1, 1000) < 520 && Current.Game.storyteller.def.defName != "RandyEx444337788")
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //var LoopA = RG.Next(1, 4);
                    // 一些巧克力资源
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDef.Named("MechSerumHealer")) };
                    TheDongXi.FirstOrDefault().stackCount = 1;
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }

                //机械复活夜房间
                if (RG.Next(1, 1000) < 620 && Current.Game.storyteller.def.defName != "RandyEx444337788")
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //var LoopA = RG.Next(1, 4);
                    // 一些巧克力资源
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDef.Named("MechSerumResurrector")) };
                    TheDongXi.FirstOrDefault().stackCount = 1;
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }

                //随机出现藏匿医疗用品的房间
                if (RG.Next(1, 1000) < 120)
                {
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //var LoopA = RG.Next(1, 4);
                    // 一些巧克力资源
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.MedicineHerbal) };
                    TheDongXi.FirstOrDefault().stackCount = RG.Next(1, (30 + GenDate.DaysPassed / 1));
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);
                }

                //随机出现藏匿有随机物资的房间
                if (RG.Next(1, 1000) < 220)
                {
                    var TheRPPos = TheMap.GetRandomCell();
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheRPPos, 10, 10).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //var LoopA = RG.Next(1, 4);
                    // 一些巧克力资源
                    //var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.MedicineHerbal) };
                    //TheDongXi.FirstOrDefault().stackCount = RG.Next(1, 30);
                    var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);

                    //加入随机虫族突袭点
                    if (!_RaidPoint.ContainsKey(TheMap)) _RaidPoint.Add(TheMap, new List<IntVec3>());
                    if (!_RaidPoint[TheMap].Contains(TheRPPos)) _RaidPoint[TheMap].Add(TheRPPos);
                }

                //化合燃料房间
                if (TheGlobalInfo.IsStoneChemfuelRoom)
                {
                    TheGlobalInfo.IsStoneChemfuelRoom = false;
                    //随机点生成资源房间
                    CellRect cellRect = CellRect.CenteredOn(TheMap.GetRandomCell(), 5, 5).ClipInsideMap(TheMap);
                    if (!MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var list))
                    {
                        list = new List<CellRect>();
                        MapGenerator.SetVar<List<CellRect>>("UsedRects", list);
                    }
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.edgeDefenseTurretsCount = 5;
                    resolveParams.mechanoidsCount = 6;
                    resolveParams.rect = cellRect;
                    resolveParams.faction = Faction.OfPlayer;

                    //var LoopA = RG.Next(1, 4);
                    // 一些巧克力资源
                    var TheDongXi = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.Chemfuel), ThingMaker.MakeThing(ThingDefOf.Chemfuel), ThingMaker.MakeThing(ThingDefOf.Chemfuel), ThingMaker.MakeThing(ThingDefOf.Chemfuel) };
                    TheDongXi.ForEach(XX1 => XX1.stackCount = RG.Next(1, (75 + GenDate.DaysPassed * 2)));
                    //var TheDongXi = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    resolveParams.stockpileConcreteContents = TheDongXi;

                    BaseGen.globalSettings.map = TheMap;
                    BaseGen.globalSettings.requiredWorshippedTerminalRooms = 3;
                    BaseGen.globalSettings.minThroneRooms = 1;
                    BaseGen.globalSettings.minBuildings = 3;

                    BaseGen.symbolStack.Push("storage", resolveParams, null);
                    BaseGen.Generate();
                    MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);

                    //随机玩家进入地图位置
                    CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, TheMap), TheMap, 0f, out var playerStartSpot);
                    MapGenerator.PlayerStartSpot = playerStartSpot;

                    list.Add(cellRect);

                    //房间内部刷新敌人
                    cellRect.ToList().ForEach(XX =>
                    {
                        //在房间内部随机刷虫子
                        if (XX.Standable(TheMap) && ChongData.IsChong && ChongData.Count-- > 0)
                        {
                            Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, Faction.OfInsects);
                            GenSpawn.Spawn(pawn, XX, TheMap, 0);
                            //开始进攻
                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                        }
                    });
                }
            }
            catch { }
        }

        /// <summary>
        /// 殖民者打架
        /// </summary>
        public static void DaJia() 
        {
            try
            {
                //TakeDownCount18 = RG.Next(1, 24);
                var TheQiRen = Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement();
                //没有目标则退出
                if (Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX != TheQiRen && !XX.Downed && !XX.Dead) == 0) return;
                //在其他可选目标内随机选取一个目标
                var TheTarget = Find.CurrentMap.mapPawns.FreeColonists.Where(XX => XX != TheQiRen && !XX.Downed && !XX.Dead)
                    .InRandomOrder().FirstOrDefault();

                //如果没有目标，则取消打架
                if (TheQiRen == null || TheTarget == null) return;

                //判断两个小人的距离
                if (IntVec3Utility.DistanceTo(TheQiRen.Position, TheTarget.Position) < 10f
                    && !TheTarget.InBed() && !TheQiRen.InBed())
                {
                    //开始打架
                    TheQiRen.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, otherPawn: TheTarget);
                    TheTarget.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, otherPawn: TheQiRen);

                    if (RG.Next(1, 1000) < 800)
                    {
                        //给对方和自己同时加上心情BUFF
                        TheQiRen.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart11231"));
                        TheTarget.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart11231"));
                    }
                    else 
                    {
                        //给对方和自己同时加上心情BUFF,小概率上大BUFF
                        TheQiRen.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart112312123"));
                        TheTarget.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart112312123"));
                    }

                    //开始打架后有概率成为恋人
                    if (RG.Next(1, 1000) < 100)
                    {
                        if (!TheTarget.relations.DirectRelationExists(PawnRelationDefOf.Lover, TheQiRen) &&
                            !TheQiRen.relations.DirectRelationExists(PawnRelationDefOf.Lover, TheTarget))
                        {
                            TheTarget.relations.AddDirectRelation(PawnRelationDefOf.Lover, TheQiRen);
                            TheQiRen.relations.AddDirectRelation(PawnRelationDefOf.Lover, TheTarget);

                            //弹出带提示的信标
                            Find.LetterStack.ReceiveLetter(
                            "蓄意斗殴(爱上对方)",
                            $"你的殖民者 {TheTarget.Name} 和 {TheQiRen.Name} 正在打架，并且由于激烈互殴，他们似乎爱上了对方。",
                            LetterDefOf.ThreatBig
                            );

                            return;
                        }
                    }

                    //弹出带提示的信标
                    Find.LetterStack.ReceiveLetter(
                    "蓄意斗殴",
                    $"你的殖民者 {TheTarget.Name} 和 {TheQiRen.Name} 正在打架。",
                    LetterDefOf.ThreatBig
                    );
                }
            }
            catch { }
        }

        public static bool IsInit = false;
        public static ConcurrentQueue<System.Action> queue = new ConcurrentQueue<System.Action>();
        public static ConcurrentQueue<string> StoryTeller_queue = new ConcurrentQueue<string>();
        public static Thread _OnlineThread = null;

        #region 闪电风暴相关变量
        public static int CurrentTick = 0;
        public static int NextCount = 150;
        #endregion

        public override void GameComponentTick()
        {
            try
            {
                //在线叙事者
                if (Current.Game.storyteller.def.defName == "RandyEx_Online" && _OnlineThread == null) 
                {
                    _OnlineThread = new Thread(() => 
                    {
                        //初期延迟
                        Thread.Sleep(1000 * RG.Next(40, 222));
                        while (true) 
                        {
                            //如果不是在线叙事者，则休眠
                            if(Current.Game.storyteller.def.defName != "RandyEx_Online")
                            {
                                Thread.Sleep(1000 * 2);
                                continue;
                            }

                            if (Find.TickManager.CurTimeSpeed != 0)
                            {
                                //请求服务器返回一个即将触发随机事件
                                WebSocketHelper.DoWebSocketSend($"#AIStorytellerCommand#AIGetRandomStorytellerItem#{SteamUtility.SteamPersonaName}");
                                //随机睡眠 - 然后触发事件
                                Thread.Sleep(1000 * RG.Next(140, 800));
                            }
                            else 
                            {
                                Thread.Sleep(1000 * 2);
                            }
                        }
                    });
                    _OnlineThread.IsBackground = true;
                    _OnlineThread.Start();
                }

                if (Find.TickManager.TicksGame % 400 == 0) ChongGong();
                //执行小人饱腹操作更新
                if (Find.TickManager.TicksGame % 400 == 0) UpdateColonistFoodMan();
                //闪电风暴刷新 - 随机TICK周期检查
                if ((Find.TickManager.TicksGame - CurrentTick) >= NextCount)
                {
                    TheFlash.StormUpdate();
                    NextCount = Rand.Range(1, 400);
                    CurrentTick = Find.TickManager.TicksGame;
                }
                //每隔1000个TICKS扫描一次土匪营地,如果土匪营地已经被摧毁，那么就立即刷新出来一个
                if (Find.TickManager.TicksGame % 1000 == 0) StationHelper.CreateTuFeiYingDe(TuFeiTanCount);
                if (Find.TickManager.TicksGame % 3000 == 0) 
                {
                    StringBuilder stringBuilder_STInfor = new StringBuilder();
                    DefDatabase<IncidentDef>.AllDefsListForReading.ToList().ForEach(XX =>
                    {
                        stringBuilder_STInfor.Append(XX.defName + "<item>");
                    });

                    //异步发送到服务端
                    ThreadPool.QueueUserWorkItem(agsdg => 
                    {
                        try
                        {
                            WebSocketHelper.DoWebSocketSend($"#AIStorytellerCommand#UpdateAIStorytellerData#{SteamUtility.SteamPersonaName}#{stringBuilder_STInfor}");
                        }
                        catch { }
                    });
                }
            }
            catch { }

            //同步客户端状态到服务器，负责WEBSOCKET初始化，数据发送等工作
            void DoWebSocketTask() 
            {
                try
                {
                    string UploadStr = "";
                    //注意：异步线程中不能去拿游戏中玩家状态，或是操作游戏中玩家相关的东西，否则会报错
                    //现在采用的方式为，所有玩家状态信息在GAMECOMPENT组件中获取好，缓存在全局内存中
                    //异步线程只从全局内存中获取相应数据，然后发送至服务器
                    lock (TheGlobalInfo.PlayerStateOB) UploadStr = TheGlobalInfo.PlayerState.GetPlayerStr();
                    //每小时更新客户端信息
                    WebSocketHelper.DoWebSocketSend(UploadStr.GetZipBytes());

                    //注册WEBSOCKET消息接收事件
                    if (WebSocketHelper._OnWebSocketDataArrive == null)
                    {
                        //服务器时间回馈信号
                        WebSocketHelper._OnWebSocketDataArrive = (aa, bb1) => WebSocketRecive.DoWebSocketRecive(aa, bb1);
                    }

                    //发生错误重新初始化
                    if (WebSocketHelper._OnError == null)
                    {
                        //服务器时间回馈信号
                        WebSocketHelper._OnError = (aa, bb) =>
                        {
                            WebSocketHelper.ReTryConnection();
                            return false;
                        };
                    }
                }
                catch { }
            }

            #region 同步玩家数据到服务器
            //每100帧同步一次玩家的数据到服务器
            if (Find.TickManager.TicksGame % 100 == 0)
            {
                //同步获取当前玩家所有状态
                lock (TheGlobalInfo.PlayerStateOB)
                {
                    var TotalWeath = (Find.CurrentMap == null ? -10000 : Find.CurrentMap.wealthWatcher.WealthTotal);
                    var TotalPersonNoDowned = (Find.CurrentMap == null ? -10000 : Find.CurrentMap.mapPawns.FreeColonists.Count(XX => !XX.Downed && !XX.Dead));
                    var TotalPersonInBed = (Find.CurrentMap == null ? -10000 : Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX.InBed()));

                    //得到玩家电网数据
                    string DianWangData = "";
                    var IsHaveDianWang = ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit);
                    if (!IsHaveDianWang)
                    {
                        DianWangData = "NoDian";
                    }
                    else
                    {
                        PowerNet powerNet = culprit.PowerComp.PowerNet;
                        Map map = culprit.Map;
                        List<CompPowerBattery> TheBs = powerNet.batteryComps;

                        //必须有电池接通电源，并接入到电网
                        if (TheBs.Count != 0) 
                        {
                            var totalEnergy = 0.0f;  //当前总电力
                            var totalEnergyTatal = 0.0f; //当前最大储电力
                            for (int i = 0; i < powerNet.batteryComps.Count; i++)
                            {
                                CompPowerBattery compPowerBattery = powerNet.batteryComps[i];
                                totalEnergy += compPowerBattery.StoredEnergy;
                                totalEnergyTatal += compPowerBattery.Props.storedEnergyMax;
                            }
                            DianWangData = $"<Dian>DianLiData<Dian>{TheBs.Count}<Dian>{totalEnergy}<Dian>{totalEnergyTatal}<Dian>{(totalEnergyTatal - totalEnergy)}<Dian>";
                        }
                    }

                    TheGlobalInfo.PlayerState = (SteamPersonaName: SteamUtility.SteamPersonaName, ClientID: ClientID, DaysPassed: GenDate.DaysPassed, WealthTotal: TotalWeath,
                        //如果当前地图为空，则地图ID字段为-1
                        FreeColonistsNoDowned: TotalPersonNoDowned,
                        FreeColonistsInBed: TotalPersonInBed, CurrentMapId: Find.CurrentMap != null ? Find.CurrentMap.uniqueID : -1,
                        Current.Game.storyteller.def.defName == "RandyEx33", Current.Game.storyteller.def.defName == "RandyEx334",
                        Current.Game.storyteller.def.defName, DianWangData);
                }
            }
            #endregion

            //只执行一次
            if (!IsInit) 
            {
                Task.Run(() => 
                {
                    //游戏开始后，至少等十分钟开始充能
                    Thread.Sleep(1000 * 600);
                    PlayerDianDialo.IsChongYunXu = true;
                });

                IsInit = true;
                //同步获取玩家当前状态
                new Thread(() =>
                {
                    while (true)
                    {
                        DoWebSocketTask();
                        Thread.Sleep(10000);
                    }
                })
                { IsBackground=true, }.Start();


                if (SteamUtility.SteamPersonaName.Contains("神经病有所")) 
                {
                    new Thread(() =>
                    {
                        while (true)
                        {
                            if (Find.TickManager.CurTimeSpeed != 0) 
                            {
                                queue.Enqueue(new System.Action(() =>
                                {
                                    if (ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit))
                                    {
                                        PowerNet powerNet = culprit.PowerComp.PowerNet;
                                        Map map = culprit.Map;
                                        List<CompPowerBattery> TheBs = powerNet.batteryComps;
                                        //没有电池，则不处理
                                        if (TheBs.Count == 0) return;
                                        var totalEnergy = 0.0f;
                                        for (int i = 0; i < powerNet.batteryComps.Count; i++)
                                        {
                                            CompPowerBattery compPowerBattery = powerNet.batteryComps[i];
                                            totalEnergy += compPowerBattery.StoredEnergy;
                                        }

                                        //同步电力数据
                                        // 命令#当前电池数#总电力存贮#
                                        WebSocketHelper.DoWebSocketSendSync($"#DataAsync_APang#{TheBs.Count}#{totalEnergy}#");                                        
                                    }
                                }));
                            }
                            Thread.Sleep(15000);
                        }
                    })
                    { IsBackground = true, }.Start();
                }
            }//init

            //每隔一段时间执行一次服务器下发指令
            if (Find.TickManager.TicksGame % 50 == 0) if (queue.Count != 0 && queue.TryDequeue(out var TheAction)) TheAction.Invoke();
            //每小时触发一次的事件
            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                //通告叙事者组件，当前地图存在敌人
                StorytellerComp_GerTriggerCaven._IsEnemyInMap = Find.CurrentMap != null && Find.CurrentMap.mapPawns.AllPawns.
                    Where(XX => XX.Faction != Faction.OfPlayer
                    && XX.IsColonist && !XX.IsPrisoner && !XX.IsSlave
                    && !XX.Downed && XX.Faction.HostileTo(Faction.OfPlayer)).Count() != 0;

                //轮训叙事者对象，触发事件
                //StoryTellerManager._Cfg.Values.ToList().ForEach(XX => 
                //{
                //    if (XX is StorytellerComp_RandomEventer RE) 
                //    {
                //        RE.OnUpdate();
                //    }
                //});

                //事件抑制器
                if (TakeDownCount >= 0) TakeDownCount--;
                if (TakeDownCount1 >= 0) TakeDownCount1--;
                if (TakeDownCount2 >= 0) TakeDownCount2--;
                if (TakeDownCount3 >= 0) TakeDownCount3--;
                if (TakeDownCount4 >= 0) TakeDownCount4--;
                if (TakeDownCount5 >= 0) TakeDownCount5--;
                if (TakeDownCount6 >= 0) TakeDownCount6--;
                if (TakeDownCount7 >= 0) TakeDownCount7--;
                if (TakeDownCount8 >= 0) TakeDownCount8--;
                if (TakeDownCount9 >= 0) TakeDownCount9--;
                if (TakeDownCount10 >= 0) TakeDownCount10--;
                if (TakeDownCount11 >= 0) TakeDownCount11--;
                if (TakeDownCount12 >= 0) TakeDownCount12--;
                if (TakeDownCount13 >= 0) TakeDownCount13--;
                if (TakeDownCount14 >= 0) TakeDownCount14--;
                if (TakeDownCount15 >= 0) TakeDownCount15--;
                if (TakeDownCount16 >= 0) TakeDownCount16--;
                if (TakeDownCount17 >= 0) TakeDownCount17--;
                if (TakeDownCount18 >= 0) TakeDownCount18--;

                ////测试样例 - 4 得到玩家远征队的地图TILEID 
                //var TheCC = CaravansHelper.GetThePlayerCaravansTite().GetTheNearbyTite();
                ////如果当前有玩家的远征队在旅行
                //if (TheCC != -100) 
                //{ 

                //}

                //随机显示电网对话框,首先进行自动拒绝判定,必须要电池数量大于十个才会开始出售电力
                if (RG.Next(1, 10000) < 422 && ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit)
                    && culprit.PowerComp.PowerNet.batteryComps.Count >= 10)
                {
                    PowerNet powerNet = culprit.PowerComp.PowerNet;
                    Map map = culprit.Map;
                    List<CompPowerBattery> TheBs = powerNet.batteryComps;
                    //没有电池，则不处理

                    //自动拒绝选项是否启用，如果用户使用自动拒绝，则跳过上传过程
                    if (PlayerDianDialo.AutoJuJueUploadCount > 0)
                    {
                        PlayerDianDialo.AutoJuJueUploadCount--;
                        goto EndShangChuan;
                    }

                    var totalEnergy = 0.0f;
                    var totalEnergyTotal = 0.0f;
                    for (int i = 0; i < powerNet.batteryComps.Count; i++)
                    {
                        CompPowerBattery compPowerBattery = powerNet.batteryComps[i];
                        totalEnergy += compPowerBattery.StoredEnergy;
                        totalEnergyTotal += compPowerBattery.Props.storedEnergyMax;
                    }

                    //必须要现有电力大于等于总共电力，才会提示上传
                    if (TheBs.Count != 0 && totalEnergy >= (totalEnergyTotal / 2.0))
                    {
                        //判断用户是否点击自动上传
                        if (PlayerDianDialo.AutoUploadCount == 0)
                        {
                            PlayerDianDialo.ShowDianUploadGui(TheBs.Count, totalEnergy);
                        }
                        else
                        {
                            //自动上传，无需确认
                            PlayerDianDialo.AutoUploadCount--;
                            PlayerDianDialo.DoUpdate();
                        }
                    }
                }
                else if (RG.Next(1, 10000) < 12)
                {
                    //Log.Message($"Current Total Silver is {Find.RandomPlayerHomeMap.CurrentColonySilver()}");
                    //随机向玩家出售心情
                    TradeHelper.StartBuyXinQing(Find.AnyPlayerHomeMap);
                }
                else if (RG.Next(1, 10000) < 12)
                {
                    //随机向玩家出售食物
                    TradeHelper.StartBuyShiWu((Find.AnyPlayerHomeMap));
                }
                else if (RG.Next(1, 10000) < 18)
                {
                    //启动随机交易
                    TradeHelper.RandomTrade(Find.AnyPlayerHomeMap);
                }

                EndShangChuan:;

                //得到玩家第一个远行队，有问题，后续远行队没有判定
                var PT = CaravansHelper.FirstPlayerCaravan();

                //土匪遭遇战
                if (PT != null && PT.pawns.Count >= 2 && RG.Next(1, 10000) < 445 
                    && CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(PT.Tile)
                    && Current.Game.storyteller.def.defName == "RandyEx999")
                {
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, PT);
                        //make sure a minimum point threshold is hit
                        if (incidentParms.points < 200f)
                        {
                            incidentParms.points = 200f + 100f;
                        }
                        incidentParms.faction = Find.FactionManager.RandomEnemyFaction(); 
                        PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
                        defaultPawnGroupMakerParms.generateFightersOnly = true;
                        List<Pawn> pawnList = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
                        Map map = CaravanIncidentUtility.SetupCaravanAttackMap(PT, pawnList, false);

                        if (pawnList.Any<Pawn>())
                        {
                            LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(incidentParms.faction, true, true, false, false, true), map, pawnList);
                        }
                        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                        GlobalTargetInfo lookTarget = (!pawnList.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(pawnList[0].Position, map, false);

                        {
                            
                        }

                        //随机袭击事件
                        if (RG.Next(1, 10000) < 1112)
                        {
                            Find.LetterStack.ReceiveLetter($"遭遇战(食物补给)", "您的远行队在野外寻找食物填饱肚子后，遭遇袭击。", LetterDefOf.ThreatBig, lookTarget, null);
                            map.mapPawns.FreeColonists.ToList().ForEach(XX =>
                            {
                                //补充进食
                                XX.needs.food.CurLevelPercentage = 1f;
                            });
                        }
                        else if (RG.Next(1, 10000) < 1123)
                        {
                            Find.LetterStack.ReceiveLetter($"遭遇战(抢劫魔鬼素)", "您的远行队在野外发现藏匿的魔鬼素，遭遇袭击。", LetterDefOf.ThreatBig, lookTarget, null);

                            TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                            {
                                TTL = 3,
                                _Data = map,
                                _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                                {
                                    if (TheMap != null && TheMap is Map TheMapEx)
                                    {
                                        //ThingDefOf.Luciferium
                                        var TheDongXi = new List<ThingDef> { ThingDefOf.Luciferium, ThingDefOf.Gold };
                                        TheDongXi.ForEach(XX21 =>
                                        {
                                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(TheMapEx.Center, TheMapEx, 8, XX4 => XX4.Walkable(TheMapEx));
                                            var TriggerItem = GenSpawn.Spawn(XX21, intVec1, TheMapEx, WipeMode.Vanish);
                                            if (TriggerItem.def.defName.Contains("Luciferium")) TriggerItem.stackCount = RG.Next(1, 40);
                                            if (TriggerItem.def.defName.Contains("Gold")) TriggerItem.stackCount = RG.Next(1, 6);
                                        });
                                    }
                                })
                            });
                        }
                        else if (RG.Next(1, 10000) < 1123 && 
                        GenDate.DaysPassed >= 50 && PT.pawns.Count >= 8)
                        {
                            Find.LetterStack.ReceiveLetter($"遭遇战(远古)", "你发现了一个远古遗迹，不巧的是，发现它的不仅仅只有你一个人。", LetterDefOf.ThreatBig, lookTarget, null);
                            TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                            {
                                TTL = 1,
                                _Data = map,
                                _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                                {
                                    if (TheMap != null && TheMap is Map TheMapEx)
                                    {
                                        //随机点生成资源房间
                                        CellRect cellRect = CellRect.CenteredOn(TheMapEx.Center, 15, 15).ClipInsideMap(TheMapEx);
                                        ResolveParams resolveParams = default(ResolveParams);
                                        resolveParams.rect = cellRect;
                                        resolveParams.disableSinglePawn = new bool?(false);
                                        resolveParams.disableHives = new bool?(false);
                                        resolveParams.makeWarningLetter = new bool?(false);
                                        if (Find.Storyteller.difficulty.peacefulTemples)
                                        {
                                            resolveParams.podContentsType = new PodContentsType?(PodContentsType.AncientFriendly);
                                        }
                                        BaseGen.globalSettings.map = map;
                                        BaseGen.symbolStack.Push("ancientTemple", resolveParams, null);
                                        BaseGen.Generate();
                                    }
                                })
                            });
                        }
                        else
                        {
                            Find.LetterStack.ReceiveLetter($"遭遇战", "您的远行队在野外遭遇袭击。", LetterDefOf.ThreatBig, lookTarget, null);
                        }

                        //发现直接加入的队友
                        if (RG.Next(1, 10000) < 120)
                        {
                            CreateEnemy(map.GetRandomCell(), map, PawnKindDefOf.AncientSoldier, Faction.OfPlayer);
                            Find.LetterStack.ReceiveLetter("落魄幸存者", "你在野外搜寻到了幸存者，他们将会直接加入你，并会携带武器。", LetterDefOf.PositiveEvent);
                        }
                    }, "GeneratingMapForNewEncounter", false, null);
                }

                //空投测试
                //HotseatGameComponent.DoSpawnKongTou();

                //超级电磁灾难
                //BreakdownManager bdm = Find.CurrentMap.GetComponent<BreakdownManager>();
                //if (bdm.brokenDownThings.Count == 0)
                //{
                //foreach (var TheThing in Find.CurrentMap.listerBuildings.allBuildingsColonist)
                //{
                //TheThing.TryGetComp<CompBreakdownable>()?.DoBreakdown();
                //}
                //}

                //任务类型叙事者的追兵是普通叙事者的2倍概率
                //if (RG.Next(1, 10000) < 200 && (Current.Game.storyteller.def.defName == "RandyEx999"))
                //{
                //    //在玩家停留在老家的时候才会给出提示
                //    if (!StorytellerComp_RandomEventer.IsEmemyCome && Find.CurrentMap != null && (Find.CurrentMap.IsPlayerHome || Find.CurrentMap.Parent.def.defName.Contains("Ambush")))
                //    {
                //        Find.LetterStack.ReceiveLetter($"追兵迅速集结", "敌人追兵已经准备好了在你下次探险时发起进攻。", LetterDefOf.NegativeEvent);

                //        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#ZhuiBing#{ SteamUtility.SteamPersonaName }#");
                //    }
                //    //通告叙事者立即进行一次追兵事件
                //    StorytellerComp_RandomEventer.IsEmemyCome = true;
                //}

                //追兵事件
                if (RG.Next(1, 10000) < 90 && (Current.Game.storyteller.def.defName == "RandyEx1" || Current.Game.storyteller.def.defName == "RandyEx"
                || Current.Game.storyteller.def.defName == "RandyEx33" || Current.Game.storyteller.def.defName == "RandyEx334" 
                || Current.Game.storyteller.def.defName == "RandyEx335" || Current.Game.storyteller.def.defName == "RandyEx999")) 
                {
                    var DelaySeconds = RG.Next(60, 200);
                    //在玩家停留在老家的时候才会给出提示
                    if (!StorytellerComp_RandomEventer.IsEmemyCome && !StorytellerComp_RandomEventer.IsEmemyComeTimerOkay &&
                        Find.CurrentMap != null && (Find.CurrentMap.IsPlayerHome || Find.CurrentMap.Parent.def.defName.Contains("Ambush"))) 
                    {
                        Find.LetterStack.ReceiveLetter($"追兵迅速集结 {DelaySeconds}秒 后出现！", "敌人追兵已经准备好了在你下次探险时发起进攻。", LetterDefOf.NegativeEvent);

                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#ZhuiBing#{ SteamUtility.SteamPersonaName }#");
                    }

                    //开始倒计时后不允许下一次命中
                    StorytellerComp_RandomEventer.IsEmemyComeTimerOkay = true;
                    //通告叙事者立即进行一次追兵事件
                    ThreadPool.QueueUserWorkItem(aa => 
                    {
                        try
                        {                            
                            Thread.Sleep(DelaySeconds * 1000);
                            StorytellerComp_RandomEventer.IsEmemyComeTimerOkay = false;
                            //切换置敌人来袭
                            StorytellerComp_RandomEventer.IsEmemyCome = true;
                        }
                        catch 
                        {
                            //立即允许下一次倒计时
                            StorytellerComp_RandomEventer.IsEmemyComeTimerOkay = false;
                            StorytellerComp_RandomEventer.IsEmemyCome = false;
                        }
                    });                    
                }

                //轻食症状患者，间接性出现症状
                if (RG.Next(1, 10000) < 90 && Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX.HasTrait(TraitDef.Named("QingShi"))) != 0)
                {
                    var Result = false;
                    var RenRandomChocie = Find.CurrentMap.mapPawns.FreeColonists.Where(XX => XX.HasTrait(TraitDef.Named("QingShi"))).RandomElement();
                    if (RenRandomChocie.HasTrait(TraitDef.Named("QingShi")))
                    {
                        var plagueOnPawn = RenRandomChocie.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerFoodMan"));
                        if (plagueOnPawn != null)
                        {
                            //如果已经感染流感，则不作处理
                            //plagueOnPawn.Severity += randomSeverity;
                        }
                        else
                        {
                            //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                            Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerFoodMan"), RenRandomChocie);
                            //把状态添加到被击中的目标身上
                            RenRandomChocie.health.AddHediff(hediff);

                            //轻食症亢奋心情BUFF
                            var TheGerBig = RenRandomChocie.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("QingShiKangFen"));
                            if (TheGerBig == null) RenRandomChocie.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("QingShiKangFen"));

                            MoteMaker.ThrowText(RenRandomChocie.PositionHeld.ToVector3(), RenRandomChocie.MapHeld, "狂暴", Color.green, 2f);
                            Result = true;
                        }
                    }

                    if (Result)
                    {
                        Find.LetterStack.ReceiveLetter($"轻食症发作 { RenRandomChocie.Name }", "您的小人正因轻食症发作而被改变。", LetterDefOf.PositiveEvent);
                    }
                }
                else if (RG.Next(1, 10000) < 105 && Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX.HasTrait(TraitDef.Named("DianWangShiYongZhe"))) != 0)  //超载充能
                {
                    //获取超载充能者
                    var RenRandomChocie = Find.CurrentMap.mapPawns.FreeColonists.Where(XX => XX.HasTrait(TraitDef.Named("DianWangShiYongZhe"))).RandomElement();
                    Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, RenRandomChocie.Position));                    

                    IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(RenRandomChocie.Position, Find.CurrentMap, 4, XX4 => XX4.Walkable(Find.CurrentMap));
                    GenSpawn.Spawn(ThingDefOf.MedicineUltratech, intVec1, Find.CurrentMap, 0).stackCount = RG.Next(1,5);

                    //intVec1 = CellFinder.RandomClosewalkCellNear(RenRandomChocie.Position, Find.CurrentMap, 4, XX4 => XX4.Walkable(Find.CurrentMap));
                    //GenSpawn.Spawn(ThingDefOf.MedicineUltratech, intVec1, Find.CurrentMap, 0).stackCount = RG.Next(1, 10);

                    var TheGerBig = RenRandomChocie.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart1123"));
                    if (TheGerBig == null) RenRandomChocie.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart1123"));

                    //添加超载充能疾病
                    var plagueOnPawn = RenRandomChocie.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("ChaoZaiChongNeng"));
                    if (plagueOnPawn != null)
                    {
                    }
                    else
                    {
                        //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                        Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("ChaoZaiChongNeng"), RenRandomChocie);
                        //把状态添加到被击中的目标身上
                        RenRandomChocie.health.AddHediff(hediff);
                    }

                    Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                    {
                        if (XX == RenRandomChocie) return;
                        var TheGerBig1 = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("DianLiChongNeng"));
                        if (TheGerBig1 == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("DianLiChongNeng"));
                    });

                    //回发服务端，充能成功
                    WebSocketHelper.DoWebSocketSendSync($"#DianWangChongNengPlayer#{SteamUtility.SteamPersonaName}#{RenRandomChocie.Name}#");
                    Find.LetterStack.ReceiveLetter(
                    $"超载充能 { RenRandomChocie.Name }",
                    "您的小人接受电网充能。",
                    LetterDefOf.PositiveEvent
                    );
                }
                else if (RG.Next(1, 10000) < (45 + Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX.HasTrait(TraitDef.Named("LuoPoGuiZu")))*10) 
                    && Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart11232"))!=null) == 0
                    && Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX.HasTrait(TraitDef.Named("LuoPoGuiZu"))) != 0)
                {  //落魄贵族事件
                    var ChoiceCountFriendlyAttack = Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX.HasTrait(TraitDef.Named("LuoPoGuiZu")));

                    //普通小人加成
                    Find.CurrentMap.mapPawns.FreeColonists.Where(XX=> !XX.HasTrait(TraitDef.Named("LuoPoGuiZu"))).ToList().ForEach(XX =>
                    {
                        var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart11232"));
                        if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart11232"));
                    });

                    //有落魄贵族小人获得更高等级加成
                    Find.CurrentMap.mapPawns.FreeColonists.Where(XX => XX.HasTrait(TraitDef.Named("LuoPoGuiZu"))).ToList().ForEach(XX =>
                    {
                        var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart11233"));
                        if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart11233"));
                    });

                    Find.LetterStack.ReceiveLetter(
                    $"贵族鼓舞(落魄贵族{ChoiceCountFriendlyAttack}人)",
                    "落魄贵族提升所有小人心情。",
                    LetterDefOf.PositiveEvent
                    );
                }
                else if (RG.Next(1, 10000) < 205 && Find.CurrentMap.mapPawns.FreeColonists.Count(XX => XX.HasTrait(TraitDef.Named("MoRiMingChu"))) != 0)
                {
                    //HuangYeMingChuDebuff
                    var TotalCookingCount = 0;
                    var RenRandomChocie = Find.CurrentMap.mapPawns.FreeColonists.Where(XX => XX.HasTrait(TraitDef.Named("MoRiMingChu")));
                    foreach (var item in RenRandomChocie)
                    {
                        var AddCount = item.skills.GetSkill(SkillDefOf.Cooking).Level / 2 <= 0 ? 1 :
                            item.skills.GetSkill(SkillDefOf.Cooking).Level / 2;

                        IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(item.Position, Find.CurrentMap, 4, XX4 => XX4.Walkable(Find.CurrentMap));
                        GenSpawn.Spawn(ThingDefOf.MealSimple, intVec1, Find.CurrentMap, 0).stackCount = AddCount;
                        TotalCookingCount += AddCount;
                        //添加做饭后的DEBUFF
                        item.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("HuangYeMingChuDebuff"));
                    }

                    //做出的食物必须不等于0才能显示消息
                    if (TotalCookingCount != 0)
                    {
                        Find.LetterStack.ReceiveLetter(
                        $"食物(荒野厨师{ TotalCookingCount } 人份)",
                        "您的荒野厨师利用身边索能找到的所有食材做出了料理。",
                        LetterDefOf.PositiveEvent
                        );
                    }
                }

                ////搜寻玩家远行队
                if (PT != null && PT.pawns.Count >= 1 && RG.Next(1, 10000) < 30) 
                {
                    Find.LetterStack.ReceiveLetter($"远行队学习", "您的远行队中一个成员技能得到提升", LetterDefOf.PositiveEvent);
                    PT.PawnsListForReading.RandomElement()?.skills.skills.RandomElement().Learn((float)RG.Next(1, 2000), true);
                    goto end;
                }
                if (PT != null && PT.pawns.Count >= 1 && RG.Next(1, 10000) < 10)
                {
                    Find.LetterStack.ReceiveLetter($"远行队学习(良好)", "您的远行队中一个成员技能得到大量提升", LetterDefOf.PositiveEvent);
                    PT.PawnsListForReading.RandomElement()?.skills.skills.RandomElement().Learn((float)RG.Next(1, 8000), true);
                    goto end;
                }

                //QuestHelper
                if (PT != null && PT.pawns.Count >= 2 && RG.Next(1, 10000) < 40)
                {
                    Find.LetterStack.ReceiveLetter($"远行队(发现任务)", "您的远行队经过村落，发现了任务信息！", LetterDefOf.PositiveEvent);

                    QuestHelper.AddRamdomQuest(2);
                    if(RG.Next(1, 10000)<=2333) QuestHelper.AddRamdomQuest(RG.Next(1, (GenDate.DaysPassed == 0 ? 1 : GenDate.DaysPassed) * 30));
                    if (RG.Next(1, 10000) <= 2333) QuestHelper.AddRamdomQuest(RG.Next(1, (GenDate.DaysPassed == 0 ? 1 : GenDate.DaysPassed) * 30));
                    if (RG.Next(1, 10000) <= 2333) QuestHelper.AddRamdomQuest(RG.Next(1, (GenDate.DaysPassed == 0 ? 1 : GenDate.DaysPassed) * 30));

                    goto end;
                }

                //队伍中小人数量大于2，并且有小人烹饪技能大于6
                if (PT != null && PT.needs.AnyPawnOutOfFood(out var TheState) && PT.pawns.Count >= 2 && RG.Next(1, 10000) < 1830 && PT.PawnsListForReading.Count(XX=> XX.skills.GetSkill(SkillDefOf.Cooking).Level >= 6) != 0)
                {
                    //统计队伍中烹饪技能较高的小人
                    var CountCooking = PT.PawnsListForReading.Count(XX => XX.skills.GetSkill(SkillDefOf.Cooking).Level >= 6);
                    var rr = ThingMaker.MakeThing(ThingDef.Named("Pemmican"));
                    rr.stackCount = CountCooking * (RG.Next(50, 100));
                    PT.AddPawnOrItem(rr, true);

                    Find.LetterStack.ReceiveLetter($"烹饪肉干({ CountCooking }人) { rr.stackCount } 个", "您的远行队中，烹饪技能较高的小人利用自然资源烹饪了食物", LetterDefOf.PositiveEvent);
                    goto end;
                }

                //烹饪大量肉干
                if (PT != null && PT.pawns.Count >= 6 && RG.Next(1, 10000) < 330 && PT.PawnsListForReading.Count(XX => XX.skills.GetSkill(SkillDefOf.Cooking).Level >= 10) != 0)
                {
                    //统计队伍中烹饪技能较高的小人
                    var CountCooking = PT.PawnsListForReading.Count(XX => XX.skills.GetSkill(SkillDefOf.Cooking).Level >= 8);
                    var rr = ThingMaker.MakeThing(ThingDef.Named("Pemmican"));
                    rr.stackCount = CountCooking * (RG.Next(100, 250));
                    PT.AddPawnOrItem(rr, true);

                    Find.LetterStack.ReceiveLetter($"烹饪大量肉干({ CountCooking }人) { rr.stackCount } 个", "您的远行队中，烹饪技能较高的小人利用自然资源烹饪了食物", LetterDefOf.PositiveEvent);
                    goto end;
                }

                //烹饪巨量肉干
                if (PT != null && PT.pawns.Count >= 8 && RG.Next(1, 10000) < 130 && PT.PawnsListForReading.Count(XX => XX.skills.GetSkill(SkillDefOf.Cooking).Level >= 16) != 0)
                {
                    //统计队伍中烹饪技能较高的小人
                    var CountCooking = PT.PawnsListForReading.Count(XX => XX.skills.GetSkill(SkillDefOf.Cooking).Level >= 5);
                    var rr = ThingMaker.MakeThing(ThingDef.Named("Pemmican"));
                    rr.stackCount = CountCooking * (RG.Next(100, 450));
                    PT.AddPawnOrItem(rr, true);

                    Find.LetterStack.ReceiveLetter($"打猎获得巨量肉干({ CountCooking }人) { rr.stackCount } 个", "您的远行队中，烹饪技能较高的小人利用自然资源烹饪了食物", LetterDefOf.PositiveEvent);
                    goto end;
                }

                if (PT != null && PT.pawns.Count >= 5) 
                {
                    if (RG.Next(1, 10000) < 130)
                    {
                        var TheWp = WaponCommon.GenerateWeapons(RG.Next(1, 5)).First();
                        PT.AddPawnOrItem(TheWp, true);
                        Find.LetterStack.ReceiveLetter($"远行队搜索到武器-({ TheWp.def.defName })", "您的远行队在地图上发现遗失的武器装备。", LetterDefOf.PositiveEvent);
                        goto end;
                    }
                    else if (RG.Next(1, 10000) < 130) 
                    {
                        Find.LetterStack.ReceiveLetter($"远行队好心情(低)", "您的远行队在地图上路过一处风景美好的地方，全员心情提升。", LetterDefOf.PositiveEvent);
                        //GerYeWai
                        PT.PawnsListForReading.ForEach(XX =>
                        {
                            if (XX.NonHumanlikeOrWildMan()) return;
                            var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerYeWai"));
                            if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerYeWai"));
                        });
                        goto end;
                    }
                    //foreach (ThingWithComps t in PT.pawns[0].equipment.AllEquipmentListForReading.ToArray())
                    //{
                    //    if (t.def.weaponTags != null)
                    //    {
                    //        t.Destroy();
                    //    }
                    //}

                    ////PT.pawns[0].inventory.().FirstOrDefault()?.Destroy();
                    //var TheWapons = WaponCommon.GenerateWeapons(RG.Next(1, 5));
                    ////PT.pawns[0].equipment.wa
                    //PT.pawns[0].equipment.AddEquipment((ThingWithComps)TheWapons.FirstOrDefault());
                }

            end:;

                //if (RG.Next(1,1000) < 115) Find.CurrentMap.MakeRandomBuildingRepair(); //测试样例3 - 随机建筑可修复
                #region 商队，测试成功
                //{
                //    IsInit1 = true;
                //    //商队
                //    var allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName == "TraderCaravanArrival");
                //    //判断玩家有没有盟友派系
                //    if (allDefsListForReading?.Count() != 0 && Find.FactionManager.RandomAlliedFaction() != null)
                //    {
                //        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, Find.CurrentMap);
                //        incidentParms.target = Find.CurrentMap;
                //        incidentParms.faction = Find.FactionManager.RandomAlliedFaction();
                //        allDefsListForReading.FirstOrDefault().Worker.TryExecute(incidentParms);
                //    }
                //}
                #endregion

                //得到地图上所有自定义营地的数量
                //var CountOfItemStash = Find.WorldObjects.Sites.Count(XX => 
                //{
                //    //找到地图上的现有搜刮点地图
                //    if (XX.questTags != null && (XX.questTags.Contains("GerCreate"))) return true;
                //    return false;
                //});

                //if (CountOfItemStash == 0)
                //{
                //    //Find.LetterStack.ReceiveLetter("物资藏匿点被发现 - 猎杀人类","一些可采摘的植物在地图上生长",LetterDefOf.PositiveEvent);
                //    TileFinder.TryFindNewSiteTile(out var num2, 1, 3, false, TileFinderMode.Random, -1, false);
                //    //bool flag2 = Find.WorldGrid[num2].temperature > 0f && Find.WorldGrid[num2].hilliness != Hilliness.Mountainous;
                //    var Part = SiteMakerHelper.SitePartDefsWithTag("ItemStashGer").FirstOrDefault();
                //    Part.forceExitAndRemoveMapCountdownDurationDays = 0.5f;
                //    Site site = SiteMaker.MakeSite(Part ?? SitePartDefOf.AmbushEdge, 1, Faction.OfPlayer, true, null);
                //    site.Tile = num2;
                //    //site.GetComponent<TimeoutComp>().StartTimeout(3 * 2500);
                //    site.GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown(24 * 2500);
                //    site.questTags = new List<string> { "GerCreate" };
                //    Find.WorldObjects.Add(site);
                //}
                //}

                //轻食症
                if (RG.Next(1, 10000) < 4) RandomColonistFoodMan();

                //派系自动结盟
                if (RG.Next(1, 10000) < 97) StationHelper.MakeFriends();

                //植物生长
                //if (RG.Next(1, 4000) < 7) Find.CurrentMap.JiangGuoZhang();


                //所有小人动态加上任务标签 - 防止出错
                Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX => { if (XX.questTags == null) XX.questTags = new List<string>(); });
                //患有失忆症小人在地图上随机移动
                Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                {
                    //如果持有发病标签，则执行操作
                    if (XX.questTags.Contains("LostMem"))
                    {
                        XX.jobs.EndCurrentJob(JobCondition.Succeeded, false, true);
                        XX.jobs.StartJob(new Job(JobDefOf.Goto, CellFinder.RandomCell(Find.CurrentMap)), 0, null, false, true, null, null, false, false);
                        XX.jobs.ClearQueuedJobs(true);
                    }

                    //发病小人随机删除发病标签
                    if (RG.Next(1, 3000) < 441) XX.questTags.Remove("LostMem");
                });

                if (TheFlash.InitDelay > 0) TheFlash.InitDelay--;
                //雷暴发生
                if (TheFlash.KeepOnCount > 0 && TheFlash.InitDelay <= 0) 
                {
                    TheFlash.KeepOnCount--;
                    TheFlash.StartStorm();
                }

                return;
                

                //全员失血
                //if (RG.Next(1, 3000) < 8 && TakeDownCount14 <= 0)
                //{
                //    TakeDownCount14 = Rand.Range(1, 600);
                //    TakeAllBloodLess();
                //    return;
                //}

                

                //小人仓皇逃窜
                //if (RG.Next(1, 3000) < 4)
                //{
                //    //小人仓皇逃窜
                //    var RI = DefDatabase<MentalStateDef>.AllDefsListForReading.Where(XX => XX.defName == "PanicFlee").FirstOrDefault();
                //    if (RI != null)
                //    {
                //        var TheQiRen = Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement();
                //        TheQiRen.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("PanicFlee"));
                //    }
                //}


                //pawn.story.traits.HasTrait
                
                //发脾气和打架不能同时发生
                if (RG.Next(1, 4000) < 6 && Find.CurrentMap.mapPawns.FreeColonists.Where(XX1 => !XX1.Downed && !XX1.Dead && !XX1.InBed()).Count() != 0 && TakeDownCount18 <= 0) 
                {
                    TakeDownCount18 = RG.Next(1, 24);
                    var TheQiRen = Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement();
                    TheQiRen.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Tantrum"));

                    Find.LetterStack.ReceiveLetter(
                    "失去控制",
                    "你的殖民者正在打架",
                    LetterDefOf.NegativeEvent
                    );

                    return;
                }

                //必须要在殖民者都处于健康状态下，才能开始打架 - 测试样例1
                if (RG.Next(1, 1000) < 6 && TakeDownCount16 <= 0 && Find.CurrentMap.mapPawns.FreeColonists.Where(XX1 => !XX1.Downed && !XX1.Dead && !XX1.InBed()).Count() >= 2 && TakeDownCount18 <= 0)
                {
                    //打架
                    DaJia();
                    return;
                }

                //黑夜寒流
                //if (GameCondition_ColdSnapGer.IsColdSnapOver && RG.Next(1, 4000) < 2 && TakeDownCount9<=0)
                //{
                //	TakeDownCount9 = Rand.Range(1, 100);
                //	GameCondition_ColdSnapGer.IsColdSnapOver = false;
                //	Find.LetterStack.ReceiveLetter(
                //	"黑夜寒流",
                //	"一场恐怖的寒流正朝你袭来",
                //	LetterDefOf.ThreatBig
                //	);

                //	int duration = Mathf.RoundToInt((60000f * Rand.Range(0.1f, 1f)));
                //	GameCondition_ColdSnapGer conditionMeteor2 = (GameCondition_ColdSnapGer)GameConditionMaker.MakeCondition(new GameConditionDef { endMessage = "距离黑夜寒流结束", label = "距离黑夜寒流结束", description = "带来极端气候的黑夜寒流", conditionClass = typeof(GameCondition_ColdSnapGer) }, duration);
                //	Find.CurrentMap.gameConditionManager.RegisterCondition(conditionMeteor2);
                //	return;
                //}

                //随机殖民者技能异变概率
                if (RG.Next(1, 4000) < 3 && TakeDownCount6 <= 0)
                {
                    TakeDownCount6 = Rand.Range(1, 40);
                    ChangeTheSkillRandom();
                    return;
                }

                //建筑随机爆炸概率
                //if (RG.Next(1, 1000) < 3 && TakeDownCount7 <= 0)
                //{
                //    TakeDownCount7 = Rand.Range(1, 40);
                //    RandomBuildingBoom();
                //    return;
                //}

                //建筑坍塌
                if (RG.Next(1, 1000) < 3 && TakeDownCount8 <= 0)
                {
                    TakeDownCount8 = Rand.Range(1, 40);
                    RandomBuildingDestory();
                    return;
                }

                //处理虫族连续进攻
                //if (TheState.IsZerg && TakeDownCount3 <= 0)
                //{
                //    //40%概率解除封锁
                //    if (RG.Next(1, 1000) < 600)
                //    {
                //        Find.LetterStack.ReceiveLetter(
                //        "解除封锁",
                //        "虫族围攻似乎结束了，但这仅仅只是现在而已",
                //        LetterDefOf.NeutralEvent
                //        );
                //        TheState.IsZerg = false; //允许连续进攻
                //        return;
                //    }

                //    //距离下一次的进攻时间
                //    TakeDownCount3 = Rand.Range(1, 24);
                //    if (Rand.Chance(0.5f)) DoZergAttack(); else DoZergAttackFJ();
                //    return;
                //}

                //Scribe_Collections.Look<Pawn>()

                //刷新被遗弃的资源
                //if (RG.Next(1, 1000) < 22 && TakeDownCount5 <= 0)
                //{
                //	TakeDownCount5 = Rand.Range(1, 56);
                //	//一定概率立即出现下一波判定
                //	if (RG.Next(1, 1000) < 200) TakeDownCount5 = 0;
                //	TheLostResouce.DoReflushLostResouce();
                //}

                //大量虫族进攻
                if (GenDate.DaysPassed >= 25 && RG.Next(1, 4000) < 4 && TakeDownCount4 <= 0 && IsAttackOver)
                {
                    IsAttackOver = false;
                    TakeDownCount4 = Rand.Range(1, 1200);

                    Find.LetterStack.ReceiveLetter(
                        "大量虫族接近中",
                        "大量虫族生物开始轮番袭击这片区域",
                        LetterDefOf.NegativeEvent
                        );

                    TheWaveCount = 0; //清空波数计数器
                    var TheFinalWave = Rand.Range(3, (7 + (GenDate.DaysPassed / 40))); //天数越高，波数越多
                    for (int i = 0; i < TheFinalWave; i++)
                    {
                        if (i != (TheFinalWave - 1))
                        {
                            TheTimerManager.AddTimer(("ZergAttack_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
                            {
                                TTL = i * Rand.Range(15, 24),
                                _OnTimerDown = new System.Action(() =>
                                {
                                    DoZergAttack(1);
                                })
                            });
                        }
                        else if (i == (TheFinalWave - 1))
                        {
                            //最后一波为大量资源舱坠毁
                            TheTimerManager.AddTimer(("ZergAttack_ResoursePod" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
                            {
                                TTL = i * Rand.Range(1, 10),
                                _OnTimerDown = new System.Action(() =>
                                {
                                    if (RG.Next(1, 10000) < 11) TheResoucePodCrashedMore();
                                    else TheResoucePodCrashed();
                                    IsAttackOver = true; //允许再次发起袭击
                                })
                            });
                        }
                    }

                    return;
                }

                //虫族进攻
                //if (GenDate.DaysPassed >= 20 && RG.Next(1, 4000) < 2 && TakeDownCount3 <= 0)
                //{
                //    //判定是否进入虫族封锁状态
                //    if (RG.Next(1, 1000) < 10 && !TheState.IsZerg)
                //    {
                //        Find.LetterStack.ReceiveLetter(
                //        "封锁",
                //        "虫族似乎封锁了整个地区，封锁行动导致所有额外空投事件不会发生，直至封锁行动结束",
                //        LetterDefOf.NegativeEvent
                //        );
                //        TheState.IsZerg = true; //允许连续进攻 - 虫族围攻
                //    }


                //    TakeDownCount3 = Rand.Range(20, 200);
                //    if (Rand.Chance(0.5f)) DoZergAttack(); else DoZergAttackFJ();
                //    return;
                //}

                //货仓坠毁
                //if (RG.Next(1, 4000) < 15 && TakeDownCount1 <= 0)
                //{
                //	TakeDownCount1 = Rand.Range(1, 148);
                //	TheResoucePodCrashed();
                //	return; //保证同一个时间点只触发一个事件
                //}

                //大量货仓坠毁
                //if (RG.Next(1, 4000) < 10 && TakeDownCount2 <= 0)
                //{
                //	TakeDownCount2 = Rand.Range(200, 1400);
                //	//TheResoucePodCrashed();
                //	TheResoucePodCrashedMore();
                //	return; //保证同一个时间点只触发一个事件
                //}

                if (RG.Next(1, 4000) < 13 && TakeDownCount10 <= 0)
                {
                    TakeDownCount10 = Rand.Range(24, 164);

                    Find.LetterStack.ReceiveLetter(
                        "快乐",
                        "您的所有殖民者今天不知道为何，异常高兴",
                        LetterDefOf.PositiveEvent
                        );

                    if (RG.Next(1, 1000) < 933)
                    {
                        //小人添加好心情BUFF
                        Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                        {
                            var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerBig"));
                            if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerBig"));
                        });
                        //TheResoucePodCrashed();
                        //TheResoucePodCrashedMore();
                    }
                    else
                    {
                        //小人添加好心情BUFF
                        Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                        {
                            var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart"));
                            if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart"));
                            //XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart"));
                        });
                    }
                    return; //保证同一个时间点只触发一个事件
                }

                //陨石撞击事件 - 极低概率事件
                if (RG.Next(1, 2000) < 4 && TakeDownCount11 <= 0)
                {
                    TakeDownCount11 = Rand.Range(1, 224);

                    if (Rand.Range(1, 1000) < 960)
                    {
                        //90%概率普通流星
                        //Find.LetterStack.ReceiveLetter(
                        //    "流星",
                        //    "来自外太空的陨石",
                        //    LetterDefOf.PositiveEvent
                        //    );

                        DoTheMetorCreate();
                    }
                    else
                    {
                        //10概率高密度流星
                        //Find.LetterStack.ReceiveLetter(
                        //    "流星雨",
                        //    "一大群陨石正向你袭来",
                        //    LetterDefOf.PositiveEvent
                        //    );

                        TheWaveCount = 0; //清空波数计数器
                        var TheFinalWave = Rand.Range(1, 6);
                        for (int i = 0; i < TheFinalWave; i++)
                        {
                            if (i != (TheFinalWave - 1))
                            {
                                TheTimerManager.AddTimerSmall(("MetorCreate_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                                {
                                    TTL = i * 3,
                                    _OnTimerDown = new System.Action(() =>
                                    {
                                        DoTheMetorCreate();
                                    })
                                });
                            }
                            else if (i == (TheFinalWave - 1))
                            {
                                TheTimerManager.AddTimerSmall(("MetorCreate_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                                {
                                    TTL = i * 3,
                                    _OnTimerDown = new System.Action(() =>
                                    {
                                        DoTheMetorCreate();
                                    })
                                });
                            }
                        }
                    }
                    return; //保证同一个时间点只触发一个事件
                }

                //超级流星
                //if (RG.Next(1, 4000) < 11 && TakeDownCount12 <= 0)
                //{
                //	//保证三天之内不会复发这个事件
                //	TakeDownCount12 = Rand.Range(24*3, 644);

                //	Find.LetterStack.ReceiveLetter(
                //		"超级流星",
                //		"一块巨大的流星坠落在了地面上",
                //		LetterDefOf.PositiveEvent
                //		);

                //	DoTheMetorCreateBig();
                //	return; //保证同一个时间点只触发一个事件
                //}
            }
        }
    }
}
