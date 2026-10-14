using UnityEngine;

/// <summary>
/// Basit skor sayacı; mini oyunlar SkorEkle ile puan ekler.
/// </summary>
public class SkorYoneticisi : MonoBehaviour
{
    // Güncel skor
    public int skor;

    // Her doğru için varsayılan puan
    public int dogruPuan = 10;

    /// <summary>Skora puan ekler.</summary>
    public void SkorEkle(int miktar)
    {
        skor += miktar;
    }

    /// <summary>Varsayılan doğru puanını ekler.</summary>
    public void DogruPuanEkle()
    {
        SkorEkle(dogruPuan);
    }

    /// <summary>Skoru sıfırlar.</summary>
    public void SkoruSifirla()
    {
        skor = 0;
    }
}
