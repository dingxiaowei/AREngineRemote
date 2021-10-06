using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    /// <summary>
    /// 运行在 Editor 中
    /// </summary>
    public class TcpClient : TcpBase
    {
        private PreviewStreamVisualizer _previewVisualizer;
        private PointCloudVisualizer _pointCloudVisualizer;
        private ARPlaneVisualizer _arPlaneVisualizer;
        private ARSceneMeshVisulizer _arSceneVisualizer;

        public TcpClient(string ip, int port, SceneState st, Action<string, TcpState> notify) : base(st)
        {
            try
            {
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

        private void InitVisualizer()
        {
            var obj = Resources.Load<GameObject>("ClientARDevice");
            GameObject.Instantiate(obj);
            obj = new GameObject("PreviewStreamVisualizer");
            _previewVisualizer = obj.AddComponent<PreviewStreamVisualizer>();
            _previewVisualizer.Set(this);
            if (sceneState == SceneState.World)
            {
                var ob = Resources.Load<GameObject>("PointCloudVisualizer");
                obj = GameObject.Instantiate(ob);
                obj.name = "PointCloudVisualizer";
                _pointCloudVisualizer = obj.AddComponent<PointCloudVisualizer>();
                _pointCloudVisualizer.Set(this);
                
                ob = Resources.Load<GameObject>("PlaneVisualizer");
                obj = GameObject.Instantiate(ob);
                obj.name = "PlaneVisualizer";
                _arPlaneVisualizer = obj.AddComponent<ARPlaneVisualizer>();
                _arPlaneVisualizer.Set(this);
            }
            if (sceneState == SceneState.Scene)
            {
                var ob = Resources.Load<GameObject>("SceneMeshVisulizer");
                obj = GameObject.Instantiate(ob);
                obj.name = "ARSceneMeshVisulizer";
                _arSceneVisualizer = obj.AddComponent<ARSceneMeshVisulizer>();
                _arSceneVisualizer.Set(this);
            }
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
                _previewVisualizer.UpdateVisual(ar_image);
                PreviewStreamVisualizer.change = false;
            }
            if (PointCloudVisualizer.change)
            {
                _pointCloudVisualizer.UpdateVisual(ar_point);
                PointCloudVisualizer.change = false;
            }
            if (ARPlaneVisualizer.change)
            {
                _arPlaneVisualizer.UpdateVisual(ar_plane);
                ARPlaneVisualizer.change = true;
            }
            if (ARSceneMeshVisulizer.change)
            {
                _arSceneVisualizer.UpdateVisual(ar_mesh);
                ARSceneMeshVisulizer.change = true;
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