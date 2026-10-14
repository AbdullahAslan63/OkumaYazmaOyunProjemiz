using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI seçenek balonu: obje resmi + tıklanınca doğru/yanlış bildirir.
/// Parent slot boyutunu alır; objeyi balon sepetinde merkezli ve orantılı gösterir.
/// </summary>
public class SecenekBalonu : MonoBehaviour
{
    public Image objeResmi;

    private char dogruHarf;
    private char aktifSoruHarfi;
    private Button buton;

    [Header("İç yerleşim — balon ZARFI (sepet değil)")]
    [Tooltip("Balon genişliğine göre obje kutusu (taşmayı önlemek için düşük tut)")]
    [Range(0.25f, 0.7f)]
    public float objeGenislikOrani = 0.46f;
    [Range(0.2f, 0.65f)]
    public float objeYukseklikOrani = 0.40f;
    [Tooltip("Pozitif = yukarı (zarf merkezi); negatif = sepete doğru")]
    [Range(-0.2f, 0.35f)]
    public float objeDikeyKayma = 0.14f;

    private void Awake()
    {
        buton = GetComponent<Button>();
        if (objeResmi == null)
        {
            Transform child = transform.Find("ObjeResmi");
            if (child != null)
                objeResmi = child.GetComponent<Image>();
        }
        YerlesimiDuzenle(false);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled)
            YerlesimiDuzenle(false);
    }

    /// <summary>Sprite ve harf bilgisini ayarlar; tıklamayı bağlar.</summary>
    public void Ayarla(Sprite resim, char objeHarfi, char soruHarfi)
    {
        dogruHarf = objeHarfi;
        aktifSoruHarfi = soruHarfi;

        if (objeResmi != null && resim != null)
        {
            objeResmi.sprite = resim;
            objeResmi.preserveAspect = true;
            objeResmi.type = Image.Type.Simple;
            objeResmi.raycastTarget = false;
        }

        // Sprite atandıktan sonra zarf kutusuna sığdır
        YerlesimiDuzenle(false);

        if (buton == null)
            buton = GetComponent<Button>();

        if (buton != null)
        {
            buton.onClick.RemoveAllListeners();
            buton.onClick.AddListener(Tiklandi);
            buton.interactable = true;
        }
    }

    /// <summary>
    /// Balon boyutunu slot ile eşler; objeyi sepet bölgesinde ortalar.
    /// pozisyonuSifirla=false iken yükseliş animasyonunun Y’sine dokunmaz.
    /// </summary>
    public void YerlesimiDuzenle(bool pozisyonuSifirla = false)
    {
        RectTransform kok = transform as RectTransform;
        if (kok == null) return;

        RectTransform slot = kok.parent as RectTransform;
        float alanW = 360f;
        float alanH = 430f;

        if (slot != null)
        {
            Canvas.ForceUpdateCanvases();
            Rect sr = slot.rect;
            if (sr.width > 1f) alanW = sr.width;
            if (sr.height > 1f) alanH = sr.height;

            kok.anchorMin = new Vector2(0.5f, 0.5f);
            kok.anchorMax = new Vector2(0.5f, 0.5f);
            kok.pivot = new Vector2(0.5f, 0.5f);
            kok.sizeDelta = new Vector2(alanW, alanH);
            kok.localScale = Vector3.one;
            if (pozisyonuSifirla)
                kok.anchoredPosition = Vector2.zero;
        }

        if (objeResmi == null) return;

        RectTransform objeRt = objeResmi.rectTransform;
        objeRt.anchorMin = new Vector2(0.5f, 0.5f);
        objeRt.anchorMax = new Vector2(0.5f, 0.5f);
        objeRt.pivot = new Vector2(0.5f, 0.5f);
        objeRt.localScale = Vector3.one;
        objeRt.localRotation = Quaternion.identity;

        // Zarf içi kutu — sprite preserveAspect ile bu kutuya sığar, taşmaz
        float kutuW = alanW * objeGenislikOrani;
        float kutuH = alanH * objeYukseklikOrani;
        objeRt.sizeDelta = new Vector2(kutuW, kutuH);
        objeRt.anchoredPosition = new Vector2(0f, alanH * objeDikeyKayma);

        objeResmi.preserveAspect = true;
        objeResmi.type = Image.Type.Simple;
        objeResmi.raycastTarget = false;

        // Sprite en-boy oranı kutuyu aşmasın: kutu içinde Fit
        if (objeResmi.sprite != null)
        {
            Sprite s = objeResmi.sprite;
            float sw = s.rect.width;
            float sh = s.rect.height;
            if (sw > 0.01f && sh > 0.01f)
            {
                float spriteOran = sw / sh;
                float kutuOran = kutuW / kutuH;
                if (spriteOran > kutuOran)
                {
                    // Geniş sprite: genişliği sabitle, yüksekliği oranla
                    objeRt.sizeDelta = new Vector2(kutuW, kutuW / spriteOran);
                }
                else
                {
                    // Uzun sprite: yüksekliği sabitle, genişliği oranla
                    objeRt.sizeDelta = new Vector2(kutuH * spriteOran, kutuH);
                }
            }
        }
    }

    public void SoruHarfiniGuncelle(char soruHarfi)
    {
        aktifSoruHarfi = soruHarfi;
    }

    public void Tiklandi()
    {
        if (HarfSecmeYoneticisi.Instance == null)
            return;

        bool dogruMu = dogruHarf == aktifSoruHarfi;
        HarfSecmeYoneticisi.Instance.SecenekSecildi(dogruMu);
    }

    public void TiklamayiAyarla(bool acik)
    {
        if (buton == null)
            buton = GetComponent<Button>();
        if (buton != null)
            buton.interactable = acik;
    }
}
