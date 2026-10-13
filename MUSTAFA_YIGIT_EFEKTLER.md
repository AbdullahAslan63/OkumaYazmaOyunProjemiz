
# MUSTAFA_YIGIT_EFEKTLER.md — Mustafa Yiğit Avan · Paket 3 (Ses + Partikül)

Bu dosya **sadece Mustafa Yiğit Avan** için faz kapılı çalışma planıdır. Kod yazmadan önce `AGENTS.md` okuyun. Genel özet: `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` → Paket 3.

**Branch:** `mustafayigit/efektler`

**Önemli:** Ses işi bu branch’te başlar. Avatar, level ve harf seçme sahneleri buradadır. Enes’in SepetOyunu ses kancaları oyun bitene kadar ertelenir. Tıklama sesi tek API’dir; karakter, level ve ana menü aynı çağrıyı kullanır.

---

## Katı kurallar

1. `SesYoneticisi` gövdesine Faz 1’den başla. SepetOyunu dosyalarına ses kancası ekleme; o iş “Ertelenen” bölümündedir.
2. Bir fazın checklist’i bitmeden sonraki faza geçme.
3. Clip dosyalarını Abdullah Aslan hazırlar; sen API + bağlama + çalma mantığını yazarsın. Clip yokken sessiz geç, kırmızı error üretme. Inspector’da son clip yerleşimi finali Paket 6 ile paylaşılabilir.
4. Başkasının paket klasörüne yeni oyun scripti ekleme. Ortak ses bileşeni `Assets/_Scripts/Ortak/` altındadır. Avatar butonları kodla kurulduğu için `AvatarSahneKurucu` yalnızca bu bileşeni butona ekler; seçim mantığına dokunulmaz. Mini Oyun 2’ye yalnızca null-safe çağrı gider.
5. Her faz çıkışında agent **commit + push** hatırlatır (`AGENTS.md`).
6. Checklist’i çocuk doldurmaz — agent canlı tutar.

---

## Onaylanmış kararlar

| Konu | Karar |
| ---- | ----- |
| Branch | `mustafayigit/efektler` |
| Kapı (2026-10-13) | Sahneler bu branch’te. Said / Mustafa Üz `main` push’u beklenmez. SepetOyunu sesi ertelenir |
| Buton SFX | Tek clip + `TiklamaSesiCal()`. Karakter, level ve ana menü aynı bileşeni kullanır |
| Müzik | Karakter (hayvan) değişince müzik değişir; oyun sahnelerinde müzik sürer (DDOL) |
| MO2 harf | Soru çıkar çıkmaz harf sesi **bir kez** |
| Yanlış SFX | Şimdilik Mini Oyun 2 (`yanlisSes`). SepetOyunu sonra |
| Doğru obje sesi | Şimdilik Mini Oyun 2’de doğru balon. Klip: `Assets/_Audio/Objeler/<Harf>/<ad>.wav`. Sepet bırakması sonra |
| Tik / çarpı görseli | Mustafa Üz koyar (`correct_check.png` / `wrong_x.png`). Bu pakette sprite konumu yok |
| SesliHarfler | Yalnızca harf adı (Faz 4). Klasör şu an boş; obje klipleri burada değil |
| Partikül | Tik arkasında patlama; **1.5 saniye** |
| Final clip bağlama | Paket 6 ile koordineli |

---

## Mevcut durum özeti (2026-10-13)

`mustafayigit/efektler` üzerinde avatar, level (`LevelEkranı`) ve harf seçme sahneleri var. Said ve Mustafa Üz’ün sonraki sahne işleri ses API’sini değiştirmez. SepetOyunu ses kancası yazılmaz.

**Sıradaki odak:** Faz 1 — `SesYoneticisi`, ardından Faz 2’de ortak tıklama sesi.

| Script | Durum |
| ------ | ----- |
| `HarfObjeVerisi` | Var (harf + sprite; kelime/harf clip alanı henüz yok / eksik) |
| `SoruSecici` | Var |
| `SkorYoneticisi` | Var |
| `GeriBildirimYoneticisi` | Var (ikon + isteğe bağlı ses; **partikül alanı eksik / süre 0.8**) |
| `SesYoneticisi` | **Yok** — Faz 1 |
| `ButonTiklamaSesi` | **Yok** — Faz 2 |
| `_Audio/Objeler/` | Kelime klipleri duruyor |
| `_Audio/SesliHarfler/` | Harf klipleri henüz yok (Faz 4 null-safe bekler) |

---

## Faz akışı

