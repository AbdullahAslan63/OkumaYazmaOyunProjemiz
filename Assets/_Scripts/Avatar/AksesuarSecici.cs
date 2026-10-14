using UnityEngine;

/// <summary>
/// Aksesuar butonuna eklenir; On Click → BuAksesuariSec().
/// aksesuarIndex: -1 = yok, 0+ = AvatarGorunumu.aksesuarlar dizisi.
/// </summary>
public class AksesuarSecici : MonoBehaviour
{
    // Görünümü yenilemek için
    public AvatarGorunumu avatarGorunumu;

    // Bu butonun aksesuar indeksi (-1 = aksesuar yok)
    public int aksesuarIndex = -1;

    /// <summary>Aksesuarı yöneticiye yazar ve görünümü yeniler.</summary>
    public void BuAksesuariSec()
    {
        if (AvatarYoneticisi.Instance != null)
            AvatarYoneticisi.Instance.AksesuarSec(aksesuarIndex);

        if (avatarGorunumu != null)
            avatarGorunumu.Guncelle();
    }
}
