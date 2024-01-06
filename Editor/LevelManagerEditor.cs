using System;
using Level;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : UnityEditor.Editor
    {
        private LevelManager _levelManager;

        private void OnEnable()
        {
            _levelManager = (LevelManager)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Find All Spawners"))
            {
                _levelManager.FindAllCarSpawns();
            }
        }
    }
}
