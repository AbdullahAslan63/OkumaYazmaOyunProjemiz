using UnityEngine;
using TMPro;

public class AvatarArayuzKontrolu : MonoBehaviour
{
    [Header("Ekranda Gösterge (İsteğe Bağlı)")]
    public TextMeshProUGUI bilgiMetni;

    private void Update()
    {
        if (bilgiMetni == null || AvatarYoneticisi.Instance == null)
            return;

        string hayvan = AvatarYoneticisi.Instance.seciliHayvanIndex switch
        {
            0 => "Kedi",
            1 => "Tavşan",
            2 => "Köpek",
            3 => "Rakun",
            _ => "Seçilmedi"
        };

        string aksesuar = AvatarCustomizer.Instance != null
            ? AvatarCustomizer.Instance.GetSelectedAccessory()
            : AvatarYoneticisi.Instance.seciliAksesuarIndex.ToString();

        bilgiMetni.text = $"Seçilen: {hayvan}\nAksesuar: {aksesuar}";
    }

    public void Buton_HayvanSec(int index)
    {
        string character = index switch
        {
            0 => "kedi",
            1 => "tavsan",
            2 => "kopek",
            3 => "rakun",
            _ => "tavsan"
        };

        if (AvatarCustomizer.Instance != null)
            AvatarCustomizer.Instance.SelectCharacter(character);
        else if (AvatarYoneticisi.Instance != null)
            AvatarYoneticisi.Instance.HayvanSec(index);
    }

    public void Buton_RenkKirmizi() => UygulaRenk(Color.red);
    public void Buton_RenkMavi() => UygulaRenk(Color.blue);
    public void Buton_RenkYesil() => UygulaRenk(Color.green);
    public void Buton_RenkBeyaz() => UygulaRenk(Color.white);

    public void Buton_AksesuarSec(int index)
    {
        string accessory = index switch
        {
            -1 => "yok",
            0 => "sapka",
            1 => "parti sapkasi",
            2 => "papyon",
            3 => "kulaklik",
            4 => "gozluk",
            _ => "yok"
        };

        if (AvatarCustomizer.Instance != null)
            AvatarCustomizer.Instance.SelectAccessory(accessory);
        else if (AvatarYoneticisi.Instance != null)
            AvatarYoneticisi.Instance.AksesuarSec(index);
    }

    private void UygulaRenk(Color renk)
    {
        if (AvatarCustomizer.Instance != null)
            AvatarCustomizer.Instance.SelectColor(renk);
        else if (AvatarYoneticisi.Instance != null)
            AvatarYoneticisi.Instance.RenkSec(renk);
    }
}
