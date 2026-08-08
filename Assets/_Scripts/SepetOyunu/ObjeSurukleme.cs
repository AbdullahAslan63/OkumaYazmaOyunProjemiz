using UnityEngine; // Unity bileşenleri için

/// <summary>
/// Objeyi fare veya parmakla sürükleyip balona bırakmayı yönetir.
/// </summary>
public class ObjeSurukleme : MonoBehaviour
{
    // Bu objenin adı (Inspector'dan doldurulur)
    public string objeAdi;

    // Bu objenin doğru baş harfi
    public char dogruHarf;

    // Balona bırakma için kabul edilen maksimum mesafe
    public float birakmaMesafesi = 1.5f;

    // Objenin tur başındaki konumu
    private Vector3 baslangicPozisyonu;

    // Şu an sürükleniyor mu?
    private bool surukleniyor = false;

    // Oyun başında başlangıç konumunu sakla
    private void Start()
    {
        // Mevcut dünyadaki pozisyonu kaydet
        baslangicPozisyonu = transform.position;
    }

    // Her karede sürükleme varsa pozisyonu güncelle
    private void Update()
    {
        // Sürüklenmiyorsa bir şey yapma
        if (!surukleniyor)
        {
            // Erken çık
            return;
        }

        // İşaretçinin dünya konumunu al
        Vector3 hedef = GetIsaretDunyaPozisyonu();

        // Objenin z'sini sıfırda tut (2D)
        hedef.z = 0f;

        // Objeyi işaretçiye taşı
        transform.position = hedef;
    }

    // Fare ile objeye tıklanınca sürüklemeyi başlat
    private void OnMouseDown()
    {
        // Sürükleme durumunu aç
        surukleniyor = true;
    }

    // Fare basılı sürüklenirken de takip et (Update ile birlikte güvenli)
    private void OnMouseDrag()
    {
        // Sürükleme açıkken Update zaten taşır; yine de işaretçiye bağla
        Vector3 hedef = GetIsaretDunyaPozisyonu();

        // Z eksenini sıfırla
        hedef.z = 0f;

        // Pozisyonu güncelle
        transform.position = hedef;
    }

    // Fare bırakılınca bırakma kontrolünü yap
    private void OnMouseUp()
    {
        // Sürüklemeyi bitir
        surukleniyor = false;

        // Balonu tag ile bul
        GameObject balon = GameObject.FindGameObjectWithTag("Balon");

        // Balon yoksa başlangıca dön ve çık
        if (balon == null)
        {
            // Uyarı yaz
            Debug.LogWarning("Balon tag'li obje bulunamadi.");

            // Başlangıç pozisyonuna geri koy
            transform.position = baslangicPozisyonu;

            // Çık
            return;
        }

        // Obje ile balon arasındaki mesafeyi ölç
        float mesafe = Vector3.Distance(transform.position, balon.transform.position);

        // Mesafe kabul aralığındaysa başarılı bırakma
        if (mesafe <= birakmaMesafesi)
        {
            // Faz 2: yönetici henüz yok; sadece log yaz
            // Faz 4'te: SepetOyunYoneticisi.Instance.ObjeBirakildi(this);
            Debug.Log("Balona birakildi: " + objeAdi);
        }
        else
        {
            // Uzak bırakıldıysa başlangıç yerine dön
            transform.position = baslangicPozisyonu;
        }
    }

    // Fare veya touch ekran konumunu dünya koordinatına çevirir
    private Vector3 GetIsaretDunyaPozisyonu()
    {
        // Varsayılan: fare konumu
        Vector3 ekranPozisyonu = Input.mousePosition;

        // Dokunma varsa ilk parmağı kullan
        if (Input.touchCount > 0)
        {
            // İlk dokunmanın ekran konumunu al
            ekranPozisyonu = Input.GetTouch(0).position;
        }

        // Ana kamerayı al
        Camera kamera = Camera.main;

        // Kamera yoksa mevcut pozisyonu geri ver
        if (kamera == null)
        {
            // Güvenli dönüş
            return transform.position;
        }

        // Ekranı dünyaya çevir
        Vector3 dunyaPozisyonu = kamera.ScreenToWorldPoint(ekranPozisyonu);

        // 2D için z'yi sıfırla
        dunyaPozisyonu.z = 0f;

        // Dünya konumunu döndür
        return dunyaPozisyonu;
    }
}
