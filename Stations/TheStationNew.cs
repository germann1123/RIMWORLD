using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncidentGer
{
    public class WorldObject_Ger : WorldObject
    {
        protected const float treasureChance = 0.66f;

        protected const float treasureAmbushChance = 0.7f;

        protected const float humanAmbushChance = 0.6f;

        protected const float ManhunterAmbushPointsFactor = 1.0f;

        protected const float MinimumPointThreshold = 400.0f;

        public void Notify_CaravanArrived(Caravan caravan)
        {
            Find.LetterStack.ReceiveLetter(
                $"调试",
                "一支受雇与其他玩家的友军部队来到你的星系。",
                LetterDefOf.PositiveEvent
                );
        }
    }
}
