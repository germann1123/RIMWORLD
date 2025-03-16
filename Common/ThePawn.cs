using MeteorIncidentGer;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MeteorIncident.Common
{
    public class MyKongTou : GameComponent
    {

        public MyKongTou()
        {
        }

        public MyKongTou(Game game)
        {
        }

        public override void GameComponentTick()
        {
            //OnTick();
            //ProcessKongTou();
        }

        public void UpdateKongTou(Thing XX1)
        {
            //时间抑制
            if (Find.CurrentMap == null || HotseatGameComponent.IsCreateHive) return;
            //判断周围是否有殖民者

            //指定物品是否被破坏
            FleckMaker.ThrowAirPuffUp(XX1.TrueCenter(), Find.CurrentMap);
            //散发出大量热量
            GenTemperature.PushHeat(XX1, 60);
            Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX3 =>
            {
                //不符合条件的小人剔除掉
                if (!(XX3.Position.DistanceTo(XX1.Position) < 4 && !XX3.Dead && !XX3.Downed)) return;
                //寻找空投箱附近的殖民者
                HotseatGameComponent.IsCreateHive = true; //不重复触发事件标志

                WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#ShenMiKongTouTanSuo#{SteamUtility.SteamPersonaName}#{HotseatGameComponent.KongTouTanCiShu++}#");

                //20%概率殖民者会获得一个好心情
                if (Rand.Chance(0.05f))
                {
                    //娱乐加满
                    XX3.needs.joy.GainJoy(100f, JoyKindDefOf.Social);

                    //添加BUFF
                    XX3.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerExcited"));

                    HotseatGameComponent.IsCreateHive = true;
                    return;
                }

                //大于二十天的时候，有概率找到货仓密钥，下次开启得到大量资源
                if (GenDate.DaysPassed > 20 && Rand.Chance(0.05f) && !HotseatGameComponent.FindTheKey)
                {
                    HotseatGameComponent.FindTheKey = true;
                    Find.LetterStack.ReceiveLetter(
                    "货仓密钥",
                    "下一次开启货仓的时候，将会得到成倍的物资",
                    LetterDefOf.NeutralEvent
                    );

                    XX3.needs.joy.GainJoy(80f, JoyKindDefOf.Social);

                    //添加BUFF
                    XX3.needs.mood.thoughts.memories.
                    TryGainMemoryFast(ThoughtDef.Named("GerSmall"));

                    HotseatGameComponent.IsCreateHive = true;
                    return;
                }

                //FindTheKeyToRescure
                if (GenDate.DaysPassed > 5 && Rand.Chance(0.05f) && !HotseatGameComponent.FindTheKeyToRescure)
                {
                    HotseatGameComponent.FindTheKeyToRescure = true;
                    Find.LetterStack.ReceiveLetter(
                    "帝国签证(下次营救直接招募)",
                    "持有此签证，下次营救的偷渡客将直接招募为您的成员",
                    LetterDefOf.NeutralEvent
                    );

                    XX3.needs.joy.GainJoy(80f, JoyKindDefOf.Social);

                    //添加BUFF
                    XX3.needs.mood.thoughts.memories.
                    TryGainMemoryFast(ThoughtDef.Named("GerSmall"));

                    HotseatGameComponent.IsCreateHive = true;
                    return;
                }

                //FindTheKeyToRescure
                if (GenDate.DaysPassed > 5 && Rand.Chance(0.05f) && !HotseatGameComponent.FindTheKeyToRescure)
                {
                    HotseatGameComponent.FindTheKeyToRescure = true;
                    Find.LetterStack.ReceiveLetter(
                    "帝国征召令(下次拜访友军基地有人会加入你)",
                    "持有此征召令拜访友军基地，有人会直接加入你的阵营。",
                    LetterDefOf.NeutralEvent
                    );

                    XX3.needs.joy.GainJoy(80f, JoyKindDefOf.Social);

                    //添加BUFF
                    XX3.needs.mood.thoughts.memories.
                    TryGainMemoryFast(ThoughtDef.Named("GerSmall"));

                    HotseatGameComponent.IsCreateHive = true;
                    return;
                }

                //使用钥匙即可打开得到大批资源
                if (HotseatGameComponent.FindTheKey)
                {
                    HotseatGameComponent.FindTheKey = false; //重置密钥

                    for (int i = 0; i < Rand.Range(15, 30); i++)
                    {
                        var TheBank112 = Rand.Range(1, 4);

                        //FleckMaker.ThrowSmoke(loc, map, size);

                        //随机散落一些资源
                        if (TheBank112 == 1)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.MealSurvivalPack, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.FreeColonists.Count * 2);
                        }
                        else if (TheBank112 == 2)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.Steel, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, 20);
                        }
                        else if (TheBank112 == 3)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.MedicineHerbal, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.FreeColonists.Count * 2);
                        }
                    }

                    HotseatGameComponent.IsCreateHive = true;
                    return;
                }

                //20天前的保护机制
                if (GenDate.DaysPassed < 20)
                {
                    for (int i = 0; i < Rand.Range(2, 5); i++)
                    {
                        var TheBank112 = Rand.Range(1, 4);

                        //随机散落一些资源
                        if (TheBank112 == 1)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.MealSurvivalPack, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.FreeColonists.Count * 2);
                        }
                        else if (TheBank112 == 2)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.Steel, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, 20);
                        }
                        else if (TheBank112 == 3)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.MedicineHerbal, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.FreeColonists.Count * 2);
                        }
                    }

                    //空投不会爆炸
                    //XX1.questTags?.Remove("Boomer");
                    //小人娱乐增加
                    XX3.needs.joy.GainJoy(80f, JoyKindDefOf.Social);
                    //goto End;
                }
                else if (Rand.Chance(0.05f))
                {
                    //随机增加娱乐
                    XX3.needs.joy.GainJoy(100f, JoyKindDefOf.Social);

                    Find.LetterStack.ReceiveLetter(
                    "虫族伏击",
                    "你被一些埋伏在神秘残骸周围的虫子盯上了",
                    LetterDefOf.ThreatBig
                    );

                    //随机创建一些虫子
                    for (int i = 0; i < Rand.Range(2, 3); i++)
                    {
                        IntVec3 intVec = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 5, null);
                        if (intVec != null)
                        {
                            IEnumerable<PawnKindDef> enumerable = new List<PawnKindDef>
                            {
                                PawnKindDefOf.Megascarab,
                                PawnKindDefOf.Spelopede,
                                PawnKindDefOf.Megaspider
                            };

                            bool flag = GenCollection.TryRandomElement<PawnKindDef>(enumerable, out var pawnKindDefO);
                            Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDefO, Faction.OfInsects);
                            GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, 0);
                            IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                        }

                        var TheBank112 = Rand.Range(1, 4);

                        //随机散落一些资源
                        if (TheBank112 == 1)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.MealSurvivalPack, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.FreeColonists.Count * 5);
                        }
                        else if (TheBank112 == 2)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.Steel, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, 75);
                        }
                        else if (TheBank112 == 3)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.MedicineHerbal, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.FreeColonists.Count * 5);
                        }
                    }
                    //30概率出现额外虫子
                    if (Rand.Chance(0.08f))
                    {
                        //随机创建一些虫子
                        for (int i = 0; i < Rand.Range(2, 6); i++)
                        {
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(XX1.Position, Find.CurrentMap, 20, null);
                            if (intVec != null)
                            {
                                IEnumerable<PawnKindDef> enumerable = new List<PawnKindDef>
                                {
                                    PawnKindDefOf.Megascarab,
                                    PawnKindDefOf.Spelopede,
                                    PawnKindDefOf.Megaspider
                                };

                                bool flag = GenCollection.TryRandomElement<PawnKindDef>(enumerable, out var pawnKindDefO);
                                Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDefO, Faction.OfInsects);
                                GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, 0);
                                IncidentWorker_AnimalInsanityMass.DriveInsane(pawn);
                            }
                        }
                    }

                    //if (Rand.Chance(0.09f))
                    //{
                    //    Find.LetterStack.ReceiveLetter(
                    //    "虫族大规模进攻进行中",
                    //    "空投箱中的爆炸物吸引了区域内虫族，它们将在200秒后扑向这片区域",
                    //    LetterDefOf.NegativeEvent
                    //    );

                    //    TheTimerManager.AddTimer(("ZergAttack_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
                    //    {
                    //        TTL = 200,
                    //        _OnTimerDown = new System.Action(() =>
                    //        {
                    //            HotseatGameComponent.DoZergAttackWithGroupCount(Rand.Range(2, 6));
                    //        })
                    //    });
                    //}
                }
                else if (Rand.Chance(0.15f))
                {
                    if (HotseatGameComponent.FindTheKeyToRescure)
                    {
                        //帝国签证
                        HotseatGameComponent.FindTheKeyToRescure = false;
                        //刷出来的小人直接给予玩家
                        HotseatGameComponent.CreateFriend(XX1.Position, false, true);
                    }
                    else
                    {
                        //50%概率不会离开地图，50%会离开地图
                        HotseatGameComponent.CreateFriend(XX1.Position, (HotseatGameComponent.RG.Next(1, 10000) < 5120 ? true : false));
                    }

                    Find.LetterStack.ReceiveLetter(
                        "货仓偷渡客",
                        "货仓舱门打开后，你意外发现了藏匿的偷渡客",
                        LetterDefOf.PositiveEvent
                        );
                }
                else if (Rand.Chance(0.03f))
                {
                    //指定物品是否被破坏
                    if (!XX1.Destroyed)
                    {
                        if (Find.CurrentMap.mapPawns.FreeColonists.Count > 1 && !HotseatGameComponent.IsShang && Rand.Chance(0.4f))
                        {
                            HotseatGameComponent.IsShang = true;
                            HealthUtility.DamageUntilDowned(XX3, false);
                        }
                        else
                        {
                            XX1.Destroy();
                        }

                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, Rand.Range(5.5f, 20.5f), DamageDefOf.Flame, null, 8, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);
                    }
                }
                else if (Rand.Chance(0.1f))
                {
                    //指定物品是否被破坏
                    if (!XX1.Destroyed)
                    {
                        Find.LetterStack.ReceiveLetter(
                        "货仓自动爆炸(高级药物)",
                        "由于你的殖民者触发了陷阱，空投箱立即爆炸！",
                        LetterDefOf.PositiveEvent
                        );
                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, 10, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);

                        GenSpawn.Spawn(ThingDefOf.MedicineUltratech, XX1.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 8);


                        XX1.Destroy();
                    }
                }
                else if (Rand.Chance(0.1f))
                {
                    //指定物品是否被破坏
                    if (!XX1.Destroyed)
                    {
                        Find.LetterStack.ReceiveLetter(
                        "货仓自动爆炸(大量钢铁)",
                        "由于你的殖民者触发了陷阱，空投箱立即爆炸！",
                        LetterDefOf.PositiveEvent
                        );
                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, 10, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);

                        //GenSpawn.Spawn(ThingDefOf.Steel, XX1.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 500);

                        var Loopm = HotseatGameComponent.RG.Next(1, 20);
                        for (int loopa = 0; loopa < Loopm; loopa++)
                        {
                            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.Steel, intVec1, Find.CurrentMap, 0).stackCount = Rand.Range(1, 50);
                        }
                        XX1.Destroy();
                    }
                }
                else if (Rand.Chance(0.1f))
                {
                    //指定物品是否被破坏
                    if (!XX1.Destroyed)
                    {
                        Find.LetterStack.ReceiveLetter(
                        "货仓自动爆炸(零件)",
                        "由于你的殖民者触发了陷阱，空投箱立即爆炸！",
                        LetterDefOf.PositiveEvent
                        );
                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, 8, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);

                        GenSpawn.Spawn(ThingDefOf.ComponentIndustrial, XX1.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 20);

                        XX1.Destroy();
                    }
                }
                else if (Rand.Chance(0.1f))
                {
                    //指定物品是否被破坏
                    if (!XX1.Destroyed)
                    {
                        Find.LetterStack.ReceiveLetter(
                        "货仓自动爆炸(食物)",
                        "由于你的殖民者触发了陷阱，空投箱立即爆炸！",
                        LetterDefOf.PositiveEvent
                        );
                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, 8, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);

                        GenSpawn.Spawn(ThingDefOf.MealSurvivalPack, XX1.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 100);

                        XX1.Destroy();
                    }
                }
                else if (Rand.Chance(0.1f))
                {

                    //指定物品是否被破坏
                    if (!XX1.Destroyed)
                    {
                        Find.LetterStack.ReceiveLetter(
                        "货仓自动爆炸(黄金)",
                        "由于你的殖民者触发了陷阱，空投箱立即爆炸！",
                        LetterDefOf.PositiveEvent
                        );
                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, 8, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);

                        GenSpawn.Spawn(ThingDefOf.Gold, XX1.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 100);

                        XX1.Destroy();
                    }
                }
                else if (Rand.Chance(0.1f))
                {
                    if (!XX1.Destroyed)
                    {
                        Find.LetterStack.ReceiveLetter(
                        "被困的敲击兽",
                        "一只特殊动物被困在货舱中",
                        LetterDefOf.PositiveEvent
                        );
                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, 8, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);

                        Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Thrumbo, Find.FactionManager.OfPlayer);
                        GenSpawn.Spawn(pawn, XX1.Position, Find.CurrentMap, 0);

                        XX1.Destroy();
                    }
                }
            });
        }
        public void UpdateKongTou_Tie(Thing XX3)
        {
            if (HotseatGameComponent.RG.Next(1, 10000) < 6111) return;
            //随机产生一些铁矿石
            var SteelCreateCount = HotseatGameComponent.RG.Next(1, 20);
            //根据不同的空投展示不同文字
            MoteMaker.ThrowText(XX3.PositionHeld.ToVector3(), XX3.MapHeld, $"铁矿石+{SteelCreateCount}", Color.blue, 2f);
            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(XX3.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
            if (intVec1 != null) GenSpawn.Spawn(ThingDefOf.Steel, intVec1, Find.CurrentMap, 0).stackCount = SteelCreateCount;
            //Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX3 =>
            //{
            //});
        }
        public void ProcessKongTou() 
        {
            if (Find.TickManager.TicksGame % 400 != 0) return;
            var TheShenMiKongtou = Find.CurrentMap.listerThings.AllThings.Where(XX => XX.questTags != null &&
                (XX.questTags.Contains("Boomer"))).FirstOrDefault();
            if (TheShenMiKongtou != null) UpdateKongTou(TheShenMiKongtou);

            TheShenMiKongtou = Find.CurrentMap.listerThings.AllThings.Where(XX => XX.questTags != null &&
               (XX.questTags.Contains("Boomer_Tie"))).FirstOrDefault();
            if (TheShenMiKongtou != null) UpdateKongTou_Tie(TheShenMiKongtou);
        }

        public void OnTick()
        {
            if (Find.TickManager.TicksGame % 700 != 0) return;
            //神秘空投红字提示
            if (Find.CurrentMap != null)
            {
                Find.CurrentMap.listerThings.AllThings.Where(XX => XX.questTags != null &&
                (XX.questTags.Contains("Boomer") || XX.questTags.Contains("Boomer_Tie"))).ToList().ForEach(XX1 =>
                {
                    //根据不同的空投展示不同文字
                    MoteMaker.ThrowText(XX1.PositionHeld.ToVector3(), XX1.MapHeld, $"Help!!!", Color.red, 2f);
                });
            }
        }
    }

    /// <summary>
    /// 空投产生使用类
    /// </summary>
    public class GerKongTouGen 
    {
        public static bool IsTimerDown = true;
        public static string _ThePrefabTag = "Boomer_Tie";
        public static void DoSpawnKongTou_Tie()
        {
            var TheList = Find.CurrentMap.listerThings.AllThings.Where(XX => XX.questTags != null && XX.questTags.Contains(_ThePrefabTag));
            //不重复刷新空投箱，必须等待前一个爆炸后
            if (TheList.Count() != 0) return;

            //在未来一个随机时间爆炸
            TheTimerManager.AddTimer(("KongTou(ChanTie)_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
            {
                TTL = Rand.Range(48, 88),
                _OnTimerDown = new System.Action(() =>
                {
                    IsTimerDown = true;
                    //如果没有指定物件，则不执行
                    if (Find.CurrentMap.listerThings.AllThings.Count(XX => XX.questTags != null && XX.questTags.Contains(_ThePrefabTag)) == 0) return;
                    //所有神秘空投爆炸
                    Find.CurrentMap.listerThings.AllThings.Where(XX => XX.questTags != null && XX.questTags.Contains(_ThePrefabTag)).ToList().ForEach(XX1 =>
                    {
                        if (XX1.Destroyed) return;
                        //指定物品是否被破坏
                        Find.LetterStack.ReceiveLetter("铁矿源爆炸", "携带大量铁矿资源生产残骸由于某种不稳定因素，产生剧烈爆炸", LetterDefOf.ThreatSmall);
                        GenExplosion.DoExplosion(XX1.Position, Find.CurrentMap, Rand.Range(0.5f, 5.5f), DamageDefOf.Flame, null, 8, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);
                        XX1.Destroy();
                    });
                })
            });

            IsTimerDown = false;

            HotseatGameComponent.TryFindCell(out var TheCell, Find.CurrentMap);
            Find.LetterStack.ReceiveLetter(
                    "神秘空投(铁矿脉)",
                    "一个能在有限时间内探测并生产出铁矿的空投箱被发现。",
                    LetterDefOf.NegativeEvent, new TargetInfo(TheCell, Find.CurrentMap, false)
                    );

            SoundDefOf.ShipTakeoff.PlayOneShot(new TargetInfo(TheCell, Find.CurrentMap, false));
            var rr = ThingMaker.MakeThing(ThingDefOf.ShipChunk);
            var TheThing12 = GenSpawn.Spawn(rr, TheCell, Find.CurrentMap, 0);

            TheThing12.questTags = new List<string>();
            TheThing12.questTags.Add(_ThePrefabTag);
        }
    }
}
