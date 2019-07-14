using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;

namespace BaronReplays
{
    public class LoLRecordPlayer
    {
        private int _port;
        private LoLRecord record;
        private TcpListener listener = null;
        public Boolean useAdvanceReplay { get; set; }

        public Int32 Port
        {
            get
            {
                return _port;
            }
        }

        public LoLRecordPlayer(LoLRecord playThis)
        {
            initResponseWorker();
            waitingResponse = new Queue<Socket>();
            useAdvanceReplay = Properties.Settings.Default.AdvanceReplay;
            this.record = playThis;
            randomPort();
            System.Net.IPAddress serverAddress = System.Net.IPAddress.Parse("127.0.0.1"); // <-- Change that as appropriate!
            try
            {
                this.listener = new TcpListener(serverAddress, _port);
                Logger.Instance.WriteLog(String.Format("Record player start listen to port {0}", _port));
                this.listener.Start();
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog("TcpListener failed");
                Logger.Instance.WriteLog(ex.Message);
            }
        }



        public void StopPlaying()
        {
            listener.Server.Close();
            listener.Stop();
        }

        private void randomPort()
        {
            Random rd = new Random();
            _port = rd.Next(10000, 60000);
        }

        Queue<Socket> waitingResponse;

        private BackgroundWorker responseWorker;

        private void initResponseWorker()
        {
            responseWorker = new BackgroundWorker();
            responseWorker.DoWork += Response;
            responseWorker.RunWorkerCompleted += ResponseDone;
        }

        private void Response(object sender, DoWorkEventArgs e)
        {
            try
            {
                byte[] buf = new byte[512];
                Socket socket = waitingResponse.Dequeue();
                int dLen = socket.Receive(buf);
                string request = Encoding.ASCII.GetString(buf, 0, dLen);
                byte[] response = gotRequest(request);
                socket.Send(response, SocketFlags.None);
                socket.Close();
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog("Record player response exception");
                Logger.Instance.WriteLog(ex.Message);
            }
        }

        private void ResponseDone(object sender, RunWorkerCompletedEventArgs e)
        {
            if(waitingResponse.Count > 0)
                responseWorker.RunWorkerAsync();
        }

        public void startPlaying()
        {
            do
            {
                try
                {
                    Socket dataSocket = listener.AcceptSocket();
                    waitingResponse.Enqueue(dataSocket);
                    if (!responseWorker.IsBusy)
                        responseWorker.RunWorkerAsync();
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode == 10004)
                    {
                        Logger.Instance.WriteLog("Stop listening");
                    }
                    break;
                }
            }
            while (true);
        }


        private string makeHeader(int size, int type)
        {
            StringBuilder header = new StringBuilder();
            header.Append("HTTP/1.1 200 OK\r\n");
            //header.Append("Server: Apache-Coyote/1.1\r\n");
            header.Append("Server: BaronReplays\r\n");
            header.Append("Pragma: no-cache\r\n");
            header.Append("Cash-Control: no-cache\r\n");
            header.Append("Expires: Thu, 01 Jan 1970 08:00:00 CST \r\n");
            if (type == 1)
                header.Append("Content-Type: application/json\r\n");
            if (type == 2)
                header.Append("Content-Type: application/octet-stream\r\n");
            else
                header.Append("Content-Type: text/plain\r\n");
            header.Append("Content-Length: " + size + "\r\n");
            header.Append("Accept-Ranges: bytes\r\n");
            CultureInfo usclut = new CultureInfo("en-US");
            DateTime now = new DateTime();
            now = DateTime.UtcNow;
            header.Append("Date: " + now.ToString("ddd, dd MM yyyy HH:mm:ss" + " GMT\r\n", usclut));
            header.Append("Connection: close\r\n\r\n");
            return header.ToString();
        }

        private string makeNotFoundHeader()
        {
            StringBuilder header = new StringBuilder();
            header.Append("HTTP/1.1 404 Not Found\r\n");
            //header.Append("Server: Apache-Coyote/1.1\r\n");
            header.Append("Server: BaronReplays\r\n");
            header.Append("Pragma: no-cache\r\n");
            header.Append("Cash-Control: no-cache\r\n");
            header.Append("Expires: Thu, 01 Jan 1970 08:00:00 CST \r\n");
            header.Append("Content-Type: text/plain\r\n");
            header.Append("Content-Length: " + 0 + "\r\n");
            header.Append("Accept-Ranges: bytes\r\n");
            CultureInfo usclut = new CultureInfo("en-US");
            DateTime now = new DateTime();
            now = DateTime.UtcNow;
            header.Append("Date: " + now.ToString("ddd, dd MM yyyy HH:mm:ss" + " GMT\r\n", usclut));
            header.Append("Connection: close\r\n\r\n");
            return header.ToString();
        }


