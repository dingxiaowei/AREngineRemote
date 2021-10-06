using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class ARSceneMeshVisulizer: BaseVisualizer<AREngineSceneMesh>
    {
        private Mesh m_mesh;
        
        public static volatile bool change;
        
        protected override TcpHead head
        {
            get { return TcpHead.SceneMesh; }
        }

        protected override void Init()
        {
            m_mesh = GetComponent<MeshFilter>().mesh;
            m_mesh.Clear();
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