# AGENTS.md — Okuma Yazma Oyun Projesi (Bölüm 1)

Bu dosya Cursor ve kodlama ekibi için ortak sözleşmedir. Görev vermeden önce bu dosyayı okutun. Uzun faz planları burada değildir; her kişinin kendi görev dosyasına bakın (ör. `ENES_MINI_OYUN_1.md`, `MUSTAFA_UZ_MINI_OYUN_2.md`).

---

## İlerleme takibi (checklist’i çocuğa bırakma)

Faz planlarındaki `- [ ]` kutuları **çocukların tek başına doldurması için değildir**. Cursor / agent bunları **canlı tutar**.

1. **Sık durum ver:** Her anlamlı adımdan sonra (veya çocuk sorduğunda) kısa özet söyle — ne bitti, ne eksik, sırada ne var. Uzun sessizlik veya “checklist’i sen işaretle” deme.
2. **Bilgi iste (olgu):** Unity Editor / Play / asset gibi senin göremediğin **gerçekleri** sor (“Kaydettin mi?”, “Play’de ne oldu?”). Tasarım / mimari **karar sordurma**.
3. **Onaya göre dökümanı güncelle:** Çocuk net bilgi/onay verince ilgili plandaki kutuları agent **kendisi** `[x]` yapar; gerekirse “Mevcut durum” notunu yazar.
4. **Tahminle işaretleme:** Repoda görüp emin olduğun teknik maddeleri `[x]` yapabilirsin; Editor/Play için teyit şart.
5. **Bekleme tuzağı:** “Checklist’i doldurup gel” deme. Net olgu soruları sor.

---

## “Ben (isim), ne durumdayım?” — yönlendirici cevap

İsim → paket eşlemesi için aşağıdaki **Paket sahipleri** tablosunu kullan (kısa ad / soyad / “Mustafa Üz” gibi varyasyonları tanı). Emin değilsen bir kez netleştir, sonra plana geç.

**Cevap şablonu (sırayla, kısa):**

1. **Durum:** Paketin + hangi fazdasın (plana ve repoya bak).
2. **Biten / eksik:** Bir-iki cümle.
3. **Sıradaki tek iş:** Plana ve onaylanmış kararlara göre **tek net emir** (Editor veya kod). Seçenek listesi / “hangisini istersin?” yok.
4. **Bitince ne diyeceksin:** Örn. “Kaydettim” / “Play’de çalıştı” — gelince checklist’i agent günceller.

**Karar alma vs uygulama:**

- Projede / `AGENTS.md` / kişi planında **karar zaten yazıyorsa** onu uygula; çocuğa yeniden karar aldırma.
- “A mı B mi?” diye sorma. Doğru yolu söyle ve yaptır: “Şimdi şunu yap: …”
- Belirsizlik yalnızca planda gerçekten boşsa ve ilerlemeden gerekirse: o zaman da 2 uzun seçenek menüsü değil, **tek önerilen yol** + gerekirse tek evet/hayır.

**Yanlış örnek:** “UI için TMP mi Legacy mi istersin? Animasyonu şimdi mi sonra mı yapalım?”  
**Doğru örnek:** “Sırada Faz 4. Canvas Scaler’ı Scale With Screen Size yap, süre yazısını panelli büyüt. Bitince ‘UI’yi ayarladım’ yaz.”

---

## Proje amacı

İlkokul seviyesinde okuma-yazma destekleyen Unity 2D oyunu. Bölüm 1’de:

1. Avatar seçimi (hayvan + renk + aksesuar)
2. Mini Oyun 1 — Sepetle Toplama (sabit balona obje sürükleme)
3. Mini Oyun 2 — Hangisinin Baş Harfi (4 seçenekten doğru objeyi seçme)

Bölüm 2+ farklı mekanikler getirebilir. Bu bölüm için “her şeye uyan büyük sistem” kurmayın; paket sınırları içinde küçük, anlaşılır scriptler yazın.

**Hedef cihazlar:** Hem **mobil** hem **Windows akıllı tahta** (büyük ekran). UI punto, Canvas Scaler ve sahne düzeni buna göre düşünülür.

---

## Klasör sözleşmesi

```
Assets/
├── _Scripts/
│   ├── Avatar/           # Paket 1
│   ├── Ortak/            # Paket 3
│   ├── SepetOyunu/       # Paket 4 (Mini Oyun 1)
│   └── HarfSecmeOyunu/   # Paket 5 (Mini Oyun 2)
├── _Art/                 # Grafik asset’leri
├── _Data/                # ScriptableObject veri kartları (HarfObjeVerisi vb.)
├── _Prefabs/
│   ├── Ortak/
│   ├── SepetOyunu/
│   └── HarfSecmeOyunu/
└── _Scenes/              # Oyun sahneleri (bazı sahneler geçici olarak Assets/Scenes/ altında olabilir)
```

Yeni scripti doğru paket klasörüne koyun. Rastgele `Assets/` köküne script atmayın.

---

## Paket sahipleri

| Paket | Sorumlu | Ana dosyalar / plan |
| ----- | ------- | ------------------- |
| 1 — Avatar | Mustafa Said Bayram | `AvatarYoneticisi`, `AvatarGorunumu`, `AvatarSecimEkrani`, `RenkSecici`, `AksesuarSecici` |
| 3 — Ortak sistemler | Mustafa Yiğit Avan | `HarfObjeVerisi`, `SoruSecici`, `SkorYoneticisi`, `GeriBildirimYoneticisi`, `SesYoneticisi` |
| 4 — Mini Oyun 1 | Enes Barış | `ObjeSurukleme`, `SepetOyunYoneticisi` — detay: `ENES_MINI_OYUN_1.md` |
| 5 — Mini Oyun 2 | Mustafa Üz | `HarfSecmeYoneticisi`, `SecenekBalonu` — detay: `MUSTAFA_UZ_MINI_OYUN_2.md` |
| 6 — UI / font / entegrasyon | Süleyman Öz | `SahneGecisi`, font, ses/efekt Inspector bağlama, genel test |

