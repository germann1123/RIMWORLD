using MeteorIncident;
using MeteorIncidentGer;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_ItemStash_Ger : GenStep
	{
		public ThingSetMakerDef thingSetMakerDef;
		public static System.Random RG { get; set; } = new System.Random(Guid.NewGuid().GetHashCode());

		public FloatRange defaultPointsRange = new FloatRange(300f, 500f);

		private int MinRoomCells = 225;

		public override int SeedPart
		{
			get=>RG.Next(1,int.MaxValue);
		}

		//protected override bool CanScatterAt(IntVec3 c, Map map)
		//{
		//	if (!base.CanScatterAt(c, map))
		//	{
		//		return false;
		//	}
		//	if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
		//	{
		//		return false;
		//	}
		//	if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false, false, false)))
		//	{
		//		return false;
		//	}
		//	CellRect rect = CellRect.CenteredOn(c, 7, 7);
		//	List<CellRect> list;
		//	if (MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out list) && list.Any((CellRect x) => x.Overlaps(rect)))
		//	{
		//		return false;
		//	}
		//	foreach (IntVec3 current in rect)
		//	{
		//		if (!current.InBounds(map) || current.GetEdifice(map) != null)
		//		{
		//			return false;
		//		}
		//	}
		//	return true;
		//}

		/// <summary>
		/// 空头仓随机空投资源
		/// </summary>
		/// <param name="TheMap"></param>
		public void DoDrop(Map TheMap) 
		{
			try
			{
				Map map = TheMap;
				if (TheMap != null)
				{
					List<Thing> things = ThingSetMakerDefOf.ResourcePod.root.Generate();
					IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
					DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true, true);
				}
			}
			catch { }
		}

		public static Map TriggerMap = null;
		public override void Generate(Map map, GenStepParams parms)
		{
			TriggerMap = map;

			try
            {
				//随机刷出发狂动物
				if (RG.Next(1, 10000) >= 7000) goto TheEnd;
                TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false).WithFenceblocked(true);
                IntVec3 root;
                if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParams) && x.GetRoom(map).CellCount >= this.MinRoomCells, map, out root))
                {
                    float points = (parms.sitePart != null) ? parms.sitePart.parms.threatPoints : this.defaultPointsRange.RandomInRange;
                    PawnKindDef animalKind;
                    if (parms.sitePart != null && parms.sitePart.parms.animalKind != null)
                    {
                        animalKind = parms.sitePart.parms.animalKind;
                    }
                    else if (!ManhunterPackGenStepUtility.TryGetAnimalsKind(points, map.Tile, out animalKind))
                    {
                        goto TheEnd;
                        //return;
                    }
                    //List<Pawn> list = ManhunterPackIncidentUtility.GenerateAnimals(animalKind, map.Tile, points, 0);

                    //TriggerMap
					//过一段时间后的，临时地图偷袭行动
                    TheTimerManager.AddTimer(("ManHunterPack_ResoursePod" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
                    {
                        TTL = Rand.Range(8, 20),
                        _OnTimerDown = new System.Action(() =>
                        {
							try
							{
								if (TriggerMap == null) return;
								if (RG.Next(1, 10000) < 4444) return; //偷袭行动到的时候，44%概率取消
								TraverseParms traverseParams1 = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false).WithFenceblocked(true);
								IntVec3 root1;
								if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => x.Standable(TriggerMap) && !x.Fogged(TriggerMap) && TriggerMap.reachability.CanReachMapEdge(x, traverseParams1) && x.GetRoom(TriggerMap).CellCount >= this.MinRoomCells, TriggerMap, out root1))
								{
									return;
								}

								List<Pawn> list1 = new List<Pawn> { PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, Faction.OfInsects), };
								for (int loopa = 0; loopa < RG.Next(1, (3 + (HotseatGameComponent.TuFeiTanCount / 10))); loopa++)
								{
									list1.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, Faction.OfInsects));
								}

								for (int i = 0; i < list1.Count; i++)
								{
									IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root1, TriggerMap, 10);
									GenSpawn.Spawn(list1[i], loc, TriggerMap, Rot4.Random, WipeMode.Vanish, false);
									list1[i].health.AddHediff(HediffDefOf.Scaria, null, null, null);
									list1[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, false, null, false, false);
								}
							}
							catch { }
						}),
                    });

                    List<Pawn> list = new List<Pawn> { PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, Faction.OfInsects), };
                    for (int loopa = 0; loopa < RG.Next(1, (3 + (HotseatGameComponent.TuFeiTanCount / 10))); loopa++)
                    {
                        list.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, Faction.OfInsects));
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root, map, 10);
                        GenSpawn.Spawn(list[i], loc, map, Rot4.Random, WipeMode.Vanish, false);
                        list[i].health.AddHediff(HediffDefOf.Scaria, null, null, null);
                        list[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, false, null, false, false);
                    }
                }//if
            }
            catch { }

        TheEnd:;
			//随机空头仓
			//if (RG.Next(1, 1000) < 100)
			//{
			//Find.LetterStack.ReceiveLetter("货舱即将到达","你发现了一个资源舱投放点，等待一段时间后接收物资",LetterDefOf.NegativeEvent);

			//TheTimerManager.AddTimer(("ItemStash_ResoursePod" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
			//{
			//	TTL = Rand.Range(2, 4),
			//	_OnTimerDown = new Action(() => DoDrop(map)),
			//});

			//TheTimerManager.AddTimer(("ItemStash_ResoursePod" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
			//{
			//	TTL = Rand.Range(4, 8),
			//	_OnTimerDown = new Action(() => DoDrop(map)),
			//});
			//}

			//HotseatGameComponent.CreateRandomRoom(map);

			WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#TuFeiYingDeTanSuo#{SteamUtility.SteamPersonaName}#");		
			HotseatGameComponent.CreateRandomRoom(map);
		}
	}

	public class GenStep_ItemStash_Ger1 : GenStep
	{
		public ThingSetMakerDef thingSetMakerDef;
		public static System.Random RG { get; set; } = new System.Random(Guid.NewGuid().GetHashCode());

		public FloatRange defaultPointsRange = new FloatRange(300f, 500f);

		private int MinRoomCells = 225;

		public override int SeedPart
		{
			get => RG.Next(1, int.MaxValue);
		}

		public static Map TriggerMap = null;
		/// <summary>
		/// 探索友军地图
		/// </summary>
		public override void Generate(Map map, GenStepParams parms)
		{
			TriggerMap = map;
			///map.info.Size = new IntVec3(new Vector2(20, 100));
			//WebSocketHelper.DoWebSocketSendSync($"#ClientCommand#TuFeiYingDeTanSuo#{SteamUtility.SteamPersonaName}#");
			HotseatGameComponent.CreateRandomRoom_HasFriend(map);
		}
	}
}
