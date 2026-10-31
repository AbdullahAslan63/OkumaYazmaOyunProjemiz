using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI seçenek balonu: obje resmi + tıklanınca doğru/yanlış bildirir.
/// Obje altında yumuşak beyaz bulut/parıltı; Mask ile balon dışına taşmaz.
/// </summary>
public class SecenekBalonu : MonoBehaviour
{
    public Image objeResmi;
    // Obje altındaki yumuşak beyaz bulut
    public Image objeArkaPlan;

    private char dogruHarf;
    private char aktifSoruHarfi;
    private Button buton;

    [Header("İç yerleşim — balon ZARFI (sepet değil)")]
    [Tooltip("Balon genişliğine göre obje kutusu (taşmayı önlemek için düşük tut)")]
    [Range(0.25f, 0.7f)]
    public float objeGenislikOrani = 0.46f;
    [Range(0.2f, 0.65f)]
    public float objeYukseklikOrani = 0.40f;
    [Tooltip("Pozitif = yukarı (zarf merkezi); negatif = sepete doğru")]
    [Range(-0.2f, 0.35f)]
    public float objeDikeyKayma = 0.14f;

    [Header("Obje altı bulut / parıltı")]
    [Range(1f, 1.35f)]
    public float arkaPlanOlcek = 1.2f;
    // Bir tık daha beyaz / yoğun
    public Color arkaPlanRengi = new Color(1f, 1f, 1f, 1f);

    // Paylaşılan yumuşak elips sprite (bir kez üretilir)
    private static Sprite yumusakBulutSprite;

