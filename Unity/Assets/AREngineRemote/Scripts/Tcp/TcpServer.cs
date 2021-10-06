using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Common;
using HuaweiARUnitySDK;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    /// <summary>
    /// 运行在Android端
    /// </summary>
    public class TcpServer : TcpBase
    {
        private Socket socketWatch;
        private TcpState state;
        private Thread rcvThread;
        private float lastT;

        private Camera camera;

        protected Camera MainCamera
        {
            get
            {
                if (camera == null)
                    camera = Camera.main;
                return camera;
            }
        }

        public TcpServer(string ip, int port, SceneState st, Action<string, TcpState> notify) : base(st)
        {
            SetupEnv();
            state = TcpState.None;
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            callback = notify;
            var ipaddress = IPAddress.Parse(ip);
            var endpoint = new IPEndPoint(ipaddress, port);
            socketWatch.Bind(endpoint);
            //将套接字的监听队列长度限制为1
            socketWatch.Listen(1);
            callback(" begin listening", TcpState.Connect);
            //创建一个监听线程
            thread = new Thread(WatchConnecting);
            thread.IsBackground = true;
            thread.Start();
        }

        private void SetupEnv()
        {
            string prefab = "ARWorldDevice";
            if (sceneState == SceneState.Scene)
                prefab = "ARSceneDevice";
            var obj = Resources.Load<GameObject>(prefab);
            var go = GameObject.Instantiate(obj);
            if (sceneState == SceneState.World)
                go.AddComponent<PointcloudVisualizer>();
            var session = go.GetComponent<SessionComponent>();
            session.OnApplicationPause(false);
        }

        public override void Close(bool notify)
        {
            try
            {
                state = TcpState.Quit;
                base.Close(notify);
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
                    switch (sceneState)
                    {
                        case SceneState.World:
                            AcquireCloudPoint();
                            AcquirePlanes();
                            break;
                        case SceneState.Scene:
                            AcquireScene();
                            break;
                    }
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
                    callback(" connect:" + sock.RemoteEndPoint, TcpState.Connect);
                    state = TcpState.Connect;
                    rcvThread = new Thread(Receive);
                    rcvThread.IsBackground = true;
                    rcvThread.Start(sock);
                    threadRun = true;
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
                    int offset = headLen;
                    cnt = points.Count;
                    WriteInt32(cnt, ref offset);
                    for (int i = 0; i < cnt; i++)
                    {
                        WriteVector3(points[i], ref offset);
                    }
                    SendWithHead(TcpHead.PointCloud, 28 + 12 * cnt);
                }
            }
        }

        List<Vector3> meshVertices3D = new List<Vector3>();
        List<Vector2> meshVertices2D = new List<Vector2>();

        private void AcquirePlanes()
        {
            List<ARPlane> planes = new List<ARPlane>();
            ARFrame.GetTrackables(planes, ARTrackableQueryFilter.ALL);
            int cnt = planes.Count;
            int offset = headLen;
            int index = offset;
            offset += 4; // sizeof(int) = place holder
            int count = 0;
            for (int i = 0; i < cnt; i++)
            {
                var plane = planes[i];
                var st = plane.GetTrackingState();
                if (st == ARTrackable.TrackingState.TRACKING)
                {
                    count++;
                    plane.GetPlanePolygon(meshVertices3D);
                    var pose = plane.GetCenterPose();
                    plane.GetPlanePolygon(ref meshVertices2D);
                    WriteInt32(meshVertices3D.Count, ref offset);
                    WriteInt32(meshVertices2D.Count, ref offset);
                    var label = plane.GetARPlaneLabel();
                    WriteString(label.ToString(), ref offset);
                    WriteVector3(pose.position, ref offset);
                    WriteRot(pose.rotation, ref offset);
                    for (int j = 0; j < meshVertices3D.Count; j++)
                    {
                        meshVertices3D[j] = pose.rotation * meshVertices3D[j] + pose.position;
                        WriteVector3(meshVertices3D[j], ref offset);
                    }
                    for (int j = 0; j < meshVertices2D.Count; j++)
                    {
                        WriteVector2(meshVertices2D[j], ref offset);
                    }
                }
                WriteInt32(count, ref index);
            }
            if (count > 0)
            {
                SendWithHead(TcpHead.Plane, offset - headLen);
            }
        }

        private void AcquireScene()
        {
            if (ARFrame.GetTrackingState() == ARTrackable.TrackingState.TRACKING)
            {
                var sceneMesh = ARFrame.AcquireSceneMesh();
                var points = sceneMesh.Vertices;
                if (points.Length > 0)
                {
                    int offset = headLen;
                    WriteInt32(points.Length, ref offset);
                    for (int i = 0; i < points.Length; i++)
                    {
                        WriteVector3(points[i], ref offset);
                    }
                    var trigers = sceneMesh.TriangleIndices;
                    WriteInt32(trigers.Length, ref offset);
                    for (int i = 0; i < trigers.Length; i++)
                    {
                        WriteInt32(trigers[i], ref offset);
                    }
                    SendWithHead(TcpHead.SceneMesh, offset - headLen);
                }
            }
        }

        private void WriteVector2(Vector2 v, ref int offset)
        {
            int x = (int) (v.x * scale_point);
            int y = (int) (v.y * scale_point);
            WriteInt32(x, ref offset);
            WriteInt32(y, ref offset);
        }

        private void WriteVector3(Vector3 v, ref int offset)
        {
            int x = (int) (v.x * scale_point);
            int y = (int) (v.y * scale_point);
            int z = (int) (v.z * scale_point);
            WriteInt32(x, ref offset);
            WriteInt32(y, ref offset);
            WriteInt32(z, ref offset);
        }

        private void WriteRot(Quaternion v, ref int offset)
        {
            int x = (int) (v.x * scale_point);
            int y = (int) (v.y * scale_point);
            int z = (int) (v.z * scale_point);
            int w = (int) (v.w * scale_point);
            WriteInt32(x, ref offset);
            WriteInt32(y, ref offset);
            WriteInt32(z, ref offset);
            WriteInt32(w, ref offset);
        }

        private void WriteInt32(int v, ref int offset)
        {
            var bytes = Int2Bytes(v);
            Array.Copy(bytes, 0, sendBuf, offset, 4);
            offset += 4;
        }

        private void WriteString(string v, ref int offset)
        {
            var bytes = System.Text.Encoding.Default.GetBytes(v);
            int len = bytes.Length;
            WriteInt32(len, ref offset);
            Array.Copy(bytes, 0, sendBuf, offset, len);
            offset += len;
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
            int offset = headLen;
            var transform = MainCamera.transform;
            var pos = transform.position;
            var angle = transform.eulerAngles;
            WriteVector3(pos, ref offset);
            WriteVector3(angle, ref offset);
            WriteInt32(width, ref offset);
            WriteInt32(height, ref offset);
            WriteInt32(len, ref offset);
            Marshal.Copy(ptr, sendBuf, offset, len);
            SendWithHead(TcpHead.Preview, len + 12);
        }
    }
}