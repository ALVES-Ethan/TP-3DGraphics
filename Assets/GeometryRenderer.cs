using System.Collections.Generic;
using UnityEngine;

public class GeometryRenderer : MonoBehaviour
{
    public enum Mode
    {
        None,
        Triangle,
        Circle
    }
    [SerializeField] bool regenerate;

    [SerializeField] bool debug = false;
    [SerializeField] public Mode mode = Mode.None;

    [SerializeField] bool rotate = false;
    [SerializeField] Vector3 axis = Vector3.forward;

    [Range(8, 256)]
    [SerializeField] int resolution = 16;

    Mesh mesh;
    Material material;

    float yaw, pitch, roll;
    Matrix4x4 matrix;

    Mode previous_mode;

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
        ConfigureMaterial("Unlit/VertColor");
        CircleData circle = GenerateCircle(Vector3.zero, 2.0f, resolution);
        ConfigureMesh(
            circle.positions.ToArray(),
            circle.indices.ToArray(),
            circle.colors.ToArray()
        );
    }

    void ConfigureMode()
    {
        if (mode == previous_mode && !regenerate) return;

        switch (mode)
        {
            case Mode.None:
                ConfigureMode_None();
                break;
            case Mode.Triangle:
                ConfigureMode_ColoredTriangle();
                break;
            case Mode.Circle:
                ConfigureMode_MonochromeElipse();
                break;
            default:
                break;
        }
        previous_mode = mode;
        regenerate = false;
    }

    void Awake()
    {
        yaw = 0; pitch = 0; roll = 0;
        matrix = Matrix4x4.identity;
        ConfigureMode();
    }

    public struct CircleData
    {
        public List<int> indices;
        public List<Vector3> positions;
        public List<Color> colors;

        public CircleData(List<int> indices, List<Vector3> positions, List<Color> colors)
        {
            this.indices = indices;
            this.positions = positions;
            this.colors = colors;
        }
    }

    Color GenerateRandomColor()
    {
        Color color;
        color.r = Random.Range(0, 255) / 255.0f;
        color.g = Random.Range(0, 255) / 255.0f;
        color.b = Random.Range(0, 255) / 255.0f;
        color.a = 1.0f;

        return color;
    }

    public CircleData GenerateCircle(Vector3 center, float radius, int resolution)
    {
        float pas = 360.0f / (float)resolution;

        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(center);
        for (int i = 0; i <= resolution; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * (pas * i)) * radius;
            float y = Mathf.Sin(Mathf.Deg2Rad * (pas * i)) * radius;
            float z = 0;

            vertices.Add(new Vector3(x, y, z));
        }

        List<int> indices = new List<int>();
        for (int i = 1; i < resolution + 1; i++)
        {
            indices.Add(i + 1);
            indices.Add(i);
            indices.Add(0);
        }
        
        List<Color> colors = new List<Color>();
        //int count = 0;
        //for (int i = 0; i < resolution; i+=count)
        //{
        //    Color color = GenerateRandomColor();
        //    count = (i == 0) ? 4 : 3;

        //    for (int j = 0; j < count; j++)
        //    {
        //        colors.Add(color);
        //    }
        //}

        int index = 0;
        while(index <= resolution)
        {
            Color color = GenerateRandomColor();

            int count = (index == 0) ? 4 : 3;

            for (int i = 0; i < count; i++)
            {
                colors.Add(color);
            }

            index += count;
        }

        Debug.Log($"Verts : {vertices.Count}");
        Debug.Log($"Colors : {colors.Count}");

        return new CircleData(indices, vertices, colors);
    }

    void Update()
    {
        ConfigureMode();

        if (!rotate) return;

        float scaledDeltaTime = Time.deltaTime * 64;

        pitch += scaledDeltaTime * axis.x;
        yaw += scaledDeltaTime * axis.y;
        roll += scaledDeltaTime * axis.z;

        matrix = Matrix4x4.Rotate(Quaternion.Euler(pitch, yaw, roll));
    }

    void OnPostRender()
    {
        if(mesh == null || material == null) return;
        material.SetPass(0);
        Graphics.DrawMeshNow(mesh, matrix);
    }

    void OnDrawGizmos()
    {
        if (mesh == null || material == null) return;

        if (!debug) return;
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;

        Vector3[] array = new Vector3[mesh.vertices.Length];

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vertex = mesh.vertices[i];
            Vector4 converted = new Vector4(vertex.x, vertex.y, vertex.z, 1);
            converted = matrix * converted;

            vertex = new Vector3(converted.x, converted.y, converted.z);

            array.SetValue(vertex, i);

            Gizmos.DrawSphere(array[i], 0.05f);
        }

        Gizmos.color = Color.blue;

        for (int i = 0; i < mesh.triangles.Length; i+=3)
        {
            int firstIndex = mesh.triangles[i];
            int secondIndex = mesh.triangles[i+1];
            int thirdIndex = mesh.triangles[i+2];

            Vector3 firstPos = array[firstIndex];
            Vector3 secondPos = array[secondIndex];
            Vector3 thirdPos = array[thirdIndex];

            Gizmos.DrawLine(firstPos, secondPos);
            Gizmos.DrawLine(secondPos, thirdPos);
            Gizmos.DrawLine(thirdPos, firstPos);
        }
    }
}