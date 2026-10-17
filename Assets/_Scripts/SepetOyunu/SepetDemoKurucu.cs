using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Demo turu: rastgele sesli harf, balon gövdesinde çok renkli harf, altta 4 obje.
/// Objelerden tam biri seçilen harfle başlar.
/// </summary>
[DefaultExecutionOrder(-50)]
public class SepetDemoKurucu : MonoBehaviour
{
    // Arkaplan görselinin proje yolu
    private const string ArkaplanYolu = "Assets/_Art/Bolum1/Arkaplan/arkaplan_adiyaman.png";

    // Balon yoksa kullanılacak görsel
    private const string BalonYolu = "Assets/_Art/Bolum1/Balon/Balon_01.png";

    // Bir turda kaç obje
    private const int ObjeSayisi = 4;

    // Seçilen sprite ve onun baş harfi
    private struct SecilenObje
    {
        // Obje görseli
        public Sprite sprite;
        // Bu görselin baş harfi
        public char harf;
    }

    // Objelerin Start'ından önce sahneyi kur
    private void Awake()
    {
        // Ana kamerayı al
        Camera kamera = Camera.main;
        // Kamera yoksa kurulumu durdur
        if (kamera == null)
        {
            // Uyarı yaz
            Debug.LogWarning("SepetDemoKurucu: Main Camera yok.");
            // Çık
            return;
        }

        // Arkaplanı kameraya yay
        ArkaplaniHazirla(kamera);
        // Ortadaki deneme arabasını kapat
        DenemeArabasiniKapat();
        // Avatar seçimleri karakter yerleşiminde kullanılsın
        AvatarYoneticisiniGarantiEt();
        // Balonu bul veya oluştur, referans çerçeveye oturt
        GameObject balon = BalonuHazirla(kamera);
        // Özelleştirilmiş karakteri sepete (balonun arkasında) yerleştir
        BalonKarakteriniYerlestir(balon);
        // Harf kartlarını yükle
        List<HarfObjeVerisi> harfler = DoluHarfleriYukle();
        // Kart yoksa obje çıkarma
        if (harfler.Count == 0)
        {
            // Eksik veri uyarısı
            Debug.LogWarning("SepetDemoKurucu: Resources/Harfler boş.");
            // Çık
            return;
        }

        // Rastgele sesli harf seç
        HarfObjeVerisi aktif = harfler[Random.Range(0, harfler.Count)];
        // Harfi balon gövdesine, çok renkli yaz
        HarfiYaz(balon, aktif.harf, kamera);
        // 1 doğru + 3 çeldirici üret
        List<SecilenObje> secilenler = SecimleriOlustur(aktif, harfler);
        // Seçimleri karıştır
        Karistir(secilenler);
        // Alt sıraya yerleştir
        ObjeleriYerlestir(kamera, secilenler);
    }

    // Adı Arkaplan olan objeye görseli verir ve ekranı kaplatır
    private void ArkaplaniHazirla(Camera kamera)
    {
        // İsimle arkaplanı bul
        GameObject arkaplan = GameObject.Find("Arkaplan");
        // Yoksa uyar ve çık
        if (arkaplan == null)
        {
            // Eksik obje
            Debug.LogWarning("SepetDemoKurucu: Arkaplan objesi yok.");
            // Çık
            return;
        }

        // Çiziciyi al
        SpriteRenderer cizici = arkaplan.GetComponent<SpriteRenderer>();
        // Çizici yoksa ekle
        if (cizici == null)
        {
            // Sprite çizici oluştur
            cizici = arkaplan.AddComponent<SpriteRenderer>();
        }

        // _Art klasöründen arkaplan sprite'ı
        Sprite sprite = ArtSpriteYukle(ArkaplanYolu);
        // Yüklendiyse ata
        if (sprite != null)
        {
            // Görseli değiştir
            cizici.sprite = sprite;
        }

        // Her şeyin arkasında kalsın
        cizici.sortingOrder = -10;
        // Sprite hâlâ yoksa ölçekleme
        if (cizici.sprite == null)
        {
            // Ölçek için görsel şart
            return;
        }

        // Kameranın gördüğü yükseklik
        float yukseklik = kamera.orthographicSize * 2f;
        // Kameranın gördüğü genişlik
        float genislik = yukseklik * kamera.aspect;
        // Sprite'ın yerel boyutu
        Vector2 boyut = cizici.sprite.bounds.size;
        // Genişliğe göre ölçek
        float olcekX = boyut.x > 0.001f ? genislik / boyut.x : 1f;
        // Yüksekliğe göre ölçek
        float olcekY = boyut.y > 0.001f ? yukseklik / boyut.y : 1f;
        // Ekranı kaplamak için büyük olan
        float olcek = Mathf.Max(olcekX, olcekY);
        // Görsel merkezi kamera ortasına gelsin
        MerkezeOturt(arkaplan.transform, cizici.sprite, Vector3.zero, olcek);

        // Aynı arkaplanın ikinci kopyasını kapat
        SpriteRenderer[] hepsi = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        // Her çiziciyi dolaş
        for (int i = 0; i < hepsi.Length; i++)
        {
            // Seçilen arkaplan kalsın
            if (hepsi[i].gameObject == arkaplan)
            {
                // Atla
                continue;
            }

            // Adında arkaplan geçen diğer objeler
            if (hepsi[i].gameObject.name.ToLowerInvariant().Contains("arkaplan"))
            {
                // Çift görüntüyü kapat
                hepsi[i].gameObject.SetActive(false);
            }
        }
    }

    // Sahneye elle konmuş deneme arabasını gizle
    private void DenemeArabasiniKapat()
    {
        // Elle bırakılan araba
        GameObject araba = GameObject.Find("araba_0");
        // Varsa kapat
        if (araba != null)
        {
            // Demo objeleriyle çakışmasın
            araba.SetActive(false);
        }
    }

