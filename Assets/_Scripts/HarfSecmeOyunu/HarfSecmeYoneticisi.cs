using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Mini Oyun 2 yöneticisi: soru seçer, slotları ayarlar, tıklama sonucunu işler.
/// Davranış mevcut sahne/tag yapısıyla uyumludur (prefab Instantiate yok).
/// </summary>
public class HarfSecmeYoneticisi : MonoBehaviour
{
    // Tek kopyaya erişim
    public static HarfSecmeYoneticisi Instance;

    // 1=A(araba) 2=E(elma) 3=Ö(örümcek) 4=Ü(üzüm)
    // 5=I(ıslak) 6=İ(inek) 7=O(oyuncak) 8=U(uçak)
    // Slotlar: araba↔uçak | elma↔oyuncak | üzüm↔ıslak | örümcek↔inek

    // Harf id → görünen harf adı
    private readonly Dictionary<int, string> harfAdlari = new Dictionary<int, string>
    {
        { 1, "A" },
        { 2, "E" },
        { 3, "Ö" },
        { 4, "Ü" },
        { 5, "I" },
        { 6, "İ" },
        { 7, "O" },
        { 8, "U" }
    };

    // Harf id → char (SecenekBalonu karşılaştırması için)
    private readonly Dictionary<int, char> harfChar = new Dictionary<int, char>
    {
        { 1, 'A' },
        { 2, 'E' },
        { 3, 'Ö' },
        { 4, 'Ü' },
        { 5, 'I' },
        { 6, 'İ' },
        { 7, 'O' },
        { 8, 'U' }
    };

    private float yukselmehizi = 5.0f;
    private float ucusSuresi = 2.0f;
    private float oyunSuresi = 60f;
    public float kalanSure;
    private int secilenharf;
    private int dogruSayisi;
    private int yanlisSayisi;
    private bool islemYapiliyor;
    private bool oyunBitti;
    private GameObject ucanBalon;
    private Camera anaKamera;
    private Text sureYazi;

    private readonly Dictionary<int, bool> harfDogruBildimi = new Dictionary<int, bool>();
    private readonly Dictionary<int, int> harfYanlisSayisi = new Dictionary<int, int>();
    private readonly List<int> soruHavuzu = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
    private readonly List<int> soruDestesi = new List<int>();

    // Sekiz seçenek balonu bileşeni
    private SecenekBalonu arabaBalon, elmaBalon, orumcekBalon, uzumBalon;
    private SecenekBalonu islakBalon, inekBalon, oyuncakBalon, ucakBalon;
    private readonly List<SecenekBalonu> tumBalonlar = new List<SecenekBalonu>();

    // Feedback sahnedeki GO'lar (geçici; Ayarla'ya verilir)
    private GameObject dogruAraba, dogruElma, dogruOrumcek, dogruUzum;
    private GameObject dogruIslak, dogruInek, dogruOyuncak, dogruUcak;
    private GameObject yanlisAraba, yanlisElma, yanlisOrumcek, yanlisUzum;
    private GameObject yanlisIslak, yanlisInek, yanlisOyuncak, yanlisUcak;

    // Büyük harf soru görselleri
    private GameObject soruA, soruE, soruOe, soruUe, soruI, soruIi, soruO, soruUu;

    // Slot sabit pozisyonları
    private Vector3 posAraba, posElma, posOrumcek, posUzum;
    private Vector3 posIslak, posInek, posOyuncak, posUcak;

    // ----------------- Yaşam döngüsü -----------------

