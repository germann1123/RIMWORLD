using MeteorIncident;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncidentGer
{
    public class PlayerDianDialo
    {
        public static DiaNode TheDN = null;
        public static int UploadCount = 0;

        public static int AutoUploadCount = 0;
        public static int AutoJuJueUploadCount = 0;

        /// <summary>
        /// 是否允许充能，用于游戏刚开始时，就会充能的BUG
        /// </summary>
        public static bool IsChongYunXu = false;

        public static void DoUpdate() 
        {
            if (ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit))
            {
                PowerNet powerNet = culprit.PowerComp.PowerNet;
                Map map = culprit.Map;
                List<CompPowerBattery> TheBs = powerNet.batteryComps;
                var totalEnergy = 0.0f;

                //计算所有电池的当前电量，并算出提供给盟友的电量后回发
                for (int i = 0; i < powerNet.batteryComps.Count; i++)
                {
                    CompPowerBattery compPowerBattery = powerNet.batteryComps[i];
                    totalEnergy += compPowerBattery.StoredEnergy;
                    //电池丢失所有电量
                    compPowerBattery.DrawPower(compPowerBattery.StoredEnergy);
                }

                //统计电力上传次数
                if (UploadCount++ <= 0)
                {
                    Find.LetterStack.ReceiveLetter(
                    $"电力上传 {totalEnergy} Wd",
                    $"您上传到星际网络的电能千瓦时。",
                    LetterDefOf.NeutralEvent
                    );
                }
                else
                {
                    Find.LetterStack.ReceiveLetter(
                    $"电力上传 {totalEnergy} Wd 持续电力出口",
                    $"您上传到星际网络的电能千瓦时，上传电力越多，就能得到更多电力飞升奖励。",
                    LetterDefOf.NeutralEvent
                    );
                }

                //List<Thing> list = ThingSetMakerDefOf.MapGen_AncientTempleContents.root.Generate();
                var RandomGG = HotseatGameComponent.RG.Next(1, 13);
                //低概率随机事件
                if(HotseatGameComponent.RG.Next(1, 10000) < 224) RandomGG = 100;
                if (RandomGG == 1)
                {
                    TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = HotseatGameComponent.RG.Next(2, 8),
                        _Data = map,
                        _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                        {
                            if (TheMap != null && TheMap is Map TheMapEx)
                            {
                                List<Thing> contents = ThingSetMakerDefOf.MapGen_AncientTempleContents.root.Generate();

                                //随机给出循环，进行飞升补给
                                var Count = HotseatGameComponent.RG.Next(1, 3);
                                for (int loopa = 0; loopa < Count; loopa++)
                                {
                                    var ThingOut = ThingSetMakerDefOf.ResourcePod.root.Generate();
                                    contents.AddRange(ThingOut);
                                }

                                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                                var refugee = ThingUtility.FindPawn(contents);
                                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                                Find.LetterStack.ReceiveLetter(
                                                "电力飞升(稀有物资补给)",
                                                "出口电力，得到飞升物资补给包",
                                                LetterDefOf.RitualOutcomePositive,
                                                new TargetInfo(dropSpot, Find.CurrentMap, false)
                                                );

                                var podInfo = new ActiveDropPodInfo();
                                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                                podInfo.openDelay = 100;
                                podInfo.leaveSlag = false;
                                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
                            }
                        })
                    });
                }
                else if (RandomGG == 2)
                {
                    TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = HotseatGameComponent.RG.Next(2, 8),
                        _Data = map,
                        _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                        {
                            if (TheMap != null && TheMap is Map TheMapEx)
                            {
                                List<Thing> contents = new List<Thing>();
                                contents.Add(ThingMaker.MakeThing(ThingDefOf.MedicineIndustrial));
                                contents.Add(ThingMaker.MakeThing(ThingDefOf.MedicineUltratech));
                                if (contents[0] != null) contents[0].stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.ColonistCount * 8);
                                if (contents[1] != null) contents[1].stackCount = Rand.Range(1, Find.CurrentMap.mapPawns.ColonistCount * 3);

                                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                                var refugee = ThingUtility.FindPawn(contents);
                                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                                Find.LetterStack.ReceiveLetter(
                                                "电力飞升(医疗补给)",
                                                "出口电力，得到飞升物资补给包",
                                                LetterDefOf.RitualOutcomePositive,
                                                new TargetInfo(dropSpot, Find.CurrentMap, false)
                                                );

                                var podInfo = new ActiveDropPodInfo();
                                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                                podInfo.openDelay = 100;
                                podInfo.leaveSlag = false;
                                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
                            }
                        })
                    });
                }
                else if (RandomGG == 3) //电力飞升，心情提升
                {
                    TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = HotseatGameComponent.RG.Next(2, 8),
                        _Data = Find.RandomPlayerHomeMap,
                        _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                        {
                            if (TheMap != null && TheMap is Map TheMapEx)
                            {
                                //小人添加好心情BUFF
                                TheMapEx.mapPawns.FreeColonists.ToList().ForEach(XX =>
                                {
                                    XX.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("GerStart11231_DianWang"));
                                });

                                //心情抚慰
                                Find.LetterStack.ReceiveLetter(
                                        "电力飞升(心情抚慰)",
                                        "出口电力，得到临时心情BUFF。",
                                        LetterDefOf.RitualOutcomePositive
                                        );
                            }
                        })
                    });
                }
                else if (RandomGG == 4)
                {
                    TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = HotseatGameComponent.RG.Next(2, 8),
                        _Data = Find.RandomPlayerHomeMap,
                        _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                        {
                            if (TheMap != null && TheMap is Map TheMapEx)
                            {
                                var RR = TheMapEx.mapPawns.FreeColonists.RandomElement();
                                if (RR != null && TheMapEx != null)
                                {
                                    //自动维修随机小人身上的装备
                                    HotseatGameComponent.RepairAllApparelsInOnePawn(RR);

                                    Find.LetterStack.ReceiveLetter(
                                        $"电力飞升 ({RR.Name}) 装备被自动修理",
                                        "电力出口奖励。",
                                        LetterDefOf.RitualOutcomePositive
                                        );
                                }
                            }
                        })
                    });
                }
                else if (RandomGG == 5)
                {
                    TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = HotseatGameComponent.RG.Next(2, 8),
                        _Data = Find.RandomPlayerHomeMap,
                        _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                        {
                            var TheRR = false;
                            if (TheMap != null && TheMap is Map TheMapEx)
                            {
                                var RR = TheMapEx.mapPawns.FreeColonists.RandomElement();
                                if (RR != null && TheMapEx != null)
                                {
                                    //为小人添加加速状态
                                    var plagueOnPawn = RR.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerXingHongZuZhou_DianLi"));
                                    if (plagueOnPawn != null)
                                    {
                                        //如果已经感染流感，则不作处理
                                        //plagueOnPawn.Severity += randomSeverity;
                                    }
                                    else
                                    {
                                        TheRR = true;
                                        //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                                        Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerXingHongZuZhou_DianLi"), RR);
                                        //把状态添加到被击中的目标身上
                                        RR.health.AddHediff(hediff);
                                    }

                                    if (TheRR)
                                    {
                                        Find.LetterStack.ReceiveLetter(
                                            $"电力飞升 ({RR.Name}) 进入加速状态",
                                            "电力出口奖励。",
                                            LetterDefOf.RitualOutcomePositive
                                            );
                                    }
                                }
                            }
                        })
                    });
                }
                else if (RandomGG == 6)
                {
                    TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = HotseatGameComponent.RG.Next(2, 8),
                        _Data = Find.RandomPlayerHomeMap,
                        _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                        {
                            var TheRR = false;
                            if (TheMap != null && TheMap is Map TheMapEx)
                            {
                                var RR = TheMapEx.mapPawns.FreeColonists.RandomElement();
                                if (RR != null && TheMapEx != null)
                                {
                                    //为小人添加加速状态
                                    var plagueOnPawn = RR.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerBengKuiHouYi"));
                                    if (plagueOnPawn != null)
                                    {
                                        //如果已经感染流感，则不作处理
                                        //plagueOnPawn.Severity += randomSeverity;
                                    }
                                    else
                                    {
                                        TheRR = true;
                                        //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                                        Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerBengKuiHouYi"), RR);
                                        //把状态添加到被击中的目标身上
                                        RR.health.AddHediff(hediff);
                                    }

                                    //
                                    plagueOnPawn = RR.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GerFoodMan"));
                                    if (plagueOnPawn != null)
                                    {
                                        //如果已经感染流感，则不作处理
                                        //plagueOnPawn.Severity += randomSeverity;
                                    }
                                    else
                                    {
                                        TheRR = true;
                                        //我们调用HediffMaker.MakeHediff生成一个新的hediff状态，类型就是我们之前设置过的HediffDefOf.Plague瘟疫类型
                                        Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("GerFoodMan"), RR);
                                        //把状态添加到被击中的目标身上
                                        RR.health.AddHediff(hediff);
                                    }

                                    if (TheRR)
                                    {
                                        Find.LetterStack.ReceiveLetter(
                                            $"电力飞升 ({RR.Name}) 进入崩溃觉醒状态",
                                            "电力出口奖励。",
                                            LetterDefOf.RitualOutcomePositive
                                            );
                                    }
                                }
                            }
                        })
                    });
                }
                else if (RandomGG == 7) 
                {
                    
                    TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = HotseatGameComponent.RG.Next(2, 8),
                        _Data = map,
                        _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                        {
                            if (TheMap != null && TheMap is Map TheMapEx)
                            {
                                List<Thing> contents = new List<Thing> { ThingMaker.MakeThing(ThingDefOf.Silver) };
                                var SliverToGive = HotseatGameComponent.RG.Next(1, 10000);
                                contents.FirstOrDefault().stackCount = SliverToGive;

                                var dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                                var refugee = ThingUtility.FindPawn(contents);
                                if (refugee != null) refugee.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;

                                Find.LetterStack.ReceiveLetter(
                                                $"电力飞升(获得收入({SliverToGive}))",
                                                "出口电力，得到飞升物资补给包",
                                                LetterDefOf.RitualOutcomePositive,
                                                new TargetInfo(dropSpot, Find.CurrentMap, false)
                                                );

                                var podInfo = new ActiveDropPodInfo();
                                podInfo.innerContainer.TryAddRangeOrTransfer(contents);
                                podInfo.openDelay = 100;
                                podInfo.leaveSlag = false;
                                if (dropSpot != null) DropPodUtility.MakeDropPodAt(dropSpot, Find.CurrentMap, podInfo);
                            }
                        })
                    });
                }

                if (RandomGG == 100) 
                {
                    TheTimerManager.AddTimerSmall(("MoGui_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                    {
                        TTL = HotseatGameComponent.RG.Next(2, 8),
                        _Data = Find.RandomPlayerHomeMap,
                        _OnTimerDownWithOB = new System.Action<object>(TheMap =>
                        {
                            if (TheMap != null && TheMap is Map TheMapEx)
                            {
                                if (TheMapEx != null)
                                {
                                    var RR = TheMapEx.mapPawns.FreeColonists.RandomElement();
                                    if (RR.health.hediffSet.hediffs.Where((Hediff hd) => hd.IsPermanent() || hd.def.chronic).TryRandomElement(out var result))
                                    {
                                        HealthUtility.Cure(result);
                                        Find.LetterStack.ReceiveLetter(
                                                        $"电力飞升 ({RR.Name}) 的随机永久负面状态被移除!",
                                                        "电力出口奖励。",
                                                        LetterDefOf.RitualOutcomePositive
                                                        );
                                    }
                                }
                            }
                        })
                    });
                }

                //上传电力到星际网络的请求
                WebSocketHelper.DoWebSocketSendSync($"#PlayerDianUpload#{SteamUtility.SteamPersonaName}#{totalEnergy}#");
            }
        }

        /// <summary>
        /// 显示电力上传询问框
        /// </summary>
        public static void ShowDianUploadGui(int BCount, float TotalDianLiang)
        {
            DiaNode diaNode = TheDN = new DiaNode("是否上传【多余电力】到星际网络？");
            DiaOption val = new DiaOption($"同意上传 { BCount } 电池 { TotalDianLiang } Wd 电力")
            {
                action = delegate
                {
                    DoUpdate();
                },
                resolveTree = true
            };

            diaNode.options.Add(val);

            DiaOption val2 = new DiaOption($"同意上传 {BCount} 电池 {TotalDianLiang} Wd 电力(后面5次自动上传)")
            {
                action = delegate
                {
                    AutoUploadCount = 5;
                    DoUpdate();
                },
                resolveTree = true
            };

            diaNode.options.Add(val2);

            DiaOption val3 = new DiaOption($"同意上传 {BCount} 电池 {TotalDianLiang} Wd 电力(后面10次自动上传)")
            {
                action = delegate
                {
                    AutoUploadCount = 10;
                    DoUpdate();
                },
                resolveTree = true
            };

            diaNode.options.Add(val3);

            DiaOption val1 = new DiaOption($"拒绝")
            {
                action = delegate
                {
                    TheDN.PreClose();
                },
                resolveTree = true
            };
            diaNode.options.Add(val1);

            DiaOption val4 = new DiaOption($"拒绝(后面5次自动拒绝)")
            {
                action = delegate
                {
                    AutoJuJueUploadCount = 5;
                    TheDN.PreClose();
                },
                resolveTree = true
            };

            diaNode.options.Add(val4);

            DiaOption val5 = new DiaOption($"拒绝(后面10次自动拒绝)")
            {
                action = delegate
                {
                    AutoJuJueUploadCount = 10;
                    TheDN.PreClose();
                },
                resolveTree = true
            };

            diaNode.options.Add(val5);

            //超时时间自动关闭对话框
            Task.Run(() =>
            {
                Task.Delay(10000);
                TheDN?.PreClose();
            });
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, "星际电网系统"));
        }
    }
}
