using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mini Oyun 2: UI prefab ile 4 seçenek; HarfObjeVerisi + SoruSecici kullanır.
/// AutoDoldur / isim araması yoktur.
/// </summary>
public class HarfSecmeYoneticisi : MonoBehaviour
{
    // Tek kopya erişimi
    public static HarfSecmeYoneticisi Instance;

    [Header("Ortak sistemler")]
    public SoruSecici soruSecici;
    public SkorYoneticisi skorYoneticisi;
    public GeriBildirimYoneticisi geriBildirim;

    [Header("Seçenekler")]
    public GameObject secenekPrefab;
    public Transform[] secenekPozisyonlari = new Transform[4];

    [Header("UI")]
    public TextMeshProUGUI buyukHarf;
    public TextMeshProUGUI soruBaslikYazisi;
    public TextMeshProUGUI sureYazisi;
    public GameObject bitisPaneli;
    public TextMeshProUGUI bitisTebrikYazisi;
    public TextMeshProUGUI bitisSkorYazisi;
    public Image[] bitisYildizlari;
    public TMP_FontAsset soruFontu;
    public string soruBaslikMetni = "Hangisinin baş harfi";

    [Header("Süre")]
    public float oyunSuresi = 60f;
    public float kalanSure;

    [Header("Sahne")]
    public string anaMenuSahneAdi = "AvatarOlusturmaEkrani";

    [Header("Animasyon")]
    public float yukselmeSuresi = 0.45f;
    public float yukselmeOfsetY = 400f;

    // Aktif tur harfi
    private HarfObjeVerisi aktifHarf;
    private readonly List<GameObject> aktifSecenekler = new List<GameObject>();
    private bool islemYapiliyor;
    private bool oyunBitti;
    private int dogruSayisi;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        kalanSure = oyunSuresi;
        dogruSayisi = 0;
        if (skorYoneticisi != null)
            skorYoneticisi.SkoruSifirla();

        if (bitisPaneli != null)
            bitisPaneli.SetActive(false);

