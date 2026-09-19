using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Avatar seçim UI'si yoksa Play'de Canvas + butonları kurar.
/// </summary>
[DefaultExecutionOrder(-50)]
public class AvatarSahneKurucu : MonoBehaviour
{
    public AvatarGorunumu avatarGorunumu;

    void Awake()
    {
        if (Object.FindAnyObjectByType<AvatarSecimEkrani>() != null)
            return;

        if (avatarGorunumu == null)
            avatarGorunumu = Object.FindAnyObjectByType<AvatarGorunumu>();

        // Hayvan sprite'ları boşsa doldurmayı dene (Resources yok — Inspector tercih)
        if (avatarGorunumu != null && (avatarGorunumu.hayvanlar == null || avatarGorunumu.hayvanlar.Length == 0))
            Debug.LogWarning("AvatarSahneKurucu: hayvanlar listesi boş — Inspector'dan sprite ata veya Editor menüsünü çalıştır.");

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

        GameObject canvasGo = new GameObject("AvatarCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        AvatarSecimEkrani secim = canvasGo.AddComponent<AvatarSecimEkrani>();
        secim.avatarGorunumu = avatarGorunumu;
        secim.sonrakiSahneAdi = "HarfSecmeOyunu";

        Button geri = Buton(canvasGo.transform, "GeriButon", new Vector2(-700, 0), new Vector2(120, 120), new Color(0.9f, 0.9f, 0.9f));
        geri.onClick.AddListener(secim.OncekiHayvan);
        Text gt = TextEkle(geri.transform, "<");
        Button ileri = Buton(canvasGo.transform, "IleriButon", new Vector2(700, 0), new Vector2(120, 120), new Color(0.9f, 0.9f, 0.9f));
        ileri.onClick.AddListener(secim.SonrakiHayvan);
        TextEkle(ileri.transform, ">");

        Color[] renkler = { Color.white, new Color(1f, 0.55f, 0.2f), new Color(1f, 0.6f, 0.8f), new Color(0.4f, 0.75f, 1f) };
        for (int i = 0; i < renkler.Length; i++)
        {
            Button rb = Buton(canvasGo.transform, "Renk" + i, new Vector2(-180 + i * 120, -320), new Vector2(80, 80), renkler[i]);
            RenkSecici rs = rb.gameObject.AddComponent<RenkSecici>();
            rs.avatarGorunumu = avatarGorunumu;
            rs.builtinRenk = renkler[i];
            rb.onClick.AddListener(rs.BuButonunRenginiSec);
        }

        Button yok = Buton(canvasGo.transform, "AksesuarYok", new Vector2(-400, -420), new Vector2(100, 80), new Color(0.85f, 0.85f, 0.85f));
        TextEkle(yok.transform, "Yok");
        AksesuarSecici yokSec = yok.gameObject.AddComponent<AksesuarSecici>();
        yokSec.avatarGorunumu = avatarGorunumu;
        yokSec.aksesuarIndex = -1;
        yok.onClick.AddListener(yokSec.BuAksesuariSec);

        if (avatarGorunumu != null && avatarGorunumu.aksesuarlar != null)
        {
            for (int i = 0; i < avatarGorunumu.aksesuarlar.Length && i < 6; i++)
            {
                Button ab = Buton(canvasGo.transform, "Aks" + i, new Vector2(-250 + i * 110, -420), new Vector2(90, 90), Color.white);
                Image img = ab.GetComponent<Image>();
                // AksesuarGorseli içindeki sprite'ı butona yaz
                if (avatarGorunumu.aksesuarlar[i] != null && avatarGorunumu.aksesuarlar[i].sprite != null)
                    img.sprite = avatarGorunumu.aksesuarlar[i].sprite;
                AksesuarSecici asec = ab.gameObject.AddComponent<AksesuarSecici>();
                asec.avatarGorunumu = avatarGorunumu;
                asec.aksesuarIndex = i;
                ab.onClick.AddListener(asec.BuAksesuariSec);
            }
        }

        Button devam = Buton(canvasGo.transform, "DevamButon", new Vector2(0, -520), new Vector2(280, 90), new Color(0.3f, 0.75f, 0.45f));
        TextEkle(devam.transform, "Devam");
        devam.onClick.AddListener(secim.DevamEt);
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
        return go.GetComponent<Button>();
    }

    static Text TextEkle(Transform parent, string metin)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.text = metin;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.black;
        t.fontSize = 36;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform rt = t.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return t;
    }
}
