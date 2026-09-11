using UnityEngine;

/// <summary>
/// Harf oyunu sahnesini yönetir. Inspector'dan sesli harf verilerini sürükleyip bırakarak atayın.
/// </summary>
public class HarfOyunuYoneticisi : MonoBehaviour
{
    // Büyük sesli harfler (A, E, I, İ, O, Ö, U, Ü) — Inspector'dan HarfObjeVerisi asset'lerini sürükleyin.
    public HarfObjeVerisi[] sesliHarfler;

    void Start()
    {
        if (sesliHarfler == null || sesliHarfler.Length == 0)
        {
            Debug.LogWarning("HarfOyunuYoneticisi: sesliHarfler dizisi boş. Inspector'dan harf asset'lerini atayın.");
            return;
        }

        Debug.Log($"HarfOyunuYoneticisi: {sesliHarfler.Length} sesli harf yüklendi.");

        foreach (var harfVerisi in sesliHarfler)
        {
            if (harfVerisi != null)
                Debug.Log($"  Harf: {harfVerisi.harf}");
        }
    }
}
