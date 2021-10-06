using System.Collections.Generic;
using System.Linq;
using HuaweiARUnitySDK;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HuaweiAREngineRemote
{
    public class ARPlaneVisualizer : BaseVisualizer<AREnginePlane>
    {
        public static volatile bool change;
        private List<MeshFilter> filters = new List<MeshFilter>();
        private static Material gridMat;
        private static readonly int PlaneNormal = Shader.PropertyToID("_PlaneNormal");

        protected override TcpHead head
        {
            get { return TcpHead.Plane; }
        }

        protected override void Init()
        {
            var p = "Assets/Examples/Materials/grid.mat";
            gridMat = AssetDatabase.LoadAssetAtPath<Material>(p);
        }

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
                int count = m3d?.Length ?? 0;
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
                var text = filters[i].transform.GetChild(0).GetComponent<TextMesh>();
                UpdateLabel(p, text);
            }
        }

        private void UpdateLabel(AREngineVectices p, TextMesh textMesh)
        {
            Pose centerPose = p.pose;
            textMesh.text = p.label;
            var tf = textMesh.transform;
            tf.position = centerPose.position;
            tf.rotation = centerPose.rotation;
            tf.RotateAround(centerPose.position, centerPose.rotation * Vector3.right, 90f);
            tf.RotateAround(centerPose.position, centerPose.rotation * Vector3.up, 180f);
        }

        private MeshFilter AddFilter()
        {
            var child = new GameObject("plane");
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            var filter = child.AddComponent<MeshFilter>();
            var render = child.AddComponent<MeshRenderer>();
            render.material = gridMat;
            render.receiveShadows = false;
            render.shadowCastingMode = ShadowCastingMode.Off;
            render.lightProbeUsage = LightProbeUsage.Off;
            render.reflectionProbeUsage = ReflectionProbeUsage.Off;
            AddTextMesh(child);
            return filter;
        }

        private void AddTextMesh(GameObject go)
        {
            var child = new GameObject("text");
            child.transform.SetParent(go.transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            var tm = child.AddComponent<TextMesh>();
            tm.offsetZ = 0;
            tm.characterSize = 0.1f;
            tm.lineSpacing = 1;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.tabSize = 1;
            tm.fontSize = 64;
            tm.fontStyle = FontStyle.Normal;
            tm.richText = true;
            tm.color = Color.magenta;
        }
    }
}