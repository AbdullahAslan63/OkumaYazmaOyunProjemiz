# AGENTS.md — Okuma Yazma Oyunu

Bu dosya, `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` ile birlikte projenin çalışma anayasasıdır. İkisi çelişirse kılavuzdaki görev tanımı (alan adları, fonksiyon adları, mekanik) esas alınır. Uygulama sırası ise bu dosyadaki faz kapısına uyar: bir faz bitmeden sonraki faza geçilmez.

Aktif çalışan: **Enes Barış**. Branch: `enes/level_1`. Sahip olduğu iş: **Paket 4 — Mini Oyun 1: Sepetle Toplama**. Adım adım sıra: `ENES_BARIS_YOL_HARITASI.md`.

## Faz kapısı

- Çalışma birimi fazdır. Yol haritasındaki fazlar 0’dan 7’ye sırayla gider.
- Bir fazın “Bitti” kriterlerinin hepsi sağlanmadan sonraki fazın kodu, sahnesi veya bağlantısı yazılmaz.
- Aynı turda bir sonraki faza “hazırlık olsun” diye dosya, script veya sahne kurulumu eklenmez.
- Fazın bittiği, kısa bir kontrol listesiyle kullanıcıya söylenir. Kullanıcı o fazı kapatmadan agent bir sonraki faza geçmez.
- Takılılan yerde faz genişletilmez; eksik olan tek şey tamamlanır.

## Oyun mekaniği

Balon sabit durur. Objeler ekranın altında sabit durur. Oyuncu objeyi sürükleyip balonun üzerine bırakır.

## İsim sözleşmesi

Kılavuzdaki `public` alan ve fonksiyon adları aynen kullanılır. Agent bu isimleri yeniden adlandırmaz, İngilizce karşılık üretmez, Inspector bağını bozacak sarmalayıcı eklemez.

Paket 4’te dondurulmuş isimler:

- `ObjeSurukleme`: `objeAdi`, `dogruHarf`
- `SepetOyunYoneticisi`: `soruSecici`, `skorYoneticisi`, `geriBildirim`, `objePrefab`, `objePozisyonlari`, `harfRozeti`, `sureYazisi`, `kalanSure`
- `SepetOyunYoneticisi`: `YeniTurBaslat()`, `ObjeleriOlustur()`, `ObjeBirakildi(ObjeSurukleme obje)`, `OyunuBitir()`
- Sahne: `SepetOyunu`. Tag: `Balon`. Prefab: `Assets/_Prefabs/Obje.prefab`.

Paket 3’ten sadece bu tipler kullanılır, bu branch’te yeniden yazılmaz: `HarfObjeVerisi`, `SoruSecici`, `SkorYoneticisi`, `GeriBildirimYoneticisi`.

## Agent nasıl çalışır

1. Görevden önce bu dosyayı ve `ENES_BARIS_YOL_HARITASI.md` içindeki aktif fazı okur.
2. Bir turda tek script. Paket 4’te sıra `ObjeSurukleme`, sonra `SepetOyunYoneticisi`.
3. `SepetOyunYoneticisi` için önce plan yazılır, kullanıcı onaylar, onaydan sonra kod yazılır.
4. Üretilen koda kısa Türkçe satır yorumu eklenir.
5. Kod yazıldıktan sonra kullanıcıya ne yaptığı, hangi alan adlarının kılavuzla aynı kaldığı ve fazın bitip bitmediği söylenir.
6. Unity editör işi (sahne, buton, sürükle-bırak bağlama, Inspector referansı) koda gömülmez. Agent bu adımları yol haritasından madde madde söyler; sahnedeki tıklamayı kullanıcı yapar.

## Klasörler

- Scriptler: `Assets/_Scripts/` altında, Paket 4 için `Assets/_Scripts/Sepet/`
- Prefab: `Assets/_Prefabs/`
- Sahne: `Assets/_Scenes/SepetOyunu.unity` (sahneyi kullanıcı Unity’de kaydeder)

## Sınırlar

- Paket 1, 3, 5 ve 6’nın scriptleri bu branch’te yazılmaz.
- `main` ve başka kişilerin branch’leri değiştirilmez.
- Commit, kullanıcı açıkça isteyince atılır. Bir fazın bitmesi tek başına commit sebebi değildir.
