using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorIncidentGer
{
    public class StoryTellerManager
    {
        public static ConcurrentDictionary<string, object> _Cfg = new ConcurrentDictionary<string, object>();
    }
}
