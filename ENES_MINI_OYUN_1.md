# ENES_MINI_OYUN_1.md — Enes Barış · Mini Oyun 1 (Sepetle Toplama)

Bu dosya **sadece Enes Barış** için faz kapılı çalışma planıdır. Kod yazmadan önce proje kökündeki `AGENTS.md` dosyasını okuyun. Genel görev özeti: `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` → Paket 4.

---

## Katı kurallar

1. Bir fazın **Tamamlandı kontrol listesi** tamamen işaretlenmeden sonraki faza geçme.
2. Kod üretildikten sonra o fazın **Unity Editor** bölümünü bitirmeden fazı kapatma.
3. Paket 3 (Mustafa Yiğit Avan) bitmeden **Faz 4’e** (`SepetOyunYoneticisi`) geçme.
4. `SepetHareketi.cs` yazma — iptal edildi; yerine `ObjeSurukleme.cs`.
5. Bu sahnede avatar gösterme (avatar entegrasyonu yok).
6. Cursor’a bir seferde tek script ver; yönetici için önce Plan Mode.

---

## Onaylanmış kararlar

| Konu | Karar |
| ---- | ----- |
| Mekanik | Balon sabit; altta 4 pozisyon; objeler balona sürüklenir |
| Scriptler | `ObjeSurukleme.cs` + `SepetOyunYoneticisi.cs` |
| Giriş | Fare + touch/parmak |
| Yanlış bırakma | Geri bildirim + obje başlangıç pozisyonuna döner |
| Süre | Varsayılan **60** saniye (`kalanSure`) |
| Doğru puan | Varsayılan **+10** (`dogruPuan`) |
| Paket 3 | Bitmeden oyun yöneticisi yazılmaz |

---

## Faz akışı

```
Faz 0 (Hazırlık)
  → Faz 1 (Sahne — kod yok)
    → Faz 2 (ObjeSurukleme + touch)
      → Faz 3 (Paket 3 checkpoint)
        → Faz 4 (SepetOyunYoneticisi)
          → Faz 5 (Bağlama + entegrasyon)
            → Faz 6 (Teslim)
```

---

# Faz 0 — Hazırlık

**Amaç:** Ortam ve asset’leri kontrol et; klasörleri aç.

## Yapılacaklar

1. `AGENTS.md` dosyasını baştan sona oku.
2. Grafik ekibinden şu asset’lerin gelip gelmediğini not et (yoksa isimleri / “eksik” yaz):
   - Arkaplan (Mini Oyun 1)
   - Balon görseli
   - Örnek obje sprite’ları (test için en az 2–4 tane)
3. Unity Project penceresinde klasörleri oluştur (yoksa):
   - `Assets/_Scripts/SepetOyunu/`
   - `Assets/_Prefabs/SepetOyunu/`
   - `Assets/_Scenes/` (yoksa)

## Tamamlandı kontrol listesi

- [ ] `AGENTS.md` okundu
- [ ] Asset envanteri not edildi (var / eksik)
- [ ] `Assets/_Scripts/SepetOyunu/` var
- [ ] `Assets/_Prefabs/SepetOyunu/` var
- [ ] `Assets/_Scenes/` var

**Çıkış:** Klasörler hazır; eksik asset’ler yazılı. → Faz 1

---

# Faz 1 — Sahne kurulumu (kod yok)

**Amaç:** `SepetOyunu` sahnesini mekanik testine hazır hale getir. Bu fazda C# yazılmaz.

## Unity Editor — adım adım

### 1.1 Yeni sahne

1. Menü: **File > New Scene** (2D şablon varsa onu seç).
2. **File > Save As…** → yol: `Assets/_Scenes/SepetOyunu.unity`
3. Hierarchy’de **Main Camera** seçili olsun; Projection **Orthographic** olsun.

### 1.2 Arkaplan

