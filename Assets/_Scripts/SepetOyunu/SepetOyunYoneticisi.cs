using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Mini Oyun 1 beyni: süre, harf, 4 obje, bırakma değerlendirmesi, skor.
/// </summary>
public class SepetOyunYoneticisi : MonoBehaviour
{
    // Tek kopya — ObjeSurukleme buradan çağırır
    public static SepetOyunYoneticisi Instance;

    // Harf seçici (Paket 3)
    public SoruSecici soruSecici;

    // Skor sayacı (Paket 3)
    public SkorYoneticisi skorYoneticisi;

    // Doğru/yanlış ikon + ses (Paket 3)
    public GeriBildirimYoneticisi geriBildirim;

    // Spawn şablonu (SpriteRenderer + Collider + ObjeSurukleme)
    public GameObject objePrefab;

    // Alt sıradaki 4 sabit slot
    public Transform[] objePozisyonlari;

    // Balondaki harf yazısı (dünya uzayı TMP)
    public TextMeshPro harfRozeti;

    // Süre yazısı (UI)
    public TextMeshProUGUI sureYazisi;

    // Üst ortadaki puan yazısı (UI bar)
    public TextMeshProUGUI skorYazisi;

    // Geri sayım (saniye)
    public float kalanSure = 60f;

    // Her doğru cevapta eklenecek puan
    public int dogruPuan = 1;

    // Bu turdaki soru kartı
    private HarfObjeVerisi aktifHarf;

    // Sahnedeki canlı objeler
    private readonly List<GameObject> aktifObjeler = new List<GameObject>();

    // Süre bitince false olur
    private bool oyunAktif = true;

    // Doğru sonrası yeni tur hazırlanırken çift bırakmayı engelle
    private bool islemKilitli;

    // Singleton ata
    private void Awake()
    {
        // İkinci kopya varsa yok et
        if (Instance != null && Instance != this)
        {
            // Fazla yönetici
            Destroy(gameObject);
            // Çık
            return;
        }

        // Tek örneği kaydet
        Instance = this;
    }

    // Yok olunca singleton temizle
    private void OnDestroy()
    {
        // Bu kopya aktifse temizle
        if (Instance == this)
        {
            // Referansı bırak
            Instance = null;
        }
    }

    // İlk turu başlat
    private void Start()
    {
        // Skoru sıfırla
        if (skorYoneticisi != null)
        {
            // Sıfırdan başla
            skorYoneticisi.SkoruSifirla();
        }

        // Puan yazısını 0 yap
        SkorYazisiniGuncelle();
        // Süre yazısını doldur
        SureYazisiniGuncelle();
        // İlk soruyu kur
        YeniTurBaslat();
    }

    // Her karede süre say
    private void Update()
    {
        // Oyun bittiyse sayma
        if (!oyunAktif)
        {
            // Çık
            return;
        }

        // Süreyi azalt
        kalanSure -= Time.deltaTime;
        // Yazıyı yenile
        SureYazisiniGuncelle();
        // Süre dolduysa bitir
        if (kalanSure <= 0f)
        {
            // Negatif kalmasın
            kalanSure = 0f;
            // Oyunu durdur
            OyunuBitir();
        }
    }

    /// <summary>Yeni harf seçer, objeleri yeniden dizer.</summary>
    public void YeniTurBaslat()
    {
        // Oyun bittiyse tur açma
        if (!oyunAktif)
        {
            // Çık
            return;
        }

        // Kilidi aç (yeni turda bırakmaya izin)
        islemKilitli = false;
        // Harf seçici yoksa uyarı
        if (soruSecici == null)
        {
            // Eksik bağ
            Debug.LogWarning("SepetOyunYoneticisi: soruSecici yok.");
            // Çık
            return;
        }

        // Rastgele (mümkünse farklı) harf
        aktifHarf = soruSecici.YeniHarfSec();
        // Kart gelmediyse çık
        if (aktifHarf == null)
        {
            // Veri yok
            Debug.LogWarning("SepetOyunYoneticisi: harf seçilemedi.");
            // Çık
            return;
        }

        // Rozete harfi yaz
        if (harfRozeti != null)
        {
            // Büyük harf metni
            harfRozeti.text = aktifHarf.harf.ToString();
            // Mesh'i tazeleyerek renk geçişini koru
            harfRozeti.ForceMeshUpdate();
        }

        // Eski objeleri sil
        ObjeleriTemizle();
        // Yeni 4 objeyi üret
        ObjeleriOlustur();
    }

