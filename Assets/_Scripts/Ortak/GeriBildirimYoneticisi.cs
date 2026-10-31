using UnityEngine;
using TMPro;

/// <summary>
/// Doğru/yanlış geri bildirimi: UI aç/kapa + partikül.
/// Kısa SFX SesYoneticisi üzerinden; obje adı sesi HarfSecme'de ayrı çalar.
/// </summary>
public class GeriBildirimYoneticisi : MonoBehaviour
{
    // Doğru geri bildirim objesi (panel veya Image)
    public GameObject dogruGorsel;

    // Yanlış geri bildirim objesi
    public GameObject yanlisGorsel;

    // İsteğe bağlı skor yazısı
    public TextMeshProUGUI skorYazisi;

    // Tik arkasında partikül patlaması
    public ParticleSystem parlıltıEfekti;

    // Yerel yedek — öncelik SesYoneticisi
    public AudioSource sesKaynagi;
    public AudioClip dogruSes;
    public AudioClip yanlisSes;

    // Gösterim süresi (partikül ile uyumlu 1.5 sn)
    public float gosterimSuresi = 1.5f;

    /// <summary>Doğru cevabı gösterir; kısa ding + partikül. Obje adı sesi ayrı çalar.</summary>
    public void DogruGoster()
    {
        GorseliGoster(dogruGorsel, yanlisGorsel);

        // Partikül yoksa sessizce geç
        if (parlıltıEfekti != null)
            parlıltıEfekti.Play();

        // Öncelik: ortak SesYoneticisi
        if (SesYoneticisi.Instance != null)
        {
            SesYoneticisi.Instance.DogruSesiCal();
            return;
        }

        // Yedek: yerel kaynak (clip yoksa sessiz)
        if (sesKaynagi != null && dogruSes != null)
            sesKaynagi.PlayOneShot(dogruSes);
    }

    /// <summary>Yanlış cevabı gösterir; SesYoneticisi.YanlisSesiCal (yoksa yerel clip).</summary>
    public void YanlisGoster()
    {
        GorseliGoster(yanlisGorsel, dogruGorsel);

        // Öncelik: ortak SesYoneticisi
        if (SesYoneticisi.Instance != null)
        {
            SesYoneticisi.Instance.YanlisSesiCal();
            return;
        }

        // Yedek: yerel kaynak (clip yoksa sessiz)
        if (sesKaynagi != null && yanlisSes != null)
            sesKaynagi.PlayOneShot(yanlisSes);
    }

    /// <summary>Skor yazısını günceller (null ise dokunmaz).</summary>
    public void SkorYazisiniGuncelle(int skor)
    {
        if (skorYazisi != null)
            skorYazisi.text = "Doğru: " + skor;
    }

    /// <summary>Bir görseli açar, diğerini kapatır; süre sonunda gizler.</summary>
    private void GorseliGoster(GameObject acilacak, GameObject kapanacak)
    {
        if (kapanacak != null) kapanacak.SetActive(false);
        if (acilacak != null)
        {
            acilacak.SetActive(true);
            CancelInvoke(nameof(GizleHepsi));
            Invoke(nameof(GizleHepsi), gosterimSuresi);
        }
    }

    private void GizleHepsi()
    {
        if (dogruGorsel != null) dogruGorsel.SetActive(false);
        if (yanlisGorsel != null) yanlisGorsel.SetActive(false);
    }
}
