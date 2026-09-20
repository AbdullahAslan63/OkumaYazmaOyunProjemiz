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

    // Paylaşılan basit kare / daire / yuvarlak kare sprite'ları
    private static Sprite kareSprite;
    private static Sprite daireSprite;
    private static Sprite yuvarlakKareSprite;

    // Dolap görselinin dünya merkezi — karakter halıya hizalansın
    private static Vector3 dolapMerkez;
    private static float dolapYukseklik;

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

        EventSystemiGarantiEt();

        // Play'de sahneye kayıtlı butonların OnClick'i boş kalır — her Play'de bir kez yeniden kur
        if (Application.isPlaying)
        {
            if (secim.playDuzeniKuruldu)
                return;
            secim.playDuzeniKuruldu = true;
        }
        else
        {
            bool eskiOkVar =
                secim.transform.Find("GeriButon") != null ||
                secim.transform.Find("IleriButon") != null ||
                secim.transform.Find("RenkPaneli") != null;
            bool solVar = secim.transform.Find("SolPanel") != null;
            if (solVar && !eskiOkVar)
                return;
        }

        CanvasScaler scaler = secim.GetComponent<CanvasScaler>();
        if (scaler != null)
            scaler.matchWidthOrHeight = 0.5f;

        AvatarGorunumu gorunum = secim.avatarGorunumu;
        if (gorunum == null)
            gorunum = Object.FindAnyObjectByType<AvatarGorunumu>();
        secim.avatarGorunumu = gorunum;

        // Eski çocukları hemen gizle, sonra sil (Play'de oklar bir kare bile görünmesin)
        for (int i = secim.transform.childCount - 1; i >= 0; i--)
        {
            GameObject cocuk = secim.transform.GetChild(i).gameObject;
            cocuk.SetActive(false);
            Object.DestroyImmediate(cocuk);
        }

        SolPaneliKur(secim, gorunum);
        DolapArkaPlaniniKur();
        AvatariSagaAl(gorunum);
    }

    /// <summary>Karakteri sağ yarıya alır; dolabın önünde dursun.</summary>
    static void AvatariSagaAl(AvatarGorunumu gorunum)
    {
        if (gorunum == null)
            return;

        if (dolapYukseklik < 0.1f)
            dolapYukseklik = 10f;
        if (dolapMerkez == Vector3.zero)
            dolapMerkez = new Vector3(4.2f, 0f, 0f);

        // Ayaklar halının / ışık lekesinin ortasında dursun
        float haliY = dolapMerkez.y - dolapYukseklik * 0.5f + dolapYukseklik * 0.20f;
        float zemin = gorunum.hedefZeminY;
        gorunum.transform.position = new Vector3(dolapMerkez.x, haliY - zemin - 1.35f, 0f);

        if (gorunum.govdeRenderer != null)
            gorunum.govdeRenderer.sortingOrder = 10;
        if (gorunum.aksesuarRenderer != null)
            gorunum.aksesuarRenderer.sortingOrder = 11;
    }

    /// <summary>Sağ tarafta giyinme odası görselini arka plan yapar.</summary>
    static void DolapArkaPlaniniKur()
    {
        GameObject eski = GameObject.Find("DolapSahne");
        if (eski != null)
            Object.DestroyImmediate(eski);

        GameObject kok = new GameObject("DolapSahne");
        Sprite dolap = DolapSpriteYukle();

        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = new Color(0.96f, 0.97f, 0.98f, 1f);

        if (dolap == null)
        {
            Debug.LogWarning("AvatarSahneKurucu: dolap_arkaplan yüklenemedi. Resources/dolap_arkaplan kontrol et.");
            return;
        }

        // Kameranın gördüğü dünya boyutu
        float yukseklik = 10f;
        float genislik = 16f;
        if (cam != null && cam.orthographic)
        {
            yukseklik = cam.orthographicSize * 2f;
            genislik = yukseklik * cam.aspect;
        }

        // Sol katalog %48 — görsel biraz alta girsin ki birleşim çizgisi görünmesin
        float solOran = 0.46f;
        float hedefW = genislik * (1f - solOran);
        float hedefH = yukseklik;
        float merkezX = -genislik * 0.5f + genislik * solOran + hedefW * 0.5f;
        float merkezY = 0f;

        // Görseli sağ paneli kaplayacak şekilde ölçekle (en-boy bozulmasın)
        float spriteW = Mathf.Max(0.001f, dolap.bounds.size.x);
        float spriteH = Mathf.Max(0.001f, dolap.bounds.size.y);
        float olcek = Mathf.Max(hedefW / spriteW, hedefH / spriteH);

        dolapMerkez = new Vector3(merkezX, merkezY, 0f);
        dolapYukseklik = spriteH * olcek;
        SpriteObje(kok.transform, "DolapArkaPlan", dolap, Color.white, dolapMerkez, new Vector3(olcek, olcek, 1f), -50);
    }

    /// <summary>Giyinme odası sprite'ını Resources'tan yükler.</summary>
    static Sprite DolapSpriteYukle()
    {
        Sprite sprite = Resources.Load<Sprite>("dolap_arkaplan");
        if (sprite != null)
            return sprite;

        Texture2D tex = Resources.Load<Texture2D>("dolap_arkaplan");
        if (tex == null)
            return null;

        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>Soldaki katalog paneli + sekmeleri + seçenek ızgaralarını kurar.</summary>
    static void SolPaneliKur(AvatarSecimEkrani secim, AvatarGorunumu gorunum)
    {
        Transform canvas = secim.transform;

        GameObject sol = Panel(canvas, "SolPanel", new Color(0.94f, 0.95f, 0.98f, 1f));
        Stretch(sol.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.48f, 1f), Vector2.zero, Vector2.zero);
        YuvarlakYap(sol.GetComponent<Image>(), true);
        GolgeEkle(sol, new Color(0.04f, 0.05f, 0.10f, 0.22f), new Vector2(10f, 0f));
        GolgeEkle(sol, new Color(0.04f, 0.05f, 0.10f, 0.16f), new Vector2(22f, -2f));
        GolgeEkle(sol, new Color(0.04f, 0.05f, 0.10f, 0.10f), new Vector2(40f, -4f));
        GolgeEkle(sol, new Color(0.04f, 0.05f, 0.10f, 0.05f), new Vector2(64f, -6f));

        GameObject ustIsik = Panel(sol.transform, "UstIsik", new Color(1f, 1f, 1f, 0.35f));
        Stretch(ustIsik.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -70f), Vector2.zero);
        ustIsik.GetComponent<Image>().raycastTarget = false;

        GameObject kenar1 = Panel(sol.transform, "Kenar1", new Color(0.08f, 0.09f, 0.14f, 0.22f));
        Stretch(kenar1.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-18f, 0f), Vector2.zero);
        kenar1.GetComponent<Image>().raycastTarget = false;
        GameObject kenar2 = Panel(sol.transform, "Kenar2", new Color(0.08f, 0.09f, 0.14f, 0.10f));
        Stretch(kenar2.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-36f, 0f), Vector2.zero);
        kenar2.GetComponent<Image>().raycastTarget = false;

        // Üst sekme çubuğu
        GameObject sekmeCubugu = Panel(sol.transform, "SekmeCubugu", new Color(0.88f, 0.90f, 0.95f, 1f));
        RectTransform sekmeRt = sekmeCubugu.GetComponent<RectTransform>();
        Stretch(sekmeRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -112f), new Vector2(-20f, -18f));
        YuvarlakYap(sekmeCubugu.GetComponent<Image>(), true);
        GolgeEkle(sekmeCubugu, new Color(0.08f, 0.09f, 0.14f, 0.18f), new Vector2(0f, -4f));
        GolgeEkle(sekmeCubugu, new Color(0.08f, 0.09f, 0.14f, 0.08f), new Vector2(0f, -10f));
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

        GameObject karakterPaneli = IzgaraPanel(icerik.transform, "KarakterPaneli", new Vector2(210f, 240f), 18f, 2);
        GameObject renkPaneli = IzgaraPanel(icerik.transform, "RenkPaneli", new Vector2(86f, 86f), 16f, 4);
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
        Button devam = Buton(sol.transform, "DevamButon", Vector2.zero, new Vector2(340f, 92f), new Color(0.22f, 0.72f, 0.48f));
        YuvarlakYap(devam.GetComponent<Image>(), true);
        RectTransform devamRt = devam.GetComponent<RectTransform>();
        devamRt.anchorMin = new Vector2(0.5f, 0f);
        devamRt.anchorMax = new Vector2(0.5f, 0f);
        devamRt.pivot = new Vector2(0.5f, 0f);
        devamRt.anchoredPosition = new Vector2(0f, 28f);
        TextEkle(devam.transform, "Devam", 42, Color.white);
        GolgeEkle(devam.gameObject, new Color(0.08f, 0.32f, 0.18f, 0.40f), new Vector2(0f, -6f));
        GolgeEkle(devam.gameObject, new Color(0.08f, 0.32f, 0.18f, 0.18f), new Vector2(0f, -14f));
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
            kapak.color = new Color(1f, 1f, 1f, 1f);
            YuvarlakYap(kapak, true);
            KartGolgeEkle(kart);

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

    /// <summary>Orijinal 4 rengi korur, ten ve canlı renklerle paleti genişletir.</summary>
    static void RenkKartlariniKur(Transform parent, AvatarGorunumu gorunum)
    {
        Color[] renkler =
        {
            Color.white,
            new Color(1f, 0.55f, 0.2f),
            new Color(1f, 0.6f, 0.8f),
            new Color(0.4f, 0.75f, 1f),
            new Color(1.00f, 0.88f, 0.74f),
            new Color(0.90f, 0.68f, 0.48f),
            new Color(0.62f, 0.40f, 0.26f),
            new Color(1.00f, 0.86f, 0.28f),
            new Color(0.42f, 0.82f, 0.48f),
            new Color(0.72f, 0.48f, 0.95f),
            new Color(0.95f, 0.32f, 0.34f),
            new Color(0.55f, 0.58f, 0.62f),
            new Color(0.22f, 0.22f, 0.26f),
            new Color(0.42f, 0.90f, 0.82f),
            new Color(0.95f, 0.78f, 0.32f),
            new Color(0.78f, 0.88f, 1.00f)
        };

        for (int i = 0; i < renkler.Length; i++)
        {
            Button rb = Buton(parent, "Renk" + i, Vector2.zero, new Vector2(86f, 86f), Color.white);
            Image img = rb.GetComponent<Image>();
            img.sprite = DaireSpriteAl();
            img.color = renkler[i];
            img.preserveAspect = true;

            GolgeEkle(rb.gameObject, new Color(0.10f, 0.12f, 0.16f, 0.28f), new Vector2(0f, -5f));
            Outline cerceve = rb.gameObject.AddComponent<Outline>();
            cerceve.effectColor = new Color(1f, 1f, 1f, 0.85f);
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
        yokKart.GetComponent<Image>().color = new Color(0.93f, 0.94f, 0.96f, 1f);
        YuvarlakYap(yokKart.GetComponent<Image>(), true);
        KartGolgeEkle(yokKart);
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
            kart.GetComponent<Image>().color = Color.white;
            YuvarlakYap(kart.GetComponent<Image>(), true);
            KartGolgeEkle(kart);

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

    static GameObject IzgaraPanel(Transform parent, string ad, Vector2 hucre, float bosluk, int sutun = 2)
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
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = sutun;
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
        Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-22f, 0f));
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

        Scrollbar kaydirma = DikeyKaydirmaCubugu(scrollGo.transform);

        ScrollRect scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = icerikRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.inertia = true;
        scroll.decelerationRate = 0.03f;
        scroll.scrollSensitivity = 90f;
        scroll.verticalScrollbar = kaydirma;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scroll.verticalScrollbarSpacing = 6f;
        return scrollGo;
    }

    /// <summary>Aksesuar listesi için sağdaki dikey kaydırma çubuğu.</summary>
    static Scrollbar DikeyKaydirmaCubugu(Transform parent)
    {
        GameObject sbGo = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
        sbGo.transform.SetParent(parent, false);
        RectTransform sbRt = sbGo.GetComponent<RectTransform>();
        sbRt.anchorMin = new Vector2(1f, 0f);
        sbRt.anchorMax = new Vector2(1f, 1f);
        sbRt.pivot = new Vector2(1f, 1f);
        sbRt.sizeDelta = new Vector2(16f, 0f);
        sbRt.anchoredPosition = Vector2.zero;
        Image sbBg = sbGo.GetComponent<Image>();
        sbBg.color = new Color(0.88f, 0.89f, 0.92f, 1f);

        GameObject alan = new GameObject("Sliding Area", typeof(RectTransform));
        alan.transform.SetParent(sbGo.transform, false);
        Stretch(alan.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(3f, 8f), new Vector2(-3f, -8f));

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handle.transform.SetParent(alan.transform, false);
        Stretch(handle.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image handleImg = handle.GetComponent<Image>();
        handleImg.color = new Color(0.48f, 0.50f, 0.56f, 1f);

        Scrollbar sb = sbGo.GetComponent<Scrollbar>();
        sb.handleRect = handle.GetComponent<RectTransform>();
        sb.targetGraphic = handleImg;
        sb.direction = Scrollbar.Direction.BottomToTop;
        sb.size = 0.3f;
        return sb;
    }

    static Button SekmeButon(Transform parent, string ad, string yazi)
    {
        Button btn = Buton(parent, ad, Vector2.zero, new Vector2(180f, 64f), new Color(0.96f, 0.97f, 0.99f, 1f));
        YuvarlakYap(btn.GetComponent<Image>(), true);
        LayoutElement le = btn.gameObject.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minHeight = 64f;
        GolgeEkle(btn.gameObject, new Color(0.08f, 0.09f, 0.14f, 0.20f), new Vector2(0f, -3f));
        GolgeEkle(btn.gameObject, new Color(0.08f, 0.09f, 0.14f, 0.08f), new Vector2(0f, -8f));
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

    /// <summary>Kartın altına yumuşak gölge ve ince çerçeve ekler.</summary>
    static void KartGolgeEkle(GameObject kart)
    {
        GolgeEkle(kart, new Color(0.08f, 0.10f, 0.16f, 0.22f), new Vector2(0f, -6f));
        GolgeEkle(kart, new Color(0.08f, 0.10f, 0.16f, 0.10f), new Vector2(0f, -14f));
        Outline cerceve = kart.AddComponent<Outline>();
        cerceve.effectColor = new Color(1f, 1f, 1f, 0.55f);
        cerceve.effectDistance = new Vector2(1f, -1f);
    }

    /// <summary>Image'i 9-slice yuvarlak köşeli yapar.</summary>
    static void YuvarlakYap(Image img, bool sliced)
    {
        if (img == null)
            return;
        img.sprite = YuvarlakKareSpriteAl();
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        img.pixelsPerUnitMultiplier = sliced ? 1.15f : 1f;
    }

    /// <summary>Unity UI Shadow ile düşen gölge ekler.</summary>
    static void GolgeEkle(GameObject go, Color renk, Vector2 mesafe)
    {
        Shadow golge = go.AddComponent<Shadow>();
        golge.effectColor = renk;
        golge.effectDistance = mesafe;
        golge.useGraphicAlpha = true;
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

    /// <summary>9-slice yuvarlak köşeli beyaz kare üretir (kart ve panel için).</summary>
    static Sprite YuvarlakKareSpriteAl()
    {
        if (yuvarlakKareSprite != null)
            return yuvarlakKareSprite;

        int s = 64;
        int r = 14;
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
        yuvarlakKareSprite.name = "YuvarlakKareSprite";
        return yuvarlakKareSprite;
    }

    static void EventSystemiGarantiEt()
    {
        EventSystem es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            GameObject go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (es.GetComponent<InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<InputSystemUIInputModule>();
        StandaloneInputModule eski = es.GetComponent<StandaloneInputModule>();
        if (eski != null)
            Object.DestroyImmediate(eski);
#else
        if (es.GetComponent<StandaloneInputModule>() == null)
            es.gameObject.AddComponent<StandaloneInputModule>();
#endif
    }
}
