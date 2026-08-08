using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Mini Oyun 2 yöneticisi.
/// Her turda 1 doğru + 3 çeldiriciyi 4 sabit slota yerleştirir (mevcut balon GO'larını taşır).
/// Paket 3 API'sine bağlı değildir; Inspector veya AutoDoldur ile çalışır.
/// </summary>
public class HarfSecmeYoneticisi : MonoBehaviour
{
    // Tek kopya erişimi
    public static HarfSecmeYoneticisi Instance;

    // Bir seçenek kaydı (balon + feedback + soru harfi görseli)
    [System.Serializable]
    public class SecenekKaydi
    {
        public string adi;
        public char harf;
        public int harfId;
        public GameObject balon;
        public GameObject dogruFeedback;
        public GameObject yanlisFeedback;
        public GameObject soruGorseli;
    }

    [Header("Süre")]
    // Inspector'dan süre yazısı (boşsa runtime oluşturulur)
    public Text sureYazisi;
    // Oyun süresi (saniye)
    public float oyunSuresi = 60f;
    // Kalan süre (kılavuz alan adı)
    public float kalanSure;

    [Header("Sahne geçişi")]
    // Ana menü / çıkış sahne adı (Build Settings'te olmalı)
    public string anaMenuSahneAdi = "AbdullahScene";

    [Header("4 sabit slot")]
    // Dört seçenek pozisyonu (boşsa Start'ta ilk 4 balondan alınır)
    public Transform[] secenekPozisyonlari = new Transform[4];

    [Header("8 seçenek kaydı")]
    // Harf objeleri; boş bırakılırsa AutoDoldur isimle doldurur
    public SecenekKaydi[] secenekler;

    // Uçuş hızı / süresi
    private float yukselmeHizi = 5f;
    private float ucusSuresi = 2f;

    // Skor sayaçları (Paket 3 SkorYoneticisi gelene kadar yerel)
    private int dogruSayisi;
    private int yanlisSayisi;
    private bool islemYapiliyor;
    private bool oyunBitti;

    // Aktif soru harf id'si
    private int secilenHarfId;
    private GameObject ucanBalon;
    private Camera anaKamera;

    // Harf sonuçları
    private readonly Dictionary<int, bool> harfDogruBildimi = new Dictionary<int, bool>();
    private readonly Dictionary<int, int> harfYanlisSayisi = new Dictionary<int, int>();
    private readonly List<int> soruDestesi = new List<int>();

    // SecenekBalonu listesi ve slot dünya koordinatları
    private readonly List<SecenekBalonu> tumBalonlar = new List<SecenekBalonu>();
    private readonly Vector3[] slotPozisyonlari = new Vector3[4];

    // Bu turda aktif 4 seçenek
    private readonly List<SecenekKaydi> turSecenekleri = new List<SecenekKaydi>(4);

    // ----------------- Yaşam döngüsü -----------------

    void Awake()
    {
        // Singleton
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
        anaKamera = Camera.main;
        kalanSure = oyunSuresi;

        // Inspector boşsa sahneden isimle doldur
        AutoDoldur();
        SlotPozisyonlariniHazirla();
        BalonlariHazirla();
        FeedbackleriGizle();
        SonucSozlukleriniSifirla();

        if (sureYazisi == null)
            SureYazisiniOlustur();
        SureYazisiniGuncelle();

        YeniSoru();
    }

    // ----------------- Kurulum -----------------

