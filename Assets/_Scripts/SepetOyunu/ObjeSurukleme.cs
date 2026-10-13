using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Objeyi fare veya parmakla sürükler. Balonun üzerine bırakılmazsa başa döner.
/// </summary>
public class ObjeSurukleme : MonoBehaviour
{
    // Bu objenin adı (Inspector veya kurucu doldurur)
    public string objeAdi;

    // Bu objenin baş harfi
    public char dogruHarf;

    // Balon kenarının dışında kabul edilen ek mesafe
    public float birakmaMesafesi = 1.5f;

    // Tur başındaki dünya konumu
    private Vector3 baslangicPozisyonu;

    // Şu an bu obje sürükleniyor mu
    private bool surukleniyor;

    // Sürüklemeden önceki çizim sırası
    private int eskiSira;

    // Aynı anda tek obje sürüklensin diye paylaşılan seçim
    private static ObjeSurukleme aktif;

    // Çakışan collider listesi (her kare yeni dizi açmamak için)
    private static readonly List<Collider2D> vuruslar = new List<Collider2D>();

    // Oyun başında başlangıç konumunu sakla
    private void Start()
    {
        // Spawn konumu bu karede hazır olur
        baslangicPozisyonu = transform.position;
    }

    // Obje kapanırsa sürüklemeyi bırak
    private void OnDisable()
    {
        // Bu obje seçiliyse seçimi temizle
        if (aktif == this)
        {
            // Başka obje sürüklenemesin diye kilidi kaldır
            aktif = null;
            // Sürükleme bayrağını kapat
            surukleniyor = false;
        }
    }

    // Her karede basış, takip ve bırakmayı yönet
    private void Update()
    {
        // İşaretçi bu kare basılı mı
        bool basili = IsaretBasiliMi();

        // Yeni basışta, henüz seçim yoksa en üstteki objeyi al
        if (YeniBasildiMi() && aktif == null)
        {
            // İmlecin altındaki en üst sürüklenebilir obje
            ObjeSurukleme bulunan = UsttekiObje();
            // Bir obje bulunduysa onu seç
            if (bulunan != null)
            {
                // Paylaşılan seçimi ata
                aktif = bulunan;
                // Sürüklemeyi o objede aç
                bulunan.SuruklemeyiBaslat();
            }
        }

        // Bu obje seçili değilse veya sürükleme açılmadıysa dokunma
        if (aktif != this || !surukleniyor)
        {
            // Erken çık
            return;
        }

        // Basılı tutuluyorsa işaretçiyi takip et
        if (basili)
        {
            // Ekran konumunu dünyaya çevir
            Vector3 hedef = GetIsaretDunyaPozisyonu();
            // 2D düzlemde kal
            hedef.z = 0f;
            // Objeyi işaretçiye taşı
            transform.position = hedef;
        }
        else
        {
            // Parmak veya fare kalktı
            SuruklemeyiBitir();
            // Seçimi serbest bırak
            aktif = null;
        }
    }

    // Sürüklemeyi aç ve objeyi üste çiz
    private void SuruklemeyiBaslat()
    {
        // Sürükleme durumunu aç
        surukleniyor = true;
        // Çiziciyi bul
        SpriteRenderer cizici = GetComponent<SpriteRenderer>();
        // Çizici varsa sırayı yükselt
        if (cizici != null)
        {
            // Eski sırayı sakla
            eskiSira = cizici.sortingOrder;
            // Sürüklenen obje her şeyin üstünde kalsın
            cizici.sortingOrder = 30;
        }
    }

    // Bırakınca balona yakın mı bak, değilse başa dön
    private void SuruklemeyiBitir()
    {
        // Sürükleme durumunu kapat
        surukleniyor = false;
        // Çiziciyi bul
        SpriteRenderer cizici = GetComponent<SpriteRenderer>();
        // Çizim sırasını eski haline al
        if (cizici != null)
        {
            // Saklanan sırayı geri yaz
            cizici.sortingOrder = eskiSira;
        }

        // Balonu tag ile bul
        GameObject balon = GameObject.FindGameObjectWithTag("Balon");
        // Balon yoksa başa dön
        if (balon == null)
        {
            // Eksik tag uyarısı
            Debug.LogWarning("Balon tag'li obje bulunamadi.");
            // Başlangıç konumuna koy
            transform.position = baslangicPozisyonu;
            // Çık
            return;
        }

        // Varsayılan mesafe: obje merkezi ile balon merkezi
        float mesafe = Vector3.Distance(transform.position, balon.transform.position);
        // Balonun collider'ı varsa kenara olan mesafeyi kullan
        Collider2D kutu = balon.GetComponent<Collider2D>();
        if (kutu != null)
        {
            // Collider üzerindeki en yakın nokta (içerdeyse mesafe 0)
            Vector2 yakin = kutu.ClosestPoint(transform.position);
            // Obje merkezi ile o nokta arası
            mesafe = Vector2.Distance(transform.position, yakin);
        }

        // Kabul mesafesi içindeyse balona bırakıldı say
        if (mesafe <= birakmaMesafesi)
        {
            // Demo: yönetici yok, sadece log
            Debug.Log("Balona birakildi: " + objeAdi);
        }
        else
        {
            // Boşluğa bırakıldıysa eski konuma dön
            transform.position = baslangicPozisyonu;
        }
    }

