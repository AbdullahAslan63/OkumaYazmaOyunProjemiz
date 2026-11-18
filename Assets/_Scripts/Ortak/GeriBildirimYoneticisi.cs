using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Doğru/yanlış geri bildirimi: UI aç/kapa + partikül.
/// Hedef verilirse tik/çarpı tıklanan objenin üzerinde gösterilir.
/// </summary>
public class GeriBildirimYoneticisi : MonoBehaviour
{
    public GameObject dogruGorsel;
    public GameObject yanlisGorsel;
    public TextMeshProUGUI skorYazisi;
    public ParticleSystem parlıltıEfekti;

    public AudioSource sesKaynagi;
    public AudioClip dogruSes;
    public AudioClip yanlisSes;

    public float gosterimSuresi = 1.5f;

    // Hedefe taşımadan önceki parent / konum
    private Transform dogruEskiParent;
    private Transform yanlisEskiParent;
    private Vector3 dogruEskiLocalPos;
    private Vector3 yanlisEskiLocalPos;
    private Transform psEskiParent;
    private Vector3 psEskiLocalPos;

    /// <summary>Doğru cevabı gösterir; hedef varsa onun üzerinde.</summary>
    public void DogruGoster(Transform hedef = null)
    {
        HedefeTasi(dogruGorsel, hedef, true);
        GorseliGoster(dogruGorsel, yanlisGorsel);

        if (parlıltıEfekti != null)
            parlıltıEfekti.Play();

        if (SesYoneticisi.Instance != null)
        {
            SesYoneticisi.Instance.DogruSesiCal();
            return;
        }

        if (sesKaynagi != null && dogruSes != null)
            sesKaynagi.PlayOneShot(dogruSes);
    }

    /// <summary>Yanlış cevabı gösterir; hedef varsa onun üzerinde.</summary>
    public void YanlisGoster(Transform hedef = null)
    {
        HedefeTasi(yanlisGorsel, hedef, false);
        GorseliGoster(yanlisGorsel, dogruGorsel);

        // Yanlışta da kısa partikül (aynı burst varsa)
        if (parlıltıEfekti != null && hedef != null)
            parlıltıEfekti.Play();

        if (SesYoneticisi.Instance != null)
        {
            SesYoneticisi.Instance.YanlisSesiCal();
            return;
        }

        if (sesKaynagi != null && yanlisSes != null)
            sesKaynagi.PlayOneShot(yanlisSes);
    }

    public void SkorYazisiniGuncelle(int skor)
    {
        if (skorYazisi != null)
            skorYazisi.text = "Doğru: " + skor;
    }

    /// <summary>İkonu (ve partikülü) hedef UI altına taşır.</summary>
    void HedefeTasi(GameObject ikon, Transform hedef, bool dogruMu)
    {
        if (ikon == null)
            return;

        // Eski parent kaydet
        if (dogruMu)
        {
            if (dogruEskiParent == null)
            {
                dogruEskiParent = ikon.transform.parent;
                dogruEskiLocalPos = ikon.transform.localPosition;
            }
        }
        else
        {
            if (yanlisEskiParent == null)
            {
                yanlisEskiParent = ikon.transform.parent;
                yanlisEskiLocalPos = ikon.transform.localPosition;
            }
        }

        if (hedef == null)
        {
            // Merkez / eski konuma dön
            Transform eskiP = dogruMu ? dogruEskiParent : yanlisEskiParent;
            Vector3 eskiPos = dogruMu ? dogruEskiLocalPos : yanlisEskiLocalPos;
            if (eskiP != null)
            {
                ikon.transform.SetParent(eskiP, false);
                ikon.transform.localPosition = eskiPos;
            }

            PsEskiyeDon();
            return;
        }

        ikon.transform.SetParent(hedef, false);
        RectTransform rt = ikon.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            // Obje üstünde belirgin boyut
            if (rt.sizeDelta.x < 10f || rt.sizeDelta.y < 10f)
                rt.sizeDelta = new Vector2(140f, 140f);
            else
                rt.sizeDelta = new Vector2(140f, 140f);
        }
        else
        {
            ikon.transform.localPosition = Vector3.zero;
            ikon.transform.localScale = Vector3.one;
        }

        ikon.transform.SetAsLastSibling();

        // Image raycast kapalı; Mask dışına taşabilsin (balon kırpmasın)
        Image img = ikon.GetComponent<Image>();
        if (img != null)
        {
            img.enabled = true;
            img.color = Color.white;
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.maskable = false;
        }

        // Partikülü de hedefe taşı
        if (parlıltıEfekti != null)
        {
            if (psEskiParent == null)
            {
                psEskiParent = parlıltıEfekti.transform.parent;
                psEskiLocalPos = parlıltıEfekti.transform.localPosition;
            }

            parlıltıEfekti.transform.SetParent(hedef, false);
            parlıltıEfekti.transform.localPosition = Vector3.zero;
            parlıltıEfekti.transform.localScale = Vector3.one;
        }
    }

    void PsEskiyeDon()
    {
        if (parlıltıEfekti == null || psEskiParent == null)
            return;
        parlıltıEfekti.transform.SetParent(psEskiParent, false);
        parlıltıEfekti.transform.localPosition = psEskiLocalPos;
    }

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
        if (dogruGorsel != null)
        {
            dogruGorsel.SetActive(false);
            if (dogruEskiParent != null)
            {
                dogruGorsel.transform.SetParent(dogruEskiParent, false);
                dogruGorsel.transform.localPosition = dogruEskiLocalPos;
            }
        }

        if (yanlisGorsel != null)
        {
            yanlisGorsel.SetActive(false);
            if (yanlisEskiParent != null)
            {
                yanlisGorsel.transform.SetParent(yanlisEskiParent, false);
                yanlisGorsel.transform.localPosition = yanlisEskiLocalPos;
            }
        }

        PsEskiyeDon();
    }
}
