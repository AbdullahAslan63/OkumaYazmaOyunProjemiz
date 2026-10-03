# yol_haritasi.md — Enes Barış · Mini Oyun 1 (Sepetle Toplama)

Bu dosya **sadece Enes Barış** içindir ve çalışmanın tek yol haritasıdır. Kaynak: `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` → Paket 4 (Adım 4.1–4.6). Kod yazmadan önce kökteki `AGENTS.md` dosyasını okuyun. Bu dosyadaki sıranın dışına çıkma.

Durum tarihi: **28 Eylül 2026**. Faz 0 kapandı. Faz 1’de kurulan sahne ve prefab geri alındı. Enes Faz 1’i yeniden kuracak.

---

## Mevcut durum

**Faz:** Faz 1. Sahne henüz yok. Faz 2’ye onay yok.

**Biten:** Faz 0 klasörleri. Tag `Balon` projede duruyor.

**Eksik:** `SepetOyunu` sahnesi, balon, rozet, dört pozisyon ve `Obje.prefab`.

**Sıradaki tek iş:** Gelen arkaplan görselini `Assets/_Art/` içine koy. Hierarchy’de `Arkaplan` objesi oluştur, Sprite alanına o görseli sürükle. Kameranın gördüğü alanı kaplasın. **Order in Layer** = `-10`. Bitince “Arkaplanı koydum” yaz.

---

## Katı kurallar

1. Bir fazın **Tamamlandı kontrol listesi** tamamen işaretlenmeden ve faz çalışır durumdayken Enes’in açık onayı gelmeden sonraki faza geçme. Faz bitince sıradaki fazı öner; onaysız başlama.
2. Kod üretildikten sonra o fazın **Unity Editor** bölümünü bitirmeden fazı kapatma.
3. Paket 3 (Mustafa Yiğit Avan) bu projede hazır olmadan **Faz 4** (`SepetOyunYoneticisi`) yazılmaz.
4. `SepetHareketi.cs` yazma. Yerine `ObjeSurukleme.cs`.
5. Bu sahnede avatar gösterme.
6. Cursor’a bir seferde tek script ver. Yönetici için önce Plan Mode.
7. Checklist kutularını agent günceller (`AGENTS.md` → İlerleme takibi).
8. “Ne durumdayım?” sorusuna `AGENTS.md` yönlendirici cevap şablonuyla yanıt ver.

---

## Onaylanmış kararlar

| Konu | Karar |
| ---- | ----- |
| Mekanik | Balon sabit. Altta 4 pozisyon. Objeler balona sürüklenir. |
| Scriptler | `ObjeSurukleme.cs` + `SepetOyunYoneticisi.cs` |
| Giriş | Fare + touch/parmak |
| Yanlış bırakma | Geri bildirim + obje başlangıç pozisyonuna döner |
| Süre | Varsayılan **60** saniye (`kalanSure`) |
| Doğru puan | Varsayılan **+10** (`dogruPuan`) |
| Harf rozeti | Dünya uzayı `TextMeshPro` (`harfRozeti`). UI `TextMeshProUGUI` hedef tip değil. |
| Sahne yolu | `Assets/Scenes/SepetOyunu.unity` |
| Paket 3 | Bitmeden oyun yöneticisi yazılmaz |
| Avatar | Bu sahnede yok |

Kılavuzdaki `Assets/_Scenes/` yolu bu projede kullanılmıyor. Sahneler `Assets/Scenes/` altında.

---

## Faz akışı

```
Faz 0 (Hazırlık)          bitti
  → Faz 1 (Sahne)         yeniden kurulacak, sahne yok
    → Faz 2 (ObjeSurukleme)   onay yok, script yok
      → Faz 3 (Paket 3 kapısı)   bekleniyor
        → Faz 4 (SepetOyunYoneticisi)
          → Faz 5 (Bağlama + test)
            → Faz 6 (Teslim)
```

Kılavuz eşlemesi: Adım 4.1–4.3 = Faz 1, Adım 4.4 = Faz 2, Adım 4.5 = Faz 4, Adım 4.6 = Faz 5.

---

## Asset envanteri (Faz 0)

| Asset | Bu projedeki durum |
| ----- | ------------------ |
| Arkaplan (Mini Oyun 1) | Yok. `Assets/_Art/` bu kopyada yok. |
| Balon görseli | Yok. |
| Obje sprite’ları | Yok. Prefab da yok. |
| Avatar görselleri | Yok. Paket 1 işi; bu sahnenin parçası değil. |
| Ses | Yok. `Assets/_Audio/` bu kopyada yok. Paket 6 bağlar. |

