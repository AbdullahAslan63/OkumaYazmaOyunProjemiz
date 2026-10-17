using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Doğru/yanlış geri bildirimi: ikon pop animasyonu + null-safe ses.
/// </summary>
public class GeriBildirimYoneticisi : MonoBehaviour
{
    // Doğru geri bildirim objesi (correct_check Image)
    public GameObject dogruGorsel;

    // Yanlış geri bildirim objesi (wrong_x Image)
    public GameObject yanlisGorsel;

    // İsteğe bağlı skor yazısı
    public TextMeshProUGUI skorYazisi;

    // Ses (clip yoksa sessiz geçer)
    public AudioSource sesKaynagi;
    public AudioClip dogruSes;
    public AudioClip yanlisSes;

    // PlayOneShot ses çarpanı (1 = normal, 1.5+ daha yüksek)
    public float sesSeviyesi = 1.5f;

    // Otomatik gizleme süresi
    public float gosterimSuresi = 0.8f;

    // Pop animasyon süresi
    public float animasyonSuresi = 0.35f;

    // Çalışan gizleme / animasyon
    private Coroutine aktifAnim;

    /// <summary>Doğru cevabı gösterir (tik + ses).</summary>
    public void DogruGoster()
    {
        // Tik aç, çarpı kapat, doğru ses
        Goster(dogruGorsel, yanlisGorsel, dogruSes);
    }

    /// <summary>Yanlış cevabı gösterir (çarpı + ses).</summary>
    public void YanlisGoster()
    {
        // Çarpı aç, tik kapat, yanlış ses
        Goster(yanlisGorsel, dogruGorsel, yanlisSes);
    }

    /// <summary>Skor yazısını günceller (null ise dokunmaz).</summary>
    public void SkorYazisiniGuncelle(int skor)
    {
        // Yazı bağlıysa
        if (skorYazisi != null)
        {
            // Metin
            skorYazisi.text = "Puan: " + skor;
        }
    }

    // İkonu pop ile aç, diğerini kapat, ses çal
    private void Goster(GameObject acilacak, GameObject kapanacak, AudioClip clip)
    {
        // Diğer ikonu kapat
        if (kapanacak != null)
        {
            // Gizle
            kapanacak.SetActive(false);
        }

        // Açılacak ikon varsa animasyonla göster
        if (acilacak != null)
        {
            // Önceki animasyonu iptal et
            if (aktifAnim != null)
            {
                // Durdur
                StopCoroutine(aktifAnim);
            }

            // Pop + gizleme
            aktifAnim = StartCoroutine(PopVeGizle(acilacak));
        }

        // Ses kaynağı ve clip varsa çal
        if (sesKaynagi != null && clip != null)
        {
            // Tek seferlik, sesSeviyesi ile yüksek
            sesKaynagi.PlayOneShot(clip, Mathf.Clamp(sesSeviyesi, 0f, 1.5f));
        }
    }

    // Küçük → büyük → normale pop, sonra gizle
    private IEnumerator PopVeGizle(GameObject hedef)
    {
        // Objeyi aç
        hedef.SetActive(true);
        // Ölçek dönüşümü
        Transform t = hedef.transform;
        // Hedef ölçek (Inspector'daki)
        Vector3 taban = Vector3.one;
        // Sıfırdan başla
        t.localScale = Vector3.zero;
        // Animasyon zamanı
        float sure = Mathf.Max(0.05f, animasyonSuresi);
        float gecen = 0f;
        // Büyüme + hafif taşma
        while (gecen < sure)
        {
            // Zaman ilerle
            gecen += Time.deltaTime;
            // 0..1
            float n = Mathf.Clamp01(gecen / sure);
            // Ease-out + overshoot
            float ease = 1f - Mathf.Pow(1f - n, 3f);
            // 1.25'e çıkıp 1'e otur
            float carpim = n < 0.7f
                ? Mathf.Lerp(0f, 1.25f, ease / 0.7f)
                : Mathf.Lerp(1.25f, 1f, (n - 0.7f) / 0.3f);
            // Ölçeği yaz
            t.localScale = taban * carpim;
            // Sonraki kare
            yield return null;
        }

        // Tam boyut
        t.localScale = taban;
        // Bir süre ekranda kalsın
        yield return new WaitForSeconds(gosterimSuresi);
        // Her iki ikonu kapat
        GizleHepsi();
        // Coroutine bitti
        aktifAnim = null;
    }

    // İkonları kapat
    private void GizleHepsi()
    {
        // Doğru
        if (dogruGorsel != null)
        {
            dogruGorsel.SetActive(false);
        }

        // Yanlış
        if (yanlisGorsel != null)
        {
            yanlisGorsel.SetActive(false);
        }
    }
}
