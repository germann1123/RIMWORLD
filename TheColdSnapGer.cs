using System;
using System.Collections.Generic;
using System.Linq;
using MeteorIncident;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MeteorIncident
{
	/// <summary>
	/// 寒流
	/// </summary>
    public class GameCondition_ColdSnapGer : GameCondition
	{
		private const float MaxTempOffset = -40f;
		public static bool IsColdSnapOver = true;

		private SkyColorSet EclipseSkyColors = new SkyColorSet(new Color(0.0f, 0.0f, 0.25f), Color.white, new Color(0.0f, 0.0f, 0.25f), 0.9f);
		public override SkyTarget? SkyTarget(Map map) => new SkyTarget?(new SkyTarget(0f, this.EclipseSkyColors, 1f, 0f));

		public override float SkyTargetLerpFactor(Map map)
		{
			return GameConditionUtility.LerpInOutValue(this, 200, 1f);
		}

		public override void End()
		{
			base.End();

			Find.LetterStack.ReceiveLetter(
					"黑夜寒流结束",
					"恐怖寒流已经离开本区域",
					LetterDefOf.NeutralEvent
					);

			IsColdSnapOver = true;
		}

		public override int TransitionTicks
		{
			get
			{
				return 5000;
			}
		}

		public override float TemperatureOffset()
		{
			return GameConditionUtility.LerpInOutValue(this, (float)this.TransitionTicks, -40f);
		}
	}
}
