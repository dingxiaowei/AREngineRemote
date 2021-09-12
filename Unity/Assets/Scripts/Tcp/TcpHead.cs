using System;


[Flags]
public enum TcpState
{
    /// <summary>
    /// 初始状态
    /// </summary>
    None = 0,

    /// <summary>
    /// 建立连接
    /// </summary>
    Connect = 1,

    /// <summary>
    /// 发送消息
    /// </summary>
    Send = 2,

    /// <summary>
    /// 接收消息
    /// </summary>
    Receive = 4,

    /// <summary>
    /// 退出连接
    /// </summary>
    Quit = 8
}


public enum TcpHead
{
    /// <summary>
    /// 发送字符串
    /// </summary>
    String = 0x0,

    /// <summary>
    /// 发送断开连接
    /// </summary>
    Quit = 0x1,

    /// <summary>
    /// 发送预览流
    /// </summary>
    Preview = 0x2,
}