    // İmleç veya ilk parmak basılı mı
    private bool IsaretBasiliMi()
    {
        // Dokunmatik varsa ve parmak basılıysa
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            // Parmak basılı
            return true;
        }

        // Yoksa fare sol tuşu
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            // Fare basılı
            return true;
        }

        // Hiçbiri basılı değil
        return false;
    }

    // Bu kare yeni bir basış mı
    private bool YeniBasildiMi()
    {
        // Parmak bu kare değdiyse
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            // Yeni dokunuş
            return true;
        }

        // Fare bu kare tıklandıysa
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Yeni tıklama
            return true;
        }

        // Yeni basış yok
        return false;
    }

    // İmlecin altındaki en üst ObjeSurukleme
    private ObjeSurukleme UsttekiObje()
    {
        // İşaretçinin dünya konumu
        Vector3 dunya = GetIsaretDunyaPozisyonu();
        // Önceki kareden kalan çarpmaları sil
        vuruslar.Clear();
        // Tüm katmanları kabul eden filtre
        ContactFilter2D filtre = new ContactFilter2D();
        // Filtreyi kapat, her collider gelsin
        filtre.NoFilter();
        // Noktaya değen collider'ları topla
        Physics2D.OverlapPoint(dunya, filtre, vuruslar);

        // En yüksek çizim sırasındaki obje
        ObjeSurukleme enIyi = null;
        // Karşılaştırma için en düşük başlangıç
        int enYuksekSira = int.MinValue;
        // Her çarpmayı dolaş
        for (int i = 0; i < vuruslar.Count; i++)
        {
            // Bu collider'da sürükleme var mı
            ObjeSurukleme aday = vuruslar[i].GetComponent<ObjeSurukleme>();
            // Yoksa atla
            if (aday == null)
            {
                // Sonraki collider
                continue;
            }

            // Çizim sırasını oku
            SpriteRenderer cizici = aday.GetComponent<SpriteRenderer>();
            // Çizici yoksa sıra 0
            int sira = cizici != null ? cizici.sortingOrder : 0;
            // Daha üstteyse bunu seç
            if (enIyi == null || sira >= enYuksekSira)
            {
                // Yeni kazanan
                enIyi = aday;
                // Onun sırasını sakla
                enYuksekSira = sira;
            }
        }

        // Bulunan objeyi döndür (yoksa null)
        return enIyi;
    }

    // Fare veya parmak konumunu dünya koordinatına çevirir
    private Vector3 GetIsaretDunyaPozisyonu()
    {
        // Varsayılan ekran noktası
        Vector2 ekran = Vector2.zero;
        // Konum okundu mu
        bool okundu = false;

        // Parmak basılıysa onun konumunu al
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            // İlk parmağın piksel konumu
            ekran = Touchscreen.current.primaryTouch.position.ReadValue();
            // Okuma tamam
            okundu = true;
        }
        else if (Mouse.current != null)
        {
            // Fare piksel konumu
            ekran = Mouse.current.position.ReadValue();
            // Okuma tamam
            okundu = true;
        }

        // Ana kamerayı al
        Camera kamera = Camera.main;
        // Kamera veya işaretçi yoksa yerinde kal
        if (kamera == null || !okundu)
        {
            // Güvenli dönüş
            return transform.position;
        }

        // Kameradan oyun düzlemine uzaklık (ortografik)
        float derinlik = Mathf.Abs(kamera.transform.position.z);
        // Ekran noktasını derinlikle birlikte hazırla
        Vector3 ekran3 = new Vector3(ekran.x, ekran.y, derinlik);
        // Dünyaya çevir
        Vector3 dunya = kamera.ScreenToWorldPoint(ekran3);
        // 2D için z'yi sıfırla
        dunya.z = 0f;
        // Dünya konumunu döndür
        return dunya;
    }
}
