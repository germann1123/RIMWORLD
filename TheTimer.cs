using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MeteorIncident
{

    public class TimerInfo 
    {
        /// <summary>
        /// 多少小时候触发
        /// </summary>
        public int TTL = 0;
        public (IncidentDef TheIncidentDef, bool IsCheck, IIncidentTarget Target) Data;

        public object _TheData = null;
        /// <summary>
        /// 定时器到期触发事件
        /// </summary>
        public System.Action _OnTimerDown = null;
        public System.Action<object> _OnTimerDown_with_OB = null;
        public System.Action<(IncidentDef TheIncidentDef, bool IsCheck, IIncidentTarget Target)> _OnTimerDown_WithArgrments = null;
    }

    /// <summary>
    /// 小事件计数单位
    /// </summary>
    public class TimerInfoSmall
    {
        /// <summary>
        /// 多少小时候触发
        /// </summary>
        public int TTL = 0;
        public object _Data;
        /// <summary>
        /// 定时器到期触发事件
        /// </summary>
        public System.Action _OnTimerDown = null;

        public System.Action<object> _OnTimerDownWithOB = null;
    }

    public class TheTimerManager 
    {
        public static Dictionary<string, TimerInfo> _TimerInfos = new Dictionary<string, TimerInfo>();
        public static Dictionary<string, TimerInfoSmall> _TimerInfosSmall = new Dictionary<string, TimerInfoSmall>();
        public static object SyncOB = new object();
        public static Map _TriggerMap = null;

        public static bool AddTimer(string TimerStr, TimerInfo Data) 
        {
            lock (SyncOB)
            {
                if (!_TimerInfos.ContainsKey(TimerStr))
                {
                    _TimerInfos.Add(TimerStr, Data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool AddTimerSmall(string TimerStr, TimerInfoSmall Data)
        {
            lock (SyncOB)
            {
                if (!_TimerInfosSmall.ContainsKey(TimerStr))
                {
                    _TimerInfosSmall.Add(TimerStr, Data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool DeleteTimerSmall(string TimerStr)
        {
            lock (SyncOB)
            {
                if (_TimerInfosSmall.ContainsKey(TimerStr))
                {
                    _TimerInfosSmall.Remove(TimerStr);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 批量删除所有同类定时器
        /// </summary>
        public static void DeleteTimer(string TimerStr) 
        {
            lock (SyncOB)
            {
                foreach (var item in _TimerInfos.ToArray())
                {
                    if (item.Key.Contains(TimerStr)) _TimerInfos.Remove(item.Key);
                }
            }
        }
    }

    /// <summary>
    /// 定时器插件
    /// </summary>
    public class TheTimer : GameComponent
    {
        public TheTimer()
        {
        }

        public TheTimer(Game game)
        {
        }

        public override void GameComponentTick()
        {
            //200Tick时间单位
            if (Find.TickManager.TicksGame % 50 == 0)
            {
                lock (TheTimerManager.SyncOB)
                {
                    foreach (var item in TheTimerManager._TimerInfosSmall.ToList())
                    {
                        if (item.Value.TTL-- <= 0)
                        {
                            if(item.Value._Data == null)
                            item.Value._OnTimerDown?.Invoke();
                            else
                                item.Value._OnTimerDownWithOB?.Invoke(item.Value._Data);

                            TheTimerManager._TimerInfosSmall.Remove(item.Key);
                        }
                    }
                }
            }

            //小时单位
            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                lock (TheTimerManager.SyncOB)
                {
                    foreach (var item in TheTimerManager._TimerInfos.ToList())
                    {
                        if (item.Value.TTL-- <= 0) 
                        {
                            item.Value._OnTimerDown?.Invoke();
                            item.Value._OnTimerDown_WithArgrments?.Invoke(item.Value.Data);
                            item.Value._OnTimerDown_with_OB?.Invoke(item.Value._TheData);
                            TheTimerManager._TimerInfos.Remove(item.Key);
                        }
                    }
                }
            }
        }
    }
}
