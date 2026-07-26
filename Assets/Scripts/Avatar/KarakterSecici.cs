using UnityEngine;

public class KarakterSecici : MonoBehaviour
{
    [Header("Hayvan Fotoğrafları")]
    public Sprite kediSprite;
    public Sprite kopekSprite;
    public Sprite tavsanSprite;
    public Sprite rakunSprite;

    [Header("Ekranda Değişecek Karakter Objesi")]
    public SpriteRenderer ekranGorseli;

    void Start()
    {
        // Oyun başladığında otomatik olarak Tavşan ile başlasın
        TavsanSec();
    }

    public void KediSec()
    {
        if (ekranGorseli != null) ekranGorseli.sprite = kediSprite;
    }

    public void KopekSec()
    {
        if (ekranGorseli != null) ekranGorseli.sprite = kopekSprite;
    }

    public void TavsanSec()
    {
        if (ekranGorseli != null) ekranGorseli.sprite = tavsanSprite;
    }

    public void RakunSec()
    {
        if (ekranGorseli != null) ekranGorseli.sprite = rakunSprite;
    }
}