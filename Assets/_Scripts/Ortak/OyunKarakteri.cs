using System.Collections;
using UnityEngine;

/// <summary>
/// Mini oyunlarda sağ alt köşede seçili avatarı gösterir.
/// Doğruda mutlu + zıplar; yanlışta üzgün; süre bitince normal.
/// </summary>
public class OyunKarakteri : MonoBehaviour
{
    // Tek örnek (sahne başına)
    public static OyunKarakteri Instance;

    // Kamera yarısına göre ofset oranı (x sağ, y alt) — köşeye yapışık
    public Vector2 ekranOfsetOrani = new Vector2(0.90f, 0.88f);

    // Ekran yüksekliğine göre karakter boyu oranı
    public float ekranYukseklikOrani = 0.30f;

    // Zıplama süresi (sn)
    public float ziplamaSuresi = 0.4f;

    // Zıplama yüksekliği (dünya birimi)
    public float ziplamaYuksekligi = 0.35f;

    // Gövde çizim sırası
    public int sortingOrderGovde = 7;

    // Aksesuar çizim sırası
    public int sortingOrderAksesuar = 8;

    // Seçim ekranı ile aynı normalize yüksekliği; kök scale küçültür
    private const float SablonGovdeYuksekligi = 4.5f;

    // Avatar görünümü
    private AvatarGorunumu gorunum;

    // Zıplama taban dünya konumu
    private Vector3 tabanKonum;

    // Aktif zıplama coroutine
    private Coroutine ziplaRutin;

    /// <summary>Sahnede yoksa oluşturur; konumu kameraya göre ayarlar.</summary>
    public static OyunKarakteri GarantiEt(Camera kamera)
    {
        // Şablon sprite listesi hazır olsun
        SablonuGarantiEt();

        OyunKarakteri mevcut = Instance;
        if (mevcut == null)
        {
            mevcut = Object.FindAnyObjectByType<OyunKarakteri>();
        }

        if (mevcut == null)
        {
            GameObject go = new GameObject("OyunKarakteri");
            mevcut = go.AddComponent<OyunKarakteri>();
            mevcut.HiyerarsiKur();
            mevcut.SablonuUygula();
        }

        if (kamera != null)
        {
            mevcut.YerlesimiAyarla(kamera);
        }

        mevcut.GorunumuYenile();
        mevcut.NormalPoz();
        // Soft halo + ambient sparkle (her iki mini oyun)
        mevcut.PariltiyiGarantiEt();
        return mevcut;
    }

    private void Awake()
    {
        // Tek örnek
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Child'lar yoksa kur
        HiyerarsiKur();
        SablonuUygula();
        PariltiyiGarantiEt();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>Sağ alt köşeye yerleştirir ve boyutu ayarlar.</summary>
    public void YerlesimiAyarla(Camera kamera)
    {
        if (kamera == null)
        {
            return;
        }

        // Hiyerarşi hazır olsun
        HiyerarsiKur();

        float halfH = kamera.orthographicSize;
        float halfW = halfH * kamera.aspect;
        Vector3 cam = kamera.transform.position;

        // Sağ alt
        float x = cam.x + halfW * ekranOfsetOrani.x;
        float y = cam.y - halfH * ekranOfsetOrani.y;
        tabanKonum = new Vector3(x, y, 0f);
        transform.position = tabanKonum;

        // Ekranın ~%22'si yükseklik
        float hedefWorldH = (halfH * 2f) * ekranYukseklikOrani;
        if (gorunum != null)
        {
            gorunum.hedefGovdeYuksekligi = SablonGovdeYuksekligi;
            gorunum.hedefZeminY = 0f;
        }

        float sablonH = SablonGovdeYuksekligi;
        float olcek = sablonH > 0.001f ? hedefWorldH / sablonH : 0.2f;
        transform.localScale = new Vector3(olcek, olcek, 1f);
        SortingAyarla();
        PariltiyiGarantiEt();
    }

    /// <summary>Seçime göre sprite/renk/aksesuar yeniler.</summary>
    public void GorunumuYenile()
    {
        HiyerarsiKur();
        SablonuUygula();
        if (gorunum != null)
        {
            gorunum.Guncelle();
            SortingAyarla();
        }
    }

    /// <summary>Mutlu poz + kısa zıplama.</summary>
    public void DogruTepki()
    {
        if (AvatarYoneticisi.Instance != null)
        {
            AvatarYoneticisi.Instance.IfadeSec(1);
        }
        else
        {
            GorunumuYenile();
        }

        // Önceki zıplamayı kes
        if (ziplaRutin != null)
        {
            StopCoroutine(ziplaRutin);
        }

        ziplaRutin = StartCoroutine(Zipla());
    }

    /// <summary>Üzgün poz (zıplama yok).</summary>
    public void YanlisTepki()
    {
        if (AvatarYoneticisi.Instance != null)
        {
            AvatarYoneticisi.Instance.IfadeSec(2);
        }
        else
        {
            GorunumuYenile();
        }
    }

    /// <summary>Normal poza döner.</summary>
    public void NormalPoz()
    {
        if (AvatarYoneticisi.Instance != null)
        {
            AvatarYoneticisi.Instance.IfadeSec(0);
        }
        else if (gorunum != null)
        {
            gorunum.Guncelle();
        }

        // Zıplama ortasında kaldıysa tabana oturt
        transform.position = tabanKonum;
    }

    // Kısa sine zıplama
    private IEnumerator Zipla()
    {
        Vector3 bas = tabanKonum;
        float sure = Mathf.Max(0.05f, ziplamaSuresi);
        float t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / sure);
            // 0→1→0 parabola
            float yukari = Mathf.Sin(n * Mathf.PI) * ziplamaYuksekligi;
            transform.position = new Vector3(bas.x, bas.y + yukari, bas.z);
            yield return null;
        }

