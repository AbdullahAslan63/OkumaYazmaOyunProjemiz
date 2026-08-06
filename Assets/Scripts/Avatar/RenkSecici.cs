using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RenkPaneli'ne eklenir. Panel açılınca renk kutucuklarını oluşturur;
/// birine basınca seçili avatar o renge boyanır.
/// Düzen: 3 sıra — 3 / 3 / 2 renk.
/// </summary>
public class RenkSecici : MonoBehaviour
{
    [System.Serializable]
    public struct RenkSecenegi
    {
        public string ad;
        public Color renk;
    }

    [Header("Renk Kutucukları")]
    public RenkSecenegi[] renkler;

    [Header("Izgara Düzeni (3+3+2)")]
    public float butonBoyutu = 78f;
    public float yatayBosluk = 16f;
    public float dikeyBosluk = 16f;
    public Vector2 gridMerkezi = new Vector2(-280f, 40f);
    public int[] siraAdetleri = { 3, 3, 2 };

    private bool olusturuldu;
    private static Sprite beyazSprite;

    private void OnEnable()
    {
        if (renkler == null || renkler.Length == 0)
            renkler = VarsayilanRenkler();

        // Eski butonları anında sil, 3+3+2 ızgarayı kur
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        olusturuldu = false;
        ButonlariOlustur();
    }

    public void RenkUygula(Color renk)
    {
        renk.a = 1f;

        if (AvatarCustomizer.Instance != null)
        {
            AvatarCustomizer.Instance.SelectColor(renk);
            return;
        }

        if (AvatarYoneticisi.Instance != null)
            AvatarYoneticisi.Instance.RenkSec(renk);
    }

    public void BeyazSec() => RenkUygula(Color.white);
    public void KirmiziSec() => RenkUygula(new Color(0.92f, 0.28f, 0.28f));
    public void MaviSec() => RenkUygula(new Color(0.28f, 0.55f, 0.95f));
    public void YesilSec() => RenkUygula(new Color(0.30f, 0.78f, 0.40f));
    public void SariSec() => RenkUygula(new Color(1f, 0.88f, 0.25f));
    public void TuruncuSec() => RenkUygula(new Color(1f, 0.55f, 0.18f));
    public void PembeSec() => RenkUygula(new Color(1f, 0.55f, 0.75f));
    public void KahveSec() => RenkUygula(new Color(0.62f, 0.40f, 0.22f));

    private void ButonlariOlustur()
    {
        Sprite sprite = BeyazSpriteAl();

        if (siraAdetleri == null || siraAdetleri.Length == 0)
            siraAdetleri = new[] { 3, 3, 2 };

        float satirAdimi = butonBoyutu + dikeyBosluk;
        float sutunAdimi = butonBoyutu + yatayBosluk;
        float toplamYukseklik = siraAdetleri.Length * butonBoyutu + (siraAdetleri.Length - 1) * dikeyBosluk;
        float baslangicY = gridMerkezi.y + toplamYukseklik * 0.5f - butonBoyutu * 0.5f;

        int renkIndex = 0;
        for (int sira = 0; sira < siraAdetleri.Length; sira++)
        {
            int buSirada = siraAdetleri[sira];
            float satirGenislik = buSirada * butonBoyutu + (buSirada - 1) * yatayBosluk;
            float baslangicX = gridMerkezi.x - satirGenislik * 0.5f + butonBoyutu * 0.5f;
            float y = baslangicY - sira * satirAdimi;

            for (int sutun = 0; sutun < buSirada; sutun++)
            {
                if (renkIndex >= renkler.Length)
                    break;

                RenkSecenegi secenek = renkler[renkIndex++];
                Color renk = secenek.renk;
                renk.a = 1f;

                float x = baslangicX + sutun * sutunAdimi;
                RenkButonuOlustur(secenek.ad, renk, sprite, new Vector2(x, y));
            }
        }

        olusturuldu = true;
    }

    private void RenkButonuOlustur(string ad, Color renk, Sprite sprite, Vector2 konum)
    {
        GameObject butonObj = new GameObject($"Renk_{ad}", typeof(RectTransform));
        butonObj.transform.SetParent(transform, false);
        butonObj.layer = gameObject.layer;

        RectTransform rt = butonObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(butonBoyutu, butonBoyutu);
        rt.anchoredPosition = konum;

        Image image = butonObj.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = renk;
        image.raycastTarget = true;

        Outline outline = butonObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.12f, 0.12f, 0.12f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        Button button = butonObj.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        colors.selectedColor = Color.white;
        button.colors = colors;

        Color yakalanan = renk;
        button.onClick.AddListener(() => RenkUygula(yakalanan));
    }

    private static Sprite BeyazSpriteAl()
    {
        if (beyazSprite != null)
            return beyazSprite;

        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] pikseller = new Color[16];
        for (int i = 0; i < pikseller.Length; i++)
            pikseller[i] = Color.white;
        tex.SetPixels(pikseller);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        beyazSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        return beyazSprite;
    }

    private static RenkSecenegi[] VarsayilanRenkler()
    {
        return new[]
        {
            new RenkSecenegi { ad = "Beyaz", renk = Color.white },
            new RenkSecenegi { ad = "Kirmizi", renk = new Color(0.92f, 0.28f, 0.28f) },
            new RenkSecenegi { ad = "Mavi", renk = new Color(0.28f, 0.55f, 0.95f) },
            new RenkSecenegi { ad = "Yesil", renk = new Color(0.30f, 0.78f, 0.40f) },
            new RenkSecenegi { ad = "Sari", renk = new Color(1f, 0.88f, 0.25f) },
            new RenkSecenegi { ad = "Turuncu", renk = new Color(1f, 0.55f, 0.18f) },
            new RenkSecenegi { ad = "Pembe", renk = new Color(1f, 0.55f, 0.75f) },
            new RenkSecenegi { ad = "Kahve", renk = new Color(0.62f, 0.40f, 0.22f) },
        };
    }
}
