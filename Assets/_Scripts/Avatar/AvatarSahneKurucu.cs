using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Avatar seçim UI'sini Roblox tipi dolap düzenine kurar:
/// solda karakter / renk / aksesuar sekmeleri, sağda 2D karakter.
/// Mevcut hayvan ve aksesuar listelerine dokunmaz.
/// </summary>
[DefaultExecutionOrder(-50)]
public class AvatarSahneKurucu : MonoBehaviour
{
    public AvatarGorunumu avatarGorunumu;

    // Paylaşılan basit kare / daire sprite'ları (kodda bir kez üretilir)
    private static Sprite kareSprite;
    private static Sprite daireSprite;

    void Awake()
    {
        if (avatarGorunumu == null)
            avatarGorunumu = Object.FindAnyObjectByType<AvatarGorunumu>();

        EventSystemiGarantiEt();

        AvatarSecimEkrani mevcut = Object.FindAnyObjectByType<AvatarSecimEkrani>();
        if (mevcut != null)
        {
            MevcutCanvasaDolapUygula(mevcut);
            return;
        }

        if (avatarGorunumu != null && (avatarGorunumu.hayvanlar == null || avatarGorunumu.hayvanlar.Length == 0))
            Debug.LogWarning("AvatarSahneKurucu: hayvanlar listesi boş — Inspector'dan sprite ata veya Editor menüsünü çalıştır.");

        GameObject canvasGo = new GameObject("AvatarCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        AvatarSecimEkrani secim = canvasGo.AddComponent<AvatarSecimEkrani>();
        secim.avatarGorunumu = avatarGorunumu;
        secim.sonrakiSahneAdi = "HarfSecmeOyunu";

        SolPaneliKur(secim, avatarGorunumu);
        DolapArkaPlaniniKur();
        AvatariSagaAl(avatarGorunumu);
    }

    /// <summary>
    /// Sahnedeki eski alt-bar Canvas'ı Roblox tipi dolap düzenine çevirir.
    /// Hayvan / aksesuar verisini silmez.
    /// </summary>
    public static void MevcutCanvasaDolapUygula(AvatarSecimEkrani secim)
    {
        if (secim == null)
            return;

        // Zaten yeni düzen varsa tekrar kurma
        if (secim.transform.Find("SolPanel") != null)
            return;

        CanvasScaler scaler = secim.GetComponent<CanvasScaler>();
        if (scaler != null)
            scaler.matchWidthOrHeight = 0.5f;

        AvatarGorunumu gorunum = secim.avatarGorunumu;
        if (gorunum == null)
            gorunum = Object.FindAnyObjectByType<AvatarGorunumu>();
        secim.avatarGorunumu = gorunum;

        // Eski geri/ileri/renk/aksesuar çocuklarını sil (script canvas'ta kalır)
        for (int i = secim.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(secim.transform.GetChild(i).gameObject);

        SolPaneliKur(secim, gorunum);
        DolapArkaPlaniniKur();
        AvatariSagaAl(gorunum);
    }

    /// <summary>Karakteri sağ yarıya alır; dolabın önünde dursun.</summary>
    static void AvatariSagaAl(AvatarGorunumu gorunum)
    {
        if (gorunum == null)
            return;

        gorunum.transform.position = new Vector3(4.15f, 0.35f, 0f);

        if (gorunum.govdeRenderer != null)
            gorunum.govdeRenderer.sortingOrder = 10;
        if (gorunum.aksesuarRenderer != null)
            gorunum.aksesuarRenderer.sortingOrder = 11;
    }

    /// <summary>Sağ tarafta 2D giyinme dolabı dekorunu kurar.</summary>
    static void DolapArkaPlaniniKur()
    {
        if (GameObject.Find("DolapSahne") != null)
            return;

        GameObject kok = new GameObject("DolapSahne");
        Sprite kare = KareSpriteAl();
        Sprite daire = DaireSpriteAl();

        // Krem oda duvarı (yalnızca sağ yarı — sol paneli örtmesin)
        SpriteObje(kok.transform, "Duvar", kare, new Color(0.99f, 0.92f, 0.76f), new Vector3(5.05f, 0.3f, 0f), new Vector3(8.6f, 12f, 1f), -40);

        // Ahşap zemin
        SpriteObje(kok.transform, "Zemin", kare, new Color(0.72f, 0.50f, 0.32f), new Vector3(5.05f, -4.35f, 0f), new Vector3(8.6f, 2.4f, 1f), -39);

        // Karakterin arkasında yumuşak ışık
        SpriteObje(kok.transform, "Isik", daire, new Color(1f, 0.98f, 0.88f, 0.55f), new Vector3(4.15f, 0.8f, 0f), new Vector3(4.8f, 6.2f, 1f), -38);

        // Halı
        SpriteObje(kok.transform, "Hali", daire, new Color(0.95f, 0.80f, 0.74f), new Vector3(4.15f, -2.55f, 0f), new Vector3(5.2f, 1.85f, 1f), -37);

        // Halı iç halka
        SpriteObje(kok.transform, "HaliIc", daire, new Color(0.99f, 0.94f, 0.88f), new Vector3(4.15f, -2.55f, 0f), new Vector3(3.1f, 1.1f, 1f), -37);

        // Sağ raf
        SpriteObje(kok.transform, "RafSag", kare, new Color(0.90f, 0.74f, 0.52f), new Vector3(7.55f, 1.4f, 0f), new Vector3(2.2f, 7.2f, 1f), -36);

        // Raf rafları
        SpriteObje(kok.transform, "RafCizgi1", kare, new Color(0.80f, 0.62f, 0.42f), new Vector3(7.55f, 3.4f, 0f), new Vector3(2.0f, 0.12f, 1f), -35);
        SpriteObje(kok.transform, "RafCizgi2", kare, new Color(0.80f, 0.62f, 0.42f), new Vector3(7.55f, 1.6f, 0f), new Vector3(2.0f, 0.12f, 1f), -35);
        SpriteObje(kok.transform, "RafCizgi3", kare, new Color(0.80f, 0.62f, 0.42f), new Vector3(7.55f, -0.2f, 0f), new Vector3(2.0f, 0.12f, 1f), -35);

        // Sol dolap gövdesi (karakterin arkasında, ince)
        SpriteObje(kok.transform, "DolapSol", kare, new Color(0.93f, 0.80f, 0.58f), new Vector3(1.55f, 1.1f, 0f), new Vector3(1.35f, 6.4f, 1f), -36);

        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = new Color(0.97f, 0.90f, 0.74f, 1f);
    }

    /// <summary>Soldaki katalog paneli + sekmeleri + seçenek ızgaralarını kurar.</summary>
    static void SolPaneliKur(AvatarSecimEkrani secim, AvatarGorunumu gorunum)
    {
        Transform canvas = secim.transform;

        GameObject sol = Panel(canvas, "SolPanel", new Color(1f, 1f, 1f, 0.97f));
        Stretch(sol.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.48f, 1f), new Vector2(28f, 28f), new Vector2(-18f, -28f));

        GameObject kenar = Panel(sol.transform, "Kenar", new Color(0.88f, 0.88f, 0.90f, 1f));
        Stretch(kenar.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-8f, 0f), Vector2.zero);

        // Üst sekme çubuğu
        GameObject sekmeCubugu = Panel(sol.transform, "SekmeCubugu", new Color(0.96f, 0.96f, 0.97f, 1f));
        RectTransform sekmeRt = sekmeCubugu.GetComponent<RectTransform>();
        Stretch(sekmeRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -104f), new Vector2(-16f, -16f));
        HorizontalLayoutGroup sekmeLay = sekmeCubugu.AddComponent<HorizontalLayoutGroup>();
        sekmeLay.spacing = 10f;
        sekmeLay.padding = new RectOffset(10, 10, 10, 10);
        sekmeLay.childAlignment = TextAnchor.MiddleCenter;
        sekmeLay.childForceExpandWidth = true;
        sekmeLay.childForceExpandHeight = true;