1. Hierarchy’de sağ tık > **2D Object > Sprite** (veya boş obje + Sprite Renderer).
2. Adını `Arkaplan` yap.
3. Sprite Renderer > **Sprite** alanına arkaplan resmi sürükle (yoksa geçici renkli kare koy).
4. **Order in Layer** = `-10` yap (her şeyin arkasında kalsın).
5. Kameranın gördüğü alanı kaplayacak şekilde ölçekle / konumla.

### 1.3 Balon (sabit hedef)

1. Hierarchy > 2D Sprite (veya boş + Sprite Renderer), adı: `Balon`.
2. Balon görselini ata; ekranın üst-orta bölgesine yerleştir (sabit kalacak).
3. **Add Component > Box Collider 2D**.
4. Inspector’da **Is Trigger** kutusunu **işaretle**.
5. Tag oluştur:
   - Inspector’da Tag > **Add Tag…**
   - `+` ile yeni tag adı: `Balon`
   - `Balon` objesine geri dön, Tag = **Balon** seç.
6. Collider boyutunu balon görseline göre ayarla (Edit Collider).

### 1.4 Harf rozeti

1. Hierarchy’de `Balon`’a sağ tık > **3D Object > Text - TextMeshPro** (veya TextMeshPro > Text).
   - İlk kullanımda TMP Import penceresi çıkarsa **Import TMP Essentials** de.
2. Child’ın adını `HarfRozeti` yap.
3. Balonun üstünde görünecek şekilde konumlandır; puntoyu büyüt.
4. Geçici test için yazıya `A` yaz (Faz 4’te kod güncelleyecek).

### 1.5 Obje prefab şablonu

1. Hierarchy’de boş/sprite obje oluştur, adı: `Obje`.
2. **Sprite Renderer** ekle; örnek bir obje sprite’ı ata.
3. **Add Component > Box Collider 2D**.
4. **Is Trigger işaretli OLMASIN** (sürükleme için solid collider).
5. Collider’ı sprite’a sığdır.
6. Project’te `Assets/_Prefabs/SepetOyunu/` klasörüne `Obje`’yi sürükle → Prefab oluşsun.
7. Prefab kaydolduktan sonra Hierarchy’deki örnek instance’ı silebilirsin (Faz 2’de test için tekrar koyarsın).
8. Bu fazda scripte gerek yok; script Faz 2’de eklenecek.

### 1.6 Sabit obje pozisyonları

1. Hierarchy’de Create Empty, adı: `ObjePozisyonlari` (üst grup, isteğe bağlı).
2. Altına 4 boş obje:
   - `ObjePozisyon1`
   - `ObjePozisyon2`
   - `ObjePozisyon3`
   - `ObjePozisyon4`
3. Dördünü ekranın **alt** kısmına, yan yana eşit aralıklarla yerleştir.
4. Gizmo ile Scene görünümünde konumları kontrol et (Play’de de kamera çerçevesinde kalsın).

### 1.7 Hızlı görsel kontrol

1. **Play**’e bas.
2. Arkaplan, balon, dört pozisyon (boş da olsa) yerinde mi bak.
3. Play’den çık (**Play** tekrar).
4. Sahneyi kaydet (**Ctrl/Cmd+S**).

## Tamamlandı kontrol listesi

- [ ] `Assets/_Scenes/SepetOyunu.unity` kayıtlı
- [ ] Arkaplan `Order in Layer = -10`
- [ ] `Balon` üzerinde Box Collider 2D + **Is Trigger**
- [ ] Tag `Balon` atanmış
- [ ] Child `HarfRozeti` (TMP) var
- [ ] Prefab `Assets/_Prefabs/SepetOyunu/Obje.prefab` var
- [ ] Prefab’da Box Collider 2D **trigger değil**
- [ ] `ObjePozisyon1`…`4` altta yerleştirildi
- [ ] Play’de görsel kontrol yapıldı

**Çıkış:** Sahne iskeleti hazır. → Faz 2

---

