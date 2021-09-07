using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DefaultNamespace;
using UnityEngine;

public class TcpClient:ITcp
{
    private Socket socketClient;
    private Thread threadClient;
    private Action<string, TcpState> callback;
    private static readonly byte[] m_oRecvBuff = new byte[1024 * 1024];
    private AddressFamily m_NetworkType = AddressFamily.InterNetwork;

    public TcpClient(string ip, int port, Action<string, TcpState> notify)
    {
        GetNetworkType(ip);
        try
        {
            Debug.Log("start tid: " + Thread.CurrentThread.ManagedThreadId);
            socketClient = new Socket(m_NetworkType, SocketType.Stream, ProtocolType.Tcp);
            socketClient.NoDelay = true;
            callback = notify;
            Connect(ip, port);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private void GetNetworkType(string ip)
    {
        try
        {
            IPAddress[] address = Dns.GetHostAddresses(ip);
            m_NetworkType = address[0].AddressFamily;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private void Connect(string ip, int port)
    {
        try
        {
            var ipaddress = IPAddress.Parse(ip);
            var endpoint = new IPEndPoint(ipaddress, port);
            socketClient.Connect(endpoint);
            threadClient = new Thread(RecvMsg);
            threadClient.IsBackground = true;
            threadClient.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private void RecvMsg()
    {
        while (true)
        {
            try
            {
                int length = socketClient.Receive(m_oRecvBuff);
                string strRecMsg = Encoding.UTF8.GetString(m_oRecvBuff, 0, length);
                Debug.Log(socketClient.RemoteEndPoint + " " + DateTime.Now + "\n" + strRecMsg);
                callback(strRecMsg, TcpState.Recv);
                if (strRecMsg == "\\q")
                {
                    callback("connect quit", TcpState.Quit);
                    Close(false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("server disconnect！" + ex);
                break;
            }
        }
    }

    public void SendMsg(string sendMsg)
    {
        try
        {
            if (!string.IsNullOrEmpty(sendMsg))
            {
                byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
                socketClient.Send(arrClientSendMsg);
                Debug.Log(DateTime.Now + " send: " + sendMsg);
                callback(sendMsg, TcpState.Send);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("server disconnect！：" + ex);
        }
    }

    public void Close(bool notify)
    {
        if (socketClient == null) return;
        try
        {
            if (notify)
            {
                SendMsg("\\q");
            }
            socketClient.Close();
            threadClient.Abort();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        socketClient = null;
    }
}