# SAID_AVATAR.md — Mustafa Said Bayram · Paket 1 (Avatar)

Bu dosya **sadece Mustafa Said Bayram** için faz kapılı çalışma planıdır. Kod yazmadan önce proje kökündeki `AGENTS.md` dosyasını okuyun. Genel görev özeti: `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` → Paket 1.

**Branch:** `said/avatar` (bu branch’te çalış; bitince `main`’e PR/merge)

---

## Katı kurallar

1. Bir fazın **Tamamlandı kontrol listesi** tamamen işaretlenmeden sonraki faza geçme.
2. Kod üretildikten sonra o fazın **Unity Editor** bölümünü bitirmeden fazı kapatma.
3. Başkasının paket klasörüne (`Ortak/`, `SepetOyunu/`, `HarfSecmeOyunu/`) script ekleme.
4. `public` alan / fonksiyon isimleri kılavuzdaki Türkçe isimlerle sabittir (`RenkSec`, `AksesuarSec`, `govdeOlcekCarpani`…).
5. Checklist’i çocuğun tek başına doldurmasını bekleme — `AGENTS.md` → **İlerleme takibi**.
6. Her faz çıkışında agent **commit + push** hatırlatır (`AGENTS.md` → Commit + push). Çocuk “pushladım” deyince ilgili kutu `[x]` yapılır.
7. “Ne durumdayım?” sorusuna `AGENTS.md` şablonuyla yanıt ver; karar aldırma, sıradaki **tek işi** yaptır.

---

## Onaylanmış kararlar

| Konu | Karar |
| ---- | ----- |
| Branch | `said/avatar` |
| Scriptler | `AvatarYoneticisi`, `AvatarGorunumu`, `AvatarSecimEkrani`, `RenkSecici`, `AksesuarSecici` (+ `AvatarSahneKurucu`) |
| Sahne | `Assets/Scenes/AvatarOlusturmaEkrani.unity` |
| Renk | `RenkSecici` + `AvatarYoneticisi.RenkSec` — gövde tint |
| Karakter ölçü | Her hayvan için ayrı `govdeOlcekCarpani` (+ zemin hizası) |
| Aksesuar ölçü/konum | Her aksesuar `olcek` / `yerelOfset` + hayvan `aksesuarOfset` |
| Hedef cihazlar | Mobil + Windows akıllı tahta |

---

## Mevcut durum özeti (2026-09)

- Çekirdek scriptler + renk + aksesuar seçimi **kodda hazır**; sahne kısmen bağlı.
- `govdeOlcekCarpani` değerleri çoğunlukla **1** — karakter bazlı ince ayar Faz 6.
- Arka plan sprite / sahne cilası Faz 4; UI düzeni Faz 5.
- Aksesuar butonlarında index 8–10 eksik olabilir → Faz 5.

**Sıradaki odak:** Faz 4 — arka plan + sahne tasarımı.

---

## Faz akışı

```
Faz 0 (Hazırlık)                         [tamam]
  → Faz 1 (Çekirdek scriptler)           [tamam]
    → Faz 2 (Renk seçimi)                [tamam]
      → Faz 3 (Aksesuar seçimi)          [büyük ölçüde tamam]
        → Faz 4 (Arka plan + sahne)              ← sıradaki
          → Faz 5 (UI düzeni)
            → Faz 6 (Karakter ölçü/konum)
              → Faz 7 (Aksesuar uyumu)
                → Faz 8 (Teslim → main)
```

---

# Faz 0 — Hazırlık

**Amaç:** Ortam ve asset’leri kontrol et.

## Tamamlandı kontrol listesi

- [x] `AGENTS.md` okundu
- [x] `Assets/_Scripts/Avatar/` var
- [x] Karakter / aksesuar art klasörleri var
- [x] Sahne yolu biliniyor: `Assets/Scenes/AvatarOlusturmaEkrani.unity`
- [ ] Commit + push bu faz için (geçmiş işler `main`’de olabilir — yeni branch ilk push’ta yapılır)

**Çıkış:** → Faz 1

---

# Faz 1 — Çekirdek scriptler

**Amaç:** Avatar hafıza + görünüm + seçim ekranı.

## Tamamlandı kontrol listesi

- [x] `AvatarYoneticisi.cs` (Singleton, `DontDestroyOnLoad`, PlayerPrefs)
- [x] `AvatarGorunumu.cs` (`Guncelle`, hayvan/aksesuar listeleri)
- [x] `AvatarSecimEkrani.cs` (ileri/geri, `DevamEt`)
- [x] Sahneye temel bağlar mevcut
- [ ] Commit + push yapıldı (`said/avatar`)

**Çıkış:** → Faz 2

---

# Faz 2 — Renk seçim sistemi

**Amaç:** Renk butonları gövde rengini değiştirir.

