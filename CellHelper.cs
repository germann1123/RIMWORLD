using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using HarmonyLib;
using MeteorIncident;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;
using Verse.Sound;

namespace MeteorIncidentGer
{
	public static class CellHelper
	{
		/// <summary>
		/// 返回一个随机地图坐标
		/// </summary>
		public static IntVec3 GetRandomCell(int DistanceToEdge=20)
		{
			return CellFinderLoose.RandomCellWith((IntVec3 sq) => GenGrid.Standable(sq, Find.CurrentMap) && !Find.CurrentMap.roofGrid.Roofed(sq) && !Find.CurrentMap.fogGrid.IsFogged(sq) && !GenGrid.CloseToEdge(sq, Find.CurrentMap, DistanceToEdge), Find.CurrentMap, 1000);
		}

		public static IntVec3 GetRandomCell(this Map TheMap)
		{
			return CellFinderLoose.RandomCellWith((IntVec3 sq) => GenGrid.Standable(sq, TheMap), TheMap, 1000);
		}


		/// <summary>
		/// 选择一个地图里随机殖民者投放陨石 - 老陆专用
		/// </summary>
        public static IntVec3 GetPawnPosRandomly(this Map TheMap,out Pawn _TheResult)
        {
			_TheResult = null;
            if (TheMap == null) return IntVec3.Invalid;
			if(Find.CurrentMap.mapPawns.FreeColonists.Count == 0) return IntVec3.Invalid;

			//获取一个随机小人信息
            var RR = Find.CurrentMap.mapPawns.FreeColonists.RandomElement();

            IntVec3 intVec1 = CellFinder.RandomClosewalkCellNear(RR.Position, Find.CurrentMap, 4, XX4 => XX4.Walkable(Find.CurrentMap));
			_TheResult = RR;
            return intVec1;
        }

        /// <summary>
        /// 指定角色开始偷东西
        /// </summary>
        public static void StartSteal(this Pawn TheP) 
		{
			var TheJob = TryGiveJob(TheP);
			if (TheJob != null) 
			{
				TheP.jobs.EndCurrentJob(JobCondition.Succeeded, false, true);
				TheP.jobs.StartJob(TheJob, 0, null, false, true, null, null, false, false);
				TheP.jobs.ClearQueuedJobs(true);
			}
		}

