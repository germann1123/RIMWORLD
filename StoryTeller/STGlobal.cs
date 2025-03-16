using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncidentGer
{
    public class STGlobal
    {
        /// <summary>
        /// 指示叙事者触发动物发狂事件标志
        /// </summary>
        public static int AnimalKuangCount = 0;
		public static List<IntVec3> tmpCells { get; set; } = new List<IntVec3>();

		public static int APang_BatteryCount = 0;
		public static int APang_ZongDianLiang = 0;
		public static float APang_ZongDianLiang_Give = 0;

		public static float TotalDianGei = 0.0f;
		public static int GiveCount = 0;

		/// <summary>
		/// 友方玩家电网距离满的需求量
		/// </summary>
		public static int PengYouDianWangXu = 0;
	}

    public static class MapManHunterPackHelper 
    {
  //      public static bool DoRandomManHunterPack(this Map TheMap,float Point,int AnimalCount) 
  //      {
		//	try
  //          {
  //              if (TheMap == null) return false;
  //              Map map = TheMap;
  //              PawnKindDef pawnKind = null;
  //              if ((pawnKind == null && !GetManhunterPackIncidentUtility().TryFindManhunterAnimalKind(Point, map.Tile, out pawnKind)) || ManhunterPackIncidentUtility.GetAnimalsCount(pawnKind, Point) == 0)
  //              {
  //                  return false;
  //              }
  //              IntVec3 spawnCenter = TheMap.GetRandomCell();
  //              if (!spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out spawnCenter, map, CellFinder.EdgeRoadChance_Animal, false, null))
  //              {
  //                  return false;
  //              }
  //              List<Pawn> list = ManhunterPackIncidentUtility.GenerateAnimals(pawnKind, map.Tile, Point * 1f, AnimalCount);
  //              Rot4 rot = Rot4.FromAngleFlat((map.Center - spawnCenter).AngleFlat);
  //              for (int i = 0; i < list.Count; i++)
  //              {
  //                  Pawn arg_B5_0 = list[i];
  //                  //IntVec3 loc = CellFinder.RandomClosewalkCellNear(spawnCenter, map, 10, null);
  //                  IntVec3 loc = TheMap.GetRandomCell();
  //                  QuestUtility.AddQuestTag(GenSpawn.Spawn(arg_B5_0, loc, map, rot, WipeMode.Vanish, false), "");
  //                  arg_B5_0.health.AddHediff(HediffDefOf.Scaria, null, null, null);
  //                  arg_B5_0.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null, false, false, false);
  //                  arg_B5_0.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(60000, 120000);
  //              }
  //              return true;
  //          }
  //          catch 
		//	{
		//		return false;
		//	}
		//}

        //private static object GetManhunterPackIncidentUtility()
        //{
        //    return ManhunterPackIncidentUtility;
        //}
    }
}
