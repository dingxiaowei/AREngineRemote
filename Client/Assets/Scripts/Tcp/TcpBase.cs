using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpBase
{
    protected const int headLen = 1;
    private const int bufSize =  1024 * 1024; // 1M
    protected byte[] recvBuf = new byte[bufSize];
    protected byte[] sendBuf = new byte[bufSize];

    protected Thread thread;
    protected Socket sock;
    protected Action<string, TcpState> callback;

    protected volatile bool prev_change;
    protected AREngineImage ar_image = new AREngineImage();

    public virtual void Update()
    {
    }

    public virtual void Close(bool notify)
    {
        if (notify)
        {
            NotifyQuit();
        }
        sock.Close();
        thread.Abort();
    }

    public void Send(string sendMsg)
    {
        try
        {
            if (!string.IsNullOrEmpty(sendMsg))
            {
                callback(sendMsg, TcpState.Send);
                int len = Encoding.UTF8.GetBytes(sendMsg, 0, sendMsg.Length, sendBuf, headLen);
                sendBuf[0] = (byte) TcpHead.String;
                sock.Send(sendBuf, 0, len + headLen, SocketFlags.None);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("send error：" + ex);
        }
    }

    protected void NotifyQuit()
    {
        sendBuf[0] = (byte) TcpHead.Quit;
        sock.Send(sendBuf, 0, headLen, SocketFlags.None);
    }


    public byte[] Int2Bytes(int value)
    {
        byte[] src = new byte[4];
        src[3] = (byte) ((value >> 24) & 0xFF);
        src[2] = (byte) ((value >> 16) & 0xFF);
        src[1] = (byte) ((value >> 8) & 0xFF); //高8位
        src[0] = (byte) (value & 0xFF); //低位
        return src;
    }

    public int Bytes2Int(byte[] src, int offset)
    {
        return (src[offset] & 0xFF)
               | ((src[offset + 1] & 0xFF) << 8)
               | ((src[offset + 2] & 0xFF) << 16)
               | ((src[offset + 3] & 0xFF) << 24);
    }

    protected void Process(int length)
    {
        var head = (TcpHead) recvBuf[0];
        switch (head)
        {
            case TcpHead.Preview:
                int width = Bytes2Int(recvBuf, headLen);
                int heigth = Bytes2Int(recvBuf, 4 + headLen);
                ar_image.Set(width,heigth);
                int len = Bytes2Int(recvBuf, 8 + headLen);
                int y_len = len * 2 / 3;
                int uv_len = len / 3;
                if (ar_image.y_buf == null)
                {
                    ar_image.y_buf = new byte[y_len];
                    ar_image.uv_buf = new byte[uv_len];
                }
                Array.Copy(recvBuf, 12 + headLen, ar_image.y_buf, 0, y_len);
                Array.Copy(recvBuf, 12 + headLen + y_len, ar_image.uv_buf, 0, uv_len);
                prev_change = true;
                break;
            case TcpHead.String:
                string strRecMsg = Encoding.UTF8.GetString(recvBuf, headLen, length - headLen);
                Debug.Log(sock.RemoteEndPoint + " " + DateTime.Now + "\n" + strRecMsg);
                callback(strRecMsg, TcpState.Recv);
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

}

public class AREngineImage
{
    public int width, height;
    public byte[] y_buf, uv_buf;

    public void Set(int w, int h)
    {
        width = w;
        height = h;
    }

    public void Set(byte[] yBuf, byte[] uvBuf)
    {
        y_buf = yBuf;
        uv_buf = uvBuf;
    }
}
