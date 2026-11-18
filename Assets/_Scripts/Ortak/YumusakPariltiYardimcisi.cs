using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Prosedürel yumuşak halo sprite + world/UI çocuk ekleme.
/// Level 1/2 objeleri, harf ve OyunKarakteri aynı görsel dili paylaşır.
/// </summary>
public static class YumusakPariltiYardimcisi
{
    // Varsayılan saf beyaz soft halo
    public static readonly Color VarsayilanRenk = new Color(1f, 1f, 1f, 0.88f);

    // Önbellek: daire / elips / yıldız-halo
    private static Sprite daireSprite;
    private static Sprite elipsSprite;
    private static Sprite yildizHaloSprite;
    private static Sprite koniSprite;

    /// <summary>Yumuşak kenarlı daire (1 birim bounds).</summary>
    public static Sprite DaireSpriteAl()
    {
        if (daireSprite != null)
            return daireSprite;

        // Merkez daha opak — beyaz halo objeyi net sarsın
        daireSprite = SoftDiskOlustur(128, 1f, 1f, 1.45f);
        daireSprite.name = "PariltiDaire";
        return daireSprite;
    }

    /// <summary>Yatay yumuşak elips (obje altı bulut).</summary>
    public static Sprite ElipsSpriteAl()
    {
        if (elipsSprite != null)
            return elipsSprite;

        elipsSprite = SoftDiskOlustur(128, 0.98f, 0.82f, 1.25f);
        elipsSprite.name = "PariltiElips";
        return elipsSprite;
    }

