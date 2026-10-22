using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 4 balon kuyruğu: 1 ana (harfli) + solda 3 küçük bekleyen.
/// Doğru cevapta ana sağa küçülerek gider; sıradaki ana olur.
/// </summary>
public class BalonKuyrugu : MonoBehaviour
{
    // Balon sprite yolları (01→04)
    private static readonly string[] BalonYollari =
    {
        "Assets/_Art/Bolum1/Balon/Balon_01.png",
        "Assets/_Art/Bolum1/Balon/Balon_02.png",
        "Assets/_Art/Bolum1/Balon/Balon_03.png",
        "Assets/_Art/Bolum1/Balon/Balon_04.png"
    };

    // Kuyruk: [0] = ana balon, [1..3] = soldaki bekleyenler (sıradaki = [1])
    private readonly List<BalonParca> sira = new List<BalonParca>(4);

    // Ana balonun hedef dünya merkezi
    private Vector3 anaMerkez;

    // Ana balonun tek eksen ölçeği (sprite'a göre)
    private float anaOlcek = 1f;

    // Küçük kuyruk balonu: ekran yüksekliğinin oranı (ana'dan bağımsız)
    private float kuyrukEkranOrani = 0.18f;

    // Soldaki slot merkezleri (sıradaki → en son)
    private Vector3[] kuyrukMerkezleri = new Vector3[3];

    // Sağa kaybolma noktası
    private Vector3 cikisNoktasi;

    // Geçiş süresi
    public float gecisSuresi = 0.55f;

    // Ana balondaki harf rozeti
    public TextMeshPro HarfRozeti
    {
        get
        {
            // Ana yoksa null
            if (sira.Count == 0 || sira[0] == null)
            {
                return null;
            }

            // Ana'nın TMP'si
            return sira[0].harf;
        }
    }

    /// <summary>4 balonu kurar; solda 3 küçük, ortada 1 ana.</summary>
    public void Kur(Camera kamera)
    {
        // Eski sahnedeki tek balonu gizle (çakışmasın)
        EskiBalonuGizle();
        // Listeyi temizle
        sira.Clear();
        // Konumları kameraya göre hesapla
        KonumlariHesapla(kamera);
        // 4 sprite yükle ve üret
        for (int i = 0; i < BalonYollari.Length; i++)
        {
            // Sprite
            Sprite sprite = ArtSpriteYukle(BalonYollari[i]);
            // Parça oluştur
            BalonParca parca = BalonOlustur("Balon_0" + (i + 1), sprite);
            // Listeye ekle
            sira.Add(parca);
        }

        // İlk düzen: [0] ana, [1..3] kuyruk
        YerlesimiUygula();
        // Ana'ya boş harf stili (yönetici yazacak)
        if (sira.Count > 0)
        {
            // Rozeti hazırla
            HarfStiliniKur(sira[0], '?');
            // Harfi şimdilik gizle (ilk tur yazılınca açılır)
            HarfiGoster(sira[0], false);
        }
    }

    /// <summary>Ana balona harf yazar ve gösterir.</summary>
    public void HarfiAyarla(char harf)
    {
        // Ana yoksa çık
        if (sira.Count == 0)
        {
            return;
        }

        // Ana parça
        BalonParca ana = sira[0];
        // Stili koruyarak yaz
        HarfStiliniKur(ana, harf);
        // Görünür yap
        HarfiGoster(ana, true);
        // Parıltıyı ana boyuta göre koy
        PariltiyiGuncelle(ana);
    }

