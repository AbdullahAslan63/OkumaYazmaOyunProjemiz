using UnityEngine;

// SesYoneticisi: Oyuncunun seçtiği hayvana göre arka plan müziğini çalar.
// AvatarYoneticisi'ndeki seciliHayvanIndex değerini okuyarak uygun müziği başlatır.
//
// Kurulum: Bu script'i bir GameObject'e ekleyin.
// - muzikKaynagi: AudioSource bileşeni (Inspector'dan veya aynı nesneye ekleyin)
// - hayvanMuzikleri: 4 elemanlı dizi (0=Kedi, 1=Tavşan, 2=Kuş, 3=Rakun)
public class SesYoneticisi : MonoBehaviour
{
    // Arka plan müziğini çalacak AudioSource.
    public AudioSource muzikKaynagi;

    // Her hayvan için bir müzik klibi; dizi uzunluğu 4 olmalıdır.
    public AudioClip[] hayvanMuzikleri;

    private void Start()
    {
        if (muzikKaynagi == null)
        {
            Debug.LogWarning("SesYoneticisi: muzikKaynagi atanmamış.");
            return;
        }

        if (hayvanMuzikleri == null || hayvanMuzikleri.Length == 0)
        {
            Debug.LogWarning("SesYoneticisi: hayvanMuzikleri dizisi boş.");
            return;
        }

        // AvatarYoneticisi singleton'ından seçili hayvan indeksini al.
        int hayvanIndeksi = 0;

        if (AvatarYoneticisi.Instance != null)
        {
            hayvanIndeksi = AvatarYoneticisi.Instance.seciliHayvanIndex;
        }
        else
        {
            Debug.LogWarning("SesYoneticisi: AvatarYoneticisi bulunamadı; varsayılan müzik (indeks 0) kullanılacak.");
        }

        // İndeks dizi sınırları içinde değilse güvenli aralığa sıkıştır.
        hayvanIndeksi = Mathf.Clamp(hayvanIndeksi, 0, hayvanMuzikleri.Length - 1);

        AudioClip secilenMuzik = hayvanMuzikleri[hayvanIndeksi];

        if (secilenMuzik == null)
        {
            Debug.LogWarning($"SesYoneticisi: hayvanMuzikleri[{hayvanIndeksi}] boş.");
            return;
        }

        // Seçilen hayvanın müziğini ata, döngüde çal.
        muzikKaynagi.clip = secilenMuzik;
        muzikKaynagi.loop = true;
        muzikKaynagi.Play();
    }
}
