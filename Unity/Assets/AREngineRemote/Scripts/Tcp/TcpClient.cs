using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private ARHandVisualizer _arHandVisualizer;
        private ARFaceVisualizer _arFaceVisualizer;

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
            _previewVisualizer.Init();
            if (sceneState == SceneState.World)
            {
                var ob = Resources.Load<GameObject>("PointCloudVisualizer");
                obj = GameObject.Instantiate(ob);
                obj.name = "PointCloudVisualizer";
                _pointCloudVisualizer = obj.AddComponent<PointCloudVisualizer>();
                _pointCloudVisualizer.Init();
                ob = Resources.Load<GameObject>("PlaneVisualizer");
                obj = GameObject.Instantiate(ob);
                obj.name = "PlaneVisualizer";
                _arPlaneVisualizer = obj.AddComponent<ARPlaneVisualizer>();
                _arPlaneVisualizer.Init();
            }
            if (sceneState == SceneState.Scene)
            {
                var ob = Resources.Load<GameObject>("SceneMeshVisulizer");
                obj = GameObject.Instantiate(ob);
                obj.name = "ARSceneMeshVisulizer";
                _arSceneVisualizer = obj.AddComponent<ARSceneMeshVisulizer>();
                _arSceneVisualizer.Init();
            }
            if (sceneState == SceneState.Hand)
            {
                obj = new GameObject("HandVisualizer");
                _arHandVisualizer = obj.AddComponent<ARHandVisualizer>();
                _arHandVisualizer.Init();
            }
            if (sceneState == SceneState.Face)
            {
                var ob = Resources.Load<GameObject>("FaceVisualizer");
                obj = GameObject.Instantiate(ob);
                obj.name = "FaceVisualizer";
                _arFaceVisualizer = obj.AddComponent<ARFaceVisualizer>();
                _arFaceVisualizer.Init();
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

        protected override void Process(int length)
        {
            int offset = headLen;
            var head = (TcpHead) recvBuf[4];
            switch (head)
            {
                case TcpHead.Preview:
                    _previewVisualizer.ProcessData(recvBuf, ref offset);
                    break;
                case TcpHead.PointCloud:
                    _pointCloudVisualizer.ProcessData(recvBuf, ref offset);
                    break;
                case TcpHead.Plane:
                    _arPlaneVisualizer.ProcessData(recvBuf, ref offset);
                    break;
                case TcpHead.SceneMesh:
                    _arSceneVisualizer.ProcessData(recvBuf, ref offset);
                    break;
                case TcpHead.Hand:
                    _arHandVisualizer.ProcessData(recvBuf, ref offset);
                    break;
                case TcpHead.Face:
                    _arFaceVisualizer.ProcessData(recvBuf, ref offset);
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

        public override void Update()
        {
            _previewVisualizer.Update();
            switch (sceneState)
            {
                case SceneState.World:
                    _pointCloudVisualizer.Update();
                    _arPlaneVisualizer.Update();
                    break;
                case SceneState.Scene:
                    _arSceneVisualizer.Update();
                    break;
                case SceneState.Hand:
                    _arHandVisualizer.Update();
                    break;
                case SceneState.Face:
                    _arFaceVisualizer.Update();
                    break;
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