        SoruUiHazirla();
        SlotlariEkranaGoreAyarla();
        SureYazisiniGuncelle();
        YeniSoru();
    }

    /// <summary>
    /// Sabit "Hangisinin baş harfi" yazısı + tatlı/kalın font uygular.
    /// Eksikse Canvas altında oluşturur.
    /// </summary>
    void SoruUiHazirla()
    {
        try
        {
            SoruFontunuGuvenliYukle();
            SoruPaneliniHazirla();

            if (soruBaslikYazisi == null && buyukHarf != null)
            {
                Transform parent = buyukHarf.transform.parent;
                Transform mevcut = parent != null ? parent.Find("SoruBaslik") : null;
                if (mevcut != null)
                    soruBaslikYazisi = mevcut.GetComponent<TextMeshProUGUI>();
                else if (parent != null)
                {
                    GameObject go = new GameObject("SoruBaslik");
                    go.transform.SetParent(parent, false);
                    soruBaslikYazisi = go.AddComponent<TextMeshProUGUI>();
                }
            }

            // Yerleşim: süre üstte, başlık, harf — panel üzerinde
            if (sureYazisi != null)
            {
                RectTransform srt = sureYazisi.rectTransform;
                srt.anchoredPosition = new Vector2(0f, 520f);
                srt.sizeDelta = new Vector2(900f, 56f);
                sureYazisi.fontSize = 40f;
                sureYazisi.color = new Color(1f, 1f, 1f, 1f);
                sureYazisi.outlineWidth = 0f;
                FontuUygula(sureYazisi);
            }

            if (soruBaslikYazisi != null)
            {
                RectTransform rt = soruBaslikYazisi.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 455f);
                rt.sizeDelta = new Vector2(1000f, 70f);
                soruBaslikYazisi.text = soruBaslikMetni;
                soruBaslikYazisi.alignment = TextAlignmentOptions.Center;
                soruBaslikYazisi.fontSize = 44f;
                soruBaslikYazisi.fontStyle = FontStyles.Normal;
                soruBaslikYazisi.color = Color.white;
                soruBaslikYazisi.outlineWidth = 0f; // bozuk atlas outline’ı bozuyor
                FontuUygula(soruBaslikYazisi);
                soruBaslikYazisi.transform.SetAsLastSibling();
            }

            if (buyukHarf != null)
            {
                RectTransform brt = buyukHarf.rectTransform;
                brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 0.5f);
                brt.anchoredPosition = new Vector2(0f, 355f);
                brt.sizeDelta = new Vector2(400f, 200f);
                buyukHarf.alignment = TextAlignmentOptions.Center;
                buyukHarf.outlineWidth = 0f;
                FontuUygula(buyukHarf);
                buyukHarf.transform.SetAsLastSibling();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("SoruUiHazirla atlandı (font): " + e.Message);
            soruFontu = null;
        }
    }

    /// <summary>Soru metninin okunması için yarı saydam koyu panel.</summary>
    void SoruPaneliniHazirla()
    {
        if (buyukHarf == null) return;
        Transform parent = buyukHarf.transform.parent;
        if (parent == null) return;

        Transform panelT = parent.Find("SoruPanel");
        GameObject panelGo;
        if (panelT == null)
        {
            panelGo = new GameObject("SoruPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGo.transform.SetParent(parent, false);
            panelGo.transform.SetSiblingIndex(0);
        }
        else
        {
            panelGo = panelT.gameObject;
        }

        RectTransform prt = panelGo.GetComponent<RectTransform>();
        prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
        prt.anchoredPosition = new Vector2(0f, 420f);
        prt.sizeDelta = new Vector2(720f, 260f);

        Image img = panelGo.GetComponent<Image>();
        img.color = new Color(0.08f, 0.12f, 0.22f, 0.72f);
        img.raycastTarget = false;
    }

    void SoruFontunuGuvenliYukle()
    {
        if (soruFontu == null)
            soruFontu = Resources.Load<TMP_FontAsset>("Fonts/SoruFontu");

        if (soruFontu != null && !FontAtlasGecerliMi(soruFontu))
        {
            Debug.LogWarning("SoruFontu geçersiz — varsayılan TMP font kullanılacak.");
            soruFontu = null;
        }
    }

    static bool FontAtlasGecerliMi(TMP_FontAsset font)
    {
        if (font == null) return false;
        try
        {
            if (font.material == null) return false;
            Texture2D atlas = font.atlasTexture;
            if (atlas == null) return false;
            return atlas.width > 0;
        }
        catch
        {
            return false;
        }
    }

    void FontuUygula(TextMeshProUGUI yazi)
    {
        if (yazi == null) return;
        if (soruFontu == null || !FontAtlasGecerliMi(soruFontu)) return;

        try
        {
            yazi.font = soruFontu;
            if (soruFontu.material != null)
                yazi.fontSharedMaterial = soruFontu.material;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Font uygulanamadı: " + e.Message);
        }
    }

    /// <summary>
    /// 4 slot’u Canvas genişlik/yüksekliğine göre büyütür ve yatayda eşit aralar.
    /// Mobil (dar) ve akıllı tahta (geniş) için clamp’li ölçek.
    /// </summary>
    public void SlotlariEkranaGoreAyarla()
    {
        if (secenekPozisyonlari == null || secenekPozisyonlari.Length == 0)
            return;

        Canvas canvas = null;
        for (int i = 0; i < secenekPozisyonlari.Length; i++)
        {
            if (secenekPozisyonlari[i] == null) continue;
            canvas = secenekPozisyonlari[i].GetComponentInParent<Canvas>();
            if (canvas != null) break;
        }
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRt = canvas.GetComponent<RectTransform>();
        Canvas.ForceUpdateCanvases();
        float cw = canvasRt.rect.width;
        float ch = canvasRt.rect.height;
        if (cw < 100f) cw = 1920f;
        if (ch < 100f) ch = 1080f;

        // Her balon: ekranın ~%23’ü genişlik, yükseklik 1.18 oran; sınırlar mobil/tahta
        float balonW = Mathf.Clamp(cw * 0.23f, 300f, 480f);
        float balonH = Mathf.Clamp(balonW * 1.18f, 360f, 560f);
        // 4 balon sığmazsa küçült
        float maxW = (cw * 0.92f) / 4f;
        if (balonW > maxW)
        {
            balonW = maxW;
            balonH = balonW * 1.18f;
        }

        float ara = Mathf.Min(cw * 0.24f, balonW + cw * 0.02f);
        float basX = -1.5f * ara;
        float y = -ch * 0.06f;

        // Yükseliş ofsetini ekrana göre ayarla
        yukselmeOfsetY = Mathf.Clamp(ch * 0.55f, 350f, 700f);

        for (int i = 0; i < secenekPozisyonlari.Length && i < 4; i++)
        {
            Transform t = secenekPozisyonlari[i];
            if (t == null) continue;
            RectTransform rt = t as RectTransform;
            if (rt == null) continue;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(balonW, balonH);
            rt.anchoredPosition = new Vector2(basX + i * ara, y);
        }
    }

    void Update()
    {
        if (oyunBitti) return;

        kalanSure -= Time.deltaTime;
        if (kalanSure < 0f) kalanSure = 0f;
        SureYazisiniGuncelle();

        if (kalanSure <= 0f && !islemYapiliyor)
            OyunuBitir();
    }

    /// <summary>Yeni rastgele soru: 1 doğru + 3 çeldirici → 4 slot.</summary>
    public void YeniSoru()
    {
        if (oyunBitti || kalanSure <= 0f)
        {
            if (!oyunBitti) OyunuBitir();
            return;
        }

        if (soruSecici == null)
        {
            Debug.LogWarning("HarfSecmeYoneticisi: soruSecici atanmamış.");
            return;
        }

        aktifHarf = soruSecici.YeniHarfSec();
        if (aktifHarf == null)
        {
            Debug.LogWarning("HarfSecmeYoneticisi: harf seçilemedi.");
            return;
        }

        if (buyukHarf != null)
        {
            buyukHarf.text = aktifHarf.harf.ToString();
            buyukHarf.color = new Color(1f, 0.92f, 0.2f, 1f);
            buyukHarf.fontSize = 160f;
            buyukHarf.fontStyle = FontStyles.Normal;
            buyukHarf.enableAutoSizing = false;
            buyukHarf.outlineWidth = 0f;
            buyukHarf.alignment = TextAlignmentOptions.Center;
            FontuUygula(buyukHarf);
        }

        if (soruBaslikYazisi != null)
            soruBaslikYazisi.text = soruBaslikMetni;

        StartCoroutine(SecenekleriYerlestir());
    }

    /// <summary>Seçenek tıklanınca çağrılır.</summary>
    public void SecenekSecildi(bool dogruMu)
    {
        if (oyunBitti || islemYapiliyor || kalanSure <= 0f) return;

        if (dogruMu)
            StartCoroutine(DogruCevapSureci());
        else
            StartCoroutine(YanlisCevapSureci());
    }

    /// <summary>Ana menü sahnesine döner.</summary>
    public void AnaMenuyeDon()
    {
        if (string.IsNullOrEmpty(anaMenuSahneAdi)) return;
        SceneManager.LoadScene(anaMenuSahneAdi);
    }

    IEnumerator SecenekleriYerlestir()
    {
        islemYapiliyor = true;
        SecenekleriTemizle();
        SlotlariEkranaGoreAyarla();

        List<SecenekVerisi> liste = SecenekListesiOlustur();
        if (liste.Count == 0)
        {
            islemYapiliyor = false;
            yield break;
        }

        Karistir(liste);

        List<SecenekBalonu> balonlar = new List<SecenekBalonu>();
        List<Vector2> hedefler = new List<Vector2>();

        for (int i = 0; i < liste.Count && i < secenekPozisyonlari.Length; i++)
        {
            Transform slot = secenekPozisyonlari[i];
            if (slot == null || secenekPrefab == null) continue;

            GameObject go = Instantiate(secenekPrefab, slot);
            go.SetActive(true);
            go.name = "Secenek_" + i;

            SecenekBalonu balon = go.GetComponent<SecenekBalonu>();
            if (balon == null)
                balon = go.AddComponent<SecenekBalonu>();

            // Önce boyutu slot’a oturt, sonra animasyon Y’si
            balon.YerlesimiDuzenle(true);
            balon.Ayarla(liste[i].resim, liste[i].harf, aktifHarf.harf);
            balon.TiklamayiAyarla(false);
            balonlar.Add(balon);

            RectTransform rt = go.GetComponent<RectTransform>();
            Vector2 hedef = Vector2.zero;
            if (rt != null)
            {
                hedefler.Add(hedef);
                float hizFarki = Random.Range(0.85f, 1.15f);
                rt.anchoredPosition = new Vector2(0f, -yukselmeOfsetY * hizFarki);
            }
            else
            {
                hedefler.Add(Vector2.zero);
            }

            aktifSecenekler.Add(go);
        }

        // Bir frame bekle — layout rect güncellensin, obje merkezlensin
        yield return null;
        for (int i = 0; i < balonlar.Count; i++)
            balonlar[i].YerlesimiDuzenle(false);

        float sure = yukselmeSuresi;
        float t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / sure);
            float s = u * u * (3f - 2f * u);
            for (int i = 0; i < aktifSecenekler.Count; i++)
            {
                RectTransform rt = aktifSecenekler[i].GetComponent<RectTransform>();
                if (rt == null) continue;
                float basY = -yukselmeOfsetY;
                rt.anchoredPosition = new Vector2(hedefler[i].x, Mathf.Lerp(basY, hedefler[i].y, s));
            }
            yield return null;
        }

        for (int i = 0; i < aktifSecenekler.Count; i++)
        {
            RectTransform rt = aktifSecenekler[i].GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = hedefler[i];
        }

        for (int i = 0; i < balonlar.Count; i++)
        {
            balonlar[i].YerlesimiDuzenle(true);
            balonlar[i].TiklamayiAyarla(true);
        }

        islemYapiliyor = false;
    }

    List<SecenekVerisi> SecenekListesiOlustur()
    {
        List<SecenekVerisi> sonuc = new List<SecenekVerisi>(4);

        if (aktifHarf.dogruObjeler == null || aktifHarf.dogruObjeler.Length == 0)
        {
            Debug.LogWarning("Aktif harfin dogruObjeler listesi boş: " + aktifHarf.harf);
            return sonuc;
        }

        // 1 doğru
        Sprite dogruSprite = aktifHarf.dogruObjeler[Random.Range(0, aktifHarf.dogruObjeler.Length)];
        sonuc.Add(new SecenekVerisi(dogruSprite, aktifHarf.harf));

        // 3 çeldirici
        List<HarfObjeVerisi> digerler = new List<HarfObjeVerisi>();
        if (soruSecici != null && soruSecici.tumHarfler != null)
        {
            for (int i = 0; i < soruSecici.tumHarfler.Length; i++)
            {
                HarfObjeVerisi h = soruSecici.tumHarfler[i];
                if (h == null || h == aktifHarf) continue;
                if (h.dogruObjeler == null || h.dogruObjeler.Length == 0) continue;
                digerler.Add(h);
            }
        }
        Karistir(digerler);

        for (int i = 0; i < digerler.Count && sonuc.Count < 4; i++)
        {
            Sprite s = digerler[i].dogruObjeler[Random.Range(0, digerler[i].dogruObjeler.Length)];
            sonuc.Add(new SecenekVerisi(s, digerler[i].harf));
        }

        return sonuc;
    }

    IEnumerator DogruCevapSureci()
    {
        islemYapiliyor = true;
        dogruSayisi++;

        if (skorYoneticisi != null)
            skorYoneticisi.DogruPuanEkle();

        if (geriBildirim != null)
        {
            geriBildirim.DogruGoster();
            geriBildirim.SkorYazisiniGuncelle(dogruSayisi);
        }

        SureYazisiniGuncelle();
        yield return new WaitForSeconds(0.7f);

        if (kalanSure <= 0f)
            OyunuBitir();
        else
            YeniSoru();

        islemYapiliyor = false;
    }

    IEnumerator YanlisCevapSureci()
    {
        islemYapiliyor = true;

        if (geriBildirim != null)
            geriBildirim.YanlisGoster();

        yield return new WaitForSeconds(0.6f);

        // Yanlışta aynı soru kalır — seçenekler yerinde
        if (kalanSure <= 0f)
            OyunuBitir();

        islemYapiliyor = false;
    }

    void OyunuBitir()
    {
        if (oyunBitti) return;
        oyunBitti = true;
        islemYapiliyor = true;
        StopAllCoroutines();
        SecenekleriTemizle();

        if (bitisPaneli != null)
            bitisPaneli.SetActive(true);

        int yildiz = YildizHesapla(dogruSayisi);
        if (bitisTebrikYazisi != null)
            bitisTebrikYazisi.text = TebrikMetni(yildiz);
        if (bitisSkorYazisi != null)
            bitisSkorYazisi.text = "Doğru: " + dogruSayisi;

        if (bitisYildizlari != null)
        {
            for (int i = 0; i < bitisYildizlari.Length; i++)
            {
                if (bitisYildizlari[i] == null) continue;
                bitisYildizlari[i].enabled = true;
                bitisYildizlari[i].color = i < yildiz
                    ? new Color(1f, 0.78f, 0.2f, 1f)
                    : new Color(0.85f, 0.88f, 0.92f, 1f);
            }
        }
    }

    void SecenekleriTemizle()
    {
        for (int i = 0; i < aktifSecenekler.Count; i++)
        {
            if (aktifSecenekler[i] != null)
                Destroy(aktifSecenekler[i]);
        }
        aktifSecenekler.Clear();
    }

    void SureYazisiniGuncelle()
    {
        if (sureYazisi == null) return;
        int sn = Mathf.CeilToInt(kalanSure);
        sureYazisi.text = string.Format("{0:00}:{1:00}   Doğru: {2}", sn / 60, sn % 60, dogruSayisi);
    }

    int YildizHesapla(int dogru)
    {
        if (dogru >= 5) return 3;
        if (dogru >= 2) return 2;
        return 1;
    }

    string TebrikMetni(int yildiz)
    {
        if (yildiz >= 3) return "Süper!";
        if (yildiz == 2) return "Aferin!";
        return "Güzel denedin!";
    }

    void Karistir<T>(List<T> liste)
    {
        for (int i = liste.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = liste[i];
            liste[i] = liste[j];
            liste[j] = tmp;
        }
    }

    // İç seçenek kaydı
    private struct SecenekVerisi
    {
        public Sprite resim;
        public char harf;
        public SecenekVerisi(Sprite r, char h)
        {
            resim = r;
            harf = h;
        }
    }
}
