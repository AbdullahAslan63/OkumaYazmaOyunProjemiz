# AGENTS.md — Okuma Yazma Oyun Projesi (Bölüm 1)

Bu dosya Cursor ve kodlama ekibi için ortak sözleşmedir. Görev vermeden önce bu dosyayı okutun. Uzun faz planları burada değildir; her kişinin kendi görev dosyasına bakın (`yol_haritasi.md`).

Aktif Unity projesi bu kök dizindir (`Assets/`, `ProjectSettings/`).

---

## İlerleme takibi

Faz planlarındaki `- [ ]` kutuları çocuğun tek başına doldurması için değildir. Cursor / agent bunları canlı tutar.

1. **Sık durum ver:** Her anlamlı adımdan sonra kısa özet söyle — ne bitti, ne eksik, sırada ne var.
2. **Bilgi iste (olgu):** Unity Editor / Play / asset gibi göremediğin gerçekleri sor (“Kaydettin mi?”, “Play’de ne oldu?”). Tasarım kararını yeniden sordurma.
3. **Onaya göre dökümanı güncelle:** Net bilgi veya onay gelince ilgili plandaki kutuları agent `[x]` yapar; gerekirse “Mevcut durum” notunu yazar.
4. **Tahminle işaretleme:** Repoda görüp emin olduğun teknik maddeleri `[x]` yap. Editor ve Play için teyit şart.
5. **Bekleme tuzağı:** “Checklist’i doldurup gel” deme. Net olgu soruları sor.

---

## “Ben (isim), ne durumdayım?” — yönlendirici cevap

İsim → paket eşlemesi için aşağıdaki **Paket sahipleri** tablosunu kullan. Emin değilsen bir kez netleştir, sonra plana geç.

**Cevap şablonu (sırayla, kısa):**

1. **Durum:** Paket + hangi faz (plana ve repoya bak).
2. **Biten / eksik:** Bir-iki cümle.
3. **Sıradaki tek iş:** Plandaki **tek net emir** (Editor veya kod). Seçenek listesi yok.
4. **Bitince ne diyeceksin:** Örn. “Kaydettim” / “Play’de çalıştı” — gelince checklist’i agent günceller.

Projede, bu dosyada veya kişi planında karar yazıyorsa onu uygula. “A mı B mi?” diye sorma. Belirsizlik yalnızca planda gerçekten boşsa: tek önerilen yol + gerekirse tek evet/hayır.

---

## Proje amacı

İlkokul seviyesinde okuma-yazma destekleyen Unity 2D oyunu. Bölüm 1’de:

1. Avatar seçimi (hayvan + renk + aksesuar)
2. Mini Oyun 1 — Sepetle Toplama (sabit balona obje sürükleme)
3. Mini Oyun 2 — Hangisinin Baş Harfi (4 seçenekten doğru objeyi seçme)

Bölüm 2+ farklı mekanikler getirebilir. Bu bölüm için her şeye uyan büyük bir sistem kurmayın; paket sınırları içinde küçük, anlaşılır scriptler yazın.

**Hedef cihazlar:** Hem **mobil** hem **Windows akıllı tahta**. UI punto, Canvas Scaler ve sahne düzeni buna göre düşünülür.

---

## Klasör sözleşmesi

```
Assets/
├── _Scripts/
│   ├── Avatar/           # Paket 1
│   ├── Ortak/            # Paket 3 (bu kopyada henüz yok)
│   ├── SepetOyunu/       # Paket 4 (Mini Oyun 1)
│   └── HarfSecmeOyunu/   # Paket 5 (bu kopyada henüz yok)
├── _Art/                 # Grafik (şu an avatar görselleri)
├── _Audio/               # Müzik, SFX, sesli harfler (klasörler boş)
├── _Data/                # ScriptableObject harf kartları (henüz yok)
├── _Prefabs/
│   ├── Ortak/
│   ├── SepetOyunu/       # Obje.prefab
│   └── HarfSecmeOyunu/
├── _UI/
└── Scenes/               # Güncel yol: SepetOyunu.unity, AbdullahScene.unity
```

Yeni scripti doğru paket klasörüne koyun. Rastgele `Assets/` köküne script atmayın.

