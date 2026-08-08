# MUSTAFA_UZ_MINI_OYUN_2.md — Mustafa Üz · Mini Oyun 2 (Hangisinin Baş Harfi)

Bu dosya **sadece Mustafa Üz** için faz kapılı çalışma planıdır. Kod yazmadan önce proje kökündeki `AGENTS.md` dosyasını okuyun. Genel görev özeti: `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` → Paket 5.

---

## Katı kurallar

1. Bir fazın **Tamamlandı kontrol listesi** tamamen işaretlenmeden sonraki faza geçme.
2. Kod üretildikten sonra o fazın **Unity Editor** bölümünü bitirmeden fazı kapatma.
3. Paket 3 (Mustafa Yiğit Avan) bitmeden **Faz 7’ye** (ortak sistem entegrasyonu) geçme.
4. Başkasının paket klasörüne (`Ortak/`, `Avatar/`, `SepetOyunu/`) script ekleme.
5. Ses clip üretimi / TTS / `SesYoneticisi` gövdesi **Paket 3**; partikül+ses asset bağlama finali **Paket 6**. Senin işin: sahnende UI/animasyon ve çağrı kancalarının hazır olması.
6. Cursor’a bir seferde tek script / tek odak ver; yöneticide büyük değişiklik için önce Plan Mode.
7. Bu planda “şimdilik kod/sahne kilidi” yok — yeni işe `AGENTS.md` + bu dosyadaki mevcut fazdan devam et.
8. Checklist’i çocuğun tek başına doldurmasını bekleme — `AGENTS.md` → **İlerleme takibi**: sık durum bildir, olgu sor, onaylara göre bu dosyadaki `[ ]` kutularını agent günceller.
9. “Ne durumdayım?” sorusuna `AGENTS.md` → **yönlendirici cevap** şablonuyla yanıt ver; karar aldırma, plandaki sıradaki **tek işi** yaptır.

---

## Onaylanmış kararlar

| Konu | Karar |
| ---- | ----- |
| Mekanik | Büyük harf sorulur; ekranda **4 seçenek**; 1 doğru + 3 çeldirici; tıklayarak seçim |
| Scriptler | `HarfSecmeYoneticisi.cs` + `SecenekBalonu.cs` |
| Sahne (güncel yol) | `Assets/Scenes/HarfSecmeOyunu.unity` |
| Giriş | Fare + touch/parmak (`Input System`) |
| Yanlış cevap | Geri bildirim; **aynı soru kalır** |
| Doğru cevap | Kısa uçuş / tik; sonra yeni soru |
| Süre | Varsayılan **60** saniye (`kalanSure`) |
| Bitiş ekranı | Sade ve çocuk dostu: tebrik + 1–3 yıldız (daire) + `Doğru: N` + Ana Menü — harf rapor listesi **yok** |
| Hedef cihazlar | **Mobil** ve **Windows akıllı tahta** (büyük ekran) — UI ölçeği kritik |
| Balon girişi | Anında belirme yok; **aşağıdan yukarı** yükseliş; hızlar hafif rastgele farklarla |
| Seslendirme | Çocuklar okumadığı için soru + obje adı sesleri gerekir → clip/API **Paket 3**; Paket 5 yalnızca güvenli çağrı noktası bırakır |
| Paket 3 | Bitmeden ortak skor / soru seçici / geri bildirim yöneticisine tam bağlanılmaz |

---

## Mevcut durum özeti (2026-08)

**Çalışıyor / büyük ölçüde hazır:**

- `HarfSecmeYoneticisi` + `SecenekBalonu` scriptleri
- Sahne + art (harf görselleri, 8 obje balonu, tik/çarpı, arkaplan)
- Her turda 1 doğru + 3 çeldirici, karıştırma, 4 slota yerleştirme
- Tıklama (mouse/touch), süre, sade bitiş ekranı, Ana Menü (`SceneManager`)
- Build Settings’te `HarfSecmeOyunu` kayıtlı
- Inspector alanları + bulunamazsa `AutoDoldur` (isimle bulma)