    /// <summary>Merkez + yıldız kolları (harf arkası).</summary>
    public static Sprite YildizHaloSpriteAl()
    {
        if (yildizHaloSprite != null)
            return yildizHaloSprite;

        const int n = 128;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        float cx = (n - 1) * 0.5f;
        float cy = (n - 1) * 0.5f;
        float rMax = cx * 0.98f;

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / rMax;
                float a = 0f;
                if (d < 1f)
                {
                    float t = 1f - d;
                    a = Mathf.Pow(t, 1.4f) * 0.85f;
                    // 8 kollu yıldız
                    float aci = Mathf.Atan2(dy, dx);
                    float kol = Mathf.Abs(Mathf.Cos(aci * 4f));
                    a = Mathf.Clamp01(a + kol * Mathf.Pow(t, 2.2f) * 0.55f);
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply(false, true);
        yildizHaloSprite = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), n);
        yildizHaloSprite.name = "PariltiYildizHalo";
        return yildizHaloSprite;
    }

    /// <summary>Yukarıdan aşağıya açılan yumuşak ışık konisi (pivot üst orta).</summary>
    public static Sprite KoniSpriteAl()
    {
        if (koniSprite != null)
            return koniSprite;

        const int w = 64;
        const int h = 128;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            float v = y / (float)(h - 1); // 0 üst, 1 alt
            float yaricap = Mathf.Lerp(0.08f, 0.98f, v);
            float dikeyFade = Mathf.Pow(1f - v * 0.35f, 1.2f);
            for (int x = 0; x < w; x++)
            {
                float nx = (x / (float)(w - 1)) * 2f - 1f; // -1..1
                float a = 0f;
                float absNx = Mathf.Abs(nx);
                if (absNx <= yaricap)
                {
                    float kenar = 1f - (absNx / Mathf.Max(0.001f, yaricap));
                    a = Mathf.Pow(kenar, 1.3f) * dikeyFade * 0.55f;
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply(false, true);
        // Pivot üst orta — balondan aşağı doğru uzasın
        koniSprite = Sprite.Create(tex, new Rect(0f, 0f, w, h), new Vector2(0.5f, 1f), h);
        koniSprite.name = "PariltiIsikKonisi";
        return koniSprite;
    }

    /// <summary>World-space soft halo çocuğu ekler veya günceller.</summary>
    public static SpriteRenderer WorldHaloEkle(
        Transform parent,
        string ad,
        float yerelOlcek,
        int sortingOrder,
        Color? renk = null,
        bool nabiz = true,
        bool yildiz = false)
    {
        return WorldHaloEkle(
            parent,
            ad,
            Vector3.one * Mathf.Max(0.01f, yerelOlcek),
            Vector3.zero,
            sortingOrder,
            renk,
            nabiz,
            yildiz);
    }

    /// <summary>Ölçek + yerel konum ile world halo ekler.</summary>
    public static SpriteRenderer WorldHaloEkle(
        Transform parent,
        string ad,
        Vector3 yerelOlcek,
        Vector3 yerelKonum,
        int sortingOrder,
        Color? renk = null,
        bool nabiz = true,
        bool yildiz = false)
    {
        if (parent == null)
            return null;

        Transform eski = parent.Find(ad);
        GameObject go = eski != null ? eski.gameObject : new GameObject(ad);
        if (eski == null)
            go.transform.SetParent(parent, false);

        go.transform.localPosition = yerelKonum;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = yerelOlcek;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = go.AddComponent<SpriteRenderer>();

        sr.sprite = yildiz ? YildizHaloSpriteAl() : DaireSpriteAl();
        sr.color = renk ?? VarsayilanRenk;
        sr.sortingOrder = sortingOrder;

        if (nabiz)
        {
            PariltiNabzi nab = go.GetComponent<PariltiNabzi>();
            if (nab == null)
                nab = go.AddComponent<PariltiNabzi>();
            nab.TabanOlcegiKaydet(go.transform.localScale);
        }

        return sr;
    }

    /// <summary>
    /// SpriteRenderer boyutuna göre objeyi saran beyaz halo.
    /// padding: 1 = sprite sınırı, 1.25 = hafif taşma.
    /// </summary>
    public static SpriteRenderer ObjeHalosuEkle(
        SpriteRenderer hedef,
        string ad = "ObjePariltisi",
        float padding = 1.28f,
        Color? renk = null)
    {
        if (hedef == null || hedef.sprite == null)
            return null;

        Bounds b = hedef.sprite.bounds;
        float w = Mathf.Max(0.05f, b.size.x) * Mathf.Max(1.05f, padding);
        float h = Mathf.Max(0.05f, b.size.y) * Mathf.Max(1.05f, padding);
        int order = hedef.sortingOrder - 1;

        return WorldHaloEkle(
            hedef.transform,
            ad,
            new Vector3(w, h, 1f),
            b.center,
            order,
            renk ?? VarsayilanRenk,
            true,
            false);
    }

    /// <summary>UI Image soft halo çocuğu ekler veya günceller.</summary>
    public static Image UiHaloEkle(
        Transform parent,
        string ad,
        Vector2 sizeDelta,
        Vector2 anchoredPos,
        Color? renk = null,
        bool nabiz = true,
        bool elips = false,
        int siblingIndex = 0)
    {
        if (parent == null)
            return null;

        Transform eski = parent.Find(ad);
        GameObject go;
        if (eski != null)
        {
            go = eski.gameObject;
        }
        else
        {
            go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        Image img = go.GetComponent<Image>();
        img.sprite = elips ? ElipsSpriteAl() : DaireSpriteAl();
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.color = renk ?? VarsayilanRenk;
        img.raycastTarget = false;
        img.maskable = false;

        if (siblingIndex >= 0)
            go.transform.SetSiblingIndex(Mathf.Min(siblingIndex, parent.childCount - 1));

        if (nabiz)
        {
            PariltiNabzi nab = go.GetComponent<PariltiNabzi>();
            if (nab == null)
                nab = go.AddComponent<PariltiNabzi>();
            nab.TabanOlcegiKaydet(Vector3.one);
        }

        return img;
    }

    // Soft disk / elips texture
    private static Sprite SoftDiskOlustur(int size, float rxOran, float ryOran, float alphaCarpan)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        float cx = (size - 1) * 0.5f;
        float cy = (size - 1) * 0.5f;
        float rx = cx * rxOran;
        float ry = cy * ryOran;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - cx) / Mathf.Max(0.001f, rx);
                float ny = (y - cy) / Mathf.Max(0.001f, ry);
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float a = 0f;
                if (d < 1f)
                {
                    float t = 1f - d;
                    a = Mathf.Clamp01(Mathf.Pow(t, 1.1f) * alphaCarpan);
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

/// <summary>Halo üzerinde yavaş scale + alpha nabzı.</summary>
public class PariltiNabzi : MonoBehaviour
{
    private Vector3 tabanOlcek = Vector3.one;
    private SpriteRenderer spriteCizici;
    private Image uiCizici;
    private float faz;
    private Color tabanRenk = Color.white;
    private bool renkKayitli;

    private void Awake()
    {
        spriteCizici = GetComponent<SpriteRenderer>();
        uiCizici = GetComponent<Image>();
        faz = Random.Range(0f, Mathf.PI * 2f);
        RenkTabaniniKaydet();
    }

    /// <summary>Nabız ortası ölçeği kaydeder.</summary>
    public void TabanOlcegiKaydet(Vector3 olcek)
    {
        tabanOlcek = olcek;
        transform.localScale = olcek;
        RenkTabaniniKaydet();
    }

    private void RenkTabaniniKaydet()
    {
        if (spriteCizici != null)
            tabanRenk = spriteCizici.color;
        else if (uiCizici != null)
            tabanRenk = uiCizici.color;
        renkKayitli = true;
    }

    private void Update()
    {
        float nabiz = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.6f + faz);
        float carpan = Mathf.Lerp(0.92f, 1.08f, nabiz);
        transform.localScale = tabanOlcek * carpan;

        if (!renkKayitli)
            RenkTabaniniKaydet();

        float a = Mathf.Lerp(tabanRenk.a * 0.78f, tabanRenk.a, nabiz);
        Color c = new Color(tabanRenk.r, tabanRenk.g, tabanRenk.b, a);
        if (spriteCizici != null)
            spriteCizici.color = c;
        if (uiCizici != null)
            uiCizici.color = c;

        // Dünya yıldız-halo için hafif dönüş
        if (spriteCizici != null)
            transform.Rotate(0f, 0f, 12f * Time.deltaTime);
    }
}
