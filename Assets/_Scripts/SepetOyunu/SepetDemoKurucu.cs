using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Demo turu: rastgele sesli harf, balonun üstünde harf, altta 4 obje.
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
        // Balonu bul veya oluştur, ekranın üstüne oturt
        GameObject balon = BalonuHazirla(kamera);
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
        // Harfi balonun üstüne yaz
        HarfiYaz(balon, aktif.harf);
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

        // Collider'ı al veya ekle
        BoxCollider2D kutu = balon.GetComponent<BoxCollider2D>();
        if (kutu == null)
        {
            // Kutu collider ekle
            kutu = balon.AddComponent<BoxCollider2D>();
        }

        // Bırakma alanı çarpışarak itmesin
        kutu.isTrigger = true;
        // Sprite varsa kutuyu görsele oturt
        if (cizici.sprite != null)
        {
            // Yerel genişlik ve yükseklik
            kutu.size = cizici.sprite.bounds.size;
            // Pivot kaymasını dengele
            kutu.offset = cizici.sprite.bounds.center;
        }

        // Sprite yoksa konumlama yapma
        if (cizici.sprite == null)
        {
            // Balonu olduğu gibi bırak
            return balon;
        }

        // Ekran yüksekliği
        float yukseklik = kamera.orthographicSize * 2f;
        // Balon yüksekliği ekranın üçte biri
        float hedef = yukseklik / 3f;
        // Sprite yüksekliği
        float spriteH = cizici.sprite.bounds.size.y;
        // Sıfıra bölme
        float olcek = spriteH > 0.001f ? hedef / spriteH : 1f;
        // Üst ortaya yakın merkez
        float merkezY = kamera.orthographicSize - hedef * 0.55f;
        // Görsel merkezi bu noktaya gelsin
        MerkezeOturt(balon.transform, cizici.sprite, new Vector3(0f, merkezY, 0f), olcek);
        // Hazır balonu döndür
        return balon;
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

    // Balonun üstüne büyük harf yazar
    private void HarfiYaz(GameObject balon, char harf)
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
        // Beyaz dolgu
        tmp.color = Color.white;
        // İlk punto; sonra balona göre ölçeklenir
        tmp.fontSize = 8f;
        // Siyah kontur
        tmp.outlineWidth = 0.25f;
        // Kontur rengi
        tmp.outlineColor = Color.black;
        // Yazı kutusu
        tmp.rectTransform.sizeDelta = new Vector2(4f, 4f);

        // Balon ölçeğinden bağımsız dünya boyutu
        float ebeveyn = Mathf.Max(0.001f, balon.transform.lossyScale.y);
        // Ebeveyn ölçeğini nötrle
        tmp.transform.localScale = Vector3.one / ebeveyn;
        // Mesh'i hemen üret
        tmp.ForceMeshUpdate();

        // Balon çizicisi
        SpriteRenderer balonCizici = balon.GetComponent<SpriteRenderer>();
        // Çizici varsa harfi balon gövdesine koy
        if (balonCizici != null && balonCizici.sprite != null)
        {
            // Balonun dünya sınırları
            Bounds sinir = balonCizici.bounds;
            // Gövdeye yakın, biraz yukarı
            float y = sinir.center.y + sinir.extents.y * 0.12f;
            // Dünya konumu
            tmp.transform.position = new Vector3(sinir.center.x, y, 0f);
            // Harf yüksekliği balonun yaklaşık dörtte biri olsun
            float hedef = sinir.size.y * 0.28f;
            // Şimdiki dünya yüksekliği
            float mevcut = tmp.textBounds.size.y * tmp.transform.lossyScale.y;
            // Ölçek çarpanı
            if (mevcut > 0.001f)
            {
                // Hedef boyuta getir
                tmp.transform.localScale *= hedef / mevcut;
            }
        }

        // Yazı çizicisini balonun üstüne al
        MeshRenderer ag = tmp.GetComponent<MeshRenderer>();
        if (ag != null)
        {
            // Harf objelerin altında, balonun üstünde
            ag.sortingOrder = 12;
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
        }
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
