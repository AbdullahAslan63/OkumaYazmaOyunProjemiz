#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using TMPro;

/// <summary>
/// Soru UI fontu: Andika Bold (Türkçe + okunaklı) + atlas alt varlık kaydı.
/// Menü: Oyun > Soru Fontunu Olustur (TMP)
/// </summary>
public static class TmpSoruFontKurulum
{
    private const string KaynakFont = "Assets/_Art/Fonts/Andika-Bold.ttf";
    private const string CiktiKlasor = "Assets/Resources/Fonts";
    private const string CiktiAsset = "Assets/Resources/Fonts/SoruFontu.asset";

    private const string KarakterSeti =
        "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZabcçdefgğhıijklmnoöprsştuüvyz" +
        " Hangisinin baş harfiDoğru:0123456789!,.-'";

    [MenuItem("Oyun/Soru Fontunu Olustur (TMP)")]
    public static void FontuOlustur()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(CiktiKlasor))
            AssetDatabase.CreateFolder("Assets/Resources", "Fonts");

        if (File.Exists(CiktiAsset))
            AssetDatabase.DeleteAsset(CiktiAsset);
        string eskiYedek = CiktiKlasor + "/SoruFontu_AndikaYedek.asset";
        if (File.Exists(eskiYedek))
            AssetDatabase.DeleteAsset(eskiYedek);

        Font kaynak = AssetDatabase.LoadAssetAtPath<Font>(KaynakFont);
        if (kaynak == null)
        {
            Debug.LogError("Andika-Bold.ttf bulunamadı: " + KaynakFont);
            return;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            kaynak,
            90,
            9,
            UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic);

        if (fontAsset == null)
        {
            Debug.LogError("TMP_FontAsset oluşturulamadı.");
            return;
        }

        fontAsset.name = "SoruFontu";

        // Türkçe + soru metni gliflerini atlas’a yaz
        fontAsset.TryAddCharacters(KarakterSeti, out string eksik);
        if (!string.IsNullOrEmpty(eksik))
            Debug.LogWarning("Fonta eklenemeyen karakterler: " + eksik);

        AssetDatabase.CreateAsset(fontAsset, CiktiAsset);
        AltVarliklariEkle(fontAsset);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("SoruFontu hazır (Andika Bold + Türkçe atlas): " + CiktiAsset);
    }

    private static void AltVarliklariEkle(TMP_FontAsset fontAsset)
    {
        if (fontAsset.material != null)
        {
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        if (fontAsset.atlasTextures != null)
        {
            for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
            {
                Texture2D tex = fontAsset.atlasTextures[i];
                if (tex == null) continue;
                tex.name = fontAsset.name + " Atlas " + i;
                AssetDatabase.AddObjectToAsset(tex, fontAsset);
            }
        }
        else if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }
    }

    [InitializeOnLoadMethod]
    private static void EksikVeyaBozuksaOlustur()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(KaynakFont)) return;

            bool yeniden = !File.Exists(CiktiAsset);
            if (!yeniden)
            {
                TMP_FontAsset fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CiktiAsset);
                yeniden = fa == null || fa.atlasTexture == null || fa.material == null;
            }
            if (yeniden)
                FontuOlustur();
        };
    }
}
#endif
