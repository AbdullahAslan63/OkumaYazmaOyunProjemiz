/// <summary>
/// Her iki mini oyun için ortak yıldız eşiği (ilkokul dostu).
/// 0–1 doğru → 1, 2–4 → 2, ≥5 → 3.
/// </summary>
public static class YildizHesaplayici
{
    /// <summary>Doğru sayısına göre 1–3 yıldız döner.</summary>
    public static int Hesapla(int dogruSayisi)
    {
        if (dogruSayisi >= 5)
        {
            return 3;
        }

        if (dogruSayisi >= 2)
        {
            return 2;
        }

        return 1;
    }
}
