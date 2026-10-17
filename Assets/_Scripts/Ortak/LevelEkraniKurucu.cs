using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// LevelEkranı UI taslağını kurar (arka plan, banner, 3 kart, noktalar).
/// Play'de stub sahneyi doldurur; Editor menüsü aynı kurucuyu kullanır.
/// </summary>
[DefaultExecutionOrder(-40)]
public class LevelEkraniKurucu : MonoBehaviour
{
    public const string Kapak1Yol = "Assets/_Art/Leveller/seviye1_kapak.png";
    public const string Kapak2Yol = "Assets/_Art/Leveller/seviye2_kapak.png";
    public const string YildizYol = "Assets/_Art/Bolum2/UI/Dogru_Yildiz.png";
    public const string ArkaplanYol = "Assets/_Art/Bolum1/Arkaplan/arkaplan_adiyaman.png";
    public const string OkYol = "Assets/_Art/Avatar/UI/ui_icon_sag_ok.png";
    public const string FontYol = "Assets/Resources/Fonts/SoruFontu.asset";

    private static readonly Color KartKrem = new Color(0.96f, 0.90f, 0.78f, 1f);
    private static readonly Color KartCerceve = new Color(0.45f, 0.28f, 0.14f, 1f);
    private static readonly Color BannerMor = new Color(0.35f, 0.18f, 0.48f, 1f);
    private static readonly Color KilitMor = new Color(0.42f, 0.22f, 0.55f, 1f);
    private static readonly Color SariSecili = new Color(1f, 0.85f, 0.15f, 1f);
    private static readonly Color YaziKahve = new Color(0.35f, 0.20f, 0.10f, 1f);
    private static readonly Color YildizGri = new Color(0.55f, 0.55f, 0.55f, 0.7f);

    // Play'de bir kez kurulsun
    [System.NonSerialized] public bool kuruldu;

    void Awake()
    {
        if (kuruldu) return;
        // Tıklama sesi için ortak yönetici (clip yoksa sessiz)
        SesYoneticisi.GarantiEt();
        UiKur(transform);
        kuruldu = true;
    }

