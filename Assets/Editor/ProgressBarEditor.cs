using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
[CustomEditor(typeof(ProgressBar))]
public class ProgressBarEditor : UnityEditor.UI.ImageEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();//Draw inspector UI of ImageEditor

        ProgressBar bar = (ProgressBar)target;
        //bar.colorBar = EditorGUILayout.ColorField("Bar Color:", bar.colorBar);
        //bar.colorLocked = EditorGUILayout.ColorField("Locked Color:", bar.colorLocked);
        bar.fill = (Image)EditorGUILayout.ObjectField("Fill Image GameObject:", bar.fill, typeof(Image), true);
        bar.counter = (Image)EditorGUILayout.ObjectField("Counter Image GameObject:", bar.counter, typeof(Image), true);
        bar.counterText = (TMP_Text)EditorGUILayout.ObjectField("Counter Text GameObject:", bar.counterText, typeof(TMP_Text), true);
    }
}