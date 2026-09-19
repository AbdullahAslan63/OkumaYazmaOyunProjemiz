using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Avatar seçim ekranı: hayvan ileri/geri ve Devam ile sonraki sahneye geçiş.
/// </summary>
public class AvatarSecimEkrani : MonoBehaviour
{
    // Görünümü yenilemek için referans
    public AvatarGorunumu avatarGorunumu;

    // Devam butonu hedef sahnesi
    public string sonrakiSahneAdi = "HarfSecmeOyunu";

    // Hayvan sayısı (Kedi, Tavşan, Kuş, Rakun)
    private const int HayvanSayisi = 4;

    /// <summary>Sıradaki hayvana geçer (3'ten sonra 0).</summary>
    public void SonrakiHayvan()
    {
        if (AvatarYoneticisi.Instance == null) return;

        int yeni = (AvatarYoneticisi.Instance.seciliHayvanIndex + 1) % HayvanSayisi;
        AvatarYoneticisi.Instance.HayvanSec(yeni);
        GorunumuYenile();
    }

    /// <summary>Önceki hayvana geçer (0'dan önce 3).</summary>
    public void OncekiHayvan()
    {
        if (AvatarYoneticisi.Instance == null) return;

        int yeni = AvatarYoneticisi.Instance.seciliHayvanIndex - 1;
        if (yeni < 0) yeni = HayvanSayisi - 1;
        AvatarYoneticisi.Instance.HayvanSec(yeni);
        GorunumuYenile();
    }

    /// <summary>Seçimi kaydedip sonraki sahneye geçer.</summary>
    public void DevamEt()
    {
        if (string.IsNullOrEmpty(sonrakiSahneAdi))
        {
            Debug.LogWarning("AvatarSecimEkrani: sonrakiSahneAdi boş.");
            return;
        }
        SceneManager.LoadScene(sonrakiSahneAdi);
    }

    // AvatarGorunumu varsa Guncelle çağır
    private void GorunumuYenile()
    {
        if (avatarGorunumu != null)
            avatarGorunumu.Guncelle();
    }
}
