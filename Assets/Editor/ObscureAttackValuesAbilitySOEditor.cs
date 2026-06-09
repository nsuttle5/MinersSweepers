using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObscureAttackValuesAbilitySO))]
public class ObscureAttackValuesAbilitySOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObscureAttackValuesAbilitySO ability = (ObscureAttackValuesAbilitySO)target;

        EditorGUILayout.LabelField("Grid Size odd");
        int newSize = Mathf.Max(3, EditorGUILayout.IntField(ability.size));
        if (newSize % 2 == 0) newSize++;

        int totalCells = newSize * newSize;

        if (newSize != ability.size || ability.obscurePattern == null || ability.obscurePattern.Length != totalCells)
        {
            var newPattern = new bool[totalCells];

            if (ability.obscurePattern != null)
            {
                int min = Mathf.Min(ability.size, newSize);
                for (int y = 0; y < min; y++)
                    for (int x = 0; x < min; x++)
                    {
                        int oldIndex = x + y * ability.size;
                        int newIndex = x + y * newSize;
                        if (oldIndex < ability.obscurePattern.Length)
                            newPattern[newIndex] = ability.obscurePattern[oldIndex];
                    }
            }

            ability.size = newSize;
            ability.obscurePattern = newPattern;
            EditorUtility.SetDirty(ability);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern (center = this cell, toggled = obscured)");
        EditorGUILayout.Space();

        int half = ability.size / 2;
        bool changed = false;

        for (int y = 0; y < ability.size; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < ability.size; x++)
            {
                int index = x + y * ability.size;
                bool isCenter = x == half && y == half;

                if (isCenter)
                {
                    GUI.enabled = false;
                    GUILayout.Toggle(false, "●", "Button", GUILayout.Width(24), GUILayout.Height(24));
                    GUI.enabled = true;
                }
                else
                {
                    bool oldVal = ability.obscurePattern[index];
                    bool newVal = GUILayout.Toggle(oldVal, oldVal ? "?" : " ", "Button", GUILayout.Width(24), GUILayout.Height(24));
                    if (newVal != oldVal) { ability.obscurePattern[index] = newVal; changed = true; }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (changed)
            EditorUtility.SetDirty(ability);
    }
}