using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ana ekranı kurar: gökyüzü, güneş, bulutlar ve büyük Oyna butonu.
/// Oyna, karakter seçim sahnesini açar.
/// </summary>
[DefaultExecutionOrder(-40)]
public class AnaEkranKurucu : MonoBehaviour
{
    // Oyna basılınca açılacak sahne
    public string karakterSahneAdi = "AvatarOlusturmaEkrani";

    // Gökyüzüne serilecek resimler
    public Sprite bulutSprite;
    public Sprite gunesSprite;

    static Sprite kareSprite;
    static Sprite yuvarlakKareSprite;

    void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        RectTransform kok = GetComponent<RectTransform>();
        kok.localScale = Vector3.one;
        kok.anchorMin = Vector2.zero;
        kok.anchorMax = Vector2.one;
        kok.offsetMin = Vector2.zero;
        kok.offsetMax = Vector2.zero;
        kok.pivot = new Vector2(0.5f, 0.5f);

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        EkraniKur();
    }

    /// <summary>Oyna butonu karakter ekranını açar.</summary>
    public void Oyna()
    {
        if (string.IsNullOrEmpty(karakterSahneAdi))
        {
            Debug.LogWarning("AnaEkranKurucu: karakterSahneAdi boş.");
            return;
        }

        SceneManager.LoadScene(karakterSahneAdi);
    }

    void EkraniKur()
    {
        if (bulutSprite == null)
            bulutSprite = Resources.Load<Sprite>("ana_bulut");
        if (gunesSprite == null)
            gunesSprite = Resources.Load<Sprite>("ana_gunes");

        // Gökyüzü: üst koyu mavi, alta doğru açılır
        GameObject gokyuzu = Panel(transform, "Gokyuzu", new Color(0.34f, 0.70f, 0.98f, 1f));
        Stretch(gokyuzu.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        GameObject ufuk = Panel(transform, "Ufuk", new Color(0.72f, 0.90f, 1f, 1f));
        Stretch(ufuk.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0.46f), Vector2.zero, Vector2.zero);

        // Yumuşak tepe
        GameObject tepe = Panel(transform, "Tepe", new Color(0.48f, 0.82f, 0.46f, 1f));
        RectTransform tepeRt = tepe.GetComponent<RectTransform>();
        tepeRt.anchorMin = new Vector2(0.5f, 0f);
        tepeRt.anchorMax = new Vector2(0.5f, 0f);
        tepeRt.pivot = new Vector2(0.5f, 0f);
        tepeRt.sizeDelta = new Vector2(2400f, 340f);
        tepeRt.anchoredPosition = new Vector2(0f, -80f);
        YuvarlakYap(tepe.GetComponent<Image>());

        GameObject cimen = Panel(transform, "Cimen", new Color(0.40f, 0.74f, 0.40f, 1f));
        Stretch(cimen.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0.08f), Vector2.zero, Vector2.zero);

        // Güneş sağ üstte, bulutların arkasında
        Dekor(transform, "Gunes", gunesSprite, new Vector2(1f, 1f), new Vector2(-210f, -190f), new Vector2(520f, 520f), 0f, false);

        // Arkadaki bulutlar
        Dekor(transform, "Bulut1", bulutSprite, new Vector2(0f, 1f), new Vector2(280f, -150f), new Vector2(460f, 300f), -6f, false);
        Dekor(transform, "Bulut2", bulutSprite, new Vector2(0f, 1f), new Vector2(620f, -280f), new Vector2(340f, 220f), 4f, true);
        Dekor(transform, "Bulut3", bulutSprite, new Vector2(1f, 0.5f), new Vector2(-120f, 80f), new Vector2(300f, 190f), 8f, false);

        // Ortadaki kart
        GameObject kart = Panel(transform, "Kart", new Color(1f, 1f, 1f, 0.96f));
        RectTransform kartRt = kart.GetComponent<RectTransform>();
        kartRt.anchorMin = new Vector2(0.5f, 0.5f);
        kartRt.anchorMax = new Vector2(0.5f, 0.5f);
        kartRt.pivot = new Vector2(0.5f, 0.5f);
        kartRt.sizeDelta = new Vector2(780f, 460f);
        kartRt.anchoredPosition = new Vector2(0f, 30f);
        YuvarlakYap(kart.GetComponent<Image>());
        GolgeEkle(kart, new Color(0.08f, 0.22f, 0.40f, 0.22f), new Vector2(0f, -18f));

        Text baslik = Yazi(kart.transform, "Baslik", "Okuma Yazma", 84, new Color(0.14f, 0.32f, 0.55f, 1f));
        RectTransform baslikRt = baslik.rectTransform;
        baslikRt.anchorMin = new Vector2(0.5f, 1f);
        baslikRt.anchorMax = new Vector2(0.5f, 1f);
        baslikRt.pivot = new Vector2(0.5f, 1f);
        baslikRt.sizeDelta = new Vector2(700f, 120f);
        baslikRt.anchoredPosition = new Vector2(0f, -48f);

        Button oyna = Buton(kart.transform, "OynaButon", new Vector2(520f, 148f), new Color(0.18f, 0.74f, 0.42f, 1f));
        RectTransform oynaRt = oyna.GetComponent<RectTransform>();
        oynaRt.anchorMin = new Vector2(0.5f, 0f);
        oynaRt.anchorMax = new Vector2(0.5f, 0f);
        oynaRt.pivot = new Vector2(0.5f, 0f);
        oynaRt.anchoredPosition = new Vector2(0f, 64f);
        YuvarlakYap(oyna.GetComponent<Image>());
        GolgeEkle(oyna.gameObject, new Color(0.08f, 0.32f, 0.16f, 0.35f), new Vector2(0f, -8f));
        Yazi(oyna.transform, "Yazi", "Oyna", 72, Color.white);
        oyna.onClick.AddListener(Oyna);

        // Öndeki bulutlar kenarda kalsın, butonu kapatmasın
        Dekor(transform, "Bulut4", bulutSprite, new Vector2(0f, 0.5f), new Vector2(40f, 40f), new Vector2(420f, 270f), -4f, true);
        Dekor(transform, "Bulut5", bulutSprite, new Vector2(0.5f, 1f), new Vector2(-40f, -40f), new Vector2(280f, 180f), 3f, false);
    }

    /// <summary>Saydam güneş veya bulut resmini ekrana koyar.</summary>
    static void Dekor(Transform parent, string ad, Sprite sprite, Vector2 anchor, Vector2 konum, Vector2 boyut, float aci, bool aynala)
    {
        if (sprite == null)
            return;

        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = boyut;
        rt.anchoredPosition = konum;
        rt.localEulerAngles = new Vector3(0f, 0f, aci);
        if (aynala)
            rt.localScale = new Vector3(-1f, 1f, 1f);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    static GameObject Panel(Transform parent, string ad, Color renk)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.sprite = KareSpriteAl();
        img.color = renk;
        return go;
    }

    static Button Buton(Transform parent, string ad, Vector2 boyut, Color renk)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = boyut;
        Image img = go.GetComponent<Image>();
        img.sprite = KareSpriteAl();
        img.color = renk;
        return go.GetComponent<Button>();
    }

    static Text Yazi(Transform parent, string ad, string metin, int punto, Color renk)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.text = metin;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = punto;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = renk;
        t.raycastTarget = false;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return t;
    }

    static void Stretch(RectTransform rt, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    static void YuvarlakYap(Image img)
    {
        img.sprite = YuvarlakKareSpriteAl();
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1.1f;
    }

    static void GolgeEkle(GameObject go, Color renk, Vector2 mesafe)
    {
        Shadow golge = go.AddComponent<Shadow>();
        golge.effectColor = renk;
        golge.effectDistance = mesafe;
        golge.useGraphicAlpha = true;
    }

    static Sprite KareSpriteAl()
    {
        if (kareSprite != null)
            return kareSprite;

        Texture2D t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] px = new Color[16];
        for (int i = 0; i < px.Length; i++)
            px[i] = Color.white;
        t.SetPixels(px);
        t.Apply();
        kareSprite = Sprite.Create(t, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        return kareSprite;
    }

    static Sprite YuvarlakKareSpriteAl()
    {
        if (yuvarlakKareSprite != null)
            return yuvarlakKareSprite;

        int s = 64;
        int r = 16;
        Texture2D t = new Texture2D(s, s, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float cx = Mathf.Clamp(x, r, s - 1 - r);
                float cy = Mathf.Clamp(y, r, s - 1 - r);
                float dx = x - cx;
                float dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                Color c = Color.white;
                if (d > r)
                    c = Color.clear;
                else if (d > r - 1.2f)
                    c.a = Mathf.Clamp01(r - d);
                t.SetPixel(x, y, c);
            }
        }
        t.Apply();
        yuvarlakKareSprite = Sprite.Create(
            t,
            new Rect(0f, 0f, s, s),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(r, r, r, r));
        return yuvarlakKareSprite;
    }
}