# Faz 2 — ObjeSurukleme.cs (touch destekli)

**Amaç:** Objeyi fare/parmakla sürükle; balona yakın bırakınca yöneticiye haber ver (yoksa log); uzaksa geri dön.

**Bağımlılık:** Paket 3 gerekmez. `SepetOyunYoneticisi` henüz yoksa `Instance == null` dalı ile test edilir.

## 2.A — Script görev tanımı

**Dosya yolu:** `Assets/_Scripts/SepetOyunu/ObjeSurukleme.cs`

**Olması gereken public alanlar:**

| Alan | Tip | Varsayılan / not |
| ---- | --- | ---------------- |
| `objeAdi` | `string` | Inspector’dan |
| `dogruHarf` | `char` | Bu objenin baş harfi |
| `birakmaMesafesi` | `float` | `1.5` |

**Davranış:**

- `Start` (veya ilk tutuşta): başlangıç pozisyonunu private değişkende sakla.
- Tutunca sürüklemeyi başlat; sürüklerken objeyi işaretçiye takip ettir; **Z = 0**.
- Bırakınca Tag `Balon` objeyi bul (`FindGameObjectWithTag`), mesafe ≤ `birakmaMesafesi` ise:
  - `SepetOyunYoneticisi.Instance != null` → `ObjeBirakildi(this)`
  - değilse → `Debug.Log` ile “balona bırakıldı (yönetici yok)” yaz
- Mesafe büyükse objeyi başlangıç pozisyonuna geri koy.
- **Touch:** `GetIsaretDunyaPozisyonu()` gibi bir yardımcı; `Input.touchCount > 0` iken `GetTouch(0).position`, yoksa `Input.mousePosition`; `Camera.main.ScreenToWorldPoint` ile dünya koordinatı (z ekrandan sonra 0’a çek).

**Beklenen kavramlar:** sürükleme state’i, ScreenToWorldPoint, Vector3.Distance, null kontrolü, Collider2D.

## 2.B — Cursor’a örnek prompt

> Unity 2D için `ObjeSurukleme` adında bir C# script yaz. Dosya `Assets/_Scripts/SepetOyunu/` altına gelecek. Önce proje `AGENTS.md` kurallarına uy. Public alanlar: `string objeAdi`, `char dogruHarf`, `float birakmaMesafesi` (varsayılan 1.5f). Start’ta başlangıç pozisyonunu sakla. Fare ve touch destekle: `GetIsaretDunyaPozisyonu()` ile touch varsa `Input.GetTouch(0).position`, yoksa `Input.mousePosition` kullan; Camera.main.ScreenToWorldPoint sonrası z=0 yap. OnMouseDown ile sürüklemeyi başlat, sürüklerken pozisyonu işaretçiye bağla (OnMouseDrag ve/veya Update), OnMouseUp’ta bitir: Tag "Balon" bul, mesafe birakmaMesafesi altındaysa ve SepetOyunYoneticisi.Instance null değilse Instance.ObjeBirakildi(this) çağır; Instance null ise Debug.Log yaz; uzaktaysa başlangıç pozisyonuna dön. SepetOyunYoneticisi sınıfı henüz yoksa ObjeBirakildi çağrısını Instance null kontrolüyle yaz (derleme için yönetici sınıfını ayrı dosyada sonra ekleyeceğiz — şimdilik Instance null dalı yeterli olsun; yönetici tipi için forward declare gerekirse basit bir public class SepetOyunYoneticisi stub’u önerme, Instance kontrolünü yorumla belirt). Her satıra kısa Türkçe yorum ekle.

