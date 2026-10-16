using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Herhangi bir Button'a takılır; tıklanınca ortak TiklamaSesiCal çağırır.
/// Butonun kendi OnClick işine karışmaz.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButonTiklamaSesi : MonoBehaviour
{
    private Button buton;

    void Awake()
    {
        buton = GetComponent<Button>();
        if (buton == null) return;

        // Mevcut dinleyicilere ek olarak tıklama sesi
        buton.onClick.AddListener(TiklamayiCal);
    }

    void OnDestroy()
    {
        if (buton != null)
            buton.onClick.RemoveListener(TiklamayiCal);
    }

    /// <summary>SesYoneticisi yoksa veya clip yoksa sessiz.</summary>
    void TiklamayiCal()
    {
        if (SesYoneticisi.Instance != null)
            SesYoneticisi.Instance.TiklamaSesiCal();
    }
}
