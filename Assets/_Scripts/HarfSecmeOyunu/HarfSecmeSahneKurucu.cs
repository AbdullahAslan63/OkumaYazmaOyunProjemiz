using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Inspector bağları eksikse Play'de Canvas / slot / prefab / harf verisini kurar.
/// Ortak sözleşme: GunesSayac, TekrarOyna, dogruPuan=1, check/x, SesYoneticisi.
/// </summary>
[DefaultExecutionOrder(-50)]
public class HarfSecmeSahneKurucu : MonoBehaviour
{
    private const string GunesYolu = "Assets/_Art/Bolum1/UI/güneş.jpeg";
    private const string IsinYolu = "Assets/_Art/Bolum1/UI/ışın.jpeg";
    private const string TekrarDeneYolu = "Assets/_Art/Bolum2/UI/Tekrar_Dene_Mavi.png";
    private const string DogruIkonYolu = "Assets/_Art/Bolum2/UI/correct_check.png";
    private const string YanlisIkonYolu = "Assets/_Art/Bolum2/UI/wrong_x.png";
    private const string YildizYolu = "Assets/_Art/Bolum2/UI/Dogru_Yildiz.png";

    public HarfSecmeYoneticisi yonetici;

    void Awake()
    {
        if (yonetici == null)
            yonetici = GetComponent<HarfSecmeYoneticisi>();
        if (yonetici == null)
            yonetici = gameObject.AddComponent<HarfSecmeYoneticisi>();

        // Clip'li ses yöneticisi + Level 2 müziği
        SesYoneticisi ses = SesYoneticisi.GarantiEt();
        if (ses != null)
            ses.SahneMuziginiAyarla("HarfSecmeOyunu");

        // Tam kurulum gerekiyorsa
        bool eksik = yonetici.secenekPrefab == null ||
            yonetici.secenekPozisyonlari == null ||
            yonetici.secenekPozisyonlari.Length != 4 ||
            yonetici.secenekPozisyonlari[0] == null ||
            yonetici.soruSecici == null ||
            yonetici.soruSecici.tumHarfler == null ||
            yonetici.soruSecici.tumHarfler.Length == 0;

        if (eksik)
            Kur();

        // Her durumda parite parçalarını tamamla
        PariteyiGarantiEt();
    }