**Not (derleme):** `SepetOyunYoneticisi` Faz 4’te gelecek. Faz 2’de derleme hatası alırsan geçici olarak sadece `Debug.Log` kullanıp Faz 4’te `ObjeBirakildi` satırını ekle — ya da Faz 4’e geçene kadar log-only bırak. **Tercih edilen yol:** Faz 2 kodunda çağrıyı yaz; derleyici hatası için Mini bir boş `SepetOyunYoneticisi` iskeleti **yazma** — bunun yerine çağrıyı `#` ile geçici kapatma. En temiz yol: Faz 2’yi log-only bitir, Faz 4’te yönetici + `ObjeBirakildi` bağını ekle. Aşağıdaki Editor test listesi log-only ile de geçerlidir.

**Güncel öneri (net):** Faz 2 script’inde bırakma başarılıysa yalnızca:

```csharp
Debug.Log("Balona birakildi: " + objeAdi);
```

Faz 4’te bu satırı `SepetOyunYoneticisi.Instance.ObjeBirakildi(this);` ile değiştir (null kontrolü ile). Faz planında “Faz 4 kod adımı” bunu açıkça tekrarlar.

## 2.C — Unity Editor (kod yazıldıktan sonra)

1. Project’te `Assets/_Scripts/SepetOyunu/ObjeSurukleme.cs` olduğundan emin ol (Compile hatası yok).
2. Prefab’ı aç: `Assets/_Prefabs/SepetOyunu/Obje.prefab` (çift tık veya Prefab Mode).
3. **Add Component** > `ObjeSurukleme`.
4. Örnek değerler: `objeAdi` = `Elma`, `dogruHarf` = `E`, `birakmaMesafesi` = `1.5`.
5. Prefab’ı kaydet, Prefab Mode’dan çık.
6. `SepetOyunu` sahnesinde Hierarchy’ye Obje prefab’ından **1 veya 2** instance sürükle; birini `ObjePozisyon1` yakınına koy.
7. **Main Camera** seçili; oyun objeleri camera view içinde olsun.
8. Collider’ların açık olduğunu kontrol et (`Obje` trigger değil, `Balon` trigger).
9. **Play**:
   - Objeyi sürükle — fareyi takip etmeli.
   - Balondan uzağa bırak — başlangıç yerine dönmeli.
   - Balona yakın bırak — Console’da log görünmeli; yanlışlıkla yok olmamalı.
10. Play’den çık; sahneyi kaydet.
11. (İsteğe bağlı) Unity Device Simulator / dokunmatik test: touchPath ile aynı davranışı doğrula.

## Tamamlandı kontrol listesi

- [ ] `ObjeSurukleme.cs` compile oluyor
- [ ] Prefab’a script ekli; public alanlar dolu
- [ ] Play: sürükleme çalışıyor
- [ ] Play: uzak bırakınca geri dönüyor
- [ ] Play: balona yakın bırakınca Console log geliyor
- [ ] Touch veya en azından mousePosition yolu kodda mevcut

**Çıkış:** Sürükle-bırak tek başına doğrulandı. → Faz 3

---

# Faz 3 — Paket 3 checkpoint (kod yazma yok)

**Amaç:** `SepetOyunYoneticisi` yazmadan önce ortak sistemlerin hazır olduğunu doğrula. **Bu fazda yeni oyun kodu yazılmaz.**

Sorumlu: **Mustafa Yiğit Avan**. Sen sadece checklist’i doldurursun; eksikse bekle / hatırlat.

## Kontrol edilecekler

### 3.1 Veri

- [ ] `HarfObjeVerisi` ScriptableObject script’i var (`Assets/_Scripts/Ortak/` veya kılavuza uygun yer)
- [ ] `Assets/_Data/Harfler/` (veya benzeri) altında en az birkaç harf asset’i (hedef: A, E, I, İ, O, Ö, U, Ü)
- [ ] Her asset’te `harf` + `dogruObjeler` sprite listesi dolu (test için en az 1 harfte 2+ obje)

### 3.2 Script API’leri (public isimler kılavuza uymalı)

