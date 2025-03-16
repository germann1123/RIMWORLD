using RimWorld;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using MeteorIncident;

namespace MeteorIncidentGer
{
	public class StorytellerCompProperties_GerManHunterPacktTimer : StorytellerCompProperties
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
		public bool IsAttackInPlayer;
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

		public StorytellerCompProperties_GerManHunterPacktTimer()
		{
			compClass = typeof(StorytellerComp_GerManHunterPacktTimer);
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

	
	public class StorytellerComp_GerManHunterPacktTimer : StorytellerComp
	{
		protected StorytellerCompProperties_GerManHunterPacktTimer Props => (StorytellerCompProperties_GerManHunterPacktTimer)props;
		public static System.Random RG { get; set; } = new System.Random(Guid.NewGuid().GetHashCode());
		/// <summary>
		/// 配置文件数据存贮
		/// </summary>
		public static ConcurrentDictionary<string, List<string>> _Cfg = new ConcurrentDictionary<string, List<string>>();

		[DebuggerHidden]
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
				//通过概率决定事件是否触发
				if ((int)Props.TriggerChance > RG.Next(1, 1000))
				{
					//解析STR参数
					var DataSplit = Props.IncidentStr.Trim().Replace("\r\n", "").Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
					//Log.Message(Props.IncidentStr);
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
							_Cfg[Props.DebugCommentStr].Add(XX1);
						}
					});
					//Log.Message(SB.ToString());

					bool RR = false;
					string TheResult = "";
					//根据权重随机选择元素
					if (_Cfg.ContainsKey(Props.DebugCommentStr))
					{
						RR = _Cfg[Props.DebugCommentStr].TryRandomElementByWeight(XX1 =>
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
						}, out var TheResult1);
						TheResult = TheResult1;
					}
					else
					{
						break;
					}

					//Log.Message(TheResult.Trim().Replace("\r\n", ""));
					//当第一个参数给NOTHING的时候，忽略本次命中
					if (RR)
					{
						var IndintName = TheResult.Split(new string[] { "item" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().Replace("\r\n", "");
						if (IndintName != "Nothing")
						{
							var allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading.Where(XX => XX.defName == IndintName);
							if (allDefsListForReading.Count() != 0)  //官方事件
							{
								IncidentParms parms = GenerateParms(allDefsListForReading.FirstOrDefault().category, target);

								var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
								var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

								parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
								allDefsListForReading.FirstOrDefault().Worker.TryExecute(parms);
							}
							else if (IndintName == "JiangGuoZhang")  //浆果生长事件
							{
								var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
								var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

								(TriggerMap.FirstOrDefault() ?? Find.CurrentMap).JiangGuoZhang();
							}
							else if (IndintName == "RandomIncident") //随机叙事者事件
							{
								var RI = DefDatabase<IncidentDef>.AllDefsListForReading.RandomElement();
								if (RI != null)
								{
									IncidentParms parms = GenerateParms(RI.category, target);
									var TheMaxCCount = Find.Maps.Where(XX => XX.IsPlayerHome).Max(XX1 => XX1.mapPawns.FreeColonistsAndPrisonersCount);
									var TriggerMap = Find.Maps.Where(XX => XX.IsPlayerHome && XX.mapPawns.FreeColonistsAndPrisonersCount >= TheMaxCCount);

									parms.target = TriggerMap.FirstOrDefault() ?? Find.CurrentMap;
									if (RI.Worker.CanFireNow(parms)) RI.Worker.TryExecute(parms);
								}
							}
						}
						//Log.Message();
					}
					//Log.Message(SB.ToString());
				}
			}//for

			//if (new List<int> { 121, 223, 444, 555, 1231 }.TryRandomElementByWeight(AA => AA, out var Result)) 
			//{
			//	Log.Message($"{Props.DebugCommentStr}=>{ Result }");
			//}

			//Settings.DebugString($"Accept Fraction: {num} + Incident Count: {incCount}");
			//for (int i = 0; i < incCount; i++)
			//{
			//	FiringIncident firingIncident = GenerateIncident(target);
			//	if (firingIncident != null)
			//	{
			//		//Settings.DebugString($"Make Incident: {firingIncident.def.defName}");
			//		yield return firingIncident;
			//	}
			//}
			//Settings.DebugString("== Exit StorytellerComp_OmenOnOffCycle.MakeIntervalIncidents ==");

			yield break;
		}

		public override string ToString()
		{
			return base.ToString() + " (" + ((Props.incident != null) ? Props.incident.defName : Props.IncidentCategory.defName) + ")";
		}
	}
}
