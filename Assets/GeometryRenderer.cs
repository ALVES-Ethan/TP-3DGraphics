using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GeometryRenderer : MonoBehaviour
{
    enum Mode
    {
        None,
        ColoredTriangle,
        MonochromeElipse
    }

    [SerializeField] bool debugging = true;

    [Space]

    [Header("Realtime Params")]
    [SerializeField] Mode mode = Mode.ColoredTriangle;
    [SerializeField] bool spin = true;
    [SerializeField] Vector3 axis = Vector3.forward;

    Mesh mesh;
    Material material;

    float yaw, pitch, roll;
    Matrix4x4 matrix;
    Mode previous;

    void ConfigureMaterial(string shader)
    {

        material = new Material(Shader.Find(shader));
        material.color = Color.white;
    }
    void ConfigureMesh(Vector3[] vertices, int[] indices, Color[] colors = null)
    {
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        if (colors != null)
            mesh.colors = colors;
    }

    void ConfigureMode_None()
    {
        mesh = null;
        material = null;
    }
    void ConfigureMode_ColoredTriangle()
    {
        float scale = 8.0f;
        ConfigureMaterial("Unlit/VertColor");
        ConfigureMesh(
            // vertices
            new Vector3[] {
                new Vector3(-0.5f, 0.0f, 0.0f) * scale,
                new Vector3(0.0f, 0.5f, 0.0f) * scale,
                new Vector3(0.5f, 0.0f, 0.0f) * scale,
            },
            // indices
            new int[] {
                0, 1, 2
            },
            // colors
            new Color[] {
                new Color(1.0f, 0.0f, 0.0f),
                new Color(0.0f, 1.0f, 0.0f),
                new Color(0.0f, 0.0f, 1.0f)
            }
        );
    }
    void ConfigureMode_MonochromeElipse()
    {
        ConfigureMaterial("Unlit/Color");
        KeyValuePair<List<int>, List<Vector3>> circle = GenerateCircle(Vector3.zero, 2.0f, 196);
        ConfigureMesh(
            circle.Value.ToArray(),
            circle.Key.ToArray()
        );
    }

    void ConfigureMode()
    {
        if (mode == previous) return;

        switch (mode)
        {
            case Mode.None:
                ConfigureMode_None();
                break;
            case Mode.ColoredTriangle:
                ConfigureMode_ColoredTriangle();
                break;
            case Mode.MonochromeElipse:
                ConfigureMode_MonochromeElipse();
                break;
            default:
                break;
        }
        previous = mode;
    }

    void Awake()
    {
        yaw = 0; pitch = 0; roll = 0;
        matrix = Matrix4x4.identity;
        ConfigureMode();
    }

    public KeyValuePair<List<int>, List<Vector3>> GenerateCircle(Vector3 center, float radius, int resolution)
    {
        float pas = 360.0f / (float)resolution;

        List<Vector3> verts = new List<Vector3>();

        verts.Add(center);

        for (int i = 0; i <= resolution; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * (pas * i)) * radius;
            float y = Mathf.Sin(Mathf.Deg2Rad * (pas * i)) * radius;
            float z = 0;

            verts.Add(new Vector3(x, y, z));
        }

        List<int> indices = new List<int>();

        int lastIndex = 1;
        for (int i = 0; i < resolution; i++)
        {
            indices.Add(0);
            indices.Add(lastIndex);
            indices.Add(lastIndex + 1);
            lastIndex += 1;
        }

        indices.Reverse();

        KeyValuePair<List<int>, List<Vector3>> pairs = new KeyValuePair<List<int>, List<Vector3>>(indices, verts);

        return pairs;
    }

    void Update()
    {
        ConfigureMode();

        if (!spin) return;

        float scaledDeltaTime = Time.deltaTime * 64;

        pitch += scaledDeltaTime * axis.x;
        yaw += scaledDeltaTime * axis.y;
        roll += scaledDeltaTime * axis.z;

        matrix = Matrix4x4.Rotate(Quaternion.Euler(pitch, yaw, roll));
    }

    void OnPostRender()
    {
        material.SetPass(0);
        Graphics.DrawMeshNow(mesh, matrix);
    }

    void OnDrawGizmos()
    {
        if (!debugging) return;
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;

        foreach (Vector3 vertex in mesh.vertices)
        {
            Gizmos.DrawSphere(vertex, 0.05f);
        }
    }
}