﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpBase
{
    protected const int headLen = 5;
    protected const int bufSize = 1024 * 1024; // 1M
    protected AREngineImage ar_image = new AREngineImage();
    protected Action<string, TcpState> callback;
    protected const SocketFlags flag = SocketFlags.None;

    protected volatile bool prev_change;
    protected byte[] recvBuf = new byte[bufSize];
    protected byte[] sendBuf = new byte[bufSize];
    protected Socket sock;

    protected Thread thread;

    public virtual void Update()
    {
    }

    public virtual void Close(bool notify)
    {
        if (notify) NotifyQuit();
        sock.Close();
        thread.Abort();
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
        while (true)
            try
            {
                var len = socket.Receive(recvBuf,0, bufSize, flag);
                if (len > 0)
                {
                    int packLen = Bytes2Int(recvBuf, 0);
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

    protected int Bytes2Int(byte[] src, int offset)
    {
        return (src[offset] & 0xFF)
               | ((src[offset + 1] & 0xFF) << 8)
               | ((src[offset + 2] & 0xFF) << 16)
               | ((src[offset + 3] & 0xFF) << 24);
    }

    protected void Process(int length)
    {
        var head = (TcpHead) recvBuf[4];
        switch (head)
        {
            case TcpHead.Preview:
                var width = Bytes2Int(recvBuf, headLen);
                var heigth = Bytes2Int(recvBuf, 4 + headLen);
                ar_image.Set(width, heigth);
                var len = Bytes2Int(recvBuf, 8 + headLen);
                var y_len = len * 2 / 3;
                var uv_len = len / 3;
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
}