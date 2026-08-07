using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tek bir seçenek balonunu temsil eder (world-space sprite + collider).
/// Tıklanınca doğru/yanlış sonucunu HarfSecmeYoneticisi'ne bildirir.
/// </summary>
public class SecenekBalonu : MonoBehaviour
{
    // Bu balonun obje kimliği (araba, elma, ...)
    public string objeAdi;

    // Bu objenin baş harfi (A, E, Ö, ...)
    private char dogruHarf;

    // Şu an sorulan harf
    private char aktifSoruHarfi;

    // İç skor/süreç için harf numarası (1..8)
    public int harfId { get; private set; }

    // Doğru / yanlış görsel geri bildirimi
    public GameObject dogruFeedback;
    public GameObject yanlisFeedback;

    // Slotun sabit dünya pozisyonu
    public Vector3 slotPozisyonu;

    // Collider ve tıklama hazırlığı
    public void TiklanabilirYap()
    {
        // Varsa UI Button bileşenlerini kapat (world tıklama kullanıyoruz)
        Button[] butonlar = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < butonlar.Length; i++)
            butonlar[i].enabled = false;

        // BoxCollider2D yoksa ekle
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider2D>();

        // Collider boyutunu sprite sınırına göre ayarla
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            box.size = sr.sprite.bounds.size;
        else
            box.size = new Vector2(7f, 12f);
    }

    // Yöneticiden kimlik, harf ve feedback referanslarını al
    public void Ayarla(string adi, char objeHarfi, int id, GameObject dogruFb, GameObject yanlisFb, Vector3 slotPos)
    {
        objeAdi = adi;
        dogruHarf = objeHarfi;
        harfId = id;
        dogruFeedback = dogruFb;
        yanlisFeedback = yanlisFb;
        slotPozisyonu = slotPos;
    }

    // Aktif soru harfini güncelle (sprite sahnede zaten var)
    public void Ayarla(Sprite resim, char objeHarfi, char soruHarfi)
    {
        // İsteğe bağlı sprite güncellemesi (şu an sahne sprite'larını kullanıyoruz)
        if (resim != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = resim;
        }

        dogruHarf = objeHarfi;
        aktifSoruHarfi = soruHarfi;
    }

    // Sadece soru harfini yenile
    public void SoruHarfiniGuncelle(char soruHarfi)
    {
        aktifSoruHarfi = soruHarfi;
    }

    // Slot pozisyonunu güncelle (çift slot değişince)
    public void SlotPozisyonunuGuncelle(Vector3 pos)
    {
        slotPozisyonu = pos;
    }

    // Tıklanınca doğru mu kontrol et ve yöneticiye bildir
    public void Tiklandi()
    {
        // Yönetici yoksa işlem yapma
        if (HarfSecmeYoneticisi.Instance == null)
            return;

        // Bu obje harfi sorulan harfle aynı mı?
        bool dogruMu = dogruHarf == aktifSoruHarfi;
        HarfSecmeYoneticisi.Instance.SecenekSecildi(dogruMu, this);
    }

    // Dünya noktasının bu balonun sprite sınırında olup olmadığı
    public bool NoktaSpriteIcinde(Vector2 dunya2D)
    {
        // Pasif veya yoksa isabet yok
        if (!gameObject.activeInHierarchy)
            return false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || !sr.enabled || sr.sprite == null)
            return false;

        return sr.bounds.Contains(new Vector3(dunya2D.x, dunya2D.y, sr.bounds.center.z));
    }
}
