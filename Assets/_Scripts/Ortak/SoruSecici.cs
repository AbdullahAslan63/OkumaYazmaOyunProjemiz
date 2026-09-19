using UnityEngine;

/// <summary>
/// Harf listesinden rastgele soru seçer; ardışık aynı harfi engeller.
/// </summary>
public class SoruSecici : MonoBehaviour
{
    // Inspector'dan 8 harf kartı
    public HarfObjeVerisi[] tumHarfler;

    // Bir önceki seçilen kart (aynı gelmesin diye)
    private HarfObjeVerisi oncekiHarf;

    /// <summary>Rastgele harf döndürür; mümkünse öncekiyle aynı olmaz.</summary>
    public HarfObjeVerisi YeniHarfSec()
    {
        if (tumHarfler == null || tumHarfler.Length == 0)
        {
            Debug.LogWarning("SoruSecici: tumHarfler boş.");
            return null;
        }

        // Tek kart varsa onu döndür
        if (tumHarfler.Length == 1)
        {
            oncekiHarf = tumHarfler[0];
            return oncekiHarf;
        }

        HarfObjeVerisi secilen = null;
        int deneme = 0;
        do
        {
            int i = Random.Range(0, tumHarfler.Length);
            secilen = tumHarfler[i];
            deneme++;
        }
        while (secilen == oncekiHarf && deneme < 20);

        oncekiHarf = secilen;
        return secilen;
    }
}