---

# Faz 0 — Hazırlık

**Amaç:** Klasörler ve asset notu hazır olsun.

## Tamamlandı kontrol listesi

- [x] `AGENTS.md` bu çalışma kopyasının kökünde duruyor
- [x] Asset envanteri yukarıya yazıldı
- [x] `Assets/_Scripts/SepetOyunu/` var
- [x] `Assets/_Prefabs/SepetOyunu/` var
- [x] Sahne klasörü var: `Assets/Scenes/`

**Çıkış:** Klasörler hazır. → Faz 1

---

# Faz 1 — Sahne kurulumu (Adım 4.1–4.3)

**Amaç:** `SepetOyunu` sahnesi mekanik teste hazır olsun. Bu fazda yeni C# yazılmaz.

## Repoda görülenler

| Öğe | Durum |
| --- | ----- |
| Sahne | Yok. `Assets/Scenes/SepetOyunu.unity` silindi. |
| Prefab | Yok. `Obje.prefab` silindi. |
| Tag | `Balon` Tag Manager’da duruyor. |
| Build Settings | `SepetOyunu` listede yok. Faz 5’te eklenecek. |

## Unity Editor — baştan

1. **File > New Scene**. **File > Save As…** → `Assets/Scenes/SepetOyunu.unity`. Main Camera **Orthographic** olsun.
2. `Arkaplan` objesine gelen arkaplan görselini koy. Kameranın gördüğü alanı kaplasın. **Order in Layer** = `-10`.
3. `Balon` sprite’ı üst-orta bölgeye koy (örnek `y` yaklaşık `2`). **Box Collider 2D** ekle, **Is Trigger** işaretle, Tag = `Balon`. Collider görsele otursun.
4. Balon’a **3D Object > Text - TextMeshPro** ekle, adı `HarfRozeti`. Yazı `A`, balonun üstünde okunaklı dursun. UI Canvas kullanma.
5. `Obje` oluştur: Sprite Renderer + **Box Collider 2D** (trigger kapalı). `Assets/_Prefabs/SepetOyunu/Obje.prefab` olarak kaydet. Bu fazda script ekleme.
6. Boş objeler: `ObjePozisyon1` … `ObjePozisyon4`. Ekranın altına yan yana koy.
7. Play’e bas. Arkaplan, balon ve rozet görünsün. Play’den çık. Sahneyi kaydet.

Görsel gereken adımda gerçek dosyayı kullan. Görsel gerekmeyen adımda Sprite atama.

## Tamamlandı kontrol listesi

- [x] `Assets/Scenes/SepetOyunu.unity` kayıtlı
- [ ] Arkaplan `Order in Layer = -10`
- [ ] Arkaplan kamera çerçevesini kaplıyor
- [ ] `Balon` üzerinde Box Collider 2D + **Is Trigger**
- [ ] Tag `Balon` atanmış (`ProjectSettings/TagManager.asset`)
- [ ] Balon üst-orta bölgede, collider görsele oturuyor
- [ ] `HarfRozeti` dünya uzayı `TextMeshPro` ve balonun üstünde görünür
- [ ] Prefab `Assets/_Prefabs/SepetOyunu/Obje.prefab` var
- [ ] Prefab’da Box Collider 2D **trigger değil**
- [ ] `ObjePozisyon1`…`4` altta duruyor
- [ ] Play’de görsel kontrol yapıldı ve sahne kaydedildi

**Çıkış:** Sahne iskeleti gözle doğrulandı. → Faz 2 Editor testi

---

# Faz 2 — ObjeSurukleme.cs (Adım 4.4)

**Amaç:** Objeyi fare veya parmakla sürükle. Balona yakın bırakınca log yaz. Uzaksa başlangıca dön.

**Bağımlılık:** Paket 3 gerekmez. `SepetOyunYoneticisi` bu fazda çağrılmaz.

## Kodda duran davranış

Dosya: `Assets/_Scripts/SepetOyunu/ObjeSurukleme.cs`

| Alan | Tip | Prefab’daki değer |
| ---- | --- | ----------------- |
| `objeAdi` | `string` | `Ayva` |
| `dogruHarf` | `char` | `A` (kayıt değeri 65) |
| `birakmaMesafesi` | `float` | `1.5` |