```
Faz −1 (Kapı: sahneler branch’te, Sepet sesi ertelendi)  [karar yazıldı]
  → Faz 0 (Ortak çekirdek)              [tamam — SesYoneticisi hariç]
    → Faz 1 (SesYoneticisi)             ← sıradaki
      → Faz 2 (Ortak tıklama SFX: karakter + level + ana menü)
        → Faz 3 (Karakter değişince müzik)
          → Faz 4 (MO2 harf seslendirme)
            → Faz 5 (Yanlış SFX — yalnızca MO2)
              → Faz 6 (Doğru obje klibi — yalnızca MO2)
                → Faz 7 (Müzik sürekliliği: avatar, level, harf seçme)
                  → Faz 8 (Partikül 1.5 sn)
                    → Faz 9 (Teslim → main)
SepetOyunu ses kancaları → oyun bitince, bu akışın dışında
```

---

# Faz −1 — Kapı

**Amaç:** Ses işine bu branch’teki sahnelerle başlamak. `main` merge’i ve SepetOyunu tamamlanması bekleme kapısı değildir.

## Kapı notu (2026-10-13)

Vefa: Said ve Mustafa Üz sahneleri `mustafayigit/efektler` üzerine alındı. Onlar `main`’e henüz pushlamadı; yapacakları iş ses ve efekti ilgilendirmez. Oyunu bitmeyen tek parça Enes’in SepetOyunu’dur; ses kancaları şimdilik atlanır. Tıklama sesi karakter ekranında kurulur ve level ile ana menüde aynı API ile kullanılır.

## Kontrol listesi

- [x] Avatar, level ve harf seçme sahneleri bu branch’te
- [x] Said / Mustafa Üz `main` merge’i ses işi için beklenmez
- [x] SepetOyunu ses entegrasyonu ertelendi (aşağıdaki “Ertelenen” bölümü)
- [x] `mustafayigit/efektler` güncel `main` kodunu içeriyor (2026-10-13, `0958895`; plan notu `b08d1b0`)
- [x] Kapı notu yazıldı (2026-10-13)

**Çıkış:** → Faz 1

---

# Faz 0 — Ortak çekirdek (mevcut)

**Amaç:** Veri / skor / soru / geri bildirim iskeleti.

## Tamamlandı kontrol listesi

- [x] `HarfObjeVerisi.cs`
- [x] `SoruSecici.cs` (`tumHarfler`, `YeniHarfSec`)
- [x] `SkorYoneticisi.cs` (`SkorEkle` / `DogruPuanEkle` / `SkoruSifirla`)
- [x] `GeriBildirimYoneticisi.cs` (`DogruGoster` / `YanlisGoster`) — partikül Faz 8’de tamamlanır
- [x] Harf asset’leri `_Data/Harfler/` altında mevcut
- [x] Commit + push — çekirdek `main`’de; bu branch o kodu içeriyor

**Çıkış:** → Faz 1

---

# Faz 1 — SesYoneticisi.cs

**Amaç:** Fon müzik yöneticisi (DDOL); hayvan müziği listesi.

**Dosya:** `Assets/_Scripts/Ortak/SesYoneticisi.cs`

## Beklenen public yüzey (kılavuzla uyumlu)

| Alan / API | Not |
| ---------- | --- |
| `muzikKaynagi` | `AudioSource`, `loop = true` |
| `efektKaynagi` | Ayrı `AudioSource`. Müzik kaynağında `PlayOneShot` kullanılmaz |
| `hayvanMuzikleri` | `AudioClip[]` (4) |
| `tiklamaSesi` | Tek tıklama klibi. Sahne başına kopyalanmaz |
| `TiklamaSesiCal()` | Null-safe. Clip veya kaynak yoksa sessiz |
| Harf / kelime / yanlış SFX yardımcıları | Hepsi `efektKaynagi` üzerinden, null-safe |

## Tamamlandı kontrol listesi

- [ ] Plan Mode (gerekirse) onaylandı
- [ ] `SesYoneticisi.cs` compile
- [ ] Singleton veya sahne-bağımsız yaşam (DDOL) net
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 2

---

# Faz 2 — Ortak buton tıklama sesi

**Amaç:** Karakter ekranındaki tıklama sesi, level ekranında ve ana menüde aynı çağrıyla kullanılsın. İkinci bir clip, ikinci bir `AudioSource` veya sahneye gömülü kopya yok.

## Nasıl kullanılır

1. Klip ve çalma `SesYoneticisi` içindedir: `tiklamaSesi` + `TiklamaSesiCal()`. Yönetici DDOL olduğu için sahne değişince clip kaybolmaz.
2. `Assets/_Scripts/Ortak/ButonTiklamaSesi.cs` bir `Button` üzerindedir. Tıklanınca yalnızca `SesYoneticisi.TiklamaSesiCal()` çağırır. Butonun kendi `OnClick` işine (sahne geçişi, sekme, renk) karışmaz.
3. Sahneye elle konan butonlar (level ekranı, ana menü, harf seçme “Ana Menü”): bileşen Inspector’dan eklenir.
4. Karakter ekranı butonları `AvatarSahneKurucu` içinde kodla kurulur. Buton yaratılırken aynı `ButonTiklamaSesi` eklenir. Seçim, renk ve aksesuar kodu değişmez.

