using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level ekranı navigasyonu: seviye kartları ve geri butonu.
/// </summary>
public class SeviyeSecimEkrani : MonoBehaviour
{
    // Geri butonu hedefi
    public string geriSahneAdi = "AvatarOlusturmaEkrani";

    /// <summary>Seçilen seviye sahnesini açar (boş veya kilitli ise dokunmaz).</summary>
    public void SeviyeAc(string sahneAdi)
    {
        if (string.IsNullOrEmpty(sahneAdi))
        {
            Debug.LogWarning("SeviyeSecimEkrani: sahneAdi boş (kilitli seviye).");
            return;
        }

        SceneManager.LoadScene(sahneAdi);
    }

    /// <summary>Avatar / karakter ekranına döner.</summary>
    public void GeriDon()
    {
        if (string.IsNullOrEmpty(geriSahneAdi))
        {
            Debug.LogWarning("SeviyeSecimEkrani: geriSahneAdi boş.");
            return;
        }

        SceneManager.LoadScene(geriSahneAdi);
    }
}
