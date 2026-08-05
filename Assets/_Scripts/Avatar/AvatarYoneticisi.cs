using UnityEngine;
// Abdullah Aslan test yaptı.
/// <summary>
/// Oyuncunun seçtiği hayvan, renk ve aksesuarı tutar.
/// Sahne değişse bile silinmez; kalıcı seçimler PlayerPrefs ile kaydedilir.
/// </summary>
[DefaultExecutionOrder(-100)]
public class AvatarYoneticisi : MonoBehaviour
{
    // Tek kopyaya her yerden erişim için Singleton
    public static AvatarYoneticisi Instance;

    // PlayerPrefs anahtarları
    private const string PrefHayvan = "Avatar_Hayvan";
    private const string PrefRenkR = "Avatar_Renk_R";
    private const string PrefRenkG = "Avatar_Renk_G";
    private const string PrefRenkB = "Avatar_Renk_B";
    private const string PrefRenkA = "Avatar_Renk_A";
    private const string PrefAksesuar = "Avatar_Aksesuar";

    // 0=Kedi, 1=Tavşan, 2=Köpek, 3=Rakun
    public int seciliHayvanIndex = 0;

    // Gövdeye uygulanacak renk (varsayılan beyaz = orijinal renk)
    public Color seciliRenk = Color.white;

    // -1 = aksesuar yok
    public int seciliAksesuarIndex = -1;

    // 0=normal, 1=mutlu, 2=üzgün — geçici; PlayerPrefs'e yazılmaz
    public int seciliIfadeIndex = 0;

    private void Awake()
    {
        // Zaten bir kopya varsa bu yenisini yok et
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Tek kopyayı ayarla
        Instance = this;

        // Sahne değişince bu objeyi silme
        DontDestroyOnLoad(gameObject);

        // Kayıtlı seçimleri yükle
        KayitlariYukle();
    }

    /// <summary>
    /// Inspector'dan alan değiştirilince (Play içinde dahil) görünümü yeniler.
    /// </summary>
    private void OnValidate()
    {
        // Editörde değer değişince de ekranı güncelle
        GorunumuYenile();
    }

    /// <summary>Hayvan seçer ve cihaza kaydeder.</summary>
    public void HayvanSec(int index)
    {
        seciliHayvanIndex = index;
        PlayerPrefs.SetInt(PrefHayvan, seciliHayvanIndex);
        PlayerPrefs.Save();
        GorunumuYenile();
    }

    /// <summary>Renk seçer ve cihaza kaydeder.</summary>
    public void RenkSec(Color renk)
    {
        seciliRenk = renk;
        PlayerPrefs.SetFloat(PrefRenkR, seciliRenk.r);
        PlayerPrefs.SetFloat(PrefRenkG, seciliRenk.g);
        PlayerPrefs.SetFloat(PrefRenkB, seciliRenk.b);
        PlayerPrefs.SetFloat(PrefRenkA, seciliRenk.a);
        PlayerPrefs.Save();
        GorunumuYenile();
    }

    /// <summary>Aksesuar seçer (-1 = yok) ve cihaza kaydeder.</summary>
    public void AksesuarSec(int index)
    {
        seciliAksesuarIndex = index;
        PlayerPrefs.SetInt(PrefAksesuar, seciliAksesuarIndex);
        PlayerPrefs.Save();
        GorunumuYenile();
    }

    /// <summary>
    /// İfadeyi değiştirir (0=normal, 1=mutlu, 2=üzgün).
    /// Geçici olduğu için PlayerPrefs'e yazılmaz.
    /// </summary>
    public void IfadeSec(int index)
    {
        // 0–2 aralığına sabitle
        seciliIfadeIndex = Mathf.Clamp(index, 0, 2);
        GorunumuYenile();
    }

    /// <summary>Sahnedeki tüm AvatarGorunumu bileşenlerini yeniler.</summary>
    private void GorunumuYenile()
    {
        // Oyun çalışmıyorsa Find güvenli değil / gerek yok
        if (!Application.isPlaying)
            return;

        AvatarGorunumu[] gorunumler = Object.FindObjectsByType<AvatarGorunumu>(FindObjectsSortMode.None);
        for (int i = 0; i < gorunumler.Length; i++)
        {
            if (gorunumler[i] != null)
                gorunumler[i].Guncelle();
        }
    }

    /// <summary>PlayerPrefs'ten kayıtlı seçimleri okur.</summary>
    private void KayitlariYukle()
    {
        seciliHayvanIndex = PlayerPrefs.GetInt(PrefHayvan, 0);

        // Renk kaydı yoksa beyaz kalsın
        if (PlayerPrefs.HasKey(PrefRenkR))
        {
            seciliRenk = new Color(
                PlayerPrefs.GetFloat(PrefRenkR, 1f),
                PlayerPrefs.GetFloat(PrefRenkG, 1f),
                PlayerPrefs.GetFloat(PrefRenkB, 1f),
                PlayerPrefs.GetFloat(PrefRenkA, 1f)
            );
        }

        seciliAksesuarIndex = PlayerPrefs.GetInt(PrefAksesuar, -1);

        // İfade her açılışta normal başlar
        seciliIfadeIndex = 0;
    }
}
