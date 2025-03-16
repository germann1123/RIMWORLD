using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WebSocketSharp;

namespace MeteorIncident
{
    public class WebSocketHelper
    {
        static WebSocket WS = null;

        public static Func<object, MessageEventArgs,bool> _OnWebSocketDataArrive = null;
        public static Func<object, ErrorEventArgs, bool> _OnError = null;
        public static string ServerUrl = "ws://124.221.144.2:6661";

        public static void ReTryConnection() => WS = null;

        /// <summary>
        /// 连接游戏服务器方法
        /// </summary>
        public static void ReConnection() 
        {
            try
            {
                if (WS == null)
                {
                    WS = new WebSocket(SteamUtility.SteamPersonaName.Contains("german") ? ServerUrl : ServerUrl);
                    WS.OnMessage += (sender, e) => { _OnWebSocketDataArrive?.Invoke(sender, e); };
                    WS.OnError += (sender, e) => { _OnError?.Invoke(sender, e); };
                    WS.Connect();
                    WS.Send($"{ SteamUtility.SteamPersonaName } am alive!!!!");
                }
            }
            catch { }
        }

        /// <summary>
        /// 测试样例，RIMWORLD WEBSOCKET发送数据
        /// </summary>
        public static void DoWebSocketSend(byte[] Data) 
        {
            if (WS == null) ReConnection();
            try
            {
                WS.Send(Data);
            }
            catch 
            {
                ReConnection();
            }
        }

        public static void DoWebSocketSend(string Data)
        {
            if (WS == null) ReConnection();
            try
            {
                WS.Send(Data);
            }
            catch
            {
                ReConnection();
            }
        }

        public static async void DoWebSocketSendSync(string Data)
        {
            await Task.Run(() => { });
            if (WS == null) ReConnection();
            try
            {
                WS.Send(Data);
            }
            catch
            {
                ReConnection();
            }
        }
    }
}
