# MUSTAFA_YIGIT_EFEKTLER.md — Mustafa Yiğit Avan · Paket 3 (Ses + Partikül)

Bu dosya **sadece Mustafa Yiğit Avan** için faz kapılı çalışma planıdır. Kod yazmadan önce `AGENTS.md` okuyun. Genel özet: `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` → Paket 3.

**Branch:** `mustafayigit/efektler`

**Önemli:** Ses ve partikül efektlerini **Said + Enes + Mustafa Üz oyunlarını bitirip `main`’e aldıktan sonra** ekle. Kapı (Faz −1) yeşil olmadan Faz 1+ kod yazma.

---

## Katı kurallar

1. Faz −1 (kapı) tamamlanmadan efekt / `SesYoneticisi` gövdesine geçme.
2. Bir fazın checklist’i bitmeden sonraki faza geçme.
3. Clip dosyalarını Abdullah Aslan hazırlar; sen API + bağlama + çalma mantığını yazarsın. Inspector’da son clip yerleşimi finali Paket 6 ile paylaşılabilir.
4. Başkasının paket klasörüne izinsiz script ekleme; Mini Oyun 1/2’ye yalnızca **null-safe çağrı** veya üzerinde anlaşılmış public API ekle.
5. Her faz çıkışında agent **commit + push** hatırlatır (`AGENTS.md`).
6. Checklist’i çocuk doldurmaz — agent canlı tutar.

---

## Onaylanmış kararlar

| Konu | Karar |
| ---- | ----- |
| Branch | `mustafayigit/efektler` |
| Kapı | `said/avatar` + `enes/level1` + `mustafauz/level2` → `main`; + Abdullah clip’leri |
| Buton SFX | Tek tıklama sesi |
| Müzik | Karakter (hayvan) değişince müzik değişir; oyun sahnelerinde müzik sürer (DDOL) |
| MO2 harf | Soru çıkar çıkmaz harf sesi **bir kez** |
| Doğru/yanlış | Her iki mini oyunda başarı / yanlış SFX |
| Kelime | Doğruda başarı SFX + kelime sesi bir kez (örn. “EEE-Elma”) |
| Partikül | Tik arkasında patlama; **1.5 saniye** |
| Final clip bağlama | Paket 6 ile koordineli |

---

## Mevcut durum özeti (2026-09)

| Script | Durum |
| ------ | ----- |
| `HarfObjeVerisi` | Var (harf + sprite; kelime/harf clip alanı henüz yok / eksik) |
| `SoruSecici` | Var |
| `SkorYoneticisi` | Var |
| `GeriBildirimYoneticisi` | Var (ikon + isteğe bağlı ses; **partikül alanı eksik / süre 0.8**) |
| `SesYoneticisi` | **Yok** — Faz 1 |
| `_Audio/` | Klasörler boş placeholder |

**Sıradaki odak:** Kapı (Faz −1) — efekt koduna şimdi başlama.

---

## Faz akışı

```
Faz −1 (Kapı: main + clip’ler)          ← beklemede
  → Faz 0 (Ortak çekirdek)              [tamam — SesYoneticisi hariç]
    → Faz 1 (SesYoneticisi)
      → Faz 2 (Buton tıklama SFX)
        → Faz 3 (Karakter değişince müzik)
          → Faz 4 (MO2 harf seslendirme)
            → Faz 5 (Doğru/yanlış SFX her iki oyun)
              → Faz 6 (Kelime seslendirme)
                → Faz 7 (Müzik sürekliliği)
                  → Faz 8 (Partikül 1.5 sn)
                    → Faz 9 (Teslim → main)
```

---

# Faz −1 — Kapı (kod yazma yok)

**Amaç:** Efekt işine başlamadan önce birleşik `main` ve ses asset’leri hazır olsun.

## Kontrol listesi

- [ ] `said/avatar` `main`’e merge edildi
- [ ] `enes/level1` `main`’e merge edildi
- [ ] `mustafauz/level2` `main`’e merge edildi
- [ ] Abdullah harf ses clip’lerini verdi (`_Audio/SesliHarfler/` veya kararlaştırılan yol)
- [ ] Abdullah kelime ses clip’lerini verdi (doğru cevap seslendirmesi)
- [ ] `mustafayigit/efektler` branch’i güncel `main`’den çekildi / rebase edildi
- [ ] Kapı notu bu dosyaya yazıldı (tarih)

**Çıkış şartı:** Hepsi yeşil olmadan Faz 1’e geçme.

---

# Faz 0 — Ortak çekirdek (mevcut)

**Amaç:** Veri / skor / soru / geri bildirim iskeleti.

## Tamamlandı kontrol listesi

- [x] `HarfObjeVerisi.cs`
- [x] `SoruSecici.cs` (`tumHarfler`, `YeniHarfSec`)
- [x] `SkorYoneticisi.cs` (`SkorEkle` / `DogruPuanEkle` / `SkoruSifirla`)
- [x] `GeriBildirimYoneticisi.cs` (`DogruGoster` / `YanlisGoster`) — partikül Faz 8’de tamamlanır
- [x] Harf asset’leri `_Data/Harfler/` altında mevcut
- [ ] Commit + push (çekirdek zaten `main`’deyse not düş)

**Çıkış:** → Faz −1 kapısı → Faz 1

---

# Faz 1 — SesYoneticisi.cs

**Amaç:** Fon müzik yöneticisi (DDOL); hayvan müziği listesi.

