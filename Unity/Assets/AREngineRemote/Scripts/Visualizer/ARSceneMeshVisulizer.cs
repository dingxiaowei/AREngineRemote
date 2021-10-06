using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class ARSceneMeshVisulizer: BaseVisualizer<AREngineSceneMesh>
    {
        private Mesh m_mesh;

        protected override TcpHead head
        {
            get { return TcpHead.SceneMesh; }
        }

        protected override void OnInitial()
        {
            m_mesh = GetComponent<MeshFilter>().mesh;
            m_mesh.Clear();
        }

        protected override void OnProcess(byte[] recvBuf, ref int offset)
        {
            int p_cnt = Bytes2Int(recvBuf, ref offset);
            ar_data.vertices = new Vector3[p_cnt];
            for (int i = 0; i < p_cnt; i++)
            {
                ar_data.vertices[i] = RecvVector3(recvBuf, ref offset);
            }
            p_cnt = Bytes2Int(recvBuf, ref offset);
            ar_data.triangles = new int[p_cnt];
            for (int i = 0; i < p_cnt; i++)
            {
                ar_data.triangles[i] = Bytes2Int(recvBuf, ref offset);
            }
        }

        protected override void OnUpdate()
        {
            if (ar_data.vertices.Length > 0)
            {
                m_mesh.Clear();
                m_mesh.vertices = ar_data.vertices;
                m_mesh.triangles = ar_data.triangles;
                m_mesh.RecalculateBounds();
                m_mesh.RecalculateNormals();
            }
        }
    }
}