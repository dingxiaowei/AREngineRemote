using System;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class AREngineRemote : MonoBehaviour
    {
        private TcpBase tcp;
        [SerializeField]
        private SceneState state;
        public const int ANDROID_PORT = 35001;
        public const int HOST_PORT = 35002;

        private void Start()
        {
#if UNITY_EDITOR
            AdbForward();
#else
            if (tcp == null)
            {
                tcp = new TcpServer("127.0.0.1", ANDROID_PORT, state, OnRecvMsg);
            }
#endif
        }

        private void Update()
        {
            tcp?.Update();
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.Space))
                Connect();
#endif
        }

        private void OnRecvMsg(string msg, TcpState st)
        {
            if (!string.IsNullOrEmpty(msg.Trim()))
            {
                var pref = "";
                switch (st)
                {
                    case TcpState.Send:
                        pref = " send: ";
                        break;
                    case TcpState.Receive:
                        pref = " recv: ";
                        break;
                }
                Debug.Log(DateTime.Now.ToString("t") + pref + msg);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("adb forward")]
        private void AdbForward()
        {
            var executor = new ADBExecutor();
            var device = executor.AdbDevice(); 
            executor.AdbSingleDevicePortForward(device);
        }

        [ContextMenu("connect")]
        private void Connect()
        {
            tcp = new TcpClient("127.0.0.1", HOST_PORT, state, OnRecvMsg);
        }

        [ContextMenu("close")]
        private void OnDestroy()
        {
            tcp?.Close(true);
        }
#endif
    }
}