using UnityEditor;
using UnityEngine;

public static class EditorGUILayoutExtensions
{
    public static bool BetterToggle(string label, bool value, string onText = "On", string offText = "Off", float buttonWidth = 64f)
    {
        EditorGUILayout.BeginHorizontal();

        if (label == null)
            label = string.Empty;

        EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 1));
        int index = value ? 0 : 1;
        index = GUILayout.Toolbar(index, new string[] { onText, offText }, GUILayout.Width(buttonWidth));
        
        EditorGUILayout.EndHorizontal();

        return index == 0 ? true : false;
    }
}