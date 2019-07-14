using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BaronReplays
{
    class PlayFileListener
    {
        private UdpClient _listener;
        private IPEndPoint _endPoint;
        private IPEndPoint _localEndPoint;
        private Int32 _port;
        public MainWindow.RecordDelegate PlayEvent;

        public void StartListening()
        {
            _port = 52039;
            _localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            try
            {
                _listener = new UdpClient(_localEndPoint);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10048)
                    Logger.Instance.WriteLog("The port was binded by other socket.");
                return; //如果已經被bind則放棄監聽replay功能
            }
            ReveiveLoop();
        }

        private void ReveiveLoop()
        {
            while (true)
            {
                byte[] content = _listener.Receive(ref _endPoint);
                String message = Encoding.Default.GetString(content);
                Logger.Instance.WriteLog(String.Format("Receive message: {0}", message));

                if (message.CompareTo("Exit") == 0)
                {
                    return;
                }
                else if (File.Exists(message))
                {
                    MainWindow.Instance.PlayFile(message);
                }
                else
                {
                    MainWindow.Instance.TryToPlayCommand(message);
                }
            }

        }

        public void StopListening()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            UdpClient uc = new UdpClient();
            byte[] exitBytes = Encoding.Default.GetBytes("Exit");
            uc.Send(exitBytes, exitBytes.Length, ipep);
            uc.Close();
        }
    }
}
