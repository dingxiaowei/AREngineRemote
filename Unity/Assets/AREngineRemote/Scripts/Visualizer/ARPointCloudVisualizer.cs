using UnityEngine;

namespace HuaweiAREngineRemote
{

    public class PointCloudVisualizer : BaseVisualizer<AREnginePointCloud>
    {
        public static volatile bool change;
        private Mesh pointCloudMesh;

        protected override TcpHead head
        {
            get { return TcpHead.PointCloud; }
        }

        protected override void Init()
        {
            pointCloudMesh = GetComponent<MeshFilter>().mesh;
            pointCloudMesh.Clear();
        }

        protected override void OnUpdate()
        {
            int len = ar_data.len;
            if (len > 1)
            {
                var points = new Vector3[len];
                int[] indexs = new int[len];
                int offset = 0;
                for (int i = 0; i < len; i++)
                {
                    points[i] = tcp.RecvVector3(ar_data.buf, ref offset);
                    indexs[i] = i;
                }
                pointCloudMesh.Clear();
                pointCloudMesh.vertices = points;
                pointCloudMesh.SetIndices(indexs, MeshTopology.Points, 0);
            }
        }

    }

}