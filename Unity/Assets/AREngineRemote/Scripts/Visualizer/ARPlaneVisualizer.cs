using System.Collections.Generic;
using System.Linq;
using HuaweiARUnitySDK;
using UnityEngine;
using UnityEngine.Rendering;

namespace HuaweiAREngineRemote
{
    public class ARPlaneVisualizer : BaseVisualizer<AREnginePlane>
    {
        public static volatile bool change;
        private List<MeshFilter> filters = new List<MeshFilter>();
        private static readonly int PlaneNormal = Shader.PropertyToID("_PlaneNormal");

        protected override TcpHead head
        {
            get { return TcpHead.Plane; }
        }

        protected override void Init() { }

        protected override void OnUpdate()
        {
            int len = ar_data.planes.Length;
            int cnt = filters.Count;
            for (int i = 0; i < len; i++)
            {
                if (i >= cnt)
                {
                    var filter = AddFilter();
                    filters.Add(filter);
                }
                var p = ar_data.planes[i];
                var m3d = p.meshVertices3D;
                int count = m3d.Length;
                for (int j = 0; j < count; j++)
                {
                    p.meshVertices3D[i] = p.pose.rotation * m3d[i] + p.pose.position;
                }
                Vector3 planeNormal = p.pose.rotation * Vector3.up;
                var render = filters[i].GetComponent<Renderer>();
                render.material.SetVector(PlaneNormal, planeNormal);
                Triangulator tr = new Triangulator(p.meshVertices2D.ToList());
                var m_mesh = filters[i].mesh;
                m_mesh.SetVertices(p.meshVertices3D.ToList());
                m_mesh.SetIndices(tr.Triangulate(), MeshTopology.Triangles, 0);
                var text = filters[i].GetComponent<TextMesh>();
                UpdateLabel(p, text);
            }
        }

        private void UpdateLabel(AREngineVectices p, TextMesh textMesh)
        {
            Pose centerPose = p.pose;
            textMesh.text = p.label.ToString();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.transform.position = centerPose.position;
            textMesh.transform.rotation = centerPose.rotation;
            transform.RotateAround(centerPose.position, centerPose.rotation * Vector3.right, 90f);
            transform.RotateAround(centerPose.position, centerPose.rotation * Vector3.up, 180f);
        }

        private MeshFilter AddFilter()
        {
            var child = new GameObject("child");
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            var filter = child.AddComponent<MeshFilter>();
            var render = child.AddComponent<MeshRenderer>();
            render.receiveShadows = false;
            render.shadowCastingMode = ShadowCastingMode.Off;
            render.lightProbeUsage = LightProbeUsage.Off;
            return filter;
        }
    }
}