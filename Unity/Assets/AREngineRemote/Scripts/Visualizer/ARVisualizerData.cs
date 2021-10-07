using System.Collections.Generic;
using HuaweiARUnitySDK;
using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class VisualizerData { }

    public class AREnginePointCloud : VisualizerData
    {
        public int len;
        public byte[] buf = new byte[TcpBase.max_point * 12];
    }

    public class AREngineImage : VisualizerData
    {
        public Vector3 camPos;
        public Vector3 camAngle;
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
        public List<Vector3> meshVertices3D =new List<Vector3>();
        public List<Vector2> meshVertices2D = new List<Vector2>();
        public Pose pose = Pose.identity;
        public string label;
    }

    public class AREnginePlane : VisualizerData
    {
        public AREngineVectices[] planes;
    }

    public class AREngineSceneMesh : VisualizerData
    {
        public Vector3[] vertices;
        public int[] triangles;
    }

    public class ARHand
    {
        public Vector3[] handBox;
        public int gestureType;
        public HuaweiARUnitySDK.ARHand.HandType handType;
        public ARCoordinateSystemType coordSystem;
        public ARCoordinateSystemType skeletonSystem;

        public ARHand(int cnt)
        {
            handBox = new Vector3[cnt];
        }
    }
    
    public class ARHands : VisualizerData
    {
        public ARHand[] hands;
    }

    public class ARFace : VisualizerData
    {
        public Vector3 pos;
        public Vector3 euler;
        public Vector3[] vertices;
        public Vector2[] uv;
        public int[] triangles;
    }
}