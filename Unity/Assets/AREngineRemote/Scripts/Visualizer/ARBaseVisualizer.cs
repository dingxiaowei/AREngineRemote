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

        public void Init()
        {
            ar_data = new T();
            OnInitial();
        }

        protected virtual void OnInitial() { }

        public void Update()
        {
            if (change)
            {
                UpdateVisual(ar_data);
                change = false;
            }
        }

        public void UpdateVisual(T data)
        {
            ar_data = data;
            OnUpdate();
        }

        public void ProcessData(byte[] recvBuf, ref int offset)
        {
            OnProcess(recvBuf, ref offset);
            change = true;
        }

        protected virtual void OnProcess(byte[] recvBuf, ref int offset) { }

        protected virtual void OnUpdate() { }

        protected Quaternion RecvRot(byte[] buf, ref int offset)
        {
            float scale_point = TcpBase.scale_point;
            float x = Bytes2Int(buf, ref offset) / (float) scale_point;
            float y = Bytes2Int(buf, ref offset) / (float) scale_point;
            float z = Bytes2Int(buf, ref offset) / (float) scale_point;
            float w = Bytes2Int(buf, ref offset) / (float) scale_point;
            return new Quaternion(x, y, z, w);
        }

        public Vector3 RecvVector3(byte[] buf, ref int offset)
        {
            float scale_point = TcpBase.scale_point;
            int x = Bytes2Int(buf, ref offset);
            int y = Bytes2Int(buf, ref offset);
            int z = Bytes2Int(buf, ref offset);
            return new Vector3(x, y, z) / scale_point;
        }

        protected Vector2 RecvVector2(byte[] buf, ref int offset)
        {
            float scale_point = TcpBase.scale_point;
            int x = Bytes2Int(buf, ref offset);
            int y = Bytes2Int(buf, ref offset);
            return new Vector2(x, y) / scale_point;
        }

        protected string RecvString(byte[] buf, ref int offset)
        {
            int len = Bytes2Int(buf, ref offset);
            var bytes = new byte[len];
            Array.Copy(buf, offset, bytes, 0, len);
            offset += len;
            return Encoding.Default.GetString(bytes);
        }

        protected int Bytes2Int(byte[] src)
        {
            return (src[0] & 0xFF) | ((src[1] & 0xFF) << 8) | ((src[2] & 0xFF) << 16) | ((src[3] & 0xFF) << 24);
        }

        protected int Bytes2Int(byte[] src, ref int offset)
        {
            int v = (src[offset] & 0xFF) | ((src[offset + 1] & 0xFF) << 8) | ((src[offset + 2] & 0xFF) << 16) |
                    ((src[offset + 3] & 0xFF) << 24);
            offset += 4;
            return v;
        }
    }
}