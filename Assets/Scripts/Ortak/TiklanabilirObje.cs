using UnityEngine;
using UnityEngine.Events;

// TiklanabilirObje: Oyuncunun tıklayabileceği harf/nesne seçeneklerini yönetir.
// Spawn sırasında dogruMu alanı ayarlanır; doğru tıklamada skor artar, yanlışta geri bildirim için yer bırakılır.
//
// Kurulum: Bu script'i tıklanabilir GameObject'e ekleyin.
// - Collider2D veya Collider (OnMouseDown için zorunlu)
// - SpriteRenderer veya görünür bir bileşen
// Inspector'dan skorYoneticisi referansını sürükleyin.
public class TiklanabilirObje : MonoBehaviour
{
    // Puanı tutan ve ekranda gösteren yönetici (Inspector'dan sürükleyin).
    public SkorYoneticisi skorYoneticisi;

    // Bu nesne doğru cevap mı? Spawn anında atanır (ör. harf eşleşmesi).
    public bool dogruMu;

    // Doğru tıklamada eklenecek puan miktarı.
    public int puanMiktari = 10;

    // İsteğe bağlı: Harf oyunu akışını yöneten bileşen.
    // HarfOyunuYoneticisi'ne sonraki soru metodu eklendiğinde buradan çağrılabilir.
    public HarfOyunuYoneticisi oyunYoneticisi;

    // Doğru cevap verildiğinde Inspector'dan bağlanabilecek olay (ör. sonraki soruya geç).
    public UnityEvent dogruCevapVerildi;

    // Oyuncu nesneye tıkladığında Unity tarafından çağrılır (Collider + kamera gerekir).
    private void OnMouseDown()
    {
        if (dogruMu)
        {
            DogruTiklamaIsle();
        }
        else
        {
            YanlisTiklamaIsle();
        }
    }

    // Doğru nesneye tıklandığında skoru artırır ve isteğe bağlı geri bildirim olaylarını tetikler.
    private void DogruTiklamaIsle()
    {
        if (skorYoneticisi != null)
        {
            skorYoneticisi.SkorEkle(puanMiktari);
        }
        else
        {
            Debug.LogWarning("TiklanabilirObje: skorYoneticisi atanmamış; puan eklenemedi.");
        }

        // HarfOyunuYoneticisi henüz sonraki soru metodu içermiyor;
        // oyun akışı için dogruCevapVerildi olayını Inspector'dan bağlayın.
        dogruCevapVerildi?.Invoke();
    }

    // Yanlış nesneye tıklandığında çalışır; ileride ses/animasyon geri bildirimi eklenebilir.
    private void YanlisTiklamaIsle()
    {
        // TODO: Yanlış cevap sesi, kırmızı flaş veya benzeri geri bildirim buraya eklenebilir.
        Debug.Log("TiklanabilirObje: Yanlış nesne seçildi.");
    }
}
