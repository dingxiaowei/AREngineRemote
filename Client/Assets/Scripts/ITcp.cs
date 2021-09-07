using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITcp
{
    void SendMsg(string sendMsg);

    void Close(bool notify);
}


public enum TcpState
{
    Send,
    Recv,
    Quit,
    Connect
}