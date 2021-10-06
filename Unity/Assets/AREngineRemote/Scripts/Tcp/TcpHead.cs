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

public enum SceneState
{
    World,
    SceneMesh,
    Body,
    Face
}


[Flags]
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
    /// 预览流
    /// </summary>
    Preview = 0x2,

    /// <summary>
    /// 点云
    /// </summary>
    PointCloud = 0x3,

    /// <summary>
    /// 平面
    /// </summary>
    Plane = 0x4,

    /// <summary>
    /// scene mesh
    /// </summary>
    SceneMesh = 0x5,

    /// <summary>
    /// 放在最后
    /// </summary>
    MAX,
}