    // Sahnedeki balonu kullanır; yoksa Balon_01 oluşturur
    private GameObject BalonuHazirla(Camera kamera)
    {
        // Tag veya Balon ile başlayan isim
        GameObject balon = BalonuBul();
        // Hiç yoksa yeni obje aç
        if (balon == null)
        {
            // Boş balon objesi
            balon = new GameObject("Balon");
            // Çizici ekle
            SpriteRenderer yeni = balon.AddComponent<SpriteRenderer>();
            // Varsayılan balon görseli
            yeni.sprite = ArtSpriteYukle(BalonYolu);
        }

        // Adı Balon olsun
        balon.name = "Balon";
        // Bırakma kontrolü bu tag'i arar
        balon.tag = "Balon";

        // Çiziciyi al
        SpriteRenderer cizici = balon.GetComponent<SpriteRenderer>();
        // Çizici yoksa ekle
        if (cizici == null)
        {
            // Sprite çizici
            cizici = balon.AddComponent<SpriteRenderer>();
        }

        // Görsel boşsa varsayılanı yükle
        if (cizici.sprite == null)
        {
            // Balon_01
            cizici.sprite = ArtSpriteYukle(BalonYolu);
        }

        // Objelerin altında, arkaplanın üstünde
        cizici.sortingOrder = 1;
        // Bırakma alanı görselin kendisi olsun, boş dikdörtgen olmasın
        BalonSeklineOturt(balon, cizici.sprite);

        // Sprite yoksa konumlama yapma
        if (cizici.sprite == null)
        {
            // Balonu olduğu gibi bırak
            return balon;
        }

        // Referans görsel: balon ~ekran yüksekliği, zarf üstte kesik
        float yukseklik = kamera.orthographicSize * 2f;
        float hedef = yukseklik * 1.05f;
        float spriteH = cizici.sprite.bounds.size.y;
        float olcek = spriteH > 0.001f ? hedef / spriteH : 1f;

        // Sepet ekran ortasında; üst zarf kadraj dışında kalsın
        Bounds yerel = cizici.sprite.bounds;
        float sepetLocalY = yerel.min.y + yerel.size.y * 0.06f;
        float merkezY = 0f - (sepetLocalY - yerel.center.y) * olcek;
        MerkezeOturt(balon.transform, cizici.sprite, new Vector3(0f, merkezY, 0f), olcek);
        // Hazır balonu döndür
        return balon;
    }

    // Karakter ekranındaki seçimi balon sepetinde gösterir (üst gövde; balonun arkasında)
    private void BalonKarakteriniYerlestir(GameObject balon)
    {
        if (balon == null) return;

        // Seçimler DDOL / PlayerPrefs'ten gelsin
        AvatarYoneticisiniGarantiEt();

        SpriteRenderer balonCizici = balon.GetComponent<SpriteRenderer>();
        if (balonCizici == null || balonCizici.sprite == null) return;

        // Eski kopyayı temizle
        Transform eski = balon.transform.Find("BalonKarakteri");
        if (eski != null)
            Destroy(eski.gameObject);

        GameObject kok = new GameObject("BalonKarakteri");
        kok.transform.SetParent(balon.transform, false);

        GameObject govdeGo = new GameObject("Govde");
        govdeGo.transform.SetParent(kok.transform, false);
        SpriteRenderer govdeSr = govdeGo.AddComponent<SpriteRenderer>();
        // Balonun arkasında (balon sortingOrder = 1)
        govdeSr.sortingOrder = 0;

        GameObject aksGo = new GameObject("Aksesuar");
        aksGo.transform.SetParent(kok.transform, false);
        SpriteRenderer aksSr = aksGo.AddComponent<SpriteRenderer>();
        aksSr.sortingOrder = 0;

        AvatarGorunumu gorunum = kok.AddComponent<AvatarGorunumu>();
        gorunum.govdeRenderer = govdeSr;
        gorunum.aksesuarRenderer = aksSr;

        // Karakter ekranından gelen şablon; yoksa Art'tan yükle
        if (AvatarGorunumu.SablonHazir)
        {
            gorunum.hayvanlar = AvatarGorunumu.SablonHayvanlar;
            gorunum.aksesuarlar = AvatarGorunumu.SablonAksesuarlar;
        }
        else
        {
            gorunum.hayvanlar = BasitHayvanSablonuOlustur();
            gorunum.aksesuarlar = new AksesuarGorseli[0];
        }

        // Şablon sprite'ları boşsa yine Art'a düş
        if (!HayvanSpriteVarMi(gorunum))
        {
            gorunum.hayvanlar = BasitHayvanSablonuOlustur();
            gorunum.aksesuarlar = new AksesuarGorseli[0];
        }

        Bounds yerel = balonCizici.sprite.bounds;

        // Sepet opak (~alt %18); karakter en az bunun üstüne taşmalı ki ip boşluğundan görünsün
        // Orta ip genişliğine sığdırma ikincil — önce görünürlük
        float hedefLocalH = yerel.size.y * 0.26f;
        float balonWorldW = yerel.size.x * Mathf.Max(0.001f, Mathf.Abs(balon.transform.lossyScale.x));
        float ortaIpAraligi = balonWorldW * 0.12f;
        float enBoy = SeciliGovdeEnBoyOrani(gorunum);
        float maxLocalH = (ortaIpAraligi * 1.05f)
            / Mathf.Max(0.2f, enBoy)
            / Mathf.Max(0.001f, Mathf.Abs(balon.transform.lossyScale.y));
        // İplere sığdır; ama sepet altında kaybolacak kadar küçültme
        float minLocalH = yerel.size.y * 0.20f;
        hedefLocalH = Mathf.Clamp(Mathf.Min(hedefLocalH, maxLocalH), minLocalH, yerel.size.y * 0.35f);

        gorunum.hedefGovdeYuksekligi = hedefLocalH;
        gorunum.hedefZeminY = 0f;
        gorunum.Guncelle();

        // Ayaklar sepet tabanında; gövde şeffaf ip bölgesine uzanır (balon sortingOrder=1 üstte)
        float sepetTabanY = yerel.min.y + yerel.size.y * 0.04f;
        kok.transform.localPosition = new Vector3(0f, sepetTabanY, 0f);
        kok.transform.localRotation = Quaternion.identity;
        kok.transform.localScale = Vector3.one;
        kok.transform.SetAsFirstSibling();

        if (govdeSr.sprite == null)
            Debug.LogWarning("SepetDemoKurucu: Balon karakteri sprite alamadi — Hierarchy'de BalonKarakteri/Govde kontrol et.");
    }