Bu çalışma kopyasında şu an duran scriptler: `AvatarYoneticisi`, `AvatarGorunumu`, `ObjeSurukleme`. `SepetOyunYoneticisi` henüz yok.

---

## Paket sahipleri

| Paket | Sorumlu | Ana dosyalar / plan |
| ----- | ------- | ------------------- |
| 1 — Avatar | Mustafa Said Bayram | `AvatarYoneticisi`, `AvatarGorunumu`, `AvatarSecimEkrani`, `RenkSecici`, `AksesuarSecici` |
| 3 — Ortak sistemler | Mustafa Yiğit Avan | `HarfObjeVerisi`, `SoruSecici`, `SkorYoneticisi`, `GeriBildirimYoneticisi`, `SesYoneticisi` |
| 4 — Mini Oyun 1 | Enes Barış | `ObjeSurukleme`, `SepetOyunYoneticisi` — detay: `yol_haritasi.md` |
| 5 — Mini Oyun 2 | Mustafa Üz | `HarfSecmeYoneticisi`, `SecenekBalonu` |
| 6 — UI / font / entegrasyon | Süleyman Öz | `SahneGecisi`, font, ses/efekt Inspector bağlama, genel test |

Başkasının paket klasörüne izinsiz script eklemeyin; API ihtiyacı varsa sahip kişiyle konuşun.

---

## Isimlendirme

- `public` alan ve fonksiyon isimleri kılavuzdaki **Türkçe isimlerle sabittir**. Cursor’a değiştirtmeyin.
- Örnekler: `seciliHayvanIndex`, `ObjeBirakildi`, `harfRozeti`, `kalanSure`, `objeAdi`, `dogruHarf`, `birakmaMesafesi`.
- Sahne/prefab adları: `SepetOyunu`, `Obje.prefab`, `HarfRozeti`, `ObjePozisyon1`…
- Tag: Mini Oyun 1 balonu için `Balon`.

---

## Cursor’a prompt verirken

1. Önce bu `AGENTS.md` dosyasını okutun; Enes için `yol_haritasi.md` dosyasını da okutun; sonra tek bir script görevi verin.
2. Bir seferde **tek script** isteyin; yöneticiyi sürükleme ile karıştırmayın.
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
- Bu sahnede avatar gösterilmez.
- Süre varsayılan **60** saniye. Doğru cevap **+10** puan.
- Giriş: fare ve parmak (touch).

### İptal edilen dosya

Eski planda geçen `SepetHareketi.cs` (balonu hareket ettirme) **geçerli değildir**. Yerine `ObjeSurukleme.cs` kullanılır. Bu dosyayı oluşturmayın.

Detaylı fazlar ve güncel ilerleme: `yol_haritasi.md`. Bu dosyaya sadık kal. Bir faz bitip çalışır durumdaysa sıradaki fazı öner; Enes açıkça onaylamadan o faza geçme.

---

## Mini Oyun 2 mekanik hatırlatması (Paket 5)

- Ekranda büyük **soru harfi** gösterilir; oyuncu 4 seçenekten doğru objeyi **tıklar**.
- Her tur: **1 doğru + 3 çeldirici**, karıştırılmış 4 slot.
- Yanlışta soru değişmez; doğruda yeni soru.
- Bu çalışma kopyasında Paket 5 sahneleri ve scriptleri yoktur. Mini Oyun 2 işi Mustafa Üz’ün paketindedir.

---

## Bağımlılık sırası

```
Paket 1 + Paket 3  →  Paket 4 ve 5  →  Paket 6 entegrasyon
```

Paket 4’te `ObjeSurukleme` Paket 3 olmadan yazılır ve test edilir. `SepetOyunYoneticisi` ise `SoruSecici`, `SkorYoneticisi` ve `GeriBildirimYoneticisi` hazır olmadan yazılmaz. Kapı: `yol_haritasi.md` Faz 3.

---

## Kaynak kılavuzlar

- `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` — adım adım görev + Cursor prompt örnekleri (Enes için Paket 4, Adım 4.1–4.6)
- `Kodlama Ekibi Planı Bölüm 1.md` — paket dağılımı. Mekanik çakışırsa detaylı kılavuz ve bu dosya üstündür (`SepetHareketi` yazılmaz).
- `yol_haritasi.md` — Enes Barış faz kapılı çalışma planı (Mini Oyun 1)
