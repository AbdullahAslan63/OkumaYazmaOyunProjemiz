using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI seçenek balonu: obje resmi + tıklanınca doğru/yanlış bildirir.
/// Glow yalnız objenin arkasında (beyaz); balon dışı halo yok.
/// </summary>
public class SecenekBalonu : MonoBehaviour
{
    public Image objeResmi;
    // Obje altındaki yumuşak beyaz bulut
    public Image objeArkaPlan;

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

    [Header("Obje altı bulut / parıltı")]
    [Range(1f, 1.45f)]
    public float arkaPlanOlcek = 1.32f;
    // Saf beyaz soft halo — yalnız obje
    public Color arkaPlanRengi = new Color(1f, 1f, 1f, 0.95f);

    private void Awake()
    {
        buton = GetComponent<Button>();
        if (objeResmi == null)
        {
            Transform child = transform.Find("ObjeResmi");
            if (child != null)
                objeResmi = child.GetComponent<Image>();
        }

        BalonMaskesiniGarantiEt();
        ObjeArkaPlaniniGarantiEt();
        BalonDisHaloyuKaldir();
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
    /// Balon boyutunu slot ile eşler; objeyi zarf bölgesinde ortalar.
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

        BalonMaskesiniGarantiEt();
        ObjeArkaPlaniniGarantiEt();
        BalonDisHaloyuKaldir();

        if (objeResmi == null) return;

        RectTransform objeRt = objeResmi.rectTransform;
        objeRt.anchorMin = new Vector2(0.5f, 0.5f);
        objeRt.anchorMax = new Vector2(0.5f, 0.5f);
        objeRt.pivot = new Vector2(0.5f, 0.5f);
        objeRt.localScale = Vector3.one;
        objeRt.localRotation = Quaternion.identity;

        float kutuW = alanW * objeGenislikOrani;
        float kutuH = alanH * objeYukseklikOrani;
        objeRt.sizeDelta = new Vector2(kutuW, kutuH);
        Vector2 objePoz = new Vector2(0f, alanH * objeDikeyKayma);
        objeRt.anchoredPosition = objePoz;

        objeResmi.preserveAspect = true;
        objeResmi.type = Image.Type.Simple;
        objeResmi.raycastTarget = false;

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
                    objeRt.sizeDelta = new Vector2(kutuW, kutuW / spriteOran);
                else
                    objeRt.sizeDelta = new Vector2(kutuH * spriteOran, kutuH);
            }
        }

        // Beyaz halo — objeyi sıkı saran, mask içinde
        if (objeArkaPlan != null)
        {
            RectTransform arkaRt = objeArkaPlan.rectTransform;
            arkaRt.anchorMin = new Vector2(0.5f, 0.5f);
            arkaRt.anchorMax = new Vector2(0.5f, 0.5f);
            arkaRt.pivot = new Vector2(0.5f, 0.5f);
            arkaRt.localScale = Vector3.one;
            arkaRt.localRotation = Quaternion.identity;
            arkaRt.anchoredPosition = objePoz;
            arkaRt.SetSiblingIndex(objeRt.GetSiblingIndex());

            float olcek = Mathf.Clamp(arkaPlanOlcek, 1.05f, 1.45f);
            arkaRt.sizeDelta = new Vector2(
                objeRt.sizeDelta.x * olcek,
                objeRt.sizeDelta.y * olcek);

            objeArkaPlan.color = arkaPlanRengi;
            objeArkaPlan.raycastTarget = false;
            objeArkaPlan.maskable = true;
            objeResmi.maskable = true;

            PariltiNabzi nab = objeArkaPlan.GetComponent<PariltiNabzi>();
            if (nab == null)
                nab = objeArkaPlan.gameObject.AddComponent<PariltiNabzi>();
            nab.TabanOlcegiKaydet(Vector3.one);
        }
    }

    /// <summary>Eski balon dışı halo'yu temizler.</summary>
    void BalonDisHaloyuKaldir()
    {
        Transform slot = transform.parent;
        if (slot == null)
            return;

        Transform haloT = slot.Find("BalonDisHalo");
        if (haloT != null)
            Destroy(haloT.gameObject);
    }

    void BalonMaskesiniGarantiEt()
    {
        Mask mask = GetComponent<Mask>();
        if (mask == null)
            mask = gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;
    }

    void ObjeArkaPlaniniGarantiEt()
    {
        if (objeArkaPlan == null)
        {
            Transform mevcut = transform.Find("ObjeArkaPlan");
            if (mevcut != null)
                objeArkaPlan = mevcut.GetComponent<Image>();
        }

        if (objeArkaPlan == null)
        {
            GameObject go = new GameObject("ObjeArkaPlan", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            objeArkaPlan = go.GetComponent<Image>();

            if (objeResmi != null)
                go.transform.SetSiblingIndex(objeResmi.transform.GetSiblingIndex());
            else
                go.transform.SetAsFirstSibling();
        }

        objeArkaPlan.sprite = YumusakPariltiYardimcisi.DaireSpriteAl();
        objeArkaPlan.type = Image.Type.Simple;
        objeArkaPlan.preserveAspect = false;
        objeArkaPlan.color = arkaPlanRengi;
        objeArkaPlan.raycastTarget = false;
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
        Sprite resim = objeResmi != null ? objeResmi.sprite : null;
        // Tıklanan balonu ilet — tik/çarpı bunun üzerinde
        Transform hedef = objeResmi != null ? objeResmi.transform : transform;
        HarfSecmeYoneticisi.Instance.SecenekSecildi(dogruMu, resim, hedef);
    }

    public void TiklamayiAyarla(bool acik)
    {
        if (buton == null)
            buton = GetComponent<Button>();
        if (buton != null)
            buton.interactable = acik;
    }
}
