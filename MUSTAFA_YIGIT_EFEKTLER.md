
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
| Yanlış SFX | Her iki mini oyunda yanlış cevap sesi (`yanlisSes`) |
| Doğru obje sesi | Doğruda tıklanan / bırakılan objenin klibi: `Assets/_Audio/Objeler/<Harf>/<ad>.wav` |
| Tik / çarpı görseli | Mustafa Üz koyar (`correct_check.png` / `wrong_x.png`). Bu pakette sprite konumu yok |
| SesliHarfler | Yalnızca harf adı (Faz 4). Klasör şu an boş; obje klipleri burada değil |
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
            → Faz 5 (Yanlış SFX her iki oyun)
              → Faz 6 (Doğru obje klibi)
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
- [ ] Abdullah kelime ses clip’lerini verdi (`Assets/_Audio/Objeler/` — doğru obje sesi)
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

Harf klipleri `Assets/_Audio/SesliHarfler/` altındadır. Bu klasör şu an boş. Obje sesleri burada aranmaz; onlar `Assets/_Audio/Objeler/` (Faz 6).

## Entegrasyon noktası

- `HarfSecmeYoneticisi.YeniSoru()` sonrası null-safe çağrı.

## Tamamlandı kontrol listesi

- [ ] Harf clip eşlemesi (SO veya dizi)
- [ ] `YeniSoru` sonrası bir kez çalıyor
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 5

---

# Faz 5 — Yanlış cevap SFX (Mini Oyun 1 + 2)

**Amaç:** Yanlış cevapta yanlış sesi. Tik ve çarpı görsellerini bu faz koymaz; onları Mustafa Üz yerleştirir.

## Entegrasyon

- `GeriBildirimYoneticisi.yanlisSes` + `YanlisGoster`
- `dogruSes` alanı durur; doğru cevabın sesi Faz 6’daki obje klibidir
- Mini Oyun 1 `ObjeBirakildi` ve Mini Oyun 2 yanlış süreci null-safe bağlanır

## Tamamlandı kontrol listesi

- [ ] `yanlisSes` API hazır; yanlışta çalıyor
- [ ] `dogruSes` alanı duruyor (obje klibi Faz 6)
- [ ] Her iki oyunda yanlış tetikleniyor
- [ ] `correct_check.png` / `wrong_x.png` konumuna dokunulmadı
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 6

---

# Faz 6 — Doğru obje sesi

**Amaç:** Doğru cevapta tıklanan veya bırakılan objenin kendi klibi bir kez çalar.

Klip yolu: `Assets/_Audio/Objeler/<Harf>/<ad>.wav` (ör. `Objeler/E/elma.wav`). `SesliHarfler` kullanılmaz.

## Not

- Eşleme sprite / obje adına göre. Gerekirse `HarfObjeVerisi` veya `SesYoneticisi` üzerinde Türkçe public API (Plan Mode).
- Mini Oyun 2: doğru balon. Mini Oyun 1: doğru bırakılan obje.
- Görsel tik Mustafa Üz’ün işi.

## Tamamlandı kontrol listesi

- [ ] Obje klibi `Assets/_Audio/Objeler/` yolundan eşleniyor
- [ ] Doğruda bir kez çalıyor
- [ ] `SesliHarfler` obje sesi için kullanılmıyor
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
- SesYoneticisi: müzik + yanlış SFX + harf API + obje klibi (`_Audio/Objeler/`)
- GeriBildirim: yanlış ses + partikül 1.5 sn
- Tik/çarpı görseli: Mustafa Üz (`correct_check.png` / `wrong_x.png`)
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
| Audio | `Assets/_Audio/{Muzik,SFX,SesliHarfler,Objeler}/` |
| Plan | `MUSTAFA_YIGIT_EFEKTLER.md` |
| Kurallar | `AGENTS.md` |
