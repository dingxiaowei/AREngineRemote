using System.Collections.Generic;
using HuaweiARUnitySDK;
using UnityEngine;

public class PlaneVisualizer : MonoBehaviour
{
    private Mesh m_mesh;
    private MeshRenderer m_meshRenderer;
    private List<Vector3> m_previousFrameMeshVertices = new List<Vector3>();
    private List<Vector3> m_meshVertices3D = new List<Vector3>();
    private List<Vector2> m_meshVertices2D = new List<Vector2>();
    private List<Color> m_meshColors = new List<Color>();
    private Pose centerPose = Pose.identity;
    private static int s_planeCount;

    private readonly Color[] k_planeColors =
    {
        new Color(1.0f, 1.0f, 1.0f), new Color(0.5f, 0.3f, 0.9f), new Color(0.8f, 0.4f, 0.8f),
        new Color(0.5f, 0.8f, 0.4f), new Color(0.5f, 0.9f, 0.8f)
    };

    private static readonly int PlaneNormal = Shader.PropertyToID("_PlaneNormal");
    private static readonly int GridColor = Shader.PropertyToID("_GridColor");

    void Start()
    {
        m_mesh = GetComponent<MeshFilter>().mesh;
        m_meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(AREngineVectices plane)
    {
        m_meshVertices2D.Clear();
        m_meshVertices3D.Clear();
        m_meshVertices3D.AddRange(plane.meshVertices3D);
        m_meshVertices2D.AddRange(plane.meshVertices2D);
        centerPose = plane.pose;
        m_meshRenderer.material.SetColor(GridColor, k_planeColors[s_planeCount++ % k_planeColors.Length]);
        Update();
    }

    public void Update()
    {
        if (AreVerticesListsEqual(m_previousFrameMeshVertices, m_meshVertices3D))
        {
            return;
        }
        for (int i = 0; i < m_meshVertices3D.Count; i++)
        {
            m_meshVertices3D[i] = centerPose.rotation * m_meshVertices3D[i] + centerPose.position;
        }
        Vector3 planeNormal = centerPose.rotation * Vector3.up;
        m_meshRenderer.material.SetVector(PlaneNormal, planeNormal);
        m_previousFrameMeshVertices.Clear();
        m_previousFrameMeshVertices.AddRange(m_meshVertices3D);
        Triangulator tr = new Triangulator(m_meshVertices2D);
        m_mesh.Clear();
        m_mesh.SetVertices(m_meshVertices3D);
        m_mesh.SetIndices(tr.Triangulate(), MeshTopology.Triangles, 0);
        m_mesh.SetColors(m_meshColors);
    }

    private bool AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
    {
        if (firstList.Count != secondList.Count)
        {
            return false;
        }
        for (int i = 0; i < firstList.Count; i++)
        {
            if (firstList[i] != secondList[i])
            {
                return false;
            }
        }
        return true;
    }
}