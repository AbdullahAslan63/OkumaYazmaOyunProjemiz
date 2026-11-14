using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Güneş etrafında ışınlar; her 4 saniyede bir ışın söner (15×4 = 60 sn).
/// </summary>
public class GunesSayac : MonoBehaviour
{
    // Güneş görseli
    public Image gunesGorsel;

    // Işınlar (saat yönünde)
    public Image[] isinlar = new Image[15];

    // Bir ışının süresi (saniye)
    public float isinSuresi = 4f;

    // Süre bitince çağrılır
    public System.Action SureBitti;

    // Kalan ışın sayısı
    private int kalanIsin = 20;

    // Bir sonraki ışına kalan süre
    private float sayac;

    // Sayaç çalışıyor mu
    private bool aktif;

    /// <summary>Tüm ışınları açıp sayacı başlatır.</summary>
    public void Baslat()
    {
        kalanIsin = isinlar != null ? isinlar.Length : 0;
        sayac = isinSuresi;
        aktif = true;
        // Tüm ışınları göster
        for (int i = 0; i < isinlar.Length; i++)
        {
            if (isinlar[i] != null)
            {
                isinlar[i].enabled = true;
                isinlar[i].gameObject.SetActive(true);
            }
        }
    }

    /// <summary>Sayacı durdurur.</summary>
    public void Durdur()
    {
        aktif = false;
    }

    private void Update()
    {
        if (!aktif)
        {
            return;
        }

        sayac -= Time.deltaTime;
        if (sayac > 0f)
        {
            return;
        }

        // Sonraki ışın periyodu
        sayac = isinSuresi;
        // Bir ışın azalt (sondan başa doğru kapat)
        if (kalanIsin > 0)
        {
            kalanIsin--;
            if (kalanIsin < isinlar.Length && isinlar[kalanIsin] != null)
            {
                isinlar[kalanIsin].gameObject.SetActive(false);
            }
        }

        // Hepsi bittiyse oyun sonu
        if (kalanIsin <= 0)
        {
            aktif = false;
            if (SureBitti != null)
            {
                SureBitti.Invoke();
            }
        }
    }
}