    private void Awake()
    {
        buton = GetComponent<Button>();
        if (objeResmi == null)
        {
            Transform child = transform.Find("ObjeResmi");
            if (child != null)
                objeResmi = child.GetComponent<Image>();
        }

        // Çocukları balon sprite alfa sınırına kırp (dışa taşma yok)
        BalonMaskesiniGarantiEt();
        ObjeArkaPlaniniGarantiEt();
        YerlesimiDuzenle(false);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled)
            YerlesimiDuzenle(false);
    }

    /// <summary>Sprite ve harf bilgisini ayarlar; tıklamayı bağlar.</summary>
    public void Ayarla(Sprite resim, char objeHarfi, char soruHarfi)
    {
        dogruHarf = objeHarfi;
        aktifSoruHarfi = soruHarfi;

        if (objeResmi != null && resim != null)
        {
            objeResmi.sprite = resim;
            objeResmi.preserveAspect = true;
            objeResmi.type = Image.Type.Simple;
            objeResmi.raycastTarget = false;
        }

        // Sprite atandıktan sonra zarf kutusuna sığdır
        YerlesimiDuzenle(false);

        if (buton == null)
            buton = GetComponent<Button>();

        if (buton != null)
        {
            buton.onClick.RemoveAllListeners();
            buton.onClick.AddListener(Tiklandi);
            buton.interactable = true;
        }
    }

    /// <summary>
    /// Balon boyutunu slot ile eşler; objeyi sepet bölgesinde ortalar.
    /// pozisyonuSifirla=false iken yükseliş animasyonunun Y’sine dokunmaz.
    /// </summary>
    public void YerlesimiDuzenle(bool pozisyonuSifirla = false)
    {
        RectTransform kok = transform as RectTransform;
        if (kok == null) return;

        RectTransform slot = kok.parent as RectTransform;
        float alanW = 360f;
        float alanH = 430f;

        if (slot != null)
        {
            Canvas.ForceUpdateCanvases();
            Rect sr = slot.rect;
            if (sr.width > 1f) alanW = sr.width;
            if (sr.height > 1f) alanH = sr.height;

            kok.anchorMin = new Vector2(0.5f, 0.5f);
            kok.anchorMax = new Vector2(0.5f, 0.5f);
            kok.pivot = new Vector2(0.5f, 0.5f);
            kok.sizeDelta = new Vector2(alanW, alanH);
            kok.localScale = Vector3.one;
            if (pozisyonuSifirla)
                kok.anchoredPosition = Vector2.zero;
        }

        BalonMaskesiniGarantiEt();
        ObjeArkaPlaniniGarantiEt();

        if (objeResmi == null) return;

        RectTransform objeRt = objeResmi.rectTransform;
        objeRt.anchorMin = new Vector2(0.5f, 0.5f);
        objeRt.anchorMax = new Vector2(0.5f, 0.5f);
        objeRt.pivot = new Vector2(0.5f, 0.5f);
        objeRt.localScale = Vector3.one;
        objeRt.localRotation = Quaternion.identity;

        // Zarf içi kutu — sprite preserveAspect ile bu kutuya sığar, taşmaz
        float kutuW = alanW * objeGenislikOrani;
        float kutuH = alanH * objeYukseklikOrani;
        objeRt.sizeDelta = new Vector2(kutuW, kutuH);
        Vector2 objePoz = new Vector2(0f, alanH * objeDikeyKayma);
        objeRt.anchoredPosition = objePoz;

        objeResmi.preserveAspect = true;
        objeResmi.type = Image.Type.Simple;
        objeResmi.raycastTarget = false;

        // Sprite en-boy oranı kutuyu aşmasın: kutu içinde Fit
        if (objeResmi.sprite != null)
        {
            Sprite s = objeResmi.sprite;
            float sw = s.rect.width;
            float sh = s.rect.height;
            if (sw > 0.01f && sh > 0.01f)
            {
                float spriteOran = sw / sh;
                float kutuOran = kutuW / kutuH;
                if (spriteOran > kutuOran)
                    objeRt.sizeDelta = new Vector2(kutuW, kutuW / spriteOran);
                else
                    objeRt.sizeDelta = new Vector2(kutuH * spriteOran, kutuH);
            }
        }

        // Bulut: objeden biraz büyük, zarf kutusunu aşmaz
        if (objeArkaPlan != null)
        {
            RectTransform arkaRt = objeArkaPlan.rectTransform;
            arkaRt.anchorMin = new Vector2(0.5f, 0.5f);
            arkaRt.anchorMax = new Vector2(0.5f, 0.5f);
            arkaRt.pivot = new Vector2(0.5f, 0.5f);
            arkaRt.localScale = Vector3.one;
            arkaRt.localRotation = Quaternion.identity;
            arkaRt.anchoredPosition = objePoz;
            arkaRt.SetSiblingIndex(objeRt.GetSiblingIndex()); // objeden hemen önce

            float olcek = Mathf.Clamp(arkaPlanOlcek, 1f, 1.35f);
            float arkaW = Mathf.Min(objeRt.sizeDelta.x * olcek, kutuW * 1.05f);
            float arkaH = Mathf.Min(objeRt.sizeDelta.y * olcek, kutuH * 1.05f);
            // Hafif yatay bulut hissi
            arkaW = Mathf.Min(arkaW * 1.08f, kutuW * 1.08f);
            arkaRt.sizeDelta = new Vector2(arkaW, arkaH);

            objeArkaPlan.color = arkaPlanRengi;
            objeArkaPlan.raycastTarget = false;
            objeArkaPlan.maskable = true;
            objeResmi.maskable = true;
        }
    }

    /// <summary>Balon Image alfa maskesi — çocuklar balon dışına çizilmez.</summary>
    void BalonMaskesiniGarantiEt()
    {
        Mask mask = GetComponent<Mask>();
        if (mask == null)
            mask = gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;
    }

    /// <summary>ObjeResmi altında yumuşak beyaz bulut Image oluşturur.</summary>
    void ObjeArkaPlaniniGarantiEt()
    {
        if (objeArkaPlan != null) return;

        Transform mevcut = transform.Find("ObjeArkaPlan");
        if (mevcut != null)
        {
            objeArkaPlan = mevcut.GetComponent<Image>();
            if (objeArkaPlan != null) return;
        }

        GameObject go = new GameObject("ObjeArkaPlan", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        objeArkaPlan = go.GetComponent<Image>();
        objeArkaPlan.sprite = YumusakBulutSpriteAl();
        objeArkaPlan.type = Image.Type.Simple;
        objeArkaPlan.preserveAspect = false;
        objeArkaPlan.color = arkaPlanRengi;
        objeArkaPlan.raycastTarget = false;

        // ObjeResmi'nden önce (altta) dursun
        if (objeResmi != null)
            go.transform.SetSiblingIndex(objeResmi.transform.GetSiblingIndex());
        else
            go.transform.SetAsFirstSibling();
    }

    /// <summary>Yumuşak kenarlı beyaz elips (bulut/parıltı).</summary>
    static Sprite YumusakBulutSpriteAl()
    {
        if (yumusakBulutSprite != null) return yumusakBulutSprite;

        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float cx = (size - 1) * 0.5f;
        float cy = (size - 1) * 0.5f;
        float rx = cx * 0.98f;
        float ry = cy * 0.82f; // yatay bulut

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - cx) / rx;
                float ny = (y - cy) / ry;
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float a = 0f;
                if (d < 1f)
                {
                    // Merkez daha opak/beyaz, kenar hâlâ yumuşak
                    float t = 1f - d;
                    a = Mathf.Pow(t, 1.05f);
                    a = Mathf.Clamp01(a * 1.25f);
                }
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply(false, true);
        yumusakBulutSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f);
        yumusakBulutSprite.name = "ObjeBulutParilti";
        return yumusakBulutSprite;
    }

    public void SoruHarfiniGuncelle(char soruHarfi)
    {
        aktifSoruHarfi = soruHarfi;
    }

    public void Tiklandi()
    {
        if (HarfSecmeYoneticisi.Instance == null)
            return;

        bool dogruMu = dogruHarf == aktifSoruHarfi;
        // Doğruysa obje adı sesi için sprite iletilir
        Sprite resim = objeResmi != null ? objeResmi.sprite : null;
        HarfSecmeYoneticisi.Instance.SecenekSecildi(dogruMu, resim);
    }

    public void TiklamayiAyarla(bool acik)
    {
        if (buton == null)
            buton = GetComponent<Button>();
        if (buton != null)
            buton.interactable = acik;
    }
}
