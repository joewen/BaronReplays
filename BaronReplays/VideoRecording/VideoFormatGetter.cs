using BaronReplays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BaronReplays.VideoRecording
{
    public class VideoFormatGetter
    {
        private VideoFormat result;
        public VideoFormat Result
        {
            get
            {
                return result;
            }
        }

        public String IpAddress
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public VideoFormatGetter()
        {
            IpAddress = "127.0.0.1";
            Port = 13579;
        }

        public VideoFormatGetter(String ip, int port)
        {
            IpAddress = ip;
            Port = port;
        }

        public VideoFormat GetVideoFormat()
        {
            result = new VideoFormat();

            System.Net.IPAddress serverAddress = System.Net.IPAddress.Parse(IpAddress);
            TcpListener listener = new TcpListener(serverAddress, Port);

            listener.Start();

            Socket dataSocket = listener.AcceptSocket();
            dataSocket.ReceiveTimeout = 10000;
            byte[] buf = new byte[128];
            try
            {
                if (dataSocket.Connected)
                {
                    int size = dataSocket.Receive(buf);
                    ParseDataToForamt(buf);
                }
            }
            catch (SocketException se)
            {
                result = null;
            }
            finally
            {
                dataSocket.Close();
                listener.Stop();
            }

            return result;
        }

        private void ParseDataToForamt(byte[] data)
        {
            result = new VideoFormat();
            result.Width = BitConverter.ToInt32(data, 0);
            result.Height = BitConverter.ToInt32(data, 4);
            result.Bits = BitConverter.ToInt32(data, 8);
        }

    }
}
