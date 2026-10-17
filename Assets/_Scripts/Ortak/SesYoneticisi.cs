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
    // Doğru cevap kısa efekti (obje adı sesinin yanında)
    public AudioClip dogruSes;
    // Yanlış cevap
    public AudioClip yanlisSes;
    // Oyun başı soru cümlesi ("Hangisinin baş harfi")
    public AudioClip soruPromptSesi;

    [Header("Harf sesleri (soru çıkınca)")]
    // İsteğe bağlı; Inspector'dan harf adına göre doldurulur
    public AudioClip[] harfSesleri;

    [Header("Obje adı sesleri")]
    // sprite.name ile eşleşir (örn. elma; elma_0 de kabul)
    public AudioClip[] objeSesleri;

    // Oyun sahnelerinde hayvan müziği yerine oyun şarkısı
    private static readonly HashSet<string> OyunSahneAdlari = new HashSet<string>
    {
        "HarfSecmeOyunu",
        "SepetOyunu"
    };

    // Obje clip sözlüğü (isim → clip)
    private Dictionary<string, AudioClip> objeSozlugu;

    /// <summary>Sahnedeki / Instance kopyasını döner; yoksa oluşturur (clip'ler prefab / Editor kurulumunda).</summary>
    public static SesYoneticisi GarantiEt()
    {
        if (Instance != null) return Instance;

        // Sahneye yerleştirilmiş prefab Awake olmadan da bulunabilsin
        SesYoneticisi sahnede = Object.FindAnyObjectByType<SesYoneticisi>();
        if (sahnede != null)
        {
            Instance = sahnede;
            return sahnede;
        }

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

    /// <summary>Doğru cevap kısa SFX (obje adı sesinin yerine geçmez).</summary>
    public void DogruSesiCal()
    {
        EfektCal(dogruSes);
    }

    /// <summary>Yanlış cevap sesi.</summary>
    public void YanlisSesiCal()
    {
        EfektCal(yanlisSes);
    }

    /// <summary>
    /// "Hangisinin baş harfi" soru cümlesini bir kez çalar.
    /// Dönüş: clip süresi (yoksa 0).
    /// </summary>
    public float SoruPromptSesiCal()
    {
        EfektCal(soruPromptSesi);
        return soruPromptSesi != null ? soruPromptSesi.length : 0f;
    }

    /// <summary>Soru harfinin adını bir kez seslendirir; süre döner.</summary>
    public float HarfSesiCal(char harf)
    {
        AudioClip clip = HarfClipiniBul(harf);
        EfektCal(clip);
        return clip != null ? clip.length : 0f;
    }

    /// <summary>Doğru objenin adını sprite ismine göre seslendirir; süre döner.</summary>
    public float ObjeSesiCal(Sprite resim)
    {
        AudioClip clip = ObjeClipiniBul(resim);
        EfektCal(clip);
        return clip != null ? clip.length : 0f;
    }

    /// <summary>Sprite adına uyan obje klibini bulur (elma_0 → elma).</summary>
    public AudioClip ObjeClipiniBul(Sprite resim)
    {
        if (resim == null) return null;
        if (objeSozlugu == null || objeSozlugu.Count == 0)
            ObjeSozlugunuKur();
        if (objeSozlugu == null || objeSozlugu.Count == 0) return null;

        string anahtar = IsmiNormalizeEt(resim.name);
        while (!string.IsNullOrEmpty(anahtar))
        {
            AudioClip clip;
            if (objeSozlugu.TryGetValue(anahtar, out clip))
                return clip;

            // elma_0 / isik,_1 gibi sonekleri soy
            string kisaltilmis = SpriteSonekiniSoy(anahtar);
            if (kisaltilmis == anahtar)
                break;
            anahtar = kisaltilmis;
        }

        return null;
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

    /// <summary>Karşılaştırma için küçük harf + trim + Multiple sprite sonek temizliği.</summary>
    static string IsmiNormalizeEt(string ad)
    {
        if (string.IsNullOrEmpty(ad)) return string.Empty;
        // Unity "(Clone)" ekini temizle
        int parantez = ad.IndexOf('(');
        if (parantez > 0)
            ad = ad.Substring(0, parantez);
        ad = ad.Trim().ToLowerInvariant();
        // "isik," gibi sondaki noktalama
        ad = ad.TrimEnd(',', '.', '!', '?', ' ', '_');
        return ad;
    }

    /// <summary>elma_0 → elma; isik,_1 → isik,</summary>
    static string SpriteSonekiniSoy(string anahtar)
    {
        if (string.IsNullOrEmpty(anahtar)) return anahtar;

        // Sondaki _digit veya ,digit
        int i = anahtar.Length - 1;
        if (i < 0 || !char.IsDigit(anahtar[i]))
            return anahtar.TrimEnd(',', '.', '!', '?', ' ', '_');

        while (i >= 0 && char.IsDigit(anahtar[i]))
            i--;
        if (i >= 0 && (anahtar[i] == '_' || anahtar[i] == ','))
            return anahtar.Substring(0, i).TrimEnd(',', '.', '!', '?', ' ', '_');

        return anahtar;
    }
}