**Eksik / sonraki fazlar:**

- Soru harfi ve üst UI’nin arka plan üzerinde **okunabilirliği**
- UI punto / renk / ölçek — çocuklara canlı; mobil + akıllı tahta
- Seçenek balonlarının **aşağıdan yükselerek** girmesi
- Prefab klasörü boş; kılavuzdaki UI Button prefab modeline tam geçilmedi (world sprite modeli kullanılıyor — bilinçli mevcut durum)
- Paket 3 API bağları (`SoruSecici`, `SkorYoneticisi`, `GeriBildirimYoneticisi`, `SesYoneticisi`) henüz yok

**Bilinen notlar (bug / teknik borç):**

- Süre/skor yazısı runtime `Legacy Text` ile üretiliyor; TextMeshPro + sabit Canvas hedeflenmeli
- Yıldızlar UI `Image` kareleri (gerçek yıldız sprite’ı yok) — Faz 4’te görsel polish
- `Ana Menü` hedefi `anaMenuSahneAdi` (varsayılan `AbdullahScene`) — menü sahnesi netleşince güncelle
- Inspector’da `secenekler` / slotlar elle bağlanmadan `AutoDoldur`’a güveniliyor; teslimde mümkünse elle bağla

---

## Faz akışı

```
Faz 0 (Hazırlık)                          [büyük ölçüde tamam]
  → Faz 1 (Sahne + art — kod yok)         [büyük ölçüde tamam]
    → Faz 2 (SecenekBalonu + tıklama)     [tamam]
      → Faz 3 (Yönetici temel + bitiş)    [tamam]
        → Faz 4 (UI okunabilirlik + ölçek)        ← sıradaki odak
          → Faz 5 (Balon yükseliş animasyonu)
            → Faz 6 (Paket 3 checkpoint)
              → Faz 7 (Ortak entegrasyon + ses kancaları)
                → Faz 8 (Teslim)
```

---

# Faz 0 — Hazırlık

**Amaç:** Ortam ve asset’leri kontrol et; klasörleri doğrula.

## Yapılacaklar

1. `AGENTS.md` dosyasını baştan sona oku.
2. Bu dosyayı (`MUSTAFA_UZ_MINI_OYUN_2.md`) oku.
3. Asset envanteri:
   - Arkaplan (Kapadokya / balon teması)
   - 8 harf soru görseli (A E Ö Ü I İ O U)
   - 8 seçenek balonu sprite’ı
   - Doğru / yanlış feedback görselleri
4. Klasörler:
   - `Assets/_Scripts/HarfSecmeOyunu/`
   - `Assets/_Art/HarfSecmeOyunu/`
   - `Assets/_Prefabs/HarfSecmeOyunu/` (şu an boş olabilir — not et)
   - Sahne: `Assets/Scenes/HarfSecmeOyunu.unity`

## Tamamlandı kontrol listesi

- [x] `AGENTS.md` okundu
- [x] Script klasörü var
- [x] Art klasörü + temel sprite’lar var
- [x] Sahne dosyası var
- [ ] Prefab klasöründe en az bir seçenek şablonu (isteğe bağlı; world-sprite modelinde zorunlu değil — boşsa not düş)
- [x] Asset envanteri bu dosyaya işlendi (yukarıdaki madde 3)

**Çıkış:** Hazırlık net. → Faz 1

---

# Faz 1 — Sahne + art iskeleti (kod yok)

**Amaç:** Oynanabilir sahne görselleri yerinde. Bu fazda yeni C# yazılmaz.

## Mevcut sahne kontrolü (Unity Editor)

1. `HarfSecmeOyunu` sahnesini aç.
2. Arkaplan, 8 balon, 8 soru harfi GO, tik/çarpı feedback’ler Hierarchy’de mi bak.
3. Main Camera Orthographic mi kontrol et.
4. Play olmadan Scene/Game view’da dört slot bölgesinin ekranı kapladığını not et (mobil/tahta için Faz 4’te yeniden ölçeklenecek).

## Tamamlandı kontrol listesi