    /// <summary>1 doğru + 3 çeldiriciyi karıştırıp slotlara koyar.</summary>
    public void ObjeleriOlustur()
    {
        // Prefab veya harf yoksa üretme
        if (objePrefab == null || aktifHarf == null)
        {
            // Eksik
            Debug.LogWarning("SepetOyunYoneticisi: prefab veya aktif harf yok.");
            // Çık
            return;
        }

        // Slot dizisi yoksa çık
        if (objePozisyonlari == null || objePozisyonlari.Length == 0)
        {
            // Pozisyon eksik
            Debug.LogWarning("SepetOyunYoneticisi: objePozisyonlari boş.");
            // Çık
            return;
        }

        // Bu turda seçilecek sprite + harf
        List<ObjeSecimi> secimler = new List<ObjeSecimi>(4);
        // Doğru harften bir sprite
        Sprite dogruSprite = RastgeleSprite(aktifHarf, secimler);
        // Doğru sprite varsa ekle
        if (dogruSprite != null)
        {
            // Doğru seçenek
            secimler.Add(new ObjeSecimi { sprite = dogruSprite, harf = aktifHarf.harf, ad = dogruSprite.name });
        }

        // Diğer harflerden çeldirici topla
        if (soruSecici.tumHarfler != null)
        {
            // Aday harf listesi
            List<HarfObjeVerisi> diger = new List<HarfObjeVerisi>();
            // Tüm kartları dolaş
            for (int i = 0; i < soruSecici.tumHarfler.Length; i++)
            {
                // Boş veya aktif kartı atla
                HarfObjeVerisi kart = soruSecici.tumHarfler[i];
                if (kart == null || kart == aktifHarf)
                {
                    // Sonraki
                    continue;
                }

                // Çeldirici adayı
                diger.Add(kart);
            }

            // Çeldirici sırasını karıştır
            Karistir(diger);
            // 4 slot dolana kadar ekle
            for (int i = 0; i < diger.Count && secimler.Count < 4; i++)
            {
                // Bu harften kullanılmamış sprite
                Sprite celdirici = RastgeleSprite(diger[i], secimler);
                // Yoksa atla
                if (celdirici == null)
                {
                    // Sonraki harf
                    continue;
                }

                // Çeldiriciyi ekle
                secimler.Add(new ObjeSecimi { sprite = celdirici, harf = diger[i].harf, ad = celdirici.name });
            }
        }

        // Slot sırasını karıştır (doğru hep aynı yerde olmasın)
        Karistir(secimler);
        // Her seçimi bir slota koy
        for (int i = 0; i < secimler.Count && i < objePozisyonlari.Length; i++)
        {
            // Slot boşsa atla
            Transform slot = objePozisyonlari[i];
            if (slot == null)
            {
                // Sonraki slot
                continue;
            }

            // Prefab'tan örnek
            GameObject go = Instantiate(objePrefab, slot.position, Quaternion.identity);
            // Aktif olsun
            go.SetActive(true);
            // İsim = sprite adı
            go.name = secimler[i].ad;
            // Çiziciyi bul
            SpriteRenderer cizici = go.GetComponent<SpriteRenderer>();
            // Sprite ata
            if (cizici != null)
            {
                // Görsel
                cizici.sprite = secimler[i].sprite;
                // Balonun üstünde çiz
                cizici.sortingOrder = 5;
            }

            // Collider'ı sprite'a oturt
            BoxCollider2D kutu = go.GetComponent<BoxCollider2D>();
            if (kutu != null && secimler[i].sprite != null)
            {
                // Trigger değil
                kutu.isTrigger = false;
                // Boyut
                kutu.size = secimler[i].sprite.bounds.size;
                // Pivot kayması
                kutu.offset = secimler[i].sprite.bounds.center;
            }

            // Sürükleme bileşeni
            ObjeSurukleme suruk = go.GetComponent<ObjeSurukleme>();
            // Yoksa ekle
            if (suruk == null)
            {
                // Sürükleme
                suruk = go.AddComponent<ObjeSurukleme>();
            }

            // Obje adı
            suruk.objeAdi = secimler[i].ad;
            // Baş harf
            suruk.dogruHarf = secimler[i].harf;
            // Slot ölçeğine göre görseli sığdır
            if (cizici != null && cizici.sprite != null)
            {
                // Hedef dünya boyutu (slot scale veya sabit)
                float hedef = slot.localScale.x > 0.01f ? slot.localScale.x : 1.6f;
                // Sprite'ın en uzun kenarı
                float uzun = Mathf.Max(cizici.sprite.bounds.size.x, cizici.sprite.bounds.size.y);
                // Ölçek
                float olcek = uzun > 0.001f ? hedef / uzun : 1f;
                // Uygula
                go.transform.localScale = new Vector3(olcek, olcek, 1f);
                // Merkezi slota oturt
                Vector3 kayma = Vector3.Scale(cizici.sprite.bounds.center, go.transform.localScale);
                Vector3 konum = slot.position - kayma;
                konum.z = 0f;
                go.transform.position = konum;
            }

            // Başlangıç konumunu şimdi kaydet (Start'tan önce de güvenli)
            suruk.BaslangiciKaydet();
            // Listeye ekle
            aktifObjeler.Add(go);
        }
    }