- [ ] `SoruSecici` — `HarfObjeVerisi[] tumHarfler`, `YeniHarfSec()`
- [ ] `SkorYoneticisi` — `SkorEkle(int miktar)`, skor yazısı alanı
- [ ] `GeriBildirimYoneticisi` — `DogruGoster()`, `YanlisGoster()`

### 3.3 Sahneye taşınabilirlik

- [ ] Bu bileşenler `SepetOyunu` sahnesine eklenebilir / prefab’lanabilir durumda (en azından diğer sahnede çalıştığı gösterilmiş)

## Tamamlandı kontrol listesi

- [ ] Yukarıdaki tüm maddeler işaretli **veya** Yiğit ile “API kilitlendi, asset’ler yakında” yazılı mutabakat var ve sen en azından boş `HarfObjeVerisi` + çalışan `SoruSecici` iskeletine sahipsin
- [ ] Eksik kalanlar bu dosyaya not edildi

**Çıkış şartı:** Checklist yeşil olmadan Faz 4’e geçme. → Faz 4

---

# Faz 4 — SepetOyunYoneticisi.cs

**Amaç:** Oyunun beyni: süre, harf rozeti, 4 pozisyona obje spawn, bırakma değerlendirmesi, oyun bitişi.

**Ön koşul:** Faz 3 tamam.

## 4.A — Önce Plan Mode (kod yok)

Cursor’a (Plan Mode):

> Unity için `SepetOyunYoneticisi` yazacağım. Paket 4 Mini Oyun 1. Mekanik: balon sabit; altta 4 pozisyona Instantiate; ObjeSurukleme bırakınca ObjeBirakildi çağırır. Görevler: 60 sn geri sayım, süre bitince OyunuBitir; SoruSecici ile harf seçip harfRozeti’ne yaz; ObjeleriOlustur’da 1–2 doğru + 2–3 çeldirici karıştırıp 4 pozisyona prefab Instantiate; doğruda skor +10, DogruGoster, YeniTurBaslat; yanlışta sadece YanlisGoster (obje kendi script’inde döner). Avatar yok. Singleton Instance. Kod yazmadan adım adım plan anlat.

Planı oku, onayla, sonra kod prompt’una geç.

## 4.B — Script görev tanımı

**Dosya yolu:** `Assets/_Scripts/SepetOyunu/SepetOyunYoneticisi.cs`

**Public alanlar (isimler sabit):**

| Alan | Tip | Varsayılan |
| ---- | --- | ---------- |
| `soruSecici` | `SoruSecici` | Inspector |
| `skorYoneticisi` | `SkorYoneticisi` | Inspector |
| `geriBildirim` | `GeriBildirimYoneticisi` | Inspector |
| `objePrefab` | `GameObject` | Obje.prefab |
| `objePozisyonlari` | `Transform[]` | 4 eleman |
| `harfRozeti` | `TextMeshPro` | dünya uzayı TMP |
| `sureYazisi` | `TextMeshProUGUI` | UI |
| `kalanSure` | `float` | **60** |
| `dogruPuan` | `int` | **10** |

**Fonksiyonlar:**

- `YeniTurBaslat()` — harf seç, rozeti güncelle, eski objeleri temizle, `ObjeleriOlustur`
- `ObjeleriOlustur()` — doğru + çeldirici seç, karıştır, Instantiate, `ObjeSurukleme` alanlarını (sprite / `objeAdi` / `dogruHarf`) ata
- `ObjeBirakildi(ObjeSurukleme obje)` — `obje.dogruHarf` aktif harfle aynı mı? doğru → skor + geri bildirim + yeni tur; yanlış → sadece `YanlisGoster`
- `OyunuBitir()` — süreyi durdur, sürüklemeyi / spawn’ı kes (objeleri yok et veya input’u kilitle — planda seçtiğin basit yol)

**Update:** oyun aktifken `kalanSure -= Time.deltaTime`; `sureYazisi` güncelle; ≤ 0 ise `OyunuBitir`.

**Singleton:** `static Instance`, `Awake` içinde ata.

