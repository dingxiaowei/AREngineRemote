using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using HuaweiARUnitySDK;
using UnityEngine;

public class TcpServer : TcpBase
{

    private Socket socketWatch;

    public TcpServer(string ip, int port, Action<string, TcpState> notify)
    {
        socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        callback = notify;
        var ipaddress = IPAddress.Parse(ip);
        var endpoint = new IPEndPoint(ipaddress, port);
        socketWatch.Bind(endpoint);
        //将套接字的监听队列长度限制为1
        socketWatch.Listen(1);
        //创建一个监听线程
        thread = new Thread(WatchConnecting);
        thread.IsBackground = true;
        thread.Start();
    }

    public override void Close(bool notify)
    {
        try
        {
            socketWatch.Close();
            base.Close(notify);
        }
        catch (Exception e)
        {
            Debug.LogError("close ex: " + e);
        }
        Debug.Log("connect close");
    }

    private void WatchConnecting()
    {
        while (true)
            try
            {
                sock = socketWatch.Accept();
                callback("connect:" + sock.RemoteEndPoint, TcpState.Connect);
                var thr = new Thread(Receive);
                thr.IsBackground = true;
                thr.Start(sock);
            }
            catch (Exception ex)
            {
                Debug.LogError("connect error：" + ex);
            }
    }

    private void Receive(object socketClientPara)
    {
        var socketServer = socketClientPara as Socket;
        while (true)
            try
            {
                var length = socketServer.Receive(recvBuf);
                if (length > 0)
                {
                    Process(length);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(socketServer.RemoteEndPoint + " disconnect " + ex);
                break;
            }
    }

    public void AcquireCpuImage()
    {
        var image = ARFrame.AcquireCameraImageBytes();
        int width = image.Width;
        int height = image.Height;
        int len = (int) (width * height * 1.5f);
        var bytes = new byte[len];
        Marshal.Copy(image.Y, bytes, 0, len);
        SendPreview(width, height, len, bytes);
        image.Release();
    }

    protected void SendPreview(int width, int height, int len, byte[] bytes)
    {
        var b1 = Int2Bytes(width);
        var b2 = Int2Bytes(height);
        var b3 = Int2Bytes(len);
        Array.Copy(b1, 0, sendBuf, 1, 4);
        Array.Copy(b2, 0, sendBuf, 5, 4);
        Array.Copy(b3, 0, sendBuf, 9, 4);
        Array.Copy(bytes, 0, sendBuf, 13, len);
        sendBuf[0] =(byte) TcpHead.Preview;
        sock.Send(sendBuf, 0, len + headLen + 12, SocketFlags.None);
    }
}