- [x] `Assets/Scenes/HarfSecmeOyunu.unity` kayıtlı
- [x] Arkaplan sahnede
- [x] Seçenek balon sprite’ları sahnede
- [x] Soru harfi görselleri sahnede
- [x] Doğru/yanlış feedback görselleri (çoğu) sahnede
- [ ] Dört sabit slot için boş `Transform` referans objeleri (`SecenekPozisyon1`…`4`) Hierarchy’de net isimlendirildi (yoksa Faz 4’te ekle)

**Çıkış:** Sahne art iskeleti. → Faz 2

---

# Faz 2 — SecenekBalonu.cs

**Amaç:** Tek seçenek; tıklanınca doğru/yanlış sonucunu yöneticiye bildirir.

**Dosya:** `Assets/_Scripts/HarfSecmeOyunu/SecenekBalonu.cs`

## Public / beklenen API

| Alan / fonksiyon | Not |
| ---------------- | --- |
| `objeAdi` | string |
| `objeResmi` | `Image` (kılavuz; UI yolu kullanılırsa) |
| `Ayarla(...)` | Kimlik + harf + feedback + slot |
| `Ayarla(Sprite, char, char)` | Kılavuz imzası |
| `SoruHarfiniGuncelle(char)` | Aktif soru harfi |
| `Tiklandi()` | `HarfSecmeYoneticisi.Instance.SecenekSecildi(...)` |
| `TiklanabilirYap()` | Collider |
| `NoktaSpriteIcinde` | Collider kaçırırsa yedek isabet |

## Tamamlandı kontrol listesi

- [x] `SecenekBalonu.cs` compile oluyor
- [x] Tıklanınca yöneticiye sonuç bildiriliyor
- [x] World-space sprite + collider yolu çalışıyor
- [x] `objeResmi` alanı tanımlı (UI’ye geçilirse kullanılacak)

**Çıkış:** Seçenek birimi hazır. → Faz 3

---

# Faz 3 — HarfSecmeYoneticisi temel akış + bitiş ekranı

**Amaç:** Soru seç, 4 seçenek yerleştir, süre, doğru/yanlış süreçleri, sade bitiş.

**Dosya:** `Assets/_Scripts/HarfSecmeOyunu/HarfSecmeYoneticisi.cs`

## Temel davranış (mevcut)

- Singleton `Instance`
- `YeniSoru()` — desteden harf; 1 doğru + 3 çeldirici; Fisher-Yates; 4 slot
- `SecenekSecildi(bool)` + `SecenekSecildi(bool, SecenekBalonu)`
- Süre `kalanSure`; bitince `OyunuBitir`
- Bitiş: tebrik + 1–3 işaret + `Doğru: N` + Ana Menü (harf rapor listesi yok)
- Paket 3 olmadan yerel skor sayacı kullanılabilir

## Tamamlandı kontrol listesi

- [x] Temel soru / seçenek / tıklama akışı Play’de çalışıyor
- [x] 60 sn süre ve süre yazısı (geçici UI kabul)
- [x] Doğru → uçuş + yeni soru; yanlış → aynı soru
- [x] Bitiş ekranı sade (rapor listesi kaldırıldı)
- [x] Ana Menü sahne yüklemesi bağlı
- [x] Build Settings’te `HarfSecmeOyunu` var
- [ ] `codes` / yönetici objesinde Inspector alanları mümkün olduğunca elle dolu (`AutoDoldur` yedek kalsın)

**Çıkış:** Oynanabilir çekirdek. → Faz 4

---

# Faz 4 — UI okunabilirlik + ölçekleme (mobil / akıllı tahta)

**Amaç:** “Hangisinin baş harfi” sorusu ve üst UI, meşgul arka plan üzerinde **net okunur**; punto/renk/layout **çocuklara canlı**; hem **küçük mobil** hem **büyük Windows tahta** ekranında bozulmaz.

**Paket sınırı:** Font asset üretimi / proje geneli font standardı Paket 6 ile netleşir; bu fazda sahnende okunabilirlik ve Canvas ölçeğini sen çözersin (geçici font da olur).