## 4.C — Cursor’a örnek prompt (plan onayından sonra)

> Planı onayladım. `SepetOyunYoneticisi` C# kodunu yaz. AGENTS.md’ye uy. Public alanlar: soruSecici, skorYoneticisi, geriBildirim, objePrefab, Transform[] objePozisyonlari, TextMeshPro harfRozeti, TextMeshProUGUI sureYazisi, float kalanSure=60f, int dogruPuan=10. Singleton Instance. Start’ta YeniTurBaslat. Update’te süre. ObjeleriOlustur: aktif harfin dogruObjeler’inden 1–2, diğer harflerden 2–3 çeldirici, karıştır, pozisyonlara Instantiate et, her ObjeSurukleme’ye sprite/objeAdi/dogruHarf ata. ObjeBirakildi: doğruysa SkorEkle(dogruPuan), DogruGoster, YeniTurBaslat; yanlışsa YanlisGoster. OyunuBitir süre bitince. Avatar kodu yok. Her satıra Türkçe yorum.

## 4.D — ObjeSurukleme bağını güncelle

Faz 2’de sadece `Debug.Log` vardıysa, bırakma başarılı dalını şuna çevir:

- `SepetOyunYoneticisi.Instance != null` iken `Instance.ObjeBirakildi(this);`
- null ise log (güvenlik)

## 4.E — Unity Editor (bu fazda zorunlu değil ama önerilir)

Sadece compile kontrolü yeterli olabilir; tam bağlama **Faz 5**. En azından:

1. Script’in Project’te göründüğünü ve Console’da error olmadığını doğrula.
2. Henüz sahneye ekleme — Faz 5.

## Tamamlandı kontrol listesi

- [ ] Plan Mode çıktısı okundu ve onaylandı
- [ ] `SepetOyunYoneticisi.cs` compile oluyor
- [ ] Public alan isimleri tablodakiyle aynı
- [ ] `kalanSure` varsayılan 60, `dogruPuan` 10
- [ ] `ObjeSurukleme` yöneticiyi çağıracak şekilde güncellendi
- [ ] Avatar referansı yok

**Çıkış:** Kod hazır. → Faz 5

---

# Faz 5 — Bağlama ve entegrasyon testi

**Amaç:** Her şeyi Inspector’da bağla; Play’de tam akışı doğrula.

## Unity Editor — adım adım

### 5.1 Oyun yöneticisi objesi

1. `SepetOyunu` sahnesini aç.
2. Hierarchy > Create Empty, adı: `OyunYoneticisi`.
3. **Add Component > SepetOyunYoneticisi**.

### 5.2 UI — süre ve skor

1. Hierarchy > UI > **Canvas** (yoksa oluştur).
   - Canvas Scaler: **Scale With Screen Size**; Reference Resolution örn. 1920×1080.
2. Canvas altında UI > Text - TextMeshPro, adı: `SureYazisi` (örn. sağ üst).
3. Paket 3’teki skor objesini bu sahneye ekle / oluştur (`SkorYazisi` + `SkorYoneticisi` component’i kılavuza göre).
4. Geri bildirim objelerini ekle (`DogruIkon`, `YanlisIkon`, Particle, Audio — Paket 3’ün Editor adımları).

### 5.3 Ortak sistem objeleri

1. Boş obje veya Paket 3 prefab’ı: `SoruSecici` component + Inspector’da `tumHarfler` dizisini harf asset’leriyle doldur.
2. `SkorYoneticisi` ve `GeriBildirimYoneticisi` referanslarını hazırla.

### 5.4 Inspector doldurma (`OyunYoneticisi`)

`SepetOyunYoneticisi` alanlarına sürükle:

