using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatternModifyDamageAbilitySO))]
public class PatternModifyDamageAbilitySOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PatternModifyDamageAbilitySO ability = (PatternModifyDamageAbilitySO)target;

        EditorGUILayout.LabelField("Grid Size odd");
        int newSize = Mathf.Max(3, EditorGUILayout.IntField(ability.size));
        if (newSize % 2 == 0) newSize++;

        int totalCells = newSize * newSize;

        if (newSize != ability.size || ability.modifyPattern == null || ability.modifyPattern.Length != totalCells)
        {
            var newPattern = new PatternModifyDamageAbilitySO.ModifyCell[totalCells];
            for (int i = 0; i < totalCells; i++)
                newPattern[i] = new PatternModifyDamageAbilitySO.ModifyCell();

            if (ability.modifyPattern != null)
            {
                int min = Mathf.Min(ability.size, newSize);
                for (int y = 0; y < min; y++)
                    for (int x = 0; x < min; x++)
                    {
                        int oldIndex = x + y * ability.size;
                        int newIndex = x + y * newSize;
                        if (oldIndex < ability.modifyPattern.Length)
                            newPattern[newIndex] = ability.modifyPattern[oldIndex];
                    }
            }

            ability.size = newSize;
            ability.modifyPattern = newPattern;
            EditorUtility.SetDirty(ability);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern (center = this cell)");
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

                EditorGUILayout.BeginVertical(GUILayout.Width(90));

                if (isCenter)
                {
                    GUI.enabled = false;
                    GUILayout.Toggle(false, "●", "Button", GUILayout.Width(90), GUILayout.Height(20));
                    EditorGUILayout.LabelField("(origin)", GUILayout.Width(90));
                    GUI.enabled = true;
                }
                else
                {
                    var cell = ability.modifyPattern[index];

                    bool newActive = GUILayout.Toggle(cell.active, cell.active ? "ON" : "OFF", "Button", GUILayout.Width(90), GUILayout.Height(20));
                    if (newActive != cell.active) { cell.active = newActive; changed = true; }

                    GUI.enabled = cell.active;

                    DamageModifyOperation newOp = (DamageModifyOperation)EditorGUILayout.EnumPopup(cell.operation, GUILayout.Width(90));
                    if (newOp != cell.operation) { cell.operation = newOp; changed = true; }

                    int newVal = EditorGUILayout.IntField(cell.value, GUILayout.Width(90));
                    if (newVal != cell.value) { cell.value = newVal; changed = true; }

                    GUI.enabled = true;
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(6);
        }

        if (changed)
            EditorUtility.SetDirty(ability);
    }
}