        Button karakterSekme = SekmeButon(sekmeCubugu.transform, "KarakterSekme", "Karakter");
        Button renkSekme = SekmeButon(sekmeCubugu.transform, "RenkSekme", "Renk");
        Button aksesuarSekme = SekmeButon(sekmeCubugu.transform, "AksesuarSekme", "Aksesuar");

        karakterSekme.onClick.AddListener(secim.KarakterSekmesiniAc);
        renkSekme.onClick.AddListener(secim.RenkSekmesiniAc);
        aksesuarSekme.onClick.AddListener(secim.AksesuarSekmesiniAc);

        // İçerik alanı (sekme ile değişen paneller)
        GameObject icerik = new GameObject("IcerikAlani", typeof(RectTransform));
        icerik.transform.SetParent(sol.transform, false);
        Stretch(icerik.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 140f), new Vector2(-20f, -120f));

        GameObject karakterPaneli = IzgaraPanel(icerik.transform, "KarakterPaneli", new Vector2(210f, 240f), 18f);
        GameObject renkPaneli = IzgaraPanel(icerik.transform, "RenkPaneli", new Vector2(110f, 110f), 20f);
        GameObject aksesuarKoku = ScrollIzgara(icerik.transform, "AksesuarPaneli", new Vector2(190f, 190f), 16f);
        Transform aksesuarIcerikT = aksesuarKoku.transform.Find("Viewport/Icerik");
        Transform aksesuarParent = aksesuarIcerikT != null ? aksesuarIcerikT : aksesuarKoku.transform;

        KarakterKartlariniKur(karakterPaneli.transform, gorunum);
        RenkKartlariniKur(renkPaneli.transform, gorunum);
        AksesuarKartlariniKur(aksesuarParent, gorunum);

        secim.karakterPaneli = karakterPaneli;
        secim.renkPaneli = renkPaneli;
        secim.aksesuarPaneli = aksesuarKoku;
        secim.karakterSekmeButon = karakterSekme;
        secim.renkSekmeButon = renkSekme;
        secim.aksesuarSekmeButon = aksesuarSekme;

        // Devam butonu sol panelin altında
        Button devam = Buton(sol.transform, "DevamButon", Vector2.zero, new Vector2(320f, 88f), new Color(0.30f, 0.76f, 0.46f));
        RectTransform devamRt = devam.GetComponent<RectTransform>();
        devamRt.anchorMin = new Vector2(0.5f, 0f);
        devamRt.anchorMax = new Vector2(0.5f, 0f);
        devamRt.pivot = new Vector2(0.5f, 0f);
        devamRt.anchoredPosition = new Vector2(0f, 28f);
        TextEkle(devam.transform, "Devam", 40, Color.white);
        devam.onClick.AddListener(secim.DevamEt);

        // Açılışta karakter sekmesi görünsün
        secim.KarakterSekmesiniAc();
    }

    /// <summary>Inspector'daki hayvan listesinden kart üretir; listeyi değiştirmez.</summary>
    static void KarakterKartlariniKur(Transform parent, AvatarGorunumu gorunum)
    {
        if (gorunum == null || gorunum.hayvanlar == null)
            return;

        for (int i = 0; i < gorunum.hayvanlar.Length; i++)
        {
            HayvanGorseli hayvan = gorunum.hayvanlar[i];
            if (hayvan == null)
                continue;

            GameObject kart = Kart(parent, "Karakter" + i);
            Image kapak = kart.GetComponent<Image>();
            kapak.color = new Color(0.94f, 0.95f, 0.97f, 1f);

            GameObject resimGo = new GameObject("Resim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            resimGo.transform.SetParent(kart.transform, false);
            RectTransform resimRt = resimGo.GetComponent<RectTransform>();
            Stretch(resimRt, new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.96f), Vector2.zero, Vector2.zero);
            Image resim = resimGo.GetComponent<Image>();
            resim.sprite = hayvan.normalPoz;
            resim.preserveAspect = true;
            resim.color = Color.white;
            resim.raycastTarget = false;

            GameObject yaziGo = new GameObject("Ad", typeof(RectTransform));
            yaziGo.transform.SetParent(kart.transform, false);
            Stretch(yaziGo.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0.22f), Vector2.zero, Vector2.zero);
            string ad = string.IsNullOrEmpty(hayvan.hayvanAdi) ? ("Karakter " + (i + 1)) : hayvan.hayvanAdi;
            TextEkle(yaziGo.transform, ad, 26, new Color(0.16f, 0.16f, 0.18f));

            Button btn = kart.GetComponent<Button>();
            HayvanSecici secici = kart.AddComponent<HayvanSecici>();
            secici.avatarGorunumu = gorunum;
            secici.hayvanIndex = i;
            btn.onClick.AddListener(secici.BuHayvaniSec);
        }
    }

    /// <summary>Eski 4 rengi aynı sırayla renk kartı olarak ekler.</summary>
    static void RenkKartlariniKur(Transform parent, AvatarGorunumu gorunum)
    {
        Color[] renkler =
        {
            Color.white,
            new Color(1f, 0.55f, 0.2f),
            new Color(1f, 0.6f, 0.8f),
            new Color(0.4f, 0.75f, 1f)
        };

        for (int i = 0; i < renkler.Length; i++)
        {
            Button rb = Buton(parent, "Renk" + i, Vector2.zero, new Vector2(100f, 100f), Color.white);
            Image img = rb.GetComponent<Image>();
            img.sprite = DaireSpriteAl();
            img.color = renkler[i];
            img.preserveAspect = true;

            // Beyaz daireyi ayırt etmek için ince çerçeve
            Outline cerceve = rb.gameObject.AddComponent<Outline>();
            cerceve.effectColor = new Color(0.75f, 0.75f, 0.78f, 1f);
            cerceve.effectDistance = new Vector2(3f, -3f);

            RenkSecici rs = rb.gameObject.AddComponent<RenkSecici>();
            rs.avatarGorunumu = gorunum;
            rs.builtinRenk = renkler[i];
            rb.onClick.AddListener(rs.BuButonunRenginiSec);
        }
    }

    /// <summary>Inspector'daki aksesuar listesini kart olarak ekler; ayarları değiştirmez.</summary>
    static void AksesuarKartlariniKur(Transform parent, AvatarGorunumu gorunum)
    {
        // Yok kartı
        GameObject yokKart = Kart(parent, "AksesuarYok");
        yokKart.GetComponent<Image>().color = new Color(0.90f, 0.90f, 0.92f, 1f);
        TextEkle(yokKart.transform, "Yok", 32, new Color(0.2f, 0.2f, 0.22f));
        AksesuarSecici yokSec = yokKart.AddComponent<AksesuarSecici>();
        yokSec.avatarGorunumu = gorunum;
        yokSec.aksesuarIndex = -1;
        yokKart.GetComponent<Button>().onClick.AddListener(yokSec.BuAksesuariSec);

        if (gorunum == null || gorunum.aksesuarlar == null)
            return;

        for (int i = 0; i < gorunum.aksesuarlar.Length; i++)
        {
            AksesuarGorseli aks = gorunum.aksesuarlar[i];
            GameObject kart = Kart(parent, "Aksesuar" + i);
            kart.GetComponent<Image>().color = new Color(0.94f, 0.95f, 0.97f, 1f);

            if (aks != null && aks.sprite != null)
            {
                GameObject resimGo = new GameObject("Resim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                resimGo.transform.SetParent(kart.transform, false);
                Stretch(resimGo.GetComponent<RectTransform>(), new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);
                Image resim = resimGo.GetComponent<Image>();
                resim.sprite = aks.sprite;
                resim.preserveAspect = true;
                resim.color = Color.white;
                resim.raycastTarget = false;
            }

            AksesuarSecici asec = kart.AddComponent<AksesuarSecici>();
            asec.avatarGorunumu = gorunum;
            asec.aksesuarIndex = i;
            kart.GetComponent<Button>().onClick.AddListener(asec.BuAksesuariSec);
        }
    }

    static GameObject IzgaraPanel(Transform parent, string ad, Vector2 hucre, float bosluk)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0f);
        img.raycastTarget = false;

        GridLayoutGroup grid = go.AddComponent<GridLayoutGroup>();
        grid.cellSize = hucre;
        grid.spacing = new Vector2(bosluk, bosluk);
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        return go;
    }

    static GameObject ScrollIzgara(Transform parent, string ad, Vector2 hucre, float bosluk)
    {
        GameObject scrollGo = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
        scrollGo.transform.SetParent(parent, false);
        Stretch(scrollGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image bg = scrollGo.GetComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0f);
        bg.raycastTarget = true;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGo.transform, false);
        Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image vpImg = viewport.GetComponent<Image>();
        vpImg.color = Color.white;
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject icerik = new GameObject("Icerik", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        icerik.transform.SetParent(viewport.transform, false);
        RectTransform icerikRt = icerik.GetComponent<RectTransform>();
        icerikRt.anchorMin = new Vector2(0f, 1f);
        icerikRt.anchorMax = new Vector2(1f, 1f);
        icerikRt.pivot = new Vector2(0.5f, 1f);
        icerikRt.anchoredPosition = Vector2.zero;
        icerikRt.sizeDelta = new Vector2(0f, 0f);

        GridLayoutGroup grid = icerik.GetComponent<GridLayoutGroup>();
        grid.cellSize = hucre;
        grid.spacing = new Vector2(bosluk, bosluk);
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        ContentSizeFitter fitter = icerik.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = icerikRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        return scrollGo;
    }

    static Button SekmeButon(Transform parent, string ad, string yazi)
    {
        Button btn = Buton(parent, ad, Vector2.zero, new Vector2(180f, 64f), new Color(0.93f, 0.93f, 0.95f, 1f));
        LayoutElement le = btn.gameObject.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minHeight = 64f;
        TextEkle(btn.transform, yazi, 30, new Color(0.18f, 0.18f, 0.2f));
        return btn;
    }

    static GameObject Kart(Transform parent, string ad)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = Color.white;
        return go;
    }

    static GameObject Panel(Transform parent, string ad, Color renk)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = renk;
        img.sprite = KareSpriteAl();
        img.type = Image.Type.Simple;
        return go;
    }

    static Button Buton(Transform parent, string ad, Vector2 pos, Vector2 size, Color renk)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.GetComponent<Image>();
        img.color = renk;
        img.sprite = KareSpriteAl();
        return go.GetComponent<Button>();
    }

    static Text TextEkle(Transform parent, string metin, int punto, Color renk)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.text = metin;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = renk;
        t.fontSize = punto;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

    static void SpriteObje(Transform parent, string ad, Sprite sprite, Color renk, Vector3 pos, Vector3 olcek, int sira)
    {
        GameObject go = new GameObject(ad);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = olcek;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = renk;
        sr.sortingOrder = sira;
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
        t.filterMode = FilterMode.Bilinear;
        kareSprite = Sprite.Create(t, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        kareSprite.name = "KareSprite";
        return kareSprite;
    }

    static Sprite DaireSpriteAl()
    {
        if (daireSprite != null)
            return daireSprite;

        int s = 64;
        Texture2D t = new Texture2D(s, s, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Bilinear;
        float c = (s - 1) * 0.5f;
        float r = c - 1f;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x - c;
                float dy = y - c;
                t.SetPixel(x, y, (dx * dx + dy * dy) <= r * r ? Color.white : Color.clear);
            }
        }
        t.Apply();
        daireSprite = Sprite.Create(t, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), s);
        daireSprite.name = "DaireSprite";
        return daireSprite;
    }

    static void EventSystemiGarantiEt()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
            return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }
}
