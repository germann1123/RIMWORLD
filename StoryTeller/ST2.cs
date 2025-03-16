using MeteorIncident;
using RimWorld;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace MeteorIncidentGer
{
	public class StorytellerCompProperties_RandomEventer : StorytellerCompProperties
	{
		/// <summary>
		/// 每一个叙事者周期的触发概率
		/// </summary>
		public float TriggerChance;
		public string IncidentStr;
		public string DebugCommentStr;
		public string NextCircleWaitTick;
		public string StartUpWaitTick;
		public string FriendlyAttackInDangerChance;
		public string FriendlyAttackInDangerPoint;
		public string FriendlyAttackWaitTick;
		public string JianJinSuanFa;

        //public IncidentCategoryDef IncidentCategory
        //{
        //	get
        //	{
        //		if (incident != null)
        //		{
        //			return incident.category;
        //		}
        //		return category;
        //	}
        //}

        public StorytellerCompProperties_RandomEventer()
		{
			compClass = typeof(StorytellerComp_RandomEventer);
		}

		//public override IEnumerable<string> ConfigErrors(StorytellerDef parentDef)
		//{
		//	if (incident != null && category != null)
		//	{
		//		yield return "incident and category should not both be defined";
		//	}
		//	if (onDays <= 0f)
		//	{
		//		yield return "onDays must be above zero";
		//	}
		//	if (numIncidentsRange.TrueMax <= 0f)
		//	{
		//		yield return "numIncidentRange not configured";
		//	}
		//	if (minSpacingDays * numIncidentsRange.TrueMax > onDays * 0.9f)
		//	{
		//		yield return "minSpacingDays too high compared to max number of incidents.";
		//	}
		//}
	}

	public class StorytellerComp_RandomEventer : StorytellerComp
	{
		protected StorytellerCompProperties_RandomEventer Props => (StorytellerCompProperties_RandomEventer)props;
		public static System.Random RG { get; set; } = new System.Random(Guid.NewGuid().GetHashCode());
		/// <summary>
		/// 配置文件数据存贮
		/// </summary>
		public static ConcurrentDictionary<string, List<string>> _Cfg = new ConcurrentDictionary<string, List<string>>();
		/// <summary>
		/// 抑制器
		/// </summary>
		public static ConcurrentDictionary<string, int> _CfgWaitCircle = new ConcurrentDictionary<string, int>();
		/// <summary>
		/// 起始抑制器
		/// </summary>
        public static ConcurrentDictionary<string, int> _CfgStartWaitCircle = new ConcurrentDictionary<string, int>();

        public static ConcurrentDictionary<string, string> _CfgJianJinSuanFa = new ConcurrentDictionary<string, string>();

   //     public override void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = null)
   //     {
			////如果不是小人死亡，则不处理
   //         //if (!p.RaceProps.Humanlike || !p.IsColonist) return;
   //         //if (ev == AdaptationEvent.Died || ev == AdaptationEvent.Kidnapped || ev == AdaptationEvent.LostBecauseMapClosed || ev == AdaptationEvent.Downed)
			////敌人小人死亡
			//if(ev == AdaptationEvent.Died)
   //         {
			//	WebSocketHelper.DoWebSocketSendSync($"#EnemyPawnKilled#");

			//	Find.LetterStack.ReceiveLetter(
			//			$"(测试)=>{(p.Name == null ? "动物非人类生物" : p.Name.ToStringFull)} 死亡",
			//			"一支受雇与其他玩家的友军部队来到你的星系。",
			//			LetterDefOf.NeutralEvent
			//			);

			//	//foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
			//	//{
			//	//    if (current.RaceProps.Humanlike && !current.Downed)
			//	//    {
			//	//        return;
			//	//    }
			//	//}
			//	//Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
			//	//if (anyPlayerHomeMap != null)
			//	//{
			//	//    IncidentParms parms = StorytellerUtility.DefaultParmsNow(this.Props.incident.category, anyPlayerHomeMap);
			//	//    if (this.Props.incident.Worker.CanFireNow(parms))
			//	//    {
			//	//        QueuedIncident qi = new QueuedIncident(new FiringIncident(this.Props.incident, this, parms), Find.TickManager.TicksGame + this.Props.delayTicks, 0);
			//	//        Find.Storyteller.incidentQueue.Add(qi);
			//	//    }
			//	//}
			//}
   //     }

		public static bool IsFriendlyComeByOtherPlayer = false;
		public static string FriendlyComeTriggerPlayerName = "";
		/// <summary>
		/// 呼叫友军支援 
		/// </summary>		
		public static void PushFriendlyAttack()=>IsFriendlyComeByOtherPlayer = true;


		public static bool DebugFlag = true;
        /// <summary>
        /// 
        /// </summary>
        public static DateTime _DieseaseLimiteSecond = DateTime.Now;
		public static DateTime _DianLimiteSecond = DateTime.Now;

		public static bool IsFriendlyCome = true;

		/// <summary>
		/// 遭遇敌人时，友军支援
		/// </summary>
        public static bool IsFriendlyCome_Zhi = true;

        public static bool IsEmemyCome = false;
        public static bool IsEmemyComeTimerOkay = false;

        /// <summary>
        /// 立即在下一个轮训周期触发空投舱坠毁请求
        /// </summary>
        public static bool IsKongTouRequest = false;
		public static bool IsZhuiBingRequest = false;
		public static bool IsRandomEventStart = false;
		/// <summary>
		/// 空投触发请求3次连续随机叙事者事件请求
		/// </summary>
		public static bool IsRandomEventStart3 { get; set; } = false;

		/// <summary>
		/// 为0时候在一个非老家地图产生婴儿空投事件
		/// </summary>
        public static uint BabyIsCreate { get; set; } = 0;

        /// <summary>
        /// 落魄贵族进攻参数
        /// </summary>
        public static (bool LuoPoGui, int Count) _LuoPoGuiZu = (LuoPoGui: false, Count: 0);

		public static IIncidentTarget CurrentTarget = null;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			CurrentTarget = target;
			//if(Find.CurrentMap != null)
			//	Log.Message($"地图危险=>{ Find.CurrentMap.dangerWatcher.DangerRating }");

            if (!StoryTellerManager._Cfg.ContainsKey(Props.DebugCommentStr)) 
			{
				StoryTellerManager._Cfg.TryAdd(Props.DebugCommentStr,this);
			}

            //起始抑制器配置
            if (Props.StartUpWaitTick != null && Props.DebugCommentStr != null)
            {
                if (!_CfgStartWaitCircle.ContainsKey(Props.DebugCommentStr) )
                {
                    _CfgStartWaitCircle.TryAdd(Props.DebugCommentStr, Props.StartUpWaitTick.GetStrTime().GetRandomTime());
                }
            }

			//在初始抑制器到期前不会动作
			if (_CfgStartWaitCircle.TryGetValue(Props.DebugCommentStr, out var TheTime) && TheTime > 0) 
			{
				if (SteamUtility.SteamPersonaName.Contains("神经病有所") &&
                 Find.TickManager.CurTimeSpeed == TimeSpeed.Normal)
				{
                    _CfgStartWaitCircle[Props.DebugCommentStr]-=3;
                }
				else 
				{
                    _CfgStartWaitCircle[Props.DebugCommentStr]--;
                }
                yield break;
            }

            //抑制器配置
            if (Props.NextCircleWaitTick != null && Props.DebugCommentStr != null) 
			{
				//var TheYiZhiTime = Props.NextCircleWaitTick.GetStrTime().GetRandomTime();
				var TheYiZhiTime = 0;
				if (!_CfgWaitCircle.ContainsKey(Props.DebugCommentStr) && TheYiZhiTime != -100) 
				{
					_CfgWaitCircle.TryAdd(Props.DebugCommentStr, TheYiZhiTime);
				}
			}

            //渐进算法参数
            if (Props.JianJinSuanFa != null && Props.DebugCommentStr != null)
            {
                if (!_CfgJianJinSuanFa.ContainsKey(Props.DebugCommentStr))
                {
                    _CfgJianJinSuanFa.TryAdd(Props.DebugCommentStr, Props.JianJinSuanFa);
                }
            }

            //友军根据概率来支援
            if (Find.CurrentMap != null && Props.FriendlyAttackInDangerChance != null && Props.FriendlyAttackInDangerPoint != null
                && int.TryParse(Props.FriendlyAttackInDangerChance,out var ChanceFriend) && RG.Next(1,1000) < ChanceFriend 
				&& Find.CurrentMap.dangerWatcher.DangerRating != StoryDanger.None && IsFriendlyCome_Zhi && Props.FriendlyAttackWaitTick != null) 
			{
                try
                {
                    var TheF = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("RaidFriendly"));
                    if (TheF != null)
                    {
                        void DoTaskInTime(int Delay, System.Action _DoTask = null)
                        {
							new Thread(() =>
							{
								Thread.Sleep(Delay);
								_DoTask?.Invoke();
							})
							{ IsBackground=true, }.Start();
                        }

                        //Log.Message("Okay1!");
                        IncidentParms parms = GenerateParms(TheF.category, target);
                        parms.target = Find.CurrentMap;
						//设置友军强度
                        parms.points = Props.FriendlyAttackInDangerPoint.GetStrTime().GetRandomTime().GetNoErrorInt();
                        //if (TheF.Worker.CanFireNow(parms))
                        {

                            //执行增援
                            var RR2 = TheF.Worker.TryExecute(parms);
                            if (RR2)
                            {
								//如果盟友成功派出友军，则等待这个时间后，再次派出
								DoTaskInTime((Props.FriendlyAttackWaitTick.GetStrTime().GetRandomTime().GetNoErrorInt()) *1000,()=> IsFriendlyCome_Zhi=true);
                                IsFriendlyCome_Zhi = false;
                                yield break;
                            }

                            DoTaskInTime(RG.Next(1, 20), () => IsFriendlyCome_Zhi = true);
                        }
                    }
                }
                catch { }
            }

			//RefugeePodCrash_Baby
			if (BabyIsCreate <= 0 && Find.CurrentMap != null && !Find.CurrentMap.IsPlayerHome && 
				RG.Next(1, 1000) < 200 && Current.Game.storyteller.def.defName == "RandyEx444337788") //孤儿叙事者专用
			{
                var TheF = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("RefugeePodCrash_Baby"));
                IncidentParms parms = GenerateParms(TheF.category, target);
                parms.target = Find.CurrentMap;

                var RR2 = TheF.Worker.TryExecute(parms);
                if (RR2)
                {
                    BabyIsCreate = 11;

                    void DoTaskInTime(int Delay, System.Action _DoTask = null)
                    {
                        new Thread(() =>
                        {
                            Thread.Sleep(Delay);
                            _DoTask?.Invoke();
                        })
                        { IsBackground = true, }.Start();
                    }

					DoTaskInTime(RG.Next(360, 888) * 1000, () => BabyIsCreate = 0);
                    yield break;
                }
            }

			//在线叙事者
			if (HotseatGameComponent.StoryTeller_queue.TryDequeue(out var TheStoryTeller)) 
			{
				//测试分支
				if (SteamUtility.SteamPersonaName.Contains("神经病有所"))
				{
					Find.LetterStack.ReceiveLetter(
						$"在线随机事件(阿胖测试)=>{TheStoryTeller}",
						"一支受雇与其他玩家的友军部队来到你的星系。",
						LetterDefOf.NeutralEvent
						);
				}
				var TheF = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains(TheStoryTeller));
                IncidentParms parms = GenerateParms(TheF.category, target);
                parms.target = Find.CurrentMap;
                var RR2 = TheF.Worker.TryExecute(parms);
                yield break;
            }

            //空投执行连续三次叙事者随机事件3
            if (IsRandomEventStart3)
			{
				IsRandomEventStart3 = false;

				//循环三次触发随机事件
                for (int i = 0; i < 3; i++)
                {
					ReChoice1:;
					//完全随机选取一个事件
					var RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();

					//如果命中的事件不为空，则执行它
					if (RI != null)
					{
						IncidentParms parms = GenerateParms(RI.category, target);
						var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
						var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

						parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
						//50%概率叙事者时间会切换到当前地图
						if (Find.CurrentMap != null && RG.Next(1, 1000) < 500) parms.target = Find.CurrentMap;
						if (RI.Worker.CanFireNow(parms)) RI.Worker.TryExecute(parms);
						else goto ReChoice1;
					}
				}
			}

			//使用空投开启随机事件处理
			if (IsRandomEventStart) 
			{
				IsRandomEventStart = false;
				ReChoice1:;
				//完全随机选取一个事件
				var RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();

				//每次随机选择，只有91%概率会擦除疾病事件 ，当命中疾病事件的时候，91概率切换到其他随机事件
				if (RG.Next(1, 1000) < 912)
				{
					while (true)
					{
						if (RI.defName.Contains("Disease"))
						{
							RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();
							continue;
						}
						else break;  //如果不是疾病事件，则通过
					}
				}

				//如果是疾病事件
				if (RI.defName.Contains("Disease"))
				{
					if ((DateTime.Now - _DieseaseLimiteSecond).TotalSeconds < 3600)
					{
						//如果半小时内命中疾病事件，则选择其他事件
						while (true)
						{
							if (RI.defName.Contains("Disease"))
							{
								RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();
								continue;
							}
							else break;  //如果不是疾病事件，则通过
						}
					}
					else
					{
						//如果是疾病事件，则记录当前时间
						_DieseaseLimiteSecond = DateTime.Now;
					}
				}



				//如果命中的事件不为空，则执行它
				if (RI != null)
				{
					IncidentParms parms = GenerateParms(RI.category, target);
					var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
					var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

					parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
					//50%概率叙事者时间会切换到当前地图
					if (Find.CurrentMap != null && RG.Next(1, 1000) < 500) parms.target = Find.CurrentMap;
					if (RI.Worker.CanFireNow(parms)) RI.Worker.TryExecute(parms);
					else goto ReChoice1;
				}
			}

			//处理其他玩家友军突袭请求
			if (IsFriendlyComeByOtherPlayer) 
			{
				try
				{
					var RI = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName.Contains("RaidFriendly")).FirstOrDefault();
					if (RI == null) yield break;
					IncidentParms parms = GenerateParms(RI.category, target);
					var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
					var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);
					//if (RI.Worker.CanFireNow(parms))
					//{
					var R2 = RI.Worker.TryExecute(parms);
					//空投舱成功坠落，清理标志
					if (R2)
					{
						IsFriendlyComeByOtherPlayer = false;

						//立即返回给服务端消息，通告客户端爆炸事件成功执行
						WebSocketHelper.DoWebSocketSendSync($"#ClientComFanKui#{SteamUtility.SteamPersonaName}#QiTaPlayerFriendlyAttack#{FriendlyComeTriggerPlayerName}#");

						Find.LetterStack.ReceiveLetter(
								$"友军(来自玩家 {FriendlyComeTriggerPlayerName} 赞助)",
								"一支受雇与其他玩家的友军部队来到你的星系。",
								LetterDefOf.RitualOutcomePositive
								);

                        //SoundDefOf.un.PlayOneShot(new TargetInfo(TheCell, Find.CurrentMap, false));
                    }
					//}
				}
				catch 
				{
					IsFriendlyComeByOtherPlayer = false;
                }

				yield break;
			}

			//落魄贵族触发的友军事件
			if (_LuoPoGuiZu.LuoPoGui)
			{
				try
				{
					var RI = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName.Contains("RaidFriendly")).FirstOrDefault();
					if (RI == null) yield break;
					IncidentParms parms = GenerateParms(RI.category, target);
					var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
					var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);
					var R2 = RI.Worker.TryExecute(parms);
					//空投舱成功坠落，清理标志
					if (R2)
					{
						_LuoPoGuiZu.LuoPoGui = false;
					}
				}
				catch 
				{
					//出错就不执行
					_LuoPoGuiZu.LuoPoGui = false;
                }

				yield break;
			}

			//空投舱坠毁事件触发 - 优先处理
			if (IsKongTouRequest)
			{
				//找到资源舱坠毁事件
				var RI = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName.Contains("ResourcePodCrash")).FirstOrDefault();
				var DoCount = RG.Next(1, 4);
                for (int i = 0; i < DoCount; i++)
                {
					//如果命中的事件不为空，则执行它
					if (RI == null) break;
					IncidentParms parms = GenerateParms(RI.category, target);
					var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
					var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

					parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
					//50%概率叙事者时间会切换到当前地图
					if (Find.CurrentMap != null && RG.Next(1, 1000) < 500) parms.target = Find.CurrentMap;
					if (RI.Worker.CanFireNow(parms))
					{
						var R2 = RI.Worker.TryExecute(parms);
						//空投舱成功坠落，清理标志
						if (R2)
						{
							IsKongTouRequest = false;
						}
					}
				}

				yield break;
			}

			//叙事者必须不是温柔的菲比，追兵事件不能在遭遇战图发生
			if (Find.CurrentMap != null && !Find.CurrentMap.IsPlayerHome && IsEmemyCome
				&& Current.Game.storyteller.def.defName != "RandyEx334" && !Find.CurrentMap.Parent.def.defName.Contains("Ambush"))
			{
				//Log.Message("Okay!");
				IsEmemyCome = false;
				try
				{
					IncidentDef TheF = null;
					//RaidEnemy
					if (RG.Next(1,10000) < 5000)
						TheF = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("ManhunterPack"));
					else
						TheF = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("RaidEnemy"));

					if (RG.Next(1, 10000) < 2000) TheF = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("MechCluster"));

					if (TheF != null)
					{
						//Log.Message("Okay1!");
						IncidentParms parms = GenerateParms(TheF.category, target);
						parms.target = Find.CurrentMap;

						//随机追兵强度比率
						parms.points = StorytellerUtility.DefaultThreatPointsNow(Find.World)*(float)(RG.NextDouble()+1.0);
						//if (TheF.Worker.CanFireNow(parms))
						{
							int NextTimerExpire = Rand.Range(1, 1200);

							WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#ZhuiBing-Attack#{ SteamUtility.SteamPersonaName }#");
							//执行增援
							TheF.Worker.TryExecute(parms);
						}
					}
				}
				catch { }
				yield break;
			}

			//如果在其他地图上，则有一定概率盟友支援
			if (Find.CurrentMap != null && !Find.CurrentMap.IsPlayerHome && IsFriendlyCome && RG.Next(1,10000) < 25)
			{				
				//Log.Message("Okay!");
				try
				{
					var TheF = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("RaidFriendly"));
					if (TheF != null)
					{						
						//Log.Message("Okay1!");
						IncidentParms parms = GenerateParms(TheF.category, target);
						parms.target = Find.CurrentMap;
						parms.points = RG.Next(1, 1000);
						//if (TheF.Worker.CanFireNow(parms))
						{
							
							//执行增援
							var RR2 = TheF.Worker.TryExecute(parms);
							if (RR2) 
							{
                                Find.LetterStack.ReceiveLetter(
                                "友军支援",
                                "您的盟友派系派出了部队和你一起并肩作战，铁杆盟友永远支持你的一切决定，你不是一个人在战斗。",
                                LetterDefOf.PositiveEvent
                                );
                            }
							IsFriendlyCome = false;

							TheTimerManager.AddTimer(("ItemStash_TraderCaravanArrival" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
							{
								TTL = Rand.Range(1, 100),
								_OnTimerDown = () => IsFriendlyCome = true, //下一次友军支援判定
							});
						}
					}
				}
				catch { }
				yield break;
			}

            if (SteamUtility.SteamPersonaName.Contains("神经病有所") && 
				_CfgWaitCircle.ContainsKey(Props.DebugCommentStr) && 
				_CfgWaitCircle[Props.DebugCommentStr] != -100 && _CfgWaitCircle[Props.DebugCommentStr] > 0 &&
                 Find.TickManager.CurTimeSpeed  == TimeSpeed.Normal)
            {
                _CfgStartWaitCircle[Props.DebugCommentStr] -= 2;
            }

            //如果叙事者没有到达指定抑制器定时，则忽略其中的命中
            if (_CfgWaitCircle.ContainsKey(Props.DebugCommentStr) && _CfgWaitCircle[Props.DebugCommentStr] != -100 && _CfgWaitCircle[Props.DebugCommentStr]-- > 0)
			{
				//Log.Message($"_CfgWaitCircle[Props.DebugCommentStr]=>{ _CfgWaitCircle[Props.DebugCommentStr] }");
				yield break;
			}
			//重新记录抑制器计时
			_CfgWaitCircle[Props.DebugCommentStr] = Props.NextCircleWaitTick.GetStrTime().GetRandomTime();

			if (_CfgJianJinSuanFa.TryGetValue(Props.DebugCommentStr, out var TheData56)) 
			{ 
				var JianJinCanShu = TheData56.GetJianJinSuanStr();
				if (JianJinCanShu.IsEnable) 
				{
                    //Find.LetterStack.ReceiveLetter(
                    //            "调试",
                    //            $"GenDate.DaysPassed = {GenDate.DaysPassed} JianJinCanShu.DayPassed={JianJinCanShu.DayPassed}" +
                    //            $" JianJinCanShu.ChuXi={JianJinCanShu.ChuXi} ",
                    //            LetterDefOf.PositiveEvent
                    //            );
                    var DayResult = (58 / JianJinCanShu.DayPassed) * JianJinCanShu.ChuXi;
					//Find.LetterStack.ReceiveLetter(
					//			"调试",
					//			$"DayResult = {DayResult}",
					//			LetterDefOf.PositiveEvent
					//			);
					//            Find.LetterStack.ReceiveLetter(
					//                        "调试",
					//                        $"GenDate.DaysPassed = {GenDate.DaysPassed} JianJinCanShu.DayPassed={JianJinCanShu.DayPassed}" +
					//$" JianJinCanShu.ChuXi={JianJinCanShu.ChuXi} ",
					//                        LetterDefOf.PositiveEvent
					//                        );
					//渐进参数修改间隔
					_CfgWaitCircle[Props.DebugCommentStr] = _CfgWaitCircle[Props.DebugCommentStr] / (DayResult == 0 ? 1 : DayResult);
                }
			}

            //通过概率决定事件是否触发
            if (!((int)Props.TriggerChance > RG.Next(1, 1000)))
			{
				//如果没有配置渐进参数，则按照默认规则来
				if (!_CfgJianJinSuanFa.TryGetValue(Props.DebugCommentStr, out var TheData1))
				{
					//如果没有命中，则下一次延迟减半
					_CfgWaitCircle[Props.DebugCommentStr] /= 2;
				}
                yield break;
			}

            //解析STR参数
            var DataSplit = Props.IncidentStr.Trim().Replace("\r\n", "").Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
			//Log.Message(DataSplit.Length.ToString());
			//输出调试信息
			//StringBuilder SB = new StringBuilder();
			//SB.Append(Props.DebugCommentStr + " ");
			DataSplit.ToList().ForEach(XX1 =>
			{
				if (!_Cfg.ContainsKey(Props.DebugCommentStr))
				{
					_Cfg.TryAdd(Props.DebugCommentStr, new List<string> { XX1 });
				}
				else
				{
					if(!_Cfg[Props.DebugCommentStr].Contains(XX1))_Cfg[Props.DebugCommentStr].Add(XX1);
				}
			});
			//Log.Message($"_Cfg[Props.DebugCommentStr]={ _Cfg[Props.DebugCommentStr].Count }");

			bool RR = false;
			string TheResult = "";
			//根据权重随机选择元素
			if (_Cfg.ContainsKey(Props.DebugCommentStr) && _Cfg[Props.DebugCommentStr].Count > 1)
			{
				RR = _Cfg[Props.DebugCommentStr].TryRandomElementByWeight(XX1 =>
				{
					var SplitStr = XX1.Split(new string[] { "item" }, StringSplitOptions.RemoveEmptyEntries);
					//Log.Message($"SplitStr[0].Trim()={ SplitStr[0].Trim() }");
					if (SplitStr[0].Trim().Contains("DelayTask") && int.TryParse(SplitStr[2].Trim(), out var TheValue11))
					{
						//Log.Message($"TheValue11={ TheValue11 }");
						return TheValue11;
					}

					if (SplitStr.Length >= 2 && int.TryParse(SplitStr[1].Trim(), out var TheValue)) return TheValue;

					//Log.Message("TryRandomElementByWeight=0");
					return 0;
				}, out var TheResult1);
				TheResult = TheResult1;
			}
			if (_Cfg.ContainsKey(Props.DebugCommentStr) && _Cfg[Props.DebugCommentStr].Count == 1)
			{
				TheResult = _Cfg[Props.DebugCommentStr][0];
				RR = true;
			}

			//用于驯兽师菲比，如果随机到的事件还在进行倒计时，就随机下一个
		RestartGoto:;

			//Log.Message(RR.ToString());
			//当第一个参数给NOTHING的时候，忽略本次命中
			if (RR)
			{
				//if (TheResult.Contains("DelayTask")) 
				//{
				//	Log.Message(TheResult);
				//}
				var IndintName = TheResult.Split(new string[] { "item" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().Replace("\r\n", "");
				//Log.Message(IndintName);
				var TheArgrments = TheResult.Split(new string[] { "item" }, StringSplitOptions.RemoveEmptyEntries);
				if (IndintName != "Nothing")
				{
					//强制将别名转换为真实游戏事件
					if (IndintName == "RaidEnemy_Ger") 
					{
                        Find.LetterStack.ReceiveLetter(
                        $"老子的袭击",
                        "测试。",
                        LetterDefOf.ThreatBig
                        );
                        IndintName = "RaidEnemy";
                    }

					var allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName == IndintName);
					if (allDefsListForReading.Count() != 0)  //官方事件
					{
						IncidentParms parms = GenerateParms(allDefsListForReading.FirstOrDefault().category, target);

						var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
						var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

						parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
						if (SteamUtility.SteamPersonaName.Contains("神经病有所"))
						{
							//parms.points = RG.Next(1, (int)(parms.points));
							if (IndintName == "RaidEnemy")
							{
								Find.LetterStack.ReceiveLetter(
									$"袭击点数变更(四分之一)",
									"测试。",
									LetterDefOf.NeutralEvent
									);
							}
							parms.points /= 4.0f;
						}

                        var RRTest = allDefsListForReading.FirstOrDefault().Worker.TryExecute(parms);
						if (RRTest && SteamUtility.SteamPersonaName.Contains("神经病有所")
							&& IndintName == "RaidEnemy") 
						{
							Find.LetterStack.ReceiveLetter(
									$"成功生成事件",
									"测试。",
									LetterDefOf.NeutralEvent
									);
						}
					}
					else if (IndintName == "JiangGuoZhang")  //浆果生长事件
					{
						var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
						var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

						(TriggerMap.FirstOrDefault() ?? Find.CurrentMap).JiangGuoZhang();
					}
					else if (IndintName == "RandomIncident") //随机叙事者事件
					{
					ReChoice:;
						//完全随机选取一个事件
						var RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();

						//StringBuilder SB = new StringBuilder();
						//for (int i = 0; i < 10; i++) 
						//{
						//	SB.Append($"本次随机事件 {DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement().defName}#");
						//                  }

						//SB.AppendLine("<End>");

						//System.IO.File.AppendAllText("D:\\RandomOutput.txt", SB.ToString());

						#region 疾病时间抑制
						//每次随机选择，只有91%概率会擦除疾病事件 ，当命中疾病事件的时候，91概率切换到其他随机事件
						if (RG.Next(1, 1000) < 912)
						{
							while (true)
							{
								if (RI.defName.Contains("Disease"))
								{
									RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();
									continue;
								}
								else break;  //如果不是疾病事件，则通过
							}
						}

						//如果是疾病事件
						if (RI.defName.Contains("Disease"))
						{
							if ((DateTime.Now - _DieseaseLimiteSecond).TotalSeconds < 3600)
							{
								//如果半小时内命中疾病事件，则选择其他事件
								while (true)
								{
									if (RI.defName.Contains("Disease"))
									{
										RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();
										continue;
									}
									else break;  //如果不是疾病事件，则通过
								}
							}
							else
							{
								//如果是疾病事件，则记录当前时间
								_DieseaseLimiteSecond = DateTime.Now;
							}
						}
						#endregion

						//电路爆炸最小间隔，五分钟来一次,根据天数，这个阀值会改变
						//if (RG.Next(1, 10000) < 999 && (DateTime.Now - _DianLimiteSecond).TotalSeconds > (GenDate.DaysPassed <= 100 ? 200 : 50)) 
						//{
						//	_DianLimiteSecond = DateTime.Now;
						//	//强制随机为电路断路
						//	RI = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("ShortCircuit"));
						//}

						bool TryFindRandomBlightablePlant(Map map, out Plant plant)
						{
							IEnumerable<Thing> arg_2C_0 = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant);
							bool arg_40_0 = arg_2C_0.TryRandomElement(out var thing);
							plant = (Plant)thing;
							return arg_40_0;
						}

						//if (RG.Next(1, 10000) < 599 && !TryFindRandomBlightablePlant(Find.RandomPlayerHomeMap, out var plant1))
						//{
						//	//在建筑附近点火
						//	bool TryStartFireNearPlant(Plant b)
						//	{
						//		STGlobal.tmpCells.Clear();
						//		int num = GenRadial.NumCellsInRadius(4f);
						//		CellRect startRect = b.OccupiedRect();
						//		for (int i = 0; i < num; i++)
						//		{
						//			IntVec3 intVec = b.Position + GenRadial.RadialPattern[i];
						//			if (GenSight.LineOfSight(b.Position, intVec, b.Map, startRect, CellRect.SingleCell(intVec), null) && FireUtility.ChanceToStartFireIn(intVec, b.Map) > 0f)
						//			{
						//				STGlobal.tmpCells.Add(intVec);
						//			}
						//		}
						//		return STGlobal.tmpCells.Any() && FireUtility.TryStartFireIn(STGlobal.tmpCells.RandomElement<IntVec3>(), b.Map, Rand.Range(0.1f, 1.75f));
						//	}

						//	var RRPlantFire = TryStartFireNearPlant(plant1);
						//	if (RRPlantFire) 
						//	{
						//		Find.LetterStack.ReceiveLetter(
						//		$"作物区火灾",
						//		"你的作物区由于一些意外情况，发生火灾。",
						//		LetterDefOf.NegativeEvent
						//		);
						//		goto TheEnd;
						//	}
						//	//return false;
						//}

						if (RG.Next(1, 10000) < 28 && Current.Game.storyteller.def.defName != "RandyEx2")
						{
							Find.LetterStack.ReceiveLetter(
							"火力覆盖",
							"土匪火炮正在对你进行几轮轮狂轰滥炸，请及时指挥您的殖民者疏散避难！",
							LetterDefOf.ThreatBig
							);

							for (int i = 0; i < 2; i++)
							{
								TheTimerManager.AddTimer(("TuFeiHuoLi_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
								{
									TTL = i * 1,
									_OnTimerDown = new System.Action(() =>
									{
										for (int loopa = 0; loopa < RG.Next(1, 6); loopa++)
										{
											Projectile projectile = (Projectile)GenSpawn.Spawn(ThingDef.Named("Bullet_Shell_HighExplosive_Ger"), CellFinder.RandomEdgeCell(Find.CurrentMap), Find.CurrentMap);
											var TheTriggerCell = CellHelper.GetRandomCell();
											//projectile.Launch(Find.CurrentMap.mapPawns.AllPawns.RandomElement(), TheTriggerCell, CellHelper.GetRandomCell(), ProjectileHitFlags.NonTargetWorld, false, null);
											projectile.Launch(null, TheTriggerCell, CellHelper.GetRandomCell(), ProjectileHitFlags.NonTargetWorld, false, null);
										}
									})
								});
							}

							for (int i = 0; i < 8; i++)
							{
								Projectile projectile = (Projectile)GenSpawn.Spawn(ThingDef.Named("Bullet_Shell_HighExplosive_Ger"), CellFinder.RandomEdgeCell(Find.CurrentMap), Find.CurrentMap);
								var TheTriggerCell = CellHelper.GetRandomCell();
								//projectile.Launch(Find.CurrentMap.mapPawns.AllPawns.RandomElement(), TheTriggerCell, CellHelper.GetRandomCell(), ProjectileHitFlags.NonTargetWorld, false, null);
								projectile.Launch(null, TheTriggerCell, CellHelper.GetRandomCell(), ProjectileHitFlags.NonTargetWorld, false, null);
							}

							goto TheEnd;
						}

						//暴食狂欢
						if (RG.Next(1, 10000) < 55 && Current.Game.storyteller.def.defName != "RandyEx2")
						{
							var TheP = Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement();
							TheP.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Binging_Food"));

							//添加暴食狂欢的BUFF
							var TheGerBig = TheP.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("GerBaoShiKuangHuan"));
							if (TheGerBig == null) TheP.needs.mood.thoughts.memories.TryGainMemoryFast(ThoughtDef.Named("GerBaoShiKuangHuan"));

							var RCount = Find.CurrentMap.mapPawns.FreeColonists.ToList().
								Count(XX => XX.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("Binging_Food"));
							//if (RCount != 0)
							//{
							Find.LetterStack.ReceiveLetter(
								$"暴食狂欢 {TheP.Name}",
								"你的殖民者正在通过暴饮暴食提高心情。",
								LetterDefOf.ThreatBig
								);
							//}
						}

						//随机电路火灾
						//if (RG.Next(1, 10000) < 0)
						//{
						//	//在建筑附近点火
						//	bool TryStartFireNear(Building b)
						//	{
						//		STGlobal.tmpCells.Clear();
						//		int num = GenRadial.NumCellsInRadius(3f);
						//		CellRect startRect = b.OccupiedRect();
						//		for (int i = 0; i < num; i++)
						//		{
						//			IntVec3 intVec = b.Position + GenRadial.RadialPattern[i];
						//			if (GenSight.LineOfSight(b.Position, intVec, b.Map, startRect, CellRect.SingleCell(intVec), null) && FireUtility.ChanceToStartFireIn(intVec, b.Map) > 0f)
						//			{
						//				STGlobal.tmpCells.Add(intVec);
						//			}
						//		}
						//		return STGlobal.tmpCells.Any() && FireUtility.TryStartFireIn(STGlobal.tmpCells.RandomElement<IntVec3>(), b.Map, Rand.Range(0.1f, 1.75f));
						//	}
						//	var Count = RG.Next(1, 6);
						//	var SaiSyouKeiKa = false;
						//                      for (int loopa = 0; loopa < Count; loopa++)
						//                      {
						//		//找到一个随机建筑，发生火灾
						//		if (ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit)) 
						//		{
						//			PowerNet powerNet = culprit.PowerComp.PowerNet;
						//			Map map = culprit.Map;
						//			//List<CompPowerBattery> arg_47_0 = powerNet.batteryComps;
						//			var TmpRR = TryStartFireNear(culprit);
						//			if (TmpRR && !SaiSyouKeiKa) SaiSyouKeiKa = true;
						//		}
						//                      }
						//	if (SaiSyouKeiKa) 
						//	{
						//		Find.LetterStack.ReceiveLetter(
						//		$"电路大规模火灾",
						//		"你的电路由于一些意外情况，发生火灾。",
						//		LetterDefOf.ThreatBig
						//		);
						//		goto TheEnd;
						//	}
						//}

						//随机电路火灾
						if (RG.Next(1, 10000) < 0)
						{
							if (ShortCircuitUtility.GetShortCircuitablePowerConduits(Find.RandomPlayerHomeMap).TryRandomElement(out var culprit) &&
								culprit.PowerComp.PowerNet.batteryComps.Count != 0)
							{
								PowerNet powerNet = culprit.PowerComp.PowerNet;
								Map map = culprit.Map;
								List<CompPowerBattery> TheBs = powerNet.batteryComps;
								var ThePosBs = TheBs.RandomElement();
								ThePosBs.DrawPower(ThePosBs.StoredEnergy);

								GenExplosion.DoExplosion(culprit.Position, Find.CurrentMap, 1, DamageDefOf.Flame, null, 2, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);
								culprit.Destroy();

								Find.LetterStack.ReceiveLetter(
								$"超载爆炸",
								"电路紊乱导致电线爆炸，并且某些电池会受到影响丢失所有电力。",
								LetterDefOf.NegativeEvent
								);

								goto TheEnd;
								//GenExplosion.DoExplosion(ThePosBs.re, Find.CurrentMap, 1, DamageDefOf.Flame, null, 1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null);
							}
						}

						//破坏农田
						//if (RG.Next(1, 10000) < 555 && Current.Game.storyteller.def.defName != "RandyEx2")
						//{
						//	//强制破坏农田
						//	RI = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("CropBlight"));
						//}

						//连续动物发狂
						if (RG.Next(1, 10000) < 55 && Current.Game.storyteller.def.defName != "RandyEx2")
						{
							STGlobal.AnimalKuangCount = 1;
						}
						if (STGlobal.AnimalKuangCount-- > 0)
						{
							//goto TheEnd; //跳过
							//强制破坏农田
							RI = DefDatabase<IncidentDef>.AllDefsListForReading.Find(XX => XX.defName.Contains("AnimalInsanitySingle"));
						}

						////特殊猎杀人类 - 玩家殖民地小人大于八人
						//if (RG.Next(1, 10000) < 15 && Find.CurrentMap !=null && 
						//	Find.CurrentMap.mapPawns.FreeColonists.Count >= 8)
						//{
						//	var CountManHunter = (GenDate.DaysPassed / 10 <= 0 ? 1 : (GenDate.DaysPassed / 10));
						//	var TheXijiRR = Find.RandomPlayerHomeMap.DoRandomManHunterPack(GenDate.DaysPassed * 5, CountManHunter);
						//	if (TheXijiRR) 
						//	{
						//		Find.LetterStack.ReceiveLetter(
						//		$"猎杀人类({CountManHunter} 只发狂动物)",
						//		"一群发狂动物迅速地，强壮地，乱窜进了你的基地试图绕开你的阵地，它们会在任何地方向你发动攻击。",
						//		LetterDefOf.ThreatBig
						//		);
						//		goto TheEnd;
						//	}
						//}

						//有毒尘埃倒计时
						if (HotseatGameComponent.WuYouDuChengAi_TimeOut > 0)
							HotseatGameComponent.WuYouDuChengAi_TimeOut--;
						//如果命中的事件不为空，则执行它
						if (RI != null)
						{
							//老路叙事者禁用闪电风暴
							if (Current.Game.storyteller.def.defName == "RandyEx2" && RI.defName == "Flashstorm")
							{
								goto ReChoice;
							}

                            if (RI.defName == "RaidEnemy" && SteamUtility.SteamPersonaName.Contains("神经病有所"))
                            {
                                Find.LetterStack.ReceiveLetter(
                                $"敌人袭来",
                                "敌人部队来到你的基地附近。",
                                LetterDefOf.NegativeEvent
                                );
                            }

                            IncidentParms parms = GenerateParms(RI.category, target);
							var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
							var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

							parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
							//50%概率叙事者时间会切换到当前地图
							if (Find.CurrentMap != null && RG.Next(1, 1000) < 500) parms.target = Find.CurrentMap;
							if (RI.Worker.CanFireNow(parms))
							{
								//避开有毒尘埃
								if (RI.defName == "ToxicFallout" && HotseatGameComponent.WuYouDuChengAi_TimeOut > 0) goto ReChoice;

								//ToxicFallout
								if (HotseatGameComponent.FindTheKeyTo_WuYouDuChengAi && RI.defName == "ToxicFallout")
								{
									HotseatGameComponent.FindTheKeyTo_WuYouDuChengAi = false;
									Find.LetterStack.ReceiveLetter(
									"有毒尘埃(阻挡)",
									"预防措施控制有毒尘埃直接消散，这个保护将会持续一段时间。",
									LetterDefOf.NeutralEvent
									);

									//保持一段时间不再有有毒尘埃
									HotseatGameComponent.WuYouDuChengAi_TimeOut = RG.Next(100, 800);
									goto ReChoice;
								}

								//动物发狂计数器递减
								//if (STGlobal.AnimalKuangCount > 0) STGlobal.AnimalKuangCount--;
								RI.Worker.TryExecute(parms);
							}
							else
							{
								////如果当前没有合适动物进行发狂，则创建一个随机的进行发狂
								//if (RI.defName.Contains("AnimalInsanitySingle")) 
								//{
								//	//如果动物发狂失败，说明现目前没有合适的单个发狂动物，创建一个动物进行发狂
								//	var TheMapCreate = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
								//	if (TheMapCreate == null) TheMapCreate = Find.RandomPlayerHomeMap;
								//	//根据天数决定出现发狂动物数量
								//	var TheXijiRR = TheMapCreate?.DoRandomManHunterPack(GenDate.DaysPassed * 5, GenDate.DaysPassed<50 ? 1 : (RG.Next(1,(GenDate.DaysPassed/25+1))));
								//	if (TheXijiRR ?? false)
								//	{
								//		Find.LetterStack.ReceiveLetter(
								//		"动物猎杀人类(隐藏动物)",
								//		"当前地图上的动物有大概率会发狂，并开始袭击人类。",
								//		LetterDefOf.NegativeEvent
								//		);
								//	}
								//                        }
								goto ReChoice;
							}
						}

					TheEnd:;
					}
					else if (IndintName == "ManHunterWildMan" && TheArgrments != null &&
						TheArgrments.Length >= 4 && int.TryParse(TheArgrments[3], out var TheMaxDay) &&
						GenDate.DaysPassed > TheMaxDay) //进图野人猎杀人类,必须在特定天数后发生
					{

						var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
						var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);
						//在当前图刷新野人
						TriggerMap.FirstOrDefault()?.ManHunterWildMan();
					}
					else if (IndintName == "XieShen") //随机叙事者事件
					{

						var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
						var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);
						//在当前图刷新野人
						TriggerMap.FirstOrDefault()?.DoTheGuShen();
					}
					else if (IndintName == "DoTheHardJob" && Find.CurrentMap != null)
					{
					//贝鲁特叙事者专用
					TheReChangeBeiLuTe:;
						int TheRandomBank = RG.Next(1, 10);
						if (TheRandomBank == 1)
						{
							//第一个灾难
							Find.LetterStack.ReceiveLetter(
							"火力覆盖(贝鲁特)",
							"土匪火炮正在对你进行5轮狂轰滥炸，请及时指挥您的殖民者疏散避难！",
							LetterDefOf.ThreatBig
							);

							for (int i = 0; i < 2; i++)
							{
								TheTimerManager.AddTimer(("TuFeiHuoLi_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
								{
									TTL = i * 1,
									_OnTimerDown = new System.Action(() =>
									{
										for (int loopa = 0; loopa < RG.Next(1, 15); loopa++)
										{
											Projectile projectile = (Projectile)GenSpawn.Spawn(ThingDef.Named("Bullet_Shell_HighExplosive_Ger"), CellFinder.RandomEdgeCell(Find.CurrentMap), Find.CurrentMap);
											var TheTriggerCell = CellHelper.GetRandomCell();
											//projectile.Launch(Find.CurrentMap.mapPawns.AllPawns.RandomElement(), TheTriggerCell, CellHelper.GetRandomCell(), ProjectileHitFlags.NonTargetWorld, false, null);
											projectile.Launch(null, TheTriggerCell, CellHelper.GetRandomCell(), ProjectileHitFlags.NonTargetWorld, false, null);
										}
									})
								});

							}

							for (int i = 0; i < 20; i++)
							{
								Projectile projectile = (Projectile)GenSpawn.Spawn(ThingDef.Named("Bullet_Shell_HighExplosive_Ger"), CellFinder.RandomEdgeCell(Find.CurrentMap), Find.CurrentMap);
								var TheTriggerCell = CellHelper.GetRandomCell();
								//projectile.Launch(Find.CurrentMap.mapPawns.AllPawns.RandomElement(), TheTriggerCell, CellHelper.GetRandomCell(), ProjectileHitFlags.NonTargetWorld, false, null);
								projectile.Launch(null, TheTriggerCell, CellHelper.GetRandomCell(), ProjectileHitFlags.NonTargetWorld, false, null);
							}
						}//if
						else if (TheRandomBank == 2)
						{
							var TheBoolFlag = false;
							//灾难事件2
							BreakdownManager bdm = Find.CurrentMap.GetComponent<BreakdownManager>();
							if (bdm.brokenDownThings.Count == 0)
							{
								foreach (var TheThing in Find.CurrentMap.listerBuildings.allBuildingsColonist)
								{
									if (RG.Next(1, 1000) < 144)
									{
										var TheComingDownCom = TheThing.TryGetComp<CompBreakdownable>();
										if (TheComingDownCom != null)
										{
											//必须要成功暂停电气设备后，才能显示提示
											TheBoolFlag = true;
											TheComingDownCom.DoBreakdown();
											break;
										}
									}
								}
							}

							if (TheBoolFlag)
							{
								//第一个灾难
								Find.LetterStack.ReceiveLetter(
								"设备损坏",
								"你的一些用点设备正在被敌人特工大肆破坏！",
								LetterDefOf.ThreatBig
								);
							}
							else
							{
								goto TheReChangeBeiLuTe;
							}
						}
						else if (TheRandomBank == 3)
						{
							//建筑坍塌
							HotseatGameComponent.RandomBuildingDestory();
						}
						else if (TheRandomBank == 4)
						{
							//Find.CurrentMap.mapPawns.FreeColonists.ToList().ForEach(XX =>
							//{
							//	//如果小人躺在床上，则强制唤醒它
							//	if (XX.InBed()) RestUtility.WakeUp(XX);
							//	XX.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Tantrum"));
							//});

							//var RCount = Find.CurrentMap.mapPawns.FreeColonists.ToList().
							//	Count(XX => XX.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("Tantrum"));
							//if (RCount != 0)
							//{
							//	Find.LetterStack.ReceiveLetter(
							//		"大暴乱",
							//		"你的小人正在大肆破坏。",
							//		LetterDefOf.ThreatBig
							//		);
							//}
							//else
							//{
							//如果没有小人进入狂暴状态，则重新选择事件
							goto TheReChangeBeiLuTe;
							//}
						}
						else if (TheRandomBank == 5)
						{
							//打架斗殴

							var TheQiRen = Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement();
							var TheTarget = Find.CurrentMap.mapPawns.FreeColonists.Where(XX => XX != TheQiRen && !XX.Downed && !XX.Dead);

							//判断两个小人的距离
							if (TheTarget.FirstOrDefault() != null && IntVec3Utility.DistanceTo(TheQiRen.Position, TheTarget.FirstOrDefault().Position) < 100f
								&& !TheTarget.FirstOrDefault().InBed() && !TheQiRen.InBed())
							{
								//弹出带提示的信标
								//Find.LetterStack.ReceiveLetter(
								//"斗殴",
								//"你的殖民者正在打架",
								//LetterDefOf.NegativeEvent
								//);

								//开始打架
								TheQiRen.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, otherPawn: TheTarget.FirstOrDefault());
								TheTarget.FirstOrDefault()?.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, otherPawn: TheQiRen);

								//InteractionUtility.CanInitiateRandomInteraction();
								//RelationsUtility.PawnsKnowEachOther();

								//TheQiRen.story.

								//开始打架后有概率成为恋人
								//if (RG.Next(1, 1000) < 100)
								//{
								//	if (!TheTarget.FirstOrDefault().relations.DirectRelationExists(PawnRelationDefOf.Lover, TheQiRen)) TheTarget.FirstOrDefault().relations.AddDirectRelation(PawnRelationDefOf.Lover, TheQiRen);
								//	if (!TheQiRen.relations.DirectRelationExists(PawnRelationDefOf.Lover, TheTarget.FirstOrDefault())) TheQiRen.relations.AddDirectRelation(PawnRelationDefOf.Lover, TheTarget.FirstOrDefault());
								//}
							}
						}
						else if (TheRandomBank == 6)
						{
							Find.CurrentMap.mapPawns.FreeColonists.ToList().RandomElement().
								mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Tantrum"));

							var RCount = Find.CurrentMap.mapPawns.FreeColonists.ToList().
								Count(XX => XX.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("Tantrum"));
							if (RCount != 0)
							{
								Find.LetterStack.ReceiveLetter(
									"暴怒",
									"你的小人正在大肆破坏。",
									LetterDefOf.ThreatBig
									);
							}
							else
							{
								//如果没有小人进入狂暴状态，则重新选择事件
								goto TheReChangeBeiLuTe;
							}
						}
						else if (TheRandomBank == 7)
						{
							var Map = Find.RandomPlayerHomeMap;
							if (Map.mapPawns.PrisonersOfColony.Count != 0)
							{
								var TheRandomPawn = Map.mapPawns.PrisonersOfColony.RandomElement();
								TheRandomPawn.SetFaction(Faction.OfMechanoids);

								Find.LetterStack.ReceiveLetter(
									"囚犯叛变(贝鲁特)",
									"您的囚犯似乎开始叛变行为。",
									LetterDefOf.ThreatBig
									);
							}
							else
							{
								//如果该事件没有效果，则重新选择
								goto TheReChangeBeiLuTe;
							}
						}
						else if (TheRandomBank == 8)
						{
							var Map = Find.RandomPlayerHomeMap;
							if (Map.mapPawns.PrisonersOfColony.Count != 0)
							{
								var TheRandomPawn = Map.mapPawns.PrisonersOfColony.RandomElement();
								//TheRandomPawn.SetFaction(Faction.OfMechanoids);
								TheRandomPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, forceWake: true);
								Find.LetterStack.ReceiveLetter(
									"囚犯猎杀人类(贝鲁特)",
									"由于某种特殊宇宙射线影响，您的囚犯似乎开始疯狂捕猎同类。",
									LetterDefOf.ThreatBig
									);
							}
							else
							{
								//如果该事件没有效果，则重新选择
								goto TheReChangeBeiLuTe;
							}
						}
						else if (TheRandomBank == 9)
						{
							//直接重新选择
							//goto TheReChangeBeiLuTe;
							Find.LetterStack.ReceiveLetter(
									"敌人伏击(贝鲁特)",
									"由于某种特殊宇宙射线影响，您的囚犯似乎开始疯狂捕猎同类。",
									LetterDefOf.ThreatBig
									);

							var Map = Find.RandomPlayerHomeMap;
							List<Pawn> CreateEnemy(IntVec3 ThePos, Map TheMap, PawnKindDef pawnKindDef, Faction TheFac = null)
							{
								var TheSoilers = new List<Pawn>();
								//IntVec3 intVec = CellFinder.RandomClosewalkCellNear(ThePos, Find.CurrentMap, 10, null); Thrumbo PawnGroupKindDefOf.Settlement
								Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, TheFac ?? Find.FactionManager.OfMechanoids);

								//多少游戏周期后离开地图
								//pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(10000, 12000);
								//pawn.SetFaction(null, null);
								QuestUtility.AddQuestTag(GenSpawn.Spawn(pawn, ThePos, TheMap, 0), "");
								TheSoilers.Add(pawn);

								LordJob_DefendPoint lordJob = new LordJob_DefendPoint(ThePos, 5f);
								var TL1123 = LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, TheMap, TheSoilers);

								return TheSoilers;
							}

							foreach (var item in Find.CurrentMap.mapPawns.FreeColonists)
							{
								IntVec3 intVec = CellFinder.RandomClosewalkCellNear(item.Position, Map, 5, XX4 => XX4.Walkable(Map));
								if (intVec != null)
								{
									CreateEnemy(intVec, Map, PawnKindDefOf.AncientSoldier, Faction.OfMechanoids);
								}
							}
						}
					}
					else if (IndintName == "DelayTaskAndCropBlight" && TheArgrments != null)
					{
						var TheIndintName = TheArgrments[1].Trim();
						//Log.Message($"TheIndintName={ TheIndintName }");
						var IsCheck = TheArgrments[3].Trim();
						var TimerStr = TheArgrments[4].Trim().GetStrTime().GetRandomTime();  //出错返回(-100,-100)
						var TaskRand = TheArgrments[5].Trim();
						var TaskComment = TheArgrments[6].Trim().Replace("(Data)", TimerStr.ToString());
						//DelayTaskAndCropBlight

						var DelayTaskFlag = TaskRand + "_CropBlight";
						//同时检查是否所有的既定任务都完成
						//if (TheTimerManager._TimerInfos.ContainsKey(TaskRand)) goto Igrone;
						//if (TheTimerManager._TimerInfos.ContainsKey(DelayTaskFlag)) goto Igrone;

						//RestartGoto

						//如果计时器在，就进行下一轮随机
						if (TheTimerManager._TimerInfos.ContainsKey(TaskRand)) goto RestartGoto;
						if (TheTimerManager._TimerInfos.ContainsKey(DelayTaskFlag)) goto RestartGoto;

						Find.LetterStack.ReceiveLetter($"即将发生的事件({"破坏农作物的发狂动物".Colorize(UnityEngine.Color.yellow)})"
								, TaskComment.Colorize(UnityEngine.Color.red),
								LetterDefOf.NegativeEvent);

						TheTimerManager.AddTimer(TaskRand, new TimerInfo
						{
							TTL = TimerStr,
							Data = (TheIncidentDef: null, IsCheck: (IsCheck.Trim() == "Check"), Target: target),
							_OnTimerDown_WithArgrments = new System.Action<(IncidentDef TheIncidentDef, bool IsCheck, IIncidentTarget Target)>(TheData =>
							{
								var RI = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName.Contains("ManhunterPack")).FirstOrDefault();
								if (RI != null && Find.CurrentMap != null)
								{
									IncidentParms parms = GenerateParms(RI.category, target);
									var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
									var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

									parms.target = Find.CurrentMap;
									if (RI.Worker.CanFireNow(parms)) RI.Worker.TryExecute(parms);
								}
							}),
						});

						TheTimerManager.AddTimer(DelayTaskFlag, new TimerInfo
						{
							TTL = TimerStr + RG.Next(1, 10), //在之前动物袭击的基础上再加上1-10小时延迟引发农作物枯萎
							Data = (TheIncidentDef: null, IsCheck: (IsCheck.Trim() == "Check"), Target: target),
							_OnTimerDown_WithArgrments = new System.Action<(IncidentDef TheIncidentDef, bool IsCheck, IIncidentTarget Target)>(TheData =>
							{
								var RI = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName.Contains("CropBlight")).FirstOrDefault();
								if (RI != null)
								{
									IncidentParms parms = GenerateParms(RI.category, target);
									var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
									var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

									//枯萎事件必定发生在玩家基地
									parms.target = Find.RandomPlayerHomeMap;
									if (RI.Worker.CanFireNow(parms)) RI.Worker.TryExecute(parms);
								}
							}),
						});

					Igrone:;
					}
					else if (IndintName == "DelayTask" && TheArgrments != null && DebugFlag) //延时叙事者事件
					{
						//Log.Message("Okay");
						//DebugFlag = false;
						//async void RedoFlag() 
						//{
						//	//延时一段时间后，再继续执行
						//	await Task.Delay(1000*RG.Next(1,300));
						//	DebugFlag = true;
						//}
						//RedoFlag();
						//得到事件DEF名称
						var TheIndintName = TheArgrments[1].Trim();
						//Log.Message($"TheIndintName={ TheIndintName }");
						var IsCheck = TheArgrments[3].Trim();
						var TimerStr = TheArgrments[4].Trim().GetStrTime().GetRandomTime();  //出错返回(-100,-100)
						var TaskRand = TheArgrments[5].Trim();
						var TaskComment = TheArgrments[6].Trim().Replace("(Data)", TimerStr.ToString());

						//档事件为随机事件时候
						if (TheIndintName == "RandomIncident")
						{
							Find.LetterStack.ReceiveLetter($"即将发生的事件({"随机事件".Colorize(UnityEngine.Color.yellow)})"
								, TaskComment.Colorize(UnityEngine.Color.red),
								LetterDefOf.NegativeEvent);

							var RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();
							//叙事者延时任务
							var AddRR = TheTimerManager.AddTimer(TaskRand + "_" + RI.defName, new TimerInfo
							{
								TTL = TimerStr,
								_TheData = RI,
								_OnTimerDown_with_OB = new System.Action<object>(TheData =>
								{
								//这里执行随机函数执行内容
								ReChoice1:;

									if (TheData != null && TheData is IncidentDef TheID)
									{
										IncidentParms parms = GenerateParms(TheID.category, target);
										var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
										var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

										parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
										//50%概率叙事者时间会切换到当前地图
										if (Find.CurrentMap != null && RG.Next(1, 1000) < 500) parms.target = Find.CurrentMap;
										if (TheID.Worker.CanFireNow(parms)) TheID.Worker.TryExecute(parms);
										else
										{
											//如果没有满足事件条件，则重新选择一个触发
											TheData = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();
											goto ReChoice1;
										}
									}
								}),
							});

							//如果此时随机任务已经在定时器中，则返回重新选择一个
							if (!AddRR) goto RestartGoto;

							goto GotoEnd;
						}

						//如果是随机任务
						if (TheIndintName == "GiveRandomQuestNoLimit")
						{
							var RR231 = QuestHelper.AddRamdomQuest(HotseatGameComponent.RG.Next(1, ((GenDate.DaysPassed * 25) <= 0 ? 2 : (GenDate.DaysPassed * 25))));
							//Find.LetterStack.ReceiveLetter($"随机任务给与 {(RR231 ? "成功" : "失败")}"
							//	, "测试",
							//	LetterDefOf.NeutralEvent);
							goto GotoEnd;
						}

						//如果是随机任务
						if (TheIndintName == "GiveRandomQuest" && Find.QuestManager.questsInDisplayOrder.Count(XX => XX.State == QuestState.Ongoing) < 8)
						{
							var QuestCount = 1;

							var DoQuestCount = 0;
							for (int i = 0; i < QuestCount; i++)
							{
								var RR231 = QuestHelper.AddRamdomQuest(HotseatGameComponent.RG.Next(1, ((GenDate.DaysPassed * 25) <= 0 ? 2 : (GenDate.DaysPassed * 25))));
								if (RR231) DoQuestCount++;
							}

							//Log.Message($"GiveRandomQuest!  { Find.QuestManager.questsInDisplayOrder.Count(XX=>XX.State == QuestState.Ongoing) }");
							goto GotoEnd;
						}

						//如果是随机任务
						if (TheIndintName == "GiveRandomQuestNew" && Find.QuestManager.questsInDisplayOrder.Count(XX => XX.State == QuestState.Ongoing) < 15)
						{
							var RR231 = QuestHelper.AddRamdomQuest(HotseatGameComponent.RG.Next(1, ((GenDate.DaysPassed * 10) <= 0 ? 2 : (GenDate.DaysPassed * 10))));
							goto GotoEnd;
						}

						//在数据中寻找指定的Incident名称
						var TheIncdef = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName == TheIndintName);
						//Log.Message(TheIncdef.Count().ToString());
						//在游戏数据库中寻找目标事件
						if (TheIncdef.Count() != 0)
						{
							//判断是何种类型的事件
							string EventName = "事件";
							if (TheIndintName == "RaidEnemy") EventName = "敌人袭击";
							else if (TheIndintName == "ManhunterPack") EventName = "猎杀人类";
							else if (TheIndintName == "RaidFriendly") EventName = "友军";
							else if (TheIndintName == "StrangerInBlackJoin") EventName = "黑衣人";
							else if (TheIndintName == "ResourcePodCrash") EventName = "物资货仓";
							else if (TheIndintName == "TraderCaravanArrival") EventName = "商队交易";
							else if (TheIndintName == "ShipChunkDrop") EventName = "宇宙飞船残骸";
							else if (TheIndintName == "WildManWandersIn") EventName = "流浪者";

							//确保玩家拥有一个能够派遣军队的盟友派系
							bool MakeSurePlayerHasFriendly()
							{
								var TheFaction = Find.FactionManager.RandomNonHostileFaction();
								//如果没有中立派系，则选取皇权
								if (TheFaction == null)
								{
									TheFaction = Find.FactionManager.RandomRoyalFaction();
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
								}

								return Find.FactionManager.AllFactions.Count(XX1 => XX1 != Find.FactionManager.OfPlayer && XX1.RelationKindWith(Find.FactionManager.OfPlayer) == FactionRelationKind.Ally) != 0;
							}

							//确保玩家有盟友
							if (TheIndintName == "RaidFriendly")
							{
								var RR24 = MakeSurePlayerHasFriendly();
								//如果没有友军派系，则重新选择一个新事件
								if (!RR24) goto RestartGoto;
							}


							if (TheTimerManager._TimerInfos.ContainsKey(TaskRand)) goto Igrone;
							Find.LetterStack.ReceiveLetter($"即将发生的事件({EventName.Colorize(TheIndintName == "RaidFriendly" ? UnityEngine.Color.green : UnityEngine.Color.red)})"
								, TaskComment.Colorize(UnityEngine.Color.red),
								LetterDefOf.NegativeEvent);
							//叙事者延时任务
							TheTimerManager.AddTimer(TaskRand, new TimerInfo
							{
								TTL = TimerStr,
								Data = (TheIncidentDef: TheIncdef.FirstOrDefault(), IsCheck: (IsCheck.Trim() == "Check"), Target: target),
								_OnTimerDown_WithArgrments = new System.Action<(IncidentDef TheIncidentDef, bool IsCheck, IIncidentTarget Target)>(TheData =>
								{
									if (TheData.TheIncidentDef != null)
									{
										IncidentParms parms = GenerateParms(TheData.TheIncidentDef.category, TheData.Target);
										var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
										var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

										parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
										TheData.TheIncidentDef.Worker.TryExecute(parms);
									}
								}),
							});
						Igrone:; //如果计时器中已经包含相容的任务，则不处理
						}
					}
					else if (IndintName == "MeteoriteKiller") 
					{
						var ThePos = Find.CurrentMap.GetPawnPosRandomly(out var _TriggerPawn);
						if (ThePos != IntVec3.Invalid)
						{
							List<Thing> list = ThingSetMakerDefOf.Meteorite.root.Generate();
							SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, ThePos, Find.CurrentMap);
                            Find.LetterStack.ReceiveLetter(
                                $"杀人流星(目标:{_TriggerPawn.Name})",
                                "流星坠落在了您的小人身旁。",
                                LetterDefOf.ThreatBig
                                );
                        }
                    }
                    else if (IndintName == "OnlineStorytellerCommand")
                    {
                        //请求服务器返回一个即将触发随机事件
                        WebSocketHelper.DoWebSocketSend($"#AIStorytellerCommand#AIGetRandomStorytellerItem#{SteamUtility.SteamPersonaName}");
                    }
                    else if (IndintName == "Debug")
                    {
                        //请求服务器返回一个即将触发随机事件
                        //WebSocketHelper.DoWebSocketSend($"#AIStorytellerCommand#AIGetRandomStorytellerItem#{SteamUtility.SteamPersonaName}");
                        Find.LetterStack.ReceiveLetter(
                        $"测试",
                        "流星坠落在了您的小人身旁。",
                        LetterDefOf.ThreatBig
                        );
                    }
                    //
                }
				//Log.Message();
			}
			//Log.Message(SB.ToString());

		GotoEnd:;
			yield break;
		}

		public override string ToString()
		{
			return base.ToString() + "1123";
		}
	}
}