    /// <summary>Bu Canvas üzerinde seviye seçim UI'sini kurar (mevcut çocukları siler).</summary>
    public void UiKur(Transform canvasRoot)
    {
        if (canvasRoot == null) return;

        // Stub / eski çocukları temizle
        for (int i = canvasRoot.childCount - 1; i >= 0; i--)
            DestroySafe(canvasRoot.GetChild(i).gameObject);

        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        SeviyeSecimEkrani secim = canvasRoot.GetComponent<SeviyeSecimEkrani>();
        if (secim == null)
            secim = canvasRoot.gameObject.AddComponent<SeviyeSecimEkrani>();
        secim.geriSahneAdi = "AvatarOlusturmaEkrani";

        Sprite arkaSprite = SpriteYukle(ArkaplanYol);
        Sprite kapak1 = SpriteYukle(Kapak1Yol);
        Sprite kapak2 = SpriteYukle(Kapak2Yol);
        Sprite yildiz = SpriteYukle(YildizYol);
        Sprite okSprite = SpriteYukle(OkYol);
        TMP_FontAsset font = FontYukle();

        // Arka plan
        Image arka = ImageOlustur(canvasRoot, "Arkaplan", Vector2.zero, Vector2.zero);
        RectTransform arkaRt = arka.rectTransform;
        arkaRt.anchorMin = Vector2.zero;
        arkaRt.anchorMax = Vector2.one;
        arkaRt.offsetMin = Vector2.zero;
        arkaRt.offsetMax = Vector2.zero;
        if (arkaSprite != null)
        {
            arka.sprite = arkaSprite;
            arka.preserveAspect = false;
        }
        arka.color = new Color(0.75f, 0.75f, 0.8f, 1f);
        arka.raycastTarget = false;

        // Geri butonu
        Button geri = ButonOlustur(canvasRoot, "GeriButon", new Vector2(100f, -90f), new Vector2(96f, 96f));
        RectTransform geriRt = geri.GetComponent<RectTransform>();
        geriRt.anchorMin = new Vector2(0f, 1f);
        geriRt.anchorMax = new Vector2(0f, 1f);
        geriRt.pivot = new Vector2(0.5f, 0.5f);
        geriRt.anchoredPosition = new Vector2(100f, -90f);
        geri.GetComponent<Image>().color = SariSecili;
        if (okSprite != null)
        {
            Image okImg = ImageOlustur(geri.transform, "Ok", Vector2.zero, new Vector2(48f, 48f));
            okImg.sprite = okSprite;
            okImg.preserveAspect = true;
            okImg.color = new Color(0.35f, 0.18f, 0.35f, 1f);
            okImg.raycastTarget = false;
            okImg.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            TextMeshProUGUI okYazi = TmpOlustur(geri.transform, "OkYazi", "◀", 42f, Vector2.zero, new Vector2(80f, 80f));
            okYazi.color = new Color(0.35f, 0.18f, 0.35f, 1f);
            if (font != null) okYazi.font = font;
        }
        geri.onClick.AddListener(secim.GeriDon);
        TiklamaSesiEkle(geri.gameObject);

        // Banner
        Image banner = ImageOlustur(canvasRoot, "BaslikBanner", new Vector2(0f, -90f), new Vector2(420f, 88f));
        RectTransform bannerRt = banner.rectTransform;
        bannerRt.anchorMin = new Vector2(0.5f, 1f);
        bannerRt.anchorMax = new Vector2(0.5f, 1f);
        bannerRt.pivot = new Vector2(0.5f, 0.5f);
        bannerRt.anchoredPosition = new Vector2(0f, -90f);
        banner.color = BannerMor;
        TextMeshProUGUI baslik = TmpOlustur(banner.transform, "BaslikYazi", "Seviye Seç", 48f, Vector2.zero, new Vector2(400f, 80f));
        baslik.color = Color.white;
        baslik.fontStyle = FontStyles.Bold;
        if (font != null) baslik.font = font;

        // Kart satırı
        GameObject kartSatiri = new GameObject("KartSatiri", typeof(RectTransform));
        kartSatiri.transform.SetParent(canvasRoot, false);
        RectTransform satirRt = kartSatiri.GetComponent<RectTransform>();
        satirRt.anchorMin = new Vector2(0.5f, 0.5f);
        satirRt.anchorMax = new Vector2(0.5f, 0.5f);
        satirRt.anchoredPosition = new Vector2(0f, -20f);
        satirRt.sizeDelta = new Vector2(1600f, 620f);

        KartOlustur(kartSatiri.transform, "SeviyeKart1", new Vector2(-480f, 0f), new Vector2(380f, 520f),
            kapak1, yildiz, font, "Seviye 1", 3, false, false, secim, "SepetOyunu");
        KartOlustur(kartSatiri.transform, "SeviyeKart2", new Vector2(0f, 10f), new Vector2(420f, 560f),
            kapak2, yildiz, font, "Seviye 2", 1, true, false, secim, "HarfSecmeOyunu");
        KartOlustur(kartSatiri.transform, "SeviyeKart3", new Vector2(480f, 0f), new Vector2(380f, 520f),
            null, yildiz, font, "Seviye 3", 0, false, true, secim, null);

        // Sayfa noktaları
        GameObject noktalar = new GameObject("SayfaNoktalari", typeof(RectTransform));
        noktalar.transform.SetParent(canvasRoot, false);
        RectTransform noktaRt = noktalar.GetComponent<RectTransform>();
        noktaRt.anchorMin = new Vector2(0.5f, 0f);
        noktaRt.anchorMax = new Vector2(0.5f, 0f);
        noktaRt.pivot = new Vector2(0.5f, 0.5f);
        noktaRt.anchoredPosition = new Vector2(0f, 70f);
        noktaRt.sizeDelta = new Vector2(200f, 40f);
        for (int i = 0; i < 3; i++)
        {
            Image nokta = ImageOlustur(noktalar.transform, "Nokta" + (i + 1), new Vector2(-40f + i * 40f, 0f), new Vector2(22f, 22f));
            nokta.color = i == 0 ? SariSecili : new Color(0.75f, 0.75f, 0.75f, 1f);
            nokta.raycastTarget = false;
        }
    }