## 4.A — Tasarım hedefleri

1. Soru harfi (veya “Hangisinin baş harfi?” başlığı) arka plandan **ayrışır**: panel/çerçeve, gölge, daha büyük punto, yüksek kontrast.
2. Süre ve `Doğru` yazıları büyük, kalın, canlı renk; ince gri “sistem yazısı” hissi olmasın.
3. Canvas Scaler: **Scale With Screen Size**; Reference Resolution örn. **1920×1080**; Match değerini hem telefon hem tahta için Play Mode + mümkünse Device Simulator / farklı Game view çözünürlükleriyle dene.
4. Bitiş ekranı aynı ölçek/renk diline yaklaşsın (tebrik büyük, buton tok, yıldızlar gerçekçi veya canlı sprite).

## 4.B — Unity Editor (önce)

1. Hierarchy’de kalıcı **Canvas** oluştur (runtime’da her seferinde üretme hedefi).
2. Canvas Scaler ayarla (yukarıdaki gibi).
3. `SureYazisi`, skor/`Doğru` metni, isteğe bağlı başlık için TMP veya mevcut Text — **arkaplan üstünde panelli**.
4. Soru harfi sprite’ının arkasına yarı saydam / beyaz bubbly çerçeve (art ekibinden çerçeve varsa onu kullan; yoksa geçici Image panel).
5. Game view’da en az iki en-boy oranı dene: dar mobil (ör. 1080×1920) ve geniş tahta (ör. 1920×1080 veya daha geniş).

## 4.C — Kod (gerekirse)

- `sureYazisi` Inspector’dan atansın; runtime Canvas üretimini kaldır veya yalnızca yedek bırak.
- Renk / punto sabitlerini abartılı “sistem UI”den uzak tut; sahnedeki Canvas’a bırakmayı tercih et.
- Plan Mode: yalnızca yönetici UI bağları değişecekse kısa plan + onay.

## Tamamlandı kontrol listesi

- [ ] Soru harfi / başlık arka plan üzerinde rahat okunuyor (ekip içi göz kontrolü)
- [ ] Süre + Doğru yazıları büyük, canlı, panelli veya yüksek kontrastlı
- [ ] Canvas Scaler ayarlı; en az bir mobil + bir geniş çözünürlükte kontrol edildi
- [ ] Runtime’a bağımlı süre yazısı yerine (mümkünse) sahne Canvas referansı kullanılıyor
- [ ] Bitiş ekranı aynı dilde (büyük tebrik, net buton)
- [ ] Sahne kayıtlı

**Çıkış:** Okunabilir, ölçeklenen UI. → Faz 5

---

# Faz 5 — Balon (şık) giriş animasyonu

**Amaç:** Yeni turda seçenekler birden belirmez; **ekranın altından** hedef slot pozisyonuna yükselir. Hızlar **hafif rastgele** farklarla (çok uçurmadan) daha doğal görünür.

## 5.A — Davranış

1. `YeniSoru` seçenekleri yerleştirirken: başlangıç Y = slot Y − ofset (ekran altı).
2. Her balon kendi hızıyla (ör. baz hız ± küçük `Random.Range`) slot Y’ye çıkar.
3. Yükseliş bitene kadar tıklama kilidi (`islemYapiliyor` veya ayrı `girisAnimasyonu`) — animasyon ortasında yanlış tıklama olmasın.
4. Animasyon bitince tıklama açılır.

## 5.B — Cursor (Plan Mode önerilir)

> `HarfSecmeYoneticisi` içinde yeni soru seçeneklerini anında teleport etmek yerine aşağıdan yukarı coroutine/lerp ile yükselt. Her balona hafif rastgele hız farkı ver. Animasyon süresince tıklamayı kilitle. AGENTS.md + MUSTAFA_UZ planına uy. Kod yazmadan önce kısa plan anlat / onay sonrası uygula.

## Tamamlandı kontrol listesi

