using TMPro;
using UnityEngine;

/// <summary>
/// TextMeshPro yazısını yukarı doğru yay (kemer) şeklinde eğer.
/// Yıldızların üstünden sol → sağ kavis için kullanılır.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class EgimliYazi : MonoBehaviour
{
    // Yay yüksekliği (yerel birim / UI piksel)
    public float yayYuksekligi = 70f;

    // Karakterleri hafif yay teğetine göre döndür
    public bool karakterleriDondur = true;

    private TMP_Text tmp;
    private string sonMetin;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Yenile();
    }

    private void LateUpdate()
    {
        // Metin değiştiyse veya mesh yenilendiyse tekrar eğ
        if (tmp == null)
        {
            return;
        }

        if (tmp.text != sonMetin)
        {
            Yenile();
        }
    }

    /// <summary>Mesh'i güncelleyip eğri uygular.</summary>
    public void Yenile()
    {
        if (tmp == null)
        {
            tmp = GetComponent<TMP_Text>();
        }

        if (tmp == null)
        {
            return;
        }

        sonMetin = tmp.text;
        tmp.ForceMeshUpdate();
        EgrisiUygula();
    }

    private void EgrisiUygula()
    {
        TMP_TextInfo bilgi = tmp.textInfo;
        if (bilgi == null || bilgi.characterCount == 0)
        {
            return;
        }

        // Tüm görünür karakterlerin X aralığı
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        for (int i = 0; i < bilgi.characterCount; i++)
        {
            TMP_CharacterInfo c = bilgi.characterInfo[i];
            if (!c.isVisible)
            {
                continue;
            }

            minX = Mathf.Min(minX, c.bottomLeft.x, c.topLeft.x);
            maxX = Mathf.Max(maxX, c.bottomRight.x, c.topRight.x);
        }

        float genislik = maxX - minX;
        if (genislik < 0.01f)
        {
            return;
        }

        // Her karakteri parabol yayına taşı: y = h * (1 - (2t-1)^2)
        for (int i = 0; i < bilgi.characterCount; i++)
        {
            TMP_CharacterInfo c = bilgi.characterInfo[i];
            if (!c.isVisible)
            {
                continue;
            }

            int mi = c.materialReferenceIndex;
            int vi = c.vertexIndex;
            Vector3[] vertices = bilgi.meshInfo[mi].vertices;

            // Karakter merkezi
            Vector3 orta = (vertices[vi] + vertices[vi + 1] + vertices[vi + 2] + vertices[vi + 3]) * 0.25f;
            float t = Mathf.InverseLerp(minX, maxX, orta.x);
            // 0..1 → yukarı kemer (kenarlar düşük, orta yüksek)
            float egri = 1f - Mathf.Pow(2f * t - 1f, 2f);
            float yOfset = yayYuksekligi * egri;

            // Teğet eğimi (döndürme için)
            float egim = -2f * (2f * t - 1f) * yayYuksekligi / Mathf.Max(0.01f, genislik);
            float aci = karakterleriDondur ? Mathf.Atan(egim) * Mathf.Rad2Deg : 0f;

            for (int k = 0; k < 4; k++)
            {
                Vector3 v = vertices[vi + k];
                // Merkeze göre yerel
                Vector3 yerel = v - orta;
                if (karakterleriDondur && Mathf.Abs(aci) > 0.01f)
                {
                    float rad = aci * Mathf.Deg2Rad;
                    float cos = Mathf.Cos(rad);
                    float sin = Mathf.Sin(rad);
                    yerel = new Vector3(yerel.x * cos - yerel.y * sin, yerel.x * sin + yerel.y * cos, yerel.z);
                }

                // Yay ofseti
                vertices[vi + k] = orta + yerel + new Vector3(0f, yOfset, 0f);
            }
        }

        // Mesh'i yaz
        for (int i = 0; i < bilgi.meshInfo.Length; i++)
        {
            bilgi.meshInfo[i].mesh.vertices = bilgi.meshInfo[i].vertices;
            tmp.UpdateGeometry(bilgi.meshInfo[i].mesh, i);
        }
    }
}
