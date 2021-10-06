using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class TcpBase
    {
        public const int max_point = 1000;
        protected const int headLen = 5;
        protected const int bufSize = 1024 * 1024; // 1M
        protected const int scale_point = 1000;
        protected const SocketFlags flag = SocketFlags.None;
        protected AREngineImage ar_image = new AREngineImage();
        protected AREnginePointCloud ar_point = new AREnginePointCloud();
        protected AREnginePlane ar_plane = new AREnginePlane();
        protected Action<string, TcpState> callback;
        protected volatile bool threadRun = true;
        protected byte[] recvBuf = new byte[bufSize];
        protected byte[] sendBuf = new byte[bufSize];
        protected Socket sock;
        protected Thread thread;

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

        public void Send(string sendMsg)
        {
            try
            {
                if (!string.IsNullOrEmpty(sendMsg))
                {
                    callback(sendMsg, TcpState.Send);
                    var len = Encoding.UTF8.GetBytes(sendMsg, 0, sendMsg.Length, sendBuf, headLen);
                    SendWithHead(TcpHead.String, len);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("send error：" + ex);
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

        protected void Process(int length)
        {
            int offset = 0;
            var head = (TcpHead) recvBuf[4];
            switch (head)
            {
                case TcpHead.Preview:
                    offset = headLen;
                    var width = Bytes2Int(recvBuf, ref offset);
                    var heigth = Bytes2Int(recvBuf, ref offset);
                    ar_image.Set(width, heigth);
                    var len = Bytes2Int(recvBuf, ref offset);
                    var y_len = len * 2 / 3;
                    var uv_len = len / 3;
                    if (ar_image.y_buf == null)
                    {
                        ar_image.y_buf = new byte[y_len];
                        ar_image.uv_buf = new byte[uv_len];
                    }
                    Array.Copy(recvBuf, offset, ar_image.y_buf, 0, y_len);
                    Array.Copy(recvBuf, offset + y_len, ar_image.uv_buf, 0, uv_len);
                    PreviewStreamVisualizer.change = true;
                    break;
                case TcpHead.PointCloud:
                    offset = headLen;
                    ar_point.camPos = RecvVector3(recvBuf, ref offset);
                    ar_point.camAngle = RecvVector3(recvBuf, ref offset);
                    int cnt = Bytes2Int(recvBuf, ref offset);
                    ar_point.len = cnt;
                    Array.Copy(recvBuf, offset, ar_point.buf, 0, 12 * cnt);
                    PointCloudVisualizer.change = true;
                    break;
                case TcpHead.Plane:
                    offset = headLen;
                    int count = Bytes2Int(recvBuf, ref offset);
                    ar_plane.planes = new AREngineVectices[count];
                    for (int i = 0; i < count; i++)
                    {
                        var p = new AREngineVectices();
                        ar_plane.planes[i] = p;
                        int cnt1 = Bytes2Int(recvBuf, ref offset);
                        if (cnt1 > 0)
                        {
                            int cnt2 = Bytes2Int(recvBuf, ref offset);
                            p.label = RecvString(recvBuf, ref offset);
                            var pos = RecvVector3(recvBuf, ref offset);
                            var rot = RecvRot(recvBuf, ref offset);
                            p.pose = new Pose(pos, rot);
                            p.meshVertices3D.Clear();
                            for (int j = 0; j < cnt1; j++)
                            {
                                var v = RecvVector3(recvBuf, ref offset);
                                p.meshVertices3D.Add(v);
                            }
                            p.meshVertices2D.Clear();
                            for (int j = 0; j < cnt2; j++)
                            {
                                var v = RecvVector2(recvBuf, ref offset);
                                p.meshVertices2D.Add(v);
                            }
                        }
                    }
                    ARPlaneVisualizer.change = true;
                    break;
                case TcpHead.String:
                    var strRecMsg = Encoding.UTF8.GetString(recvBuf, headLen, length - headLen);
                    Debug.Log(sock.RemoteEndPoint + " " + DateTime.Now + "\n" + strRecMsg);
                    callback(strRecMsg, TcpState.Receive);
                    break;
                case TcpHead.Quit:
                    callback("connect quit", TcpState.Quit);
                    Close(false);
                    break;
                default:
                    Debug.Log("not process " + head);
                    break;
            }
        }

        protected Quaternion RecvRot(byte[] buf, ref int offset)
        {
            float x = Bytes2Int(buf, ref offset) / (float) scale_point;
            float y = Bytes2Int(buf, ref offset) / (float) scale_point;
            float z = Bytes2Int(buf, ref offset) / (float) scale_point;
            float w = Bytes2Int(buf, ref offset) / (float) scale_point;
            return new Quaternion(x, y, z, w);
        }

        public Vector3 RecvVector3(byte[] buf, ref int offset)
        {
            int x = Bytes2Int(buf, ref offset);
            int y = Bytes2Int(buf, ref offset);
            int z = Bytes2Int(buf, ref offset);
            return new Vector3(x, y, z) / scale_point;
        }

        protected Vector2 RecvVector2(byte[] buf, ref int offset)
        {
            int x = Bytes2Int(buf, ref offset);
            int y = Bytes2Int(buf, ref offset);
            return new Vector2(x, y) / scale_point;
        }

        protected string RecvString(byte[] buf, ref int offset)
        {
            int len = Bytes2Int(buf, ref offset);
            var bytes = new byte[len];
            Array.Copy(buf, offset, bytes, 0, len);
            offset += len;
            return Encoding.Default.GetString(bytes);
        }
    }
}