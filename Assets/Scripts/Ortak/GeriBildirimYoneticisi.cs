using System.Collections;
using UnityEngine;

// GeriBildirimYoneticisi: Doğru/yanlış cevaplarda ikon, ses ve parçacık efektlerini gösterir.
// Bu script'i bir GameObject'e ekleyin; Inspector'dan ikon, ses ve parçacık referanslarını atayın.
public class GeriBildirimYoneticisi : MonoBehaviour
{
    // Doğru ve yanlış cevapta kısa süre gösterilecek UI ikon GameObject'leri.
    public GameObject dogruIkon;
    public GameObject yanlisIkon;

    // Doğru cevapta oynatılacak parıltı parçacık efekti.
    public ParticleSystem parlitiEfekti;

    // Geri bildirim seslerini çalmak için AudioSource bileşeni.
    public AudioSource sesKaynagi;
    public AudioClip dogruSesi;
    public AudioClip yanlisSesi;

    // İkonların ekranda kalma süresi (saniye).
    private const float ikonGosterimSuresi = 1f;

    private void Start()
    {
        // Oyun başında ikonlar gizli olsun; yalnızca cevap anında gösterilir.
        if (dogruIkon != null)
        {
            dogruIkon.SetActive(false);
        }

        if (yanlisIkon != null)
        {
            yanlisIkon.SetActive(false);
        }
    }

    /// <summary>
    /// Doğru cevap geri bildirimini tetikler: parıltı efekti, ses ve ikon gösterimi.
    /// </summary>
    public void DogruGoster()
    {
        if (parlitiEfekti != null)
        {
            parlitiEfekti.Play();
        }

        if (sesKaynagi != null && dogruSesi != null)
        {
            sesKaynagi.PlayOneShot(dogruSesi);
        }

        StartCoroutine(IkonGosterVeGizle(dogruIkon));
    }

    /// <summary>
    /// Yanlış cevap geri bildirimini tetikler: ses ve ikon gösterimi.
    /// </summary>
    public void YanlisGoster()
    {
        if (sesKaynagi != null && yanlisSesi != null)
        {
            sesKaynagi.PlayOneShot(yanlisSesi);
        }

        StartCoroutine(IkonGosterVeGizle(yanlisIkon));
    }

    // Coroutine: Ana oyun akışını durdurmadan belirli süre bekleyip ikonu gizler.
    // StartCoroutine ile başlatılır; WaitForSeconds süre dolana kadar yield return bekler,
    // sonra ikonu kapatır. Böylece 1 saniyelik gösterim arka planda, kare kare ilerler.
    private IEnumerator IkonGosterVeGizle(GameObject ikon)
    {
        if (ikon == null)
        {
            yield break;
        }

        ikon.SetActive(true);

        // Belirtilen süre kadar bekle (oyun donmaz, diğer scriptler çalışmaya devam eder).
        yield return new WaitForSeconds(ikonGosterimSuresi);

        ikon.SetActive(false);
    }
}
