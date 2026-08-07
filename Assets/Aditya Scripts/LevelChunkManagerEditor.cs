using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelChunkSpawner))]
public class LevelChunkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields (Level Zones list, etc.)
        DrawDefaultInspector();

        LevelChunkSpawner chunkManager = (LevelChunkSpawner)target;

        GUILayout.Space(15);

        // Render a large GUI button in the Inspector window
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
        if (GUILayout.Button("🎲 Randomize Level Layout", GUILayout.Height(35)))
        {
            chunkManager.RebuildLevel();
        }
        GUI.backgroundColor = Color.white;
    }
}