    void AutoDoldur()
    {
        // Diziyi varsayılan 8 kayıtla oluştur
        if (secenekler == null || secenekler.Length != 8)
            secenekler = new SecenekKaydi[8];

        // Varsayılan tanımlar (sahne GO adlarıyla birebir; ç/ı/ü dikkat)
        DoldurKayit(0, "araba", 'A', 1, "shbaraba", "doğruseçenekaraba", "yanlışseçenekaraba", "HangisininBasHarfiA_0");
        DoldurKayit(1, "elma", 'E', 2, "shbelma", "doğruseçenekelma", "yanlışseçenekelma", "HangisininBasHarfiE_0");
        DoldurKayit(2, "orumcek", 'Ö', 3, "shbörümcek", "doğruseçenekörümcek", "yanlışseçenekorumcek", "HangisininBasHarfiÖ_0");
        DoldurKayit(3, "uzum", 'Ü', 4, "shbüzüm", "doğruseçeneküzüm", "yanlışseçenekuzum", "HangisininBaşHarfiÜ_0");
        DoldurKayit(4, "islakmendil", 'I', 5, "shbıslakmendil", "doğruseçenekıslak", "yanlışseçenekıslak", "HangisininBasHarfiI_0");
        DoldurKayit(5, "inek", 'İ', 6, "shbinek", "doğruseçenekinek", "yanlışseçenekinek", "HangisininBasHarfiİ_0");
        DoldurKayit(6, "oyuncak", 'O', 7, "shboyuncak", "doğruseçenekoyuncak", "yanlışseçenekoyuncak", "HangisininBasHarfiO_0");
        DoldurKayit(7, "ucak", 'U', 8, "shbuçak", "doğruseçenekuçak", "yanlışseçenekuçak", "HangisininBasHarfU_0");

        // Sahnede eksik doğru-tikileri komşu kayıttan kopyala
        FeedbackEksikseKopyala(4, 3); // ıslak ← üzüm
        FeedbackEksikseKopyala(5, 2); // inek ← örümcek
    }

    // dogruFeedback yoksa şablon kayıttan Instantiate et
    void FeedbackEksikseKopyala(int hedefIndeks, int sablonIndeks)
    {
        if (secenekler == null || hedefIndeks >= secenekler.Length || sablonIndeks >= secenekler.Length)
            return;

        SecenekKaydi hedef = secenekler[hedefIndeks];
        SecenekKaydi sablon = secenekler[sablonIndeks];
        if (hedef == null || sablon == null) return;

        if (hedef.dogruFeedback == null && sablon.dogruFeedback != null)
            hedef.dogruFeedback = FeedbackKopyala(sablon.dogruFeedback, "dogrusecenek_" + hedef.adi);

        if (hedef.yanlisFeedback == null && sablon.yanlisFeedback != null)
            hedef.yanlisFeedback = FeedbackKopyala(sablon.yanlisFeedback, "yanlissecenek_" + hedef.adi);
    }

    GameObject FeedbackKopyala(GameObject sablon, string yeniAd)
    {
        if (sablon == null) return null;
        GameObject kopya = Instantiate(sablon);
        kopya.name = yeniAd;
        kopya.transform.SetParent(null, true);
        kopya.SetActive(false);
        return kopya;
    }

    void DoldurKayit(int indeks, string adi, char harf, int harfId,
        string balonAd, string dogruAd, string yanlisAd, string soruAd)
    {
        // Null eleman oluştur
        if (secenekler[indeks] == null)
            secenekler[indeks] = new SecenekKaydi();

        SecenekKaydi k = secenekler[indeks];
        if (string.IsNullOrEmpty(k.adi)) k.adi = adi;
        if (k.harf == '\0') k.harf = harf;
        if (k.harfId == 0) k.harfId = harfId;

        // Inspector doluysa dokunma
        if (k.balon == null) k.balon = FindByNameContains(balonAd);
        if (k.dogruFeedback == null) k.dogruFeedback = FindByNameContains(dogruAd);
        if (k.yanlisFeedback == null) k.yanlisFeedback = FindByNameContains(yanlisAd);
        if (k.soruGorseli == null)
            k.soruGorseli = FindByExactName(soruAd) ?? FindByNameContains(soruAd.Replace("_0", ""));
    }

