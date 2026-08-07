using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class codes : MonoBehaviour
{
    // 1=A(araba) 2=E(elma) 3=Ö(örümcek) 4=Ü(üzüm)
    // 5=I(ıslak) 6=İ(inek) 7=O(oyuncak) 8=U(uçak)
    // Slotlar: araba↔uçak | elma↔oyuncak | üzüm↔ıslak | örümcek↔inek
    private readonly Dictionary<string, int> dogruHarf = new Dictionary<string, int>
    {
        { "araba", 1 },
        { "elma", 2 },
        { "orumcek", 3 },
        { "uzum", 4 },
        { "islakmendil", 5 },
        { "inek", 6 },
        { "oyuncak", 7 },
        { "ucak", 8 }
    };

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

    private float yukselmehizi = 5.0f;
    private float ucusSuresi = 2.0f;
    private float oyunSuresi = 60f;
    private float kalanSure;
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

    private GameObject arababalon, elmabalon, orumcekbalon, uzumbalon;
    private GameObject islakmendilbalon, inekbalon, oyuncakbalon, ucakbalon;

    private GameObject dogruAraba, dogruElma, dogruOrumcek, dogruUzum;
    private GameObject dogruIslak, dogruInek, dogruOyuncak, dogruUcak;
    private GameObject yanlisAraba, yanlisElma, yanlisOrumcek, yanlisUzum;
    private GameObject yanlisIslak, yanlisInek, yanlisOyuncak, yanlisUcak;

    private GameObject soruA, soruE, soruOe, soruUe, soruI, soruIi, soruO, soruUu;

    private Vector3 posAraba, posElma, posOrumcek, posUzum;
    private Vector3 posIslak, posInek, posOyuncak, posUcak;

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

    GameObject FindByTagSafe(string tag)
    {
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

    void TiklanabilirYap(GameObject balon)
    {
        if (balon == null) return;

        Button[] butonlar = balon.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < butonlar.Length; i++)
            butonlar[i].enabled = false;

        BoxCollider2D box = balon.GetComponent<BoxCollider2D>();
        if (box == null) box = balon.AddComponent<BoxCollider2D>();

        SpriteRenderer sr = balon.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            box.size = sr.sprite.bounds.size;
        else
            box.size = new Vector2(7f, 12f);
    }

    void SlotAyarla(GameObject a, GameObject b, Vector3 slotPos, bool aAktif)
    {
        SetActiveSafe(a, aAktif);
        SetActiveSafe(b, !aAktif);
        if (aAktif)
        {
            if (a != null) a.transform.position = slotPos;
        }
        else
        {
            if (b != null) b.transform.position = slotPos;
        }
    }

    // ----------------- Start -----------------

    void Start()
    {
        anaKamera = Camera.main;
        kalanSure = oyunSuresi;

        for (int i = 0; i < soruHavuzu.Count; i++)
        {
            harfDogruBildimi[soruHavuzu[i]] = false;
            harfYanlisSayisi[soruHavuzu[i]] = 0;
        }

        arababalon = FindByTagSafe("arababalon");
        elmabalon = FindByTagSafe("elmabalon");
        orumcekbalon = FindByTagSafe("orumcekbalon");
        uzumbalon = FindByTagSafe("uzumbalon");
        islakmendilbalon = FindFirst(
            FindByTagSafe("islakmendilbalon"),
            FindByNameContains("shb\u0131slakmendil"));
        inekbalon = FindFirst(
            FindByTagSafe("inekbalon"),
            FindByNameContains("shbinek"));
        oyuncakbalon = FindFirst(
            FindByTagSafe("oyuncakbalon"),
            FindByNameContains("shboyuncak"));
        ucakbalon = FindFirst(
            FindByTagSafe("ucakbalon"),
            FindByNameContains("shbu\u00e7ak"));

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

        TiklanabilirYap(arababalon);
        TiklanabilirYap(elmabalon);
        TiklanabilirYap(orumcekbalon);
        TiklanabilirYap(uzumbalon);
        TiklanabilirYap(islakmendilbalon);
        TiklanabilirYap(inekbalon);
        TiklanabilirYap(oyuncakbalon);
        TiklanabilirYap(ucakbalon);

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
            "Balon ucak/oyuncak: " + (ucakbalon != null) + "/" + (oyuncakbalon != null)
            + " | Soru O/U: " + (soruO != null) + "/" + (soruUu != null));

        SureYazisiniOlustur();
        YeniSoruSec();
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

        Collider2D hit = Physics2D.OverlapPoint(dunya2D);
        if (hit != null)
        {
            GameObject basilan = hit.gameObject;
            if (BalonaAitMi(basilan, arababalon)) { ButonTiklandi("araba"); return; }
            if (BalonaAitMi(basilan, elmabalon)) { ButonTiklandi("elma"); return; }
            if (BalonaAitMi(basilan, orumcekbalon)) { ButonTiklandi("orumcek"); return; }
            if (BalonaAitMi(basilan, uzumbalon)) { ButonTiklandi("uzum"); return; }
            if (BalonaAitMi(basilan, islakmendilbalon)) { ButonTiklandi("islakmendil"); return; }
            if (BalonaAitMi(basilan, inekbalon)) { ButonTiklandi("inek"); return; }
            if (BalonaAitMi(basilan, oyuncakbalon)) { ButonTiklandi("oyuncak"); return; }
            if (BalonaAitMi(basilan, ucakbalon)) { ButonTiklandi("ucak"); return; }
        }

        string balonAdi = SpriteSinirindanBul(dunya2D);
        if (balonAdi != null) ButonTiklandi(balonAdi);
    }

    bool BalonaAitMi(GameObject basilan, GameObject balon)
    {
        if (balon == null || basilan == null || !balon.activeInHierarchy) return false;
        Transform t = basilan.transform;
        while (t != null)
        {
            if (t.gameObject == balon) return true;
            t = t.parent;
        }
        return false;
    }

    string SpriteSinirindanBul(Vector2 dunya2D)
    {
        if (NoktaSpriteIcinde(arababalon, dunya2D)) return "araba";
        if (NoktaSpriteIcinde(elmabalon, dunya2D)) return "elma";
        if (NoktaSpriteIcinde(orumcekbalon, dunya2D)) return "orumcek";
        if (NoktaSpriteIcinde(uzumbalon, dunya2D)) return "uzum";
        if (NoktaSpriteIcinde(islakmendilbalon, dunya2D)) return "islakmendil";
        if (NoktaSpriteIcinde(inekbalon, dunya2D)) return "inek";
        if (NoktaSpriteIcinde(oyuncakbalon, dunya2D)) return "oyuncak";
        if (NoktaSpriteIcinde(ucakbalon, dunya2D)) return "ucak";
        return null;
    }

    bool NoktaSpriteIcinde(GameObject balon, Vector2 dunya2D)
    {
        if (balon == null || !balon.activeInHierarchy) return false;
        SpriteRenderer sr = balon.GetComponent<SpriteRenderer>();
        if (sr == null || !sr.enabled || sr.sprite == null) return false;
        return sr.bounds.Contains(new Vector3(dunya2D.x, dunya2D.y, sr.bounds.center.z));
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

    void YeniSoruSec()
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

        // Slot: araba <-> uçak | A→araba, U→uçak, diğer→rastgele
        bool arabaAktif;
        if (secilenharf == 1) arabaAktif = true;
        else if (secilenharf == 8) arabaAktif = false;
        else arabaAktif = Random.Range(0, 2) == 0;
        SlotAyarla(arababalon, ucakbalon, posAraba, arabaAktif);
        if (!arabaAktif) posUcak = posAraba;

        // Slot: elma <-> oyuncak | E→elma, O→oyuncak, diğer→rastgele
        bool elmaAktif;
        if (secilenharf == 2) elmaAktif = true;
        else if (secilenharf == 7) elmaAktif = false;
        else elmaAktif = Random.Range(0, 2) == 0;
        SlotAyarla(elmabalon, oyuncakbalon, posElma, elmaAktif);
        if (!elmaAktif) posOyuncak = posElma;

        // Slot: üzüm <-> ıslak | Ü→üzüm, I→ıslak, diğer→rastgele
        bool uzumAktif;
        if (secilenharf == 4) uzumAktif = true;
        else if (secilenharf == 5) uzumAktif = false;
        else uzumAktif = Random.Range(0, 2) == 0;
        SlotAyarla(uzumbalon, islakmendilbalon, posUzum, uzumAktif);
        if (!uzumAktif) posIslak = posUzum;

        // Slot: örümcek <-> inek | Ö→örümcek, İ→inek, diğer→rastgele
        bool orumcekAktif;
        if (secilenharf == 3) orumcekAktif = true;
        else if (secilenharf == 6) orumcekAktif = false;
        else orumcekAktif = Random.Range(0, 2) == 0;
        SlotAyarla(orumcekbalon, inekbalon, posOrumcek, orumcekAktif);
        if (!orumcekAktif) posInek = posOrumcek;
    }

    void ButonTiklandi(string butonAdi)
    {
        if (oyunBitti || islemYapiliyor || kalanSure <= 0f) return;
        if (!dogruHarf.ContainsKey(butonAdi)) return;

        bool dogruMu = dogruHarf[butonAdi] == secilenharf;
        Debug.Log("Tık: " + butonAdi + " | soru: " + secilenharf + " | dogru: " + dogruMu);

        if (dogruMu)
        {
            if (butonAdi == "araba")
                StartCoroutine(DogruCevapSureci(arababalon, dogruAraba, posAraba, 1));
            else if (butonAdi == "elma")
                StartCoroutine(DogruCevapSureci(elmabalon, dogruElma, posElma, 2));
            else if (butonAdi == "orumcek")
                StartCoroutine(DogruCevapSureci(orumcekbalon, dogruOrumcek, posOrumcek, 3));
            else if (butonAdi == "uzum")
                StartCoroutine(DogruCevapSureci(uzumbalon, dogruUzum, posUzum, 4));
            else if (butonAdi == "islakmendil")
                StartCoroutine(DogruCevapSureci(islakmendilbalon, dogruIslak, posIslak, 5));
            else if (butonAdi == "inek")
                StartCoroutine(DogruCevapSureci(inekbalon, dogruInek, posInek, 6));
            else if (butonAdi == "oyuncak")
                StartCoroutine(DogruCevapSureci(oyuncakbalon, dogruOyuncak, posOyuncak, 7));
            else if (butonAdi == "ucak")
                StartCoroutine(DogruCevapSureci(ucakbalon, dogruUcak, posUcak, 8));
        }
        else
        {
            yanlisSayisi++;
            if (harfYanlisSayisi.ContainsKey(secilenharf))
                harfYanlisSayisi[secilenharf]++;
            else
                harfYanlisSayisi[secilenharf] = 1;

            if (butonAdi == "araba")
                StartCoroutine(YanlisCevapSureci(yanlisAraba, arababalon));
            else if (butonAdi == "elma")
                StartCoroutine(YanlisCevapSureci(yanlisElma, elmabalon));
            else if (butonAdi == "orumcek")
                StartCoroutine(YanlisCevapSureci(yanlisOrumcek, orumcekbalon));
            else if (butonAdi == "uzum")
                StartCoroutine(YanlisCevapSureci(yanlisUzum, uzumbalon));
            else if (butonAdi == "islakmendil")
                StartCoroutine(YanlisCevapSureci(yanlisIslak, islakmendilbalon));
            else if (butonAdi == "inek")
                StartCoroutine(YanlisCevapSureci(yanlisInek, inekbalon));
            else if (butonAdi == "oyuncak")
                StartCoroutine(YanlisCevapSureci(yanlisOyuncak, oyuncakbalon));
            else if (butonAdi == "ucak")
                StartCoroutine(YanlisCevapSureci(yanlisUcak, ucakbalon));
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
            YeniSoruSec();

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