    // Seçili (veya ilk) hayvanın normal sprite'ı var mı
    private static bool HayvanSpriteVarMi(AvatarGorunumu gorunum)
    {
        if (gorunum == null || gorunum.hayvanlar == null || gorunum.hayvanlar.Length == 0)
            return false;
        int idx = 0;
        if (AvatarYoneticisi.Instance != null)
            idx = Mathf.Clamp(AvatarYoneticisi.Instance.seciliHayvanIndex, 0, gorunum.hayvanlar.Length - 1);
        HayvanGorseli h = gorunum.hayvanlar[idx];
        return h != null && h.normalPoz != null;
    }

    // Aktif hayvan gövdesinin içerik en/boy oranı (yoksa ~kare)
    private float SeciliGovdeEnBoyOrani(AvatarGorunumu gorunum)
    {
        HayvanGorseli[] hayvanlar = null;
        if (gorunum != null && gorunum.hayvanlar != null && gorunum.hayvanlar.Length > 0)
            hayvanlar = gorunum.hayvanlar;
        else if (AvatarGorunumu.SablonHazir)
            hayvanlar = AvatarGorunumu.SablonHayvanlar;

        int idx = 0;
        if (AvatarYoneticisi.Instance != null)
            idx = AvatarYoneticisi.Instance.seciliHayvanIndex;

        Sprite sp = null;
        if (hayvanlar != null && hayvanlar.Length > 0)
        {
            if (idx < 0 || idx >= hayvanlar.Length)
                idx = 0;
            if (hayvanlar[idx] != null)
                sp = hayvanlar[idx].normalPoz;
        }

        // Şablon yoksa varsayılan rakun sprite'ı
        if (sp == null)
            sp = ArtSpriteYukle("Assets/_Art/Avatar/Karakterler/avatar_rakun_normal.png");

        if (sp == null || sp.bounds.size.y < 0.001f)
            return 0.93f;

        return sp.bounds.size.x / sp.bounds.size.y;
    }

    // AvatarYoneticisi yoksa oluştur (PlayerPrefs seçimleriyle)
    private static void AvatarYoneticisiniGarantiEt()
    {
        if (AvatarYoneticisi.Instance != null) return;
        GameObject go = new GameObject("AvatarYoneticisi");
        go.AddComponent<AvatarYoneticisi>();
    }

    // Şablon yokken 4 hayvan normal sprite'ı
    private HayvanGorseli[] BasitHayvanSablonuOlustur()
    {
        string[] adlar = { "Kedi", "Tavşan", "Kuş", "Rakun" };
        string[] dosyalar =
        {
            "Assets/_Art/Avatar/Karakterler/avatar_kedi_normal.png",
            "Assets/_Art/Avatar/Karakterler/avatar_tavsan_normal.png",
            "Assets/_Art/Avatar/Karakterler/avatar_kus_normal.png",
            "Assets/_Art/Avatar/Karakterler/avatar_rakun_normal.png"
        };

        HayvanGorseli[] liste = new HayvanGorseli[4];
        for (int i = 0; i < 4; i++)
        {
            HayvanGorseli h = new HayvanGorseli();
            h.hayvanAdi = adlar[i];
            h.normalPoz = ArtSpriteYukle(dosyalar[i]);
            h.govdeOlcekCarpani = 1f;
            h.aksesuarOfset = new Vector2(0f, 1.8f);
            liste[i] = h;
        }
        return liste;
    }

    // Tag Balon veya adı Balon ile başlayan objeyi bulur
    private GameObject BalonuBul()
    {
        // Tag atanmış balon
        GameObject tagli = GameObject.FindGameObjectWithTag("Balon");
        // Varsa onu kullan
        if (tagli != null)
        {
            // Bulunan balon
            return tagli;
        }

        // Sahnedeki tüm çiziciler
        SpriteRenderer[] hepsi = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        // İsimle yedek ara
        for (int i = 0; i < hepsi.Length; i++)
        {
            // Obje adı
            string ad = hepsi[i].gameObject.name;
            // Tam ad veya Balon_04 gibi başlangıç
            if (ad == "Balon" || ad.StartsWith("Balon"))
            {
                // Bu balonu kullan
                return hepsi[i].gameObject;
            }
        }

        // Sahnede balon yok
        return null;
    }

