using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatternSpawnAbilitySO))]
public class PatternSpawnAbilitySOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PatternSpawnAbilitySO ability = (PatternSpawnAbilitySO)target;

        EditorGUILayout.LabelField("Grid Size");
        int newSize = Mathf.Max(3, EditorGUILayout.IntField(ability.size));
        if (newSize % 2 == 0) newSize += 1;

        int totalCells = newSize * newSize;

        if (newSize != ability.size || ability.spawnPattern == null || ability.spawnPattern.Length != totalCells)
        {
            var newPattern = new PatternSpawnAbilitySO.PatternCell[totalCells];
            for (int i = 0; i < totalCells; i++)
                newPattern[i] = new PatternSpawnAbilitySO.PatternCell();

            if (ability.spawnPattern != null)
            {
                int min = Mathf.Min(ability.size, newSize);
                for (int y = 0; y < min; y++)
                    for (int x = 0; x < min; x++)
                    {
                        int oldIndex = x + y * ability.size;
                        int newIndex = x + y * newSize;
                        if (oldIndex < ability.spawnPattern.Length)
                            newPattern[newIndex] = ability.spawnPattern[oldIndex];
                    }
            }

            ability.size = newSize;
            ability.spawnPattern = newPattern;
            EditorUtility.SetDirty(ability);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern (center = this cell, toggle to enable slot)");
        EditorGUILayout.Space();

        int half = ability.size / 2;
        bool changed = false;

        for (int y = 0; y < ability.size; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < ability.size; x++)
            {
                int index = x + y * ability.size;
                bool isCenter = (x == half && y == half);

                EditorGUILayout.BeginVertical(GUILayout.Width(80));

                if (isCenter)
                {
                    GUI.enabled = false;
                    GUILayout.Toggle(false, "●", "Button", GUILayout.Width(80), GUILayout.Height(20));
                    EditorGUILayout.LabelField("(origin)", GUILayout.Width(80));
                    GUI.enabled = true;
                }
                else
                {
                    var cell = ability.spawnPattern[index];

                    bool newActive = GUILayout.Toggle(cell.active, cell.active ? "ON" : "OFF", "Button", GUILayout.Width(80), GUILayout.Height(20));
                    if (newActive != cell.active) { cell.active = newActive; changed = true; }

                    GUI.enabled = cell.active;
                    SpawnableSO newSpawnable = (SpawnableSO)EditorGUILayout.ObjectField(
                        cell.spawnableToPlace, typeof(SpawnableSO), false, GUILayout.Width(80));
                    if (newSpawnable != cell.spawnableToPlace) { cell.spawnableToPlace = newSpawnable; changed = true; }
                    GUI.enabled = true;
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }

        if (changed)
            EditorUtility.SetDirty(ability);
    }
}