- [ ] Yeni turda balonlar aşağıdan yükseliyor
- [ ] Hızlar arasında hafif rastgele fark var (aşırı dağınık değil)
- [ ] Yükseliş bitmeden tıklama işlenmiyor
- [ ] Doğru cevap sonrası yeni tur animasyonu da tutarlı
- [ ] Play’de mobil/tahta ölçeğinde ofset abartılı kaçmıyor

**Çıkış:** Doğal giriş animasyonu. → Faz 6

---

# Faz 6 — Paket 3 checkpoint (kod yazma yok)

**Amaç:** Ortak sistemler gelmeden entegrasyon fazına geçme. **Bu fazda yeni oyun kodu yazılmaz.**

Sorumlu: **Mustafa Yiğit Avan**. Sen checklist doldurursun; eksikse bekle / hatırlat.

## Kontrol edilecekler

### 6.1 Veri

- [ ] `HarfObjeVerisi` ScriptableObject var
- [ ] Harf asset’leri (hedef: A, E, I, İ, O, Ö, U, Ü) ve `dogruObjeler` listeleri

### 6.2 Script API’leri (isimler kılavuza uymalı)

- [ ] `SoruSecici` — `tumHarfler`, `YeniHarfSec()`
- [ ] `SkorYoneticisi` — `SkorEkle(int)`
- [ ] `GeriBildirimYoneticisi` — `DogruGoster()`, `YanlisGoster()` (ikon + **partikül** + **ses** alanları)
- [ ] `SesYoneticisi` — fon müzik; ayrıca soru/obje seslendirme için **ileride** kullanılacak public API’nin varlığı / taslak imza mutabakatı

### 6.3 Mutabakat

- [ ] Yiğit ile “API kilitlendi” notu **veya** tüm kutular dolu

## Tamamlandı kontrol listesi

- [ ] Yukarıdakiler yeşil **veya** yazılı mutabakat var
- [ ] Eksikler bu dosyaya not edildi

**Çıkış şartı:** Yeşil olmadan Faz 7’ye geçme. → Faz 7

---

# Faz 7 — Ortak entegrasyon + ses kancaları

**Amaç:** Paket 3 bileşenlerine bağlan; ses/partikül için **null-safe çağrı noktaları** bırak. Clip üretmek / Inspector’a ses dosyası doldurmak senin final işin değil (Paket 3 + 6).

## 7.A — Yöneticiye eklenecek / bağlanacak alanlar (hedef isimler)

| Alan | Tip | Not |
| ---- | --- | --- |
| `soruSecici` | `SoruSecici` | Hardcode deste yerine |
| `skorYoneticisi` | `SkorYoneticisi` | Yerel `dogruSayisi` yerine veya yanında |
| `geriBildirim` | `GeriBildirimYoneticisi` | Tik/çarpı GO’larına ek / yerine `DogruGoster` / `YanlisGoster` |
| `sesYoneticisi` | `SesYoneticisi` | Soru değişince + (isteğe) seçenek tıklanınca — **Instance/alan null ise no-op** |

## 7.B — Ses kancası sözleşmesi (Paket 5 tarafı)

Çocuklar okumadığı için:

1. `YeniSoru` sonrası: soru sesi tetikle (ör. “Hangisinin baş harfi …?” / harf adı) — `sesYoneticisi != null` iken.
2. İsteğe bağlı: seçenek tıklanınca obje adı sesi — geri bildirimden önce veya birlikte; Yiğit’in API’sine uy.
3. Gerçek AI TTS clip dosyalarını **sen üretme**; sadece çağrıyı hazır tut.

## 7.C — Geri bildirim

- Doğru/yanlışta `geriBildirim.DogruGoster()` / `YanlisGoster()` çağır (null kontrolü).
- Sahne içi tik/çarpı ile çakışmayı planda seç: ya ortaka devret ya da ikisini kısa süre birlikte kullan (tercihen tek kaynak).

## 7.D — Cursor

Önce Plan Mode (yönetici büyük değişiklik). Onay sonrası tek oturumda entegrasyon.

## Unity Editor

1. Sahneye Paket 3 objelerini ekle / referansları sürükle.
2. Play: skor artıyor; doğru/yanlışta partikül+ses (clip’ler doluysa); clip boşsa hata fırlatmadan çalışıyor olmalı.

