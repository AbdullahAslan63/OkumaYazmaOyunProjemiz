using UnityEngine;
using System.Collections.Generic;

public class AvatarCustomizer : MonoBehaviour
{
    public static AvatarCustomizer Instance { get; private set; }

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

    private string selectedCharacter = "tavsan";
    private string selectedAccessory = "yok";

    private static readonly Dictionary<string, int> CharacterToIndex = new Dictionary<string, int>
    {
        { "kedi", 0 },
        { "tavsan", 1 },
        { "kopek", 2 },
        { "rakun", 3 }
    };

    private static readonly Dictionary<string, int> AccessoryToIndex = new Dictionary<string, int>
    {
        { "yok", -1 },
        { "sapka", 0 },
        { "parti sapkasi", 1 },
        { "papyon", 2 },
        { "kulaklik", 3 },
        { "gozluk", 4 }
    };

    private void Awake()
    {
        // Butonlara yanlışlıkla eklenen boş kopyaları ve KarakterKonumu kopyasını pas geç
        bool isNamedPrimary = gameObject.name == "AvatarCustomizer";
        bool hasData = allOutfits != null && allOutfits.Count > 0 && ekranGorseli != null;

        if (!isNamedPrimary && !hasData)
        {
            enabled = false;
            return;
        }

        // İsimli asıl obje her zaman kazanır
        if (Instance != null && Instance != this && Instance.gameObject.name == "AvatarCustomizer" && !isNamedPrimary)
        {
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        selectedCharacter = PlayerPrefs.GetString("SavedCharacter", "tavsan");
        selectedAccessory = PlayerPrefs.GetString("SavedAccessory", "yok");

        SyncToYoneticisi();
        UpdateAvatar();
    }

    public void SelectCharacter(string character)
    {
        if (string.IsNullOrWhiteSpace(character))
            return;

        selectedCharacter = Normalize(character);
        PlayerPrefs.SetString("SavedCharacter", selectedCharacter);
        PlayerPrefs.Save();

        SyncToYoneticisi();
        UpdateAvatar();
    }

    public void SelectAccessory(string accessory)
    {
        if (string.IsNullOrWhiteSpace(accessory))
            accessory = "yok";

        selectedAccessory = Normalize(accessory);
        PlayerPrefs.SetString("SavedAccessory", selectedAccessory);
        PlayerPrefs.Save();

        SyncToYoneticisi();
        UpdateAvatar();
    }

    public void SelectColor(Color color)
    {
        color.a = 1f;

        if (ekranGorseli != null)
            ekranGorseli.color = color;

        if (AvatarYoneticisi.Instance != null)
        {
            AvatarYoneticisi.Instance.RenkSec(color);
        }
        else
        {
            // Yoneticisi yoksa da rengi sakla
            PlayerPrefs.SetFloat("Avatar_Renk_R", color.r);
            PlayerPrefs.SetFloat("Avatar_Renk_G", color.g);
            PlayerPrefs.SetFloat("Avatar_Renk_B", color.b);
            PlayerPrefs.Save();
        }
    }

    public string GetSelectedCharacter() => selectedCharacter;
    public string GetSelectedAccessory() => selectedAccessory;

    private void SyncToYoneticisi()
    {
        if (AvatarYoneticisi.Instance == null)
            return;

        if (CharacterToIndex.TryGetValue(selectedCharacter, out int hayvanIndex))
            AvatarYoneticisi.Instance.HayvanSec(hayvanIndex);

        if (AccessoryToIndex.TryGetValue(selectedAccessory, out int aksesuarIndex))
            AvatarYoneticisi.Instance.AksesuarSec(aksesuarIndex);
        else
            AvatarYoneticisi.Instance.AksesuarSec(-1);
    }

    private void UpdateAvatar()
    {
        if (ekranGorseli == null)
        {
            Debug.LogError("HATA: 'Ekran Görseli' (SpriteRenderer) boş bırakılmış! Inspector'dan atamalısın.");
            return;
        }

        Sprite found = FindOutfitSprite(selectedCharacter, selectedAccessory);

        // Seçili aksesuar bu karakterde yoksa aksesuarsız (normal) haline düş
        if (found == null && selectedAccessory != "yok")
        {
            Debug.LogWarning($"Kombin bulunamadı, normal haline dönülüyor. Karakter: '{selectedCharacter}', Aksesuar: '{selectedAccessory}'");
            found = FindOutfitSprite(selectedCharacter, "yok");
        }

        if (found != null)
        {
            ekranGorseli.sprite = found;

            if (AvatarYoneticisi.Instance != null)
                ekranGorseli.color = AvatarYoneticisi.Instance.seciliRenk;

            Debug.Log($"Avatar güncellendi → {selectedCharacter} + {selectedAccessory} ({found.name})");
            return;
        }

        Debug.LogWarning($"Kombin bulunamadı! Karakter: '{selectedCharacter}', Aksesuar: '{selectedAccessory}'. allOutfits listesine 'yok' girdilerini de ekle.");
    }

    private Sprite FindOutfitSprite(string character, string accessory)
    {
        if (allOutfits == null)
            return null;

        string c = Normalize(character);
        string a = Normalize(accessory);

        foreach (var outfit in allOutfits)
        {
            if (outfit.outfitSprite == null)
                continue;

            if (Normalize(outfit.characterName) == c && Normalize(outfit.accessoryName) == a)
                return outfit.outfitSprite;
        }

        return null;
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "" : value.Trim().ToLowerInvariant();
    }
}
