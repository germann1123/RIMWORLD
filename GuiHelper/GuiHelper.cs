

using MeteorIncident;
using RimWorld;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace MeteorIncidentGer
{
    public static class GuiHelper
    {
        /// <summary>
        /// 缓存当前STEAM用户名称，用户网络发送
        /// </summary>
        public static string TriggerPlayerName = "";
        /// <summary>
        /// 邪神献祭事件
        /// </summary>
        public static void DoTheGuShen(this Map TheMap) 
        {
            //缓存当前STEAM玩家的昵称
            TriggerPlayerName = SteamUtility.SteamPersonaName;
            if (TheMap.mapPawns.FreeColonists.Count == 0) return;
            var KillPawn = TheMap.mapPawns.FreeColonists.RandomElement();
            var Window = new Dialog_MessageBox($"邪神的献祭 \r\n\r\n 将要被祭祀的人选为 { KillPawn.Name.ToString().Colorize(UnityEngine.Color.red) }\r\n\r\n" +
                $"献祭后你将会获得三次空投!".Colorize(UnityEngine.Color.green) + "\r\n" + "请注意您只有60秒钟的考虑时间!".Colorize(UnityEngine.Color.red), "献祭", delegate ()
             {
                 //献祭小人
                 KillPawn.Destroy();
                 Find.LetterStack.ReceiveLetter($"死亡取走了- { KillPawn.Name }", $"死亡取走了- { KillPawn.Name }", LetterDefOf.NegativeEvent);

                 HotseatGameComponent.DoTheGerPod(); //补给仓坠毁
                 HotseatGameComponent.DoTheGerPod(); //补给仓坠毁
                 HotseatGameComponent.DoTheGerPod(); //补给仓坠毁
             },"拒绝",
             async delegate ()
             {
                 await Task.Run(() => { });
                 try
                 {
                     WebSocketHelper.DoWebSocketSend($"#ClientCommand#Xieshen#{false}#{TriggerPlayerName}#");
                 }
                 catch { }
                 //Find.LetterStack.ReceiveLetter($"死亡取走了- { KillPawn.Name }", $"死亡取走了- { KillPawn.Name }", LetterDefOf.NegativeEvent);
             });

            //界面超时
            Task.Run(() =>
            {
                for (int loopa = 0; loopa < 60; loopa++)
                {
                    if (!Window.IsOpen) return;
                    Window.buttonAText = $"献祭({(60 - loopa)})";
                    Thread.Sleep(1000);
                }
                Window.Close();
            });

            Find.WindowStack.Add(Window);
        }

        //public static void MianChangDeJueBie(this Map TheMap)
        //{
        //    if (TheMap.mapPawns.FreeColonists.Count == 0) return;
        //    var KillPawn = TheMap.mapPawns.FreeColonists.RandomElement();
        //    var Window = new Dialog_MessageBox($"绵长的诀别 \r\n\r\n 将要被祭祀的人选为 { KillPawn.Name.ToString().Colorize(UnityEngine.Color.red) }\r\n\r\n" +
        //        $"献祭后你将会获得三次空投!".Colorize(UnityEngine.Color.green) + "\r\n" + "请注意您只有60秒钟的考虑时间!".Colorize(UnityEngine.Color.red), "献祭", delegate ()
        //        {
        //            //献祭小人
        //            KillPawn.Destroy();
        //            Find.LetterStack.ReceiveLetter($"死亡取走了- { KillPawn.Name }", $"死亡取走了- { KillPawn.Name }", LetterDefOf.NegativeEvent);

        //            HotseatGameComponent.DoTheGerPod(); //补给仓坠毁
        //            HotseatGameComponent.DoTheGerPod(); //补给仓坠毁
        //            HotseatGameComponent.DoTheGerPod(); //补给仓坠毁
        //        }, "拒绝",
        //     delegate ()
        //     {
        //         //Find.LetterStack.ReceiveLetter($"死亡取走了- { KillPawn.Name }", $"死亡取走了- { KillPawn.Name }", LetterDefOf.NegativeEvent);
        //     });

        //    //界面超时
        //    Task.Run(() =>
        //    {
        //        for (int loopa = 0; loopa < 60; loopa++)
        //        {
        //            if (!Window.IsOpen) return;
        //            Window.buttonAText = $"献祭({(60 - loopa)})";
        //            Thread.Sleep(1000);
        //        }
        //        Window.Close();
        //    });

        //    Find.WindowStack.Add(Window);
        //}


    }
}