    void Kur()
    {
        // Ortak bileşenler
        SoruSecici soru = yonetici.soruSecici != null ? yonetici.soruSecici : gameObject.AddComponent<SoruSecici>();
        SkorYoneticisi skor = yonetici.skorYoneticisi != null ? yonetici.skorYoneticisi : gameObject.AddComponent<SkorYoneticisi>();
        GeriBildirimYoneticisi geri = yonetici.geriBildirim != null ? yonetici.geriBildirim : gameObject.AddComponent<GeriBildirimYoneticisi>();
        skor.dogruPuan = 1;

        HarfObjeVerisi[] harfler = Resources.LoadAll<HarfObjeVerisi>("Harfler");
        if (harfler == null || harfler.Length == 0)
            Debug.LogWarning("HarfSecmeSahneKurucu: Resources/Harfler boş. Editor menüsünü çalıştırın veya Resources'a asset koyun.");
        else
            soru.tumHarfler = harfler;

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        GameObject canvasGo;
        if (canvas == null)
        {
            canvasGo = new GameObject("HarfCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasGo = canvas.gameObject;
        }

        TextMeshProUGUI buyuk = Tmp(canvasGo.transform, "BuyukHarf", "A", 190, new Vector2(0, 400));
        TextMeshProUGUI baslik = Tmp(canvasGo.transform, "SoruBaslik", "Hangisinin baş harfi", 52, new Vector2(0, 500));
        TextMeshProUGUI sure = Tmp(canvasGo.transform, "SureYazisi", "Doğru: 0", 40, new Vector2(-200, 480));
        baslik.fontStyle = FontStyles.Bold;
        baslik.color = Color.white;
        sure.color = Color.white;

        // Tik / çarpı sprite'ları
        GameObject dogruFb = FeedbackImage(canvasGo.transform, "DogruFB", DogruIkonYolu);
        GameObject yanlisFb = FeedbackImage(canvasGo.transform, "YanlisFB", YanlisIkonYolu);
        dogruFb.SetActive(false);
        yanlisFb.SetActive(false);
        geri.dogruGorsel = dogruFb;
        geri.yanlisGorsel = yanlisFb;
        geri.skorYazisi = sure;

        Transform[] slotlar = new Transform[4];
        float[] xs = { -540f, -180f, 180f, 540f };
        for (int i = 0; i < 4; i++)
        {
            GameObject slot = new GameObject("SecenekPozisyon" + (i + 1), typeof(RectTransform));
            slot.transform.SetParent(canvasGo.transform, false);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xs[i], -80f);
            rt.sizeDelta = new Vector2(380f, 450f);
            slotlar[i] = slot.transform;
        }

        GameObject bitis = Panel(canvasGo.transform, "BitisPaneli", new Color(0.72f, 0.88f, 0.98f, 0.97f));
        RectTransform brt = bitis.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        TextMeshProUGUI tebrik = Tmp(bitis.transform, "BitisTebrik", "Süper!", 72, new Vector2(0, 160));
        TextMeshProUGUI bitisSkor = Tmp(bitis.transform, "BitisSkor", "Doğru: 0", 44, new Vector2(0, 20));
        Image[] yildizlar = new Image[3];
        Sprite yildizSp = ArtSpriteYukle(YildizYolu);
        for (int i = 0; i < 3; i++)
        {
            GameObject y = new GameObject("Yildiz" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            y.transform.SetParent(bitis.transform, false);
            RectTransform yrt = y.GetComponent<RectTransform>();
            yrt.anchoredPosition = new Vector2(-120 + i * 120, 90);
            yrt.sizeDelta = new Vector2(72, 72);
            Image yi = y.GetComponent<Image>();
            if (yildizSp != null)
            {
                yi.sprite = yildizSp;
                yi.preserveAspect = true;
            }
            yildizlar[i] = yi;
        }

        // Tekrar Dene (Mavi)
        Button tekrarBtn = TekrarDeneButonuKur(bitis.transform);
        // Ana Menü
        GameObject anaGo = new GameObject("AnaMenuButon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        anaGo.transform.SetParent(bitis.transform, false);
        RectTransform art = anaGo.GetComponent<RectTransform>();
        art.anchoredPosition = new Vector2(0, -200);
        art.sizeDelta = new Vector2(320, 90);
        anaGo.GetComponent<Image>().color = new Color(0.98f, 0.55f, 0.25f);
        Button anaBtn = anaGo.GetComponent<Button>();
        anaGo.AddComponent<ButonTiklamaSesi>();
        TextMeshProUGUI anaText = Tmp(anaGo.transform, "Text", "Ana Menü", 40, Vector2.zero);
        anaText.color = Color.white;
        anaBtn.onClick.AddListener(yonetici.AnaMenuyeDon);
        bitis.SetActive(false);

        GameObject prefab = SecenekPrefabOlustur(transform);

        yonetici.soruSecici = soru;
        yonetici.skorYoneticisi = skor;
        yonetici.geriBildirim = geri;
        yonetici.secenekPrefab = prefab;
        yonetici.secenekPozisyonlari = slotlar;
        yonetici.buyukHarf = buyuk;
        yonetici.soruBaslikYazisi = baslik;
        yonetici.sureYazisi = sure;
        yonetici.soruFontu = Resources.Load<TMP_FontAsset>("Fonts/SoruFontu");
        yonetici.bitisPaneli = bitis;
        yonetici.bitisTebrikYazisi = tebrik;
        yonetici.bitisSkorYazisi = bitisSkor;
        yonetici.bitisYildizlari = yildizlar;
        yonetici.tekrarDeneButonu = tekrarBtn;
        yonetici.anaMenuButonu = anaBtn;
        yonetici.anaMenuSahneAdi = "LevelEkranı";
    }

    /// <summary>Sahne kısmen kurulu olsa bile güneş / tekrar / skor / ikon tamamlar.</summary>
    void PariteyiGarantiEt()
    {
        if (yonetici.skorYoneticisi != null)
            yonetici.skorYoneticisi.dogruPuan = 1;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        Transform canvasT = canvas.transform;

        // Güneş sayacı
        if (yonetici.gunesSayac == null)
        {
            Transform mevcut = canvasT.Find("GunesSayac");
            if (mevcut != null)
                yonetici.gunesSayac = mevcut.GetComponent<GunesSayac>();
            else
                yonetici.gunesSayac = GunesSayaciniKur(canvasT);
        }

        // Süre yazısı: güneş varken Doğru sayacı
        if (yonetici.sureYazisi != null && yonetici.gunesSayac != null)
        {
            yonetici.sureYazisi.text = "Doğru: 0";
            RectTransform srt = yonetici.sureYazisi.rectTransform;
            srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(0.5f, 1f);
            srt.anchoredPosition = new Vector2(-80f, -40f);
        }

        // Bitiş paneli içinde Tekrar Dene
        if (yonetici.bitisPaneli != null && yonetici.tekrarDeneButonu == null)
        {
            Transform t = yonetici.bitisPaneli.transform.Find("TekrarDene");
            if (t != null)
                yonetici.tekrarDeneButonu = t.GetComponent<Button>();
            else
                yonetici.tekrarDeneButonu = TekrarDeneButonuKur(yonetici.bitisPaneli.transform);
        }

        if (yonetici.tekrarDeneButonu != null &&
            yonetici.tekrarDeneButonu.GetComponent<ButonTiklamaSesi>() == null)
        {
            yonetici.tekrarDeneButonu.gameObject.AddComponent<ButonTiklamaSesi>();
        }

        // Geri bildirim sprite ikonları (renkli panel yerine)
        if (yonetici.geriBildirim != null)
        {
            if (yonetici.geriBildirim.dogruGorsel == null ||
                yonetici.geriBildirim.dogruGorsel.GetComponent<Image>() == null ||
                yonetici.geriBildirim.dogruGorsel.GetComponent<Image>().sprite == null)
            {
                GameObject dogruFb = FeedbackImage(canvasT, "DogruFB", DogruIkonYolu);
                dogruFb.SetActive(false);
                yonetici.geriBildirim.dogruGorsel = dogruFb;
            }

            if (yonetici.geriBildirim.yanlisGorsel == null ||
                yonetici.geriBildirim.yanlisGorsel.GetComponent<Image>() == null ||
                yonetici.geriBildirim.yanlisGorsel.GetComponent<Image>().sprite == null)
            {
                GameObject yanlisFb = FeedbackImage(canvasT, "YanlisFB", YanlisIkonYolu);
                yanlisFb.SetActive(false);
                yonetici.geriBildirim.yanlisGorsel = yanlisFb;
            }
        }
    }

    GunesSayac GunesSayaciniKur(Transform canvas)
    {
        GameObject kok = new GameObject("GunesSayac", typeof(RectTransform));
        kok.transform.SetParent(canvas, false);
        RectTransform kokRt = kok.GetComponent<RectTransform>();
        kokRt.anchorMin = new Vector2(1f, 1f);
        kokRt.anchorMax = new Vector2(1f, 1f);
        kokRt.pivot = new Vector2(1f, 1f);
        kokRt.anchoredPosition = new Vector2(-28f, -12f);
        kokRt.sizeDelta = new Vector2(320f, 320f);

        GunesSayac sayac = kok.AddComponent<GunesSayac>();
        Sprite gunesSp = SiyahligiSeffafYap(ArtSpriteYukle(GunesYolu));
        Sprite isinSp = SiyahligiSeffafYap(ArtSpriteYukle(IsinYolu));

        GameObject gunesGo = new GameObject("Gunes", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gunesGo.transform.SetParent(kok.transform, false);
        RectTransform gRt = gunesGo.GetComponent<RectTransform>();
        gRt.anchorMin = gRt.anchorMax = gRt.pivot = new Vector2(0.5f, 0.5f);
        gRt.anchoredPosition = Vector2.zero;
        gRt.sizeDelta = new Vector2(120f, 120f);
        Image gImg = gunesGo.GetComponent<Image>();
        gImg.sprite = gunesSp;
        gImg.preserveAspect = true;
        gImg.color = Color.white;
        sayac.gunesGorsel = gImg;

        const int isinAdet = 15;
        Image[] isinlar = new Image[isinAdet];
        float icYaricap = 70f;
        float aciAdim = 360f / isinAdet;
        for (int i = 0; i < isinAdet; i++)
        {
            GameObject isinGo = new GameObject("Isin_" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            isinGo.transform.SetParent(kok.transform, false);
            RectTransform iRt = isinGo.GetComponent<RectTransform>();
            iRt.anchorMin = iRt.anchorMax = new Vector2(0.5f, 0.5f);
            iRt.pivot = new Vector2(0.5f, 0f);
            iRt.sizeDelta = new Vector2(28f, 48f);
            float aci = i * aciAdim;
            float rad = aci * Mathf.Deg2Rad;
            iRt.anchoredPosition = new Vector2(Mathf.Sin(rad) * icYaricap, Mathf.Cos(rad) * icYaricap);
            iRt.localRotation = Quaternion.Euler(0f, 0f, -aci);
            Image iImg = isinGo.GetComponent<Image>();
            iImg.sprite = isinSp;
            iImg.preserveAspect = true;
            iImg.raycastTarget = false;
            iImg.color = Color.white;
            isinlar[i] = iImg;
        }

        sayac.isinlar = isinlar;
        sayac.isinSuresi = 4f;
        gunesGo.transform.SetAsLastSibling();
        return sayac;
    }

    Button TekrarDeneButonuKur(Transform bitisParent)
    {
        GameObject btnGo = new GameObject("TekrarDene", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(bitisParent, false);
        RectTransform bRt = btnGo.GetComponent<RectTransform>();
        bRt.anchorMin = bRt.anchorMax = bRt.pivot = new Vector2(0.5f, 0.5f);
        bRt.anchoredPosition = new Vector2(0f, -90f);
        bRt.sizeDelta = new Vector2(360f, 110f);
        Image bImg = btnGo.GetComponent<Image>();
        Sprite tekrarSp = ArtSpriteYukle(TekrarDeneYolu);
        if (tekrarSp != null)
        {
            bImg.sprite = tekrarSp;
            bImg.preserveAspect = true;
            bImg.color = Color.white;
        }
        else
        {
            bImg.color = new Color(0.35f, 0.55f, 0.95f, 1f);
        }

        Button btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = bImg;
        btnGo.AddComponent<ButonTiklamaSesi>();
        btn.onClick.AddListener(yonetici.TekrarOyna);
        return btn;
    }

    static GameObject FeedbackImage(Transform parent, string ad, string yol)
    {
        Transform eski = parent.Find(ad);
        if (eski != null)
            Destroy(eski.gameObject);

        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 120f);
        rt.sizeDelta = new Vector2(160f, 160f);
        Image img = go.GetComponent<Image>();
        Sprite sp = ArtSpriteYukle(yol);
        if (sp != null)
        {
            img.sprite = sp;
            img.preserveAspect = true;
            img.color = Color.white;
        }
        else
        {
            img.color = ad.Contains("Dogru")
                ? new Color(0.3f, 0.8f, 0.4f, 0.85f)
                : new Color(0.9f, 0.4f, 0.3f, 0.85f);
        }

        img.raycastTarget = false;
        return go;
    }

    static GameObject SecenekPrefabOlustur(Transform parent)
    {
        GameObject root = new GameObject("SecenekBalonu");
        root.transform.SetParent(parent, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(380f, 450f);
        Image cerceve = root.AddComponent<Image>();
        cerceve.color = new Color(1f, 0.95f, 0.7f, 1f);
        root.AddComponent<Button>().targetGraphic = cerceve;
        SecenekBalonu balon = root.AddComponent<SecenekBalonu>();

        GameObject obje = new GameObject("ObjeResmi");
        obje.transform.SetParent(root.transform, false);
        RectTransform ort = obje.AddComponent<RectTransform>();
        ort.anchorMin = new Vector2(0.5f, 0.5f);
        ort.anchorMax = new Vector2(0.5f, 0.5f);
        ort.sizeDelta = new Vector2(260f, 230f);
        ort.anchoredPosition = new Vector2(0f, -20f);
        Image oi = obje.AddComponent<Image>();
        oi.preserveAspect = true;
        oi.raycastTarget = false;
        balon.objeResmi = oi;

        root.SetActive(false);
        return root;
    }

    static TextMeshProUGUI Tmp(Transform parent, string ad, string metin, float size, Vector2 pos)
    {
        GameObject go = new GameObject(ad);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = metin;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.95f, 0.2f, 1f);
        RectTransform rt = tmp.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(900, size + 40f);
        return tmp;
    }

    static GameObject Panel(Transform parent, string ad, Color renk)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 200);
        rt.sizeDelta = new Vector2(420, 180);
        go.GetComponent<Image>().color = renk;
        go.GetComponent<Image>().raycastTarget = false;
        return go;
    }

    static Sprite ArtSpriteYukle(string assetYolu)
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetYolu);
#else
        return null;
#endif
    }

