using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpServer : ITcp
{
    private Thread threadWatch;
    private Socket socketWatch;
    private Socket sockConnection;
    private Action<string, TcpState> callback;
    static readonly byte[] recvBuf = new byte[1024 * 1024];

    public TcpServer(string ip, int port, Action<string, TcpState> notify)
    {
        socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        callback = notify;
        IPAddress ipaddress = IPAddress.Parse(ip);
        IPEndPoint endpoint = new IPEndPoint(ipaddress, port);
        socketWatch.Bind(endpoint);
        //将套接字的监听队列长度限制为1
        socketWatch.Listen(1);
        //创建一个监听线程
        threadWatch = new Thread(WatchConnecting);
        threadWatch.IsBackground = true;
        threadWatch.Start();
    }

    private void WatchConnecting()
    {
        while (true)
        {
            try
            {
                sockConnection = socketWatch.Accept();
                callback("connect:" + sockConnection.RemoteEndPoint, TcpState.Connect);
                Thread thr = new Thread(ServerRecMsg);
                thr.IsBackground = true;
                thr.Start(sockConnection);
            }
            catch (Exception ex)
            {
                Debug.LogError("connect error：" + ex);
            }
        }
    }

    private void ServerRecMsg(object socketClientPara)
    {
        Socket socketServer = socketClientPara as Socket;
        while (true)
        {
            try
            {
                int length = socketServer.Receive(recvBuf);
                string strSRecMsg = Encoding.UTF8.GetString(recvBuf, 0, length);
                if (strSRecMsg.Length != 0)
                {
                    Debug.Log(socketServer.RemoteEndPoint + " " + DateTime.Now + ": " + strSRecMsg);
                    callback(strSRecMsg, TcpState.Recv);
                    if (strSRecMsg == "\\q")
                    {
                        Close(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(socketServer.RemoteEndPoint + " disconnect " + ex);
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
                callback("send " + sendMsg, TcpState.Send);
                var arrSendMsg = Encoding.UTF8.GetBytes(sendMsg);
                sockConnection.Send(arrSendMsg);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("send error：" + ex);
        }
    }

    public void Close(bool notify)
    {
        try
        {
            if (notify) SendMsg("\\q");
            socketWatch.Close();
            sockConnection.Close();
            threadWatch.Abort();
        }
        catch (Exception e)
        {
            Debug.LogError("close ex: " + e);
        }
        Debug.Log("connect close");
    }
}