using System;
using System.Threading;

class Program
{
    private static TcpServer _server;

    static void Main(string[] args)
    {
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString());
        _server = new TcpServer("127.0.0.1", 30000);
        var key = Console.ReadKey().KeyChar;
        ProcessKey(key);
    }

    static void ProcessKey(char key)
    {
        Console.WriteLine("\ncommand: " + key);
        switch (key)
        {
            case '0':
            case 'q':
                _server.Close();
                break;
            case 'i':
            {
                var tx = Console.ReadLine();
                if (!string.IsNullOrEmpty(tx))
                    _server.SendMsg(tx);
                key = Console.ReadKey().KeyChar;
                ProcessKey(key);
                break;
            }
        }
    }
}