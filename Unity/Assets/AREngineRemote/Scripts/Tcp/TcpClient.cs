﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    /// <summary>
    /// 运行在Editor中
    /// </summary>
    public class TcpClient : TcpBase
    {
        private PreviewStreamVisualizer previewVisualizer;
        private PointCloudVisualizer pointCloudVisualizer;
        private PlaneVisualizer planeVisualizer;

        public TcpClient(string ip, int port, Action<string, TcpState> notify)
        {
            try
            {
                SetupEnv();
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.NoDelay = true;
                callback = notify;
                InitVisualizer();
                Connect(ip, port);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }


        private void SetupEnv()
        {
            var obj = Resources.Load<GameObject>("ClientARDevice");
            GameObject.Instantiate(obj);
        }

        private void InitVisualizer()
        {
            var obj = new GameObject("previewer");
            previewVisualizer = obj.AddComponent<PreviewStreamVisualizer>();
            previewVisualizer.Set(this);

            var ob = Resources.Load<GameObject>("PointCloudVisualizer");
            obj = GameObject.Instantiate(ob);
            obj.name = "PointCloudVisualizer";
            pointCloudVisualizer = obj.AddComponent<PointCloudVisualizer>();
            pointCloudVisualizer.Set(this);

            ob = Resources.Load<GameObject>("PlaneVisualizer");
            obj = GameObject.Instantiate(ob);
            obj.name = "PlaneVisualizer";
            planeVisualizer = obj.AddComponent<PlaneVisualizer>();
            planeVisualizer.Set(this);
        }

        private void Connect(string ip, int port)
        {
            try
            {
                var ipaddress = IPAddress.Parse(ip);
                var endpoint = new IPEndPoint(ipaddress, port);
                sock.Connect(endpoint);
                callback(" connect server success", TcpState.Connect);
                thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                callback(" connect failed", TcpState.Connect);
            }
        }

        private void Receive()
        {
            Recv(sock);
        }

        public override void Update()
        {
            if (PreviewStreamVisualizer.change)
            {
                previewVisualizer.UpdateVisual(ar_image);
                PreviewStreamVisualizer.change = false;
            }
            if (PointCloudVisualizer.change)
            {
                pointCloudVisualizer.UpdateVisual(ar_point);
                PointCloudVisualizer.change = false;
            }
            if (PlaneVisualizer.change)
            {
                planeVisualizer.UpdateVisual(ar_plane);
                PlaneVisualizer.change = true;
            }
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
}