Başkasının paket klasörüne izinsiz script eklemeyin; API ihtiyacı varsa sahip kişiyle konuşun.

---

## Isimlendirme

- `public` alan ve fonksiyon isimleri kılavuzdaki **Türkçe isimlerle sabittir**. Cursor’a değiştirtmeyin.
- Örnekler: `seciliHayvanIndex`, `ObjeBirakildi`, `harfRozeti`, `kalanSure`, `SecenekSecildi`, `YeniSoru`.
- Sahne/prefab adları: `SepetOyunu`, `HarfSecmeOyunu`, `Obje.prefab`, `HarfRozeti`, `ObjePozisyon1`…
- Tag: Mini Oyun 1 balonu için `Balon`.

---

## Cursor’a prompt verirken

1. Önce bu `AGENTS.md` dosyasını okutun; ilgili kişinin faz planını (`ENES_MINI_OYUN_1.md` / `MUSTAFA_UZ_MINI_OYUN_2.md`) okutun; sonra tek bir script görevi verin.
2. Bir seferde **tek script** isteyin; yöneticiyi sürükleme / seçenek scripti ile karıştırmayın.
3. `public` alan listesini prompt’a aynen yazın.
4. Üretilen kodu çalıştırmadan önce okuyun; satırın ne yaptığını anlayın.
5. Karmaşık yöneticilerde (`SepetOyunYoneticisi`, `HarfSecmeYoneticisi`) önce **Plan Mode**, onay, sonra kod.
6. Her satıra kısa **Türkçe yorum** isteyin (öğrenme için).

---

## Editör işi vs kod işi

Cursor kod yazar; şu işler **Unity Editor’de elle** yapılır:

- Sahne oluşturma, GameObject yerleştirme
- Collider / Tag / Layer ayarı
- Prefab kaydetme
- Canvas, Button, TextMeshPro oluşturma
- Inspector’dan referans sürükleme
- Button `On Click ()` bağlama
- Build Settings’e sahne ekleme
- Canvas Scaler / çoklu çözünürlük (mobil + tahta) kontrolü

Kod yazıldıktan sonra ilgili kişinin faz planındaki “Unity Editor” listesini bitirmeden “bitti” demeyin.

---

## Mini Oyun 1 mekanik hatırlatması (Paket 4)

- Balon **sabit** durur; yatay sürüklenmez.
- Objeler ekranın **altında sabit pozisyonlarda** durur.
- Oyuncu objeyi sürükleyip balonun üzerine bırakır.
- Yanlış bırakınca obje başlangıç yerine döner; doğruysa puan + yeni tur.

### İptal edilen dosya

Eski planda geçen `SepetHareketi.cs` (balonu hareket ettirme) **geçerli değildir**. Yerine `ObjeSurukleme.cs` kullanılır. Bu dosyayı oluşturmayın.

---

## Mini Oyun 2 mekanik hatırlatması (Paket 5)

- Ekranda büyük **soru harfi** gösterilir; oyuncu 4 seçenekten doğru objeyi **tıklar**.
- Her tur: **1 doğru + 3 çeldirici**, karıştırılmış 4 slot.
- Yanlışta soru değişmez; doğruda yeni soru.
- Süre varsayılan **60** sn; bitiş ekranı **sade** (tebrik + yıldız + doğru sayısı + Ana Menü) — uzun harf raporu yok.
- Seçenek balonları tur başında **aşağıdan yukarı** yükselerek gelir (hafif rastgele hız farkı); birden belirmez.
- Soru harfi ve üst UI, meşgul arka plan üzerinde **okunaklı** ve **büyük** olmalı (mobil + akıllı tahta).
- Çocuklar okuma bilmediği için soru + obje **seslendirmesi** gerekir: clip/API **Paket 3**, sahne çağrı kancası **Paket 5**, Inspector clip bağlama finali **Paket 6**.

Detaylı fazlar: `MUSTAFA_UZ_MINI_OYUN_2.md`.

---

## Bağımlılık sırası (özet)

```
Paket 1 + Paket 3  →  Paket 4 ve 5  →  Paket 6 entegrasyon
```

- Paket 4, Paket 3’teki `SoruSecici` / `SkorYoneticisi` / `GeriBildirimYoneticisi` olmadan oyun yöneticisini tamamlamaz. Detay: `ENES_MINI_OYUN_1.md` Faz 3.
- Paket 5 çekirdek oynanışı Paket 3’süz ilerleyebilir; **ortak skor / soru seçici / geri bildirim / seslendirme entegrasyonu** Paket 3 checkpoint’ten sonra yapılır. Detay: `MUSTAFA_UZ_MINI_OYUN_2.md` Faz 6–7.

---

## Kaynak kılavuzlar

- `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` — adım adım görev + Cursor prompt örnekleri
- `Kodlama Ekibi Planı Bölüm 1.md` — paket dağılımı (mekanik çakışırsa detaylı kılavuz + bu AGENTS üstündür)
- `ENES_MINI_OYUN_1.md` — Enes Barış faz kapılı çalışma planı (Mini Oyun 1)
- `MUSTAFA_UZ_MINI_OYUN_2.md` — Mustafa Üz faz kapılı çalışma planı (Mini Oyun 2)