Ana menü sahnesi henüz ayrı durmuyorsa API ve bileşen hazır olur; sahne gelince aynı bileşen takılır, yeni ses kodu yazılmaz.

## Tamamlandı kontrol listesi

- [ ] `tiklamaSesi` + `TiklamaSesiCal()` (clip yokken sessiz)
- [ ] `ButonTiklamaSesi` herhangi bir `Button`’a takılınca aynı sesi çalıyor
- [ ] Karakter ekranı butonlarında bileşen var
- [ ] `LevelEkranı` butonlarında aynı bileşen var
- [ ] Harf seçme ana menü butonunda aynı bileşen var
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

# Faz 5 — Yanlış cevap SFX (Mini Oyun 2)

**Amaç:** Harf seçmede yanlış cevapta yanlış sesi. Tik ve çarpı görsellerini bu faz koymaz. SepetOyunu bu fazda yoktur.

## Entegrasyon

- Çalma `SesYoneticisi` üzerinden null-safe olur (`efektKaynagi`). `GeriBildirimYoneticisi.yanlisSes` duruyorsa onu da bu kaynağa bağla; ikinci bir ses yolu açma.
- `dogruSes` genel “doğru” efekti olarak çalmaz. Doğru cevabın sesi Faz 6’daki obje klibidir.
- Yalnızca Mini Oyun 2 yanlış süreci.

## Tamamlandı kontrol listesi

- [ ] `yanlisSes` API hazır; harf seçmede yanlışta bir kez çalıyor
- [ ] `dogruSes` obje klibinin yerine geçmiyor
- [ ] SepetOyunu scriptlerine dokunulmadı
- [ ] `correct_check.png` / `wrong_x.png` konumuna dokunulmadı
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 6

---

# Faz 6 — Doğru obje sesi

**Amaç:** Harf seçmede doğru balonun kendi klibi bir kez çalar.

Klip yolu: `Assets/_Audio/Objeler/<Harf>/<ad>.wav` (ör. `Objeler/E/elma.wav`). `SesliHarfler` kullanılmaz.

## Not

- Eşleme sprite / obje adına göre. Gerekirse `HarfObjeVerisi` veya `SesYoneticisi` üzerinde Türkçe public API (Plan Mode).
- Bu fazda yalnızca Mini Oyun 2: doğru balon.
- Görsel tik Mustafa Üz’ün işi.

## Tamamlandı kontrol listesi

- [ ] Obje klibi `Assets/_Audio/Objeler/` yolundan eşleniyor
- [ ] Harf seçmede doğruda bir kez çalıyor
- [ ] `SesliHarfler` obje sesi için kullanılmıyor
- [ ] SepetOyunu scriptlerine dokunulmadı
- [ ] Commit + push yapıldı

**Çıkış:** → Faz 7

---

# Faz 7 — Ekranlar arasında müzik sürekliliği

**Amaç:** Avatar → level → harf seçme geçişinde müzik kesilmesin. Sepet sahnesi gelince aynı DDOL kaynak onu da taşır; bu fazda SepetOyunu’na girilmez.

## Tamamlandı kontrol listesi

- [ ] DDOL müzik kaynağı sahne değişiminde yaşıyor
- [ ] Play’de avatar, level ve harf seçme arasında müzik devam ediyor
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
- SesYoneticisi: müzik + ortak TiklamaSesiCal + yanlış SFX + harf API + obje klibi (`_Audio/Objeler/`)
- ButonTiklamaSesi: karakter, level, ana menü / harf seçme ana menü butonu
- GeriBildirim: yanlış ses + partikül 1.5 sn (harf seçme)
- Tik/çarpı görseli: Mustafa Üz (`correct_check.png` / `wrong_x.png`)
- SepetOyunu ses kancaları: ertelendi
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

# Ertelenen — SepetOyunu (Enes)

Oyun bitmeden bu maddeler yapılmaz. `ObjeSurukleme` ve sepet yöneticisine ses satırı eklenmez. Oyun bitince aynı `SesYoneticisi` API’si kullanılır; yeni bir ses sistemi yazılmaz.

- [ ] Yanlış bırakmada `yanlisSes`
- [ ] Doğru bırakmada obje klibi (`Assets/_Audio/Objeler/`)
- [ ] Sepet sahnesinde müzik DDOL ile sürüyor
- [ ] Sepet UI butonlarında `ButonTiklamaSesi`

---

## Hızlı referans

| Ne | Yol |
| -- | --- |
| Branch | `mustafayigit/efektler` |
| Ortak scriptler | `Assets/_Scripts/Ortak/` |
| Audio | `Assets/_Audio/{Muzik,SFX,SesliHarfler,Objeler}/` |
| Plan | `MUSTAFA_YIGIT_EFEKTLER.md` |
| Kurallar | `AGENTS.md` |
