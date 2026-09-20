using UnityEngine;

/// <summary>
/// Bu hayvanda tek bir aksesuarın duracağı yer.
/// Şapka ve gözlük ayrı ayarlanır; birbirini ezmez.
/// </summary>
[System.Serializable]
public class HayvanAksesuarAyari
{
    // Inspector'da hangi aksesuar olduğu görünsün
    public string aksesuarAdi;

    // Bu hayvanda bu aksesuarın yerel konumu (şapka kafa, gözlük yüz)
    public Vector2 ofset;

    // Bu hayvanda bu aksesuarın boyutu (1 = normal, 0.7 küçük, 1.4 büyük)
    public float olcekCarpani = 1f;
}

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

    // Varsayılan kafa noktası — yeni aksesuar satırı eklenince buradan doldurulur
    public Vector2 aksesuarOfset = new Vector2(0f, 1.8f);

    // Her aksesuar için bu hayvana özel konum (aksesuarlar dizisi ile aynı sıra)
    public HayvanAksesuarAyari[] aksesuarAyarlari;
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
        // Her hayvana tüm aksesuar satırlarını ekle (eksikse)
        AksesuarAyarlariniSenkronizeEt();
    }

    /// <summary>
    /// Inspector'da değer değişince aksesuar listesini hayvanlara yazar.
    /// </summary>
    private void OnValidate()
    {
        AksesuarAyarlariniSenkronizeEt();
        if (Application.isPlaying)
            Guncelle();
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

            // Aksesuar gövde ile orantılı kalsın; karakter-aksesuar boyutu ayrı çarpan
            float karakterOlcek = AksesuarOlcekCarpaniniAl(hayvan, aksesuarIndex);
            float aksOlcek = govdeOlcek * aks.olcek * karakterOlcek;
            aksesuarRenderer.transform.localScale = new Vector3(aksOlcek, aksOlcek, 1f);

            // Bu hayvana özel şapka/gözlük konumu (yoksa eski ofset formülü)
            Vector2 pos = AksesuarPozisyonunuAl(hayvan, aks, aksesuarIndex);
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

    /// <summary>
    /// Bu hayvanda bu aksesuarın yerini döndürür.
    /// Karakter-aksesuar satırı varsa onu kullanır; yoksa eski ortak ofset.
    /// </summary>
    private Vector2 AksesuarPozisyonunuAl(HayvanGorseli hayvan, AksesuarGorseli aks, int aksesuarIndex)
    {
        if (hayvan.aksesuarAyarlari != null &&
            aksesuarIndex >= 0 &&
            aksesuarIndex < hayvan.aksesuarAyarlari.Length &&
            hayvan.aksesuarAyarlari[aksesuarIndex] != null)
        {
            return hayvan.aksesuarAyarlari[aksesuarIndex].ofset;
        }

        return hayvan.aksesuarOfset + aks.yerelOfset;
    }

    /// <summary>
    /// Bu hayvanda bu aksesuarın boyut çarpanını döndürür (1 = olduğu gibi).
    /// Eski kayıtlarda 0 gelirse 1 kabul edilir.
    /// </summary>
    private float AksesuarOlcekCarpaniniAl(HayvanGorseli hayvan, int aksesuarIndex)
    {
        if (hayvan.aksesuarAyarlari != null &&
            aksesuarIndex >= 0 &&
            aksesuarIndex < hayvan.aksesuarAyarlari.Length &&
            hayvan.aksesuarAyarlari[aksesuarIndex] != null)
        {
            float carpani = hayvan.aksesuarAyarlari[aksesuarIndex].olcekCarpani;
            if (carpani > 0.001f)
                return carpani;
        }

        return 1f;
    }

    /// <summary>
    /// Her karaktere aksesuarlar listesindeki TÜM aksesuarlar için satır açar.
    /// Şapka ve gözlük yerleri böyle ayrı ayrı düzenlenir. Mevcut ofsetleri silmez.
    /// </summary>
    public void AksesuarAyarlariniSenkronizeEt()
    {
        if (hayvanlar == null || aksesuarlar == null)
            return;

        int aksSayi = aksesuarlar.Length;
        for (int h = 0; h < hayvanlar.Length; h++)
        {
            HayvanGorseli hayvan = hayvanlar[h];
            if (hayvan == null)
                continue;

            HayvanAksesuarAyari[] eski = hayvan.aksesuarAyarlari;
            bool uzunlukAyni = eski != null && eski.Length == aksSayi;
            HayvanAksesuarAyari[] hedef = uzunlukAyni ? eski : new HayvanAksesuarAyari[aksSayi];

            for (int i = 0; i < aksSayi; i++)
            {
                string ad = AksesuarAdiAl(i);
                HayvanAksesuarAyari kayit = (eski != null && i < eski.Length) ? eski[i] : null;

                if (kayit == null)
                    kayit = new HayvanAksesuarAyari();

                kayit.aksesuarAdi = ad;

                // Yeni satırsa mevcut kafa + aksesuar ofsetini varsayılan yap
                bool yeniSatir = eski == null || i >= eski.Length || eski[i] == null;
                if (yeniSatir)
                {
                    Vector2 varsayilan = hayvan.aksesuarOfset;
                    if (aksesuarlar[i] != null)
                        varsayilan += aksesuarlar[i].yerelOfset;
                    kayit.ofset = varsayilan;
                    kayit.olcekCarpani = 1f;
                }

                // Eski satırda boyut alanı 0 kaldıysa (yeni alandı) 1 yap
                if (kayit.olcekCarpani <= 0.001f)
                    kayit.olcekCarpani = 1f;

                hedef[i] = kayit;
            }

            hayvan.aksesuarAyarlari = hedef;
        }
    }

    /// <summary>Aksesuar dizisinden okunabilir ad üretir.</summary>
    private string AksesuarAdiAl(int index)
    {
        if (aksesuarlar == null || index < 0 || index >= aksesuarlar.Length || aksesuarlar[index] == null)
            return "Aksesuar " + index;
        if (aksesuarlar[index].sprite != null)
            return aksesuarlar[index].sprite.name;
        return "Aksesuar " + index;
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