    /// <summary>
    /// Doğru cevap: ana sağa küçülüp kaybolur → kuyruk sonuna küçük gider;
    /// sıradaki balon ana olur ve harfi alır.
    /// </summary>
    public IEnumerator DogruSonrasiGecis(char yeniHarf)
    {
        // En az 2 balon şart
        if (sira.Count < 2)
        {
            // Yine de harfi yaz
            HarfiAyarla(yeniHarf);
            yield break;
        }

        // Çıkan ana
        BalonParca cikan = sira[0];
        // Sıradaki (yeni ana)
        BalonParca gelen = sira[1];
        // Sallanmayı durdur (animasyon bozulmasın)
        for (int i = 0; i < sira.Count; i++)
        {
            SallanmayiAyarla(sira[i], false);
        }

        // Çıkan harfi kapat
        HarfiGoster(cikan, false);
        // Tag / collider ana olmaktan çıksın
        AnaOzellikleriniKaldir(cikan);
        // Hedef: sağa çıkış + küçül
        Vector3 cikanBas = cikan.kok.position;
        Vector3 cikanOlcekBas = cikan.kok.localScale;
        // Gelenin başlangıcı
        Vector3 gelenBas = gelen.kok.position;
        Vector3 gelenOlcekBas = gelen.kok.localScale;
        // Ana hedef ölçek (sprite'a göre merkez hizalı)
        Vector3 anaOlcekV = AnaOlcekVektoru(gelen);
        // Çıkan küçülme hedefi (kuyruk boyuna doğru)
        float cikanK = 1f;
        if (cikan.sprite != null)
        {
            float h = cikan.sprite.bounds.size.y;
            cikanK = h > 0.001f ? (kuyrukHedefYukseklik * 0.55f) / h : 0.3f;
        }

        Vector3 cikanKucukOlcek = new Vector3(cikanK, cikanK, 1f);

        float sure = Mathf.Max(0.1f, gecisSuresi);
        float t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / sure);
            // Ease-in-out
            float e = n * n * (3f - 2f * n);
            // Çıkan: sağa + küçül
            cikan.kok.position = Vector3.Lerp(cikanBas, cikisNoktasi, e);
            cikan.kok.localScale = Vector3.Lerp(cikanOlcekBas, cikanKucukOlcek, e);
            // Gelen: ana merkeze + büyü
            gelen.kok.position = Vector3.Lerp(gelenBas, AnaPivotKonumu(gelen, anaMerkez, anaOlcekV), e);
            gelen.kok.localScale = Vector3.Lerp(gelenOlcekBas, anaOlcekV, e);
            yield return null;
        }

        // Listeyi döndür: çıkan sona
        sira.RemoveAt(0);
        sira.Add(cikan);
        // Bekleyenleri (eski 2,3 + çıkan) sol slotlara diz; ana zaten yerinde
        YerlesimiUygula();
        // Yeni ana özellikleri
        AnaOzellikleriniVer(sira[0]);
        // Harfi yaz
        HarfiAyarla(yeniHarf);
        // Sallanmayı aç
        for (int i = 0; i < sira.Count; i++)
        {
            // Ana daha geniş, küçükler dar sallanır
            BalonSallanma sal = sira[i].sallanma;
            if (sal != null)
            {
                bool anaMi = i == 0;
                sal.yatayGenlik = anaMi ? 0.12f : 0.05f;
                sal.donusGenlik = anaMi ? 3.5f : 2f;
                sal.hiz = anaMi ? 1.15f : 0.9f;
                sal.TabanAyarla(sira[i].kok.position);
                sal.sallanmaAktif = true;
            }
        }
    }

    // Kamera alanına göre ana / kuyruk / çıkış noktaları
    private void KonumlariHesapla(Camera kamera)
    {
        float yukseklik = kamera.orthographicSize * 2f;
        float genislik = yukseklik * kamera.aspect;
        // Referans kare: ana balon büyük; zarfın geniş kısmı (harf yeri) ekranda
        float hedefH = yukseklik * 0.78f;
        // Varsayılan ana ölçek (sprite gelince güncellenir)
        anaOlcek = 1f;
        // Biraz aşağı: harf bölgesi kesilmeden üst-ortada kalsın
        float merkezY = kamera.orthographicSize - hedefH * 0.32f;
        anaMerkez = new Vector3(0f, merkezY, 0f);
        // Küçük kuyruk yüksekliği (ekrana göre sabit)
        float kucukH = yukseklik * kuyrukEkranOrani;
        kuyrukHedefYukseklik = kucukH;
        float aralikX = kucukH * 1.2f;
        // Sol sıra: ana'nın solunda, biraz daha aşağıda (ana ile çakışmasın)
        float kuyrukY = merkezY - hedefH * 0.12f;
        float ilkX = -genislik * 0.28f;
        for (int i = 0; i < 3; i++)
        {
            // i arttıkça daha sola (kuyruk sonu)
            float x = ilkX - i * aralikX;
            kuyrukMerkezleri[i] = new Vector3(x, kuyrukY, 0f);
        }

        // Sağ dışarı çıkış
        cikisNoktasi = new Vector3(genislik * 0.7f, merkezY, 0f);
        // Hedef ana yükseklik sakla (ölçek hesabı için)
        anaHedefYukseklik = hedefH;
    }

    // Ana balon hedef dünya yüksekliği
    private float anaHedefYukseklik = 3f;

    // Küçük kuyruk balonu hedef yüksekliği
    private float kuyrukHedefYukseklik = 1.2f;

    // Ana + sol kuyruk yerleşimini uygula
    private void YerlesimiUygula()
    {
        if (sira.Count == 0)
        {
            return;
        }

        // Ana
        BalonParca ana = sira[0];
        Vector3 anaOlcekV = AnaOlcekVektoru(ana);
        ana.kok.localScale = anaOlcekV;
        ana.kok.position = AnaPivotKonumu(ana, anaMerkez, anaOlcekV);
        AnaOzellikleriniVer(ana);
        // Bekleyenler
        for (int i = 1; i < sira.Count; i++)
        {
            int slot = i - 1;
            if (slot >= kuyrukMerkezleri.Length)
            {
                break;
            }

            BalonParca b = sira[i];
            float kOlcek = 1f;
            // Sprite'a göre küçük boyut
            if (b.sprite != null)
            {
                float h = b.sprite.bounds.size.y;
                kOlcek = h > 0.001f ? kuyrukHedefYukseklik / h : 1f;
            }

            Vector3 olcekV = new Vector3(kOlcek, kOlcek, 1f);
            b.kok.localScale = olcekV;
            b.kok.position = AnaPivotKonumu(b, kuyrukMerkezleri[slot], olcekV);
            AnaOzellikleriniKaldir(b);
            HarfiGoster(b, false);
            // Küçükler arkada
            if (b.cizici != null)
            {
                b.cizici.sortingOrder = 1;
            }
        }

        // Ana önde
        if (ana.cizici != null)
        {
            ana.cizici.sortingOrder = 3;
        }

        // Sallanma tabanları
        for (int i = 0; i < sira.Count; i++)
        {
            if (sira[i].sallanma != null)
            {
                sira[i].sallanma.TabanAyarla(sira[i].kok.position);
                sira[i].sallanma.sallanmaAktif = true;
            }
        }
    }

    // Sprite merkezini hedefe oturtan pivot konumu
    private Vector3 AnaPivotKonumu(BalonParca b, Vector3 merkez, Vector3 olcek)
    {
        if (b.sprite == null)
        {
            return merkez;
        }

        Vector3 kayma = Vector3.Scale(b.sprite.bounds.center, olcek);
        Vector3 konum = merkez - kayma;
        konum.z = 0f;
        return konum;
    }

    // Ana boyut vektörü
    private Vector3 AnaOlcekVektoru(BalonParca b)
    {
        if (b.sprite == null)
        {
            return Vector3.one * anaOlcek;
        }

        float h = b.sprite.bounds.size.y;
        anaOlcek = h > 0.001f ? anaHedefYukseklik / h : 1f;
        return new Vector3(anaOlcek, anaOlcek, 1f);
    }

    // Tek balon GameObject'i
    private BalonParca BalonOlustur(string ad, Sprite sprite)
    {
        GameObject go = new GameObject(ad);
        go.transform.SetParent(transform, true);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;
        // Şekil collider (ana olunca açılır)
        PolygonCollider2D poly = go.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.enabled = false;
        if (sprite != null)
        {
            ColliderSekliniKur(poly, sprite);
        }

        // Harf çocuğu
        GameObject yazi = new GameObject("HarfRozeti");
        yazi.transform.SetParent(go.transform, false);
        TextMeshPro tmp = yazi.AddComponent<TextMeshPro>();
        tmp.text = "";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.extraPadding = true;
        tmp.color = Color.white;
        tmp.enableVertexGradient = true;
        tmp.colorGradient = new VertexGradient(
            new Color32(230, 40, 40, 255),
            new Color32(255, 145, 0, 255),
            new Color32(20, 170, 75, 255),
            new Color32(145, 45, 210, 255));
        tmp.fontSize = 8f;
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
        {
            tmp.font = font;
        }

        yazi.SetActive(false);
        // Sallanma
        BalonSallanma sal = go.AddComponent<BalonSallanma>();
        sal.yatayGenlik = 0.05f;
        sal.donusGenlik = 2f;
        sal.hiz = 0.9f;
        // Parça kaydı
        BalonParca p = new BalonParca
        {
            kok = go.transform,
            cizici = sr,
            collider = poly,
            harf = tmp,
            sallanma = sal,
            sprite = sprite
        };
        return p;
    }

    // Ana: tag + collider
    private void AnaOzellikleriniVer(BalonParca b)
    {
        b.kok.gameObject.tag = "Balon";
        b.kok.gameObject.name = "Balon";
        if (b.collider != null)
        {
            if (b.sprite != null)
            {
                ColliderSekliniKur(b.collider, b.sprite);
            }

            b.collider.enabled = true;
        }

        if (b.cizici != null)
        {
            b.cizici.sortingOrder = 3;
        }
    }

    // Bekleyen: tag yok, collider kapalı
    private void AnaOzellikleriniKaldir(BalonParca b)
    {
        b.kok.gameObject.tag = "Untagged";
        // İsim sprite adına dönsün
        if (b.sprite != null)
        {
            b.kok.gameObject.name = b.sprite.name;
        }

        if (b.collider != null)
        {
            b.collider.enabled = false;
        }

        if (b.cizici != null)
        {
            b.cizici.sortingOrder = 1;
        }
    }

    // Harf göster/gizle
    private void HarfiGoster(BalonParca b, bool goster)
    {
        if (b.harf == null)
        {
            return;
        }

        b.harf.gameObject.SetActive(goster);
        // Parıltı varsa
        Transform parilti = b.kok.Find("HarfPariltisi");
        if (parilti != null)
        {
            parilti.gameObject.SetActive(goster);
        }
    }

    // Harf stili — referans: zarfın geniş üst gövdesinde büyük, renkli, tam görünür
    private void HarfStiliniKur(BalonParca b, char harf)
    {
        if (b.harf == null || b.cizici == null)
        {
            return;
        }

        TextMeshPro tmp = b.harf;
        tmp.text = harf.ToString();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.extraPadding = true;

        // Gökkuşağı balon (Balon_01): sarı/yeşil şeritlerde harf kaybolmasın → tam yeşil
        bool gokkusagiBalon = b.sprite != null && b.sprite.name.Contains("Balon_01");
        if (gokkusagiBalon)
        {
            // Gradient kapalı, düz yeşil
            tmp.enableVertexGradient = false;
            tmp.color = new Color32(20, 200, 55, 255);
        }
        else
        {
            // Diğer balonlar: turuncu → yeşil geçiş
            tmp.color = Color.white;
            tmp.enableVertexGradient = true;
            tmp.colorGradient = new VertexGradient(
                new Color32(255, 140, 30, 255),
                new Color32(255, 200, 40, 255),
                new Color32(40, 190, 70, 255),
                new Color32(30, 150, 90, 255));
        }

        if (tmp.fontMaterial != null)
        {
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.35f);
        }

        // Koyu kontur okunurluğu artırır
        tmp.outlineWidth = gokkusagiBalon ? 0.55f : 0.45f;
        tmp.outlineColor = new Color(0.05f, 0.04f, 0.1f, 1f);
        tmp.rectTransform.sizeDelta = new Vector2(10f, 10f);

        // Ebeveyn ölçeğini nötrle
        float ebeveyn = Mathf.Max(0.001f, b.kok.lossyScale.y);
        tmp.transform.localScale = Vector3.one / ebeveyn;

        Bounds sinir = b.cizici.bounds;
        // Referans: harf zarfın geniş kısmında (sepet üstü, sivri uç altı)
        // Sprite altında sepet ~%0–34, zarf gövdesi ~%38–85
        float ortaY = sinir.min.y + sinir.size.y * 0.58f;
        float ortaX = sinir.center.x;

        // Harf boyutu: balon genişliğinin ~%36'sı (referanstaki O gibi)
        float hedef = sinir.size.x * 0.36f;

        // Kamerada tam görünsün: üst/alt taşmayı önle
        Camera kamera = Camera.main;
        if (kamera != null)
        {
            float kenar = kamera.orthographicSize * 0.04f;
            float gorunenUst = kamera.transform.position.y + kamera.orthographicSize - kenar;
            float gorunenAlt = kamera.transform.position.y - kamera.orthographicSize + kenar;
            // Merkezi görünür alana çek
            float yariTahmini = hedef * 0.55f;
            ortaY = Mathf.Clamp(ortaY, gorunenAlt + yariTahmini, gorunenUst - yariTahmini);
            // Hâlâ sığmıyorsa küçült
            float maxYari = Mathf.Min(ortaY - gorunenAlt, gorunenUst - ortaY) * 0.92f;
            if (maxYari > 0.05f)
            {
                hedef = Mathf.Min(hedef, maxYari * 1.7f);
            }
        }

        // Zarf bandına da sığdır (sepet üzerine binmesin)
        float zarfAlt = sinir.min.y + sinir.size.y * 0.40f;
        float zarfUst = sinir.min.y + sinir.size.y * 0.82f;
        float zarfYari = (zarfUst - zarfAlt) * 0.42f;
        hedef = Mathf.Min(hedef, zarfYari * 1.75f);
        ortaY = Mathf.Clamp(ortaY, zarfAlt + hedef * 0.5f, zarfUst - hedef * 0.5f);

        tmp.transform.position = new Vector3(ortaX, ortaY, 0f);
        tmp.ForceMeshUpdate();

        float mevcut = tmp.textBounds.size.y * tmp.transform.lossyScale.y;
        if (mevcut < 0.001f)
        {
            mevcut = tmp.fontSize * 0.1f;
        }

        if (mevcut > 0.001f && hedef > 0.001f)
        {
            tmp.transform.localScale *= hedef / mevcut;
        }

        tmp.ForceMeshUpdate();
        tmp.transform.position = new Vector3(ortaX, ortaY, 0f);

        // Son kontrol: ekran veya zarf dışına taşan kenarı içeri al
        Bounds hb = tmp.textBounds;
        float hy = hb.extents.y * tmp.transform.lossyScale.y;
        float hx = hb.extents.x * tmp.transform.lossyScale.x;
        float maxYariX = sinir.size.x * 0.22f;
        if (hx > maxYariX && hx > 0.01f)
        {
            tmp.transform.localScale *= maxYariX / hx;
            tmp.ForceMeshUpdate();
            hy = tmp.textBounds.extents.y * tmp.transform.lossyScale.y;
        }

        if (kamera != null)
        {
            float kenar = kamera.orthographicSize * 0.03f;
            float gorunenUst = kamera.transform.position.y + kamera.orthographicSize - kenar;
            float gorunenAlt = kamera.transform.position.y - kamera.orthographicSize + kenar;
            if (ortaY + hy > gorunenUst)
            {
                ortaY = gorunenUst - hy;
            }

            if (ortaY - hy < gorunenAlt)
            {
                ortaY = gorunenAlt + hy;
            }
        }

        // Sepete değmesin
        float minOrta = sinir.min.y + sinir.size.y * 0.40f + hy;
        if (ortaY < minOrta)
        {
            ortaY = minOrta;
        }

        tmp.transform.position = new Vector3(ortaX, ortaY, 0f);

        MeshRenderer ag = tmp.GetComponent<MeshRenderer>();
        if (ag != null)
        {
            ag.sortingOrder = 12;
        }

        PariltiyiGuncelle(b);
    }

    // Üretilmiş parıltı sprite (bir kez)
    private static Sprite pariltiSprite;

    // Harfin arkasına ışıltı + nabız koyar
    private void PariltiyiGuncelle(BalonParca b)
    {
        if (b.harf == null || !b.harf.gameObject.activeSelf)
        {
            return;
        }

        Transform eski = b.kok.Find("HarfPariltisi");
        GameObject go = eski != null ? eski.gameObject : new GameObject("HarfPariltisi");
        if (eski == null)
        {
            go.transform.SetParent(b.kok, false);
        }

        go.SetActive(true);
        SpriteRenderer pc = go.GetComponent<SpriteRenderer>();
        if (pc == null)
        {
            pc = go.AddComponent<SpriteRenderer>();
        }

        // Yıldızlı hale sprite
        pc.sprite = PariltiSpriteAl();
        // Daha güçlü altın parıltı
        pc.color = new Color(1f, 0.95f, 0.65f, 0.88f);
        // Harfin arkasında, balonun üstünde
        pc.sortingOrder = 11;

        // Harfle aynı nokta
        Vector3 harfPos = b.harf.transform.position;
        go.transform.position = new Vector3(harfPos.x, harfPos.y, 0f);

        float ebeveyn = Mathf.Max(0.001f, b.kok.lossyScale.y);
        float harfBoyu = b.harf.textBounds.size.y * b.harf.transform.lossyScale.y;
        if (harfBoyu < 0.05f)
        {
            harfBoyu = 1f;
        }

        // Daha büyük hale (parıltı artsın)
        Vector3 olcek = Vector3.one * ((harfBoyu * 2.65f) / ebeveyn);
        go.transform.localScale = olcek;

        // Nabız animasyonu
        HarfPariltisi nabiz = go.GetComponent<HarfPariltisi>();
        if (nabiz == null)
        {
            nabiz = go.AddComponent<HarfPariltisi>();
        }

        nabiz.TabanOlcegiKaydet(olcek);
    }

    // Yumuşak hale + yıldız sprite üretir
    private static Sprite PariltiSpriteAl()
    {
        if (pariltiSprite != null)
        {
            return pariltiSprite;
        }

        const int n = 128;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[n * n];
        float orta = (n - 1) * 0.5f;
        float yaricap = n * 0.48f;
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x - orta) / yaricap;
                float dy = (y - orta) / yaricap;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float hale = Mathf.Clamp01(1f - d);
                hale *= hale;
                float ax = Mathf.Abs(dx);
                float ay = Mathf.Abs(dy);
                float arti = Mathf.Clamp01(1f - Mathf.Min(ax, ay) * 9f) * Mathf.Clamp01(1f - Mathf.Max(ax, ay) * 1.05f);
                float capraz = Mathf.Clamp01(1f - Mathf.Abs(ax - ay) * 8f) * Mathf.Clamp01(1f - d * 1.15f);
                float yildiz = Mathf.Max(arti, capraz * 0.8f);
                float a = Mathf.Clamp01(hale * 0.75f + yildiz);
                if (d > 1f && yildiz < 0.02f)
                {
                    a = 0f;
                }

                Color renk = Color.Lerp(new Color(1f, 0.82f, 0.28f, 1f), Color.white, Mathf.Clamp01(yildiz + hale * 0.35f));
                renk.a = a;
                px[y * n + x] = renk;
            }
        }

        // Küçük yıldız noktaları
        int[] yx = { 22, 40, 58, 76, 94, 110, 30, 48, 66, 84, 102, 18, 108, 36, 64, 92 };
        int[] yy = { 102, 112, 96, 110, 100, 90, 78, 86, 74, 84, 70, 52, 48, 34, 24, 30 };
        for (int s = 0; s < yx.Length; s++)
        {
            int cx = yx[s];
            int cy = yy[s];
            int cap = 4 + (s % 3);
            for (int y = cy - cap; y <= cy + cap; y++)
            {
                for (int x = cx - cap; x <= cx + cap; x++)
                {
                    if (x < 0 || y < 0 || x >= n || y >= n)
                    {
                        continue;
                    }

                    float dx = (x - cx) / (float)cap;
                    float dy = (y - cy) / (float)cap;
                    float ax = Mathf.Abs(dx);
                    float ay = Mathf.Abs(dy);
                    float arti = Mathf.Clamp01(1f - Mathf.Min(ax, ay) * 4.5f) * Mathf.Clamp01(1f - Mathf.Max(ax, ay));
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Max(arti, Mathf.Clamp01(1f - d * 2.4f));
                    if (a <= 0.02f)
                    {
                        continue;
                    }

                    int i = y * n + x;
                    Color once = px[i];
                    Color yildiz = new Color(1f, 0.98f, 0.86f, 1f);
                    Color sonuc = Color.Lerp(once, yildiz, a);
                    sonuc.a = Mathf.Max(once.a, a);
                    px[i] = sonuc;
                }
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        pariltiSprite = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), n);
        return pariltiSprite;
    }

    private void SallanmayiAyarla(BalonParca b, bool acik)
    {
        if (b.sallanma != null)
        {
            b.sallanma.sallanmaAktif = acik;
            if (acik)
            {
                b.sallanma.TabanAyarla(b.kok.position);
            }
        }
    }

    private void ColliderSekliniKur(PolygonCollider2D cokgen, Sprite sprite)
    {
        int adet = sprite.GetPhysicsShapeCount();
        if (adet <= 0)
        {
            cokgen.enabled = false;
            return;
        }

        cokgen.pathCount = adet;
        List<Vector2> yol = new List<Vector2>();
        for (int i = 0; i < adet; i++)
        {
            yol.Clear();
            sprite.GetPhysicsShape(i, yol);
            cokgen.SetPath(i, yol);
        }
    }

    private void EskiBalonuGizle()
    {
        GameObject tagli = GameObject.FindGameObjectWithTag("Balon");
        if (tagli != null && tagli.transform.parent != transform)
        {
            tagli.SetActive(false);
        }

        // İsimle kalanlar
        SpriteRenderer[] hepsi = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        for (int i = 0; i < hepsi.Length; i++)
        {
            string ad = hepsi[i].gameObject.name;
            if ((ad == "Balon" || ad.StartsWith("Balon_")) && hepsi[i].transform.parent != transform)
            {
                // Bizim kuyruk dışındakileri kapat
                if (hepsi[i].transform.root != transform)
                {
                    hepsi[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private Sprite ArtSpriteYukle(string yol)
    {
#if UNITY_EDITOR
        Object[] hepsi = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(yol);
        if (hepsi == null)
        {
            return null;
        }

        for (int i = 0; i < hepsi.Length; i++)
        {
            Sprite s = hepsi[i] as Sprite;
            if (s != null)
            {
                return s;
            }
        }
#endif
        return null;
    }

    // Tek balon verisi
    private class BalonParca
    {
        public Transform kok;
        public SpriteRenderer cizici;
        public PolygonCollider2D collider;
        public TextMeshPro harf;
        public BalonSallanma sallanma;
        public Sprite sprite;
    }
}
