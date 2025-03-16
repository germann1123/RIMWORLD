using System;
using System.Collections.Generic;
using System.Linq;
using MeteorIncident;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace MeteorIncidentGer
{
    public class TheFlash
    {
        public static int CurrentTick = 0;
        public static int NextCount = 150;
		/// <summary>
		/// 用于雷电季计数
		/// </summary>
		public static int KeepOnCount = 0;
		/// <summary>
		/// 雷暴到达之前等待的时间
		/// </summary>
		public static int InitDelay = 0;

        public static bool IsStorm { get; set; } = false;

		public static void StormUpdate()
		{
            //没有闪电风暴的时候，不进行轮训
            if (!IsStorm) return;

            HotseatGameComponent.TryFindCell(out var TheCell, Find.CurrentMap);
            if (Rand.Chance(0.5f)) Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, TheCell));
            else Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningFlash(Find.CurrentMap));
        }

		public static void StartStorm(System.Action OnClose=null) 
		{
			//上一次闪电还未结束的时候，不允许下一次闪电风暴开始
			if (IsStorm) return;
			IsStorm = true;
			//在未来一个随机时间爆炸
			TheTimerManager.AddTimer(("LigtingStorm_" + Guid.NewGuid().GetHashCode().ToString()), new TimerInfo
			{
				//闪电风暴持续随机小时数
				TTL = Rand.Range(1,3),
				_OnTimerDown = new System.Action(() =>
				{
					IsStorm = false;
					OnClose?.Invoke();
				})
			});
		}
	}
}
