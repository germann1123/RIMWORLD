using MeteorIncident;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MeteorIncidentGer
{
    public class KongTou_Ger_Open : CompUseEffect
    {
        //public override string CompInspectStringExtra()
        //{
        //    String str = default(String);
        //    if (mode > 1)
        //    {
        //        str += "HKJ_PrizeDraw_Mode_str".Translate(mode.ToString());
        //    }
        //    else
        //    {
        //        str += "HKJ_PrizeDraw_Mode_singel".Translate();
        //    }
        //    if (debug)
        //    {
        //        str += " Debug Mode On";
        //    }
        //    return str;
        //}
        //public override void PostExposeData()
        //{
        //    Scribe_Values.Look<int>(ref mode, "mode", 1);
        //}

        public void DoCorrectOpen(Thing ThekongTou, Pawn TheOpener) 
        {
            WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 探索空投，并解锁资源舱!");
            Find.LetterStack.ReceiveLetter($"发现资源 ({ TheOpener.Name })", "您的一个殖民者打开了神秘空投仓，发现一些货仓资源!", LetterDefOf.RitualOutcomePositive);

            var ThingOut = ThingSetMakerDefOf.ResourcePod.root.Generate();
            ThingOut.ForEach(XX21 =>
            {
                IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(ThekongTou.Position, Find.CurrentMap, 6, XX4 => XX4.Walkable(Find.CurrentMap));
                GenSpawn.Spawn(XX21, intVec1, Find.CurrentMap, WipeMode.Vanish);
            });

            ThingOut = ThingSetMakerDefOf.ResourcePod.root.Generate();
            ThingOut.ForEach(XX21 =>
            {
                IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(ThekongTou.Position, Find.CurrentMap, 6, XX4 => XX4.Walkable(Find.CurrentMap));
                GenSpawn.Spawn(XX21, intVec1, Find.CurrentMap, WipeMode.Vanish);
            });

            parent.Destroy();
        }

        public void RandomOption(DiaNode DN,Thing ThekongTou, Pawn TheOpener) 
        {
            //使用者必须烹饪等级高于10，并且货仓处于被拆除爆炸装置的前提，才有概率会出现这个选项
            if (TheOpener.skills.GetSkill(SkillDefOf.Cooking).Level >= 10 && HotseatGameComponent.RG.Next(1, 10000) < 1222
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name }搜刮食物补给(烹饪10)")
                {
                    action = delegate
                    {
                        //百分之百会开出食物的食物货仓
                        GenSpawn.Spawn(ThingDefOf.Pemmican, parent.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 300);
                        if (HotseatGameComponent.RG.Next(1, 10000) < 4222) GenSpawn.Spawn(ThingDefOf.ComponentIndustrial, parent.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 4);
                        TheOpener.needs.food.CurLevelPercentage = 1f;  //使用者饱腹
                        //随机概率学习到烹饪经验
                        var ExpLearn = HotseatGameComponent.RG.Next(1, 4000);
                        if (HotseatGameComponent.RG.Next(1, 10000) < 2222) TheOpener.skills.GetSkill(SkillDefOf.Cooking).Learn(ExpLearn);

                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 启动了空投补给仓自动烹饪模块!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 发现食物制作装置 获得({ ExpLearn })烹饪经验。", "这是一个藏有大量食物的货仓，发现它的小人会直接填饱肚子" +
                            "，并且还有不少剩下的食物!", LetterDefOf.RitualOutcomePositive);

                        //在周围寻找合适点刷新药物
                        var RandomLoopCount = HotseatGameComponent.RG.Next(1, 28);
                        for (int i = 0; i < RandomLoopCount; i++)
                        {
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec != null)
                            {
                                GenSpawn.Spawn(ThingDefOf.MealFine, intVec, Find.CurrentMap, 0).stackCount = Rand.Range(1, 3);
                            }
                        }

                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            if (GenDate.DaysPassed >= 30 && HotseatGameComponent.RG.Next(1, 10000) < 655
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 取得【铁人】特性")
                {
                    action = delegate
                    {
                        //HotseatGameComponent.FindTheKeyToRescure = true;

                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 的殖民者获取【铁人】属性!");
                        var RR = TheOpener?.TryGetTieRenTrait();
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 获得铁人",
                                $"您的小人被赋予了超强特性", LetterDefOf.RitualOutcomePositive);

                        //StorytellerComp_RandomEventer.IsKongTouRequest = true;
                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            if (SteamUtility.SteamPersonaName.Contains("神经病有所")) 
            {
                DiaOption val1123 = new DiaOption($"{TheOpener.Name} 取得【超智学习】特性")
                {
                    action = delegate
                    {                      
                        var RR = TheOpener?.TryGetTieRenTrait("ChaoZhiXueXizhe");
                        Find.LetterStack.ReceiveLetter($"({TheOpener.Name}) 获得超智学习",
                                $"您的小人被赋予了超强特性", LetterDefOf.RitualOutcomePositive);

                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            if (GenDate.DaysPassed >= 20 && HotseatGameComponent.RG.Next(1, 10000) < 655
            && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{TheOpener.Name} 取得【强力恢复】特性")
                {
                    action = delegate
                    {
                        //HotseatGameComponent.FindTheKeyToRescure = true;

                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 的殖民者获取【强力恢复】属性!");
                        //var RR = TheOpener?.TryGetTieRenTrait();
                        Find.LetterStack.ReceiveLetter($"({TheOpener.Name}) 获得强力恢复",
                                $"您的小人被赋予了超强特性", LetterDefOf.PositiveEvent);

                        {
                            var plagueOnPawn = TheOpener?.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("NaturalHealingBig"));
                            if (plagueOnPawn != null)
                            {
                                //如果已经感染流感，则不作处理
                                //plagueOnPawn.Severity += randomSeverity;
                            }
                            else
                            {
                                Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("NaturalHealingBig"), TheOpener);
                                //把状态添加到被击中的目标身上
                                TheOpener.health.AddHediff(hediff);
                            }
                        }

                        //StorytellerComp_RandomEventer.IsKongTouRequest = true;
                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            //GenSpawn.Spawn(ThingDefOf.ComponentIndustrial, XX1.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 20);

            if (GenDate.DaysPassed >= 30 && HotseatGameComponent.RG.Next(1, 10000) < 155
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 获取零部件补给")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 找到了一些零部件!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 搜寻到零部件", "一些货仓内藏匿的零部件被找到。", LetterDefOf.PositiveEvent);

                        //在周围寻找合适点刷新药物
                        var RandomLoopCount = HotseatGameComponent.RG.Next(1, 12);
                        for (int i = 0; i < RandomLoopCount; i++)
                        {
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 6, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec != null)
                            {
                                GenSpawn.Spawn(ThingDefOf.ComponentIndustrial, intVec, Find.CurrentMap, 0).stackCount = Rand.Range(1, 3);
                            }
                        }

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            if (GenDate.DaysPassed >= 12 && HotseatGameComponent.RG.Next(1, 10000) < 155
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 获取医药补给")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 获取了医药补给!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 搜索到医药补给", "一些货仓内藏匿的零部件被找到。", LetterDefOf.PositiveEvent);

                        //在周围寻找合适点刷新药物
                        var RandomLoopCount = HotseatGameComponent.RG.Next(1, 12);
                        for (int i = 0; i < RandomLoopCount; i++)
                        {
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 6, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec != null)
                            {
                                GenSpawn.Spawn(ThingDefOf.MedicineIndustrial, intVec, Find.CurrentMap, 0).stackCount = Rand.Range(1, 3);
                            }
                        }

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            if (GenDate.DaysPassed >= 12 && HotseatGameComponent.RG.Next(1, 10000) < 255
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 获取木材")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 获取了木材补给!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 获取木材", "一些木材被发现", LetterDefOf.PositiveEvent);

                        //在周围寻找合适点刷新药物
                        var RandomLoopCount = HotseatGameComponent.RG.Next(1, 12);
                        for (int i = 0; i < RandomLoopCount; i++)
                        {
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 6, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec != null)
                            {
                                GenSpawn.Spawn(ThingDefOf.WoodLog, intVec, Find.CurrentMap, 0).stackCount = Rand.Range(1, 75);
                            }
                        }

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            if (GenDate.DaysPassed >= 12 && HotseatGameComponent.RG.Next(1, 10000) < 255
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 获取简单食物")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 获取了简单食物补给!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 获取简单食物", "一些简单食物被发现。", LetterDefOf.PositiveEvent);

                        //在周围寻找合适点刷新药物
                        var RandomLoopCount = HotseatGameComponent.RG.Next(1, 12);
                        for (int i = 0; i < RandomLoopCount; i++)
                        {
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 6, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec != null)
                            {
                                GenSpawn.Spawn(ThingDefOf.MealSimple, intVec, Find.CurrentMap, 0).stackCount = Rand.Range(1, 8);
                            }
                        }

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            if (GenDate.DaysPassed >= 12 && HotseatGameComponent.RG.Next(1, 10000) < 255
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 获取钢铁补给")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 获取了钢铁补给!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 搜索到钢铁补给", "一些隐藏铁被发现。", LetterDefOf.PositiveEvent);

                        //在周围寻找合适点刷新药物
                        var RandomLoopCount = HotseatGameComponent.RG.Next(1, 12);
                        for (int i = 0; i < RandomLoopCount; i++)
                        {
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 6, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec != null)
                            {
                                GenSpawn.Spawn(ThingDefOf.Steel, intVec, Find.CurrentMap, 0).stackCount = Rand.Range(1, 75);
                            }
                        }

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            //地狱随机兰迪事件
            if (GenDate.DaysPassed >= 0 && HotseatGameComponent.RG.Next(1, 10000) < 155
               && parent.questTags.Contains("OP_Check_Okay_ChengGong") && Current.Game.storyteller.def.defName == "RandyEx1")
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 开始随机叙事者事件")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 开始了一个叙事者随机事件!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 开始一个随机事件", "叙事者被手动控制。", LetterDefOf.PositiveEvent);

                        StorytellerComp_RandomEventer.IsRandomEventStart = true;

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            //地狱随机兰迪事件
            if (GenDate.DaysPassed >= 0 && HotseatGameComponent.RG.Next(1, 10000) < 155
               && parent.questTags.Contains("OP_Check_Okay_ChengGong") && Current.Game.storyteller.def.defName == "RandyEx1")
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 开始三次叙事者随机事件")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 开始叙事者三次随机事件!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 开始三个随机事件", "叙事者被手动控制。", LetterDefOf.PositiveEvent);

                        //连续开始三次叙事者事件
                        StorytellerComp_RandomEventer.IsRandomEventStart3 = true;

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            if (GenDate.DaysPassed >= 0 && HotseatGameComponent.RG.Next(1, 10000) < 155
               && parent.questTags.Contains("OP_Check_Okay_ChengGong") &&
               Current.Game.storyteller.def.defName == "RandyEx1" && !HotseatGameComponent.FindTheKeyTo_WuYouDuChengAi)
            {
                DiaOption val1123 = new DiaOption($"{TheOpener.Name} 心情奖励")
                {
                    action = delegate
                    {
                        //WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 开始叙事者三次随机事件!");
                        Find.LetterStack.ReceiveLetter($"({TheOpener.Name}) 找到高级奢侈品", "心情奖励，能够持续好几天。", LetterDefOf.PositiveEvent);

                        //连续开始三次叙事者事件
                        //StorytellerComp_RandomEventer.IsRandomEventStart3 = true;

                        TheOpener.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart1123234234252342"));

                        //HotseatGameComponent.FindTheKeyTo_WuYouDuChengAi = true; //下一次命中有毒尘埃将会被跳过

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            if (GenDate.DaysPassed >= 30 && HotseatGameComponent.RG.Next(1, 10000) < 155
               && parent.questTags.Contains("OP_Check_Okay_ChengGong") &&
               Current.Game.storyteller.def.defName == "RandyEx1" && !HotseatGameComponent.FindTheKeyTo_WuYouDuChengAi)
            {
                DiaOption val1123 = new DiaOption($"{TheOpener.Name} 净化有毒尘埃")
                {
                    action = delegate
                    {
                        //WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 开始叙事者三次随机事件!");
                        Find.LetterStack.ReceiveLetter($"({TheOpener.Name}) 净化下次有毒尘埃", "下次有毒尘埃将会被净化，前提是叙事者为 地狱随机兰迪。", LetterDefOf.PositiveEvent);

                        //连续开始三次叙事者事件
                        //StorytellerComp_RandomEventer.IsRandomEventStart3 = true;

                        HotseatGameComponent.FindTheKeyTo_WuYouDuChengAi = true; //下一次命中有毒尘埃将会被跳过

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            //营救带有强力恢复属性的偷渡客
            if (GenDate.DaysPassed >= 25 && HotseatGameComponent.RG.Next(1, 10000) < 155
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 营救偷渡客(带有强力恢复属性)")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 营救了偷渡客（带有强力恢复疾病）!");
                        //这个地方改成殖民者，野人会出事
                        Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 成功营救偷渡客 { pawn.Name }", $"成功营救偷渡客 { pawn.Name }。", LetterDefOf.PositiveEvent);

                        var Skill8XiaoCount = pawn.skills.skills.Where(XX => XX.levelInt < 8);
                        //统计随机出来的野人技能小于8的个数
                        if (Skill8XiaoCount.Count() >= 5)
                        {
                            //8次机会修改技能等级
                            for (int loopa = 0; loopa < 6; loopa++)
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
                        IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 12, XX4 => XX4.Walkable(Find.CurrentMap));
                        GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, WipeMode.Vanish);

                        //GerXingHongZuZhou
                        {
                            var plagueOnPawn = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("NaturalHealingBig"));
                            if (plagueOnPawn != null)
                            {
                                //如果已经感染流感，则不作处理
                                //plagueOnPawn.Severity += randomSeverity;
                            }
                            else
                            {
                                //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                                Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("NaturalHealingBig"), pawn);
                                //把状态添加到被击中的目标身上
                                pawn.health.AddHediff(hediff);
                            }
                        }
                        {
                            //为小人添加加速状态
                            var plagueOnPawn = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerXingHongZuZhou"));
                            if (plagueOnPawn != null)
                            {
                                //如果已经感染流感，则不作处理
                                //plagueOnPawn.Severity += randomSeverity;
                            }
                            else
                            {
                                //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                                Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerXingHongZuZhou"), pawn);
                                //把状态添加到被击中的目标身上
                                pawn.health.AddHediff(hediff);
                            }
                        }

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            if (GenDate.DaysPassed >= 5 && HotseatGameComponent.RG.Next(1, 10000) < 155
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 营救偷渡客")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 营救了偷渡客!");
                        Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 成功营救偷渡客 { pawn.Name }", $"成功营救偷渡客 { pawn.Name }。", LetterDefOf.PositiveEvent);

                        var Skill8XiaoCount = pawn.skills.skills.Where(XX => XX.levelInt < 8);
                        //统计随机出来的野人技能小于8的个数
                        if (Skill8XiaoCount.Count() >= 5)
                        {
                            //8次机会修改技能等级
                            for (int loopa = 0; loopa < 3; loopa++)
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
                        IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 12, XX4 => XX4.Walkable(Find.CurrentMap));
                        GenSpawn.Spawn(pawn, intVec, Find.CurrentMap, WipeMode.Vanish);

                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }


            if (TheOpener.skills.GetSkill(SkillDefOf.Construction).Level >= 6 && HotseatGameComponent.RG.Next(1, 10000) < 1222
                && parent.questTags.Contains("OP_Check_Okay_ChengGong") && Current.Game.storyteller.def.defName == "RandyEx33")
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 激活失事信标(建造6)")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 激活了失事信标!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 激活失事信标","一个信标被激活，包含物资货仓即将在一段时间后坠毁。", LetterDefOf.PositiveEvent);
                        StorytellerComp_RandomEventer.IsKongTouRequest = true;
                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }            

            //HotseatGameComponent.FindTheKeyToRescure
            if (GenDate.DaysPassed >= 30 && HotseatGameComponent.RG.Next(1, 10000) < 155
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 取得帝国征召令")
                {
                    action = delegate
                    {
                        HotseatGameComponent.FindTheKeyToRescure = true;
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 取得帝国征召令!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 取得帝国征召令",
                                $"下次拜访友军基的的时候，有人将会加入你们", LetterDefOf.PositiveEvent);

                        //StorytellerComp_RandomEventer.IsKongTouRequest = true;
                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }


            //小人特性更新
            if (GenDate.DaysPassed >= 30 && HotseatGameComponent.RG.Next(1, 10000) < 1155
                && parent.questTags.Contains("OP_Check_Okay_ChengGong")) 
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 特性更新装置")
                {
                    action = delegate
                    {
                        var RR = TheOpener?.TryRemoveTrait();

                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 激活了特性重新分配模块!");
                        //显示特性刷新结果
                        if (RR?.RR ?? false)
                        {
                            Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 特性 { RR?.Qian.def.defName.Translate() } 消失，" +
                                $"特性 { RR?.Hou.def.defName.Translate() }", "特性转换。", LetterDefOf.PositiveEvent);
                        }

                        //StorytellerComp_RandomEventer.IsKongTouRequest = true;
                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            //社交越高，则越容易开出任务给予
            if (HotseatGameComponent.RG.Next(1, 10000) < 122+(TheOpener.skills.GetSkill(SkillDefOf.Social).Level* HotseatGameComponent.RG.Next(100,300))
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 接取随机任务(社交-高风险)")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 接取高风险随机任务!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 接取任务", "你的小人接取了一些随机任务。", LetterDefOf.PositiveEvent);

                        var QuestCount = HotseatGameComponent.RG.Next(1, 6);

                        var DoQuestCount = 0;
                        for (int i = 0; i < QuestCount; i++)
                        {
                            var RR231 = QuestHelper.AddRamdomQuest(HotseatGameComponent.RG.Next((GenDate.DaysPassed * 20)/2, GenDate.DaysPassed*20));
                            if (RR231) DoQuestCount++;
                        }

                        //StorytellerComp_RandomEventer.IsKongTouRequest = true;
                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            //截取低风险任务
            if (HotseatGameComponent.RG.Next(1, 10000) < 122 + (TheOpener.skills.GetSkill(SkillDefOf.Social).Level * HotseatGameComponent.RG.Next(100, 300))
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 接取随机任务(社交-低风险)")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 接取了低风险随机任务!");
                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 接取任务", "你的小人接取了一些随机任务(低风险)。", LetterDefOf.PositiveEvent);

                        var QuestCount = HotseatGameComponent.RG.Next(1, 6);

                        var DoQuestCount = 0;
                        for (int i = 0; i < QuestCount; i++)
                        {
                            var RR231 = QuestHelper.AddRamdomQuest(HotseatGameComponent.RG.Next((GenDate.DaysPassed * 5) / 2, GenDate.DaysPassed * 5));
                            if (RR231) DoQuestCount++;
                        }

                        //StorytellerComp_RandomEventer.IsKongTouRequest = true;
                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }

            if (GenDate.DaysPassed >= 35 && HotseatGameComponent.RG.Next(1, 10000) < 455
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 启动学习机器")
                {
                    action = delegate
                    {
                        var RandomSkill = TheOpener?.skills.skills.RandomElement();

                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 启动学习机器为自己的殖民者培训技能!");
                        if (HotseatGameComponent.RG.Next(1, 1000) < 900)
                        {
                            if (TheOpener != null)
                            {
                                var LearnPoint = (float)HotseatGameComponent.RG.Next(1, 20000);
                                //一个随机小人学习一样随机技能
                                RandomSkill?.Learn(LearnPoint, true);
                                Find.LetterStack.ReceiveLetter($"技能学习{ TheOpener.Name } 技能 { RandomSkill.def.defName } 提高 { LearnPoint }", "您的殖民者使用学习机器进行技能学习，并且获得大量经验", LetterDefOf.RitualOutcomePositive);
                            }
                        }


                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            //给其他玩家召唤友军
            if (GenDate.DaysPassed >= 35 && HotseatGameComponent.RG.Next(1, 10000) < 155
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 呼叫友军支援星系中的其他玩家")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 呼叫盟友支援星系中其他玩家!");
                        //发送请求到服务器，请求其他随机玩家出现友军支援
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#FriendlyAttackRequest#{SteamUtility.SteamPersonaName}#1#");


                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            //随机发现医疗物资
            if (TheOpener.skills.GetSkill(SkillDefOf.Medicine).Level >= 8 && HotseatGameComponent.RG.Next(1, 10000) < 1222
                && parent.questTags.Contains("OP_Check_Okay_ChengGong"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name }搜刮医疗补给(医疗8)")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 的殖民者利用其高超医疗技术破解了空头仓医疗模块!");
                        //百分之百会开出食物的食物货仓
                        GenSpawn.Spawn(ThingDefOf.MedicineIndustrial, parent.Position, Find.CurrentMap, 0).stackCount = Rand.Range(1, 20+(TheOpener.skills.GetSkill(SkillDefOf.Medicine).Level*2));
                        var MeachoLear = HotseatGameComponent.RG.Next(1, 4000);
                        //随机概率学习到烹饪经验
                        if (HotseatGameComponent.RG.Next(1, 10000) < 2222) TheOpener.skills.GetSkill(SkillDefOf.Medicine).Learn(MeachoLear);

                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 发现医疗补给 学习({ MeachoLear })经验", "这是一个藏有大量医疗物资的货仓," +
                            "，并且还有不少有用的物资!", LetterDefOf.RitualOutcomePositive);

                        var RandomLoopCount = HotseatGameComponent.RG.Next(1, 20);
                        for (int i = 0; i < RandomLoopCount; i++)
                        {
                            IntVec3 intVec = CellFinder.RandomClosewalkCellNear(TheOpener.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                            if (intVec != null)
                            {
                                GenSpawn.Spawn(ThingDefOf.MedicineIndustrial, intVec, Find.CurrentMap, 0).stackCount = 1;
                            }
                        }

                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }

            if (HotseatGameComponent.RG.Next(1, 10000) < 1222
                && parent.questTags.Contains("OP_Check_Okay_ChengGong") && !parent.questTags.Contains("YuLe_TanSuo"))
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name }接线(娱乐设备)")
                {
                    action = delegate
                    {
                        if (HotseatGameComponent.RG.Next(1, 10000) < 5222)
                        {
                            Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 成功接线。", "成功将娱乐模块连接至主控电脑," +
                            "，再次探索即可获得一次使用娱乐设备的机会!", LetterDefOf.RitualOutcomePositive);
                            parent.questTags.Add("YuLe_TanSuo_ChengGong");
                        }
                        else 
                        {
                            Find.LetterStack.ReceiveLetter($"娱乐设备接线失败", "探索次数过多导致空投舱彻底损坏，无法使用。!", LetterDefOf.RitualOutcomeNegative);

                            //删除空投箱自毁计时器
                            TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                            parent.Destroy();
                        }

                        parent.questTags.Add("YuLe_TanSuo");
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
                return;
            }
            if (parent.questTags.Contains("YuLe_TanSuo_ChengGong") && !parent.questTags.Contains("OP_Check_Okay_ChengGong_ShiYongWanBi")) 
            {
                DiaOption val1123 = new DiaOption($"{ TheOpener.Name } 启动娱乐设备!")
                {
                    action = delegate
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 破解并启动了空头仓特殊娱乐设备!");
                        parent.questTags.Add("OP_Check_Okay_ChengGong_ShiYongWanBi");
                        //娱乐拉满
                        TheOpener.needs.joy.GainJoy(100f, JoyKindDefOf.Social);
                        Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                        {
                            var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerBig"));
                            if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerBig"));
                        });

                        Find.LetterStack.ReceiveLetter($"({ TheOpener.Name }) 启用娱乐设备。", "成功将娱乐模块连接至主控电脑," +
                                    "，再次探索即可获得一次使用娱乐设备的机会!", LetterDefOf.RitualOutcomePositive);

                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                DN.options.Add(val1123);
            }
        }

        static DiaNode TheDN = null;
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            //对话框
            DiaNode diaNode = TheDN = new DiaNode("探索神秘空投，如果不预先执行【检查】操作，那么极有可能在打开过后会启用爆炸装置 " +
                "\r\n 对于解除失败的空投应该直接采取拆除方式安全回收资源。\r\n " +
                "请注意您对隔间货仓的操作，如果您首先打开了货仓，那么隔间将会被完全封锁，打开隔间能够搜刮到额外物资，但同时也会带来危险。");

            //如果持有空投恐惧症的小人开启空投的话，则放弃打开
            var plagueOnPawn1 = usedBy.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("ChaoZaiChongNeng_ChongFu"));
            if (plagueOnPawn1 != null)
            {
                //如果已经感染流感，则不作处理
                //plagueOnPawn.Severity += randomSeverity;
                DiaOption val_Close1 = new DiaOption("放弃打开(空投恐惧症)")
                {
                    action = delegate
                    {
                        TheDN.PreClose();
                        parent.Destroy();
                    },
                    resolveTree = true
                };
                diaNode.options.Add(val_Close1);

                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, this.parent.LabelCap));
                return;
            }

            //最大探索次数限制
            if (parent.questTags.Count(XX=>XX.Contains("ExplorerCount")) == 0)
            {
                //最大探索次数可能性受操作者智力等级限定
                parent.questTags.Add($"ExplorerCount_{HotseatGameComponent.RG.Next(2, (12+ usedBy.skills.GetSkill(SkillDefOf.Intellectual).Level))}");
            }
            else 
            {
                //得到探索次数数据
                var TheCount = parent.questTags.Find(XX => XX.Contains("ExplorerCount_")).Split(new string[] { "_" },StringSplitOptions.RemoveEmptyEntries)[1];
                if (int.TryParse(TheCount, out var ExplorerCount)) ExplorerCount--;
                if (ExplorerCount <= 0) 
                {
                    Find.LetterStack.ReceiveLetter($"空投损坏", "探索次数过多导致空投舱彻底损坏，无法使用。!", LetterDefOf.RitualOutcomeNegative);

                    //删除空投箱自毁计时器
                    TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                    parent.Destroy();
                    return;
                }

                //移除这次削减次数过后的标记
                parent.questTags.RemoveAll(XX => XX.Contains("ExplorerCount_"));
                //重新添加标签
                parent.questTags.Add($"ExplorerCount_{ExplorerCount}");
            }

            DiaOption val = new DiaOption("打开")
            {
                action = delegate
                {
                    //拆除爆炸装置失败
                    if (parent.questTags.Contains("Open_Boomer")) 
                    {
                        Find.LetterStack.ReceiveLetter($"剧烈爆炸", "这是一个由于失误操作引起的剧烈爆炸，周围大范围的可燃物会着火。!", LetterDefOf.RitualOutcomeNegative);
                        GenExplosion.DoExplosion(parent.Position, Find.CurrentMap, 10, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null,false, null, 0f, 1, 0f, false, null, null);

                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                        return;
                    }

                    DoCorrectOpen(parent, usedBy);
                },
                resolveTree = true
            };
            diaNode.options.Add(val);

            //刷新随机以及条件满足的选项
            RandomOption(diaNode,parent, usedBy);

            DiaOption val1 = new DiaOption("检查爆炸物")
            {
                action = delegate
                {
                    HotseatGameComponent.KongTouTanCiShu++;
                    //WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#ShenMiKongTouTanSuo#{SteamUtility.SteamPersonaName}#{HotseatGameComponent.KongTouTanCiShu++}#");
                    //Kouqian(usedBy.Map);
                    //Find.LetterStack.ReceiveLetter($"检查空投({ usedBy.Name })", "测试!", LetterDefOf.PositiveEvent);
                    //根据小人智力等级决定开启成功概率
                    var TheRR = 10000 - (usedBy.skills.GetSkill(SkillDefOf.Intellectual).Level * 200) < 0 ? 6000 : (10000 - (usedBy.skills.GetSkill(SkillDefOf.Intellectual).Level * 200));
                    if (HotseatGameComponent.RG.Next(1, TheRR) < 4999)
                    {
                        if (parent.questTags.Contains("Boomer"))
                        {
                            WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 探索神秘空投成功解锁 {HotseatGameComponent.KongTouTanCiShu} 次!");
                            Find.LetterStack.ReceiveLetter($"({ usedBy.Name }) 成功解除引爆装置。", "被解除引爆装置的神秘空投不会被定时器引爆!", LetterDefOf.RitualOutcomePositive);
                            parent.questTags.Remove("Boomer");
                            //追加解除爆炸物成功标志
                            parent.questTags.Add("OP_Check_Okay_ChengGong");
                        }
                    }
                    else 
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 探索神秘空投解锁失败 {HotseatGameComponent.KongTouTanCiShu} 次!");
                        Find.LetterStack.ReceiveLetter($"({ usedBy.Name }) 解除引爆失败。", "解除引爆装置失败的空投在被打开的时候极有可能会爆炸!", LetterDefOf.RitualOutcomeNegative);
                        if (!parent.questTags.Contains("Open_Boomer")) parent.questTags.Add("Open_Boomer");
                    }

                    parent.questTags.Add("OP_Check_Okay");
                },
                resolveTree = true
            };

            //检查爆炸物操作完成后不再弹出解除爆炸物选项卡 - 无论成功与否
            if(!parent.questTags.Contains("OP_Check_Okay")) diaNode.options.Add(val1);

            //没有爆炸物的空投 30%概率
            if (HotseatGameComponent.RG.Next(1, 10000) < 3000) 
            {
                parent.questTags.Remove("Boomer");
                //追加解除爆炸物成功标志
                parent.questTags.Add("OP_Check_Okay_ChengGong");
                diaNode.options.Remove(val1);
            }

            DiaOption val2 = new DiaOption("打开隔间货仓")
            {
                action = delegate
                {
                    //Kouqian(usedBy.Map);
                    //Find.LetterStack.ReceiveLetter($"检查空投({ usedBy.Name })", "测试!", LetterDefOf.PositiveEvent);
                    //隔间如果被成功打开过，则不能再重复打开
                    if (parent.questTags.Contains("GeJianOpend")) return;

                    //拆除爆炸装置失败
                    if (parent.questTags.Contains("Open_Boomer"))
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 打开隔间失败!");
                        Find.LetterStack.ReceiveLetter($"剧烈爆炸", "这是一个由于失误操作引起的剧烈爆炸，周围大范围的可燃物会着火。!", LetterDefOf.RitualOutcomeNegative);
                        GenExplosion.DoExplosion(parent.Position, Find.CurrentMap, 10, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null,false, null, 0f, 1, 0f, false, null, null);

                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                        return;
                    }

                    if (HotseatGameComponent.RG.Next(1, 10000) < 4999)
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 探索空投箱并未发现宝藏!");
                        Find.LetterStack.ReceiveLetter($"({ usedBy.Name }) 一无所获。", "被解除引爆装置的神秘空投不会被定时器引爆!", LetterDefOf.RitualOutcomeNegative);
                        parent.questTags.Add("GeJianOpend");
                    }
                    else
                    {
                        //开空投隔间，有概率得到好心情
                        if (HotseatGameComponent.RG.Next(1, 10000) < 7999)
                        {
                            var TheGerBig = usedBy.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart23312"));
                            if (TheGerBig == null) usedBy.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart23312"));
                            MoteMaker.ThrowText(usedBy.PositionHeld.ToVector3(), usedBy.MapHeld, "心情+++", Color.green, 5f);

                            var plagueOnPawn = usedBy.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("ChaoZaiChongNeng_ChongFu"));
                            if (plagueOnPawn != null)
                            {
                                //如果已经感染流感，则不作处理
                                //plagueOnPawn.Severity += randomSeverity;
                            }
                            else
                            {
                                //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                                Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("ChaoZaiChongNeng_ChongFu"), usedBy);
                                //把状态添加到被击中的目标身上
                                usedBy.health.AddHediff(hediff);

                                MoteMaker.ThrowText(usedBy.PositionHeld.ToVector3(), usedBy.MapHeld, "空投恐惧症+", Color.red, 5f);
                            }
                        }

                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 探索空投箱发现隐藏药物!");
                        Find.LetterStack.ReceiveLetter($"({ usedBy.Name }) 发现隐藏的高级药物。", "解除引爆装置失败的空投在被打开的时候极有可能会爆炸!", LetterDefOf.RitualOutcomePositive);
                        if (!parent.questTags.Contains("Open_Boomer") && HotseatGameComponent.RG.Next(1, 10000) < 5999) parent.questTags.Add("Open_Boomer");

                        //在周围寻找合适点刷新药物
                        IntVec3 intVec = CellFinder.RandomClosewalkCellNear(parent.Position, Find.CurrentMap, 8, XX4 => XX4.Walkable(Find.CurrentMap));
                        if (intVec != null)
                        {
                            GenSpawn.Spawn(ThingDefOf.MedicineUltratech, intVec, Find.CurrentMap, 0).stackCount = Rand.Range(1, (3 + (GenDate.DaysPassed/25)));
                        }
                        
                        parent.questTags.Add("GeJianOpend");
                    }
                },
                resolveTree = true
            };

            if (!parent.questTags.Contains("GeJianOpend"))  diaNode.options.Add(val2);

            DiaOption val_Close = new DiaOption("关闭")
            {
                action = delegate
                {
                    TheDN.PreClose();

                    //玩家关闭空投箱可能会引发爆炸
                    if (HotseatGameComponent.RG.Next(1, 10000) < 4999)
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 放弃探索并引发爆炸!");
                        Find.LetterStack.ReceiveLetter($"空投箱小规模爆炸", "这是一个由于失误操作引起的剧烈爆炸，周围大范围的可燃物会着火。!", LetterDefOf.RitualOutcomeNegative);
                        GenExplosion.DoExplosion(parent.Position, Find.CurrentMap, 1, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);

                        //删除空投箱自毁计时器
                        TheTimerManager.DeleteTimer(("ShenMiKongTou_"));
                        parent.Destroy();
                    }
                    else 
                    {
                        WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#GuangBoXiaoXi#{SteamUtility.SteamPersonaName} 放弃探索!");
                    }
                },
                resolveTree = true
            };
            diaNode.options.Add(val_Close);

            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, this.parent.LabelCap));
        }
    }
}