## Tamamlandı kontrol listesi

- [ ] Plan Mode onaylandı
- [ ] Inspector’da `soruSecici` / `skorYoneticisi` / `geriBildirim` / `sesYoneticisi` bağları var (veya bilinçli geçici null + log)
- [ ] Hardcode harf listesi yalnızca yedek veya kaldırıldı
- [ ] `YeniSoru` ses kancası null-safe
- [ ] Doğru/yanlışta `GeriBildirimYoneticisi` null-safe çağrılıyor
- [ ] Clip yokken Console’da kırmızı error yok

**Çıkış:** Ortak sistemlere bağlı oynanış. → Faz 8

---

# Faz 8 — Teslim (Paket 6’ya)

**Amaç:** Süleyman Öz’e entegrasyon notunu bırak; kendi checkbox’larını kapat.

## Teslim notu (kopyala-yapıştır)

```
Mini Oyun 2 (Mustafa Üz) teslim
- Sahne: Assets/Scenes/HarfSecmeOyunu.unity
- Scriptler: HarfSecmeYoneticisi.cs, SecenekBalonu.cs
- Mekanik: 4 şık, 1 doğru + 3 çeldirici, 60 sn
- UI: mobil + akıllı tahta ölçeği hedeflendi
- Balon giriş: aşağıdan yükseliş
- Bitiş: sade (tebrik + yıldız + Doğru + Ana Menü)
- Seslendirme: çağrı kancaları hazır; clip’ler Paket 3/6

Paket 6’dan beklenenler:
- [ ] Proje fontunun süre / skor / bitiş / başlık metinlerine uygulanması
- [ ] Ses clip’lerinin SesYoneticisi / GeriBildirim alanlarına bağlanması
- [ ] Avatar / menüden HarfSecmeOyunu’na SahneGecisi
- [ ] Build Settings son kontrolü (HarfSecmeOyunu + menü)
- [ ] Mobil + tahta çözünürlük smoke test
```

## Tamamlandı kontrol listesi

- [ ] Faz 0–7 ilgili maddeler bu dosyada işaretli
- [ ] Teslim notu Süleyman’a iletildi
- [ ] Bilinen bug’lar aşağıdaki bölüme yazıldı

**Bilinen sorunlar:**

- (yoksa “yok” yaz)

---

## Hızlı referans — dosya yolları

| Ne | Yol |
| -- | --- |
| Yönetici | `Assets/_Scripts/HarfSecmeOyunu/HarfSecmeYoneticisi.cs` |
| Seçenek | `Assets/_Scripts/HarfSecmeOyunu/SecenekBalonu.cs` |
| Sahne | `Assets/Scenes/HarfSecmeOyunu.unity` |
| Art | `Assets/_Art/HarfSecmeOyunu/` |
| Prefab (hedef) | `Assets/_Prefabs/HarfSecmeOyunu/` |
| Ekip kuralları | `AGENTS.md` |
| Bu plan | `MUSTAFA_UZ_MINI_OYUN_2.md` |

---

## Kapsam dışı (bu pakette yapılmaz)

| İş | Sahip paket |
| -- | ----------- |
| `HarfObjeVerisi` / `SoruSecici` / `SkorYoneticisi` script gövdesi | Paket 3 |
| AI ile soru/obje ses dosyası üretimi + `SesYoneticisi` çalma mantığı | Paket 3 |
| `GeriBildirimYoneticisi` partikül/ses component gövdesi | Paket 3 |
| Proje genel font atlas / tüm sahnelerde font unify | Paket 6 |
| Menü ↔ oyun `SahneGecisi` buton ağı finali | Paket 6 |
| Avatar’ın bu sahnede zorunlu görünmesi | Paket 1 + 6 (şu an zorunlu değil) |

> Plan notu (paket planı yazılırken): Paket 3’e “harf sorusu + obje adı seslendirme API’si”; Paket 6’ya “clip bağlama + çoklu çözünürlük smoke test” maddeleri eklenmeli.
