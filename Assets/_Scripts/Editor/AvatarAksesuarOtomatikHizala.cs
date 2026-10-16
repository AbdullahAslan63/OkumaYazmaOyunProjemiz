#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Karakter PNG landmark analiziyle aksesuar türüne göre (gözlük / şapka / kulaklık)
/// ofset ve olcekCarpani yazar.
/// Menü: Oyun > Avatar Aksesuar Otomatik Hizala
/// </summary>
public static class AvatarAksesuarOtomatikHizala
{
    private const string KarakterKlasor = "Assets/_Art/Avatar/Karakterler";
    private const float Ppu = 100f;

    // Hedef genişlik = kafa genişliği × bu oran (türe göre)
    private const float GozlukGenislikOrani = 0.95f;
    private const float SapkaGenislikOrani = 1.05f;
    private const float KulaklikGenislikOrani = 1.15f;

    // olcekCarpani güvenlik aralığı (art karaktere özel; aşırı küçültme)
    private const float OlcekMin = 0.85f;
    private const float OlcekMax = 1.75f;

    private enum AksesuarTur
    {
        Gozluk,
        Sapka,
        Kulaklik
    }

    private static readonly string[] HayvanPrefix =
    {
        "avatar_kedi_normal",
        "avatar_tavsan_normal",
        "avatar_kus_normal",
        "avatar_rakun_normal"
    };

    [MenuItem("Oyun/Avatar Aksesuar Otomatik Hizala")]
    public static void MenudenCalistir()
    {
        AvatarGorunumu gorunum = Object.FindAnyObjectByType<AvatarGorunumu>();
        if (gorunum == null)
        {
            EditorUtility.DisplayDialog(
                "Otomatik Hizala",
                "Sahnede AvatarGorunumu yok. AvatarOlusturmaEkrani'ni açıp tekrar dene.",
                "Tamam");
            return;
        }

        int yazilan = Hizala(gorunum);
        EditorUtility.SetDirty(gorunum);
        EditorSceneManager.MarkSceneDirty(gorunum.gameObject.scene);
        Debug.Log($"AvatarAksesuarOtomatikHizala: {yazilan} aksesuar satırı güncellendi.");
        EditorUtility.DisplayDialog(
            "Otomatik Hizala",
            $"{yazilan} satır güncellendi (gözlük / şapka / kulaklık).\n" +
            "Play'de kontrol et; gerekirse Inspector'dan ince ayar yap.",
            "Tamam");
    }

    /// <summary>Seçili AvatarGorunumu üzerinde tür + landmark tabanlı ince ayar.</summary>
    public static int Hizala(AvatarGorunumu gorunum)
    {
        if (gorunum == null || gorunum.hayvanlar == null)
            return 0;

        int sayac = 0;
        float hedefYukseklik = gorunum.hedefGovdeYuksekligi > 0.01f
            ? gorunum.hedefGovdeYuksekligi
            : 4.5f;
        float hedefZemin = gorunum.hedefZeminY;

        for (int h = 0; h < gorunum.hayvanlar.Length; h++)
        {
            HayvanGorseli hayvan = gorunum.hayvanlar[h];
            if (hayvan == null || hayvan.aksesuarAyarlari == null)
                continue;
            if (h >= HayvanPrefix.Length)
                continue;

            string pngYol = KarakterKlasor + "/" + HayvanPrefix[h] + ".png";
            Landmark lm = LandmarkHesapla(pngYol, hedefYukseklik, hedefZemin);
            if (lm == null)
            {
                Debug.LogWarning("Landmark okunamadı: " + pngYol);
                continue;
            }

            // Varsayılan kafa noktası = taç (şapka)
            hayvan.aksesuarOfset = new Vector2(0f, lm.tacDunyaY);

            for (int i = 0; i < hayvan.aksesuarAyarlari.Length; i++)
            {
                HayvanAksesuarAyari ayar = hayvan.aksesuarAyarlari[i];
                if (ayar == null)
                    continue;

                Sprite aksSprite = ayar.sprite;
                if (aksSprite == null && gorunum.aksesuarlar != null &&
                    i < gorunum.aksesuarlar.Length && gorunum.aksesuarlar[i] != null)
                {
                    aksSprite = gorunum.aksesuarlar[i].sprite;
                }

                AksesuarTur tur = TuruBelirle(ayar.aksesuarAdi, aksSprite);
                float ofsetY = OfsetYHesapla(tur, lm, aksSprite);
                float olcek = OlcekCarpaniHesapla(tur, lm, aksSprite);

                ayar.ofset = new Vector2(0f, ofsetY);
                ayar.olcekCarpani = olcek;

                Debug.Log(
                    $"{hayvan.hayvanAdi} / {ayar.aksesuarAdi}: tur={tur} " +
                    $"ofsetY={ofsetY:F2} olcek={olcek:F2}");

                sayac++;
            }
        }

        return sayac;
    }

