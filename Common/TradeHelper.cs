using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncident
{
    public static class TradeHelper
    {
        public static string GetItemsNameStr(this List<ThingDef> Items) 
        { 
            StringBuilder stringBuilder = new StringBuilder();
            foreach (ThingDef item in Items) 
            {
                stringBuilder.AppendLine(item.defName);
            }

            return stringBuilder.ToString();
        }

        public static void RandomTrade(this Map map)
        {
            var CurrentPlayerBaseMoney = Find.RandomPlayerHomeMap.CurrentColonySilver();
            //当前殖民地白银多于10000
            if (CurrentPlayerBaseMoney <= 5000) return;
            var TotalPrice = 0.0f;
            var MaxTryCount = 0;

            List<ThingDef> Result = new List<ThingDef>();
            while (TotalPrice < CurrentPlayerBaseMoney)
            {
                //最大尝试次数限制，防止死循环
                if(MaxTryCount++>=1000) break;
                // 查找物品类型
                var thingDefs =
                        (from def in DefDatabase<ThingDef>.AllDefs
                         where def.race == null && def.IsCorpse == false && def.LabelCap != null && def.BaseMarketValue != 0f
                         select def).ToList().RandomElement();
                TotalPrice += thingDefs.BaseMarketValue;
                //将随机物品加入到清单中
                Result.Add(thingDefs);
            }

            DiaNode diaNode = new DiaNode("随机交易 \r\n \r\n" + Result.GetItemsNameStr());
            DiaOption val = new DiaOption($"支付{CurrentPlayerBaseMoney / 2.0f}白银，得到这些物品")
            {
                action = delegate
                {
                    //删除金钱
                    map.RemoveMoney(CurrentPlayerBaseMoney / 2);

                    IntVec3 intVec = DropCellFinder.TradeDropSpot(map);

                    List<Thing> things = new List<Thing>();

                    foreach (var item in Result)
                    {
                        var rr = ThingMaker.MakeThing(item);
                        try
                        {
                            rr.stackCount = 1;
                        }
                        catch { }
                        things.Add(rr);
                    }

                    DropPodUtility.DropThingsNear(intVec, map, things, 100, false, false, false, false);

                    Find.LetterStack.ReceiveLetter($"交易完毕(物品已发送)", "物品清单 \r\n \r\n" + Result.GetItemsNameStr(), LetterDefOf.NeutralEvent, null, null);
                },
                resolveTree = true
            };

            diaNode.options.Add(val);

            DiaOption val1 = new DiaOption($"拒绝")
            {
                action = delegate
                {
                    diaNode?.PreClose();
                },
                resolveTree = true
            };

            diaNode.options.Add(val1);

            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, "星际交易请求"));

            //返回随机的物品清单
            //return Result;
        }

        /// <summary>
        /// 移除地图上的金钱
        /// </summary>
        public static void RemoveMoney(this Map map, int num)
        {
            int cost = num;
            foreach (Thing s in TradeUtility.AllLaunchableThingsForTrade(map))
            {
                if (cost <= 0) break;
                if (s.def == ThingDefOf.Silver)
                {
                    if (cost < s.stackCount)
                    {
                        s.stackCount -= cost;
                        cost = 0;
                    }
                    else
                    {
                        cost -= s.stackCount;
                        s.Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }

        /// <summary>
        /// 得到指定地图的金钱
        /// </summary>
        public static int CurrentColonySilver(this Map map)
        {
            var colonySilver = (from t in TradeUtility.AllLaunchableThingsForTrade(map)
                                       where t.def == ThingDefOf.Silver
                                       select t).Sum((Thing t) => t.stackCount);
            return colonySilver;
        }
        public static bool HasEnoughMoney(this Map map,int num)
        {
            return TradeUtility.ColonyHasEnoughSilver(map, num);
        }


        /// <summary>
        /// 随机启动金钱购买东西事件
        /// </summary>
        /// <param name="map"></param>
        public static void StartBuyXinQing(this Map map) 
        {
            if (map == null) return;
            if (map.mapPawns.FreeColonists.Count(XX => XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart11234"))!=null) != 0) return;
            //得到当前地图小人所有白银
            var CurrentPlayerBaseMoney = Find.RandomPlayerHomeMap.CurrentColonySilver();
            //当前殖民地白银多于10000
            if (CurrentPlayerBaseMoney >= 10000) 
            {
                DiaNode diaNode = new DiaNode("特殊交易请求(心情)");
                DiaOption val = new DiaOption($"支付10000白银，得到全员+10心情增益，持续10天。")
                {
                    action = delegate
                    {
                        //删除金钱
                        map.RemoveMoney(10000);
                        //DoUpdate();
                        map.mapPawns.FreeColonists.ToList().ForEach(XX =>
                        {
                            var TheGerBig = XX.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerStart11234"));
                            if (TheGerBig == null) XX.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerStart11234"));
                        });

                        Find.LetterStack.ReceiveLetter($"特殊补给激活(心情)", "全员获得心情增益。", LetterDefOf.NeutralEvent, null, null);
                    },
                    resolveTree = true
                };

                diaNode.options.Add(val);

                DiaOption val1 = new DiaOption($"拒绝")
                {
                    action = delegate
                    {
                        diaNode?.PreClose();
                    },
                    resolveTree = true
                };

                diaNode.options.Add(val1);

                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, "星际交易请求"));
            }
        }

        public static void StartBuyShiWu(this Map map)
        {
            if (map == null) return;            
            //得到当前地图小人所有白银
            var CurrentPlayerBaseMoney = Find.RandomPlayerHomeMap.CurrentColonySilver();
            //当前殖民地白银多于10000
            if (CurrentPlayerBaseMoney >= 2000)
            {
                DiaNode diaNode = new DiaNode("特殊交易请求(食物)");
                DiaOption val = new DiaOption($"支付2000白银，得到2000肉干饼。")
                {
                    action = delegate
                    {
                        //DoUpdate();
                        map.RemoveMoney(2000);
                        IntVec3 intVec = DropCellFinder.TradeDropSpot(map);

                        var rr = ThingMaker.MakeThing(ThingDef.Named("Pemmican"));
                        rr.stackCount = 2000;
                        DropPodUtility.DropThingsNear(intVec, map, new List<Thing> { rr }, 100, false, false, false, false);

                        Find.LetterStack.ReceiveLetter($"特殊补给激活(食物)", "获得大量肉干补给。", LetterDefOf.NeutralEvent, new TargetInfo(intVec, map, false), null);
                    },
                    resolveTree = true
            };

                diaNode.options.Add(val);

                DiaOption val1 = new DiaOption($"拒绝")
                {
                    action = delegate
                    {
                        diaNode?.PreClose();
                    },
                    resolveTree = true
                };

                diaNode.options.Add(val1);

                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, "星际交易请求"));
            }
        }
    }
}
