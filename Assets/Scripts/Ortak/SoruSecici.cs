using UnityEngine;

/// <summary>
/// Harf listesinden rastgele bir harf seçer.
/// Ardışık iki seçimde aynı harfin gelmemesini sağlar.
/// Unity sahnesindeki bir GameObject'e eklenerek kullanılır.
/// </summary>
public class SoruSecici : MonoBehaviour
{
    // Inspector'dan atanacak tüm harf kayıtları (HarfObjeVerisi ScriptableObject'leri).
    // Her eleman bir harfi ve ona ait görselleri temsil eder.
    public HarfObjeVerisi[] tumHarfler;

    // Bir önceki seçimin dizideki indeksini tutar.
    // İlk seçimde henüz seçim yapılmadığı için -1 ile başlar.
    private int oncekiIndeks = -1;

    /// <summary>
    /// tumHarfler dizisinden rastgele bir harf kaydı seçer ve döndürür.
    /// Önceki seçimle aynı elemanı tekrar seçmez (tek elemanlı dizi hariç).
    /// </summary>
    /// <returns>Seçilen HarfObjeVerisi; dizi boş veya null ise null döner.</returns>
    public HarfObjeVerisi YeniHarfSec()
    {
        // Dizi atanmamış veya hiç eleman yoksa seçim yapılamaz.
        if (tumHarfler == null || tumHarfler.Length == 0)
        {
            return null;
        }

        // Tek eleman varsa tekrarlı seçim kaçınılmazdır; yine de o elemanı döndürürüz.
        if (tumHarfler.Length == 1)
        {
            oncekiIndeks = 0;
            return tumHarfler[0];
        }

        // Önceki indeksten farklı bir indeks bulana kadar rastgele seç.
        int yeniIndeks;
        do
        {
            // Random.Range(0, uzunluk) → 0 dahil, uzunluk hariç tam sayı üretir.
            yeniIndeks = Random.Range(0, tumHarfler.Length);
        }
        while (yeniIndeks == oncekiIndeks);

        // Seçimi kaydet; bir sonraki çağrıda bu indeksten kaçınılacak.
        oncekiIndeks = yeniIndeks;

        return tumHarfler[yeniIndeks];
    }
}
