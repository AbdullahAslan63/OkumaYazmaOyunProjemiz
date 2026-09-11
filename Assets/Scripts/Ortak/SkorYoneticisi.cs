using TMPro;
using UnityEngine;

// SkorYoneticisi: Oyundaki puanı tutar ve ekrandaki skor yazısını günceller.
// Bu script'i bir GameObject'e ekleyin; Inspector'dan skorYazisi alanına TextMeshPro UI metnini sürükleyin.
public class SkorYoneticisi : MonoBehaviour
{
   
    // Ekranda gösterilecek skor metni (TextMeshPro - UI).
    public TextMeshProUGUI skorYazisi;

    // Oyuncunun mevcut puanı; oyun başında sıfırdan başlar.
    private int skor = 0;

    // Oyun başladığında skor yazısını mevcut puana göre ayarlar (iyi uygulama).
    private void Start()
    {
        SkorYazisiniGuncelle();
    }

    // Verilen miktar kadar skoru artırır ve ekrandaki yazıyı günceller.
    public void SkorEkle(int miktar)
    {
        skor += miktar;
        SkorYazisiniGuncelle();
    }

    // skorYazisi atanmışsa ekrandaki metni güncel skorla eşleştirir; atanmamışsa güvenle atlar.
    private void SkorYazisiniGuncelle()
    {
        if (skorYazisi != null)
        {
            skorYazisi.text = skor.ToString();
        }
    }
}