**Dosya:** `Assets/_Scripts/Ortak/SesYoneticisi.cs`

## Beklenen public yüzey (kılavuzla uyumlu)

| Alan / API | Not |
| ---------- | --- |
| `muzikKaynagi` | `AudioSource` |
| `hayvanMuzikleri` | `AudioClip[]` (4) |
| Start / hayvan seçimine göre Play | `loop = true` |
| Harf / kelime / SFX çalma yardımcıları | Null-safe `PlayOneShot` |

## Tamamlandı kontrol listesi

- [ ] Plan Mode (gerekirse) onaylandı
- [ ] `SesYoneticisi.cs` compile
- [ ] Singleton veya sahne-bağımsız yaşam (DDOL) net
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 2

---

# Faz 2 — Buton tıklama ses efekti

**Amaç:** Ortak bir tıklama SFX; UI butonlarında çalsın.

## Tamamlandı kontrol listesi

- [ ] Tek tıklama clip alanı + çalma API’si
- [ ] En az avatar / oyun UI butonlarında tetik (veya merkezi hook)
- [ ] Clip yokken kırmızı error yok
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 3

---

# Faz 3 — Karakter değişince müzik değiştirme

**Amaç:** `AvatarYoneticisi.seciliHayvanIndex` değişince ilgili müzik.

## Tamamlandı kontrol listesi

- [ ] Hayvan değişiminde müzik swap
- [ ] Play’de 4 hayvanda müzik değişimi duyuluyor (clip doluysa)
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 4

---

# Faz 4 — Mini Oyun 2 harf seslendirmesi

**Amaç:** Soru çıkar çıkmaz istenen harf sesi **bir kez**.

## Entegrasyon noktası

- `HarfSecmeYoneticisi.YeniSoru()` sonrası null-safe çağrı.

## Tamamlandı kontrol listesi

- [ ] Harf clip eşlemesi (SO veya dizi)
- [ ] `YeniSoru` sonrası bir kez çalıyor
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 5

---

# Faz 5 — Doğru / yanlış SFX (Mini Oyun 1 + 2)

**Amaç:** Doğruda başarı, yanlışta yanlış sesi.

## Entegrasyon

- `GeriBildirimYoneticisi.DogruGoster` / `YanlisGoster` clip alanları
- Mini Oyun 1 `ObjeBirakildi` / Mini Oyun 2 doğru-yanlış süreçleri zaten çağırıyorsa clip doldur; yoksa bağla

## Tamamlandı kontrol listesi

- [ ] `dogruSes` / `yanlisSes` alanları dolu veya API hazır
- [ ] Her iki oyunda tetikleniyor
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 6

---

# Faz 6 — Doğru kelime seslendirmesi

**Amaç:** Doğru cevapta başarı SFX + kelime sesi bir kez (örn. elma → “EEE-Elma”).

## Not

- Gerekirse `HarfObjeVerisi` veya obje verisine `AudioClip` alanı ekle (Türkçe public isim; Plan Mode).

## Tamamlandı kontrol listesi

- [ ] Kelime clip eşlemesi var
- [ ] Doğruda bir kez çalıyor (başarı sesi ile birlikte veya hemen sonra)
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 7

---

# Faz 7 — Oyun ekranlarında müzik sürekliliği

**Amaç:** Avatar → Mini Oyun 1 / 2 geçişinde müzik kesilmesin.

## Tamamlandı kontrol listesi

- [ ] DDOL müzik kaynağı sahne değişiminde yaşıyor
- [ ] Play’de sahne geçişinde müzik devam ediyor
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 8

---

# Faz 8 — Partikül patlama (tik arkası, 1.5 sn)

**Amaç:** Doğru bilindiğinde tik’in arkasında partikül; **1.5 saniye**.

## Yapılacaklar

1. `GeriBildirimYoneticisi` içine `ParticleSystem parlıltıEfekti` (kılavuz adı).
2. `DogruGoster` içinde `Play()`; partikül / gösterim süresi **1.5f**.
3. Sahneye Particle System yerleştir; tik ikonunun arkasında sorting.

## Tamamlandı kontrol listesi

- [ ] `parlıltıEfekti` alanı ve Play çağrısı
- [ ] Süre 1.5 sn
- [ ] Play’de tik + partikül görünüyor
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 9

---

# Faz 9 — Teslim (`main` + Paket 6)

## Teslim notu

```
Paket 3 efektler (Mustafa Yiğit) teslim
- Branch: mustafayigit/efektler → main PR
- SesYoneticisi: müzik + SFX + harf/kelime API
- GeriBildirim: doğru/yanlış ses + partikül 1.5 sn
- Clip bağlama final kontrolü: Paket 6 (Süleyman)
```

## Tamamlandı kontrol listesi

- [ ] Faz −1 … 8 ilgili maddeler işaretli
- [ ] Commit + push + `main` PR
- [ ] Paket 6’ya clip/Inspector notu iletildi
- [ ] Bilinen sorunlar:

**Bilinen sorunlar:**

- (yoksa “yok” yaz)

---

## Hızlı referans

| Ne | Yol |
| -- | --- |
| Branch | `mustafayigit/efektler` |
| Ortak scriptler | `Assets/_Scripts/Ortak/` |
| Audio | `Assets/_Audio/{Muzik,SFX,SesliHarfler}/` |
| Plan | `MUSTAFA_YIGIT_EFEKTLER.md` |
| Kurallar | `AGENTS.md` |
