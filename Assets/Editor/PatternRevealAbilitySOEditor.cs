using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatternRevealAbilitySO))]
public class PatternRevealAbilitySOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PatternRevealAbilitySO ability = (PatternRevealAbilitySO)target;
        EditorGUILayout.LabelField("Grid Size (odd, e.g. 3,5,7)");
        int newSize = Mathf.Max(3, EditorGUILayout.IntField(ability.size));
        if (newSize % 2 == 0) newSize += 1; //odd for symmetry

        if (newSize != ability.size || ability.revealPattern == null || ability.revealPattern.Length != newSize * newSize)
        {
            var newPattern = new bool[newSize * newSize];
            if (ability.revealPattern != null)
            {
                int min = Mathf.Min(ability.size, newSize);
                for (int y = 0; y < min; y++)
                    for (int x = 0; x < min; x++)
                        newPattern[x + y * newSize] = ability.revealPattern[x + y * ability.size];
            }
            ability.size = newSize;
            ability.revealPattern = newPattern;
            EditorUtility.SetDirty(ability);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern (center is this cell)");

        int half = ability.size / 2;
        for (int y = 0; y < ability.size; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < ability.size; x++)
            {
                int index = x + y * ability.size;
                GUI.enabled = !(x == half && y == half);
                bool oldValue = ability.revealPattern[index];
                bool value = GUILayout.Toggle(oldValue, "", "Button", GUILayout.Width(24), GUILayout.Height(24));
                if (value != oldValue)
                {
                    ability.revealPattern[index] = value;
                    EditorUtility.SetDirty(ability);
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}