    /// <summary>Balona bırakılan objeyi değerlendirir.</summary>
    public void ObjeBirakildi(ObjeSurukleme obje)
    {
        // Boş referans
        if (obje == null)
        {
            // Çık
            return;
        }

        // Oyun bitti veya işlem kilitliyse yok say
        if (!oyunAktif || islemKilitli || aktifHarf == null)
        {
            // Objeyi yuvasına gönder
            obje.BaslangicaDon();
            // Çık
            return;
        }

        // Harf birebir eşleşiyor mu (İ / I ayrı kalsın)
        bool dogru = obje.dogruHarf == aktifHarf.harf;

        // Doğru cevap
        if (dogru)
        {
            // Çift tetiklemeyi kes
            islemKilitli = true;
            // Puan ekle
            if (skorYoneticisi != null)
            {
                // dogruPuan kadar (varsayılan 1)
                skorYoneticisi.SkorEkle(dogruPuan);
            }

            // Puan yazısını güncelle
            SkorYazisiniGuncelle();
            // Tik animasyonu + ses
            if (geriBildirim != null)
            {
                // correct_check
                geriBildirim.DogruGoster();
            }

            // Yeni soruya geç
            YeniTurBaslat();
        }
        else
        {
            // Çarpı animasyonu + ses
            if (geriBildirim != null)
            {
                // wrong_x
                geriBildirim.YanlisGoster();
            }

            // Objeyi başlangıç yerine koy
            obje.BaslangicaDon();
        }
    }

    /// <summary>Süreyi durdurur, objeleri temizler.</summary>
    public void OyunuBitir()
    {
        // Zaten bittiyse tekrar etme
        if (!oyunAktif)
        {
            // Çık
            return;
        }

        // Oyunu kapat
        oyunAktif = false;
        // Kilidi açma
        islemKilitli = true;
        // Süreyi sıfır yaz
        kalanSure = 0f;
        // Yazıyı güncelle
        SureYazisiniGuncelle();
        // Objeleri kaldır (sürükleme bitsin)
        ObjeleriTemizle();
        // Konsola bilgi
        Debug.Log("SepetOyunu bitti. Skor: " + (skorYoneticisi != null ? skorYoneticisi.skor : 0));
    }

    // Canlı objeleri yok et
    private void ObjeleriTemizle()
    {
        // Listedeki her obje
        for (int i = 0; i < aktifObjeler.Count; i++)
        {
            // Null değilse sil
            if (aktifObjeler[i] != null)
            {
                // Yok et
                Destroy(aktifObjeler[i]);
            }
        }

        // Listeyi boşalt
        aktifObjeler.Clear();
    }

    // Süre yazısını mm:ss veya sn olarak yaz
    private void SureYazisiniGuncelle()
    {
        // Yazı yoksa çık
        if (sureYazisi == null)
        {
            // UI yok
            return;
        }

        // Kalan saniye (aşağı yuvarla, negatif olmasın)
        int sn = Mathf.Max(0, Mathf.CeilToInt(kalanSure));
        // Dakika
        int dk = sn / 60;
        // Saniye
        int kalan = sn % 60;
        // 01:00 biçimi
        sureYazisi.text = dk.ToString("00") + ":" + kalan.ToString("00");
    }

    // Üst bar ve geri bildirim skor yazısını güncelle
    private void SkorYazisiniGuncelle()
    {
        // Güncel skor
        int skor = skorYoneticisi != null ? skorYoneticisi.skor : 0;
        // Üst orta bar
        if (skorYazisi != null)
        {
            // Puan: N
            skorYazisi.text = "Puan: " + skor;
        }

        // Geri bildirimdeki yazı (varsa)
        if (geriBildirim != null)
        {
            // Doğru: N
            geriBildirim.SkorYazisiniGuncelle(skor);
        }
    }

    // Karttan listede olmayan rastgele sprite
    private Sprite RastgeleSprite(HarfObjeVerisi kart, List<ObjeSecimi> kullanilan)
    {
        // Kart boşsa
        if (kart == null || kart.dogruObjeler == null)
        {
            // Yok
            return null;
        }

        // Adaylar
        List<Sprite> adaylar = new List<Sprite>();
        // Diziyi dolaş
        for (int i = 0; i < kart.dogruObjeler.Length; i++)
        {
            // Sprite
            Sprite s = kart.dogruObjeler[i];
            // Boşsa atla
            if (s == null)
            {
                // Sonraki
                continue;
            }

            // Daha önce seçildiyse atla
            if (ListedeVar(kullanilan, s))
            {
                // Tekrar yok
                continue;
            }

            // Aday
            adaylar.Add(s);
        }

        // Aday yoksa
        if (adaylar.Count == 0)
        {
            // Null
            return null;
        }

        // Rastgele biri
        return adaylar[Random.Range(0, adaylar.Count)];
    }

    // Sprite listede var mı
    private bool ListedeVar(List<ObjeSecimi> liste, Sprite sprite)
    {
        // Dolaş
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

    // Fisher-Yates karıştırma
    private void Karistir<T>(List<T> liste)
    {
        // Sondan başa
        for (int i = liste.Count - 1; i > 0; i--)
        {
            // Rastgele indeks
            int j = Random.Range(0, i + 1);
            // Takas
            T gecici = liste[i];
            liste[i] = liste[j];
            liste[j] = gecici;
        }
    }

    // Spawn seçimi
    private struct ObjeSecimi
    {
        // Görsel
        public Sprite sprite;
        // Baş harf
        public char harf;
        // Obje adı
        public string ad;
    }
}
