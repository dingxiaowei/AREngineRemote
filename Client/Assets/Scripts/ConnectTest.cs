using System;
using DefaultNamespace;
using UnityEngine;

public class ConnectTest : MonoBehaviour
{
    private ITcp tcp;
    private string text = "";
    private string content = "";

    private void OnGUI()
    {
        if (Application.isEditor)
        {
            EditorGUI();
        }
        else
        {
            AndroidGUI();
        }
    }

    private void OnDestroy()
    {
        tcp?.Close(true);
    }

    private void GUIText()
    {
        text = GUI.TextField(new Rect(180, 46, 420, 40), text);
        GUI.Label(new Rect(180, 100, 420, 320), content);
    }

    private void EditorGUI()
    {
        GUIText();
        if (GUI.Button(new Rect(40, 40, 120, 60), "AdbForward"))
        {
            AdbForward();
        }
        if (GUI.Button(new Rect(40, 120, 120, 60), "Connect"))
        {
            tcp = new TcpClient("127.0.0.1", ADBExecutor.HOST_PORT, OnRecvMsg);
        }
        if (GUI.Button(new Rect(40, 200, 120, 60), "Send"))
        {
            tcp?.SendMsg(text);
            text = string.Empty;
        }
        if (GUI.Button(new Rect(40, 280, 120, 60), "Close"))
        {
            tcp?.Close(true);
        }
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
                case TcpState.Recv:
                    pref = " recv: ";
                    break;
            }
            content += DateTime.Now.ToString("t") + pref + msg + "\n";
        }
    }

    private void AndroidGUI()
    {
        GUIText();
        if (GUI.Button(new Rect(40, 40, 120, 60), "Listening"))
        {
            tcp = new TcpServer("127.0.0.1", ADBExecutor.ANDROID_PORT, OnRecvMsg);
        }
        if (GUI.Button(new Rect(40, 120, 120, 60), "Send"))
        {
            tcp?.SendMsg("server " + text);
            text = string.Empty;
        }
        if (GUI.Button(new Rect(40, 200, 120, 60), "Close"))
        {
            tcp?.Close(true);
        }
    }

    [ContextMenu("adb devices")]
    private void AdbForward()
    {
        var executor = new ADBExecutor();
        var device = executor.AdbDevice();
        Debug.Log(device);
        executor.AdbSingleDevicePortForward(device);
    }
}