- `Start` başlangıç pozisyonunu saklar, `Collider2D` yoksa hata loglar.
- `Update` içinde basma / basılı tutma / bırakma hem touch hem fare ile okunur.
- İşaretçi dünya konumu `Camera.main.ScreenToWorldPoint` ile alınır, `z = 0` yapılır.
- Bırakınca tag `Balon` aranır. Mesafe `birakmaMesafesi` içindeyse `Debug.Log("Balona birakildi: " + objeAdi)`. Değilse obje başlangıca döner.
- Yönetici çağrısı yok. Bu, Faz 4’e kadar bilinçli durumdur.

## 2.C — Unity Editor (kalan)

1. `SepetOyunu` sahnesinde `Obje` prefab’ından **1** instance sürükle.
2. Onu `ObjePozisyon1` konumuna koy (`-4, -3, 0`).
3. **Play:**
   - Obje fareyi takip etmeli.
   - Balondan uzağa bırakınca başladığı yere dönmeli.
   - Balona yakın bırakınca Console’da `Balona birakildi: Ayva` yazmalı. Obje yok olmamalı.
4. Play’den çık. Sahneyi kaydet.
5. Test instance’ı sahnede kalabilir. Faz 4 spawn’ı kendi objelerini üretecek; kalıcı test kopyası Faz 5’te kaldırılır.

## Tamamlandı kontrol listesi

- [] `ObjeSurukleme.cs` yazıldı; public alanlar kılavuzla aynı
- [] Touch ve fare yolu kodda var
- [] Yakın bırakma yalnızca log yazar (yönetici çağrısı Faz 4)
- [] Prefab’a script ekli; `objeAdi`, `dogruHarf`, `birakmaMesafesi` dolu
- [ ] Sahnede en az 1 Obje instance’ı var
- [ ] Unity Console’da bu script için kırmızı error yok
- [ ] Play: sürükleme çalışıyor
- [ ] Play: uzak bırakınca geri dönüyor
- [ ] Play: balona yakın bırakınca Console log geliyor

**Çıkış:** Sürükle-bırak tek başına doğrulandı. → Faz 3

---

# Faz 3 — Paket 3 checkpoint

**Amaç:** `SepetOyunYoneticisi` yazmadan önce ortak sistemlerin bu projede durduğunu doğrula. Bu fazda yeni oyun kodu yazılmaz.

Sorumlu: **Mustafa Yiğit Avan**. Eksikse bekle. Bu projede `Assets/_Scripts/Ortak/` klasörü yoktur.

## Kontrol edilecekler

### 3.1 Veri

- [ ] `Assets/_Scripts/Ortak/HarfObjeVerisi.cs` var
- [ ] `Assets/_Data/Harfler/` altında harf kartları var (hedef: A, E, I, İ, O, Ö, U, Ü)
- [ ] En az bir kartta `harf` ve `dogruObjeler` (2+ sprite) dolu

### 3.2 Script API’leri

- [ ] `SoruSecici` — `HarfObjeVerisi[] tumHarfler`, `YeniHarfSec()`
- [ ] `SkorYoneticisi` — `SkorEkle(int miktar)`, skor yazısı alanı
- [ ] `GeriBildirimYoneticisi` — `DogruGoster()`, `YanlisGoster()`

### 3.3 Sahneye taşınabilirlik

- [ ] Bu bileşenler `SepetOyunu` sahnesine eklenebilir durumda

## Tamamlandı kontrol listesi

- [ ] Yukarıdaki maddeler işaretli
- [ ] Eksikler bu dosyaya not edildi

**Çıkış şartı:** Bu liste kapanmadan Faz 4’e geçme.

---

# Faz 4 — SepetOyunYoneticisi.cs (Adım 4.5)

**Amaç:** Oyunun beyni: süre, harf rozeti, 4 pozisyona obje üretimi, bırakma kararı, oyun bitişi.

**Ön koşul:** Faz 2 Play testi geçti ve Faz 3 kapandı.

## 4.A — Önce Plan Mode

Cursor’a kod yazdırmadan:

> Unity için `SepetOyunYoneticisi` yazacağım. Paket 4 Mini Oyun 1. Mekanik: balon sabit; altta 4 pozisyona Instantiate; ObjeSurukleme bırakınca ObjeBirakildi çağırır. Görevler: 60 sn geri sayım, süre bitince OyunuBitir; SoruSecici ile harf seçip harfRozeti’ne yaz; ObjeleriOlustur’da 1–2 doğru + 2–3 çeldirici karıştırıp 4 pozisyona prefab Instantiate; doğruda skor +10, DogruGoster, YeniTurBaslat; yanlışta sadece YanlisGoster (obje kendi script’inde döner). Avatar yok. Singleton Instance. Kod yazmadan adım adım plan anlat.