    // Balonun çarpışmasını sprite kenarına oturtur; boş çevre kabul edilmez
    private void BalonSeklineOturt(GameObject balon, Sprite sprite)
    {
        // Eski geniş kutu varsa kapat
        BoxCollider2D kutu = balon.GetComponent<BoxCollider2D>();
        if (kutu != null)
        {
            // Bu karede de çarpmasın
            kutu.enabled = false;
            // Kaldır
            Destroy(kutu);
        }

        // Şekil collider'ı
        PolygonCollider2D cokgen = balon.GetComponent<PolygonCollider2D>();
        // Yoksa ekle
        if (cokgen == null)
        {
            // Balon çizimine göre çokgen
            cokgen = balon.AddComponent<PolygonCollider2D>();
        }

        // Objeyi itmesin
        cokgen.isTrigger = true;
        // Sprite yoksa şekil kurulamaz
        if (sprite == null)
        {
            // Çık
            return;
        }

        // Alfa kenarından üretilmiş parça sayısı
        int adet = sprite.GetPhysicsShapeCount();
        // Kenar yoksa çokgeni kapat
        if (adet <= 0)
        {
            // Boş alan kabul etmesin
            cokgen.enabled = false;
            // Şekil üretilmemiş
            Debug.LogWarning("SepetDemoKurucu: Balon sprite sekli yok.");
            // Çık
            return;
        }

        // Her parçayı aktar
        cokgen.pathCount = adet;
        // Nokta listesi
        List<Vector2> yol = new List<Vector2>();
        // Parçaları dolaş
        for (int i = 0; i < adet; i++)
        {
            // Önceki yolu sil
            yol.Clear();
            // Sprite yerel noktaları
            sprite.GetPhysicsShape(i, yol);
            // Çokgene yaz
            cokgen.SetPath(i, yol);
        }
    }

    // İçinde sprite olan harf kartlarını yükler
    private List<HarfObjeVerisi> DoluHarfleriYukle()
    {
        // Resources/Harfler altındaki kartlar
        HarfObjeVerisi[] tumu = Resources.LoadAll<HarfObjeVerisi>("Harfler");
        // Dolu kartlar
        List<HarfObjeVerisi> dolu = new List<HarfObjeVerisi>();
        // Kart yoksa boş liste
        if (tumu == null)
        {
            // Boş sonuç
            return dolu;
        }

        // Her kartı kontrol et
        for (int i = 0; i < tumu.Length; i++)
        {
            // Boş referansı atla
            if (tumu[i] == null || tumu[i].dogruObjeler == null)
            {
                // Sonraki kart
                continue;
            }

            // En az bir sprite var mı
            bool spriteVar = false;
            // Sprite dizisini dolaş
            for (int s = 0; s < tumu[i].dogruObjeler.Length; s++)
            {
                // Dolu sprite bulundu
                if (tumu[i].dogruObjeler[s] != null)
                {
                    // Bu kart kullanılabilir
                    spriteVar = true;
                    // Aramayı kes
                    break;
                }
            }

            // Sprite'ı olan kartı ekle
            if (spriteVar)
            {
                // Listeye al
                dolu.Add(tumu[i]);
            }
        }

        // Dolu kart listesi
        return dolu;
    }

    // 1 doğru ve 3 farklı çeldirici seçer
    private List<SecilenObje> SecimleriOlustur(HarfObjeVerisi aktif, List<HarfObjeVerisi> harfler)
    {
        // Dört slotluk liste
        List<SecilenObje> sonuc = new List<SecilenObje>(ObjeSayisi);
        // Doğru harften bir sprite
        Sprite dogru = RastgeleSprite(aktif, sonuc);
        // Doğru sprite varsa ekle
        if (dogru != null)
        {
            // Doğru seçenek
            sonuc.Add(new SecilenObje { sprite = dogru, harf = aktif.harf });
        }

        // Diğer harflerin kopyası
        List<HarfObjeVerisi> diger = new List<HarfObjeVerisi>();
        // Aktif harf dışındakileri topla
        for (int i = 0; i < harfler.Count; i++)
        {
            // Aynı kartı çeldirici yapma
            if (harfler[i] != aktif)
            {
                // Çeldirici adayı
                diger.Add(harfler[i]);
            }
        }

        // Çeldirici sırasını karıştır
        Karistir(diger);
        // Üç çeldirici dolana kadar al
        for (int i = 0; i < diger.Count && sonuc.Count < ObjeSayisi; i++)
        {
            // Bu harften kullanılmamış bir sprite
            Sprite celdirici = RastgeleSprite(diger[i], sonuc);
            // Sprite yoksa sonraki harf
            if (celdirici == null)
            {
                // Atla
                continue;
            }

            // Çeldiriciyi ekle
            sonuc.Add(new SecilenObje { sprite = celdirici, harf = diger[i].harf });
        }

        // Seçim listesi
        return sonuc;
    }

    // Listede olmayan rastgele bir sprite döndürür
    private Sprite RastgeleSprite(HarfObjeVerisi kart, List<SecilenObje> kullanilan)
    {
        // Kart veya dizi yoksa
        if (kart == null || kart.dogruObjeler == null)
        {
            // Sprite yok
            return null;
        }

        // Aday sprite'lar
        List<Sprite> adaylar = new List<Sprite>();
        // Dizideki dolu ve kullanılmamış sprite'lar
        for (int i = 0; i < kart.dogruObjeler.Length; i++)
        {
            // Boş sprite'ı atla
            Sprite s = kart.dogruObjeler[i];
            if (s == null)
            {
                // Sonraki
                continue;
            }

            // Daha önce seçildiyse atla
            if (ListedeVar(kullanilan, s))
            {
                // Tekrar etme
                continue;
            }

            // Adaylara ekle
            adaylar.Add(s);
        }

        // Aday kalmadıysa
        if (adaylar.Count == 0)
        {
            // Sprite yok
            return null;
        }

        // Rastgele bir aday
        return adaylar[Random.Range(0, adaylar.Count)];
    }

    // Sprite daha önce seçildi mi
    private bool ListedeVar(List<SecilenObje> liste, Sprite sprite)
    {
        // Listeyi dolaş
        for (int i = 0; i < liste.Count; i++)
        {
            // Aynı görsel
            if (liste[i].sprite == sprite)
            {
                // Var
                return true;
            }
        }

        // Yok
        return false;
    }