    void SlotPozisyonlariniHazirla()
    {
        // Inspector Transform dizisi doluysa oradan al
        int atanmis = 0;
        if (secenekPozisyonlari != null)
        {
            for (int i = 0; i < 4 && i < secenekPozisyonlari.Length; i++)
            {
                if (secenekPozisyonlari[i] != null)
                {
                    slotPozisyonlari[i] = secenekPozisyonlari[i].position;
                    atanmis++;
                }
            }
        }

        if (atanmis == 4)
            return;

        // Yoksa ilk dört bulunan balonun başlangıç konumunu slot yap
        int slot = 0;
        for (int i = 0; i < secenekler.Length && slot < 4; i++)
        {
            if (secenekler[i] != null && secenekler[i].balon != null)
            {
                slotPozisyonlari[slot] = secenekler[i].balon.transform.position;
                slot++;
            }
        }

        // Hâlâ eksikse varsayılan yatay yerleşim
        for (int i = slot; i < 4; i++)
            slotPozisyonlari[i] = new Vector3(-6f + i * 4f, -3f, 0f);
    }

    void BalonlariHazirla()
    {
        tumBalonlar.Clear();
        for (int i = 0; i < secenekler.Length; i++)
        {
            SecenekKaydi k = secenekler[i];
            if (k == null || k.balon == null)
                continue;

            SecenekBalonu b = k.balon.GetComponent<SecenekBalonu>();
            if (b == null)
                b = k.balon.AddComponent<SecenekBalonu>();

            // Slot sabit değil; tur başında güncellenir
            b.Ayarla(k.adi, k.harf, k.harfId, k.dogruFeedback, k.yanlisFeedback, k.balon.transform.position);
            b.TiklanabilirYap();
            tumBalonlar.Add(b);

            // Başlangıçta hepsini gizle; YeniSoru aktif 4'ü açar
            k.balon.SetActive(false);
        }
    }

    void FeedbackleriGizle()
    {
        for (int i = 0; i < secenekler.Length; i++)
        {
            SecenekKaydi k = secenekler[i];
            if (k == null) continue;
            SetActiveSafe(k.dogruFeedback, false);
            SetActiveSafe(k.yanlisFeedback, false);
            BringToFront(k.dogruFeedback);
            BringToFront(k.yanlisFeedback);
            SetActiveSafe(k.soruGorseli, false);
        }
    }

    void SonucSozlukleriniSifirla()
    {
        harfDogruBildimi.Clear();
        harfYanlisSayisi.Clear();
        for (int i = 0; i < secenekler.Length; i++)
        {
            if (secenekler[i] == null) continue;
            int id = secenekler[i].harfId;
            harfDogruBildimi[id] = false;
            harfYanlisSayisi[id] = 0;
        }
    }

    // ----------------- Bulucu (yalnızca AutoDoldur) -----------------

