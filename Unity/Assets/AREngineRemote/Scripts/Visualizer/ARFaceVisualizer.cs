using UnityEngine;

namespace HuaweiAREngineRemote
{
    public class ARFaceVisualizer : BaseVisualizer<ARFace>
    {
        protected override TcpHead head
        {
            get { return TcpHead.Face; }
        }
        
        private Mesh faceMesh;

        protected override void OnInitial()
        {
            faceMesh = GetComponent<MeshFilter>().mesh;
            faceMesh.Clear();
        }

        protected override void OnProcess(byte[] recvBuf, ref int offset)
        {
            ar_data.pos = RecvVector3(recvBuf, ref offset);
            ar_data.euler = RecvVector3(recvBuf, ref offset);
            int len = Bytes2Int(recvBuf, ref offset);
            ar_data.vertices = new Vector3[len];
            for (int i = 0; i < len; i++)
            {
                ar_data.vertices[i] = RecvVector3(recvBuf, ref offset);
            }
            len = Bytes2Int(recvBuf, ref offset);
            ar_data.uv = new Vector2[len];
            for (int i = 0; i < len; i++)
            {
                ar_data.uv[i] = RecvVector2(recvBuf, ref offset);
            }
            len = Bytes2Int(recvBuf, ref offset);
            ar_data.triangles = new int[len];
            for (int i = 0; i < len; i++)
            {
                ar_data.triangles[i] = Bytes2Int(recvBuf, ref offset);
            }
        }

        protected override void OnUpdateVisual()
        {
            if (ar_data?.vertices != null)
            {
                transform.position = ar_data.pos;
                transform.rotation = Quaternion.Euler(ar_data.euler);
                faceMesh.vertices = ar_data.vertices;
                faceMesh.uv = ar_data.uv;
                faceMesh.triangles = ar_data.triangles;
                faceMesh.RecalculateBounds();
                faceMesh.RecalculateNormals();
            }
        }
    }
}