        private byte[] strToByte(string str)
        {
            byte[] buf;
            buf = Encoding.ASCII.GetBytes(str);
            return buf;
        }

        private bool checkGameId(string getRequest)
        {
            Regex gameIdRegex = new Regex(@"/" + record.GamePlatform + @"/(?<GID>[0-9]+)", RegexOptions.IgnoreCase);
            Match match = gameIdRegex.Match(getRequest);
            if (!match.Success)
                return false;
            return this.record.gameId == long.Parse(match.Groups["GID"].Value);
        }

        private byte[] appendBytes(byte[] b1, byte[] b2)
        {
            byte[] result = new byte[b1.Length + b2.Length];
            b1.CopyTo(result, 0);
            b2.CopyTo(result, b1.Length);
            return result;
        }

        private bool LoLloaded = false;
        private byte[] gotRequest(string reqstr)
        {
            if (reqstr.Contains("/observer-mode/rest/consumer/version"))
            {
                if (!useAdvanceReplay)
                    LoLloaded = true;

                return strToByte(makeHeader(this.record.spectatorClientVersion.Length, 0) + new String(this.record.spectatorClientVersion));
                //return strToByte(makeHeader(7, 0) + "1.80.130");
            }
            else
            {
                if (!checkGameId(reqstr))
                    return strToByte(makeNotFoundHeader());
                if (reqstr.Contains("/observer-mode/rest/consumer/getGameMetaData/" + record.GamePlatform))
                {
                    string meta = this.record.getMetaData();
                    return strToByte(makeHeader(meta.Length, 1) + meta);
                }
                else if (reqstr.Contains("/observer-mode/rest/consumer/getLastChunkInfo/" +record.GamePlatform))
                {
                    if (!LoLloaded)
                    {
                        LoLloaded = gameHasLoaded();
                    }
                    String lastChunkInfo;
                    if (LoLloaded)
                    {
                        lastChunkInfo = this.record.getLastChunkInfo();
                    }
                    else
                    {
                        lastChunkInfo = this.record.getStartUpChunkInfo();
                    }
                    return strToByte(makeHeader(lastChunkInfo.Length, 1) + lastChunkInfo);
                }
                else if (reqstr.Contains("/observer-mode/rest/consumer/getGameDataChunk/" + record.GamePlatform + "/"))
                {
                    Regex gameIdRegex = new Regex(@"/observer-mode/rest/consumer/getGameDataChunk/" + record.GamePlatform + "/(?<GID>[0-9]+)/(?<NUMBER>[0-9]+)", RegexOptions.IgnoreCase);
                    Match match = gameIdRegex.Match(reqstr);
                    byte[] chunkBytes = this.record.getChunkContent(int.Parse(match.Groups["NUMBER"].Value));
                    byte[] header = strToByte(makeHeader(chunkBytes.Length, 2));
                    return appendBytes(header, chunkBytes);
                }
                else if (reqstr.Contains("/observer-mode/rest/consumer/getKeyFrame/" + record.GamePlatform + "/"))
                {
                    Regex gameIdRegex = new Regex(@"/observer-mode/rest/consumer/getKeyFrame/" + record.GamePlatform + "/(?<GID>[0-9]+)/(?<NUMBER>[0-9]+)", RegexOptions.IgnoreCase);
                    Match match = gameIdRegex.Match(reqstr);
                    byte[] chunkBytes = this.record.getKeyFrameContent(int.Parse(match.Groups["NUMBER"].Value));
                    byte[] header = strToByte(makeHeader((int)chunkBytes.Length, 2));
                    return appendBytes(header, chunkBytes);
                }
                else
                    return strToByte(makeNotFoundHeader());
            }
            return null;
        }

        private bool gameHasLoaded()
        {
            var logContent = LoLWatcher.Instance.ExecutionData.LogContent;
            if (!logContent.Contains("GAMESTATE_GAMELOOP End"))
                return false;
            return true;
        }
    }
}
