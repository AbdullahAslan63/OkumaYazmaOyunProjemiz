using UnityEngine;
using TMPro;

/// <summary>
/// Doğru/yanlış geri bildirimi: UI Image veya panel aç/kapa; ses clip null-safe.
/// </summary>
public class GeriBildirimYoneticisi : MonoBehaviour
{
    // Doğru geri bildirim objesi (panel veya Image)
    public GameObject dogruGorsel;

    // Yanlış geri bildirim objesi
    public GameObject yanlisGorsel;

    // İsteğe bağlı skor yazısı
    public TextMeshProUGUI skorYazisi;

    // Ses (clip yoksa sessiz geçer)
    public AudioSource sesKaynagi;
    public AudioClip dogruSes;
    public AudioClip yanlisSes;

    // Otomatik gizleme süresi
    public float gosterimSuresi = 0.8f;

    /// <summary>Doğru cevabı gösterir.</summary>
    public void DogruGoster()
    {
        Goster(dogruGorsel, yanlisGorsel, dogruSes);
    }

    /// <summary>Yanlış cevabı gösterir.</summary>
    public void YanlisGoster()
    {
        Goster(yanlisGorsel, dogruGorsel, yanlisSes);
    }

    /// <summary>Skor yazısını günceller (null ise dokunmaz).</summary>
    public void SkorYazisiniGuncelle(int skor)
    {
        if (skorYazisi != null)
            skorYazisi.text = "Doğru: " + skor;
    }

    private void Goster(GameObject acilacak, GameObject kapanacak, AudioClip clip)
    {
        if (kapanacak != null) kapanacak.SetActive(false);
        if (acilacak != null)
        {
            acilacak.SetActive(true);
            CancelInvoke(nameof(GizleHepsi));
            Invoke(nameof(GizleHepsi), gosterimSuresi);
        }

        if (sesKaynagi != null && clip != null)
            sesKaynagi.PlayOneShot(clip);
    }

    private void GizleHepsi()
    {
        if (dogruGorsel != null) dogruGorsel.SetActive(false);
        if (yanlisGorsel != null) yanlisGorsel.SetActive(false);
    }
}