        transform.position = bas;
        ziplaRutin = null;
    }

    // Govde + Aksesuar + AvatarGorunumu
    private void HiyerarsiKur()
    {
        if (gorunum != null)
        {
            return;
        }

        Transform govdeT = transform.Find("Govde");
        if (govdeT == null)
        {
            GameObject govdeGo = new GameObject("Govde");
            govdeGo.transform.SetParent(transform, false);
            govdeT = govdeGo.transform;
            govdeGo.AddComponent<SpriteRenderer>();
        }

        Transform aksT = transform.Find("Aksesuar");
        if (aksT == null)
        {
            GameObject aksGo = new GameObject("Aksesuar");
            aksGo.transform.SetParent(transform, false);
            aksT = aksGo.transform;
            aksGo.AddComponent<SpriteRenderer>();
        }

        gorunum = GetComponent<AvatarGorunumu>();
        if (gorunum == null)
        {
            gorunum = gameObject.AddComponent<AvatarGorunumu>();
        }

        gorunum.govdeRenderer = govdeT.GetComponent<SpriteRenderer>();
        gorunum.aksesuarRenderer = aksT.GetComponent<SpriteRenderer>();
        gorunum.hedefGovdeYuksekligi = SablonGovdeYuksekligi;
        gorunum.hedefZeminY = 0f;
    }

    private void SablonuUygula()
    {
        if (gorunum == null)
        {
            return;
        }

        SablonuGarantiEt();
        if (AvatarGorunumu.SablonHazir)
        {
            gorunum.hayvanlar = AvatarGorunumu.SablonHayvanlar;
            gorunum.aksesuarlar = AvatarGorunumu.SablonAksesuarlar;
        }
    }

    private void SortingAyarla()
    {
        if (gorunum == null)
        {
            return;
        }

        if (gorunum.govdeRenderer != null)
        {
            gorunum.govdeRenderer.sortingOrder = sortingOrderGovde;
        }

        if (gorunum.aksesuarRenderer != null)
        {
            gorunum.aksesuarRenderer.sortingOrder = sortingOrderAksesuar;
        }
    }

    /// <summary>Gövde arkasına soft halo + hafif ambient sparkle.</summary>
    public void PariltiyiGarantiEt()
    {
        HiyerarsiKur();

        // Halo gövde child'ı olarak (ölçek karakterle birlikte)
        Transform govdeT = transform.Find("Govde");
        Transform haloParent = govdeT != null ? govdeT : transform;
        // Gövde sprite varsa onu saran beyaz halo; yoksa sabit ölçek
        SpriteRenderer govdeSr = gorunum != null ? gorunum.govdeRenderer : null;
        if (govdeSr != null && govdeSr.sprite != null)
        {
            YumusakPariltiYardimcisi.ObjeHalosuEkle(
                govdeSr,
                "KarakterPariltisi",
                1.35f,
                Color.white);
        }
        else
        {
            YumusakPariltiYardimcisi.WorldHaloEkle(
                haloParent,
                "KarakterPariltisi",
                5.2f,
                sortingOrderGovde - 1,
                Color.white,
                true,
                false);
        }

        // Sparkle kökte — karakteri boğmayan düşük rate
        OrtamPariltilari.GarantiEt(
            transform,
            "KarakterOrtamPariltilari",
            0.7f,
            3f,
            sortingOrderAksesuar + 1,
            false);
    }

    /// <summary>Şablon boşsa Editor'da Karakterler art'ından doldurur.</summary>
    public static void SablonuGarantiEt()
    {
        if (AvatarGorunumu.SablonHazir)
        {
            return;
        }

#if UNITY_EDITOR
        HayvanGorseli[] hayvanlar = new HayvanGorseli[4];
        hayvanlar[0] = HayvanSpriteYukle("Kedi", "avatar_kedi");
        hayvanlar[1] = HayvanSpriteYukle("Tavşan", "avatar_tavsan");
        hayvanlar[2] = HayvanSpriteYukle("Kuş", "avatar_kus");
        hayvanlar[3] = HayvanSpriteYukle("Rakun", "avatar_rakun");
        AvatarGorunumu.SablonHayvanlar = hayvanlar;
        AvatarGorunumu.SablonAksesuarlar = new AksesuarGorseli[0];
#endif
    }

#if UNITY_EDITOR
    private static HayvanGorseli HayvanSpriteYukle(string ad, string prefix)
    {
        string klasor = "Assets/_Art/Avatar/Karakterler";
        return new HayvanGorseli
        {
            hayvanAdi = ad,
            normalPoz = ArtSpriteYukle(klasor + "/" + prefix + "_normal.png"),
            mutluPoz = ArtSpriteYukle(klasor + "/" + prefix + "_mutlu.png"),
            uzgunPoz = ArtSpriteYukle(klasor + "/" + prefix + "_uzgun.png"),
            govdeOlcekCarpani = 1f,
            aksesuarOfset = new Vector2(0f, 1.8f)
        };
    }

    private static Sprite ArtSpriteYukle(string yol)
    {
        Object[] hepsi = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(yol);
        if (hepsi == null)
        {
            return null;
        }

        for (int i = 0; i < hepsi.Length; i++)
        {
            Sprite s = hepsi[i] as Sprite;
            if (s != null)
            {
                return s;
            }
        }

        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(yol);
    }
#endif
}
