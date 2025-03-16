using RimWorld;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MeteorIncidentGer
{
	public class StorytellerCompProperties_GerCondiction : StorytellerCompProperties
	{
		public float onDays;
		public float offDays;
		public float minSpacingDays;
		public FloatRange numIncidentsRange = FloatRange.Zero;
		public SimpleCurve acceptFractionByDaysPassedCurve;
		public SimpleCurve acceptPercentFactorPerThreatPointsCurve;
		public IncidentDef incident;
		private IncidentCategoryDef category;
		public int cosmicHorrorRaidPercentage;
		public float forceCosmicHorrorRaidBeforeDaysPassed;
		public float forceRaidEnemyBeforeDaysPassed;
		public string TriggerType;
		public float TriggerChance;
		public string IncidentStr;
		public string DebugCommentStr;

		public IncidentCategoryDef IncidentCategory
		{
			get
			{
				if (incident != null)
				{
					return incident.category;
				}
				return category;
			}
		}

		public StorytellerCompProperties_GerCondiction()
		{
			compClass = typeof(StorytellerComp_GerTriggerCaven);
		}

		public override IEnumerable<string> ConfigErrors(StorytellerDef parentDef)
		{
			if (incident != null && category != null)
			{
				yield return "incident and category should not both be defined";
			}
			if (onDays <= 0f)
			{
				yield return "onDays must be above zero";
			}
			if (numIncidentsRange.TrueMax <= 0f)
			{
				yield return "numIncidentRange not configured";
			}
			if (minSpacingDays * numIncidentsRange.TrueMax > onDays * 0.9f)
			{
				yield return "minSpacingDays too high compared to max number of incidents.";
			}
		}
	}

	/// <summary>
	/// 玩家开始远行队触发器
	/// </summary>
	public class StorytellerComp_GerTriggerCaven : StorytellerComp
	{
		protected StorytellerCompProperties_GerCondiction Props => (StorytellerCompProperties_GerCondiction)props;
		public static System.Random RG { get; set; } = new System.Random(Guid.NewGuid().GetHashCode());
		/// <summary>
		/// 配置文件数据存贮
		/// </summary>
		public static ConcurrentDictionary<string, List<string>> _Cfg = new ConcurrentDictionary<string, List<string>>();

		//public override void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = null)
		//{
		//	if (!p.RaceProps.Humanlike || !p.IsColonist)
		//	{
		//		return;
		//	}
		//	if (ev == AdaptationEvent.Died || ev == AdaptationEvent.Kidnapped || ev == AdaptationEvent.LostBecauseMapClosed || ev == AdaptationEvent.Downed)
		//	{
		//		foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
		//		{
		//			if (current.RaceProps.Humanlike && !current.Downed)
		//			{
		//				return;
		//			}
		//		}
		//		Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
		//		if (anyPlayerHomeMap != null)
		//		{
		//			IncidentParms parms = StorytellerUtility.DefaultParmsNow(this.Props.incident.category, anyPlayerHomeMap);
		//			if (this.Props.incident.Worker.CanFireNow(parms))
		//			{
		//				QueuedIncident qi = new QueuedIncident(new FiringIncident(this.Props.incident, this, parms), Find.TickManager.TicksGame + this.Props.delayTicks, 0);
		//				Find.Storyteller.incidentQueue.Add(qi);
		//			}
		//		}
		//	}
		//}

		/// <summary>
		/// 当前地图里是否存在敌人
		/// </summary>
		public static bool _IsEnemyInMap = false;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			//Settings.DebugString("== Enter StorytellerComp_OmenOnOffCycle.MakeIntervalIncidents ==");
			float num = 1f;
			if (Props.acceptFractionByDaysPassedCurve != null)
			{
				num *= Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
			}
			if (Props.acceptPercentFactorPerThreatPointsCurve != null)
			{
				num *= Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(target));
			}

			//Log.Message($"IncidentCountThisInterval Start!==>{ Props.IsAttackInPlayer }");
			int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), Props.minDaysPassed, Props.onDays, Props.offDays, Props.minSpacingDays, Props.numIncidentsRange.min, Props.numIncidentsRange.max, num);
			//Log.Message($"IncidentCountThisInterval End!==>{ Props.IsAttackInPlayer } incCount={incCount }");
			if (incCount == 0) yield break;

			//触发事件
			for (int loopa = 0; loopa < incCount; loopa++)
			{
				//当时间合适，检查当前地图的敌人状态
				if (Props.TriggerType == "WhenRaidArrive" && _IsEnemyInMap) 
				{
					//通过概率决定事件是否触发
					if ((int)Props.TriggerChance > RG.Next(1, 1000))
					{
						//解析STR参数
						var DataSplit = Props.IncidentStr.Trim().Replace("\r\n", "").Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
						//输出调试信息
						StringBuilder SB = new StringBuilder();
						SB.Append(Props.DebugCommentStr + " ");
						DataSplit.ToList().ForEach(XX1 =>
						{
							if (!_Cfg.ContainsKey(Props.DebugCommentStr))
							{
								_Cfg.TryAdd(Props.DebugCommentStr, new List<string> { XX1 });
							}
							else
							{
								_Cfg[Props.DebugCommentStr].Add(XX1);
							}
						});

						//根据权重随机选择元素
						var RR = _Cfg[Props.DebugCommentStr].TryRandomElementByWeight(XX1 =>
						{
							var SplitStr = XX1.Split(new string[] { "item" }, StringSplitOptions.RemoveEmptyEntries);
							if (SplitStr.Length >= 2 && int.TryParse(SplitStr[1], out var TheValue))
							{
								return TheValue;
							}
							else
							{
								return 0;
							}
						}, out var TheResult);

						//当第一个参数给NOTHING的时候，忽略本次命中
						if (RR && !TheResult.Split(new string[] { "item" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().Replace("\r\n", "").Contains("Nothing"))
						{
							var IndintName = TheResult.Split(new string[] { "item" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().Replace("\r\n", "");
							var allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName == IndintName);
							if (allDefsListForReading.Count() != 0)
							{
								IncidentParms parms = GenerateParms(allDefsListForReading.FirstOrDefault().category, target);
								parms.target = Find.RandomPlayerHomeMap;

								//检查配置文件是否需要检查条件
								var RRR = TheResult.Split(new string[] { "item" }, StringSplitOptions.RemoveEmptyEntries);
								if (RRR.Length <= 2)
									allDefsListForReading.FirstOrDefault().Worker.TryExecute(parms);
								else if (RRR.Length == 3 && RRR[2] == "Check" && allDefsListForReading.FirstOrDefault().Worker.CanFireNow(parms)) 
								{
									allDefsListForReading.FirstOrDefault().Worker.TryExecute(parms);
								}
							}
							//Log.Message();
						}
						//Log.Message(SB.ToString());
					}
				}
			}
			yield break;
		}

		public override string ToString()
		{
			return base.ToString() + " (" + ((Props.incident != null) ? Props.incident.defName : Props.IncidentCategory.defName) + ")";
		}
	}
}
