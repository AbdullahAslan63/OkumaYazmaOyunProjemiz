using UnityEngine;

public class AvatarYoneticisi : MonoBehaviour
{
    // --- SINGLETON (Her Yerden Erişim) ---
    public static AvatarYoneticisi Instance { get; private set; }

    // --- SEÇİLEN BİLGİLER ---
    [Header("Avatar Seçimleri")]
    public int seciliHayvanIndex;      // 0=Kedi, 1=Tavşan, 2=Kuş, 3=Rakun
    public Color seciliRenk = Color.white;
    public int seciliAksesuarIndex = -1; // -1 = Aksesuar yok

    // PlayerPrefs Kayıt Anahtarları
    private const string KEY_HAYVAN = "Avatar_HayvanIndex";
    private const string KEY_AKSESUAR = "Avatar_AksesuarIndex";
    private const string KEY_RENK_R = "Avatar_Renk_R";
    private const string KEY_RENK_G = "Avatar_Renk_G";
    private const string KEY_RENK_B = "Avatar_Renk_B";

    private void Awake()
    {
        // Sahne geçişlerinde nesneyi koruma ve tekil kılma
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            KayitliVerileriYukle();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- SEÇİM VE KAYIT FONKSİYONLARI ---

    public void HayvanSec(int index)
    {
        seciliHayvanIndex = index;
        PlayerPrefs.SetInt(KEY_HAYVAN, seciliHayvanIndex);
        PlayerPrefs.Save();
    }

    public void RenkSec(Color renk)
    {
        seciliRenk = renk;
        PlayerPrefs.SetFloat(KEY_RENK_R, renk.r);
        PlayerPrefs.SetFloat(KEY_RENK_G, renk.g);
        PlayerPrefs.SetFloat(KEY_RENK_B, renk.b);
        PlayerPrefs.Save();
    }

    public void AksesuarSec(int index)
    {
        seciliAksesuarIndex = index;
        PlayerPrefs.SetInt(KEY_AKSESUAR, seciliAksesuarIndex);
        PlayerPrefs.Save();
    }

    // --- VERİ YÜKLEME ---

    private void KayitliVerileriYukle()
    {
        seciliHayvanIndex = PlayerPrefs.GetInt(KEY_HAYVAN, 0);
        seciliAksesuarIndex = PlayerPrefs.GetInt(KEY_AKSESUAR, -1);

        float r = PlayerPrefs.GetFloat(KEY_RENK_R, 1f);
        float g = PlayerPrefs.GetFloat(KEY_RENK_G, 1f);
        float b = PlayerPrefs.GetFloat(KEY_RENK_B, 1f);
        seciliRenk = new Color(r, g, b, 1f);
    }
}