#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// LevelEkranı sahnesine seviye seçim UI taslağını kalıcı kurar.
/// Menü: Oyun > Level Ekranini Kur
/// </summary>
public static class LevelEkraniKurulumEditor
{
    private const string LevelSahne = "Assets/Scenes/LevelEkranı.unity";
    private const string AvatarSahne = "Assets/Scenes/AvatarOlusturmaEkrani.unity";
    private const string HarfSahne = "Assets/Scenes/HarfSecmeOyunu.unity";
    private const string SepetSahne = "Assets/Scenes/SepetOyunu.unity";

    [MenuItem("Oyun/Level Ekranini Kur")]
    public static void LevelEkraniniKur()
    {
        if (!File.Exists(LevelSahne))
        {
            Debug.LogError("Level sahnesi yok: " + LevelSahne);
            return;
        }

        SpriteOlarakGarantiEt(LevelEkraniKurucu.Kapak1Yol);
        SpriteOlarakGarantiEt(LevelEkraniKurucu.Kapak2Yol);
        SpriteOlarakGarantiEt(LevelEkraniKurucu.ArkaplanYol);
        SpriteOlarakGarantiEt(LevelEkraniKurucu.YildizYol);
        SpriteOlarakGarantiEt(LevelEkraniKurucu.OkYol);

        Scene scene = EditorSceneManager.OpenScene(LevelSahne, OpenSceneMode.Single);

        // Eski canvas'ları temizle
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
            Object.DestroyImmediate(c.gameObject);

        EventSystem[] eskiEs = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        foreach (EventSystem es in eskiEs)
            Object.DestroyImmediate(es.gameObject);

        GameObject canvasGo = new GameObject("LevelCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<SeviyeSecimEkrani>();
        LevelEkraniKurucu kurucu = canvasGo.AddComponent<LevelEkraniKurucu>();

        EventSystemOlustur();

        kurucu.UiKur(canvasGo.transform);
        kurucu.kuruldu = true;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, LevelSahne);

        BuildSettingsGuncelle();
        AvatarDevamHedefiniGuncelle();

        AssetDatabase.SaveAssets();
        Debug.Log("LevelEkranı kuruldu. Avatar Devam → LevelEkranı. Play ile test et.");
    }

    private static void AvatarDevamHedefiniGuncelle()
    {
        if (!File.Exists(AvatarSahne)) return;

        Scene avatarScene = EditorSceneManager.OpenScene(AvatarSahne, OpenSceneMode.Single);
        AvatarSecimEkrani[] secimler = Object.FindObjectsByType<AvatarSecimEkrani>(FindObjectsSortMode.None);
        foreach (AvatarSecimEkrani s in secimler)
            s.sonrakiSahneAdi = "LevelEkranı";

        EditorSceneManager.MarkSceneDirty(avatarScene);
        EditorSceneManager.SaveScene(avatarScene, AvatarSahne);
        EditorSceneManager.OpenScene(LevelSahne, OpenSceneMode.Single);
        Debug.Log("Avatar sonrakiSahneAdi = LevelEkranı");
    }

    private static void BuildSettingsGuncelle()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(AvatarSahne, true),
            new EditorBuildSettingsScene(LevelSahne, true)
        };
        if (File.Exists(HarfSahne))
            scenes.Add(new EditorBuildSettingsScene(HarfSahne, true));
        if (File.Exists(SepetSahne))
            scenes.Add(new EditorBuildSettingsScene(SepetSahne, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build Settings: Avatar → LevelEkranı → oyunlar.");
    }

    private static void SpriteOlarakGarantiEt(string path)
    {
        if (!File.Exists(path)) return;
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        if (importer.textureType == TextureImporterType.Sprite) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();
    }

    private static void EventSystemOlustur()
    {
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }
}
#endif
