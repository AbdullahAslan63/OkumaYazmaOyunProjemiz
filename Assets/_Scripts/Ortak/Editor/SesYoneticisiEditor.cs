#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// SesYoneticisi Inspector: tek tıkla tüm clip klasörlerini doldurur.
/// </summary>
[CustomEditor(typeof(SesYoneticisi))]
public class SesYoneticisiEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Clip klasörlerini doldur (Objeler + Harf + Müzik + SFX)"))
        {
            SesYoneticisi ses = (SesYoneticisi)target;
            SesEfektKurulumEditor.ClippleriDoldur(ses);
        }

        if (GUILayout.Button("Tam kurulum: Oyun > Ses ve Efektleri Bagla (Faz 9)"))
            SesEfektKurulumEditor.HepsiniBagla();
    }
}
#endif
