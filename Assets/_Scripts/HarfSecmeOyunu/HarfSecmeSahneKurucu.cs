using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Inspector bağları eksikse Play'de Canvas / slot / prefab / harf verisini kurar.
/// Editor menü kurulumu yapıldıysa bir şey yapmaz.
/// </summary>
[DefaultExecutionOrder(-50)]
public class HarfSecmeSahneKurucu : MonoBehaviour
{
    public HarfSecmeYoneticisi yonetici;

    void Awake()
    {
        if (yonetici == null)
            yonetici = GetComponent<HarfSecmeYoneticisi>();
        if (yonetici == null)
            yonetici = gameObject.AddComponent<HarfSecmeYoneticisi>();

        // Zaten bağlıysa çık
        if (yonetici.secenekPrefab != null &&
            yonetici.secenekPozisyonlari != null &&
            yonetici.secenekPozisyonlari.Length == 4 &&
            yonetici.secenekPozisyonlari[0] != null &&
            yonetici.soruSecici != null &&
            yonetici.soruSecici.tumHarfler != null &&
            yonetici.soruSecici.tumHarfler.Length > 0)
            return;

        Kur();
    }

    void Kur()
    {
        // Ortak bileşenler
        SoruSecici soru = yonetici.soruSecici != null ? yonetici.soruSecici : gameObject.AddComponent<SoruSecici>();
        SkorYoneticisi skor = yonetici.skorYoneticisi != null ? yonetici.skorYoneticisi : gameObject.AddComponent<SkorYoneticisi>();
        GeriBildirimYoneticisi geri = yonetici.geriBildirim != null ? yonetici.geriBildirim : gameObject.AddComponent<GeriBildirimYoneticisi>();

        // Harf verileri: Resources veya Resources.LoadAll alternatif — _Data yüklenemez; Resources kopyası
        HarfObjeVerisi[] harfler = Resources.LoadAll<HarfObjeVerisi>("Harfler");
        if (harfler == null || harfler.Length == 0)
        {
            // Editor'da AssetDatabase ile doldurulmuş SoruSecici yoksa uyar
            Debug.LogWarning("HarfSecmeSahneKurucu: Resources/Harfler boş. Editor menüsünü çalıştırın veya Resources'a asset koyun.");
        }
        else
        {
            soru.tumHarfler = harfler;
        }

        // EventSystem
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

        // Canvas
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
        TextMeshProUGUI sure = Tmp(canvasGo.transform, "SureYazisi", "01:00", 48, new Vector2(0, 560));
        baslik.fontStyle = FontStyles.Bold;
        baslik.color = Color.white;

        GameObject dogruFb = Panel(canvasGo.transform, "DogruFB", new Color(0.3f, 0.8f, 0.4f, 0.85f));
        GameObject yanlisFb = Panel(canvasGo.transform, "YanlisFB", new Color(0.9f, 0.4f, 0.3f, 0.85f));
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
        TextMeshProUGUI tebrik = Tmp(bitis.transform, "BitisTebrik", "Süper!", 72, new Vector2(0, 120));
        TextMeshProUGUI bitisSkor = Tmp(bitis.transform, "BitisSkor", "Doğru: 0", 44, new Vector2(0, 0));
        Image[] yildizlar = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject y = new GameObject("Yildiz" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            y.transform.SetParent(bitis.transform, false);
            RectTransform yrt = y.GetComponent<RectTransform>();
            yrt.anchoredPosition = new Vector2(-120 + i * 120, 50);
            yrt.sizeDelta = new Vector2(72, 72);
            yildizlar[i] = y.GetComponent<Image>();
        }
        GameObject anaGo = new GameObject("AnaMenuButon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        anaGo.transform.SetParent(bitis.transform, false);
        RectTransform art = anaGo.GetComponent<RectTransform>();
        art.anchoredPosition = new Vector2(0, -180);
        art.sizeDelta = new Vector2(320, 90);
        anaGo.GetComponent<Image>().color = new Color(0.98f, 0.55f, 0.25f);
        Button anaBtn = anaGo.GetComponent<Button>();
        TextMeshProUGUI anaText = Tmp(anaGo.transform, "Text", "Ana Menü", 40, Vector2.zero);
        anaText.color = Color.white;
        anaBtn.onClick.AddListener(yonetici.AnaMenuyeDon);
        bitis.SetActive(false);

        // Runtime prefab şablonu (gizli child)
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
        yonetici.anaMenuSahneAdi = "AvatarOlusturmaEkrani";
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
        ort.offsetMin = Vector2.zero;
        ort.offsetMax = Vector2.zero;
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
        if (ad == "BuyukHarf" || ad == "SureYazisi")
        {
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        }
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
}