## Tamamlandı kontrol listesi

- [x] `RenkSecici.cs` var
- [x] `AvatarYoneticisi.RenkSec(Color)` çalışıyor
- [x] Sahnede en az 4 renk butonu bağlı
- [ ] Play’de renk değişimi teyit edildi
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 3

---

# Faz 3 — Aksesuar seçimi

**Amaç:** Aksesuar tak/çıkar; sprite’lar bağlı.

## Tamamlandı kontrol listesi

- [x] `AksesuarSecici.cs` var
- [x] `AksesuarSec(-1)` ile aksesuar yok
- [x] Aksesuar sprite’ları `AvatarGorunumu` listesinde
- [ ] Tüm aksesuarlar için UI butonu var (eksik index’ler Faz 5)
- [ ] Play’de aksesuar değişimi teyit edildi
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 4

---

# Faz 4 — Arka plan ve sahne tasarımı

**Amaç:** Avatar seçim ekranı görsel olarak tamamlanır.

## Yapılacaklar (Unity Editor)

1. Hierarchy’de `ArkaPlan` (veya eşdeğeri) Sprite Renderer / UI Image.
2. Uygun arka plan sprite’ını ata; kamera / Order in Layer ayarla.
3. Karakter ve UI’nin okunaklı durduğunu Scene + Game view’da kontrol et.
4. Işık / kamera ortografik ayarlarını gözden geçir.
5. Sahneyi kaydet.

## Tamamlandı kontrol listesi

- [ ] Arka plan sahnede görünüyor
- [ ] Karakter arka planın önünde net
- [ ] Sahne kayıtlı
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 5

---

# Faz 5 — UI tasarımı düzenlemesi

**Amaç:** Canvas Scaler, buton yerleşimi, eksik aksesuar butonları.

## Yapılacaklar

1. Canvas Scaler: **Scale With Screen Size**, örn. 1920×1080.
2. İleri / geri / devam / renk / aksesuar panellerini düzenle (mobil + tahta).
3. Eksik aksesuar butonlarını ekle (index 8–10 varsa).
4. Play’de dar ve geniş çözünürlük dene.

## Tamamlandı kontrol listesi

- [ ] Canvas Scaler ayarlı
- [ ] Butonlar taşmıyor / üst üste binmiyor
- [ ] Aksesuar butonları sprite sayısıyla uyumlu
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 6

---

# Faz 6 — Karakter boyut ve konumları (her karakter ayrı)

**Amaç:** Kedi / tavşan / kuş / rakun (veya listedeki hayvanlar) ayrı ölçek ve konum.

## Yapılacaklar

1. `AvatarGorunumu` içinde her `HayvanGorseli` için `govdeOlcekCarpani` ayarla (hepsi 1 kalmasın).
2. Gerekirse hayvan bazlı ofset / `aksesuarOfset` ince ayarı.
3. Play’de her hayvana geçip zemin hizasını kontrol et.

## Tamamlandı kontrol listesi

- [ ] Her hayvan için ayrı ölçek değeri girildi
- [ ] Play’de dört hayvan da ekranda dengeli duruyor
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 7

---

# Faz 7 — Aksesuar boyut ve konumları (karakter × aksesuar)

**Amaç:** Her aksesuar her karaktere otursun.

## Yapılacaklar

1. `AksesuarGorseli.olcek` ve `yerelOfset` değerlerini aksesuar bazında ayarla.
2. Her hayvan + her aksesuar kombinasyonunu Play’de hızlı tarama yap (en az şapka / gözlük / yok).
3. Kuş şapka vb. bilinen sorunları düzelt.

## Tamamlandı kontrol listesi

- [ ] Şapka / gözlük vb. her hayvanda kabul edilebilir konumda
- [ ] Ölçek aşırı büyük/küçük değil
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 8

---

# Faz 8 — Teslim (`main`)

## Teslim notu

```
Avatar (Said) teslim
- Branch: said/avatar → main PR
- Sahne: Assets/Scenes/AvatarOlusturmaEkrani.unity
- Renk + aksesuar + per-karakter / per-aksesuar ince ayar tamam
```

## Tamamlandı kontrol listesi

- [ ] Faz 0–7 ilgili maddeler işaretli
- [ ] Smoke: hayvan / renk / aksesuar / devam
- [ ] Commit + push + `main` PR açıldı
- [ ] Bilinen sorunlar yazıldı:

**Bilinen sorunlar:**

- (yoksa “yok” yaz)

---

## Hızlı referans

| Ne | Yol |
| -- | --- |
| Branch | `said/avatar` |
| Scriptler | `Assets/_Scripts/Avatar/` |
| Sahne | `Assets/Scenes/AvatarOlusturmaEkrani.unity` |
| Plan | `SAID_AVATAR.md` |
| Kurallar | `AGENTS.md` |
