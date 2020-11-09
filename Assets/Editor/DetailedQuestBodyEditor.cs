using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
[CustomEditor(typeof(DetailedQuestBody))]
public class DetailedQuestBodyEditor : UnityEditor.UI.LayoutElementEditor
{
    public override void OnInspectorGUI()
    {
        DetailedQuestBody component = (DetailedQuestBody)target;
        //base.OnInspectorGUI();
        component.rt = (RectTransform)EditorGUILayout.ObjectField("Content", component.rt, typeof(RectTransform), true);
    }
}