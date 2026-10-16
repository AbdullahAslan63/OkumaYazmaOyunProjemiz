#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Her karakterin altında tüm aksesuarların yerini isimle gösterir.
/// Şapka ve gözlük ayrı satır olur; tek kafa ofsetine bağlı kalmaz.
/// </summary>
[CustomEditor(typeof(AvatarGorunumu))]
public class AvatarGorunumuEditor : Editor
{
    private bool[] hayvanAcik;

    public override void OnInspectorGUI()
    {
        AvatarGorunumu gorunum = (AvatarGorunumu)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Karakter × Aksesuar Yerleri", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Her karakter için sprite, yer ve boyut ayrı. " +
            "Otomatik hizala: gözlük→göz, şapka→taç, kulaklık→kulak; olcekCarpani kafa genişliğine göre.",
            MessageType.Info);

        if (GUILayout.Button("Tüm aksesuar satırlarını karakterlere ekle"))
        {
            Undo.RecordObject(gorunum, "Aksesuar ayarlarını senkronize et");
            gorunum.AksesuarAyarlariniSenkronizeEt();
            EditorUtility.SetDirty(gorunum);
        }

        if (GUILayout.Button("Otomatik hizala (gözlük / şapka / kulaklık + ölçek)"))
        {
            Undo.RecordObject(gorunum, "Aksesuar otomatik hizala");
            int n = AvatarAksesuarOtomatikHizala.Hizala(gorunum);
            EditorUtility.SetDirty(gorunum);
            Debug.Log($"Otomatik hizala: {n} satır güncellendi.");
        }

        if (gorunum.hayvanlar == null || gorunum.aksesuarlar == null)
            return;

        if (hayvanAcik == null || hayvanAcik.Length != gorunum.hayvanlar.Length)
            hayvanAcik = new bool[gorunum.hayvanlar.Length];

        serializedObject.Update();
        SerializedProperty hayvanlarProp = serializedObject.FindProperty("hayvanlar");

        for (int h = 0; h < gorunum.hayvanlar.Length; h++)
        {
            HayvanGorseli hayvan = gorunum.hayvanlar[h];
            if (hayvan == null)
                continue;

            string baslik = string.IsNullOrEmpty(hayvan.hayvanAdi) ? ("Hayvan " + h) : hayvan.hayvanAdi;
            hayvanAcik[h] = EditorGUILayout.Foldout(hayvanAcik[h], baslik + " — aksesuar yerleri", true);
            if (!hayvanAcik[h])
                continue;

            EditorGUI.indentLevel++;
            SerializedProperty ayarlarProp = hayvanlarProp.GetArrayElementAtIndex(h).FindPropertyRelative("aksesuarAyarlari");
            if (ayarlarProp == null)
            {
                EditorGUI.indentLevel--;
                continue;
            }

            int sayi = Mathf.Min(ayarlarProp.arraySize, gorunum.aksesuarlar.Length);
            for (int i = 0; i < sayi; i++)
            {
                SerializedProperty satir = ayarlarProp.GetArrayElementAtIndex(i);
                SerializedProperty adProp = satir.FindPropertyRelative("aksesuarAdi");
                SerializedProperty spriteProp = satir.FindPropertyRelative("sprite");
                SerializedProperty ofsetProp = satir.FindPropertyRelative("ofset");
                SerializedProperty olcekProp = satir.FindPropertyRelative("olcekCarpani");
                string aksAd = adProp != null ? adProp.stringValue : ("Aksesuar " + i);
                if (string.IsNullOrEmpty(aksAd))
                    aksAd = "Aksesuar " + i;

                EditorGUILayout.BeginHorizontal();
                // Önizleme: bu hayvana özel sprite; yoksa katalog
                Sprite spr = spriteProp != null ? spriteProp.objectReferenceValue as Sprite : null;
                if (spr == null && gorunum.aksesuarlar[i] != null)
                    spr = gorunum.aksesuarlar[i].sprite;
                if (spr != null)
                {
                    GUILayout.Label(AssetPreview.GetAssetPreview(spr) ?? spr.texture, GUILayout.Width(36), GUILayout.Height(36));
                }
                EditorGUILayout.BeginVertical();
                if (spriteProp != null)
                    EditorGUILayout.PropertyField(spriteProp, new GUIContent(aksAd + " sprite"));
                EditorGUILayout.PropertyField(ofsetProp, new GUIContent(aksAd + " yer"));
                if (olcekProp != null)
                    EditorGUILayout.Slider(olcekProp, 0.2f, 2.5f, new GUIContent(aksAd + " boyut"));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
