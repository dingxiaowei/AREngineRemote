using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpServer
{
    private Thread threadWatch;
    private Socket socketWatch;
    private Socket sockConnection;
    static readonly byte[] recvBuf = new byte[1024 * 1024];

    public TcpServer(string ip, int port)
    {
        socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                Console.WriteLine("recv address:" + sockConnection.RemoteEndPoint);
                Thread thr = new Thread(ServerRecMsg);
                thr.IsBackground = true;
                thr.Start(sockConnection);
            }
            catch (Exception ex)
            {
                Console.WriteLine("connect error：" + ex);
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
                //将接收到的信息存入到内存缓冲区, 并返回其字节数组的长度
                int length = socketServer.Receive(recvBuf);
                string strSRecMsg = Encoding.UTF8.GetString(recvBuf, 0, length);
                if (strSRecMsg.Length != 0)
                {
                    Console.WriteLine(socketServer.RemoteEndPoint + " " + DateTime.Now + ": " + strSRecMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error：" + ex);
                Console.WriteLine(socketServer.RemoteEndPoint + " disconnect");
                break;
            }
        }
    }

    public void SendMsg(string sendMsg)
    {
        try
        {
            var arrSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            sockConnection.Send(arrSendMsg);
        }
        catch (Exception ex)
        {
            Console.WriteLine("send error：" + ex);
        }
    }

    public void Close()
    {
        try
        {
            socketWatch.Close();
            sockConnection.Close();
            threadWatch.Abort();
        }
        catch (Exception e)
        {
            Console.WriteLine("close ex: " + e);
        }
        Console.WriteLine("connect close");
    }
}