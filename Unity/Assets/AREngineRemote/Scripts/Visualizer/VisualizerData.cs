using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class VisualizerData { }

    public class AREnginePointCloud : VisualizerData
    {
        public int len;
        public Vector3 camPos;
        public Vector3 camAngle;
        public byte[] buf = new byte[TcpBase.max_point * 12];
    }

    public class AREngineImage : VisualizerData
    {
        public int width, height;
        public byte[] y_buf, uv_buf;

        public void Set(int w, int h)
        {
            width = w;
            height = h;
        }
    }

    public class AREngineVectices
    {
        public Vector3[] meshVertices3D;
        public Vector2[] meshVertices2D;
        public Pose pose = Pose.identity;
    }

    public class AREnginePlane : VisualizerData
    {
        public AREngineVectices[] planes;
    }
}