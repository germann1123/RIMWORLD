using MeteorIncidentGer;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using WebSocketSharp;

namespace MeteorIncident
{
    public class WebSocketRecive
    {
        /// <summary>
        /// WEBSOCKET客户端接收到服务器消息
        /// </summary>
        public static bool DoWebSocketRecive(object OB, MessageEventArgs bb1) 
        {
            try
            {
                //如果暂停，则不处理数据
                //if (Find.TickManager.Paused) return true;
                string TheData = bb1.Data;
                var TheSData = TheData.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
                //不能在玩家暂停游戏的时候触发事件
                if (TheSData.Length >= 2 && TheSData[0] == "StoryTellerCommand" && Find.TickManager.CurTimeSpeed != 0)
                {

                }
                else if (TheSData.Length >= 2 && TheSData[0] == "ShiYiZheng" && Find.TickManager.CurTimeSpeed != 0)
                {
                    //失忆症
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        var TheLostMemPawn = Find.CurrentMap.mapPawns.FreeColonists.ToList().Count(XX => XX.questTags.Contains("LostMem"));
                        if (TheLostMemPawn == 0)
                        {
                            //随机选取一个小人失去记忆
                            var ThePawn = Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement();
                            if (ThePawn != null)
                            {
                                ThePawn.jobs.EndCurrentJob(JobCondition.Succeeded, false, true);
                                //ThePawn.jobs.StartJob(new Job(JobDefOf.Goto, CellFinder.RandomCell(Find.CurrentMap)), 0, null, false, true, null, null, false, false);
                                var TheJob = JobMaker.MakeJob(JobDefOf.Goto, CellFinder.RandomCell(Find.CurrentMap));
                                ThePawn.jobs.StartJob(TheJob, 0, null, false, true, null, null, false, false);
                                ThePawn.jobs.ClearQueuedJobs(true);

                                //失忆症标签
                                ThePawn.questTags.Add("LostMem");

                                //ThePawn.GetSpouseOrLoverOrFiance();
                                //socialInteractionsManager.TryDevelopNewTrait(pawn, letterText);

                                Find.LetterStack.ReceiveLetter("失忆症蔓延", "您的一些殖民者会在短时间内拒绝执行你的命令", LetterDefOf.NegativeEvent);
                            }
                        }
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "Bad_LeiBaoHaSei" && Find.TickManager.CurTimeSpeed != 0)
                {
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        //极端雷暴
                        if (GenDate.DaysPassed >= 50 && !TheFlash.IsStorm &&
                        TheFlash.KeepOnCount <= 0 && TheFlash.InitDelay <= 0 &&
                        Current.Game.storyteller.def.defName != "RandyEx2")
                        {
                            WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#LeiBao#{SteamUtility.SteamPersonaName}#");

                            Find.LetterStack.ReceiveLetter(
                                "极端雷暴",
                                "由于星球气候不确定因素影响，雷暴将会在短时间内席卷整颗星球",
                                LetterDefOf.NegativeEvent
                                );

                            TheFlash.KeepOnCount = HotseatGameComponent.RG.Next(1, 40);
                            TheFlash.InitDelay = HotseatGameComponent.RG.Next(1, 8);
                        }
                    }));
                }
                //此事件必须当前叙事者为菲比才会触发
                else if (TheSData.Length >= 2 && TheSData[0] == "HappyEnd" && Find.TickManager.CurTimeSpeed != 0 && Current.Game.storyteller.def.defName == "RandyEx334")
                {
                    //小人添加好心情BUFF
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        Find.LetterStack.ReceiveLetter(
                        "快乐",
                        "您的所有殖民者今天不知道为何，异常高兴",
                        LetterDefOf.PositiveEvent
                        );

                        //随机给殖民者加上正面心情BUFF
                        if (HotseatGameComponent.RG.Next(1, 1000) < 933)
                        {
                            //小人添加好心情BUFF
                            Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
                            {
                                var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerBig"));
                                if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerBig"));
                            });
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
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "ResourcePodEvent" && Find.TickManager.CurTimeSpeed != 0)  //服务器空头仓事件
                {
                    //资源舱事件可无限入队
                    if (HotseatGameComponent.queue.Count <= 4) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        HotseatGameComponent.DoTheGerPod(); //补给仓坠毁
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "ManHunterPlayerManWithGerPod" && Find.TickManager.CurTimeSpeed != 0 && Current.Game.storyteller.def.defName == "RandyEx334")  //服务器空头仓事件
                {
                    //精英加入
                    if (HotseatGameComponent.queue.Count <= 4) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        HotseatGameComponent.DoTheGerPod("1"); //补给仓坠毁
                        if (Find.CurrentMap != null) TheManHunterWildMan.ManHunterPlayerMan(Find.CurrentMap);
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "TheBigShortCircuitGer" && Find.TickManager.CurTimeSpeed != 0)  //电气断路+大爆炸
                {
                    //电气短路
                    if (HotseatGameComponent.queue.Count <= 4) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        Building culprit;
                        var TheRandomPlayerMap = Find.RandomPlayerHomeMap;
                        //在玩家随机营地寻找可电气爆炸的建筑物
                        if (!ShortCircuitUtility.GetShortCircuitablePowerConduits(TheRandomPlayerMap).TryRandomElement(out culprit))
                        {
                            return;
                        }
                        ShortCircuitUtility.DoShortCircuit(culprit);
                        //屏蔽掉消息的建筑爆炸方式
                        TheRandomPlayerMap?.RandomBuildingBoom(false);
                        //随机发生其他爆炸
                        if (HotseatGameComponent.RG.Next(1, 1000) < 500) Task.Run(() => { Thread.Sleep(3000); Find.RandomPlayerHomeMap?.RandomBuildingBoom(false); });

                        //立即返回给服务端消息，通告客户端爆炸事件成功执行
                        WebSocketHelper.DoWebSocketSendSync($"#ClientComFanKui#{SteamUtility.SteamPersonaName}#DianBaoZhaOkay#");
                        Find.LetterStack.ReceiveLetter(
                        "电路断路爆炸",
                        "电路断路引发的电池漏点，电器爆炸，以及引发其他一一系列电路爆炸。",
                        LetterDefOf.ThreatBig
                        );
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "DataAsync_APang_Reqeust" && Find.TickManager.CurTimeSpeed != 0)  //接收到队友请求的电量，之后回发
                {
                    //缓存数据，之后队列需要使用
                    if (int.TryParse(TheSData[1], out var TheDataZ))
                    {
                        //如果盟友请求的电力是零，则不处理
                        if (TheDataZ <= 0) return true;
                        STGlobal.PengYouDianWangXu = TheDataZ;
                    }
                    //客户端接收到电网差异请求数据
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
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

                            STGlobal.TotalDianGei += totalEnergy;
                            if (STGlobal.GiveCount++ >= 4)
                            {
                                STGlobal.GiveCount = 0;
                                Find.LetterStack.ReceiveLetter(
                                $"电力输出 {STGlobal.TotalDianGei} Wd",
                                $"您输出的电力总和。",
                                LetterDefOf.NeutralEvent
                                );
                            }

                            //回发服务端，能够提供的总电量
                            WebSocketHelper.DoWebSocketSendSync($"#DataAsync_APang_DoTheGivePower#{totalEnergy}#JuChong#");

                            ////如果友方玩家需要电量大于现有电量
                            //if (STGlobal.PengYouDianWangXu >= totalEnergy)
                            //{
                            //    for (int i = 0; i < powerNet.batteryComps.Count; i++)
                            //    {
                            //        CompPowerBattery compPowerBattery = powerNet.batteryComps[i];

                            //    }


                            //}
                            //else 
                            //{
                            //    var TheZongXuYao = (float)STGlobal.PengYouDianWangXu;
                            //    for (int i = 0; i < powerNet.batteryComps.Count; i++)
                            //    {
                            //        CompPowerBattery compPowerBattery = powerNet.batteryComps[i];
                            //        //如果单个电池的两就已经大于了队友所需量，则直接扣除后退出
                            //        if (compPowerBattery.StoredEnergy >= TheZongXuYao)
                            //        {
                            //            //电池丢失所有电量
                            //            compPowerBattery.DrawPower(TheZongXuYao);

                            //            //回发服务端，能够提供的总电量，这种时候直接返回
                            //            WebSocketHelper.DoWebSocketSendSync($"#DataAsync_APang_DoTheGivePower#{STGlobal.PengYouDianWangXu}#QuanChong#");
                            //            break;
                            //        }
                            //        else 
                            //        {
                            //            TheZongXuYao -= compPowerBattery.StoredEnergy;
                            //            compPowerBattery.DrawPower(compPowerBattery.StoredEnergy);
                            //        }

                            //    }
                            //}
                        }
                    }));
                }
                //
                else if (TheSData.Length >= 2 && TheSData[0] == "DianNetGive" && Find.TickManager.CurTimeSpeed != 0 && PlayerDianDialo.IsChongYunXu)  //服务器随机派发给与电力事件
                {
                    var TheAddDian = STGlobal.APang_ZongDianLiang_Give;
                    if (ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit))
                    {
                        PowerNet powerNet = culprit.PowerComp.PowerNet;
                        //Map map = culprit.Map;
                        List<CompPowerBattery> TheBs = powerNet.batteryComps;
                        if (TheBs.Count == 0)
                        {
                            Find.LetterStack.ReceiveLetter(
                            $"充能失败",
                            $"由于你的电网目前存在问题，充能无法完成，请尽快修缮您的电网系统。",
                            LetterDefOf.NeutralEvent
                            );
                            return true;
                        }

                        var CountChongDianChi = 0;
                        //计算所有电池的当前电量，并算出提供给盟友的电量后回发
                        for (int i = 0; i < powerNet.batteryComps.Count; i++)
                        {
                            CompPowerBattery compPowerBattery = powerNet.batteryComps[i];
                            compPowerBattery.AddEnergy(compPowerBattery.Props.storedEnergyMax * 2.0f);
                            CountChongDianChi++;
                        }

                        Find.LetterStack.ReceiveLetter(
                            $"星际电网 {CountChongDianChi} 电池 充能完毕!",
                        $"来自星际电网的充能。",
                        LetterDefOf.NeutralEvent
                        );

                        //回发服务端，充能成功
                        WebSocketHelper.DoWebSocketSendSync($"#DianNetChongOkay#{SteamUtility.SteamPersonaName}#{TheSData[1]}#");
                    }
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "DataAsync_APang_DoTheGivePower" && Find.TickManager.CurTimeSpeed != 0)  //接收到队友请求的电量，之后回发
                {
                    //缓存数据，之后队列需要使用
                    //if (float.TryParse(TheSData[1], out var TheDataZ))
                    //{
                    //    //得到盟友给与的电量
                    //    STGlobal.APang_ZongDianLiang_Give += TheDataZ;
                    //}
                    //客户端接收到电网差异请求数据
                    //HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    //{
                    //    var TheAddDian = STGlobal.APang_ZongDianLiang_Give;
                    //    if (ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit))
                    //    {
                    //        PowerNet powerNet = culprit.PowerComp.PowerNet;
                    //        //Map map = culprit.Map;
                    //        List<CompPowerBattery> TheBs = powerNet.batteryComps;
                    //        if (TheBs.Count == 0) 
                    //        {
                    //            Find.LetterStack.ReceiveLetter(
                    //            $"充能失败",
                    //            $"由于你的电网目前存在问题，充能无法完成，请尽快修缮您的电网系统。",
                    //            LetterDefOf.NeutralEvent
                    //            );
                    //            return;
                    //        }

                    //        var CountChongDianChi = 0;
                    //        //计算所有电池的当前电量，并算出提供给盟友的电量后回发
                    //        for (int i = 0; i < powerNet.batteryComps.Count; i++)
                    //        {
                    //            CompPowerBattery compPowerBattery = powerNet.batteryComps[i];
                    //            compPowerBattery.AddEnergy(compPowerBattery.Props.storedEnergyMax);
                    //        }

                    //        Find.LetterStack.ReceiveLetter(
                    //         $"星际电网 { CountChongDianChi } 电池 自盟友电力 { TheSData[1] } Wd",
                    //        $"",
                    //        LetterDefOf.NeutralEvent
                    //        );

                    //        //回发服务端，充能成功
                    //        WebSocketHelper.DoWebSocketSendSync($"#DianNetChongOkay#{SteamUtility.SteamPersonaName}#{TheSData[1]}#");
                    //    }
                    //}));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "DataAsync_APang" && Find.TickManager.CurTimeSpeed != 0)  //接收到盟友的电力输送请求
                {
                    //电气短路
                    //if (HotseatGameComponent.queue.Count <= 4) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        //立即返回给服务端消息，通告客户端爆炸事件成功执行
                        //WebSocketHelper.DoWebSocketSendSync($"#ClientComFanKui#{ SteamUtility.SteamPersonaName }#DianBaoZhaOkay#");

                        //缓存数据
                        if (int.TryParse(TheSData[1], out var TheBCount)) STGlobal.APang_BatteryCount = TheBCount;
                        if (int.TryParse(TheSData[2], out var TheBNengLiang)) STGlobal.APang_ZongDianLiang = TheBNengLiang;
                        HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                        {
                            //比较我自己的电力数据
                            if (ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit))
                            {
                                PowerNet powerNet = culprit.PowerComp.PowerNet;
                                Map map = culprit.Map;
                                List<CompPowerBattery> TheBs = powerNet.batteryComps;
                                if (TheBs.Count == 0)
                                {
                                    WebSocketHelper.DoWebSocketSendSync($"#DataAsync_APang_Reqeust#0#");
                                    //如果没有电池，则直接发送不需要电力结果反馈
                                    return;
                                }
                                var totalEnergy = 0.0f;  //现有电力总和
                                var totalEnergyMax = 0.0f; //电网总电力存贮总和
                                for (int i = 0; i < powerNet.batteryComps.Count; i++)
                                {
                                    CompPowerBattery compPowerBattery = powerNet.batteryComps[i];
                                    totalEnergy += compPowerBattery.StoredEnergy;
                                    totalEnergyMax += compPowerBattery.Props.storedEnergyMax;
                                }

                                //我的总电力差回发给服务端，转发到阿胖游戏
                                if (totalEnergy <= (totalEnergyMax / 2))
                                    WebSocketHelper.DoWebSocketSendSync($"#DataAsync_APang_Reqeust#{(totalEnergyMax - totalEnergy)}#");
                                else
                                    WebSocketHelper.DoWebSocketSendSync($"#DataAsync_APang_Reqeust#0#"); //回复盟友不需要电力

                                Find.LetterStack.ReceiveLetter(
                                "DataAsync_APang",
                                $"我的电力距离满差 {totalEnergy} (totalEnergyMax / 2) = {(totalEnergyMax / 2)} 是否缺电 {(totalEnergy <= (totalEnergyMax / 2))}",
                                LetterDefOf.NeutralEvent
                                );
                            }
                            else
                            {
                                WebSocketHelper.DoWebSocketSendSync($"#DataAsync_APang_Reqeust#0#TriggerPlayerHaveNoActiveBaattery#");
                            }
                        }));
                    }));
                }
                //ShortCircuit
                else if (TheSData.Length >= 2 && TheSData[0] == "KuangFengGer" && Find.TickManager.CurTimeSpeed != 0)  //狂风呼啸
                {
                    try
                    {
                        var NumToClear = HotseatGameComponent.RemoveAllFilth();

                        Find.LetterStack.ReceiveLetter(
                        $"狂风呼啸 清理({NumToClear.Item1})污垢，资源转化({NumToClear.ResouceCount})次!",
                        "神奇引力波将污垢转化为可用资源。",
                        LetterDefOf.NeutralEvent
                        );
                    }
                    catch { }

                    ////狂风呼啸
                    //if (HotseatGameComponent.queue.Count <= 4) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    //HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    //{                        

                    //}));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "Msgbox" && Find.TickManager.CurTimeSpeed != 0)  //服务器空头仓事件
                {
                    //事件通告
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        //LetterDef GetMsgTypeByArgr(string Argr)
                        //{
                        //    if (Argr == "NeutralEvent")
                        //    {
                        //        return LetterDefOf.NeutralEvent;
                        //    }
                        //    else if (Argr == "ThreatSmall")
                        //    {
                        //        return LetterDefOf.ThreatSmall;
                        //    }
                        //    else if (Argr == "ThreatBig")
                        //    {
                        //        return LetterDefOf.ThreatBig;
                        //    }
                        //    else if (Argr == "NegativeEvent")
                        //    {
                        //        return LetterDefOf.NegativeEvent;
                        //    }
                        //    else if (Argr == "PositiveEvent")
                        //    {
                        //        return LetterDefOf.PositiveEvent;
                        //    }

                        //    return LetterDefOf.NeutralEvent;
                        //}
                        if (TheSData.Length >= 4)
                        {
                            Messages.Message((TheSData[1] + TheSData[2]), null, MessageTypeDefOf.NeutralEvent, true);
                        }
                        else
                        {
                            Messages.Message((TheSData[1] + TheSData[2]), null, MessageTypeDefOf.NeutralEvent, true);
                        }
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "TheMeter" && Find.TickManager.CurTimeSpeed != 0)  //流星事件
                {
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        RCellFinder.TryFindRandomPawnEntryCell(out var spawnCenter112, Find.CurrentMap, CellFinder.EdgeRoadChance_Animal, false, null);
                        List<Thing> list = ThingSetMakerDefOf.Meteorite.root.Generate();
                        SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, spawnCenter112, Find.CurrentMap);
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "AIDownloadCommand" && Find.TickManager.CurTimeSpeed != 0) 
                {
                    //如果服务器没有准备好事件下发，则忽略
                    if (TheSData[1] != "FALSE" && TheSData[1] != "OK" && TheSData[1] != "DATAERROR" && 
                        HotseatGameComponent.StoryTeller_queue.Count <= 2) //如果事件队列事件过多，则忽略
                    {
                        //接收到服务器下发叙事者指令
                        HotseatGameComponent.StoryTeller_queue.Enqueue(TheSData[1]);
                    }
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "TheMeterRandom" && int.TryParse(TheSData[1], out var Count) && Find.TickManager.CurTimeSpeed != 0)  //流星事件，随机地点流星
                {
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    TheGlobalInfo.MetorRandomCount = Count;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        var TheFinalWave = Rand.Range(1, TheGlobalInfo.MetorRandomCount);
                        //防止值过低，至少要坠落一次流星
                        if (TheFinalWave <= 0) TheFinalWave = 1;
                        if (TheFinalWave == 1) goto SingeMeterForce;
                        for (int i = 0; i < TheFinalWave; i++)
                        {
                            if (i != (TheFinalWave - 1))
                            {
                                TheTimerManager.AddTimerSmall(("MetorCreate_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                                {
                                    TTL = i * 3,
                                    _OnTimerDown = new System.Action(() =>
                                    {
                                        List<Thing> list = ThingSetMakerDefOf.Meteorite.root.Generate();
                                        //for (int loopa = 0; loopa < list.Count; loopa++)
                                        //{
                                        //    if (HotseatGameComponent.RG.Next(1, 10000) < 133) list[loopa] = ThingMaker.MakeThing(ThingDef.Named("MineableSilver"));
                                        //}
                                        var TriggerMap = Find.CurrentMap;
                                        SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, Find.CurrentMap.GetRandomCell(), TriggerMap);
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
                                        List<Thing> list = ThingSetMakerDefOf.Meteorite.root.Generate();
                                        //最后一波流星，会概率在其中夹杂铁矿石
                                        //for (int loopa = 0; loopa < list.Count; loopa++)
                                        //{
                                        //    if(HotseatGameComponent.RG.Next(1,10000) < 2333) list[loopa] = ThingMaker.MakeThing(ThingDef.Named("MineableSteel"));
                                        //}

                                        //最后的陨石要比前两波大好几倍
                                        //for (int loopa = 0; loopa < list.Count; loopa++)
                                        //{
                                        //    list.Add(ThingMaker.MakeThing(ThingDef.Named("MineableSteel")));
                                        //}

                                        //陨石坠落在玩家一个随机基地
                                        var TriggerMap = Find.CurrentMap;
                                        SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, Find.CurrentMap.GetRandomCell(), TriggerMap);
                                    })
                                });
                            }
                        }

                        return;
                    SingeMeterForce:;
                        //单次流星坠落
                        TheTimerManager.AddTimerSmall(("MetorCreate_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
                        {
                            TTL = 1 * 3,
                            _OnTimerDown = new System.Action(() =>
                            {
                                List<Thing> list = ThingSetMakerDefOf.Meteorite.root.Generate();
                                //最后的陨石要比前两波大好几倍
                                //for (int loopa = 0; loopa < list.Count; loopa++)
                                //{
                                //    list.Add(ThingMaker.MakeThing(ThingDef.Named("MineableSteel")));
                                //}

                                var TriggerMap = Find.CurrentMap;
                                SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, Find.CurrentMap.GetRandomCell(), TriggerMap);
                            })
                        });
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "DaJia" && Find.TickManager.CurTimeSpeed != 0 && Current.Game.storyteller.def.defName != "RandyEx334")  //小人打架
                {
                    //立即开始打架
                    HotseatGameComponent.DaJia();
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "PlayerFriendArmyCall" && Find.TickManager.CurTimeSpeed != 0)  //来自其他玩家的友军呼叫
                {
                    //得到呼叫援军玩家名称
                    var CallPlayerName = TheSData[3];
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        StorytellerComp_RandomEventer.FriendlyComeTriggerPlayerName = CallPlayerName;
                        StorytellerComp_RandomEventer.PushFriendlyAttack();
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "KuangBao" && Find.TickManager.CurTimeSpeed != 0 && Current.Game.storyteller.def.defName != "RandyEx334")
                {
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        Find.LetterStack.ReceiveLetter(
                        "暴怒",
                        "一股神秘心灵冲击波正在让你的殖民者变的易怒，他正在失去控制。",
                        LetterDefOf.NegativeEvent
                        );

                        //开始发脾气
                        var TheQiRen = Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement();
                        //如果小人躺在床上，则强制唤醒它
                        if (TheQiRen.InBed()) RestUtility.WakeUp(TheQiRen);
                        TheQiRen.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Tantrum"));
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "Raid" && Find.TickManager.CurTimeSpeed != 0)  //服务器空头仓事件
                {
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        //叙事者必须要是贝鲁特，才会有星际海盗，判断逻辑已经移动至服务端
                        if (float.TryParse(TheSData[1], out var thePoint))
                        {
                            Find.LetterStack.ReceiveLetter("星际海盗(来自其他星系)", "海盗同盟军正在袭击你的基地，这些海盗来自于遥远的其他星系！", LetterDefOf.ThreatBig);

                            //星际海盗会袭击当前地图，如果没有当前地图，玩家处于远行界面，则袭击玩家一个任意总部
                            var TriggerMap = Find.RandomPlayerHomeMap;
                            if (Find.CurrentMap == null) TriggerMap = Find.RandomPlayerHomeMap;
                            else TriggerMap = Find.CurrentMap;

                            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, TriggerMap);
                            incidentParms.forced = true;
                            incidentParms.faction = Find.FactionManager.RandomEnemyFaction() == null ? Faction.OfMechanoids : Find.FactionManager.RandomEnemyFaction();
                            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;

                            //星际海盗高财富值限制
                            if (thePoint >= 4500) thePoint = 4500;

                            //贝鲁特叙事者双倍突袭点数
                            //if (Current.Game.storyteller.def.defName == "RandyEx33") thePoint *= 2;

                            incidentParms.points = thePoint;
                            incidentParms.target = TriggerMap;
                            IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
                        }
                    }));
                    //IncidentDef.Named("ManhunterPack").Worker.TryExecute();
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "InfestationsRaid" && Find.TickManager.CurTimeSpeed != 0 && Current.Game.storyteller.def.defName != "RandyEx334") //微型虫灾，虫灾发生在地图随机点上
                {
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        if (float.TryParse(TheSData[1], out var thePoint))
                        {
                            var TriggerMap = Find.RandomPlayerHomeMap;
                            TunnelHiveSpawner tunnelHiveSpawner = (TunnelHiveSpawner)ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner, null);
                            tunnelHiveSpawner.spawnHive = false;
                            tunnelHiveSpawner.insectsPoints = Mathf.Clamp(thePoint * Rand.Range(0.3f, 0.6f), 200f, 1000f);
                            tunnelHiveSpawner.spawnedByInfestationThingComp = true;
                            GenSpawn.Spawn(tunnelHiveSpawner, TriggerMap.GetRandomCell(), TriggerMap, WipeMode.FullRefund);
                        }
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "PanicFlee" && Find.TickManager.CurTimeSpeed != 0 && Current.Game.storyteller.def.defName == "RandyEx33")
                {
                    //小人随机逃跑
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        var RI = DefDatabase<MentalStateDef>.AllDefsListForReading.Where(XX => XX.defName == "PanicFlee").FirstOrDefault();
                        if (RI != null)
                        {
                            var TheQiRen = Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement();
                            TheQiRen.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("PanicFlee"));
                        }
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "BuildingBoom" && Find.TickManager.CurTimeSpeed != 0) //建筑剧烈爆炸
                {
                    //if (queue.Count != 0) while (queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        Find.RandomPlayerHomeMap.RandomBuildingBoom();
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "ShiXie" && Find.TickManager.CurTimeSpeed != 0)  //玩家阵营失血症
                {
                    //全员失血症状
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        HotseatGameComponent.TakeAllBloodLess();
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "ShenMiKongTou" && Find.TickManager.CurTimeSpeed != 0)
                {
                    //刷新神秘空投
                    if (HotseatGameComponent.queue.Count != 0) while (HotseatGameComponent.queue.TryDequeue(out var TheAction)) ;
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        HotseatGameComponent.IsCreateHive = false;
                        HotseatGameComponent.DoSpawnKongTou();
                    }));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "buluoxiji" && Find.TickManager.CurTimeSpeed != 0)
                {
                    //HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    //{
                    //    try
                    //    {
                    //        //部落隐藏兵
                    //        Find.LetterStack.ReceiveLetter(
                    //            "部落袭击",
                    //            "部落开始疯狂袭击你的殖民地，它们似乎早有预谋",
                    //            LetterDefOf.NeutralEvent
                    //            );

                    //        //踏马随机选择一个玩家基地
                    //        var PlayerRandomHome = Find.RandomPlayerHomeMap;
                    //        //踏马删除上次所有小人
                    //        TheGlobalInfo._BuLuoTouXiGroup.ForEach(XX => XX.Destroy());

                    //        //部落人数因生存天数而定 - 直接刷新在玩家基地随机点
                    //        for (int loopa = 0; loopa < HotseatGameComponent.RG.Next(1, (2 + (GenDate.DaysPassed / 50))); loopa++)
                    //        {
                    //            var ThePawn = HotseatGameComponent.CreateEnemy(CellHelper.GetRandomCell(), PlayerRandomHome, PawnKindDefOf.t);

                    //            //踏马缓存刷新部落小人
                    //            TheGlobalInfo._BuLuoTouXiGroup.AddRange(ThePawn);
                    //        }
                    //    }
                    //    catch { }
                    //}));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "Zerg_Attack_bigWave" && Find.TickManager.CurTimeSpeed != 0)
                {
                    //HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    //{
                    //    Find.LetterStack.ReceiveLetter(
                    //    "虫族大规模进攻进行中",
                    //    "空投箱中的爆炸物吸引了区域内虫族，它们将在8小时后扑向这片区域",
                    //    LetterDefOf.NegativeEvent
                    //    );

                    //    TheTimerManager.AddTimer(("ZergAttack_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
                    //    {
                    //        TTL = 8,
                    //        _OnTimerDown = new System.Action(() =>
                    //        {
                    //            HotseatGameComponent.DoZergAttackWithGroupCount(Rand.Range(2, 6));
                    //        })
                    //    });
                    //}));
                }
                else if (TheSData.Length >= 2 && TheSData[0] == "Random_Pawn_SouBi_Repair" && Find.TickManager.CurTimeSpeed != 0 && Current.Game.storyteller.def.defName == "RandyEx334")
                {
                    HotseatGameComponent.queue.Enqueue(new System.Action(() =>
                    {
                        //自动维修小人身上的装备
                        HotseatGameComponent.RepairAllApparelsInPawn();
                    }));
                }
            }
            catch { }

            return false;
        }
    }
}
