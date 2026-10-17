#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Karakter PNG landmark analiziyle aksesuar türüne göre (gözlük / şapka / kulaklık)
/// ofset ve olcekCarpani yazar. Uzun kulaklı karakterlerde (tavşan) yüz daha aşağıda aranır.
/// Menü: Oyun > Avatar Aksesuar Otomatik Hizala
/// </summary>
public static class AvatarAksesuarOtomatikHizala
{
    private const string KarakterKlasor = "Assets/_Art/Avatar/Karakterler";
    private const float Ppu = 100f;

    private const float GozlukGenislikOrani = 0.95f;
    private const float SapkaGenislikOrani = 1.05f;
    private const float KulaklikGenislikOrani = 1.15f;

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
            $"{yazilan} satır güncellendi (gözlük / şapka / kulaklık; uzun kulak destekli).\n" +
            "Play'de özellikle tavşanı kontrol et.",
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
            bool zorlaUzunKulak = HayvanPrefix[h].Contains("tavsan");
            Landmark lm = LandmarkHesapla(pngYol, hedefYukseklik, hedefZemin, zorlaUzunKulak);
            if (lm == null)
            {
                Debug.LogWarning("Landmark okunamadı: " + pngYol);
                continue;
            }

            Debug.Log(
                $"{hayvan.hayvanAdi}: uzunKulak={lm.uzunKulak} " +
                $"gozY={lm.gozDunyaY:F2} tacY={lm.tacDunyaY:F2} kulakY={lm.kulakDunyaY:F2}");

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
                    $"  {ayar.aksesuarAdi}: tur={tur} ofsetY={ofsetY:F2} olcek={olcek:F2}");

                sayac++;
            }
        }

        return sayac;
    }

    private static AksesuarTur TuruBelirle(string ad, Sprite spr)
    {
        string s = ((ad ?? "") + " " + (spr != null ? spr.name : "")).ToLowerInvariant();
        if (s.Contains("kulaklik"))
            return AksesuarTur.Kulaklik;
        if (s.Contains("gozluk") || s.Contains("gunes"))
            return AksesuarTur.Gozluk;
        return AksesuarTur.Sapka;
    }

    private static float OfsetYHesapla(AksesuarTur tur, Landmark lm, Sprite aksSprite)
    {
        float yari = AksesuarYariBoyut(aksSprite, lm.govdeOlcek, dikey: true);

        switch (tur)
        {
            case AksesuarTur.Gozluk:
                return lm.gozDunyaY;

            case AksesuarTur.Kulaklik:
                // Uzun kulakta yastık göz hizasında (kulak gövdesi yüz yanında)
                return lm.kulakDunyaY;

            default:
                // Uzun kulakta taç zaten alçak; fazla yukarı itme
                float lift = lm.uzunKulak ? 0.12f : 0.25f;
                return lm.tacDunyaY + yari * lift;
        }
    }

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
                hedefGenislik = lm.kulakGenislikDunya * KulaklikGenislikOrani;
                break;
            default:
                hedefGenislik = lm.tacGenislikDunya * SapkaGenislikOrani;
                break;
        }

        // Uzun kulaklı karakterde kafa daha küçük — aksesuarı biraz küçültmeye izin ver
        float min = lm.uzunKulak ? 0.70f : OlcekMin;
        float carpani = hedefGenislik / aksGenislik;
        return Mathf.Clamp(carpani, min, OlcekMax);
    }

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
    /// Göz çifti + uzun kulak (tavşan) için alçak yüz / taç.
    /// </summary>
    private static Landmark LandmarkHesapla(
        string assetPath,
        float hedefYukseklik,
        float hedefZemin,
        bool zorlaUzunKulak)
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

            int tip = y1;
            for (int y = y1; y >= y0; y--)
            {
                if (SeritteOpak(px, w, y, w / 4, (3 * w) / 4))
                {
                    tip = y;
                    break;
                }
            }

            bool uzunKulak = zorlaUzunKulak || UzunKulakMi(px, w, h, tip, oh);

            // Göz: uzun kulakta çift-leke (yüz aşağıda); diğerlerinde üst bant
            int gozY;
            bool gozBulundu = false;
            if (uzunKulak)
                gozBulundu = GozCiftiBul(px, w, h, out gozY);
            else
                gozY = 0;

            if (!gozBulundu)
            {
                int band0 = y0 + Mathf.RoundToInt(oh * (uzunKulak ? 0.42f : 0.60f));
                int band1 = y0 + Mathf.RoundToInt(oh * (uzunKulak ? 0.72f : 0.86f));
                gozY = KoyuOrtalamaY(px, w, band0, band1, w / 5, (4 * w) / 5);
                if (gozY < 0)
                    gozY = y0 + Mathf.RoundToInt(oh * (uzunKulak ? 0.55f : 0.72f));
            }

            int tacY;
            int kulakY;
            if (uzunKulak)
            {
                // Taç = kulak dipleri / alın (göz ile kulak ucu arasında, göze yakın)
                tacY = gozY + Mathf.RoundToInt((tip - gozY) * 0.22f);
                // Kulaklık yastığı yüz yanında ≈ göz
                kulakY = gozY + Mathf.RoundToInt((tacY - gozY) * 0.35f);
            }
            else
            {
                float t = (tip - gozY) > oh * 0.22f ? 0.55f : 0.35f;
                tacY = gozY + Mathf.RoundToInt((tip - gozY) * (1f - t));
                kulakY = (gozY + tacY) / 2;
            }

            float govdeOlcek = hedefYukseklik / (h / Ppu);
            float DunyaY(int texY) =>
                hedefZemin + (texY / Mathf.Max(1f, h - 1f)) * hedefYukseklik;

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
                return ((sag - sol + 1) / Ppu) * govdeOlcek;
            }

            // Uzun kulakta göz satırında kulaklar genişliği şişirmesin — merkez şerit
            float gozGenislik = uzunKulak
                ? GenislikMerkezSerit(px, w, gozY, govdeOlcek)
                : GenislikDunya(gozY) * 0.88f;

            float tacGenislik = uzunKulak
                ? GenislikMerkezSerit(px, w, tacY, govdeOlcek)
                : GenislikDunya(tacY);

            return new Landmark
            {
                gozDunyaY = DunyaY(gozY),
                tacDunyaY = DunyaY(tacY),
                kulakDunyaY = DunyaY(kulakY),
                gozGenislikDunya = gozGenislik,
                tacGenislikDunya = tacGenislik,
                kulakGenislikDunya = uzunKulak ? gozGenislik * 1.05f : GenislikDunya(kulakY),
                govdeOlcek = govdeOlcek,
                uzunKulak = uzunKulak
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

    /// <summary>
    /// Üst bölgede ortada boşluk, yanlarda opak = iki kulak sapı (uzun kulak).
    /// </summary>
    private static bool UzunKulakMi(Color32[] px, int w, int h, int tip, int oh)
    {
        int yA = tip - Mathf.RoundToInt(oh * 0.08f);
        int yB = tip - Mathf.RoundToInt(oh * 0.18f);
        if (yB < 0) yB = 0;
        if (yA <= yB) return false;

        int sol = 0, orta = 0, sag = 0;
        int x1 = w / 3;
        int x2 = (2 * w) / 3;
        for (int y = yB; y <= yA; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (px[y * w + x].a <= 40)
                    continue;
                if (x < x1) sol++;
                else if (x < x2) orta++;
                else sag++;
            }
        }

        // İki kulak: sol ve sağ dolu, orta seyrek
        return sol > 80 && sag > 80 && orta < (sol + sag) * 0.35f;
    }

    /// <summary>Sol/sağ iki benzer koyu lekeyi göz çifti sayar.</summary>
    private static bool GozCiftiBul(Color32[] px, int w, int h, out int gozY)
    {
        gozY = 0;
        int yLo = Mathf.RoundToInt(h * 0.35f);
        int yHi = Mathf.RoundToInt(h * 0.85f);
        bool[] ziyaret = new bool[w * h];
        List<Vector3> lekeler = new List<Vector3>(); // x,y,alan

        for (int y = yLo; y < yHi; y++)
        {
            for (int x = w / 10; x < (9 * w) / 10; x++)
            {
                int i = y * w + x;
                if (ziyaret[i] || !KoyuGozPiksel(px[i]))
                    continue;

                int alan = 0;
                float sumX = 0f;
                float sumY = 0f;
                Stack<int> yigin = new Stack<int>();
                yigin.Push(i);
                ziyaret[i] = true;

                while (yigin.Count > 0)
                {
                    int cur = yigin.Pop();
                    int cy = cur / w;
                    int cx = cur % w;
                    alan++;
                    sumX += cx;
                    sumY += cy;

                    TryPush(cx + 1, cy);
                    TryPush(cx - 1, cy);
                    TryPush(cx, cy + 1);
                    TryPush(cx, cy - 1);

                    void TryPush(int nx, int ny)
                    {
                        if (nx < 0 || nx >= w || ny < yLo || ny >= yHi)
                            return;
                        int ni = ny * w + nx;
                        if (ziyaret[ni] || !KoyuGozPiksel(px[ni]))
                            return;
                        ziyaret[ni] = true;
                        yigin.Push(ni);
                    }
                }

                // Göz boyutu: çok küçük/leke veya tüm yüz maskesi değil
                if (alan >= 50 && alan <= 3500)
                    lekeler.Add(new Vector3(sumX / alan, sumY / alan, alan));
            }
        }

        lekeler.Sort((a, b) => b.z.CompareTo(a.z));
        int limit = Mathf.Min(8, lekeler.Count);
        float bestAlan = -1f;
        float bestY = 0f;

        for (int i = 0; i < limit; i++)
        {
            for (int j = i + 1; j < limit; j++)
            {
                Vector3 a = lekeler[i];
                Vector3 b = lekeler[j];
                if (Mathf.Abs(a.y - b.y) > 40f)
                    continue;
                if (Mathf.Abs(a.x - b.x) < w * 0.15f)
                    continue;
                float oran = Mathf.Max(a.z, b.z) / Mathf.Max(1f, Mathf.Min(a.z, b.z));
                if (oran > 2.8f)
                    continue;
                float toplam = a.z + b.z;
                if (toplam > bestAlan)
                {
                    bestAlan = toplam;
                    bestY = (a.y + b.y) * 0.5f;
                }
            }
        }

        if (bestAlan < 0f)
            return false;

        gozY = Mathf.RoundToInt(bestY);
        return true;
    }

    private static bool KoyuGozPiksel(Color32 c)
    {
        if (c.a < 200)
            return false;
        float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
        return lum < 60f;
    }

    private static int KoyuOrtalamaY(Color32[] px, int w, int y0, int y1, int x0, int x1)
    {
        long sum = 0;
        int say = 0;
        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x < x1; x++)
            {
                Color32 c = px[y * w + x];
                if (c.a < 180)
                    continue;
                float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                if (lum < 85f)
                {
                    sum += y;
                    say++;
                }
            }
        }
        return say > 0 ? (int)(sum / say) : -1;
    }

    private static float GenislikMerkezSerit(Color32[] px, int w, int y, float govdeOlcek)
    {
        int x0 = w / 5;
        int x1 = (4 * w) / 5;
        int sol = -1;
        int sag = -1;
        for (int x = x0; x < x1; x++)
        {
            if (px[y * w + x].a > 40)
            {
                if (sol < 0) sol = x;
                sag = x;
            }
        }
        if (sol < 0)
            return 1.2f;
        return ((sag - sol + 1) / Ppu) * govdeOlcek;
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
        public bool uzunKulak;
    }
}
#endif
