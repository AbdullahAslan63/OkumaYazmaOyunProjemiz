using UnityEngine;

/// <summary>
/// Seviye başına en iyi yıldız sayısını PlayerPrefs ile saklar.
/// 1 = SepetOyunu, 2 = HarfSecmeOyunu.
/// </summary>
public static class SeviyeIlerleme
{
    // PlayerPrefs anahtar öneki
    private const string PrefOnEk = "Yildiz_Seviye_";

    /// <summary>Kayıtlı yıldızı okur (0–3); yoksa 0.</summary>
    public static int YildizOku(int seviyeNo)
    {
        // Geçersiz seviye → 0
        if (seviyeNo < 1) return 0;
        int deger = PlayerPrefs.GetInt(PrefOnEk + seviyeNo, 0);
        return Mathf.Clamp(deger, 0, 3);
    }

    /// <summary>Yıldızı kaydeder; yalnızca eski kayıttan yüksekse yazar.</summary>
    public static void KaydetYildiz(int seviyeNo, int yildiz)
    {
        // Geçersiz seviye → dokunma
        if (seviyeNo < 1) return;
        // 0–3 aralığına sıkıştır
        int yeni = Mathf.Clamp(yildiz, 0, 3);
        int eski = YildizOku(seviyeNo);
        // En iyi skor korunur
        if (yeni <= eski) return;
        PlayerPrefs.SetInt(PrefOnEk + seviyeNo, yeni);
        PlayerPrefs.Save();
    }
}
