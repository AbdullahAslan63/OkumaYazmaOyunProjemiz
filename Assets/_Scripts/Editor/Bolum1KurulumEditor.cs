#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Avatar + HarfSecme sahnelerini, prefab'ı ve HarfObjeVerisi asset'lerini kurar.
/// Menü: Oyun > Bolum1 Kurulumu (Avatar + HarfSecme)
/// </summary>
public static class Bolum1KurulumEditor
{
    private const string HarfDataKlasor = "Assets/_Data/Harfler";
    private const string PrefabYol = "Assets/_Prefabs/HarfSecmeOyunu/SecenekBalonu.prefab";
    private const string AvatarSahne = "Assets/Scenes/AvatarOlusturmaEkrani.unity";
    private const string HarfSahne = "Assets/Scenes/HarfSecmeOyunu.unity";
    private const string EskiAvatarSahne = "Assets/Scenes/AbdullahScene.unity";

    [MenuItem("Oyun/Bolum1 Kurulumu (Avatar + HarfSecme)")]
    public static void HepsiniKur()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            KlasorleriGarantiEt();
            HarfVerileriniOlustur();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        SecenekPrefabOlustur();
        AvatarSahnesiniKur();
        HarfSahnesiniKur();
        BuildSettingsGuncelle();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorPrefs.SetBool("Bolum1Kurulum_v1_done", true);
        Debug.Log("Bolum1 kurulumu tamamlandı: AvatarOlusturmaEkrani + HarfSecmeOyunu + Harf verileri + prefab.");
    }

    // Script derlenince eski düz Hierarchy varsa otomatik kur
    [InitializeOnLoadMethod]
    private static void OtomatikKurulumKontrol()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(HarfSahne)) return;

            string yaml = File.ReadAllText(HarfSahne);
            bool harfSahneEski = yaml.Contains("m_Name: codes") || yaml.Contains("shbaraba");
            if (!harfSahneEski)
                return; // Temiz sahne / runtime kurucu hazır — otomatik dokunma

            Debug.Log("Bolum1 otomatik kurulum başlıyor (eski HarfSecme Hierarchy tespit edildi)…");
            HepsiniKur();
        };
    }

    /// <summary>
    /// Sahnedeki eski ok/renk barını silip Roblox tipi dolap UI'sini kaydeder.
    /// Hayvan ve aksesuar listesine dokunmaz.
    /// </summary>
    [MenuItem("Oyun/Avatar Dolap Ekranini Kur")]
    public static void AvatarDolapEkraniniKur()
    {
        if (!File.Exists(AvatarSahne))
        {
            Debug.LogError("Avatar sahnesi yok: " + AvatarSahne);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(AvatarSahne, OpenSceneMode.Single);
        AvatarSecimEkrani secim = Object.FindAnyObjectByType<AvatarSecimEkrani>();
        if (secim == null)
        {
            Debug.LogError("AvatarCanvas üzerinde AvatarSecimEkrani yok.");
            return;
        }

        AvatarSahneKurucu.MevcutCanvasaDolapUygula(secim);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, AvatarSahne);
        Debug.Log("Avatar dolap UI sahneye kaydedildi. Karakter/aksesuar ofsetleri aynı kaldı.");
    }

    private static void KlasorleriGarantiEt()
    {
        string[] klasorler =
        {
            "Assets/_Scripts/Ortak",
            "Assets/_Data",
            "Assets/_Data/Harfler",
            "Assets/_Prefabs/HarfSecmeOyunu",
            "Assets/_Prefabs/Avatar",
            "Assets/Scenes"
        };
        foreach (string k in klasorler)
        {
            if (!AssetDatabase.IsValidFolder(k))
            {
                string parent = Path.GetDirectoryName(k).Replace("\\", "/");
                string name = Path.GetFileName(k);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    // ---------- Harf ScriptableObject ----------

    private static void HarfVerileriniOlustur()
    {
        var harfler = new (char harf, string assetAd)[]
        {
            ('A', "Harf_A"),
            ('E', "Harf_E"),
            ('I', "Harf_I"),
            ('İ', "Harf_I_Noktali"),
            ('O', "Harf_O"),
            ('Ö', "Harf_O_IkiNokta"),
            ('U', "Harf_U"),
            ('Ü', "Harf_U_IkiNokta"),
        };

        string objelerKok = "Assets/_Art/Bolum1/Objeler";
        string[] altKlasorler = Directory.Exists(objelerKok)
            ? Directory.GetDirectories(objelerKok)
            : new string[0];

        foreach (var h in harfler)
        {
            string eslesen = ProjeYolu(AltKlasorBulHarf(altKlasorler, h.harf));
            Sprite[] spriteler = SpriteleriYukle(eslesen);

            string assetPath = HarfDataKlasor + "/" + h.assetAd + ".asset";
            HarfObjeVerisi veri = AssetDatabase.LoadAssetAtPath<HarfObjeVerisi>(assetPath);
            if (veri == null)
            {
                veri = ScriptableObject.CreateInstance<HarfObjeVerisi>();
                AssetDatabase.CreateAsset(veri, assetPath);
            }
            veri.harf = h.harf;
            veri.dogruObjeler = spriteler ?? new Sprite[0];
            EditorUtility.SetDirty(veri);
            Debug.Log($"Harf verisi: {h.assetAd} → {veri.dogruObjeler.Length} sprite ({eslesen})");
        }
    }

    private static string ProjeYolu(string fullOrAsset)
    {
        if (string.IsNullOrEmpty(fullOrAsset)) return null;
        fullOrAsset = fullOrAsset.Replace("\\", "/");
        int idx = fullOrAsset.IndexOf("Assets/", System.StringComparison.Ordinal);
        if (idx >= 0) return fullOrAsset.Substring(idx);
        return fullOrAsset;
    }

    private static string AltKlasorBulHarf(string[] altKlasorler, char harf)
    {
        if (altKlasorler == null) return null;
        string hedef = harf.ToString().Normalize(System.Text.NormalizationForm.FormC);

        foreach (string yol in altKlasorler)
        {
            string ad = Path.GetFileName(yol).Normalize(System.Text.NormalizationForm.FormC);
            if (ad == hedef) return yol.Replace("\\", "/");
        }

        // NFD klasör adları (I + combining mark)
        foreach (string yol in altKlasorler)
        {
            string ad = Path.GetFileName(yol);
            string nfc = ad.Normalize(System.Text.NormalizationForm.FormC);
            if (nfc.Length > 0 && nfc[0] == harf) return yol.Replace("\\", "/");
            if (nfc == hedef) return yol.Replace("\\", "/");
        }
        return null;
    }

    private static string AltKlasorBul(string[] altKlasorler, string hedefAd, char harf)
    {
        return AltKlasorBulHarf(altKlasorler, harf);
    }

    private static Sprite[] SpriteleriYukle(string klasor)
    {
        if (string.IsNullOrEmpty(klasor) || !AssetDatabase.IsValidFolder(klasor))
            return new Sprite[0];

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { klasor });
        List<Sprite> liste = new List<Sprite>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Sprite olarak yükle (Texture Type Sprite olmalı)
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite spr = all.OfType<Sprite>().FirstOrDefault();
            if (spr == null)
                spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr != null)
                liste.Add(spr);
        }
        return liste.ToArray();
    }

    // ---------- Prefab ----------

    private static void SecenekPrefabOlustur()
    {
        GameObject root = new GameObject("SecenekBalonu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(SecenekBalonu));
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(380f, 450f);

        Image cerceve = root.GetComponent<Image>();
        cerceve.color = Color.white;
        Sprite balonSprite = IlkSpriteBul("Assets/_Art/Bolum2/Balon");
        if (balonSprite == null)
            balonSprite = IlkSpriteBul("Assets/_Art/Bolum1/Balon");
        if (balonSprite != null)
            cerceve.sprite = balonSprite;
        cerceve.type = Image.Type.Simple;
        cerceve.preserveAspect = true;

        GameObject objeGo = new GameObject("ObjeResmi", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objeGo.transform.SetParent(root.transform, false);
        RectTransform objeRt = objeGo.GetComponent<RectTransform>();
        objeRt.anchorMin = new Vector2(0.5f, 0.5f);
        objeRt.anchorMax = new Vector2(0.5f, 0.5f);
        objeRt.sizeDelta = new Vector2(260f, 230f);
        objeRt.anchoredPosition = new Vector2(0f, -20f);
        Image objeImg = objeGo.GetComponent<Image>();
        objeImg.preserveAspect = true;
        objeImg.raycastTarget = false;

        SecenekBalonu balon = root.GetComponent<SecenekBalonu>();
        balon.objeResmi = objeImg;

        Button btn = root.GetComponent<Button>();
        btn.targetGraphic = cerceve;

        // Prefab kaydet
        string klasor = Path.GetDirectoryName(PrefabYol).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(klasor))
            AssetDatabase.CreateFolder("Assets/_Prefabs", "HarfSecmeOyunu");

        PrefabUtility.SaveAsPrefabAsset(root, PrefabYol);
        Object.DestroyImmediate(root);
        Debug.Log("SecenekBalonu prefab kaydedildi: " + PrefabYol);
    }

    private static Sprite IlkSpriteBul(string klasor)
    {
        if (!AssetDatabase.IsValidFolder(klasor)) return null;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { klasor });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite s = all.OfType<Sprite>().FirstOrDefault();
            if (s != null) return s;
        }
        return null;
    }

    // ---------- Avatar sahnesi ----------

    private static void AvatarSahnesiniKur()
    {
        // Eski adı yeniden adlandır
        if (File.Exists(EskiAvatarSahne) && !File.Exists(AvatarSahne))
        {
            AssetDatabase.MoveAsset(EskiAvatarSahne, AvatarSahne);
        }

        Scene scene;
        if (File.Exists(AvatarSahne))
            scene = EditorSceneManager.OpenScene(AvatarSahne, OpenSceneMode.Single);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Mevcut AvatarYoneticisi / Avatar bul veya oluştur
        AvatarYoneticisi yonetici = Object.FindAnyObjectByType<AvatarYoneticisi>();
        GameObject yoneticiGo;
        if (yonetici == null)
        {
            yoneticiGo = new GameObject("AvatarYoneticisi");
            yonetici = yoneticiGo.AddComponent<AvatarYoneticisi>();
        }
        else
        {
            yoneticiGo = yonetici.gameObject;
        }

        AvatarGorunumu gorunum = Object.FindAnyObjectByType<AvatarGorunumu>();
        GameObject avatarGo;
        if (gorunum == null)
        {
            avatarGo = new GameObject("Avatar");
            avatarGo.transform.SetParent(yoneticiGo.transform, false);
            GameObject govde = new GameObject("Govde");
            govde.transform.SetParent(avatarGo.transform, false);
            SpriteRenderer govdeSr = govde.AddComponent<SpriteRenderer>();
            GameObject aksesuar = new GameObject("Aksesuar");
            aksesuar.transform.SetParent(avatarGo.transform, false);
            SpriteRenderer aksSr = aksesuar.AddComponent<SpriteRenderer>();
            aksSr.sortingOrder = 1;
            gorunum = avatarGo.AddComponent<AvatarGorunumu>();
            gorunum.govdeRenderer = govdeSr;
            gorunum.aksesuarRenderer = aksSr;
        }
        else
        {
            avatarGo = gorunum.gameObject;
        }

        // Karakter UI panellerinin üstünde dursun
        avatarGo.transform.position = new Vector3(0f, 0.5f, 0f);

        // Hayvan sprite'larını doldur
        gorunum.hayvanlar = new HayvanGorseli[4];
        gorunum.hayvanlar[0] = HayvanYukle("Kedi", "avatar_kedi", new Vector2(0f, 1.9f));
        gorunum.hayvanlar[1] = HayvanYukle("Tavşan", "avatar_tavsan", new Vector2(0f, 1.7f));
        gorunum.hayvanlar[2] = HayvanYukle("Kuş", "avatar_kus", new Vector2(0f, 1.5f));
        gorunum.hayvanlar[3] = HayvanYukle("Rakun", "avatar_rakun", new Vector2(0f, 1.8f));
        gorunum.hedefGovdeYuksekligi = 4.5f;
        gorunum.hedefZeminY = -1.5f;
        gorunum.aksesuarlar = AksesuarGorselleriniYukle();
        EditorUtility.SetDirty(gorunum);

        // Canvas UI
        Canvas eskiCanvas = Object.FindAnyObjectByType<Canvas>();
        if (eskiCanvas == null || eskiCanvas.name != "AvatarCanvas")
        {
            // Eski UI varsa silme — yeni AvatarCanvas oluştur
            GameObject canvasGo = new GameObject("AvatarCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            if (Object.FindAnyObjectByType<EventSystem>() == null)
                EventSystemOlustur();

            AvatarSecimEkrani secim = canvasGo.AddComponent<AvatarSecimEkrani>();
            secim.avatarGorunumu = gorunum;
            secim.sonrakiSahneAdi = "HarfSecmeOyunu";

            // Geri / İleri butonları
            Sprite okSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Art/Avatar/UI/ui_icon_sag_ok.png");
            if (okSprite == null)
            {
                Object[] okAll = AssetDatabase.LoadAllAssetsAtPath("Assets/_Art/Avatar/UI/ui_icon_sag_ok.png");
                okSprite = okAll.OfType<Sprite>().FirstOrDefault();
            }

            Button geri = UiButonOlustur(canvasGo.transform, "GeriButon", new Vector2(-700, 0), new Vector2(100, 100), okSprite);
            if (geri != null)
            {
                geri.transform.localScale = new Vector3(-1, 1, 1);
                UnityEventTools.AddPersistentListener(geri.onClick, secim.OncekiHayvan);
            }
            Button ileri = UiButonOlustur(canvasGo.transform, "IleriButon", new Vector2(700, 0), new Vector2(100, 100), okSprite);
            if (ileri != null)
                UnityEventTools.AddPersistentListener(ileri.onClick, secim.SonrakiHayvan);

            // Renk paneli
            GameObject renkPanel = new GameObject("RenkPaneli", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            renkPanel.transform.SetParent(canvasGo.transform, false);
            RectTransform renkRt = renkPanel.GetComponent<RectTransform>();
            renkRt.anchoredPosition = new Vector2(0, -350);
            renkRt.sizeDelta = new Vector2(600, 80);
            Color[] renkler = { Color.white, new Color(1f, 0.55f, 0.2f), new Color(1f, 0.6f, 0.8f), new Color(0.4f, 0.75f, 1f) };
            Sprite renkSprite = SpriteYukle("Assets/_Art/Avatar/UI/ui_buton_renk_secili.png");
            for (int i = 0; i < renkler.Length; i++)
            {
                Button rb = UiButonOlustur(renkPanel.transform, "Renk" + i, Vector2.zero, new Vector2(80, 80), renkSprite);
                Image img = rb.GetComponent<Image>();
                img.color = renkler[i];
                RenkSecici rs = rb.gameObject.AddComponent<RenkSecici>();
                rs.avatarGorunumu = gorunum;
                rs.builtinRenk = renkler[i];
                UnityEventTools.AddPersistentListener(rb.onClick, rs.BuButonunRenginiSec);
            }

            // Aksesuar paneli — eşit hücre, okunaklı ikonlar
            GameObject aksPanel = new GameObject("AksesuarPaneli", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            aksPanel.transform.SetParent(canvasGo.transform, false);
            RectTransform aksRt = aksPanel.GetComponent<RectTransform>();
            aksRt.anchoredPosition = new Vector2(0, -480);
            aksRt.sizeDelta = new Vector2(1100, 140);
            HorizontalLayoutGroup aksLayout = aksPanel.GetComponent<HorizontalLayoutGroup>();
            aksLayout.spacing = 16f;
            aksLayout.childAlignment = TextAnchor.MiddleCenter;
            aksLayout.childControlWidth = true;
            aksLayout.childControlHeight = true;
            aksLayout.childForceExpandWidth = false;
            aksLayout.childForceExpandHeight = false;
            // Yok butonu
            Button yokBtn = UiButonOlustur(aksPanel.transform, "AksesuarYok", Vector2.zero, new Vector2(110, 110), null);
            LayoutElementEkle(yokBtn.gameObject, 110f, 110f);
            AksesuarSecici yokSec = yokBtn.gameObject.AddComponent<AksesuarSecici>();
            yokSec.avatarGorunumu = gorunum;
            yokSec.aksesuarIndex = -1;
            UnityEventTools.AddPersistentListener(yokBtn.onClick, yokSec.BuAksesuariSec);
            Text yokText = new GameObject("Text").AddComponent<Text>();
            yokText.transform.SetParent(yokBtn.transform, false);
            yokText.text = "Yok";
            yokText.alignment = TextAnchor.MiddleCenter;
            yokText.color = Color.black;
            RectTransform yt = yokText.GetComponent<RectTransform>();
            yt.anchorMin = Vector2.zero; yt.anchorMax = Vector2.one;
            yt.offsetMin = Vector2.zero; yt.offsetMax = Vector2.zero;

            AksesuarGorseli[] aks = gorunum.aksesuarlar;
            for (int i = 0; i < aks.Length && i < 8; i++)
            {
                Sprite aksSprite = aks[i] != null ? aks[i].sprite : null;
                Button ab = UiButonOlustur(aksPanel.transform, "Aksesuar" + i, Vector2.zero, new Vector2(110, 110), aksSprite);
                LayoutElementEkle(ab.gameObject, 110f, 110f);
                AksesuarSecici asec = ab.gameObject.AddComponent<AksesuarSecici>();
                asec.avatarGorunumu = gorunum;
                asec.aksesuarIndex = i;
                UnityEventTools.AddPersistentListener(ab.onClick, asec.BuAksesuariSec);
            }

            // Devam — panelin altında net boşluk
            Sprite devamSprite = SpriteYukle("Assets/_Art/Avatar/UI/ui_buton_devam_et.png");
            Button devam = UiButonOlustur(canvasGo.transform, "DevamButon", new Vector2(0, -600), new Vector2(280, 90), devamSprite);
            UnityEventTools.AddPersistentListener(devam.onClick, secim.DevamEt);
        }

        EditorSceneManager.SaveScene(scene, AvatarSahne);
        Debug.Log("Avatar sahnesi kaydedildi: " + AvatarSahne);
    }

    private static HayvanGorseli HayvanYukle(string ad, string prefix, Vector2 aksesuarOfset)
    {
        string klasor = "Assets/_Art/Avatar/Karakterler";
        return new HayvanGorseli
        {
            hayvanAdi = ad,
            normalPoz = SpritePrefixBul(klasor, prefix + "_normal"),
            mutluPoz = SpritePrefixBul(klasor, prefix + "_mutlu"),
            uzgunPoz = SpritePrefixBul(klasor, prefix + "_uzgun"),
            govdeOlcekCarpani = 1f,
            aksesuarOfset = aksesuarOfset
        };
    }

    private static Sprite SpritePrefixBul(string klasor, string prefix)
    {
        string[] guids = AssetDatabase.FindAssets(prefix + " t:Texture2D", new[] { klasor });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!Path.GetFileNameWithoutExtension(path).StartsWith(prefix)) continue;
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite s = all.OfType<Sprite>().FirstOrDefault();
            if (s != null) return s;
        }
        // Dosya yolu ile dene
        string direkt = klasor + "/" + prefix + ".png";
        Object[] all2 = AssetDatabase.LoadAllAssetsAtPath(direkt);
        return all2.OfType<Sprite>().FirstOrDefault();
    }

    private static AksesuarGorseli[] AksesuarGorselleriniYukle()
    {
        string klasor = "Assets/_Art/Avatar/Aksesuar";
        if (!AssetDatabase.IsValidFolder(klasor)) return new AksesuarGorseli[0];
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { klasor });
        List<AksesuarGorseli> list = new List<AksesuarGorseli>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite s = all.OfType<Sprite>().FirstOrDefault();
            if (s == null) continue;

            // Gözlükler daha küçük ve biraz aşağı; şapkalar kafa tepesinde
            string ad = s.name.ToLowerInvariant();
            bool gozluk = ad.Contains("gozluk") || ad.Contains("gunes");
            list.Add(new AksesuarGorseli
            {
                sprite = s,
                olcek = gozluk ? 0.28f : 0.45f,
                yerelOfset = gozluk ? new Vector2(0f, -0.35f) : Vector2.zero
            });
        }
        return list.ToArray();
    }

    private static void LayoutElementEkle(GameObject go, float genislik, float yukseklik)
    {
        LayoutElement le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.preferredWidth = genislik;
        le.preferredHeight = yukseklik;
        le.minWidth = genislik;
        le.minHeight = yukseklik;
    }

    private static Sprite SpriteYukle(string path)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        Sprite s = all.OfType<Sprite>().FirstOrDefault();
        if (s != null) return s;
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Button UiButonOlustur(Transform parent, string ad, Vector2 pos, Vector2 size, Sprite sprite)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.GetComponent<Image>();
        img.color = Color.white;
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        else
        {
            img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        }
        return go.GetComponent<Button>();
    }

    // ---------- HarfSecme sahnesi ----------

    private static void HarfSahnesiniKur()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Arkaplan
        GameObject arka = new GameObject("Arkaplan");
        SpriteRenderer arkaSr = arka.AddComponent<SpriteRenderer>();
        Sprite arkaSprite = SpriteYukle("Assets/_Art/Bolum2/Arkaplan/arkaplan_canakkale.png");
        if (arkaSprite == null)
            arkaSprite = IlkSpriteBul("Assets/_Art/Bolum2/Arkaplan");
        arkaSr.sprite = arkaSprite;
        arkaSr.sortingOrder = -10;
        arka.transform.position = Vector3.zero;
        if (arkaSprite != null)
        {
            float worldH = Camera.main != null ? Camera.main.orthographicSize * 2f : 10f;
            float scale = worldH / arkaSprite.bounds.size.y;
            arka.transform.localScale = Vector3.one * scale;
        }

        // Ortak yöneticiler
        GameObject ortak = new GameObject("OrtakSistemler");
        SoruSecici soru = ortak.AddComponent<SoruSecici>();
        SkorYoneticisi skor = ortak.AddComponent<SkorYoneticisi>();
        GeriBildirimYoneticisi geri = ortak.AddComponent<GeriBildirimYoneticisi>();

        // 8 harf asset
        string[] assetAdlari = { "Harf_A", "Harf_E", "Harf_I", "Harf_I_Noktali", "Harf_O", "Harf_O_IkiNokta", "Harf_U", "Harf_U_IkiNokta" };
        List<HarfObjeVerisi> harfler = new List<HarfObjeVerisi>();
        foreach (string ad in assetAdlari)
        {
            HarfObjeVerisi v = AssetDatabase.LoadAssetAtPath<HarfObjeVerisi>(HarfDataKlasor + "/" + ad + ".asset");
            if (v != null) harfler.Add(v);
        }
        soru.tumHarfler = harfler.ToArray();

        // Canvas
        GameObject canvasGo = new GameObject("HarfCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        EventSystemOlustur();

        // Büyük harf + sabit soru başlığı
        TextMeshProUGUI buyuk = TmpOlustur(canvasGo.transform, "BuyukHarf", "A", 190, new Vector2(0, 400));
        TextMeshProUGUI baslik = TmpOlustur(canvasGo.transform, "SoruBaslik", "Hangisinin baş harfi", 52, new Vector2(0, 500));
        TextMeshProUGUI sure = TmpOlustur(canvasGo.transform, "SureYazisi", "01:00   Doğru: 0", 48, new Vector2(0, 560));
        baslik.fontStyle = FontStyles.Bold;
        baslik.color = Color.white;

        // Geri bildirim görselleri
        GameObject dogruFb = FeedbackImage(canvasGo.transform, "DogruGeriBildirim", "Assets/_Art/Bolum2/UI/Dogru_Onay.png");
        GameObject yanlisFb = FeedbackImage(canvasGo.transform, "YanlisGeriBildirim", "Assets/_Art/Bolum2/UI/Tekrar_Dene_Mesaj.png");
        dogruFb.SetActive(false);
        yanlisFb.SetActive(false);
        geri.dogruGorsel = dogruFb;
        geri.yanlisGorsel = yanlisFb;
        geri.skorYazisi = sure;

        // 4 slot
        Transform[] slotlar = new Transform[4];
        float[] xPos = { -540f, -180f, 180f, 540f };
        for (int i = 0; i < 4; i++)
        {
            GameObject slot = new GameObject("SecenekPozisyon" + (i + 1), typeof(RectTransform));
            slot.transform.SetParent(canvasGo.transform, false);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xPos[i], -80f);
            rt.sizeDelta = new Vector2(380f, 450f);
            slotlar[i] = slot.transform;
        }

        // Bitiş paneli
        GameObject bitis = new GameObject("BitisPaneli", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bitis.transform.SetParent(canvasGo.transform, false);
        RectTransform bitisRt = bitis.GetComponent<RectTransform>();
        bitisRt.anchorMin = Vector2.zero;
        bitisRt.anchorMax = Vector2.one;
        bitisRt.offsetMin = Vector2.zero;
        bitisRt.offsetMax = Vector2.zero;
        bitis.GetComponent<Image>().color = new Color(0.72f, 0.88f, 0.98f, 0.97f);
        TextMeshProUGUI tebrik = TmpOlustur(bitis.transform, "BitisTebrik", "Süper!", 72, new Vector2(0, 120));
        TextMeshProUGUI bitisSkor = TmpOlustur(bitis.transform, "BitisSkor", "Doğru: 0", 44, new Vector2(0, 0));
        Image[] yildizlar = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject y = new GameObject("Yildiz" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            y.transform.SetParent(bitis.transform, false);
            RectTransform yrt = y.GetComponent<RectTransform>();
            yrt.anchoredPosition = new Vector2(-120 + i * 120, 50);
            yrt.sizeDelta = new Vector2(72, 72);
            Image yi = y.GetComponent<Image>();
            Sprite ys = SpriteYukle("Assets/_Art/Bolum2/UI/Dogru_Yildiz.png");
            if (ys != null) yi.sprite = ys;
            yildizlar[i] = yi;
        }
        Button anaMenu = UiButonOlustur(bitis.transform, "AnaMenuButon", new Vector2(0, -180), new Vector2(320, 90), null);
        anaMenu.GetComponent<Image>().color = new Color(0.98f, 0.55f, 0.25f);
        TextMeshProUGUI anaText = TmpOlustur(anaMenu.transform, "Text", "Ana Menü", 40, Vector2.zero);
        anaText.color = Color.white;
        bitis.SetActive(false);

        // Oyun yöneticisi
        GameObject oyun = new GameObject("OyunYoneticisi");
        HarfSecmeYoneticisi hy = oyun.AddComponent<HarfSecmeYoneticisi>();
        hy.soruSecici = soru;
        hy.skorYoneticisi = skor;
        hy.geriBildirim = geri;
        hy.secenekPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabYol);
        hy.secenekPozisyonlari = slotlar;
        hy.buyukHarf = buyuk;
        hy.soruBaslikYazisi = baslik;
        hy.sureYazisi = sure;
        hy.soruFontu = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Fonts/SoruFontu.asset");
        if (hy.soruFontu == null)
            TmpSoruFontKurulum.FontuOlustur();
        hy.soruFontu = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Fonts/SoruFontu.asset");
        hy.bitisPaneli = bitis;
        hy.bitisTebrikYazisi = tebrik;
        hy.bitisSkorYazisi = bitisSkor;
        hy.bitisYildizlari = yildizlar;
        hy.anaMenuSahneAdi = "AvatarOlusturmaEkrani";
        UnityEventTools.AddPersistentListener(anaMenu.onClick, hy.AnaMenuyeDon);

        EditorSceneManager.SaveScene(scene, HarfSahne);
        Debug.Log("HarfSecme sahnesi yeniden kuruldu: " + HarfSahne);
    }

    private static TextMeshProUGUI TmpOlustur(Transform parent, string ad, string metin, float size, Vector2 pos)
    {
        GameObject go = new GameObject(ad);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = metin;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.95f, 0.2f, 1f);
        if (ad == "BuyukHarf" || size >= 120f)
        {
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        }
        RectTransform rt = tmp.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(900, size + 40f);
        return tmp;
    }

    private static GameObject FeedbackImage(Transform parent, string ad, string spritePath)
    {
        GameObject go = new GameObject(ad, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 200);
        rt.sizeDelta = new Vector2(400, 200);
        Image img = go.GetComponent<Image>();
        Sprite s = SpriteYukle(spritePath);
        if (s != null)
        {
            img.sprite = s;
            img.preserveAspect = true;
        }
        img.raycastTarget = false;
        return go;
    }

    private static void BuildSettingsGuncelle()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(AvatarSahne, true),
            new EditorBuildSettingsScene(HarfSahne, true)
        };
        // Sepet varsa ekle
        if (File.Exists("Assets/Scenes/SepetOyunu.unity"))
            scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/SepetOyunu.unity", true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build Settings güncellendi.");
    }

    private static void EventSystemOlustur()
    {
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }
}
#endif