    void Awake()
    {
        // Singleton ata
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        // Bu kopya Instance ise temizle
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        anaKamera = Camera.main;
        kalanSure = oyunSuresi;

        // Harf sonuç sözlüklerini sıfırla
        for (int i = 0; i < soruHavuzu.Count; i++)
        {
            harfDogruBildimi[soruHavuzu[i]] = false;
            harfYanlisSayisi[soruHavuzu[i]] = 0;
        }

        // Balon GameObject'lerini sahnede bul (tag yoksa isim)
        GameObject arababalon = FindFirst(
            FindByTagSafe("arababalon"),
            FindByNameContains("shbaraba"));
        GameObject elmabalon = FindFirst(
            FindByTagSafe("elmabalon"),
            FindByNameContains("shbelma"));
        GameObject orumcekbalon = FindFirst(
            FindByTagSafe("orumcekbalon"),
            FindByNameContains("shb\u00f6r\u00fcmcek"));
        GameObject uzumbalon = FindFirst(
            FindByTagSafe("uzumbalon"),
            FindByNameContains("shb\u00fcz\u00fcm"));
        GameObject islakmendilbalon = FindFirst(
            FindByTagSafe("islakmendilbalon"),
            FindByNameContains("shb\u0131slakmendil"));
        GameObject inekbalon = FindFirst(
            FindByTagSafe("inekbalon"),
            FindByNameContains("shbinek"));
        GameObject oyuncakbalon = FindFirst(
            FindByTagSafe("oyuncakbalon"),
            FindByNameContains("shboyuncak"));
        // Sahnedeki tag: uçakbalon (unicode)
        GameObject ucakbalon = FindFirst(
            FindByTagSafe("u\u00e7akbalon"),
            FindByTagSafe("ucakbalon"),
            FindByNameContains("shbu\u00e7ak"));

        // Doğru geri bildirim görsellerini bul
        dogruAraba = FindFirst(
            FindByNameContains("do\u011fruse\u00e7enekaraba"),
            FindByTagSafe("do\u011fruse\u00e7imtikiaraba"));
        dogruElma = FindFirst(
            FindByNameContains("do\u011fruse\u00e7enekelma"),
            FindByTagSafe("do\u011fruse\u00e7imtikielma"));
        dogruOrumcek = FindFirst(
            FindByNameContains("do\u011fruse\u00e7enek\u00f6r\u00fcmcek"),
            FindByTagSafe("do\u011fruse\u00e7imtiki\u00f6r\u00fcmcek"));
        dogruUzum = FindFirst(
            FindByNameContains("do\u011fruse\u00e7enek\u00fcz\u00fcm"),
            FindByTagSafe("do\u011fruse\u00e7imtiki\u00fcz\u00fcm"));
        dogruIslak = FindFirst(
            FindByNameContains("do\u011fruse\u00e7enek\u0131slak"),
            FindByTagSafe("dogrutikiislakmendil"));
        if (dogruIslak == null) dogruIslak = FeedbackKopyala(dogruUzum, "dogrusecenekislakmendil");
        dogruInek = FindFirst(
            FindByNameContains("do\u011fruse\u00e7enekinek"),
            FindByTagSafe("dogrutikiinek"));
        if (dogruInek == null) dogruInek = FeedbackKopyala(dogruOrumcek, "dogrusecenekinek");
        dogruOyuncak = FindFirst(
            FindByNameContains("do\u011fruse\u00e7enekoyuncak"),
            FindByTagSafe("dogrutikioyuncak"));
        if (dogruOyuncak == null) dogruOyuncak = FeedbackKopyala(dogruElma, "dogrusecenekoyuncak");
        dogruUcak = FindFirst(
            FindByNameContains("do\u011fruse\u00e7eneku\u00e7ak"),
            FindByTagSafe("dogrutikiucak"));
        if (dogruUcak == null) dogruUcak = FeedbackKopyala(dogruAraba, "dogrusecenekucak");

        // Yanlış geri bildirim görsellerini bul
        yanlisAraba = FindFirst(
            FindByNameContains("yanl\u0131\u015Fse\u00e7enekaraba"),
            FindByTagSafe("yanl\u0131\u015Fse\u00e7im\u00e7arp\u0131s\u0131araba"));
        yanlisElma = FindFirst(
            FindByNameContains("yanl\u0131\u015Fse\u00e7enekelma"),
            FindByTagSafe("yanl\u0131\u015Fse\u00e7im\u00e7arp\u0131s\u0131elma"));
        yanlisOrumcek = FindFirst(
            FindByNameContains("yanl\u0131\u015Fse\u00e7enekorumcek"),
            FindByTagSafe("yanl\u0131\u015Fse\u00e7im\u00e7arp\u0131s\u0131\u00f6r\u00fcmcek"));
        yanlisUzum = FindFirst(
            FindByNameContains("yanl\u0131\u015Fse\u00e7enekuzum"),
            FindByTagSafe("yanl\u0131\u015Fse\u00e7im\u00e7arp\u0131s\u0131\u00fcz\u00fcm"));
        yanlisIslak = FindFirst(
            FindByNameContains("yanl\u0131\u015Fse\u00e7enek\u0131slak"),
            FindByTagSafe("yanliscarpiislakmendil"));
        if (yanlisIslak == null) yanlisIslak = FeedbackKopyala(yanlisUzum, "yanlissecenekislakmendil");
        yanlisInek = FindFirst(
            FindByNameContains("yanl\u0131\u015Fse\u00e7enekinek"),
            FindByTagSafe("yanliscarpiinek"));
        if (yanlisInek == null) yanlisInek = FeedbackKopyala(yanlisOrumcek, "yanlissecenekinek");
        yanlisOyuncak = FindFirst(
            FindByNameContains("yanl\u0131\u015Fse\u00e7enekoyuncak"),
            FindByTagSafe("yanliscarpioyuncak"));
        if (yanlisOyuncak == null) yanlisOyuncak = FeedbackKopyala(yanlisElma, "yanlissecenekoyuncak");
        yanlisUcak = FindFirst(
            FindByNameContains("yanl\u0131\u015Fse\u00e7eneku\u00e7ak"),
            FindByTagSafe("yanliscarpiucak"));
        if (yanlisUcak == null) yanlisUcak = FeedbackKopyala(yanlisAraba, "yanlissecenekucak");

        // Soru harfi görsellerini bul
        soruA = FindFirst(FindByExactName("HangisininBasHarfiA_0"), FindByTagSafe("soruA"));
        soruE = FindFirst(FindByExactName("HangisininBasHarfiE_0"), FindByTagSafe("soruE"));
        soruOe = FindFirst(FindByExactName("HangisininBasHarfi\u00d6_0"), FindByTagSafe("soru\u00d6"));
        soruUe = FindFirst(FindByExactName("HangisininBa\u015eHarfi\u00dc_0"), FindByTagSafe("soru\u00dc"));
        soruI = FindFirst(FindByExactName("HangisininBasHarfiI_0"), FindByTagSafe("soruI"));
        soruIi = FindFirst(
            FindByExactName("HangisininBasHarfi\u0130_1"),
            FindByExactName("HangisininBasHarfi\u0130_0"),
            FindByTagSafe("soru\u0130"));
        soruO = FindFirst(FindByExactName("HangisininBasHarfO_0"), FindByTagSafe("soruO"));
        soruUu = FindFirst(FindByExactName("HangisininBasHarfU_1"), FindByTagSafe("soruU"));

        // Slot pozisyonlarını kaydet
        if (arababalon != null) posAraba = arababalon.transform.position;
        if (elmabalon != null) posElma = elmabalon.transform.position;
        if (orumcekbalon != null) posOrumcek = orumcekbalon.transform.position;
        if (uzumbalon != null) posUzum = uzumbalon.transform.position;

        // Slot: araba <-> uçak
        if (ucakbalon != null && arababalon != null)
        {
            ucakbalon.transform.position = arababalon.transform.position;
            posUcak = arababalon.transform.position;
            SetActiveSafe(ucakbalon, false);
        }
        else if (ucakbalon != null)
            posUcak = ucakbalon.transform.position;

        // Slot: elma <-> oyuncak
        if (oyuncakbalon != null && elmabalon != null)
        {
            oyuncakbalon.transform.position = elmabalon.transform.position;
            posOyuncak = elmabalon.transform.position;
            SetActiveSafe(oyuncakbalon, false);
        }
        else if (oyuncakbalon != null)
            posOyuncak = oyuncakbalon.transform.position;

        // Slot: üzüm <-> ıslak
        if (islakmendilbalon != null)
            islakmendilbalon.transform.localScale = new Vector3(0.36789301f, 0.372160017f, 0.970443606f);
        if (uzumbalon != null && islakmendilbalon != null)
        {
            islakmendilbalon.transform.position = uzumbalon.transform.position;
            posIslak = uzumbalon.transform.position;
            SetActiveSafe(islakmendilbalon, false);
        }
        else if (islakmendilbalon != null)
            posIslak = islakmendilbalon.transform.position;

        // Slot: örümcek <-> inek
        if (inekbalon != null)
            inekbalon.transform.localScale = new Vector3(0.3216f, 0.3481f, 0.9704f);
        if (orumcekbalon != null && inekbalon != null)
        {
            inekbalon.transform.position = orumcekbalon.transform.position;
            posInek = orumcekbalon.transform.position;
            SetActiveSafe(inekbalon, false);
        }
        else if (inekbalon != null)
            posInek = inekbalon.transform.position;

        // SecenekBalonu bileşenlerini ekle / yapılandır
        arabaBalon = BalonHazirla(arababalon, "araba", 1, dogruAraba, yanlisAraba, posAraba);
        elmaBalon = BalonHazirla(elmabalon, "elma", 2, dogruElma, yanlisElma, posElma);
        orumcekBalon = BalonHazirla(orumcekbalon, "orumcek", 3, dogruOrumcek, yanlisOrumcek, posOrumcek);
        uzumBalon = BalonHazirla(uzumbalon, "uzum", 4, dogruUzum, yanlisUzum, posUzum);
        islakBalon = BalonHazirla(islakmendilbalon, "islakmendil", 5, dogruIslak, yanlisIslak, posIslak);
        inekBalon = BalonHazirla(inekbalon, "inek", 6, dogruInek, yanlisInek, posInek);
        oyuncakBalon = BalonHazirla(oyuncakbalon, "oyuncak", 7, dogruOyuncak, yanlisOyuncak, posOyuncak);
        ucakBalon = BalonHazirla(ucakbalon, "ucak", 8, dogruUcak, yanlisUcak, posUcak);

        // Feedback objelerini gizle ve öne al
        GameObject[] feedback = {
            yanlisAraba, yanlisElma, yanlisOrumcek, yanlisUzum,
            yanlisIslak, yanlisInek, yanlisOyuncak, yanlisUcak,
            dogruAraba, dogruElma, dogruOrumcek, dogruUzum,
            dogruIslak, dogruInek, dogruOyuncak, dogruUcak
        };
        for (int i = 0; i < feedback.Length; i++)
        {
            SetActiveSafe(feedback[i], false);
            BringToFront(feedback[i]);
        }

        Debug.Log(
            "Balon ucak/oyuncak: " + (ucakBalon != null) + "/" + (oyuncakBalon != null)
            + " | Soru O/U: " + (soruO != null) + "/" + (soruUu != null));

        SureYazisiniOlustur();
        YeniSoru();
    }

