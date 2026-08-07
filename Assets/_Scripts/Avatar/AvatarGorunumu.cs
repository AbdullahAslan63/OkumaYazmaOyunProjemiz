using UnityEngine;

/// <summary>
/// Tek bir hayvanın Inspector'da doldurulan sprite seti.
/// </summary>
[System.Serializable]
public class HayvanGorseli
{
    public string hayvanAdi;
    public Sprite normalPoz;
    public Sprite mutluPoz;
    public Sprite uzgunPoz;
}

/// <summary>
/// AvatarYoneticisi'ndeki seçime göre gövde ve aksesuar sprite'larını gösterir.
/// </summary>
public class AvatarGorunumu : MonoBehaviour
{
    // 4 hayvan: Kedi, Tavşan, Köpek, Rakun — Inspector'dan doldurulur
    public HayvanGorseli[] hayvanlar;

    // Aksesuar index → sprite eşlemesi — Inspector'dan doldurulur
    public Sprite[] aksesuarlar;

    public SpriteRenderer govdeRenderer;
    public SpriteRenderer aksesuarRenderer;

    private void Awake()
    {
        // Inspector boş kaldıysa child isimlerinden otomatik bul
        RendererlariOtomatikBul();
    }

    private void Start()
    {
        // Sahne açılınca seçili görünüme göre çiz
        Guncelle();
    }

    /// <summary>
    /// Aktif seçimlere göre gövde sprite/renk ve aksesuarı yeniler.
    /// Seçim ekranı veya geri bildirimden dışarıdan çağrılabilir.
    /// </summary>
    public void Guncelle()
    {
        RendererlariOtomatikBul();

        // Yönetici yoksa sahnede ara (unutulmuş obje / sıralama sorunları için)
        AvatarYoneticisi yonetici = AvatarYoneticisi.Instance;
        if (yonetici == null)
        {
            // Instance yoksa sahnede herhangi bir yönetici ara
            yonetici = Object.FindAnyObjectByType<AvatarYoneticisi>();
            if (yonetici == null)
            {
                Debug.LogWarning(
                    "AvatarGorunumu: Sahnede AvatarYoneticisi yok. " +
                    "Hierarchy'de boş bir obje oluşturup AvatarYoneticisi scriptini ekle.");
                return;
            }
        }

        if (govdeRenderer == null)
        {
            Debug.LogWarning(
                "AvatarGorunumu: govdeRenderer atanmamış. " +
                "Govde child'ına SpriteRenderer ekle ve bu alana sürükle.");
            return;
        }
        int hayvanIndex = yonetici.seciliHayvanIndex;

        if (hayvanlar == null || hayvanlar.Length == 0)
        {
            Debug.LogWarning(
                "AvatarGorunumu: hayvanlar listesi boş. " +
                "Inspector'da Size = 4 yapıp her hayvana normal/mutlu/üzgün sprite ata. " +
                "PNG'yi Project'te genişletmeden doğrudan Sprite alanına sürükle.");
            return;
        }

        if (hayvanIndex < 0 || hayvanIndex >= hayvanlar.Length)
        {
            Debug.LogWarning(
                $"AvatarGorunumu: Geçersiz hayvan index ({hayvanIndex}). " +
                $"hayvanlar uzunluğu = {hayvanlar.Length}.");
            return;
        }

        HayvanGorseli hayvan = hayvanlar[hayvanIndex];

        // İfadeye göre doğru sprite'ı seç
        Sprite govdeSprite = hayvan.normalPoz;
        switch (yonetici.seciliIfadeIndex)
        {
            case 1:
                govdeSprite = hayvan.mutluPoz != null ? hayvan.mutluPoz : hayvan.normalPoz;
                break;
            case 2:
                govdeSprite = hayvan.uzgunPoz != null ? hayvan.uzgunPoz : hayvan.normalPoz;
                break;
            default:
                govdeSprite = hayvan.normalPoz;
                break;
        }

        if (govdeSprite == null)
        {
            Debug.LogWarning(
                $"AvatarGorunumu: hayvanlar[{hayvanIndex}] ({hayvan.hayvanAdi}) için sprite yok. " +
                "Normal Poz alanına sprite sürüklediğinden emin ol. " +
                "Project'te PNG'nin yanındaki oka tıklayıp altındaki sprite'ı da deneyebilirsin.");
            return;
        }

        govdeRenderer.enabled = true;
        govdeRenderer.sprite = govdeSprite;
        govdeRenderer.color = yonetici.seciliRenk;

        // Aksesuar katmanı
        if (aksesuarRenderer == null)
            return;

        int aksesuarIndex = yonetici.seciliAksesuarIndex;
        bool aksesuarVar =
            aksesuarIndex >= 0 &&
            aksesuarlar != null &&
            aksesuarIndex < aksesuarlar.Length &&
            aksesuarlar[aksesuarIndex] != null;

        if (aksesuarVar)
        {
            aksesuarRenderer.sprite = aksesuarlar[aksesuarIndex];
            aksesuarRenderer.enabled = true;
        }
        else
        {
            aksesuarRenderer.sprite = null;
            aksesuarRenderer.enabled = false;
        }
    }

    /// <summary>govdeRenderer / aksesuarRenderer boşsa child'lardan bulur.</summary>
    private void RendererlariOtomatikBul()
    {
        if (govdeRenderer == null)
        {
            Transform govde = transform.Find("Govde");
            if (govde != null)
                govdeRenderer = govde.GetComponent<SpriteRenderer>();
        }

        if (aksesuarRenderer == null)
        {
            Transform aksesuar = transform.Find("Aksesuar");
            if (aksesuar != null)
                aksesuarRenderer = aksesuar.GetComponent<SpriteRenderer>();
        }
    }
}