    private static void KartOlustur(
        Transform parent,
        string ad,
        Vector2 pos,
        Vector2 size,
        Sprite kapak,
        Sprite yildizSprite,
        TMP_FontAsset font,
        string baslikMetin,
        int yildizSayisi,
        bool secili,
        bool kilitli,
        SeviyeSecimEkrani secim,
        string hedefSahne)
    {
        GameObject kart = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        kart.transform.SetParent(parent, false);
        RectTransform rt = kart.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image kartImg = kart.GetComponent<Image>();
        kartImg.color = KartKrem;

        Outline cerceve = kart.AddComponent<Outline>();
        cerceve.effectColor = secili ? SariSecili : KartCerceve;
        cerceve.effectDistance = secili ? new Vector2(8f, -8f) : new Vector2(4f, -4f);

        if (secili)
        {
            Shadow golge = kart.AddComponent<Shadow>();
            golge.effectColor = new Color(1f, 0.85f, 0.15f, 0.55f);
            golge.effectDistance = new Vector2(0f, -6f);
        }

        Button buton = kart.GetComponent<Button>();

        Image kapakImg = ImageOlustur(kart.transform, "Kapak", Vector2.zero, new Vector2(size.x - 28f, size.y * 0.62f - 20f));
        RectTransform kapakRt = kapakImg.rectTransform;
        kapakRt.anchorMin = new Vector2(0.5f, 1f);
        kapakRt.anchorMax = new Vector2(0.5f, 1f);
        kapakRt.pivot = new Vector2(0.5f, 1f);
        kapakRt.anchoredPosition = new Vector2(0f, -14f);
        kapakImg.raycastTarget = false;

        if (kilitli)
        {
            kapakImg.color = KilitMor;
            // Sarı kilit gövdesi (emoji fontuna bağımlı olmasın)
            Image kilitGovde = ImageOlustur(kapakImg.transform, "KilitGovde", Vector2.zero, new Vector2(100f, 120f));
            kilitGovde.color = SariSecili;
            kilitGovde.raycastTarget = false;
            TextMeshProUGUI kilitYazi = TmpOlustur(kapakImg.transform, "KilitYazi", "Kilit", 40f, new Vector2(0f, -100f), new Vector2(200f, 50f));
            kilitYazi.color = SariSecili;
            kilitYazi.fontStyle = FontStyles.Bold;
            if (font != null) kilitYazi.font = font;
            buton.interactable = false;
        }
        else if (kapak != null)
        {
            kapakImg.sprite = kapak;
            kapakImg.preserveAspect = true;
            kapakImg.color = Color.white;
            string sahne = hedefSahne;
            buton.onClick.AddListener(() => secim.SeviyeAc(sahne));
            TiklamaSesiEkle(kart);
        }

        TextMeshProUGUI baslik = TmpOlustur(
            kart.transform, "KartBaslik", baslikMetin, 36f,
            new Vector2(0f, -size.y * 0.22f), new Vector2(size.x - 20f, 48f));
        baslik.color = YaziKahve;
        baslik.fontStyle = FontStyles.Bold;
        if (font != null) baslik.font = font;

        GameObject yildizSatir = new GameObject("Yildizlar", typeof(RectTransform));
        yildizSatir.transform.SetParent(kart.transform, false);
        RectTransform ysRt = yildizSatir.GetComponent<RectTransform>();
        ysRt.anchoredPosition = new Vector2(0f, -size.y * 0.38f);
        ysRt.sizeDelta = new Vector2(200f, 56f);

        for (int i = 0; i < 3; i++)
        {
            Image yi = ImageOlustur(yildizSatir.transform, "Yildiz" + (i + 1), new Vector2(-60f + i * 60f, 0f), new Vector2(48f, 48f));
            if (yildizSprite != null)
            {
                yi.sprite = yildizSprite;
                yi.preserveAspect = true;
            }
            yi.color = i < yildizSayisi ? Color.white : YildizGri;
            yi.raycastTarget = false;
        }
    }

    private static Sprite SpriteYukle(string path)
    {
#if UNITY_EDITOR
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] is Sprite s) return s;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
        // Build'de Resources altına kopyalanmış olabilir
        string ad = System.IO.Path.GetFileNameWithoutExtension(path);
        return Resources.Load<Sprite>("Leveller/" + ad);
#endif
    }

    private static TMP_FontAsset FontYukle()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontYol);
#else
        return Resources.Load<TMP_FontAsset>("Fonts/SoruFontu");
#endif
    }

    private static Image ImageOlustur(Transform parent, string ad, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go.GetComponent<Image>();
    }

    private static Button ButonOlustur(Transform parent, string ad, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = Color.white;
        return go.GetComponent<Button>();
    }

    private static TextMeshProUGUI TmpOlustur(
        Transform parent, string ad, string metin, float size, Vector2 pos, Vector2 kutu)
    {
        GameObject go = new GameObject(ad);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = metin;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        RectTransform rt = tmp.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = kutu;
        return tmp;
    }

    private static void TiklamaSesiEkle(GameObject go)
    {
        if (go.GetComponent<ButonTiklamaSesi>() == null)
            go.AddComponent<ButonTiklamaSesi>();
    }

    private static void DestroySafe(GameObject go)
    {
        if (go == null) return;
        // Awake içinde yeniden kurarken ertelenmiş Destroy yetmez
        Object.DestroyImmediate(go);
    }
}
