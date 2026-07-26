using UnityEngine;
using TMPro;

public class AvatarArayuzKontrolu : MonoBehaviour
{
    [Header("Ekranda Gösterge (İsteğe Bağlı)")]
    public TextMeshProUGUI bilgiMetni;

    private void Update()
    {
        // Seçimlerin anlık güncellendiğini ekranda görmek için
        if (bilgiMetni != null && AvatarYoneticisi.Instance != null)
        {
            string hayvan = AvatarYoneticisi.Instance.seciliHayvanIndex switch
            {
                0 => "Kedi",
                1 => "Tavşan",
                2 => "Kuş",
                3 => "Rakun",
                _ => "Seçilmedi"
            };

            bilgiMetni.text = $"Seçilen: {hayvan}\nAksesuar ID: {AvatarYoneticisi.Instance.seciliAksesuarIndex}";
        }
    }

    // --- BUTON ONCLICK FONKSİYONLARI ---

    public void Buton_HayvanSec(int index)
    {
        AvatarYoneticisi.Instance.HayvanSec(index);
    }

    public void Buton_RenkKirmizi()
    {
        AvatarYoneticisi.Instance.RenkSec(Color.red);
    }

    public void Buton_RenkMavi()
    {
        AvatarYoneticisi.Instance.RenkSec(Color.blue);
    }

    public void Buton_RenkYesil()
    {
        AvatarYoneticisi.Instance.RenkSec(Color.green);
    }

    public void Buton_AksesuarSec(int index)
    {
        AvatarYoneticisi.Instance.AksesuarSec(index);
    }


}