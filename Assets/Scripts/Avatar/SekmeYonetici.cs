using UnityEngine;

public class SekmeYonetici : MonoBehaviour
{
    [Header("Açılıp Kapanacak Paneller")]
    public GameObject karakterPaneli;
    public GameObject renkPaneli;
    public GameObject aksesuarPaneli;

    private void Start()
    {
        // Oyun ilk başladığında sadece Karakter Paneli açık olsun
        KarakterSekmesiniAc();
    }

    // Karakter Sekmesine basılınca çağrılacak
    public void KarakterSekmesiniAc()
    {
        karakterPaneli.SetActive(true);   // Karakter butonlarını AÇ
        renkPaneli.SetActive(false);      // Renk butonlarını GİZLE
        aksesuarPaneli.SetActive(false);  // Aksesuar butonlarını GİZLE
    }

    // Renk Sekmesine basılınca çağrılacak
    public void RenkSekmesiniAc()
    {
        karakterPaneli.SetActive(false);  // Karakter butonlarını GİZLE
        renkPaneli.SetActive(true);    // Renk butonlarını AÇ
        aksesuarPaneli.SetActive(false);  // Aksesuar butonlarını GİZLE
    }

    // Aksesuar Sekmesine basılınca çağrılacak
    public void AksesuarSekmesiniAc()
    {
        karakterPaneli.SetActive(false);  // Karakter butonlarını GİZLE
        renkPaneli.SetActive(false);      // Renk butonlarını GİZLE
        aksesuarPaneli.SetActive(true);   // Aksesuar butonlarını AÇ
    }
}