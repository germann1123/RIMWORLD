using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MeteorIncident
{
    public class TheLostResouce
    {
		private static readonly IntRange CountRange = new IntRange(10, 20);

		private static bool TryFindRootCell(Map map, out IntVec3 cell)
		{
			return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => CanSpawnAt(x, map) && x.GetRoomOrAdjacent(map, RegionType.Set_Passable).CellCount >= 5, map, out cell);
		}

		private static bool CanSpawnAt(IntVec3 c, Map map)
		{
			if (!c.Standable(map) || c.Fogged(map) || map.fertilityGrid.FertilityAt(c) < ThingDefOf.Plant_Ambrosia.plant.fertilityMin || !c.GetRoomOrAdjacent(map, RegionType.Set_Passable).PsychologicallyOutdoors || c.GetEdifice(map) != null || !PlantUtility.GrowthSeasonNow(c, map, false))
			{
				return false;
			}
			//Plant plant = c.GetPlant(map);
			//if (plant != null && plant.def.plant.growDays > 10f)
			//{
			//	return false;
			//}
			//List<Thing> thingList = c.GetThingList(map);
			//for (int i = 0; i < thingList.Count; i++)
			//{
			//	if (thingList[i].def == ThingDefOf.Plant_Ambrosia)
			//	{
			//		return false;
			//	}
			//}
			return true;
		}

		public static void DoReflushLostResouce() 
        {
			Map map = Find.CurrentMap;
			IntVec3 intVec;

			if (!TryFindRootCell(map, out intVec))
			{
				return;
			}

			Find.LetterStack.ReceiveLetter(
					"被遗忘的资源",
					"一些被遗弃的资源被发现，它们可能来自于旅行者",
					LetterDefOf.PositiveEvent
					);

			//var TheCell = CellFinder.RandomCell(Find.CurrentMap);


			Thing thing = null;
			int randomInRange = CountRange.RandomInRange;
			for (int i = 0; i < Rand.Range(1, 28); i++)
			{
				IntVec3 arg_71_0 = intVec;
				Map arg_71_1 = map;
				int arg_71_2 = 6;

				IntVec3 intVec2;
				if (!CellFinder.TryRandomClosewalkCellNear(arg_71_0, arg_71_1, arg_71_2, out intVec2, XX => CanSpawnAt(XX, map)))
				{
					break;
				}

				//Find.CurrentMap.mapDrawer.MapMeshDirty(intVec2,MapMeshFlag.FogOfWar);

				//var TriggerEx = intVec2.get

				//Plant plant = intVec2.GetPlant(map);
				//if (plant != null)
				//{
				//	plant.Destroy(DestroyMode.Vanish);
				//}
				var TriggerThing = Rand.Chance(0.5f) ? ThingDefOf.Beer : ThingDefOf.WoodLog;
				if (Rand.Chance(0.1f)) TriggerThing = ThingDefOf.MedicineIndustrial;
				if (Rand.Chance(0.1f)) TriggerThing = ThingDefOf.InsectJelly;
				if (Rand.Chance(0.1f)) TriggerThing = ThingDefOf.MealFine;
				
				//根据不同东西刷新不同的数量
				Thing thing2 = GenSpawn.Spawn(TriggerThing, intVec2, map, WipeMode.Vanish);
				if (thing2 != null && thing2.def == ThingDefOf.Beer) thing2.stackCount = Rand.Range(1, 3);
				if (thing2 != null && thing2.def == ThingDefOf.WoodLog) thing2.stackCount = Rand.Range(1, 60);
				if (thing2 != null && thing2.def == ThingDefOf.MedicineIndustrial) thing2.stackCount = Rand.Range(1, 3);
				if (thing2 != null && thing2.def == ThingDefOf.InsectJelly) thing2.stackCount = Rand.Range(1, 75);
				if (thing2 != null && thing2.def == ThingDefOf.MealFine) thing2.stackCount = Rand.Range(1, 4);

				if (thing == null)
				{
					thing = thing2;
				}
			}
			if (thing == null) return;
			
			//base.SendStandardLetter(parms, thing, Array.Empty<NamedArgument>());
		}
    }
}
