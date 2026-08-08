using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tek seçenek balonu: tıklanınca doğru/yanlış sonucunu yöneticiye bildirir.
/// Hem world-space SpriteRenderer hem (isteğe bağlı) UI Image destekler.
/// </summary>
public class SecenekBalonu : MonoBehaviour
{
    // Bu balonun obje adı (araba, elma, ...)
    public string objeAdi;

    // Kılavuz alanı: UI seçenek kullanılıyorsa obje görseli buraya atanır
    public Image objeResmi;

    // Bu objenin baş harfi
    private char dogruHarf;

    // Şu an sorulan harf
    private char aktifSoruHarfi;

    // Harf numarası (1..8) — sonuç ekranı için
    public int harfId { get; private set; }

    // Doğru / yanlış görsel geri bildirimi
    public GameObject dogruFeedback;
    public GameObject yanlisFeedback;

    // Bu turun slot dünya pozisyonu (uçuş sonrası dönüş)
    public Vector3 slotPozisyonu;

    // Collider hazırla; varsa UI Button'u kapat (world tıklama kullanılır)
    public void TiklanabilirYap()
    {
        Button[] butonlar = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < butonlar.Length; i++)
            butonlar[i].enabled = false;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider2D>();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            box.size = sr.sprite.bounds.size;
        else
            box.size = new Vector2(7f, 12f);
    }

    // Yönetici: kimlik, harf, feedback ve slot pozisyonu
    public void Ayarla(string adi, char objeHarfi, int id, GameObject dogruFb, GameObject yanlisFb, Vector3 slotPos)
    {
        objeAdi = adi;
        dogruHarf = objeHarfi;
        harfId = id;
        dogruFeedback = dogruFb;
        yanlisFeedback = yanlisFb;
        slotPozisyonu = slotPos;
    }

    // Kılavuz imzası: sprite + harfler (UI Image veya SpriteRenderer)
    public void Ayarla(Sprite resim, char objeHarfi, char soruHarfi)
    {
        if (resim != null)
        {
            if (objeResmi != null)
                objeResmi.sprite = resim;

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

    // Slot pozisyonunu güncelle
    public void SlotPozisyonunuGuncelle(Vector3 pos)
    {
        slotPozisyonu = pos;
    }

    // Tıklanınca doğru mu kontrol et ve yöneticiye bildir
    public void Tiklandi()
    {
        if (HarfSecmeYoneticisi.Instance == null)
            return;

        bool dogruMu = dogruHarf == aktifSoruHarfi;
        HarfSecmeYoneticisi.Instance.SecenekSecildi(dogruMu, this);
    }

    // Dünya noktasının sprite sınırında olup olmadığı
    public bool NoktaSpriteIcinde(Vector2 dunya2D)
    {
        if (!gameObject.activeInHierarchy)
            return false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || !sr.enabled || sr.sprite == null)
            return false;

        return sr.bounds.Contains(new Vector3(dunya2D.x, dunya2D.y, sr.bounds.center.z));
    }
}