    // Sesli harfi balon gövdesine, dört ayrı renkle yazar
    private void HarfiYaz(GameObject balon, char harf, Camera kamera)
    {
        // Balon yoksa yazma
        if (balon == null)
        {
            // Çık
            return;
        }

        // Varsa mevcut rozeti kullan
        TextMeshPro tmp = balon.GetComponentInChildren<TextMeshPro>();
        // Yoksa çocuk obje aç
        if (tmp == null)
        {
            // Rozet objesi
            GameObject yazi = new GameObject("HarfRozeti");
            // Balonun çocuğu yap, dünya konumu sonra ayarlanır
            yazi.transform.SetParent(balon.transform, false);
            // Dünya uzayı yazı
            tmp = yazi.AddComponent<TextMeshPro>();
        }

        // Varsayılan TMP fontu
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        // Font bulunduysa ata
        if (font != null)
        {
            // Yazı fontu
            tmp.font = font;
        }

        // Seçilen sesli harf
        tmp.text = harf.ToString();
        // Ortala
        tmp.alignment = TextAlignmentOptions.Center;
        // Kalın görünsün
        tmp.fontStyle = FontStyles.Bold;
        // Satır kaydırma
        tmp.enableWordWrapping = false;
        // Taşsa da görünsün
        tmp.overflowMode = TextOverflowModes.Overflow;
        // Kontur kenarda kesilmesin
        tmp.extraPadding = true;
        // Köşe renkleri beyazla çarpılır; beyaz bırak
        tmp.color = Color.white;
        // Harf dört köşede farklı renk olsun
        tmp.enableVertexGradient = true;
        // Mavi-krem çizgide kaybolmayan kırmızı, turuncu, yeşil, mor
        tmp.colorGradient = new VertexGradient(
            new Color32(230, 40, 40, 255),
            new Color32(255, 145, 0, 255),
            new Color32(20, 170, 75, 255),
            new Color32(145, 45, 210, 255));
        // İlk punto; sonra gövdeye göre ölçeklenir
        tmp.fontSize = 8f;
        // Harf çizgisini şişir, gövde daha kalın görünsün
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.42f);
        // Kalın koyu kontur, çizgilerin üstünde harfi ayırır
        tmp.outlineWidth = 0.55f;
        // Kontur rengi
        tmp.outlineColor = new Color(0.08f, 0.05f, 0.12f, 1f);
        // Yazı kutusu
        tmp.rectTransform.sizeDelta = new Vector2(6f, 6f);

        // Balon ölçeğinden bağımsız dünya boyutu
        float ebeveyn = Mathf.Max(0.001f, balon.transform.lossyScale.y);
        // Ebeveyn ölçeğini nötrle
        tmp.transform.localScale = Vector3.one / ebeveyn;
        // Balon çizicisi
        SpriteRenderer balonCizici = balon.GetComponent<SpriteRenderer>();
        // Çizici varsa harfi çizgili gövdeye koy
        if (balonCizici != null && balonCizici.sprite != null)
        {
            // Sepetin üstüne, ekranda görünen gövdeye oturt
            HarfiBalonGovdesineKoy(tmp, balonCizici, kamera);
        }

        // Renk geçişi mesh üretilince yazılır
        tmp.ForceMeshUpdate();
        // Harfin dünya yüksekliği
        float harfBoyu = tmp.textBounds.size.y * tmp.transform.lossyScale.y;
        // Ölçü yoksa yine de parıltı koy
        if (harfBoyu < 0.05f)
        {
            // Yedek boy
            harfBoyu = 1f;
        }

        // Parıltı harfin arkasında, aynı noktada
        PariltiyiKoy(balon.transform, tmp.transform.position, harfBoyu);

