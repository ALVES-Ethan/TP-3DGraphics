using UnityEditor;

[CustomEditor(typeof(GeometryRenderer))]
public class GeometryRendererEditor : Editor
{
    SerializedProperty debug;
    SerializedProperty mode;

    SerializedProperty resolution;

    SerializedProperty rotate;
    SerializedProperty axis;

    GeometryRenderer component => (GeometryRenderer)target;

    void OnEnable()
    {
        debug = serializedObject.FindProperty("debug");
        mode = serializedObject.FindProperty("mode");

        resolution = serializedObject.FindProperty("resolution");

        rotate = serializedObject.FindProperty("rotate");
        axis = serializedObject.FindProperty("axis");
    }

    public override void OnInspectorGUI()
    {
        OnDrawGUI();

        if (serializedObject.hasModifiedProperties)
        {
            EditorUtility.SetDirty(target);
        }
        serializedObject.ApplyModifiedProperties();
    }

    void OnDrawGUI()
    {
        EditorGUILayout.BeginVertical("box");

        debug.boolValue = EditorGUILayoutExtensions.BetterToggle("Debug", debug.boolValue);
        EditorGUILayout.PropertyField(mode);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        switch (component.mode)
        {
            case GeometryRenderer.Mode.None:
                break;
            case GeometryRenderer.Mode.Triangle:
                break;
            case GeometryRenderer.Mode.Circle:
                EditorGUILayout.PropertyField(resolution);
                break;
            default:
                break;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        rotate.boolValue = EditorGUILayoutExtensions.BetterToggle("Rotate", rotate.boolValue);
        EditorGUILayout.PropertyField(axis);

        EditorGUILayout.EndVertical();
    }
}