1. `soruSecici` → SoruSecici component’li obje
2. `skorYoneticisi` → SkorYoneticisi
3. `geriBildirim` → GeriBildirimYoneticisi
4. `objePrefab` → `Assets/_Prefabs/SepetOyunu/Obje.prefab`
5. `objePozisyonlari` Size = 4 → `ObjePozisyon1`…`4` Transform’ları
6. `harfRozeti` → Balon child TMP
7. `sureYazisi` → Canvas’taki TMP
8. `kalanSure` = 60, `dogruPuan` = 10 (gerekirse düzelt)

### 5.5 Build Settings (önemli)

1. **File > Build Settings**
2. **Add Open Scenes** ile `SepetOyunu` listede olsun (yoksa sürükle).
3. Paket 6 tam geçiş için diğer sahneleri de ekleyecek; sen en azından kendi sahnenin listede olduğunu doğrula.

### 5.6 Play test checklist

Play’e bas ve sırayla dene:

- [ ] Oyun başında harf rozetinde bir harf görünüyor
- [ ] Altta 4 obje spawn oluyor (doğru + çeldirici karışık)
- [ ] Doğru objeyi balona bırakınca puan **+10**, doğru geri bildirim, yeni tur
- [ ] Yanlış objeyi bırakınca yanlış geri bildirim; obje eski yerine dönüyor; tur zorla değişmiyor
- [ ] Süre 60’tan geri sayıyor; `SureYazisi` güncelleniyor
- [ ] Süre 0 olunca oyun duruyor (`OyunuBitir` davranışı hissediliyor)
- [ ] Aynı harf arka arkaya gelmiyor (mümkünse birkaç tur dene — `SoruSecici`)
- [ ] Console’da kırmızı error yok

## Tamamlandı kontrol listesi

- [ ] Tüm Inspector referansları dolu (None yok)
- [ ] Play test checklist’inin tamamı geçti
- [ ] Sahne kayıtlı
- [ ] Build Settings’te `SepetOyunu` var

**Çıkış:** Mini Oyun 1 oynanabilir. → Faz 6

---

# Faz 6 — Teslim (Paket 6’ya)

**Amaç:** Süleyman Öz’e entegrasyon notunu bırak; kendi faz checkbox’larını kapat.

## Teslim notu (kopyala-yapıştır için)

```
Mini Oyun 1 (Enes) teslim
- Sahne: Assets/_Scenes/SepetOyunu.unity
- Scriptler: ObjeSurukleme.cs, SepetOyunYoneticisi.cs
- Prefab: Assets/_Prefabs/SepetOyunu/Obje.prefab
- Süre varsayılan 60 sn, doğru +10
- Avatar bu sahnede yok
- SepetHareketi YOK (iptal)

Paket 6’dan beklenenler:
- [ ] Font’un skor / süre / HarfRozeti’ne uygulanması
- [ ] Ses clip’lerinin GeriBildirim / SesYoneticisi alanlarına bağlanması
- [ ] Avatar seçim ekranından SepetOyunu’na SahneGecisi butonu
- [ ] Build Settings son kontrolü
```

## Tamamlandı kontrol listesi

- [ ] Faz 0–5 listeleri bu dosyada işaretli
- [ ] Teslim notu Süleyman’a iletildi (chat / dosya / sözlü)
- [ ] Bilinen bug’lar varsa bu bölüme yazıldı:

**Bilinen sorunlar:**

- (yoksa “yok” yaz)

---

## Hızlı referans — dosya yolları

| Ne | Yol |
| -- | --- |
| Sürükleme | `Assets/_Scripts/SepetOyunu/ObjeSurukleme.cs` |
| Yönetici | `Assets/_Scripts/SepetOyunu/SepetOyunYoneticisi.cs` |
| Prefab | `Assets/_Prefabs/SepetOyunu/Obje.prefab` |
| Sahne | `Assets/_Scenes/SepetOyunu.unity` |
| Ekip kuralları | `AGENTS.md` |

## İptal

| Dosya | Durum |
| ----- | ----- |
| `SepetHareketi.cs` | Yazılmaz. Balon sabit; sürüklenen objelerdir. |
