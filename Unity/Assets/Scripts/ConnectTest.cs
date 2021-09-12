﻿using System;
using Common;
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
            gameObject.AddComponent<PointcloudVisualizer>();
        }
    }

    private void Update()
    {
        tcp?.Update();
        if (Input.GetKeyUp(KeyCode.Space))
            Connect();
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

    [ContextMenu("point cloud")]
    private void TestPointCloud()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] points = new Vector3[1];
        int[] indexs = new int[1];
        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                int idx = i * 8 + j;
                points[idx] = new Vector3(i - 4, j - 4, 0);
                indexs[idx] = idx;
            }
        }
        mesh.Clear();
        mesh.vertices = points;
        mesh.SetIndices(indexs, MeshTopology.Points, 0);
    }
}