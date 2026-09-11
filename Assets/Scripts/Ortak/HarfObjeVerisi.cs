using UnityEngine;

// ScriptableObject: Sahne yerine proje içinde .asset dosyası olarak saklanan veri sınıfı.
// Kod değiştirmeden Inspector üzerinden harf ve görselleri düzenleyebilirsiniz.
//
// CreateAssetMenu: Unity Editor'da Project penceresinde sağ tık > Create > Oyun > Harf Verisi
// menüsünden yeni bir HarfObjeVerisi .asset dosyası oluşturmanızı sağlar.
[CreateAssetMenu(fileName = "YeniHarf", menuName = "Oyun/Harf Verisi")]
public class HarfObjeVerisi : ScriptableObject
{
    // Bu kaydın temsil ettiği harf (ör. 'A', 'B', 'Ç').
    public char harf;

    // Bu harfle eşleşen doğru nesnelerin görselleri.
    // Oyunda "doğru cevap" olarak kullanılacak Sprite'lar buraya atanır.
    public Sprite[] dogruObjeler;
}
