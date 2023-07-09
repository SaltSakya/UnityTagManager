using UnityEditor;
using UnityEngine;

namespace SaltSakya.TagManager
{
    [CustomEditor(typeof(TagEncoder))]
    [CanEditMultipleObjects]
    public class TagGeneratorEditor : Editor
    {
        private SerializedProperty SavePath;
        private SerializedProperty TagName;
        private SerializedProperty DataType;

        private void OnEnable()
        {
            SavePath = serializedObject.FindProperty("SavePath");
            TagName = serializedObject.FindProperty("TagName");
            DataType = serializedObject.FindProperty("DataType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(SavePath);
            EditorGUILayout.PropertyField(TagName);
            EditorGUILayout.PropertyField(DataType);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Generate"))
            {
                (target as TagEncoder)?.Generate();
            }

        }
    }
}