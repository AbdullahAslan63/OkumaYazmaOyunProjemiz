using UnityEngine;

/// <summary>
/// Bir harf için doğru obje sprite listesini tutan veri kartı.
/// Project: Create > Oyun > Harf Verisi
/// </summary>
[CreateAssetMenu(fileName = "YeniHarf", menuName = "Oyun/Harf Verisi")]
public class HarfObjeVerisi : ScriptableObject
{
    // Bu kartın harfi (örn. 'A', 'İ')
    public char harf;

    // Bu harfle başlayan obje görselleri
    public Sprite[] dogruObjeler;
}
