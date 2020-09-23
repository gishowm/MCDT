using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCDT
{
    public class WebSocket
    {
        static List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        public static void Init(string remote, int port)
        {
            FleckLog.Level = LogLevel.Debug;
            var server = new WebSocketServer("ws://" + remote + ":" + port);
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    
                    socket.Send("AAA Yeah!");
                };
            });
        }
    }
}
