using UnityEngine;

/// <summary>
/// Karakter butonuna eklenir; On Click → BuHayvaniSec().
/// hayvanIndex: AvatarGorunumu.hayvanlar dizisindeki sıra.
/// </summary>
public class HayvanSecici : MonoBehaviour
{
    // Görünümü yenilemek için
    public AvatarGorunumu avatarGorunumu;

    // Bu butonun hayvan indeksi (0=Kedi, 1=Tavşan, 2=Kuş, 3=Rakun)
    public int hayvanIndex = 0;

    /// <summary>Hayvanı yöneticiye yazar ve görünümü yeniler.</summary>
    public void BuHayvaniSec()
    {
        if (AvatarYoneticisi.Instance != null)
            AvatarYoneticisi.Instance.HayvanSec(hayvanIndex);

        if (avatarGorunumu != null)
            avatarGorunumu.Guncelle();
    }
}
