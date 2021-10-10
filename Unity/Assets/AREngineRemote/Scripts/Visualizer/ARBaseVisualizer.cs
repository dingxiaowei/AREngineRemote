using System;
using System.Text;
using UnityEngine;

public interface IVisualizer { }

namespace HuaweiAREngineRemote
{
    public abstract class BaseVisualizer<T> : MonoBehaviour, IVisualizer where T : VisualizerData, new()
    {
        protected T ar_data;
        private Camera cam;
        protected SceneState sceneState;
        public volatile bool change;

        protected abstract TcpHead head { get; }

        protected Camera MainCamera
        {
            get
            {
                if (cam == null)
                    cam = Camera.main;
                return cam;
            }
        }

        public void Init(SceneState st)
        {
            sceneState = st;
            ar_data = new T();
            OnInitial();
        }

        protected virtual void OnInitial() { }

        public void Update()
        {
            if (change)
            {
                OnUpdateVisual();
                change = false;
            }
        }

        public void ProcessData(byte[] recvBuf, ref int offset)
        {
            OnProcess(recvBuf, ref offset);
            change = true;
        }

        protected virtual void OnProcess(byte[] recvBuf, ref int offset) { }

        protected virtual void OnUpdateVisual() { }

        protected Quaternion RecvRot(byte[] buf, ref int offset)
        {
            float scale_point = TcpBase.scale_point;
            float x = Bytes2Int(buf, ref offset) / scale_point;
            float y = Bytes2Int(buf, ref offset) / scale_point;
            float z = Bytes2Int(buf, ref offset) / scale_point;
            float w = Bytes2Int(buf, ref offset) / scale_point;
            return new Quaternion(x, y, z, w);
        }

        public Vector3 RecvVector3(byte[] buf, ref int offset)
        {
            float scale_point = TcpBase.scale_point;
            float x = Bytes2Int(buf, ref offset) / scale_point;
            float y = Bytes2Int(buf, ref offset) / scale_point;
            float z = Bytes2Int(buf, ref offset) / scale_point;
            return new Vector3(x, y, z);
        }

        protected Vector2 RecvVector2(byte[] buf, ref int offset)
        {
            float scale_point = TcpBase.scale_point;
            float x = Bytes2Int(buf, ref offset) / scale_point;
            float y = Bytes2Int(buf, ref offset) / scale_point;
            return new Vector2(x, y);
        }

        protected string RecvString(byte[] buf, ref int offset)
        {
            int len = Bytes2Int(buf, ref offset);
            var bytes = new byte[len];
            Array.Copy(buf, offset, bytes, 0, len);
            offset += len;
            return Encoding.Default.GetString(bytes);
        }

        protected int Bytes2Int(byte[] src, ref int offset)
        {
            return TcpBase.Bytes2Int(src, ref offset);
        }
    }
}