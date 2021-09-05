using DefaultNamespace;
using UnityEngine;

public class ConnectTest : MonoBehaviour
{
    private TcpClient _client;
    private TcpServer _server;
    private string text = "";

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

    private void GUIText()
    {
        text = GUI.TextField(new Rect(160, 20, 200, 60), text);
    }

    private void EditorGUI()
    {
        GUIText();
        if (GUI.Button(new Rect(20, 20, 120, 60), "Adb Forward"))
        {
            AdbForward();
        }
        if (GUI.Button(new Rect(20, 100, 120, 60), "Connect"))
        {
            _client = new TcpClient("127.0.0.1", ADBExecutor.HOST_PORT);
        }
        if (GUI.Button(new Rect(20, 180, 120, 60), "Send"))
        {
            _client?.SendMsg("client " + text);
        }
        if (GUI.Button(new Rect(20, 260, 120, 60), "Close"))
        {
            _client?.Close();
        }
    }

    private void AndroidGUI()
    {
        GUIText();
        if (GUI.Button(new Rect(20, 20, 120, 60), "Listening"))
        {
            _server = new TcpServer("127.0.0.1", ADBExecutor.ANDROID_PORT);
        }
        if (GUI.Button(new Rect(20, 100, 120, 60), "Send"))
        {
            _server?.SendMsg("server " + text);
        }
        if (GUI.Button(new Rect(20, 180, 120, 60), "Close"))
        {
            _server?.Close();
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