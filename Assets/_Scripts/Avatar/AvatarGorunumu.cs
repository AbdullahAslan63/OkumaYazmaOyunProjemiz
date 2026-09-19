using UnityEngine;

/// <summary>
/// Tek bir hayvanın Inspector'da doldurulan sprite seti ve ince ayarları.
/// </summary>
[System.Serializable]
public class HayvanGorseli
{
    public string hayvanAdi;
    public Sprite normalPoz;
    public Sprite mutluPoz;
    public Sprite uzgunPoz;

    // Hedef yüksekliğe ek çarpan (1 = olduğu gibi)
    public float govdeOlcekCarpani = 1f;

    // Bu hayvanın kafa bağlama noktası (Avatar yerel birimi)
    public Vector2 aksesuarOfset = new Vector2(0f, 1.8f);
}

/// <summary>
/// Tek bir aksesuarın sprite'ı ile ölçek / konum ayarı.
/// </summary>
[System.Serializable]
public class AksesuarGorseli
{
    public Sprite sprite;

    // Gövde ölçeğine göre çarpan (şapka ~0.45, gözlük ~0.28)
    public float olcek = 0.45f;

    // Hayvan aksesuarOfset'ine eklenen ince kaydırma
    public Vector2 yerelOfset = Vector2.zero;
}

/// <summary>
/// AvatarYoneticisi'ndeki seçime göre gövde ve aksesuar sprite'larını gösterir.
/// Farklı sprite rect'leri hedef yüksekliğe normalize eder.
/// </summary>
public class AvatarGorunumu : MonoBehaviour
{
    // 4 hayvan: Kedi, Tavşan, Kuş, Rakun — Inspector'dan doldurulur
    public HayvanGorseli[] hayvanlar;

    // Aksesuar index → sprite + ölçek/ofset — Inspector'dan doldurulur
    public AksesuarGorseli[] aksesuarlar;

    public SpriteRenderer govdeRenderer;
    public SpriteRenderer aksesuarRenderer;

    // Tüm hayvanlar bu yüksekliğe (dünya birimi) çekilir
    public float hedefGovdeYuksekligi = 4.5f;

    // Gövde alt kenarının hizalanacağı Y (ayaklar UI'dan uzak)
    public float hedefZeminY = -1.5f;

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

        // Farklı rect'leri aynı hedef yüksekliğe çek
        float govdeOlcek = GovdeyiNormalizeEt(govdeSprite, hayvan.govdeOlcekCarpani);

        // Aksesuar katmanı
        if (aksesuarRenderer == null)
            return;

        int aksesuarIndex = yonetici.seciliAksesuarIndex;
        bool aksesuarVar =
            aksesuarIndex >= 0 &&
            aksesuarlar != null &&
            aksesuarIndex < aksesuarlar.Length &&
            aksesuarlar[aksesuarIndex] != null &&
            aksesuarlar[aksesuarIndex].sprite != null;

        if (aksesuarVar)
        {
            AksesuarGorseli aks = aksesuarlar[aksesuarIndex];
            aksesuarRenderer.sprite = aks.sprite;
            aksesuarRenderer.enabled = true;

            // Aksesuar gövde ile orantılı kalsın
            float aksOlcek = govdeOlcek * aks.olcek;
            aksesuarRenderer.transform.localScale = new Vector3(aksOlcek, aksOlcek, 1f);

            // Kafa noktası = hayvan ofseti + aksesuar ince ayarı
            Vector2 pos = hayvan.aksesuarOfset + aks.yerelOfset;
            aksesuarRenderer.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        }
        else
        {
            aksesuarRenderer.sprite = null;
            aksesuarRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Gövdeyi hedef yüksekliğe ölçekler ve alt kenarı hedefZeminY'ye hizalar.
    /// Dönen değer uygulanan yerel ölçek (aksesuar için).
    /// </summary>
    private float GovdeyiNormalizeEt(Sprite govdeSprite, float carpani)
    {
        Transform govdeT = govdeRenderer.transform;

        // Ham sprite yüksekliği (ölçeksiz yerel bounds)
        float hamYukseklik = govdeSprite.bounds.size.y;
        if (hamYukseklik < 0.001f)
            hamYukseklik = 0.001f;

        // Hedef boyuta çek; hayvan çarpanı ince ayar
        float olcek = hedefGovdeYuksekligi / hamYukseklik * carpani;
        govdeT.localScale = new Vector3(olcek, olcek, 1f);

        // Alt kenar hedef zemine gelsin (ayak hizası)
        Vector3 yerelPos = govdeT.localPosition;
        yerelPos.x = 0f;
        yerelPos.y = hedefZeminY - govdeSprite.bounds.min.y * olcek;
        govdeT.localPosition = yerelPos;

        return olcek;
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
