using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LevelEkranı sol alt buton: tıklanınca yalnızca fon müziğini aç/kapa (SFX açık kalır).
/// </summary>
[RequireComponent(typeof(Button))]
public class SesAcKapaButonu : MonoBehaviour
{
    // Buton üzerindeki durum yazısı (Müzik / Kapalı)
    public TextMeshProUGUI durumYazisi;

    private Button buton;

    void Awake()
    {
        buton = GetComponent<Button>();
        if (buton != null)
            buton.onClick.AddListener(Tiklandi);

        GorunumuGuncelle();
    }

    void OnEnable()
    {
        GorunumuGuncelle();
    }

    void OnDestroy()
    {
        if (buton != null)
            buton.onClick.RemoveListener(Tiklandi);
    }

    void Tiklandi()
    {
        SesYoneticisi ses = SesYoneticisi.GarantiEt();
        if (ses != null)
            ses.SesAcKapa();
        GorunumuGuncelle();
    }

    /// <summary>Açıkken "Müzik", kapalıyken "Kapalı" (yalnızca fon müziği).</summary>
    public void GorunumuGuncelle()
    {
        bool kapali = SesYoneticisi.Instance != null && SesYoneticisi.Instance.SesKapaliMi;
        if (durumYazisi != null)
            durumYazisi.text = kapali ? "Kapalı" : "Müzik";
    }
}
