using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Karakter / renk / aksesuar kartında seçili durumu çerçeve + tik ile gösterir.
/// </summary>
public class SecimVurgusu : MonoBehaviour
{
    public enum Tur
    {
        Hayvan,
        Renk,
        Aksesuar
    }

    // Bu kartın türü
    public Tur tur = Tur.Hayvan;

    // Hayvan / aksesuar indeksi (aksesuar yok = -1)
    public int indeks;

    // Renk kartı için karşılaştırma rengi
    public Color renk = Color.white;

    // Seçili çerçeve rengi (yeşil-turkuaz)
    private static readonly Color SeciliCerceve = new Color(0.12f, 0.72f, 0.48f, 1f);
    private static readonly Color NormalCerceve = new Color(1f, 1f, 1f, 0.45f);

    private Outline cerceve;
    private GameObject tikRozet;
    private Vector3 normalOlcek;

    /// <summary>Kart kurulurken bir kez çağrılır; çerçeve ve tik rozetini hazırlar.</summary>
    public void Kur()
    {
        cerceve = GetComponent<Outline>();
        if (cerceve == null)
            cerceve = gameObject.AddComponent<Outline>();

        normalOlcek = transform.localScale;
        if (normalOlcek.sqrMagnitude < 0.01f)
            normalOlcek = Vector3.one;

        TikRozetiniGarantiEt();
        Yenile();
    }

    /// <summary>Sahnedeki tüm seçim vurgularını yöneticiye göre günceller.</summary>
    public static void HepsiniYenile()
    {
        SecimVurgusu[] list = Object.FindObjectsByType<SecimVurgusu>(FindObjectsSortMode.None);
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] != null)
                list[i].Yenile();
        }
    }

    /// <summary>Bu kart seçili mi diye bakıp çerçeve / tik gösterir.</summary>
    public void Yenile()
    {
        bool secili = SeciliMi();

        if (cerceve != null)
        {
            cerceve.effectColor = secili ? SeciliCerceve : NormalCerceve;
            cerceve.effectDistance = secili ? new Vector2(6f, -6f) : new Vector2(1f, -1f);
        }

        if (tikRozet != null)
            tikRozet.SetActive(secili);

        // Seçili kart biraz büyüsün
        transform.localScale = secili ? normalOlcek * 1.04f : normalOlcek;
    }

    private bool SeciliMi()
    {
        AvatarYoneticisi y = AvatarYoneticisi.Instance;
        if (y == null)
            return false;

        switch (tur)
        {
            case Tur.Hayvan:
                return y.seciliHayvanIndex == indeks;
            case Tur.Aksesuar:
                return y.seciliAksesuarIndex == indeks;
            case Tur.Renk:
                return RenkYakin(y.seciliRenk, renk);
            default:
                return false;
        }
    }

    /// <summary>Float renk karşılaştırması (küçük farkları yut).</summary>
    private static bool RenkYakin(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.02f
            && Mathf.Abs(a.g - b.g) < 0.02f
            && Mathf.Abs(a.b - b.b) < 0.02f;
    }

    /// <summary>Sağ üstte yeşil tik rozeti oluşturur.</summary>
    private void TikRozetiniGarantiEt()
    {
        Transform eski = transform.Find("SecimTik");
        if (eski != null)
        {
            tikRozet = eski.gameObject;
            return;
        }

        tikRozet = new GameObject("SecimTik", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tikRozet.transform.SetParent(transform, false);
        RectTransform rt = tikRozet.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-8f, -8f);
        rt.sizeDelta = new Vector2(44f, 44f);

        Image zemin = tikRozet.GetComponent<Image>();
        zemin.color = SeciliCerceve;
        zemin.raycastTarget = false;

        GameObject yaziGo = new GameObject("Tik", typeof(RectTransform));
        yaziGo.transform.SetParent(tikRozet.transform, false);
        Text t = yaziGo.AddComponent<Text>();
        t.text = "✓";
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.fontSize = 28;
        t.fontStyle = FontStyle.Bold;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.raycastTarget = false;
        RectTransform yaziRt = t.rectTransform;
        yaziRt.anchorMin = Vector2.zero;
        yaziRt.anchorMax = Vector2.one;
        yaziRt.offsetMin = Vector2.zero;
        yaziRt.offsetMax = Vector2.zero;

        tikRozet.SetActive(false);
    }
}