        // Yazı çizicisini balonun üstüne al
        MeshRenderer ag = tmp.GetComponent<MeshRenderer>();
        if (ag != null)
        {
            // Harf objelerin altında, balonun üstünde
            ag.sortingOrder = 12;
        }
    }

    // Harfi sepetin üstündeki balon gövdesine, ekranda kalacak şekilde yerleştirir
    private void HarfiBalonGovdesineKoy(TextMeshPro tmp, SpriteRenderer cizici, Camera kamera)
    {
        // Balonun dünya sınırları
        Bounds sinir = cizici.bounds;
        // Alt yüzde sepet ve ip var; harf bunun üstündeki gövdede
        float govdeAlt = sinir.min.y + sinir.size.y * 0.27f;
        // Gövdenin üst ucu
        float govdeUst = sinir.max.y;

        // Kamera varsa harf ekranın dışında kalmasın
        if (kamera != null)
        {
            // Üst ve alt kenardan küçük pay
            float kenar = kamera.orthographicSize * 0.05f;
            // Kameranın alt sınırı
            float gorunenAlt = kamera.transform.position.y - kamera.orthographicSize + kenar;
            // Kameranın üst sınırı
            float gorunenUst = kamera.transform.position.y + kamera.orthographicSize - kenar;
            // Gövdeyi görünen alana kırp
            govdeAlt = Mathf.Max(govdeAlt, gorunenAlt);
            // Üstü de ekranda kalsın
            govdeUst = Mathf.Min(govdeUst, gorunenUst);
        }

        // Görünen gövde çok inceyse yedek bir bant kullan
        if (govdeUst <= govdeAlt + 0.05f)
        {
            // Gövdenin orta bandı
            govdeAlt = sinir.min.y + sinir.size.y * 0.45f;
            // Bandın üstü
            govdeUst = sinir.min.y + sinir.size.y * 0.80f;
        }

        // Görünen gövdenin dikey ortası
        float ortaY = (govdeAlt + govdeUst) * 0.5f;
        // Balonun yatay ortası, sepetin üstü
        tmp.transform.position = new Vector3(sinir.center.x, ortaY, 0f);
        // Mesh ölçüsü için bir kez üret
        tmp.ForceMeshUpdate();
        // Gövde yüksekliğini neredeyse doldur
        float hedefYukseklik = (govdeUst - govdeAlt) * 0.98f;
        // Çok geniş balonda harf devleşmesin
        float hedefGenislik = sinir.size.x * 0.5f;
        // İkisinden küçük olan
        float hedef = Mathf.Min(hedefYukseklik, hedefGenislik);
        // Şimdiki dünya yüksekliği
        float mevcut = tmp.textBounds.size.y * tmp.transform.lossyScale.y;
        // Ölçek çarpanı
        if (mevcut > 0.001f && hedef > 0.001f)
        {
            // Hedef boyuta getir
            tmp.transform.localScale *= hedef / mevcut;
        }
    }

    // Harfin arkasına yumuşak ışıltı ve küçük yıldızlar koyar
    private void PariltiyiKoy(Transform balon, Vector3 konum, float harfBoyu)
    {
        // Varsa aynı parıltıyı kullan
        Transform eski = balon.Find("HarfPariltisi");
        // Yeni ya da eski obje
        GameObject go;
        // Daha önce üretildiyse onu al
        if (eski != null)
        {
            // Mevcut parıltı
            go = eski.gameObject;
        }
        else
        {
            // Parıltı objesi
            go = new GameObject("HarfPariltisi");
            // Balonun çocuğu yap
            go.transform.SetParent(balon, false);
        }

        // Çiziciyi al
        SpriteRenderer cizici = go.GetComponent<SpriteRenderer>();
        // Yoksa ekle
        if (cizici == null)
        {
            // Sprite çizici
            cizici = go.AddComponent<SpriteRenderer>();
        }

        // Üretilmiş ışıltı görseli
        cizici.sprite = PariltiSpriteAl();
        // Daha beyaz ve yoğun
        cizici.color = new Color(1f, 1f, 1f, 0.82f);
        // Balonun üstünde, harfin ve sürüklenecek objenin altında
        cizici.sortingOrder = 2;
        // Harfle aynı dünya noktası
        go.transform.position = new Vector3(konum.x, konum.y, 0f);
        // Sprite 1 birim; balon ölçeğini nötrle
        float ebeveyn = Mathf.Max(0.001f, balon.lossyScale.y);
        // Harften biraz büyük hale; kalınlaşmaz, yıldızlar sıklaşır
        float hedef = harfBoyu * 1.85f;
        // Yerel ölçek
        go.transform.localScale = Vector3.one * (hedef / ebeveyn);
        // Nabız bileşenini al
        HarfPariltisi nabiz = go.GetComponent<HarfPariltisi>();
        // Yoksa ekle
        if (nabiz == null)
        {
            // Yavaş büyüyüp dönsün
            nabiz = go.AddComponent<HarfPariltisi>();
        }

        // Taban ölçeği bu boy olsun
        nabiz.TabanOlcegiKaydet(go.transform.localScale);
    }

    // Parıltı sprite'ı bir kez üretilir
    private static Sprite pariltiSprite;

    // Yumuşak hale ve kenar yıldızlarından sprite üretir
    private static Sprite PariltiSpriteAl()
    {
        // Daha önce üretildiyse onu ver
        if (pariltiSprite != null)
        {
            // Hazır görsel
            return pariltiSprite;
        }

        // Kare doku kenarı
        const int n = 128;
        // Şeffaf doku
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        // Kenarlar tekrar etmesin
        tex.wrapMode = TextureWrapMode.Clamp;
        // Yumuşak görünsün
        tex.filterMode = FilterMode.Bilinear;
        // Piksel dizisi
        Color[] px = new Color[n * n];
        // Doku merkezi
        float orta = (n - 1) * 0.5f;
        // Hale yarıçapı
        float yaricap = n * 0.48f;
        // Her piksel
        for (int y = 0; y < n; y++)
        {
            // Satır
            for (int x = 0; x < n; x++)
            {
                // Merkeze göre yatay
                float dx = (x - orta) / yaricap;
                // Merkeze göre dikey
                float dy = (y - orta) / yaricap;
                // Merkeze uzaklık
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                // Yumuşak daire
                float hale = Mathf.Clamp01(1f - d);
                // Kenarı daha da yumuşat
                hale = hale * hale;
                // Mutlak yatay
                float ax = Mathf.Abs(dx);
                // Mutlak dikey
                float ay = Mathf.Abs(dy);
                // İnce artı kolu
                float arti = Mathf.Clamp01(1f - Mathf.Min(ax, ay) * 9f) * Mathf.Clamp01(1f - Mathf.Max(ax, ay) * 1.05f);
                // İnce çapraz kol
                float capraz = Mathf.Clamp01(1f - Mathf.Abs(ax - ay) * 8f) * Mathf.Clamp01(1f - d * 1.15f);
                // İki koldan parlak olan
                float yildiz = Mathf.Max(arti, capraz * 0.8f);
                // Hale ve yıldız birlikte — bir tık daha yoğun
                float a = Mathf.Clamp01(hale * 0.95f + yildiz);
                // Dışarısı boş kalsın
                if (d > 1f && yildiz < 0.02f)
                {
                    // Şeffaf piksel
                    a = 0f;
                }

                // Beyaza yakın merkez + beyaz yıldız
                Color renk = Color.Lerp(new Color(1f, 0.97f, 0.9f, 1f), Color.white, Mathf.Clamp01(yildiz + hale * 0.55f));
                // Alfa
                renk.a = a;
                // Pikseli yaz
                px[y * n + x] = renk;
            }
        }

        // Sık ve ince yıldızlar; kol kalınlığı artmaz
        int[] yildizX = { 22, 40, 58, 76, 94, 110, 30, 48, 66, 84, 102, 18, 108, 36, 64, 92, 50, 78, 28, 100 };
        // Aynı sıradaki dikey konumlar
        int[] yildizY = { 102, 112, 96, 110, 100, 90, 78, 86, 74, 84, 70, 52, 48, 34, 24, 30, 56, 46, 64, 60 };
        // Her konuma küçük bir yıldız
        for (int s = 0; s < yildizX.Length; s++)
        {
            // 4, 5 veya 6 piksel; hepsi ince
            YildizIsle(px, n, yildizX[s], yildizY[s], 4 + (s % 3));
        }
        // Diziye bas
        tex.SetPixels(px);
        // GPU'ya gönder
        tex.Apply();
        // 1 birimlik sprite
        pariltiSprite = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), n);
        // Üretilen görsel
        return pariltiSprite;
    }

    // Küçük dört kollu yıldızı dokuya işler
    private static void YildizIsle(Color[] px, int n, int cx, int cy, int cap)
    {
        // Yıldız kutusu
        for (int y = cy - cap; y <= cy + cap; y++)
        {
            // Satır
            for (int x = cx - cap; x <= cx + cap; x++)
            {
                // Doku dışını atla
                if (x < 0 || y < 0 || x >= n || y >= n)
                {
                    // Sonraki piksel
                    continue;
                }

                // Merkeze göre yatay
                float dx = (x - cx) / (float)cap;
                // Merkeze göre dikey
                float dy = (y - cy) / (float)cap;
                // Mutlak yatay
                float ax = Mathf.Abs(dx);
                // Mutlak dikey
                float ay = Mathf.Abs(dy);
                // İnce artı kolu
                float arti = Mathf.Clamp01(1f - Mathf.Min(ax, ay) * 4.5f) * Mathf.Clamp01(1f - Mathf.Max(ax, ay));
                // Merkez çekirdeği
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                // Yuvarlak çekirdek
                float cekirdek = Mathf.Clamp01(1f - d * 2.4f);
                // Parlaklık
                float a = Mathf.Max(arti, cekirdek);
                // Boş piksele dokunma
                if (a <= 0.02f)
                {
                    // Atla
                    continue;
                }

                // Doku indeksi
                int i = y * n + x;
                // Alttaki hale
                Color once = px[i];
                // Beyaz yıldız
                Color yildiz = new Color(1f, 1f, 1f, 1f);
                // Üst üste bindir
                Color sonuc = Color.Lerp(once, yildiz, a);
                // Daha parlak alfayı tut
                sonuc.a = Mathf.Max(once.a, a);
                // Pikseli yaz
                px[i] = sonuc;
            }
        }
    }

    // Dört objeyi ekranın alt şeridine sığdırır
    private void ObjeleriYerlestir(Camera kamera, List<SecilenObje> secilenler)
    {
        // Seçim yoksa çık
        if (secilenler == null || secilenler.Count == 0)
        {
            // Yerleştirilecek obje yok
            return;
        }

        // Ekran yüksekliği
        float yukseklik = kamera.orthographicSize * 2f;
        // Ekran genişliği
        float genislik = yukseklik * kamera.aspect;
        // Kenar boşluğu
        float kenar = genislik * 0.04f;
        // Dört slota bölünen iç genişlik
        float slot = (genislik - kenar * 2f) / ObjeSayisi;
        // Objenin sığacağı en büyük kenar
        float objeMax = Mathf.Min(slot * 0.78f, yukseklik * 0.22f);
        // Alt şeridin dikey merkezi
        float altY = -kamera.orthographicSize + objeMax * 0.62f;

        // Grup objesi
        GameObject grup = new GameObject("Objeler");
        // Her seçimi kendi slotuna koy
        for (int i = 0; i < secilenler.Count && i < ObjeSayisi; i++)
        {
            // Bu slotun yatay merkezi
            float x = -genislik * 0.5f + kenar + slot * (i + 0.5f);
            // Objeyi üret
            GameObject obje = ObjeOlustur(secilenler[i]);
            // Grubun çocuğu yap
            obje.transform.SetParent(grup.transform, true);
            // Slota sığdır
            Sprite sprite = secilenler[i].sprite;
            // En uzun kenara göre tek ölçek
            float uzun = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            // Sıfıra bölme
            float olcek = uzun > 0.001f ? objeMax / uzun : 1f;
            // Görsel merkezi slotta dursun
            MerkezeOturt(obje.transform, sprite, new Vector3(x, altY, 0f), olcek);
            // Kontrast için harftekiyle aynı parıltı (objenin altında)
            SpriteRenderer objeCizici = obje.GetComponent<SpriteRenderer>();
            float objeBoyu = objeCizici != null
                ? Mathf.Max(objeCizici.bounds.size.x, objeCizici.bounds.size.y)
                : objeMax;
            ObjePariltiyiKoy(obje, objeBoyu);
        }
    }

    // Alt objelerin arkasına harfle aynı parıltıyı koyar (sürüklerken birlikte gider)
    private void ObjePariltiyiKoy(GameObject obje, float objeBoyu)
    {
        if (obje == null) return;

        Transform eski = obje.transform.Find("ObjePariltisi");
        GameObject go;
        if (eski != null)
            go = eski.gameObject;
        else
        {
            go = new GameObject("ObjePariltisi");
            go.transform.SetParent(obje.transform, false);
        }

        SpriteRenderer cizici = go.GetComponent<SpriteRenderer>();
        if (cizici == null)
            cizici = go.AddComponent<SpriteRenderer>();

        cizici.sprite = PariltiSpriteAl();
        // Harf parıltısıyla aynı beyaz / yoğunluk
        cizici.color = new Color(1f, 1f, 1f, 0.82f);
        // Obje (5) hemen altında
        cizici.sortingOrder = 4;

        go.transform.localPosition = Vector3.zero;
        float ebeveyn = Mathf.Max(0.001f, obje.transform.lossyScale.y);
        float hedef = Mathf.Max(0.2f, objeBoyu) * 1.75f;
        go.transform.localScale = Vector3.one * (hedef / ebeveyn);

        HarfPariltisi nabiz = go.GetComponent<HarfPariltisi>();
        if (nabiz == null)
            nabiz = go.AddComponent<HarfPariltisi>();
        nabiz.TabanOlcegiKaydet(go.transform.localScale);
    }

    // Sprite, collider ve sürükleme bileşenli obje üretir
    private GameObject ObjeOlustur(SecilenObje secim)
    {
        // Sprite adıyla obje
        GameObject go = new GameObject(secim.sprite.name);
        // Çizici
        SpriteRenderer cizici = go.AddComponent<SpriteRenderer>();
        // Görseli ata
        cizici.sprite = secim.sprite;
        // Balonun üstünde çizilsin
        cizici.sortingOrder = 5;
        // Tıklama alanı
        BoxCollider2D kutu = go.AddComponent<BoxCollider2D>();
        // Fiziksel collider (trigger değil)
        kutu.isTrigger = false;
        // Kutuyu sprite sınırına oturt
        kutu.size = secim.sprite.bounds.size;
        // Pivot kayması
        kutu.offset = secim.sprite.bounds.center;
        // Sürükleme bileşeni
        ObjeSurukleme suruk = go.AddComponent<ObjeSurukleme>();
        // Görünen ad
        suruk.objeAdi = secim.sprite.name;
        // Baş harf
        suruk.dogruHarf = secim.harf;
        // Hazır obje
        return go;
    }

    // Sprite merkezini hedef dünya noktasına getirir
    private void MerkezeOturt(Transform hedef, Sprite sprite, Vector3 merkez, float olcek)
    {
        // Tek tip ölçek
        hedef.localScale = new Vector3(olcek, olcek, 1f);
        // Pivot ile görsel merkez arasındaki kayma
        Vector3 kayma = Vector3.Scale(sprite.bounds.center, hedef.localScale);
        // Merkez hedefte kalsın
        Vector3 konum = merkez - kayma;
        // 2D düzlem
        konum.z = 0f;
        // Konumu yaz
        hedef.position = konum;
    }

    // Listeyi yerinde karıştırır
    private void Karistir<T>(List<T> liste)
    {
        // Fisher-Yates
        for (int i = liste.Count - 1; i > 0; i--)
        {
            // 0 ile i arasında rastgele indeks
            int j = Random.Range(0, i + 1);
            // Takas
            T gecici = liste[i];
            // i'yi j yap
            liste[i] = liste[j];
            // j'yi eski i yap
            liste[j] = gecici;
        }
    }

    // Editor'de _Art yolundan ilk sprite'ı yükler
    private Sprite ArtSpriteYukle(string yol)
    {
#if UNITY_EDITOR
        // Yoldaki tüm alt varlıklar
        Object[] hepsi = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(yol);
        // Liste boşsa çık
        if (hepsi == null)
        {
            // Sprite yok
            return null;
        }

        // İlk sprite'ı bul
        for (int i = 0; i < hepsi.Length; i++)
        {
            // Sprite türü mü
            Sprite sprite = hepsi[i] as Sprite;
            if (sprite != null)
            {
                // Bunu kullan
                return sprite;
            }
        }
#else
        // Oyuncu derlemesinde yol kullanılmaz
        yol = null;
#endif
        // Bulunamadı
        return null;
    }
}