    GameObject FindInScene(System.Func<Transform, bool> esles)
    {
        Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t == null || !t.gameObject.scene.IsValid()) continue;
            if (esles(t)) return t.gameObject;
        }
        return null;
    }

    GameObject FindByExactName(string exactName)
    {
        return FindInScene(t => string.Equals(t.name, exactName, System.StringComparison.Ordinal));
    }

    GameObject FindByNameContains(string namePart)
    {
        // Büyük/küçük harf duyarlı (Türkçe İ/I karışıklığını önler)
        if (string.IsNullOrEmpty(namePart)) return null;
        return FindInScene(t => t.name.IndexOf(namePart, System.StringComparison.Ordinal) >= 0);
    }

    void SetActiveSafe(GameObject go, bool aktif)
    {
        if (go != null) go.SetActive(aktif);
    }

    void BringToFront(GameObject go)
    {
        if (go == null) return;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 100;
    }

    // ----------------- UI -----------------

    void SureYazisiniOlustur()
    {
        GameObject canvasGo = new GameObject("SureCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject yaziGo = new GameObject("SureYazi");
        yaziGo.transform.SetParent(canvasGo.transform, false);
        sureYazisi = yaziGo.AddComponent<Text>();
        sureYazisi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (sureYazisi.font == null)
            sureYazisi.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        sureYazisi.fontSize = 42;
        sureYazisi.alignment = TextAnchor.UpperCenter;
        sureYazisi.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        RectTransform rt = sureYazisi.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -20f);
        rt.sizeDelta = new Vector2(420f, 60f);
    }

    void SureYazisiniGuncelle()
    {
        if (sureYazisi == null) return;
        int sn = Mathf.CeilToInt(kalanSure);
        sureYazisi.text = string.Format("{0:00}:{1:00}   Doğru: {2}", sn / 60, sn % 60, dogruSayisi);
    }

    // ----------------- Update / Tıklama -----------------

    void Update()
    {
        if (!oyunBitti)
        {
            kalanSure -= Time.deltaTime;
            if (kalanSure < 0f) kalanSure = 0f;
            SureYazisiniGuncelle();
            if (kalanSure <= 0f && !islemYapiliyor)
            {
                OyunuBitir();
                return;
            }
        }

        if (ucanBalon != null)
            ucanBalon.transform.position += Vector3.up * yukselmeHizi * Time.deltaTime;

        if (!islemYapiliyor && !oyunBitti)
            TiklamaKontrol();
    }

    void TiklamaKontrol()
    {
        if (anaKamera == null) anaKamera = Camera.main;
        if (anaKamera == null) return;

        bool tiklandi = false;
        Vector2 ekranPos = Vector2.zero;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            tiklandi = true;
            ekranPos = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            tiklandi = true;
            ekranPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (!tiklandi) return;

        float derinlik = Mathf.Abs(anaKamera.transform.position.z);
        Vector3 dunya = anaKamera.ScreenToWorldPoint(new Vector3(ekranPos.x, ekranPos.y, derinlik));
        Vector2 dunya2D = new Vector2(dunya.x, dunya.y);

        Collider2D hit = Physics2D.OverlapPoint(dunya2D);
        if (hit != null)
        {
            SecenekBalonu balon = hit.GetComponentInParent<SecenekBalonu>();
            if (balon != null && balon.gameObject.activeInHierarchy)
            {
                balon.Tiklandi();
                return;
            }
        }

        for (int i = 0; i < tumBalonlar.Count; i++)
        {
            SecenekBalonu b = tumBalonlar[i];
            if (b != null && b.NoktaSpriteIcinde(dunya2D))
            {
                b.Tiklandi();
                return;
            }
        }
    }

    // ----------------- Soru akışı -----------------

    void DesteyiDoldurVeKaristir()
    {
        soruDestesi.Clear();
        for (int i = 0; i < secenekler.Length; i++)
        {
            if (secenekler[i] != null && secenekler[i].balon != null)
                soruDestesi.Add(secenekler[i].harfId);
        }
        Karistir(soruDestesi);
    }

    void Karistir<T>(List<T> liste)
    {
        // Fisher-Yates
        for (int i = liste.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = liste[i];
            liste[i] = liste[j];
            liste[j] = tmp;
        }
    }

    // Yeni rastgele soru: 1 doğru + 3 çeldirici → 4 slota
    public void YeniSoru()
    {
        if (oyunBitti || kalanSure <= 0f)
        {
            if (!oyunBitti) OyunuBitir();
            return;
        }

        if (soruDestesi.Count == 0)
            DesteyiDoldurVeKaristir();

        if (soruDestesi.Count == 0)
        {
            Debug.LogWarning("Soru destesi boş — seçenek balonları bulunamadı.");
            return;
        }

        secilenHarfId = soruDestesi[0];
        soruDestesi.RemoveAt(0);
        SoruyuGuncelle();
    }

    void SoruyuGuncelle()
    {
        // Soru harfi görsellerini aç/kapa
        SecenekKaydi dogruKayit = null;
        for (int i = 0; i < secenekler.Length; i++)
        {
            SecenekKaydi k = secenekler[i];
            if (k == null) continue;
            bool buSoru = k.harfId == secilenHarfId;
            SetActiveSafe(k.soruGorseli, buSoru);
            if (buSoru) dogruKayit = k;
        }

        if (dogruKayit == null || dogruKayit.balon == null)
        {
            Debug.LogWarning("Doğru seçenek bulunamadı: harfId " + secilenHarfId);
            return;
        }

        // Çeldirici havuzu
        List<SecenekKaydi> celdiriciler = new List<SecenekKaydi>();
        for (int i = 0; i < secenekler.Length; i++)
        {
            SecenekKaydi k = secenekler[i];
            if (k == null || k.balon == null) continue;
            if (k.harfId == secilenHarfId) continue;
            celdiriciler.Add(k);
        }
        Karistir(celdiriciler);

        // 1 doğru + en fazla 3 çeldirici
        turSecenekleri.Clear();
        turSecenekleri.Add(dogruKayit);
        for (int i = 0; i < celdiriciler.Count && turSecenekleri.Count < 4; i++)
            turSecenekleri.Add(celdiriciler[i]);
        Karistir(turSecenekleri);

        // Kullanılmayan balonları gizle
        for (int i = 0; i < secenekler.Length; i++)
        {
            if (secenekler[i] != null && secenekler[i].balon != null)
                secenekler[i].balon.SetActive(false);
        }

        char soruChar = dogruKayit.harf;

        // Aktif 4'ü slotlara yerleştir
        for (int i = 0; i < turSecenekleri.Count && i < 4; i++)
        {
            SecenekKaydi k = turSecenekleri[i];
            Vector3 pos = slotPozisyonlari[i];
            k.balon.transform.position = pos;
            k.balon.SetActive(true);

            SecenekBalonu b = k.balon.GetComponent<SecenekBalonu>();
            if (b == null) continue;
            b.Ayarla(k.adi, k.harf, k.harfId, k.dogruFeedback, k.yanlisFeedback, pos);
            b.SoruHarfiniGuncelle(soruChar);
        }

        Debug.Log("Yeni soru: " + secilenHarfId + " (" + dogruKayit.harf + ") — " + turSecenekleri.Count + " seçenek");
    }

    // Kılavuz imzası (balonsuz — uyarı)
    public void SecenekSecildi(bool dogruMu)
    {
        Debug.LogWarning("SecenekSecildi(bool) balon bilgisiz; SecenekBalonu.Tiklandi kullanın.");
    }

    // Seçenek sonucu
    public void SecenekSecildi(bool dogruMu, SecenekBalonu secenek)
    {
        if (oyunBitti || islemYapiliyor || kalanSure <= 0f) return;
        if (secenek == null) return;

        if (dogruMu)
        {
            StartCoroutine(DogruCevapSureci(secenek));
        }
        else
        {
            yanlisSayisi++;
            if (harfYanlisSayisi.ContainsKey(secilenHarfId))
                harfYanlisSayisi[secilenHarfId]++;
            else
                harfYanlisSayisi[secilenHarfId] = 1;

            StartCoroutine(YanlisCevapSureci(secenek));
        }
    }

    IEnumerator DogruCevapSureci(SecenekBalonu secenek)
    {
        islemYapiliyor = true;
        dogruSayisi++;
        harfDogruBildimi[secenek.harfId] = true;
        SureYazisiniGuncelle();

        GameObject balon = secenek.gameObject;
        GameObject tiki = secenek.dogruFeedback;
        Vector3 normalKonum = secenek.slotPozisyonu;

        if (tiki != null)
        {
            tiki.transform.SetParent(balon.transform, false);
            tiki.transform.localPosition = new Vector3(0f, 0f, -1f);
            BringToFront(tiki);
            tiki.SetActive(true);
        }

        ucanBalon = balon;
        yield return new WaitForSeconds(ucusSuresi);
        ucanBalon = null;

        if (tiki != null)
        {
            tiki.SetActive(false);
            tiki.transform.SetParent(null, true);
        }

        balon.transform.position = normalKonum;

        if (kalanSure <= 0f)
            OyunuBitir();
        else
            YeniSoru();

        islemYapiliyor = false;
    }

    IEnumerator YanlisCevapSureci(SecenekBalonu secenek)
    {
        islemYapiliyor = true;

        GameObject carpi = secenek.yanlisFeedback;
        GameObject balon = secenek.gameObject;

        if (carpi != null)
        {
            if (balon != null)
            {
                carpi.transform.SetParent(balon.transform, false);
                carpi.transform.localPosition = new Vector3(0f, 0f, -1f);
            }
            BringToFront(carpi);
            carpi.SetActive(true);
        }

        // Yanlışta aynı soru kalır
        yield return new WaitForSeconds(1f);
        SetActiveSafe(carpi, false);
        if (carpi != null)
            carpi.transform.SetParent(null, true);

        if (kalanSure <= 0f)
            OyunuBitir();

        islemYapiliyor = false;
    }

    // ----------------- Bitiş -----------------

    void OyunuBitir()
    {
        if (oyunBitti) return;
        oyunBitti = true;
        islemYapiliyor = true;
        ucanBalon = null;
        StopAllCoroutines();

        // Soru görsellerini ve balonları kapat
        for (int i = 0; i < secenekler.Length; i++)
        {
            if (secenekler[i] == null) continue;
            SetActiveSafe(secenekler[i].soruGorseli, false);
            SetActiveSafe(secenekler[i].balon, false);
        }

        if (sureYazisi != null)
            sureYazisi.gameObject.SetActive(false);

        KapanisEkraniniGoster();
    }

    void KapanisEkraniniGoster()
    {
        // Yıldız ve kısa tebrik metni
        int yildiz = YildizHesapla(dogruSayisi);
        string tebrik = TebrikMetni(yildiz);

        GameObject canvasGo = new GameObject("KapanisCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        // Açık gökyüzü mavisi arka plan
        GameObject panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        Image panel = panelGo.AddComponent<Image>();
        panel.color = new Color(0.72f, 0.88f, 0.98f, 0.97f);
        RectTransform panelRt = panel.rectTransform;
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Büyük tebrik
        Text baslik = YaziEkle(panelGo.transform, tebrik, 72, new Vector2(0f, 180f), font, 1000f);
        baslik.color = new Color(0.15f, 0.45f, 0.35f);
        baslik.fontStyle = FontStyle.Bold;

        // Üç altın/gri daire (font yıldızı çoğu built-in fontta yok)
        YildizSatiriOlustur(panelGo.transform, yildiz);

        // Tek net bilgi: doğru sayısı
        Text skor = YaziEkle(panelGo.transform, "Doğru: " + dogruSayisi, 44, new Vector2(0f, -40f), font, 500f);
        skor.color = new Color(0.25f, 0.35f, 0.45f);

        // Büyük yuvarlak hisli Ana Menü butonu
        GameObject btnGo = new GameObject("AnaMenuButon");
        btnGo.transform.SetParent(panelGo.transform, false);
        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.98f, 0.55f, 0.25f, 1f);
        Button btn = btnGo.AddComponent<Button>();
        btn.onClick.AddListener(AnaMenuyeDon);
        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = new Vector2(0f, -200f);
        btnRt.sizeDelta = new Vector2(320f, 90f);

        Text btnText = YaziEkle(btnGo.transform, "Ana Menü", 40, Vector2.zero, font, 320f);
        btnText.color = Color.white;
        btnText.fontStyle = FontStyle.Bold;
        RectTransform btnTextRt = btnText.rectTransform;
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;
    }

    // Doğru sayısına göre 1–3 yıldız (çocuk hep en az 1 alır)
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

    // Üç renkli daire: dolu = altın, boş = açık gri
    void YildizSatiriOlustur(Transform parent, int dolu)
    {
        float ara = 120f;
        float baslangicX = -ara;
        Color doluRenk = new Color(1f, 0.78f, 0.2f, 1f);
        Color bosRenk = new Color(0.85f, 0.88f, 0.92f, 1f);

        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject("Yildiz" + (i + 1));
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = (i < dolu) ? doluRenk : bosRenk;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(baslangicX + i * ara, 50f);
            rt.sizeDelta = new Vector2(72f, 72f);
        }
    }

    // Ana menü sahnesine dön
    public void AnaMenuyeDon()
    {
        if (string.IsNullOrEmpty(anaMenuSahneAdi))
        {
            Debug.LogWarning("anaMenuSahneAdi boş.");
            return;
        }
        SceneManager.LoadScene(anaMenuSahneAdi);
    }

    Text YaziEkle(Transform parent, string icerik, int fontSize, Vector2 pos, Font font, float genislik = 900f)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.text = icerik;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(genislik, 90f);
        return t;
    }
}