    // GO üzerinde SecenekBalonu oluştur ve ayarla
    SecenekBalonu BalonHazirla(GameObject go, string adi, int harfId, GameObject dogruFb, GameObject yanlisFb, Vector3 slotPos)
    {
        if (go == null)
            return null;

        SecenekBalonu balon = go.GetComponent<SecenekBalonu>();
        if (balon == null)
            balon = go.AddComponent<SecenekBalonu>();

        balon.Ayarla(adi, harfChar[harfId], harfId, dogruFb, yanlisFb, slotPos);
        balon.TiklanabilirYap();
        tumBalonlar.Add(balon);
        return balon;
    }

    // ----------------- Bulucu -----------------

    GameObject FindInScene(System.Func<Transform, bool> esles)
    {
        Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t == null) continue;
            if (!t.gameObject.scene.IsValid()) continue;
            if (esles(t)) return t.gameObject;
        }
        return null;
    }

    // Daha önce sorgulanan tag sonuçları (tanımsız tag spamını kes)
    private readonly Dictionary<string, bool> tagTanimCache = new Dictionary<string, bool>();

    // Tag projede tanımlı mı? (tanımsızsa CompareTag konsola hata basar)
    bool TagTanimliMi(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            return false;

        bool cached;
        if (tagTanimCache.TryGetValue(tag, out cached))
            return cached;

        bool tanimli;
        try
        {
            // Tag yoksa UnityException atar; tanımlıysa (boş olsa bile) dizi döner
            GameObject.FindGameObjectsWithTag(tag);
            tanimli = true;
        }
        catch (UnityException)
        {
            tanimli = false;
        }

        tagTanimCache[tag] = tanimli;
        return tanimli;
    }

    GameObject FindByTagSafe(string tag)
    {
        // Tanımsız tag ile tüm Transform'larda CompareTag çağırma
        if (!TagTanimliMi(tag))
            return null;

        return FindInScene(t =>
        {
            try { return t.CompareTag(tag); }
            catch { return false; }
        });
    }

    GameObject FindByExactName(string exactName)
    {
        return FindInScene(t => string.Equals(t.name, exactName, System.StringComparison.Ordinal));
    }

    GameObject FindByNameContains(string namePart)
    {
        return FindInScene(t => t.name.IndexOf(namePart, System.StringComparison.Ordinal) >= 0);
    }

    GameObject FindFirst(params GameObject[] adaylar)
    {
        for (int i = 0; i < adaylar.Length; i++)
            if (adaylar[i] != null) return adaylar[i];
        return null;
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

    void SlotAyarla(SecenekBalonu a, SecenekBalonu b, Vector3 slotPos, bool aAktif)
    {
        GameObject goA = a != null ? a.gameObject : null;
        GameObject goB = b != null ? b.gameObject : null;
        SetActiveSafe(goA, aAktif);
        SetActiveSafe(goB, !aAktif);
        if (aAktif)
        {
            if (a != null) a.transform.position = slotPos;
        }
        else
        {
            if (b != null) b.transform.position = slotPos;
        }
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
        sureYazi = yaziGo.AddComponent<Text>();
        sureYazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (sureYazi.font == null)
            sureYazi.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        sureYazi.fontSize = 42;
        sureYazi.alignment = TextAnchor.UpperCenter;
        sureYazi.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        sureYazi.text = "01:00";

        RectTransform rt = sureYazi.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -20f);
        rt.sizeDelta = new Vector2(300f, 60f);
    }

    void SureYazisiniGuncelle()
    {
        if (sureYazi == null) return;
        int sn = Mathf.CeilToInt(kalanSure);
        sureYazi.text = string.Format("{0:00}:{1:00}", sn / 60, sn % 60);
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
            ucanBalon.transform.position += Vector3.up * yukselmehizi * Time.deltaTime;

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

        // Önce collider isabeti
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

        // Collider kaçırırsa sprite sınırından bul
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
        soruDestesi.AddRange(soruHavuzu);
        for (int i = soruDestesi.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = soruDestesi[i];
            soruDestesi[i] = soruDestesi[j];
            soruDestesi[j] = tmp;
        }
    }

    // Yeni rastgele (desteden) soru seç
    public void YeniSoru()
    {
        if (oyunBitti || kalanSure <= 0f)
        {
            if (!oyunBitti) OyunuBitir();
            return;
        }

        if (soruDestesi.Count == 0)
            DesteyiDoldurVeKaristir();

        secilenharf = soruDestesi[0];
        soruDestesi.RemoveAt(0);
        SoruyuGuncelle();
        Debug.Log("Yeni soru: " + secilenharf + " (" + harfAdlari[secilenharf] + ")");
    }

    void SoruyuGuncelle()
    {
        SetActiveSafe(soruA, secilenharf == 1);
        SetActiveSafe(soruE, secilenharf == 2);
        SetActiveSafe(soruOe, secilenharf == 3);
        SetActiveSafe(soruUe, secilenharf == 4);
        SetActiveSafe(soruI, secilenharf == 5);
        SetActiveSafe(soruIi, secilenharf == 6);
        SetActiveSafe(soruO, secilenharf == 7);
        SetActiveSafe(soruUu, secilenharf == 8);

        char soruChar = harfChar[secilenharf];

        // Slot: araba <-> uçak | A→araba, U→uçak, diğer→rastgele
        bool arabaAktif;
        if (secilenharf == 1) arabaAktif = true;
        else if (secilenharf == 8) arabaAktif = false;
        else arabaAktif = Random.Range(0, 2) == 0;
        SlotAyarla(arabaBalon, ucakBalon, posAraba, arabaAktif);
        if (!arabaAktif) posUcak = posAraba;
        if (ucakBalon != null) ucakBalon.SlotPozisyonunuGuncelle(posUcak);

        // Slot: elma <-> oyuncak
        bool elmaAktif;
        if (secilenharf == 2) elmaAktif = true;
        else if (secilenharf == 7) elmaAktif = false;
        else elmaAktif = Random.Range(0, 2) == 0;
        SlotAyarla(elmaBalon, oyuncakBalon, posElma, elmaAktif);
        if (!elmaAktif) posOyuncak = posElma;
        if (oyuncakBalon != null) oyuncakBalon.SlotPozisyonunuGuncelle(posOyuncak);

        // Slot: üzüm <-> ıslak
        bool uzumAktif;
        if (secilenharf == 4) uzumAktif = true;
        else if (secilenharf == 5) uzumAktif = false;
        else uzumAktif = Random.Range(0, 2) == 0;
        SlotAyarla(uzumBalon, islakBalon, posUzum, uzumAktif);
        if (!uzumAktif) posIslak = posUzum;
        if (islakBalon != null) islakBalon.SlotPozisyonunuGuncelle(posIslak);

        // Slot: örümcek <-> inek
        bool orumcekAktif;
        if (secilenharf == 3) orumcekAktif = true;
        else if (secilenharf == 6) orumcekAktif = false;
        else orumcekAktif = Random.Range(0, 2) == 0;
        SlotAyarla(orumcekBalon, inekBalon, posOrumcek, orumcekAktif);
        if (!orumcekAktif) posInek = posOrumcek;
        if (inekBalon != null) inekBalon.SlotPozisyonunuGuncelle(posInek);

        // Tüm balonlara aktif soru harfini yaz
        for (int i = 0; i < tumBalonlar.Count; i++)
        {
            if (tumBalonlar[i] != null)
                tumBalonlar[i].SoruHarfiniGuncelle(soruChar);
        }
    }

    // Kılavuz imzası: yalnızca bool (balonsuz çağrı desteklenmez — overload kullanılır)
    public void SecenekSecildi(bool dogruMu)
    {
        Debug.LogWarning("SecenekSecildi(bool) balon bilgisiz çağrıldı; SecenekBalonu.Tiklandi kullanın.");
    }

    // Seçenek sonucunu işle (doğru → uçuş + yeni soru, yanlış → çarpı)
    public void SecenekSecildi(bool dogruMu, SecenekBalonu secenek)
    {
        if (oyunBitti || islemYapiliyor || kalanSure <= 0f) return;
        if (secenek == null) return;

        Debug.Log("Tık: " + secenek.objeAdi + " | soru: " + secilenharf + " | dogru: " + dogruMu);

        if (dogruMu)
        {
            StartCoroutine(DogruCevapSureci(
                secenek.gameObject,
                secenek.dogruFeedback,
                secenek.slotPozisyonu,
                secenek.harfId));
        }
        else
        {
            yanlisSayisi++;
            if (harfYanlisSayisi.ContainsKey(secilenharf))
                harfYanlisSayisi[secilenharf]++;
            else
                harfYanlisSayisi[secilenharf] = 1;

            StartCoroutine(YanlisCevapSureci(secenek.yanlisFeedback, secenek.gameObject));
        }
    }

    IEnumerator DogruCevapSureci(GameObject balon, GameObject tiki, Vector3 normalKonum, int tamamlananHarfId)
    {
        if (balon == null)
        {
            Debug.LogWarning("Balon null, doğru cevap iptal: harf " + tamamlananHarfId);
            yield break;
        }

        islemYapiliyor = true;
        dogruSayisi++;
        harfDogruBildimi[tamamlananHarfId] = true;

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

    IEnumerator YanlisCevapSureci(GameObject carpi, GameObject balon)
    {
        islemYapiliyor = true;

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

        yield return new WaitForSeconds(1.0f);
        SetActiveSafe(carpi, false);

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

        SetActiveSafe(soruA, false);
        SetActiveSafe(soruE, false);
        SetActiveSafe(soruOe, false);
        SetActiveSafe(soruUe, false);
        SetActiveSafe(soruI, false);
        SetActiveSafe(soruIi, false);
        SetActiveSafe(soruO, false);
        SetActiveSafe(soruUu, false);

        if (sureYazi != null)
            sureYazi.gameObject.SetActive(false);

        KapanisEkraniniGoster();
    }

    void KapanisEkraniniGoster()
    {
        GameObject canvasGo = new GameObject("KapanisCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        Image panel = panelGo.AddComponent<Image>();
        panel.color = new Color(0.98f, 0.94f, 0.86f, 0.96f);
        RectTransform panelRt = panel.rectTransform;
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Text baslik = YaziEkle(panelGo.transform, "Oyun Bitti!", 56, new Vector2(0f, 420f), font, TextAnchor.MiddleCenter, 900f);
        baslik.color = new Color(0.2f, 0.35f, 0.25f);

        Text skor = YaziEkle(panelGo.transform,
            "Doğru: " + dogruSayisi + "    Yanlış: " + yanlisSayisi,
            36, new Vector2(0f, 350f), font, TextAnchor.MiddleCenter, 900f);
        skor.color = new Color(0.25f, 0.25f, 0.3f);

        Text harfBaslik = YaziEkle(panelGo.transform, "Harf Sonuçları", 32, new Vector2(0f, 280f), font, TextAnchor.MiddleCenter, 900f);
        harfBaslik.color = new Color(0.3f, 0.3f, 0.35f);

        float y = 220f;
        for (int i = 0; i < soruHavuzu.Count; i++)
        {
            int harfId = soruHavuzu[i];
            int yanlis = harfYanlisSayisi.ContainsKey(harfId) ? harfYanlisSayisi[harfId] : 0;
            bool dogruBildimi = harfDogruBildimi.ContainsKey(harfId) && harfDogruBildimi[harfId];
            bool gecti = dogruBildimi && yanlis <= 2;
            string isaret = gecti ? "✓" : "✗";
            string yorum = YanlisYorumu(yanlis);
            string satir = harfAdlari[harfId] + "  " + isaret
                + "   " + yanlis + " yanlış"
                + "  —  " + yorum;

            Text harfYazi = YaziEkle(panelGo.transform, satir, 28, new Vector2(0f, y), font, TextAnchor.MiddleCenter, 1100f);
            harfYazi.color = gecti
                ? new Color(0.15f, 0.55f, 0.25f)
                : new Color(0.75f, 0.2f, 0.2f);
            y -= 42f;
        }

        GameObject btnGo = new GameObject("AnaMenuButon");
        btnGo.transform.SetParent(panelGo.transform, false);
        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.85f, 0.55f, 0.25f, 1f);
        btnGo.AddComponent<Button>();
        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = new Vector2(0f, y - 40f);
        btnRt.sizeDelta = new Vector2(280f, 70f);

        Text btnText = YaziEkle(btnGo.transform, "Ana Menü", 32, Vector2.zero, font, TextAnchor.MiddleCenter, 280f);
        btnText.color = Color.white;
        RectTransform btnTextRt = btnText.rectTransform;
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;
    }

    string YanlisYorumu(int yanlis)
    {
        if (yanlis <= 0) return "mükemmel";
        if (yanlis <= 3) return "iyi, biraz çalışması gerek";
        if (yanlis <= 6) return "orta, çalışmalı";
        return "kötü, çok çalışmalı";
    }

    Text YaziEkle(Transform parent, string icerik, int fontSize, Vector2 pos, Font font,
        TextAnchor hiza = TextAnchor.MiddleCenter, float genislik = 800f)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.alignment = hiza;
        t.text = icerik;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(genislik, 50f);
        return t;
    }
}