    /// <summary>
    /// İsimden tür: kulaklık önce (kulakli_sapka şapka sayılır),
    /// sonra gözlük, kalan şapka.
    /// </summary>
    private static AksesuarTur TuruBelirle(string ad, Sprite spr)
    {
        string s = ((ad ?? "") + " " + (spr != null ? spr.name : "")).ToLowerInvariant();

        // "kulaklik" = kulaklık; "kulakli_sapka" = kulaklı şapka (şapka)
        if (s.Contains("kulaklik"))
            return AksesuarTur.Kulaklik;

        if (s.Contains("gozluk") || s.Contains("gunes"))
            return AksesuarTur.Gozluk;

        return AksesuarTur.Sapka;
    }

    /// <summary>Türe göre dikey konum (Avatar yerel Y).</summary>
    private static float OfsetYHesapla(AksesuarTur tur, Landmark lm, Sprite aksSprite)
    {
        float yari = AksesuarYariBoyut(aksSprite, lm.govdeOlcek, dikey: true);

        switch (tur)
        {
            case AksesuarTur.Gozluk:
                // Merkez göz hizasında
                return lm.gozDunyaY;

            case AksesuarTur.Kulaklik:
                // Kulak bandı göz ile taç arası; yastıklar kulağa otursun
                return lm.kulakDunyaY;

            default: // Sapka
                // Siperlik aşağı sarksın diye merkezi taçtan biraz yukarı
                return lm.tacDunyaY + yari * 0.25f;
        }
    }

    /// <summary>
    /// Hedef kafa genişliği / aksesuar genişliği → olcekCarpani.
    /// Art zaten yakınsa ~1; rakun gibi dar aksesuarda büyütür.
    /// </summary>
    private static float OlcekCarpaniHesapla(AksesuarTur tur, Landmark lm, Sprite aksSprite)
    {
        float aksGenislik = AksesuarYariBoyut(aksSprite, lm.govdeOlcek, dikey: false) * 2f;
        if (aksGenislik < 0.01f)
            return 1f;

        float hedefGenislik;
        switch (tur)
        {
            case AksesuarTur.Gozluk:
                hedefGenislik = lm.gozGenislikDunya * GozlukGenislikOrani;
                break;
            case AksesuarTur.Kulaklik:
                // Kulaklık kafayı yanlardan sarar → biraz daha geniş
                hedefGenislik = lm.kulakGenislikDunya * KulaklikGenislikOrani;
                break;
            default:
                hedefGenislik = lm.tacGenislikDunya * SapkaGenislikOrani;
                break;
        }

        float carpani = hedefGenislik / aksGenislik;
        return Mathf.Clamp(carpani, OlcekMin, OlcekMax);
    }

    /// <summary>Aksesuarın dünya biriminde yarı genişlik (dikey=false) veya yarı yükseklik.</summary>
    private static float AksesuarYariBoyut(Sprite aksSprite, float govdeOlcek, bool dikey)
    {
        if (aksSprite == null)
            return dikey ? 0.3f : 0.5f;
        float ham = dikey ? aksSprite.bounds.size.y : aksSprite.bounds.size.x;
        if (ham < 0.001f)
            ham = 0.001f;
        return ham * govdeOlcek * 0.5f;
    }