		public static Job TryGiveJob(Pawn pawn)
		{
			IntVec3 c;
			if (!RCellFinder.TryFindBestExitSpot(pawn, out c, TraverseMode.ByPawn))
			{
				return null;
			}
			Thing thing;
			if (StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 12f, out thing, pawn, null) && !GenAI.InDangerousCombat(pawn))
			{
				Job expr_3B = JobMaker.MakeJob(JobDefOf.Steal);
				expr_3B.targetA = thing;
				expr_3B.targetB = c;
				expr_3B.count = Mathf.Min(thing.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / thing.def.VolumePerUnit));
				return expr_3B;
			}
			return null;
		}

		public static void RandomBuildingBoom(this Map TheMap,bool HasTopic=true)
		{
			var building = TheMap.listerBuildings.allBuildingsColonist.RandomElement();
			if (building != null)
			{
				if (HasTopic)
				{
					Find.LetterStack.ReceiveLetter(
					"建筑爆炸",
					"一些建筑物由于某种原因发生了剧烈爆炸，爆炸造成的损失难以估计",
					LetterDefOf.ThreatSmall
					);
				}

				//进行随机次数轰炸
				for (int i = 0; i < Rand.Range(1, 6); i++)
				{
					TheTimerManager.AddTimerSmall(("TheBoom_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfoSmall
					{
						TTL = i * 2,
						_OnTimerDown = new System.Action(() =>
						{
							GenExplosion.DoExplosion(building.Position, TheMap, Rand.Range(1f, 3f), DamageDefOf.Flame, null, 20, -1f, null, null, null, null, null, 0f, 1, null,false, null, 0f, 1, 0f, false, null, null);
						})
					});
				}
			}
		}
	}

	public static class PawnHelper
	{

		public static bool HasTrait(this Pawn pawn, TraitDef traitDef)
		{
			if (traitDef != null && (pawn?.story?.traits?.HasTrait(traitDef) ?? false))
			{
				return true;
			}
			return false;
		}

		public static (bool RR,Trait Qian,Trait Hou) TryRemoveTrait(this Pawn Pawn) 
		{
			var RemoveTrait = Pawn?.story?.traits?.allTraits.RandomElement();
			Pawn?.story?.traits?.RemoveTrait(RemoveTrait);

			int TryCount = 0;
			while (true) 
			{
				var TheChoiceTrait = DefDatabase<TraitDef>.AllDefsListForReading.RandomElement();
				if (Pawn?.story?.traits?.allTraits.Count(XX => XX.def == TheChoiceTrait) == 0)
				{
					Pawn?.story?.traits?.GainTrait(new Trait(TheChoiceTrait));
					return (RR: true, Qian: RemoveTrait, Hou: new Trait(TheChoiceTrait));
				}
				else 
				{
					if (TryCount++ >= 1000) return (RR:false,Qian:null,Hou:null);
				}
			}
			
		}

		public static (bool RR, Trait Qian, Trait Hou) TryGetTieRenTrait(this Pawn Pawn,string TraitStr= "ToughBig")
		{
			var RemoveTrait = Pawn?.story?.traits?.allTraits.RandomElement();
			Pawn?.story?.traits?.RemoveTrait(RemoveTrait);

			int TryCount = 0;
			while (true)
			{
				var TheChoiceTrait = TraitDef.Named(TraitStr);
				if (Pawn?.story?.traits?.allTraits.Count(XX => XX.def == TheChoiceTrait) == 0)
				{
					Pawn?.story?.traits?.GainTrait(new Trait(TheChoiceTrait));
					return (RR: true, Qian: RemoveTrait, Hou: new Trait(TheChoiceTrait));
				}
				else
				{
					if (TryCount++ >= 1000) return (RR: false, Qian: null, Hou: null);
				}
			}

		}

		public static Pawn GetSpouseOrLoverOrFiance(this Pawn pawn)
		{
			bool flag = !pawn.RaceProps.IsFlesh;
			Pawn result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				bool flag2 = firstDirectRelationPawn == null;
				if (flag2)
				{
					Pawn firstDirectRelationPawn2 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
					bool flag3 = firstDirectRelationPawn2 == null;
					if (flag3)
					{
						Pawn firstDirectRelationPawn3 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
						result = firstDirectRelationPawn3;
					}
					else
					{
						result = firstDirectRelationPawn2;
					}
				}
				else
				{
					result = firstDirectRelationPawn;
				}
			}
			return result;
		}
	}

	//public class DeathActionWorker_Ger : DeathActionWorker
	//{
	//	public override void PawnDied(Corpse corpse)
	//	{
	//		if (corpse.Faction == Faction.OfPlayer)
	//		{
	//			Find.LetterStack.ReceiveLetter(
	//				"死亡献祭",
	//				"在殖民地中有玩家阵营的生物死亡",
	//				LetterDefOf.NegativeEvent
	//				);
	//		}

	//		//for (int i = 0; i < 3; i++)
	//		//	FleckMaker.ThrowAirPuffUp(corpse.PositionHeld.ToVector3(), corpse.Map);
	//		//if (!corpse.Destroyed) corpse.Destroy(DestroyMode.Vanish);
	//	}
	//}

	public static class PawnHelperGer
	{
		public static int GetSocialSkill(this Pawn p) => p.skills.GetSkill(SkillDefOf.Social).Level;
		public static int GetResearchSkill(this Pawn p) => p.skills.GetSkill(SkillDefOf.Intellectual).Level;
	}

	public static class StrHelper 
	{
		/// <summary>
		/// 解析时间字串
		/// </summary>
		public static (int TimeDelayMin, int TimeDelayMax) GetStrTime(this string Data) 
		{
			var DataLine = Data.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
			if (DataLine.Length != 2) return (-100,-100);
			if (int.TryParse(DataLine[0], out var MinDelay) && int.TryParse(DataLine[1], out var MaxDelay)) 
			{
				return (TimeDelayMin: Math.Abs(MinDelay), TimeDelayMax: Math.Abs(MaxDelay));	 //如果时间错误给成负数，则自动更正
			}
			return (-100, -100);
		}

		/// <summary>
		/// 解析渐进叙事者参数
		/// </summary>
		public static (bool IsEnable,int DayPassed, int ChuXi) GetJianJinSuanStr(this string Data) 
		{
            var DataLine = Data.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
			if (DataLine.Length >= 3 && int.TryParse(DataLine[1],out var DayPassed) && int.TryParse(DataLine[2], out var ChuXi)) 
			{
				return (DataLine[0].ToUpper() == "TRUE", DayPassed, ChuXi);
			}

            return (false, 1, 1);
        }


        public static int GetRandomTime(this (int TimeDelayMin, int TimeDelayMax) Data) => Rand.Range(Data.TimeDelayMin,Data.TimeDelayMax);
		public static int GetNoErrorInt(this int Data) 
		{
			if (Data < 0) return 1;
			return Data;
		}

		/// 压缩字节数组
		/// &lt;/summary&gt;
		/// &lt;param name=&quot;str&quot;&gt;&lt;/param&gt;
		public static byte[] Compress(byte[] inputBytes)
		{
			using (MemoryStream outStream = new MemoryStream())
			{
				using (GZipStream zipStream = new GZipStream(outStream, CompressionMode.Compress, true))
				{
					zipStream.Write(inputBytes, 0, inputBytes.Length);
					zipStream.Close(); //很重要，必须关闭，否则无法正确解压
					return outStream.ToArray();
				}
			}
		}

		/// &lt;summary&gt;
		/// 解压缩字节数组
		/// &lt;/summary&gt;
		/// &lt;param name=&quot;str&quot;&gt;&lt;/param&gt;
		public static byte[] Decompress(byte[] inputBytes)
		{

			using (MemoryStream inputStream = new MemoryStream(inputBytes))
			{
				using (MemoryStream outStream = new MemoryStream())
				{
					using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
					{
						zipStream.CopyTo(outStream);
						zipStream.Close();
						return outStream.ToArray();
					}
				}

			}
		}
		public static byte[] GetZipBytes(this string Data) =>Compress(System.Text.Encoding.Default.GetBytes(Data));
		public static string GetStrFrmZipData(this byte[] Data) => System.Text.Encoding.Default.GetString(Decompress(Data));
	}

	public class FactionHelper 
	{
		public void GetTheFaction(ref Faction faction) 
		{
			Faction TheNewFaction = new Faction();
			TheNewFaction.Name = "黑暗帝国";
			TheNewFaction.color = Color.black;
			TheNewFaction.temporary = true;
			TheNewFaction.questTags = new List<string> { "Tmp" };
			TheNewFaction.RemoveAllRelations();


			Find.World.factionManager.Add(TheNewFaction);

			MapGenFloatGrid elevation = MapGenerator.Elevation;
			foreach (IntVec3 current in Find.CurrentMap.AllCells)
			{
				elevation[current] = 0.9f;
			}

			//faction = Faction.OfPlayer;
			//GenCollection.TryRandomElement<Faction>(from x in Find.FactionManager.AllFactionsVisible
			//where !x.IsPlayer && !x.def.hidden && !x.defeated && x.def.humanlikeFaction && FactionUtility.HostileTo(x, Faction.OfPlayer) && FactionUtility.HostileTo(x, Faction.OfPlayer) && !x.temporary
			//select x,ref faction);
		}
	}

	//[HarmonyPatch(typeof(GenText), nameof(GenText.RandomSeedString))]
	//static class GrammarRandomStringPatch
	//{
	//	static void Postfix(ref string __result)
	//	{
	//		__result = "Ger";
	//	}
	//}
}
