using UnityEngine;

/// <summary>
/// Renk butonuna eklenir; On Click → BuButonunRenginiSec().
/// Her butonda builtinRenk Inspector'dan farklı ayarlanır.
/// </summary>
public class RenkSecici : MonoBehaviour
{
    // Görünümü yenilemek için
    public AvatarGorunumu avatarGorunumu;

    // Bu butona ait renk
    public Color builtinRenk = Color.white;

    /// <summary>Buton renginini yöneticiye yazar ve görünümü yeniler.</summary>
    public void BuButonunRenginiSec()
    {
        if (AvatarYoneticisi.Instance != null)
            AvatarYoneticisi.Instance.RenkSec(builtinRenk);

        if (avatarGorunumu != null)
            avatarGorunumu.Guncelle();
    }
}
