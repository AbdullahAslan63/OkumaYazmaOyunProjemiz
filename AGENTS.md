# AGENTS.md — Okuma Yazma Oyun Projesi (Bölüm 1)

Bu dosya Cursor ve kodlama ekibi için ortak sözleşmedir. Görev vermeden önce bu dosyayı okutun. Uzun faz planları burada değildir; her kişinin kendi görev dosyasına bakın (ör. `ENES_MINI_OYUN_1.md`).

---

## Proje amacı

İlkokul seviyesinde okuma-yazma destekleyen Unity 2D oyunu. Bölüm 1’de:

1. Avatar seçimi (hayvan + renk + aksesuar)
2. Mini Oyun 1 — Sepetle Toplama (sabit balona obje sürükleme)
3. Mini Oyun 2 — Hangisinin Baş Harfi (4 seçenekten doğru objeyi seçme)

Bölüm 2+ farklı mekanikler getirebilir. Bu bölüm için “her şeye uyan büyük sistem” kurmayın; paket sınırları içinde küçük, anlaşılır scriptler yazın.

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
└── _Scenes/              # Oyun sahneleri
```

Yeni scripti doğru paket klasörüne koyun. Rastgele `Assets/` köküne script atmayın.

---

## Paket sahipleri

| Paket | Sorumlu | Ana dosyalar |
| ----- | ------- | ------------ |
| 1 — Avatar | Mustafa Said Bayram | `AvatarYoneticisi`, `AvatarGorunumu`, `AvatarSecimEkrani`, `RenkSecici`, `AksesuarSecici` |
| 3 — Ortak sistemler | Mustafa Yiğit Avan | `HarfObjeVerisi`, `SoruSecici`, `SkorYoneticisi`, `GeriBildirimYoneticisi`, `SesYoneticisi` |
| 4 — Mini Oyun 1 | Enes Barış | `ObjeSurukleme`, `SepetOyunYoneticisi` — detay: `ENES_MINI_OYUN_1.md` |
| 5 — Mini Oyun 2 | Mustafa Üz | `HarfSecmeYoneticisi`, `SecenekBalonu` |
| 6 — UI / font / entegrasyon | Süleyman Öz | `SahneGecisi`, font, ses/efekt Inspector bağlama, genel test |

Başkasının paket klasörüne izinsiz script eklemeyin; API ihtiyacı varsa sahip kişiyle konuşun.

---

## Isimlendirme

- `public` alan ve fonksiyon isimleri kılavuzdaki **Türkçe isimlerle sabittir**. Cursor’a değiştirtmeyin.
- Örnekler: `seciliHayvanIndex`, `ObjeBirakildi`, `harfRozeti`, `kalanSure`.
- Sahne/prefab adları: `SepetOyunu`, `Obje.prefab`, `HarfRozeti`, `ObjePozisyon1`…
- Tag: Mini Oyun 1 balonu için `Balon`.

---

## Cursor’a prompt verirken

1. Önce bu `AGENTS.md` dosyasını okutun; sonra tek bir script görevi verin.
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

## Bağımlılık sırası (özet)

```
Paket 1 + Paket 3  →  Paket 4 ve 5  →  Paket 6 entegrasyon
```

Paket 4, Paket 3’teki `SoruSecici` / `SkorYoneticisi` / `GeriBildirimYoneticisi` olmadan oyun yöneticisini tamamlamaz. Detaylı kapılar için `ENES_MINI_OYUN_1.md` Faz 3’e bakın.

---

## Kaynak kılavuzlar

- `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md` — adım adım görev + Cursor prompt örnekleri
- `Kodlama Ekibi Planı Bölüm 1.md` — paket dağılımı (mekanik çakışırsa detaylı kılavuz + bu AGENTS üstündür)
- `ENES_MINI_OYUN_1.md` — Enes Barış faz kapılı çalışma planı