    /// <summary>
    /// Texture2D GetPixels32: y=0 alttadır.
    /// Göz / taç / kulak Y + o satırdaki kafa genişlikleri.
    /// </summary>
    private static Landmark LandmarkHesapla(string assetPath, float hedefYukseklik, float hedefZemin)
    {
        if (!File.Exists(assetPath))
            return null;

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool eskiReadable = false;
        if (importer != null)
        {
            eskiReadable = importer.isReadable;
            if (!eskiReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        try
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex == null)
                return null;

            int w = tex.width;
            int h = tex.height;
            Color32[] px = tex.GetPixels32();

            int y0 = h;
            int y1 = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (px[y * w + x].a > 40)
                    {
                        if (y < y0) y0 = y;
                        if (y > y1) y1 = y;
                        break;
                    }
                }
            }
            if (y1 < y0)
                return null;

            int oh = y1 - y0 + 1;
            int cx0 = w / 4;
            int cx1 = (3 * w) / 4;

            // Kulak ucu (üst opak yüz)
            int tip = y1;
            for (int y = y1; y >= y0; y--)
            {
                if (SeritteOpak(px, w, y, cx0, cx1))
                {
                    tip = y;
                    break;
                }
            }

            // Göz bandı (üst gövde)
            int band0 = y0 + Mathf.RoundToInt(oh * 0.60f);
            int band1 = y0 + Mathf.RoundToInt(oh * 0.86f);
            long sumY = 0;
            int say = 0;
            for (int y = band0; y <= band1 && y < h; y++)
            {
                for (int x = cx0; x < cx1; x++)
                {
                    Color32 c = px[y * w + x];
                    if (c.a < 180)
                        continue;
                    float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                    if (lum < 85f)
                    {
                        sumY += y;
                        say++;
                    }
                }
            }

            int gozY = say > 0
                ? (int)(sumY / say)
                : y0 + Mathf.RoundToInt(oh * 0.72f);

            // Taç: göz–kulak ucu; uzun kulakta göze yakın
            float t = (tip - gozY) > oh * 0.22f ? 0.55f : 0.35f;
            int tacY = gozY + Mathf.RoundToInt((tip - gozY) * (1f - t));

            // Kulak hizası: göz ile taç ortası (kulaklık yastığı)
            int kulakY = (gozY + tacY) / 2;

            float govdeOlcek = hedefYukseklik / (h / Ppu);
            float DunyaY(int texY) =>
                hedefZemin + (texY / Mathf.Max(1f, h - 1f)) * hedefYukseklik;

            // Satırdaki opak genişlik → dünya birimi
            float GenislikDunya(int texY)
            {
                int sol = -1;
                int sag = -1;
                for (int x = 0; x < w; x++)
                {
                    if (px[texY * w + x].a > 40)
                    {
                        if (sol < 0) sol = x;
                        sag = x;
                    }
                }
                if (sol < 0)
                    return 1.5f;
                float piksel = sag - sol + 1;
                return (piksel / Ppu) * govdeOlcek;
            }

            // Gözlük: göz satırındaki kafa genişliğinin ~%88'i (kulak uçlarını fazla alma)
            float gozGenislik = GenislikDunya(gozY) * 0.88f;

            return new Landmark
            {
                gozDunyaY = DunyaY(gozY),
                tacDunyaY = DunyaY(tacY),
                kulakDunyaY = DunyaY(kulakY),
                gozGenislikDunya = gozGenislik,
                tacGenislikDunya = GenislikDunya(tacY),
                kulakGenislikDunya = GenislikDunya(kulakY),
                govdeOlcek = govdeOlcek
            };
        }
        finally
        {
            if (importer != null && !eskiReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }
    }

    private static bool SeritteOpak(Color32[] px, int w, int y, int cx0, int cx1)
    {
        for (int x = cx0; x < cx1; x++)
        {
            if (px[y * w + x].a > 40)
                return true;
        }
        return false;
    }

    private class Landmark
    {
        public float gozDunyaY;
        public float tacDunyaY;
        public float kulakDunyaY;
        public float gozGenislikDunya;
        public float tacGenislikDunya;
        public float kulakGenislikDunya;
        public float govdeOlcek;
    }
}
#endif
