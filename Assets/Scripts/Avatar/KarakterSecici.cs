using UnityEngine;

/// <summary>
/// Karakter butonları için ince sarmalayıcı.
/// Asıl görsel güncelleme AvatarCustomizer üzerinden yapılır —
/// böylece aksesuar seçimi seçili hayvanı unutmaz.
/// </summary>
public class KarakterSecici : MonoBehaviour
{
    [Header("Hayvan Fotoğrafları (yedek — AvatarCustomizer yoksa kullanılır)")]
    public Sprite kediSprite;
    public Sprite kopekSprite;
    public Sprite tavsanSprite;
    public Sprite rakunSprite;

    [Header("Ekranda Değişecek Karakter Objesi")]
    public SpriteRenderer ekranGorseli;

    private void Start()
    {
        // İlk görünümü AvatarCustomizer.Start yükler (PlayerPrefs).
        // Burada zorla hayvan seçmiyoruz; yoksa kayıtlı seçim ezilir.
        if (AvatarCustomizer.Instance == null)
            TavsanSec();
    }

    public void KediSec() => Sec("kedi", kediSprite);
    public void KopekSec() => Sec("kopek", kopekSprite);
    public void TavsanSec() => Sec("tavsan", tavsanSprite);
    public void RakunSec() => Sec("rakun", rakunSprite);

    private void Sec(string characterName, Sprite yedekSprite)
    {
        if (AvatarCustomizer.Instance != null)
        {
            AvatarCustomizer.Instance.SelectCharacter(characterName);
            return;
        }

        // Fallback: Customizer yoksa eski davranış
        if (ekranGorseli != null && yedekSprite != null)
            ekranGorseli.sprite = yedekSprite;
    }
}
