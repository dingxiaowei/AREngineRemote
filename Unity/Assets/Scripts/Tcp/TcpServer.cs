using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using HuaweiARUnitySDK;
using UnityEngine;

/// <summary>
/// 运行在Android端
/// </summary>
public class TcpServer : TcpBase
{
    private Socket socketWatch;
    private TcpState state;
    private Thread rcvThread;
    private float lastT;

    public TcpServer(string ip, int port, Action<string, TcpState> notify)
    {
        state = TcpState.None;
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
            state = TcpState.Quit;
            rcvThread.Abort();
            base.Close(notify);
            socketWatch.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("close ex: " + e);
        }
        Debug.Log("connect close");
    }

    public override void Update()
    {
        if (state > TcpState.None && state < TcpState.Quit)
        {
            if (Time.time - lastT > 0.1f)
            {
                AcquireCpuImage();
                AcquireCloudPoint();
                lastT = Time.time;
            }
        }
    }

    private void WatchConnecting()
    {
        while (true)
            try
            {
                sock = socketWatch.Accept();
                callback("connect:" + sock.RemoteEndPoint, TcpState.Connect);
                state = TcpState.Connect;
                rcvThread = new Thread(Receive);
                rcvThread.IsBackground = true;
                rcvThread.Start(sock);
            }
            catch (Exception ex)
            {
                Debug.LogError("connect error：" + ex);
            }
    }

    private void Receive(object socketClientPara)
    {
        var socketServer = socketClientPara as Socket;
        Recv(socketServer);
    }

    private void AcquireCloudPoint()
    {
        if (ARFrame.GetTrackingState() == ARTrackable.TrackingState.TRACKING)
        {
            List<Vector3> points = new List<Vector3>();
            ARPointCloud pointCloud = ARFrame.AcquirePointCloud();
            pointCloud.GetPoints(points);
            pointCloud.Release();
            int cnt = points.Count;
            if (cnt > 0)
            {
                if (cnt > max_point)
                {
                    points.RemoveRange(max_point, cnt - max_point);
                }
                var transform = MainCamera.transform;
                var pos = transform.position;
                var angle = transform.eulerAngles;
                var projectionMatrix = ARSession.GetProjectionMatrix(MainCamera.nearClipPlane, MainCamera.farClipPlane);
                WriteVector3(pos, headLen);
                WriteVector3(angle, headLen + 12);
                cnt = points.Count;
                Array.Copy(Int2Bytes(cnt), 0, sendBuf, headLen + 24, 4);
                for (int i = 0; i < cnt; i++)
                {
                    int offset = 12 * i + headLen + 28;
                    WriteVector3(points[i], offset);
                }
                SendWithHead(TcpHead.CloudPoint, 28 + 12 * cnt);
            }
        }
    }

    private void WriteVector3(Vector3 v, int offset)
    {
        int x = (int) (v.x * scale_point);
        int y = (int) (v.y * scale_point);
        int z = (int) (v.z * scale_point);
        Array.Copy(Int2Bytes(x), 0, sendBuf, offset, 4);
        Array.Copy(Int2Bytes(y), 0, sendBuf, offset + 4, 4);
        Array.Copy(Int2Bytes(z), 0, sendBuf, offset + 8, 4);
    }
    
    private void AcquireCpuImage()
    {
        var image = ARFrame.AcquireCameraImageBytes();
        int width = image.Width;
        int height = image.Height;
        SendPreview(width, height, image.Y);
        image.Release();
    }

    private void SendPreview(int width, int height, IntPtr ptr)
    {
        int len = (int) (width * height * 1.5f);
        var b1 = Int2Bytes(width);
        var b2 = Int2Bytes(height);
        var b3 = Int2Bytes(len);
        Array.Copy(b1, 0, sendBuf, headLen, 4);
        Array.Copy(b2, 0, sendBuf, 4 + headLen, 4);
        Array.Copy(b3, 0, sendBuf, 8 + headLen, 4);
        Marshal.Copy(ptr, sendBuf, 12 + headLen, len);
        SendWithHead(TcpHead.Preview, len + 12);
    }
}