Planı oku, onayla, sonra kod.

## 4.B — Script görev tanımı

**Dosya:** `Assets/_Scripts/SepetOyunu/SepetOyunYoneticisi.cs`

**Public alanlar (isimler sabit):**

| Alan | Tip | Varsayılan |
| ---- | --- | ---------- |
| `soruSecici` | `SoruSecici` | Inspector |
| `skorYoneticisi` | `SkorYoneticisi` | Inspector |
| `geriBildirim` | `GeriBildirimYoneticisi` | Inspector |
| `objePrefab` | `GameObject` | `Obje.prefab` |
| `objePozisyonlari` | `Transform[]` | 4 eleman |
| `harfRozeti` | `TextMeshPro` | dünya uzayı TMP |
| `sureYazisi` | `TextMeshProUGUI` | UI |
| `kalanSure` | `float` | **60** |
| `dogruPuan` | `int` | **10** |

**Fonksiyonlar:**

- `YeniTurBaslat()` — harf seç, rozeti güncelle, eski objeleri temizle, `ObjeleriOlustur`
- `ObjeleriOlustur()` — doğru + çeldirici seç, karıştır, Instantiate, `ObjeSurukleme` alanlarına sprite / `objeAdi` / `dogruHarf` ata
- `ObjeBirakildi(ObjeSurukleme obje)` — `obje.dogruHarf` aktif harfle aynıysa skor + `DogruGoster` + yeni tur; değilse yalnızca `YanlisGoster`
- `OyunuBitir()` — sayacı durdur, sahnedeki spawn objelerini yok et

**Update:** oyun aktifken `kalanSure -= Time.deltaTime`; `sureYazisi` güncelle; `0` ve altında `OyunuBitir`.

**Singleton:** `static Instance`, `Awake` içinde ata.

## 4.C — Kod prompt’u (plan onayından sonra)

> Planı onayladım. `SepetOyunYoneticisi` C# kodunu yaz. AGENTS.md’ye uy. Public alanlar: soruSecici, skorYoneticisi, geriBildirim, objePrefab, Transform[] objePozisyonlari, TextMeshPro harfRozeti, TextMeshProUGUI sureYazisi, float kalanSure=60f, int dogruPuan=10. Singleton Instance. Start’ta YeniTurBaslat. Update’te süre. ObjeleriOlustur: aktif harfin dogruObjeler’inden 1–2, diğer harflerden 2–3 çeldirici, karıştır, pozisyonlara Instantiate et, her ObjeSurukleme’ye sprite/objeAdi/dogruHarf ata. ObjeBirakildi: doğruysa SkorEkle(dogruPuan), DogruGoster, YeniTurBaslat; yanlışsa YanlisGoster. OyunuBitir süre bitince. Avatar kodu yok. Her satıra Türkçe yorum.

## 4.D — ObjeSurukleme bağını güncelle

`BirakmayiIsle` içindeki başarılı log satırını şuna çevir:

- `SepetOyunYoneticisi.Instance != null` iken `Instance.ObjeBirakildi(this);`
- null ise mevcut `Debug.Log` kalsın

## Tamamlandı kontrol listesi

- [ ] Plan Mode çıktısı okundu ve onaylandı
- [ ] `SepetOyunYoneticisi.cs` Console’da error vermeden duruyor
- [ ] Public alan isimleri tablodakiyle aynı
- [ ] `kalanSure` varsayılan 60, `dogruPuan` 10
- [ ] `ObjeSurukleme` yöneticiyi çağırıyor
- [ ] Avatar referansı yok

**Çıkış:** Kod hazır. → Faz 5

---

# Faz 5 — Bağlama ve entegrasyon (Adım 4.6)

**Amaç:** Inspector bağları dolu olsun. Play’de tam tur dönsün.

## Unity Editor

### 5.1 Oyun yöneticisi

1. `SepetOyunu` sahnesini aç.
2. Create Empty, adı `OyunYoneticisi`.
3. `SepetOyunYoneticisi` ekle.

### 5.2 UI — süre ve skor

