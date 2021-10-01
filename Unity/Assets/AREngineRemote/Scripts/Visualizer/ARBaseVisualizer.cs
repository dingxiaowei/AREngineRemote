using UnityEngine;

public interface IVisualizer { }

namespace HuaweiAREngineRemote
{
    public abstract class BaseVisualizer<T> : MonoBehaviour, IVisualizer where T : VisualizerData
    {
        protected T ar_data;

        private Camera cam;

        protected TcpBase tcp;

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

        public void Set(TcpBase t)
        {
            tcp = t;
            Init();
        }

        protected virtual void Init() { }

        public void UpdateVisual(T data)
        {
            ar_data = data;
            OnUpdate();
        }

        protected virtual void OnUpdate() { }
    }
}