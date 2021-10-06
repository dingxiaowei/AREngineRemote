using System;
using UnityEngine;

namespace HuaweiAREngineRemote
{

    public class PointCloudVisualizer : BaseVisualizer<AREnginePointCloud>
    {
        private Mesh pointCloudMesh;
        private AREnginePointCloud ar_point = new AREnginePointCloud();

        protected override TcpHead head
        {
            get { return TcpHead.PointCloud; }
        }

        protected override void OnInitial()
        {
            pointCloudMesh = GetComponent<MeshFilter>().mesh;
            pointCloudMesh.Clear();
        }

        protected override void OnProcess(byte[] recvBuf, ref int offset)
        {
            int cnt = Bytes2Int(recvBuf, ref offset);
            ar_point.len = cnt;
            Array.Copy(recvBuf, offset, ar_point.buf, 0, 12 * cnt);
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
                    points[i] = RecvVector3(ar_data.buf, ref offset);
                    indexs[i] = i;
                }
                pointCloudMesh.Clear();
                pointCloudMesh.vertices = points;
                pointCloudMesh.SetIndices(indexs, MeshTopology.Points, 0);
            }
        }

    }

}