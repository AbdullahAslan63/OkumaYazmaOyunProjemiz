using UnityEngine;
using System.Collections.Generic;

public class AvatarCustomizer : MonoBehaviour
{
    [Header("Ekranda Değişecek Karakter Objesi")]
    public SpriteRenderer ekranGorseli;

    [System.Serializable]
    public struct AvatarOutfit
    {
        public string characterName;
        public string accessoryName;
        public Sprite outfitSprite;
    }

    [Header("Tüm Kombinasyonlar")]
    public List<AvatarOutfit> allOutfits; 

    private string selectedCharacter = "kopek";
    private string selectedAccessory = "yok";

    // Karakter butonlarına bunu vereceksin (Örn: "kedi", "tavsan", "kopek", "rakun")
    public void SelectCharacter(string character)
    {
        selectedCharacter = character;
        selectedAccessory = "yok"; // Karakter değiştiğinde aksesuarı sıfırlar, saf hali gelir
        UpdateAvatar();
    }

    // Aksesuar butonlarına bunu vereceksin (Örn: "kulaklik", "papyon", "gozluk", "yok")
    public void SelectAccessory(string accessory)
    {
        selectedAccessory = accessory;
        UpdateAvatar();
    }

    private void UpdateAvatar()
    {
        foreach (var outfit in allOutfits)
        {
            if (outfit.characterName == selectedCharacter && outfit.accessoryName == selectedAccessory)
            {
                if (ekranGorseli != null)
                {
                    ekranGorseli.sprite = outfit.outfitSprite;
                }
                return;
            }
        }

        Debug.LogWarning($"{selectedCharacter} ve {selectedAccessory} için kombin bulunamadı! Listeyi kontrol et.");
    }
}