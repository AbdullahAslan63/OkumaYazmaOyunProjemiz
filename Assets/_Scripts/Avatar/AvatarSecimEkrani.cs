using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Avatar seçim ekranı: sol sekmeler (karakter / renk / aksesuar) ve Devam.
/// </summary>
public class AvatarSecimEkrani : MonoBehaviour
{
    // Görünümü yenilemek için referans
    public AvatarGorunumu avatarGorunumu;

    // Devam butonu hedef sahnesi
    public string sonrakiSahneAdi = "HarfSecmeOyunu";

    // Sol paneldeki içerik panelleri (sekme ile aç/kapa)
    public GameObject karakterPaneli;
    public GameObject renkPaneli;
    public GameObject aksesuarPaneli;

    // Üst sekme butonları (seçili rengi güncellemek için)
    public Button karakterSekmeButon;
    public Button renkSekmeButon;
    public Button aksesuarSekmeButon;

    // Play'de UI bir kez kurulunca tekrar yıkılmasın
    [System.NonSerialized] public bool playDuzeniKuruldu;

    // Hayvan sayısı (Kedi, Tavşan, Kuş, Rakun)
    private const int HayvanSayisi = 4;

    // Seçili sekme rengi
    private static readonly Color SekmeAcik = new Color(0.22f, 0.22f, 0.25f, 1f);
    private static readonly Color SekmeKapali = new Color(0.93f, 0.93f, 0.95f, 1f);

    private void Awake()
    {
        AvatarSahneKurucu.MevcutCanvasaDolapUygula(this);
    }

    private void Start()
    {
        // Awake atlanırsa (scene reload kapalı vb.) eski oklar kalmasın
        AvatarSahneKurucu.MevcutCanvasaDolapUygula(this);
    }

    /// <summary>Sıradaki hayvana geçer (3'ten sonra 0).</summary>
    public void SonrakiHayvan()
    {
        if (AvatarYoneticisi.Instance == null) return;

        int yeni = (AvatarYoneticisi.Instance.seciliHayvanIndex + 1) % HayvanSayisi;
        AvatarYoneticisi.Instance.HayvanSec(yeni);
        GorunumuYenile();
    }

    /// <summary>Önceki hayvana geçer (0'dan önce 3).</summary>
    public void OncekiHayvan()
    {
        if (AvatarYoneticisi.Instance == null) return;

        int yeni = AvatarYoneticisi.Instance.seciliHayvanIndex - 1;
        if (yeni < 0) yeni = HayvanSayisi - 1;
        AvatarYoneticisi.Instance.HayvanSec(yeni);
        GorunumuYenile();
    }

    /// <summary>Verilen indeksteki hayvanı seçer.</summary>
    public void HayvanSec(int index)
    {
        if (AvatarYoneticisi.Instance == null) return;
        AvatarYoneticisi.Instance.HayvanSec(index);
        GorunumuYenile();
    }

    /// <summary>Sol panelde karakter listesini açar.</summary>
    public void KarakterSekmesiniAc()
    {
        SekmeGoster(0);
    }

    /// <summary>Sol panelde renk listesini açar.</summary>
    public void RenkSekmesiniAc()
    {
        SekmeGoster(1);
    }

    /// <summary>Sol panelde aksesuar listesini açar.</summary>
    public void AksesuarSekmesiniAc()
    {
        SekmeGoster(2);
    }

    /// <summary>Seçimi kaydedip sonraki sahneye geçer.</summary>
    public void DevamEt()
    {
        if (string.IsNullOrEmpty(sonrakiSahneAdi))
        {
            Debug.LogWarning("AvatarSecimEkrani: sonrakiSahneAdi boş.");
            return;
        }
        SceneManager.LoadScene(sonrakiSahneAdi);
    }

    /// <summary>0=karakter, 1=renk, 2=aksesuar panelini gösterir.</summary>
    private void SekmeGoster(int sekme)
    {
        if (karakterPaneli != null) karakterPaneli.SetActive(sekme == 0);
        if (renkPaneli != null) renkPaneli.SetActive(sekme == 1);
        if (aksesuarPaneli != null) aksesuarPaneli.SetActive(sekme == 2);

        SekmeRenginiAyarla(karakterSekmeButon, sekme == 0);
        SekmeRenginiAyarla(renkSekmeButon, sekme == 1);
        SekmeRenginiAyarla(aksesuarSekmeButon, sekme == 2);
    }

    /// <summary>Sekme butonunun açık/kapalı rengini ve yazı rengini ayarlar.</summary>
    private static void SekmeRenginiAyarla(Button buton, bool acik)
    {
        if (buton == null) return;

        Image img = buton.GetComponent<Image>();
        if (img != null)
            img.color = acik ? SekmeAcik : SekmeKapali;

        Text yazi = buton.GetComponentInChildren<Text>();
        if (yazi != null)
            yazi.color = acik ? Color.white : new Color(0.18f, 0.18f, 0.2f, 1f);
    }

    // AvatarGorunumu varsa Guncelle çağır
    private void GorunumuYenile()
    {
        if (avatarGorunumu != null)
            avatarGorunumu.Guncelle();
    }
}
