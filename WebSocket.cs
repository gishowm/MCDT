﻿using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCDT
{
    public class WebSocket
    {
        public static void Init(string remote, int port)
        {
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
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
                    //allSockets.ToList().ForEach(
                    //    s =>
                    //s.Send("Echo: " + message)
                    //);
                    socket.Send("AAA Yeah!");
                };
            });
        }
    }
}