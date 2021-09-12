using System;
using DefaultNamespace;
using UnityEngine;

public class ConnectTest : MonoBehaviour
{
    private TcpBase tcp;

    private void Start()
    {
        if (!Application.isEditor && tcp == null)
        {
            tcp = new TcpServer("127.0.0.1", ADBExecutor.ANDROID_PORT, OnRecvMsg);
        }
    }

    private void Update()
    {
        tcp?.Update();
    }

    private void OnRecvMsg(string msg, TcpState state)
    {
        if (!string.IsNullOrEmpty(msg.Trim()))
        {
            var pref = "";
            switch (state)
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
        tcp = new TcpClient("127.0.0.1", ADBExecutor.HOST_PORT, OnRecvMsg);
    }

    [ContextMenu("ping")]
    private void Ping()
    {
        tcp?.Send("ping...");
    }
    
    
    [ContextMenu("close")]
    private void OnDestroy()
    {
        tcp?.Close(true);
    }
}