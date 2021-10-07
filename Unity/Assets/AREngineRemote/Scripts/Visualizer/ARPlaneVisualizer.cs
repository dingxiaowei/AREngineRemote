using System.Collections.Generic;
using HuaweiARUnitySDK;
using UnityEngine;
using UnityEngine.Rendering;

namespace HuaweiAREngineRemote
{
    public class ARPlaneVisualizer : BaseVisualizer<AREnginePlane>
    {
        private static readonly int PlaneNormal = Shader.PropertyToID("_PlaneNormal");
        private List<MeshFilter> filters = new List<MeshFilter>();
        private Material gridMat;

        protected override TcpHead head
        {
            get { return TcpHead.Plane; }
        }

        protected override void OnInitial()
        {
#if UNITY_EDITOR
            var p = "Assets/Examples/Common/Materials/grid.mat";
            gridMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(p);      
#endif
        }

        protected override void OnProcess(byte[] recvBuf, ref int offset)
        {
            int count = Bytes2Int(recvBuf, ref offset);
            ar_data.planes = new AREngineVectices[count];
            for (int i = 0; i < count; i++)
            {
                var p = new AREngineVectices();
                ar_data.planes[i] = p;
                int cnt1 = Bytes2Int(recvBuf, ref offset);
                int cnt2 = Bytes2Int(recvBuf, ref offset);
                p.label = RecvString(recvBuf, ref offset);
                var pos = RecvVector3(recvBuf, ref offset);
                var rot = RecvRot(recvBuf, ref offset);
                p.pose = new Pose(pos, rot);
                p.meshVertices3D.Clear();
                for (int j = 0; j < cnt1; j++)
                {
                    var v = RecvVector3(recvBuf, ref offset);
                    p.meshVertices3D.Add(v);
                }
                p.meshVertices2D.Clear();
                for (int j = 0; j < cnt2; j++)
                {
                    var v = RecvVector2(recvBuf, ref offset);
                    p.meshVertices2D.Add(v);
                }
            }
        }

        protected override void OnUpdateVisual()
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
                // Debug.Log("plane 3d: " + p.meshVertices3D.Count + " 2d: " + p.meshVertices2D.Count);
                Vector3 planeNormal = p.pose.rotation * Vector3.up;
                var render = filters[i].GetComponent<Renderer>();
                render.material.SetVector(PlaneNormal, planeNormal);
                Triangulator tr = new Triangulator(p.meshVertices2D);
                Mesh mesh = filters[i].mesh;
                mesh.Clear();
                mesh.SetVertices(p.meshVertices3D);
                mesh.SetIndices(tr.Triangulate(), MeshTopology.Triangles, 0);
                var text = render.transform.GetChild(0).GetComponent<TextMesh>();
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
            child.transform.localScale = Vector3.one * 0.1f;
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