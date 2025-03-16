using MeteorIncidentGer;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncident
{
    public class QuestHelper
    {
        static float points = StorytellerUtility.DefaultThreatPointsNow(Find.World);
        static List<QuestScriptDef> questDefsToProcess = DefDatabase<QuestScriptDef>.AllDefs.Where(x => !x.isRootSpecial && x.IsRootAny).ToList();

		
		public static bool NewGiveQuest(float QuestLevel = 1) 
		{
			if (StorytellerComp_RandomEventer.CurrentTarget == null) return false;
			points = StorytellerUtility.DefaultThreatPointsNow(Find.World);
			

			QuestScriptDef newQuestCandidate = NaturalRandomQuestChooser.ChooseNaturalRandomQuest(points * QuestLevel, StorytellerComp_RandomEventer.CurrentTarget);
			//if (HotseatGameComponent.RG.Next(1, 10000) < 2500)
			//{
			//	newQuestCandidate = questDefsToProcess.Where(XX => XX.defName.Contains("OpportunitySite_ItemStash")).FirstOrDefault() ?? questDefsToProcess.RandomElement();
			//}
			//questDefsToProcess.Remove(newQuestCandidate);
			try
			{
				Slate slate = new Slate();
				if (newQuestCandidate.CanRun(slate))
				{
					Quest quest = QuestGen.Generate(newQuestCandidate, slate);
					if (quest != null)
					{
						Find.QuestManager.Add(quest);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex + " can't generate " + newQuestCandidate);
			}
			return false;
			return true;
		}

        public static bool AddRamdomQuest(float QuestLevel=-1) 
        {
			var MaxTryCount = 0;

			points = StorytellerUtility.DefaultThreatPointsNow(Find.World);
			//points = 1000;
			questDefsToProcess = DefDatabase<QuestScriptDef>.AllDefs.Where(x => !x.isRootSpecial && x.IsRootAny).ToList();
            if (!questDefsToProcess.Any())
            {
                return false;
            }

		RestartQuest:;
			QuestScriptDef newQuestCandidate = questDefsToProcess.RandomElement();
			if (HotseatGameComponent.RG.Next(1, 10000) < 2500 && Current.Game.storyteller.def.defName != "RandyEx1") 
			{
				newQuestCandidate = questDefsToProcess.Where(XX => XX.defName.Contains("OpportunitySite_ItemStash")).FirstOrDefault() ?? questDefsToProcess.RandomElement();
			}

			if (HotseatGameComponent.RG.Next(1, 10000) < 4500 && Current.Game.storyteller.def.defName == "RandyEx1")
			{
				newQuestCandidate = questDefsToProcess.Where(XX => XX.defName.Contains("OpportunitySite_PrisonerWillingToJoin")).FirstOrDefault() ?? questDefsToProcess.RandomElement();
				QuestLevel /= 5;
			}
			//questDefsToProcess.Remove(newQuestCandidate);
			try
			{
				Slate slate = new Slate();
				slate.Set("points", points* QuestLevel == -1 ? HotseatGameComponent.RG.Next(2,10) : QuestLevel);
				if (newQuestCandidate == QuestScriptDefOf.LongRangeMineralScannerLump)
				{
					slate.Set("targetMineable", ThingDefOf.MineableGold);
					slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
				}
				if (newQuestCandidate.CanRun(slate))
				{
					//Quest quest = QuestGen.Generate(newQuestCandidate, slate);					
					//if (quest != null)
					//{
					//	Find.QuestManager.Add(quest);
					//	return true;
					//}

					Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(newQuestCandidate, QuestLevel);
					if (!quest.hidden && newQuestCandidate.sendAvailableLetter)
					{
						QuestUtility.SendLetterQuestAvailable(quest);
						return true;
					}
				}
				else 
				{
					//如果尝试50次给与任务后，依然失败，则放弃
					if (MaxTryCount++ >= 50) return false;
					goto RestartQuest;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex + " can't generate " + newQuestCandidate);
			}
			return false;
		}
    }
}
