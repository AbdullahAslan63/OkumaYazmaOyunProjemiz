using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fon müzik + SFX yöneticisi. Sahne değişince silinmez (DDOL).
/// Clip veya kaynak yoksa sessiz geçer; hata üretmez.
/// </summary>
[DefaultExecutionOrder(-90)]
public class SesYoneticisi : MonoBehaviour
{
    // Tek kopya erişimi
    public static SesYoneticisi Instance;

    [Header("Kaynaklar")]
    // Loop eden fon müzik
    public AudioSource muzikKaynagi;
    // OneShot efektler (tıklama, yanlış, harf, obje)
    public AudioSource efektKaynagi;

    [Header("Menü müziği — hayvanlara göre")]
    // 0=Kedi, 1=Tavşan, 2=Kuş, 3=Rakun
    public AudioClip[] hayvanMuzikleri = new AudioClip[4];

    [Header("Oyun müziği")]
    // Mini oyunda çalan Sesli Harfler şarkısı
    public AudioClip oyunMuzigi;

    [Header("SFX")]
    // Ortak buton tıklama
    public AudioClip tiklamaSesi;
    // Yanlış cevap
    public AudioClip yanlisSes;

    [Header("Harf sesleri (soru çıkınca)")]
    // İsteğe bağlı; Inspector'dan harf adına göre doldurulur
    public AudioClip[] harfSesleri;

    [Header("Obje adı sesleri")]
    // sprite.name ile eşleşir (örn. elma)
    public AudioClip[] objeSesleri;

    // Oyun sahnelerinde hayvan müziği yerine oyun şarkısı
    private static readonly HashSet<string> OyunSahneAdlari = new HashSet<string>
    {
        "HarfSecmeOyunu",
        "SepetOyunu"
    };

    // Obje clip sözlüğü (isim → clip)
    private Dictionary<string, AudioClip> objeSozlugu;

    /// <summary>Sahnede yoksa boş bir SesYoneticisi oluşturur (clip'ler sonra bağlanır).</summary>
    public static SesYoneticisi GarantiEt()
    {
        if (Instance != null) return Instance;
        GameObject go = new GameObject("SesYoneticisi");
        return go.AddComponent<SesYoneticisi>();
    }

    void Awake()
    {
        // Çift kopyayı yok et
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Kaynaklar yoksa oluştur
        KaynaklariGarantiEt();
        ObjeSozlugunuKur();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += SahneYuklendi;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SahneYuklendi;
    }

