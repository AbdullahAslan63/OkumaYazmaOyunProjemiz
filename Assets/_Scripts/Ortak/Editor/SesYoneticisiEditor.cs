#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SesYoneticisi Inspector: Objeler klasöründeki wav'ları objeSesleri dizisine doldurur.
/// </summary>
[CustomEditor(typeof(SesYoneticisi))]
public class SesYoneticisiEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Objeler klasöründen objeSesleri doldur"))
        {
            SesYoneticisi ses = (SesYoneticisi)target;
            string[] guidler = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Audio/Objeler" });
            List<AudioClip> liste = new List<AudioClip>();
            for (int i = 0; i < guidler.Length; i++)
            {
                string yol = AssetDatabase.GUIDToAssetPath(guidler[i]);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(yol);
                if (clip != null)
                    liste.Add(clip);
            }

            Undo.RecordObject(ses, "Obje seslerini doldur");
            ses.objeSesleri = liste.ToArray();
            EditorUtility.SetDirty(ses);
            Debug.Log("SesYoneticisi: " + liste.Count + " obje klibi yüklendi.");
        }
    }
}
#endif
