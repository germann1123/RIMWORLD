using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncident.Common
{
    public class WaponCommon
    {
		public static List<Thing> GenerateWeapons(int gunCount)
		{
			List<Thing> list = new List<Thing>();
			IEnumerable<ThingDef> arg_2B_0 = ThingSetMakerUtility.allGeneratableItems;
			Func<ThingDef, bool> arg_2B_1;

			IEnumerable<ThingDef> source = arg_2B_0.Where(XX=>XX.weaponTags != null);
			for (int i = 0; i < gunCount; i++)
			{
				ThingDef thingDef;
				source.TryRandomElement(out thingDef);
				bool flag = thingDef == null;
				if (flag)
				{
					Log.Warning("Could not resolve thingdef to spawn weapons");
				}
				else
				{
					Thing thing = ThingMaker.MakeThing(thingDef, null);
					CompQuality compQuality = thing.TryGetComp<CompQuality>();
					bool flag2 = compQuality != null;
					if (flag2)
					{
						compQuality.SetQuality(QualityUtility.GenerateQualityTraderItem(), ArtGenerationContext.Outsider);
					}
					thing.HitPoints -= Rand.RangeInclusive(1, 10);
					list.Add(thing);
				}
			}
			return list;
		}
	}
}