1. Ayrı bir **Screen Space** Canvas oluştur. Balon’un çocuğu yapma.
2. Canvas Scaler: **Scale With Screen Size**, Reference Resolution `1920 x 1080` (mobil + akıllı tahta).
3. Canvas altına TextMeshPro: `SureYazisi` (sağ üst).
4. Paket 3’teki `SkorYazisi` + `SkorYoneticisi` ve geri bildirim objelerini (`DogruIkon`, `YanlisIkon`, parçacık, ses) bu sahneye koy.

### 5.3 Ortak sistemler

1. `SoruSecici` ekle. `tumHarfler` dizisini harf kartlarıyla doldur.
2. `SkorYoneticisi` ve `GeriBildirimYoneticisi` referanslarını hazırla.

### 5.4 Inspector

`OyunYoneticisi` üzerinde:

1. `soruSecici`
2. `skorYoneticisi`
3. `geriBildirim`
4. `objePrefab` → `Assets/_Prefabs/SepetOyunu/Obje.prefab`
5. `objePozisyonlari` Size = 4 → dört pozisyon
6. `harfRozeti` → Balon’daki dünya TMP
7. `sureYazisi` → Canvas’taki UI yazı
8. `kalanSure` = 60, `dogruPuan` = 10

Faz 2’de elle koyduğun test Obje instance’ını sil. Tur başında spawn edecek olan yönetici.

### 5.5 Build Settings

**File > Build Settings** içinde `SepetOyunu` listede olsun.

### 5.6 Play test

- [ ] Oyun başında harf rozetinde bir harf görünüyor
- [ ] Altta 4 obje oluşuyor (doğru + çeldirici)
- [ ] Doğru objede puan **+10**, doğru geri bildirim, yeni tur
- [ ] Yanlış objede yanlış geri bildirim; obje eski yerine dönüyor; tur değişmiyor
- [ ] Süre 60’tan geri sayıyor; `SureYazisi` güncelleniyor
- [ ] Süre 0 olunca oyun duruyor
- [ ] Aynı harf arka arkaya gelmiyor
- [ ] Console’da kırmızı error yok

## Tamamlandı kontrol listesi

- [ ] Inspector referanslarında None yok
- [ ] Play test listesinin tamamı geçti
- [ ] Sahne kayıtlı
- [ ] Build Settings’te `SepetOyunu` var

**Çıkış:** Mini Oyun 1 oynanabilir. → Faz 6

---

# Faz 6 — Teslim (Paket 6)

**Amaç:** Süleyman Öz’e entegrasyon notunu bırak.

```
Mini Oyun 1 (Enes) teslim
- Sahne: Assets/Scenes/SepetOyunu.unity
- Scriptler: ObjeSurukleme.cs, SepetOyunYoneticisi.cs
- Prefab: Assets/_Prefabs/SepetOyunu/Obje.prefab
- Süre varsayılan 60 sn, doğru +10
- Avatar bu sahnede yok
- SepetHareketi YOK (iptal)
- HarfRozeti dünya uzayı TextMeshPro

Paket 6’dan beklenenler:
- [ ] Font’un skor / süre / HarfRozeti’ne uygulanması
- [ ] Ses clip’lerinin GeriBildirim / SesYoneticisi alanlarına bağlanması
- [ ] Avatar seçim ekranından SepetOyunu’na SahneGecisi butonu
- [ ] Build Settings son kontrolü
```

## Tamamlandı kontrol listesi

- [ ] Faz 0–5 listeleri bu dosyada işaretli
- [ ] Teslim notu Süleyman’a iletildi
- [ ] Bilinen sorunlar aşağıya yazıldı

**Bilinen sorunlar (28 Eylül 2026):**

- Mini Oyun 1 grafik ve ses dosyaları yok.
- Faz 1 sahnesi ve `Obje.prefab` geri alındı. Enes yeniden kuracak.
- `SepetOyunu` Build Settings’te yok. Faz 5’te eklenecek.

---

## Hızlı referans

| Ne | Yol |
| -- | --- |
| Sürükleme | `Assets/_Scripts/SepetOyunu/ObjeSurukleme.cs` |
| Yönetici (henüz yok) | `Assets/_Scripts/SepetOyunu/SepetOyunYoneticisi.cs` |
| Prefab | `Assets/_Prefabs/SepetOyunu/Obje.prefab` |
| Sahne | `Assets/Scenes/SepetOyunu.unity` |
| Ekip kuralları | `AGENTS.md` |
| Görev kaynağı | `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` Paket 4 |

## İptal

| Dosya | Durum |
| ----- | ----- |
| `SepetHareketi.cs` | Yazılmaz. Balon sabit; sürüklenen objelerdir. |
