using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class TcpBase
    {
        public const int max_point = 1000;
        public const int scale_point = 1000;
        protected const int headLen = 5;
        protected const int bufSize = 1024 * 1024; // 1M
        protected const SocketFlags flag = SocketFlags.None;
        protected Action<string, TcpState> callback;
        protected volatile bool threadRun = true;
        protected byte[] recvBuf = new byte[bufSize];
        protected byte[] sendBuf = new byte[bufSize];
        protected Socket sock;
        protected Thread thread;
        protected SceneState sceneState;

        public TcpBase(SceneState st)
        {
            sceneState = st;
        }

        public virtual void Update() { }

        public virtual void Close(bool notify)
        {
            if (notify) NotifyQuit();
            // thread.Abort();
            threadRun = false;
            sock.Close();
        }

        protected void SendWithHead(TcpHead head, int len)
        {
            int packLen = len + headLen;
            var blen = Int2Bytes(packLen);
            Array.Copy(blen, 0, sendBuf, 0, 4);
            sendBuf[4] = (byte) head;
            sock.Send(sendBuf, 0, packLen, flag);
        }

        protected void Recv(Socket socket)
        {
            while (threadRun)
            {
                try
                {
                    var len = socket.Receive(recvBuf, 0, bufSize, flag);
                    if (len > 0)
                    {
                        int packLen = Bytes2Int(recvBuf);
                        while (len < packLen)
                        {
                            len += socket.Receive(recvBuf, len, bufSize - len, flag);
                        }
                        Process(len);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(socket.RemoteEndPoint + " disconnect " + ex);
                    break;
                }
            }
        }

        protected void NotifyQuit()
        {
            SendWithHead(TcpHead.Quit, 0);
        }

        protected byte[] Int2Bytes(int value)
        {
            var src = new byte[4];
            src[3] = (byte) ((value >> 24) & 0xFF);
            src[2] = (byte) ((value >> 16) & 0xFF);
            src[1] = (byte) ((value >> 8) & 0xFF); //高8位
            src[0] = (byte) (value & 0xFF); //低位
            return src;
        }

        protected int Bytes2Int(byte[] src)
        {
            return (src[0] & 0xFF) | ((src[1] & 0xFF) << 8) | ((src[2] & 0xFF) << 16) | ((src[3] & 0xFF) << 24);
        }

        protected int Bytes2Int(byte[] src, ref int offset)
        {
            int v = (src[offset] & 0xFF) | ((src[offset + 1] & 0xFF) << 8) | ((src[offset + 2] & 0xFF) << 16) |
                    ((src[offset + 3] & 0xFF) << 24);
            offset += 4;
            return v;
        }

        protected virtual void Process(int length) { }
    }
}