    // JPEG siyah arka planı şeffaf yapar (UI sayaç için)
    static Sprite SiyahligiSeffafYap(Sprite kaynak)
    {
        if (kaynak == null || kaynak.texture == null)
            return kaynak;

        Texture2D okunabilir = TextureOkunabilirKopyala(kaynak.texture);
        if (okunabilir == null)
            return kaynak;

        Color[] pikseller = okunabilir.GetPixels();
        for (int i = 0; i < pikseller.Length; i++)
        {
            Color c = pikseller[i];
            if (c.r < 0.12f && c.g < 0.12f && c.b < 0.12f)
                pikseller[i] = new Color(c.r, c.g, c.b, 0f);
        }

        okunabilir.SetPixels(pikseller);
        okunabilir.Apply();
        return Sprite.Create(okunabilir, new Rect(0, 0, okunabilir.width, okunabilir.height), new Vector2(0.5f, 0.5f), 100f);
    }

    static Texture2D TextureOkunabilirKopyala(Texture kaynak)
    {
        if (kaynak == null)
            return null;

        int w = kaynak.width;
        int h = kaynak.height;
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(kaynak, rt);
        RenderTexture onceki = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D kopya = new Texture2D(w, h, TextureFormat.RGBA32, false);
        kopya.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        kopya.Apply();
        RenderTexture.active = onceki;
        RenderTexture.ReleaseTemporary(rt);
        return kopya;
    }
}