    void Start()
    {
        // İlk sahnede doğru müzik modunu aç
        string sahneAdi = SceneManager.GetActiveScene().name;
        SahneMuziginiAyarla(sahneAdi);
        if (!OyunSahneAdlari.Contains(sahneAdi))
            MenüButonlarinaTiklamaEkle();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Eksik AudioSource'ları ekler.</summary>
    void KaynaklariGarantiEt()
    {
        if (muzikKaynagi == null)
        {
            muzikKaynagi = gameObject.AddComponent<AudioSource>();
            muzikKaynagi.playOnAwake = false;
            muzikKaynagi.loop = true;
        }
        else
        {
            muzikKaynagi.loop = true;
            muzikKaynagi.playOnAwake = false;
        }

        if (efektKaynagi == null)
        {
            efektKaynagi = gameObject.AddComponent<AudioSource>();
            efektKaynagi.playOnAwake = false;
            efektKaynagi.loop = false;
        }
    }

    /// <summary>Obje clip dizisinden isim sözlüğü kurar.</summary>
    void ObjeSozlugunuKur()
    {
        objeSozlugu = new Dictionary<string, AudioClip>();
        if (objeSesleri == null) return;

        for (int i = 0; i < objeSesleri.Length; i++)
        {
            AudioClip clip = objeSesleri[i];
            if (clip == null) continue;
            string anahtar = IsmiNormalizeEt(clip.name);
            if (string.IsNullOrEmpty(anahtar)) continue;
            if (!objeSozlugu.ContainsKey(anahtar))
                objeSozlugu.Add(anahtar, clip);
        }
    }

    /// <summary>Sahne yüklenince menü / oyun müziğini seçer.</summary>
    void SahneYuklendi(Scene sahne, LoadSceneMode mod)
    {
        SahneMuziginiAyarla(sahne.name);

        // Level / menü butonlarına tıklama sesi (Inspector şart değil)
        if (!OyunSahneAdlari.Contains(sahne.name))
            MenüButonlarinaTiklamaEkle();
    }

    /// <summary>Sahnedeki Button'lara ButonTiklamaSesi ekler (yoksa).</summary>
    void MenüButonlarinaTiklamaEkle()
    {
        Button[] butonlar = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        for (int i = 0; i < butonlar.Length; i++)
        {
            if (butonlar[i] == null) continue;
            if (butonlar[i].GetComponent<ButonTiklamaSesi>() != null) continue;
            butonlar[i].gameObject.AddComponent<ButonTiklamaSesi>();
        }
    }

    /// <summary>Oyun sahnesinde şarkı, menüde hayvan müziği.</summary>
    public void SahneMuziginiAyarla(string sahneAdi)
    {
        if (OyunSahneAdlari.Contains(sahneAdi))
        {
            OyunMuziginiCal();
            return;
        }

        int index = 0;
        if (AvatarYoneticisi.Instance != null)
            index = AvatarYoneticisi.Instance.seciliHayvanIndex;
        HayvanMuziginiCal(index);
    }

    /// <summary>Ortak buton tıklama sesi.</summary>
    public void TiklamaSesiCal()
    {
        EfektCal(tiklamaSesi);
    }

    /// <summary>Menüde seçili hayvana göre müzik (loop).</summary>
    public void HayvanMuziginiCal(int index)
    {
        if (muzikKaynagi == null) return;
        if (hayvanMuzikleri == null || hayvanMuzikleri.Length == 0) return;

        // Geçersiz index'i sınırla
        if (index < 0 || index >= hayvanMuzikleri.Length)
            index = 0;

        AudioClip clip = hayvanMuzikleri[index];
        if (clip == null) return;

        // Aynı clip zaten çalıyorsa dokunma
        if (muzikKaynagi.clip == clip && muzikKaynagi.isPlaying)
            return;

        muzikKaynagi.clip = clip;
        muzikKaynagi.loop = true;
        muzikKaynagi.Play();
    }

    /// <summary>Oyun içinde Sesli Harfler şarkısını çalar.</summary>
    public void OyunMuziginiCal()
    {
        if (muzikKaynagi == null) return;
        if (oyunMuzigi == null) return;

        if (muzikKaynagi.clip == oyunMuzigi && muzikKaynagi.isPlaying)
            return;

        muzikKaynagi.clip = oyunMuzigi;
        muzikKaynagi.loop = true;
        muzikKaynagi.Play();
    }

    /// <summary>Yanlış cevap sesi.</summary>
    public void YanlisSesiCal()
    {
        EfektCal(yanlisSes);
    }

    /// <summary>Soru harfinin adını bir kez seslendirir.</summary>
    public void HarfSesiCal(char harf)
    {
        AudioClip clip = HarfClipiniBul(harf);
        EfektCal(clip);
    }

    /// <summary>Doğru objenin adını sprite ismine göre seslendirir.</summary>
    public void ObjeSesiCal(Sprite resim)
    {
        if (resim == null) return;
        if (objeSozlugu == null || objeSozlugu.Count == 0)
            ObjeSozlugunuKur();

        string anahtar = IsmiNormalizeEt(resim.name);
        if (string.IsNullOrEmpty(anahtar)) return;

        AudioClip clip;
        if (objeSozlugu != null && objeSozlugu.TryGetValue(anahtar, out clip))
            EfektCal(clip);
    }

    /// <summary>Harf karakterine uyan clip'i bulur (adında harf geçen).</summary>
    AudioClip HarfClipiniBul(char harf)
    {
        if (harfSesleri == null || harfSesleri.Length == 0) return null;

        string hedef = IsmiNormalizeEt(harf.ToString());
        for (int i = 0; i < harfSesleri.Length; i++)
        {
            AudioClip clip = harfSesleri[i];
            if (clip == null) continue;
            string ad = IsmiNormalizeEt(clip.name);
            if (ad == hedef || ad.StartsWith(hedef))
                return clip;
        }

        // Sıralı dizi varsayımı yoksa ilk null olmayanı zorla çalma
        return null;
    }

    /// <summary>efektKaynagi üzerinden OneShot; null ise sessiz.</summary>
    void EfektCal(AudioClip clip)
    {
        if (efektKaynagi == null || clip == null) return;
        efektKaynagi.PlayOneShot(clip);
    }

    /// <summary>Karşılaştırma için küçük harf + trim.</summary>
    static string IsmiNormalizeEt(string ad)
    {
        if (string.IsNullOrEmpty(ad)) return string.Empty;
        // Unity "(Clone)" ekini temizle
        int parantez = ad.IndexOf('(');
        if (parantez > 0)
            ad = ad.Substring(0, parantez);
        return ad.Trim().ToLowerInvariant();
    }
}