/// <summary>
/// Harfin arkasındaki parıltıyı yavaşça büyütür ve döndürür.
/// </summary>
public class HarfPariltisi : MonoBehaviour
{
    // Nabızsız yerel ölçek
    private Vector3 tabanOlcek = Vector3.one;
    // Kurulumda verilen alfa (yoğunluk korunur)
    private float tabanAlfa = 0.82f;
    // Görsel
    private SpriteRenderer cizici;
    // Her balonda faz kaymasın diye sabit başlangıç
    private float faz;

    // Çiziciyi ve fazı al
    private void Awake()
    {
        // Sprite çizici
        cizici = GetComponent<SpriteRenderer>();
        // Dönüş fazı
        faz = 0.4f;
    }

    // Ölçek kurulduktan sonra tabanı kaydet
    public void TabanOlcegiKaydet(Vector3 olcek)
    {
        // Nabzın ortası bu boy
        tabanOlcek = olcek;
        // Hemen uygula
        transform.localScale = olcek;
        // Renk alfasını taban al
        if (cizici == null)
            cizici = GetComponent<SpriteRenderer>();
        if (cizici != null)
            tabanAlfa = cizici.color.a;
    }

    // Her karede hafif büyür ve döner
    private void Update()
    {
        // 0 ile 1 arası nabız
        float nabiz = 0.5f + 0.5f * Mathf.Sin(Time.time * 3.1f + faz);
        // Biraz küçülüp büyüsün
        float carpan = Mathf.Lerp(0.9f, 1.1f, nabiz);
        // Taban ölçeğe uygula
        transform.localScale = tabanOlcek * carpan;
        // Çizici varsa parlaklık da nabızlasın (taban alfayı koru)
        if (cizici != null)
        {
            Color renk = cizici.color;
            // Kurulumdaki beyaz yoğunluğun etrafında hafif nabız
            renk.a = Mathf.Lerp(tabanAlfa * 0.88f, tabanAlfa, nabiz);
            cizici.color = renk;
        }

        // Yıldız kolları yavaş dönsün
        transform.Rotate(0f, 0f, 16f * Time.deltaTime);
    }
}
