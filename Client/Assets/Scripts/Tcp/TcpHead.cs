public enum TcpState
{
    Send,
    Recv,
    Quit,
    Connect
}


public enum TcpHead
{
    String = 0x0, // 
    Quit = 0x1,   //
    Preview= 0x2, //
}