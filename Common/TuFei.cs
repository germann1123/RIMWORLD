using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncident
{
    public class StationHelper
    {
        public static Random RG { get; set; } = new Random(Guid.NewGuid().GetHashCode());
        /// <summary>
        /// 在地图上创建一个土匪营地
        /// </summary>
        public static void CreateTuFeiYingDe(int TuFeiTanCount)
        {
            //刷出远行队探索点
            var CountOfItemStash1 = Find.WorldObjects.Sites.Count(XX =>
            {
                if (XX.questTags != null && (XX.questTags.Contains("Ger_Yuan_Tan"))) return true;
                return false;
            });

            //if (CountOfItemStash1 == 0)
            //{                
            //    var AAA = TileFinder.TryFindNewSiteTile(out var num3, 1, 7, false, TileFinderMode.Random, -1, false);
            //    //bool flag2 = Find.WorldGrid[num2].temperature > 0f && Find.WorldGrid[num2].hilliness != Hilliness.Mountainous;
            //    if (AAA)
            //    {
            //        WorldObjectName
            //        WorldObjectMaker.MakeWorldObject();                    
            //    }
            //}

            //统计土匪营地当前数量，如果营地被玩家占领，GerCreate1标记就会消失
            CountOfItemStash1 = Find.WorldObjects.Sites.Count(XX =>
            {
                if (XX.questTags != null && (XX.questTags.Contains("GerCreate1"))) return true;
                return false;
            });

            //刷出土匪营地需要概率支持
            if (CountOfItemStash1 == 0 && RG.Next(1,1000) < 29)
            {
                var AAA = TileFinder.TryFindNewSiteTile(out var num3, 1, 2, false, TileFinderMode.Random, -1, false);
                //bool flag2 = Find.WorldGrid[num2].temperature > 0f && Find.WorldGrid[num2].hilliness != Hilliness.Mountainous;
                if (AAA)
                {
                    Find.LetterStack.ReceiveLetter("土匪营地", "一个藏匿资源的土匪营地被发现", LetterDefOf.NegativeEvent);
                    var Part1 = SitePartDefOf.BanditCamp;
                    var Part3 = SiteMakerHelper.SitePartDefsWithTag("ItemStashGer").FirstOrDefault();
                    //追加一个站点信息显示器
                    var Part4 = SiteMakerHelper.SitePartDefsWithTag("TheComment_Ger").FirstOrDefault();

                    //var ThingOut = ThingSetMakerDefOf.ResourcePod.root.Generate();
                    StringBuilder SB = new StringBuilder();
                    //ThingOut.ForEach(XX12 => SB.Append($"{ XX12.def.defName.Translate() } 价值:{ XX12.def.BaseMarketValue * XX12.stackCount }"));
                    //SB.Append("\r\n");

                    //随机各种地图状态 - 地图中的随机事件
                    TheGlobalInfo.DoTheRandom();

                    //随机敌人防线刷新
                    TheGlobalInfo.IsEmemyForceDefenc = false;
                    if (RG.Next(1, 1000) < 26 && GenDate.DaysPassed >= 50)
                    {
                        TheGlobalInfo.IsEmemyForceDefenc = true;
                        SB.Append("土匪防守着他们重要走私品，抢在海盗运走物资之前拦截它们。\r\n");
                    }

                    if (TheGlobalInfo.IsEmemyHasGun) 
                    {
                        SB.Append("此地盘踞全副武装海盗士兵，他们正在疯狂抢劫物资。\r\n");
                    }

                    if (TheGlobalInfo.IsFriendlyGunSoiler) 
                    {
                        SB.Append("此地友军部队正在与敌军交战。\r\n");
                    }

                    if (TheGlobalInfo.IsEmemyBigForce)
                    {
                        SB.Append("敌军伏击圈。\r\n");
                    }

                    if (TheGlobalInfo.IsGanRouBing)
                    {
                        SB.Append("食物仓库。\r\n");
                    }

                    //决定登场敌人数量
                    TheGlobalInfo.NumberOfEmemyDefence = RG.Next(1, (2 + (TuFeiTanCount / 5)));
                    SB.Append($"敌人：{ (TheGlobalInfo.NumberOfEmemyDefence + 1) } \r\n");

                    var TheSitPartList = new List<SitePartDef>();
                    TheSitPartList.Add(Part3);  //添加宝藏房

                    //添加敌人防线
                    if (TheGlobalInfo.IsEmemyForceDefenc) TheSitPartList.Add(Part1);

                    TheGlobalInfo.ChongFuJiCount = 0;
                    //刷新虫族伏击 - 虫族伏击 100天后有概率出现
                    if (RG.Next(1, 1000) < 16 && GenDate.DaysPassed >= 100)
                    {
                        TheGlobalInfo.ChongFuJiCount = RG.Next(5, 10);
                        SB.Append($"潜伏虫族: { TheGlobalInfo.ChongFuJiCount }\r\n");
                    }

                    //土匪图藏匿医疗用品，每次土匪据点出现，12.6概率刷新
                    TheGlobalInfo.IsYiliao = false;
                    if (RG.Next(1, 1000) < 126)
                    {
                        TheGlobalInfo.IsYiliao = true;
                        SB.Append($"医疗物资与遗失的黄金。\r\n");
                    }

                    //动态地图探索点标识，标识这个点有多少敌人，多少物资之类
                    if (TheGlobalInfo.IsNoResouce) SB.Append($"资源匮乏\r\n");
                    if (TheGlobalInfo.IsMoreResource) SB.Append($"资源丰富\r\n");
                    if (TheGlobalInfo.IsStoneChemfuelRoom) SB.Append($"遗失的化合燃料\r\n");
                    if (TheGlobalInfo.IsJieLueZhe) SB.Append($"土匪空军机群经过\r\n");
                    if (TheGlobalInfo.IsBuLuoYinCangBing) SB.Append($"你的袭击将会引来部落疯狂报复。\r\n");
                    if (TheGlobalInfo.IsYuKuaiLvXing) SB.Append($"愉快的野外旅行\r\n");

                    Part4.label = SB.ToString();
                    //添加信息站点
                    TheSitPartList.Add(Part4);


                    Site site1 = SiteMaker.MakeSite(TheSitPartList, num3, Find.World.factionManager.RandomEnemyFaction(), true, null);
                    site1.GetComponent<TimeoutComp>().StartTimeout(4 * 24 * 2500);
                    site1.questTags = new List<string> { "GerCreate1" };
                    Find.WorldObjects.Add(site1);
                }
            }

            var CountOfItemStash2 = Find.WorldObjects.Sites.Count(XX =>
            {
                if (XX.questTags != null && (XX.questTags.Contains("Ger_Friendly_Base"))) return true;
                return false;
            });

            //玩家有盟友派系才会生成友军基地
            if (CountOfItemStash2 == 0 && Find.FactionManager.RandomAlliedFaction() != null) 
            {
                var AAA = TileFinder.TryFindNewSiteTile(out var num3, 1, 10, false, TileFinderMode.Random, -1, false);
                //追加一个站点信息显示器
                var Part4 = SiteMakerHelper.SitePartDefsWithTag("TheComment_Ger1").FirstOrDefault();
                var Part3 = SiteMakerHelper.SitePartDefsWithTag("ItemStashGer1").FirstOrDefault();

                var TheSitPartList = new List<SitePartDef>();
                
                TheSitPartList.Add(Part4);  //添加宝藏房
                TheSitPartList.Add(Part3);  //添加宝藏房

                Site site1 = SiteMaker.MakeSite(TheSitPartList, num3, Find.World.factionManager.RandomEnemyFaction(), true, null);
                if (!SteamUtility.SteamPersonaName.Contains("神经病有所"))
                    site1.GetComponent<TimeoutComp>().StartTimeout(2 * 24 * 2500);
                site1.questTags = new List<string> { "Ger_Friendly_Base" };
                Find.WorldObjects.Add(site1);
            }
        }

        public static void MakeFriends()
        {
            var TheFaction = Find.FactionManager.RandomNonHostileFaction();
            //如果已经没有友好派系
            if (TheFaction == null)
            {
                //随机选取一个敌对派系缓和关系
                var TheWillBeGoodFaction = Find.FactionManager.RandomEnemyFaction(false);
                if (TheWillBeGoodFaction != Find.FactionManager.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter(
                        "坚定的盟友",
                        "盟友在任何情况下，都会毫不犹豫选择相信你，无论你对它做过任何过分的事情！",
                        LetterDefOf.PositiveEvent
                        );
                    TheWillBeGoodFaction.SetRelationDirect(Find.FactionManager.OfPlayer, FactionRelationKind.Ally);
                    TheWillBeGoodFaction.TryAffectGoodwillWith(Find.FactionManager.OfPlayer, 0);

                    return;
                }
            }

            var CountFriends = Find.FactionManager.AllFactions.Where(XX1 => XX1 != Find.FactionManager.OfPlayer && XX1.RelationKindWith(Find.FactionManager.OfPlayer) == FactionRelationKind.Ally);
            //foreach (var TF in TheFaction.Where(XX=>XX != Find.FactionManager.OfPlayer)) 
            //{
            if (!TheFaction.HostileTo(Find.FactionManager.OfPlayer) && TheFaction.GoodwillWith(Find.FactionManager.OfPlayer) < 75 && CountFriends.Count() == 0)
            {
                //Find.LetterStack.ReceiveLetter(
                //	"盟友(军事援助)",
                //	"地图上的一个派系和你建立了友好的关系,并且立即向你派遣了军事援助！",
                //	LetterDefOf.PositiveEvent
                //	);
                TheFaction.TryAffectGoodwillWith(Find.FactionManager.OfPlayer, 100);
                TheFaction.SetRelationDirect(Find.FactionManager.OfPlayer, FactionRelationKind.Ally);

                Find.LetterStack.ReceiveLetter(
                    "派系主动结盟",
                    "其中一个派系主动和你缔结同盟关系，盟友永远支持你的一切决定！",
                    LetterDefOf.PositiveEvent
                    );
                //break;
            }
        }
    }
}
