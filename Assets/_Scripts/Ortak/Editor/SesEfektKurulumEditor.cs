#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Paket 3 Faz 9: SesYoneticisi clip bağlama + HarfSecme partikül kurulumu.
/// Menü: Oyun > Ses ve Efektleri Bagla (Faz 9)
/// Batchmode: -executeMethod SesEfektKurulumEditor.HepsiniBagla
/// </summary>
public static class SesEfektKurulumEditor
{
    private const string PrefabKlasor = "Assets/_Prefabs/Ortak";
    private const string PrefabYol = PrefabKlasor + "/SesYoneticisi.prefab";
    private const string AvatarSahne = "Assets/Scenes/AvatarOlusturmaEkrani.unity";
    private const string HarfSahne = "Assets/Scenes/HarfSecmeOyunu.unity";
    private const string LevelSahne = "Assets/Scenes/LevelEkranı.unity";
    private const string ForceMarker = "Assets/_Scripts/Ortak/Editor/SES_EFEKT_KURULUM.force";

    private const string ObjelerKlasor = "Assets/_Audio/Objeler";
    private const string HarfKlasor = "Assets/_Audio/SesliHarfler";
    private const string MuzikKlasor = "Assets/_Audio/Muzik";
    private const string SfxKlasor = "Assets/_Audio/SFX";

    private const string PrefsAnahtar = "SesEfektKurulum_Faz9_done_v3";
    private static bool kurulumKuyrukta;

    /// <summary>
    /// Prefab / sahne eksikse Editor açılışında bir kez kurar.
    /// Manuel: Oyun > Ses ve Efektleri Bagla (Faz 9)
    /// </summary>
    [InitializeOnLoadMethod]
    private static void OtomatikBirKezKur()
    {
        if (kurulumKuyrukta) return;
        kurulumKuyrukta = true;
        EditorApplication.delayCall += () =>
        {
            kurulumKuyrukta = false;
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (Application.isBatchMode) return;
                if (EditorPrefs.GetBool(PrefsAnahtar, false) && !SahneEksikMi())
                    return;
                if (!SahneEksikMi() && !File.Exists(ForceMarker))
                    return;

                Debug.Log("SesEfektKurulum: otomatik Faz 9 kurulumu başlıyor…");
                HepsiniBagla();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        };
    }

