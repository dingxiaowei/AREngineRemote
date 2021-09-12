using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using HuaweiARUnitySDK;
using UnityEngine;


/// <summary>
/// 运行在Editor中
/// </summary>
public class TcpClient : TcpBase
{
    private BackGroundRenderer renderer;
    private Texture2D texY, texUV;
    
    public TcpClient(string ip, int port, Action<string, TcpState> notify)
    {
        try
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.NoDelay = true;
            callback = notify;
            renderer = GameObject.FindObjectOfType<BackGroundRenderer>();
            Connect(ip, port);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void Connect(string ip, int port)
    {
        try
        {
            var ipaddress = IPAddress.Parse(ip);
            var endpoint = new IPEndPoint(ipaddress, port);
            sock.Connect(endpoint);
            callback("connect server success", TcpState.Connect);
            thread = new Thread(Receive);
            thread.IsBackground = true;
            thread.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            callback("connect failed", TcpState.Connect);
        }
    }

    private void Receive()
    {
        Recv(sock);
    }

    public override void Update()
    {
        if (prev_change)
        {
            UpdatePreview();
            prev_change = false;
        }
    }

    private void UpdatePreview()
    {
        if (texY == null)
        {
            int w = ar_image.width;
            int h = ar_image.height;
            texY = new Texture2D(w, h, TextureFormat.Alpha8, false);
            texUV = new Texture2D(w >> 1, h >> 1, TextureFormat.RG16, false);
        }
        texY.LoadRawTextureData(ar_image.y_buf);
        texUV.LoadRawTextureData(ar_image.uv_buf);
        texY.Apply();
        texUV.Apply();
        renderer.UpdateEditor(texY, texUV);
    }

    public override void Close(bool notify)
    {
        if (sock == null) return;
        try
        {
            base.Close(notify);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        sock = null;
    }
    
}