using UnityEngine;

/// <summary>
/// Ambient sparkle ParticleSystem kurucu (doğru-cevap burst'ünden ayrı).
/// Düşük rate, looping, yavaş yükselen krem/altın parçacıklar.
/// </summary>
public static class OrtamPariltilari
{
    /// <summary>World veya UI parent altında looping sparkle ekler / günceller.</summary>
    public static ParticleSystem GarantiEt(
        Transform parent,
        string ad = "OrtamPariltilari",
        float yaricap = 0.55f,
        float rate = 4f,
        int sortingOrder = 9,
        bool uiAltinda = false)
    {
        if (parent == null)
            return null;

        Transform eski = parent.Find(ad);
        GameObject go = eski != null ? eski.gameObject : new GameObject(ad);
        if (eski == null)
            go.transform.SetParent(parent, false);

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps == null)
            ps = go.AddComponent<ParticleSystem>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Ayarla(ps, yaricap, rate, sortingOrder, uiAltinda);
        ps.Play();
        return ps;
    }

    /// <summary>Tik arkası 1.5 sn burst (GeriBildirimYoneticisi.parlıltıEfekti).</summary>
    public static ParticleSystem DogruBurstGarantiEt(Transform tikParent, GeriBildirimYoneticisi geri)
    {
        if (geri == null)
            return null;

        if (geri.parlıltıEfekti != null)
            return geri.parlıltıEfekti;

        Transform parent = tikParent != null ? tikParent : geri.transform;
        Transform eski = parent.Find("ParlıltıEfekti");
        GameObject go = eski != null ? eski.gameObject : new GameObject("ParlıltıEfekti");
        if (eski == null)
            go.transform.SetParent(parent, false);

        go.transform.SetAsFirstSibling();
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps == null)
            ps = go.AddComponent<ParticleSystem>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        BurstAyarla(ps);
        geri.parlıltıEfekti = ps;
        geri.gosterimSuresi = 1.5f;
        return ps;
    }

    /// <summary>Ambient sparkle ayarları.</summary>
    public static void Ayarla(ParticleSystem ps, float yaricap, float rate, int sortingOrder, bool uiAltinda)
    {
        var main = ps.main;
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        main.startColor = new Color(1f, 0.95f, 0.75f, 0.9f);
        main.gravityModifier = -0.05f;
        main.playOnAwake = true;
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = rate;
        emission.SetBursts(System.Array.Empty<ParticleSystem.Burst>());

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = Mathf.Max(0.05f, yaricap);
        shape.radiusThickness = 0.85f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.98f, 0.85f), 0f),
                new GradientColorKey(new Color(1f, 0.85f, 0.45f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.95f, 0.25f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = g;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, 0.4f),
            new Keyframe(0.35f, 1f),
            new Keyframe(1f, 0.2f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = sortingOrder;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            // Builtin particle materyali
            Material mat = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
            if (mat != null)
                renderer.sharedMaterial = mat;
        }

        // UI Canvas altında ölçü birimi farklı — biraz büyüt
        if (uiAltinda)
        {
            main.startSize = new ParticleSystem.MinMaxCurve(8f, 18f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(12f, 36f);
            shape.radius = Mathf.Max(20f, yaricap * 40f);
        }
    }

    /// <summary>Doğru cevap tik patlaması (1.5 sn, non-loop).</summary>
    public static void BurstAyarla(ParticleSystem ps)
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

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 50;
            Material mat = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
            if (mat != null)
                renderer.sharedMaterial = mat;
        }
    }
}