    private static bool SahneEksikMi()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabYol) == null)
            return true;
        if (!File.Exists(AvatarSahne)) return true;

        string yaml = File.ReadAllText(AvatarSahne);
        // Prefab instance adı "value: SesYoneticisi" veya m_Name olarak yazılabilir
        if (!yaml.Contains("SesYoneticisi"))
            return true;

        if (File.Exists(HarfSahne))
        {
            string harfYaml = File.ReadAllText(HarfSahne);
            // Unity Türkçe ı'yı \u0131 olarak kaçırabilir
            bool partikulVar = harfYaml.Contains("ParlıltıEfekti")
                || harfYaml.Contains("Parl\\u0131lt\\u0131Efekti")
                || harfYaml.Contains("parlıltıEfekti")
                || harfYaml.Contains("parl\\u0131lt\\u0131Efekti");
            if (!partikulVar)
                return true;
        }

        return false;
    }

    [MenuItem("Oyun/Ses ve Efektleri Bagla (Faz 9)")]
    public static void HepsiniBagla()
    {
        KlasorleriGarantiEt();

        GameObject prefabKok = PrefabiOlusturVeyaGuncelle();
        ClippleriDoldur(prefabKok.GetComponent<SesYoneticisi>());
        PrefabUtility.SaveAsPrefabAsset(prefabKok, PrefabYol);
        UnityEngine.Object.DestroyImmediate(prefabKok);

        AvatarSahnesineYerlestir();
        LevelSahnesineTiklamaEkle();
        HarfSahnesinePartikulBagla();

        if (File.Exists(ForceMarker))
            AssetDatabase.DeleteAsset(ForceMarker);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorPrefs.SetBool(PrefsAnahtar, true);

        Debug.Log(
            "SesEfektKurulum: prefab + Avatar sahnesi + HarfSecme partikül tamam. " +
            "Eksik clip'ler (Muzik/SFX) null-safe bırakıldı.");

        // Batchmode'da EditorApplication.Exit ile kapanır
        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    private static void KlasorleriGarantiEt()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Prefabs"))
            AssetDatabase.CreateFolder("Assets", "_Prefabs");
        if (!AssetDatabase.IsValidFolder(PrefabKlasor))
            AssetDatabase.CreateFolder("Assets/_Prefabs", "Ortak");
    }

    /// <summary>SesYoneticisi prefab'ını oluşturur veya yükleyip düzenlenebilir kopya döner.</summary>
    private static GameObject PrefabiOlusturVeyaGuncelle()
    {
        GameObject mevcut = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabYol);
        if (mevcut != null)
            return (GameObject)PrefabUtility.InstantiatePrefab(mevcut);

        GameObject go = new GameObject("SesYoneticisi");
        go.AddComponent<SesYoneticisi>();
        return go;
    }

    /// <summary>Klasörlerdeki mevcut clip'leri Inspector alanlarına yazar.</summary>
    public static void ClippleriDoldur(SesYoneticisi ses)
    {
        if (ses == null) return;

        Undo.RecordObject(ses, "SesYoneticisi clip doldur");

        // Obje kelime klipleri
        ses.objeSesleri = ClipleriYukle(ObjelerKlasor);

        // Harf adı klipleri — "Hangisinin..." prompt ayrı alana
        List<AudioClip> harfler = new List<AudioClip>();
        ses.soruPromptSesi = null;
        foreach (AudioClip clip in ClipleriYukle(HarfKlasor))
        {
            if (clip == null) continue;
            string ad = clip.name.ToLowerInvariant();
            if (ad.Contains("hangisinin") || ad.Contains("bas_harfi") || ad.Contains("baş_harfi"))
            {
                ses.soruPromptSesi = clip;
                continue;
            }
            harfler.Add(clip);
        }
        ses.harfSesleri = harfler.ToArray();

        // Oyun müziği: önce Muzik klasörü, yoksa SesliHarfler içindeki Hangisinin prompt'u loop değil —
        // plan: oyunMuzigi = Sesli Harfler şarkısı. Muzik klasöründe varsa onu kullan.
        AudioClip[] muzikler = ClipleriYukle(MuzikKlasor);
        ses.oyunMuzigi = MuzikClipiniSec(muzikler, "oyun", "sesli", "harf", "sarki", "şarkı");
        if (ses.oyunMuzigi == null && muzikler.Length > 0)
            ses.oyunMuzigi = muzikler[0];

        // Hayvan menü müzikleri (0..3) — isimde kedi/tavsan/kus/rakun veya sıra
        ses.hayvanMuzikleri = new AudioClip[4];
        ses.hayvanMuzikleri[0] = MuzikClipiniSec(muzikler, "kedi", "cat");
        ses.hayvanMuzikleri[1] = MuzikClipiniSec(muzikler, "tavsan", "tavşan", "rabbit");
        ses.hayvanMuzikleri[2] = MuzikClipiniSec(muzikler, "kus", "kuş", "bird");
        ses.hayvanMuzikleri[3] = MuzikClipiniSec(muzikler, "rakun", "raccoon");
        // İsim eşleşmezse sırayla doldur (oyun şarkısı hariç)
        int siradaki = 0;
        for (int i = 0; i < 4; i++)
        {
            if (ses.hayvanMuzikleri[i] != null) continue;
            while (siradaki < muzikler.Length)
            {
                AudioClip aday = muzikler[siradaki++];
                if (aday == null || aday == ses.oyunMuzigi) continue;
                ses.hayvanMuzikleri[i] = aday;
                break;
            }
        }

        // SFX
        AudioClip[] sfx = ClipleriYukle(SfxKlasor);
        ses.tiklamaSesi = MuzikClipiniSec(sfx, "tik", "tiklama", "click", "ui");
        ses.dogruSes = MuzikClipiniSec(sfx, "dogru", "doğru", "correct", "ok");
        ses.yanlisSes = MuzikClipiniSec(sfx, "yanlis", "yanlış", "wrong", "error", "hata", "buzzer");
        if (ses.tiklamaSesi == null && sfx.Length > 0)
            ses.tiklamaSesi = sfx[0];
        if (ses.yanlisSes == null && sfx.Length > 1)
            ses.yanlisSes = sfx[1];
        else if (ses.yanlisSes == null && sfx.Length == 1 && sfx[0] != ses.tiklamaSesi)
            ses.yanlisSes = sfx[0];

        EditorUtility.SetDirty(ses);

        Debug.Log(
            "SesYoneticisi clip: obje=" + (ses.objeSesleri?.Length ?? 0) +
            " harf=" + (ses.harfSesleri?.Length ?? 0) +
            " prompt=" + (ses.soruPromptSesi != null ? ses.soruPromptSesi.name : "yok") +
            " oyunMuzigi=" + (ses.oyunMuzigi != null ? ses.oyunMuzigi.name : "yok") +
            " tiklama=" + (ses.tiklamaSesi != null ? ses.tiklamaSesi.name : "yok") +
            " dogru=" + (ses.dogruSes != null ? ses.dogruSes.name : "yok") +
            " yanlis=" + (ses.yanlisSes != null ? ses.yanlisSes.name : "yok"));
    }

    private static AudioClip[] ClipleriYukle(string klasor)
    {
        if (!AssetDatabase.IsValidFolder(klasor) && !Directory.Exists(klasor))
            return new AudioClip[0];

        string[] guidler = AssetDatabase.FindAssets("t:AudioClip", new[] { klasor });
        List<AudioClip> liste = new List<AudioClip>();
        for (int i = 0; i < guidler.Length; i++)
        {
            string yol = AssetDatabase.GUIDToAssetPath(guidler[i]);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(yol);
            if (clip != null)
                liste.Add(clip);
        }
        return liste.OrderBy(c => c.name).ToArray();
    }

    private static AudioClip MuzikClipiniSec(AudioClip[] klipler, params string[] anahtarlar)
    {
        if (klipler == null) return null;
        for (int i = 0; i < klipler.Length; i++)
        {
            if (klipler[i] == null) continue;
            string ad = klipler[i].name.ToLowerInvariant();
            for (int k = 0; k < anahtarlar.Length; k++)
            {
                if (ad.Contains(anahtarlar[k].ToLowerInvariant()))
                    return klipler[i];
            }
        }
        return null;
    }

    /// <summary>Avatar sahnesine DDOL SesYoneticisi yerleştirir (clip'li prefab).</summary>
    private static void AvatarSahnesineYerlestir()
    {
        if (!File.Exists(AvatarSahne))
        {
            Debug.LogWarning("Avatar sahnesi yok: " + AvatarSahne);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(AvatarSahne, OpenSceneMode.Single);
        SesYoneticisi eski = UnityEngine.Object.FindAnyObjectByType<SesYoneticisi>();
        if (eski != null)
            UnityEngine.Object.DestroyImmediate(eski.gameObject);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabYol);
        GameObject ornek = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        ornek.name = "SesYoneticisi";

        // Prefab instance üzerinde de clip'leri taze tut
        SesYoneticisi ses = ornek.GetComponent<SesYoneticisi>();
        ClippleriDoldur(ses);
        PrefabUtility.RecordPrefabInstancePropertyModifications(ses);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, AvatarSahne);
        Debug.Log("SesYoneticisi AvatarOlusturmaEkrani sahnesine yerleştirildi.");
    }

    /// <summary>Level ekranı butonlarına ButonTiklamaSesi ekler (kalıcı).</summary>
    private static void LevelSahnesineTiklamaEkle()
    {
        if (!File.Exists(LevelSahne)) return;

        Scene scene = EditorSceneManager.OpenScene(LevelSahne, OpenSceneMode.Single);
        Button[] butonlar = UnityEngine.Object.FindObjectsByType<Button>();
        int eklenen = 0;
        for (int i = 0; i < butonlar.Length; i++)
        {
            if (butonlar[i].GetComponent<ButonTiklamaSesi>() != null) continue;
            Undo.AddComponent<ButonTiklamaSesi>(butonlar[i].gameObject);
            eklenen++;
        }

        if (eklenen > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, LevelSahne);
        }
        Debug.Log("LevelEkranı: " + eklenen + " butona ButonTiklamaSesi eklendi.");
    }

    /// <summary>HarfSecme: tik arkası partikül + GeriBildirim süre 1.5 + Ana Menü tıklama sesi.</summary>
    private static void HarfSahnesinePartikulBagla()
    {
        if (!File.Exists(HarfSahne))
        {
            Debug.LogWarning("HarfSecme sahnesi yok: " + HarfSahne);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(HarfSahne, OpenSceneMode.Single);

        GeriBildirimYoneticisi geri = UnityEngine.Object.FindAnyObjectByType<GeriBildirimYoneticisi>();
        if (geri == null)
        {
            Debug.LogError("HarfSecmeOyunu: GeriBildirimYoneticisi bulunamadı.");
            return;
        }

        // Tik görselinin arkasına partikül
        Transform tikParent = geri.dogruGorsel != null
            ? geri.dogruGorsel.transform
            : geri.transform;

        ParticleSystem mevcutPs = geri.parlıltıEfekti;
        if (mevcutPs == null)
        {
            Transform eskiCocuk = tikParent.Find("ParlıltıEfekti");
            if (eskiCocuk != null)
                UnityEngine.Object.DestroyImmediate(eskiCocuk.gameObject);

            GameObject psGo = new GameObject("ParlıltıEfekti");
            psGo.transform.SetParent(tikParent, false);
            psGo.transform.SetAsFirstSibling(); // tik'in arkası (UI sibling sırası)
            psGo.transform.localPosition = Vector3.zero;
            psGo.transform.localScale = Vector3.one;

            ParticleSystem ps = psGo.AddComponent<ParticleSystem>();
            ParlikliAyarla(ps);
            mevcutPs = ps;
        }
        else
        {
            ParlikliAyarla(mevcutPs);
        }

        Undo.RecordObject(geri, "GeriBildirim partikül bağla");
        geri.parlıltıEfekti = mevcutPs;
        geri.gosterimSuresi = 1.5f;
        EditorUtility.SetDirty(geri);

        // Ana menü butonuna tıklama sesi
        Button[] butonlar = UnityEngine.Object.FindObjectsByType<Button>();
        for (int i = 0; i < butonlar.Length; i++)
        {
            if (butonlar[i].GetComponent<ButonTiklamaSesi>() != null) continue;
            butonlar[i].gameObject.AddComponent<ButonTiklamaSesi>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, HarfSahne);
        Debug.Log("HarfSecmeOyunu: parlıltıEfekti bağlandı, gosterimSuresi=1.5.");
    }

    /// <summary>1.5 sn süren basit UI parıltı partikülü.</summary>
    private static void ParlikliAyarla(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1.5f;
        main.loop = false;
        main.startLifetime = 1.2f;
        main.startSpeed = 2.5f;
        main.startSize = 0.35f;
        main.startColor = new Color(1f, 0.92f, 0.35f, 1f);
        main.gravityModifier = 0.2f;
        main.playOnAwake = false;
        main.maxParticles = 48;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 28) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.4f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.95f, 0.4f), 0f),
                new GradientColorKey(new Color(1f, 0.6f, 0.1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = g;

        // UI Canvas altında görünsün
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 50;
            // Default-Particle materyali Unity içinde var
            Material mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            if (mat != null)
                renderer.sharedMaterial = mat;
        }
    }
}
#endif
