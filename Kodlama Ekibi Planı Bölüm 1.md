# Kodlama Ekibi Planı — Bölüm 1 (Sadeleştirilmiş Sürüm)

Bu plan, `Grafik_Rehberi_Yusuf_Seyyid.md` ile birebir uyumludur ve **ilk defa proje yapan ilkokul/ortaokul öğrencileri** için mümkün olduğunca sade tutuldu. Amaç: az dosya, az soyut kavram, herkesin "ben bu parçanın tamamını anlıyorum" diyebileceği görev paketleri.

**Önemli mimari yaklaşım:** Bu bölümde sepet ve balon var, ama Bölüm 2, 3, 4... tamamen farklı mekanikler (eşleştirme, yapboz, hafıza oyunu, her ne olursa) olabilir. Bu yüzden burada "her bölümde işe yarayacak dev bir genel sistem" kurmaya **çalışmıyoruz** — böyle bir sistem soyut ve karmaşık olur, kurması da öğrenmesi de zor olur. Bunun yerine: Bölüm 1'i kendi içinde basit ve anlaşılır kuruyoruz. Bölüm 2 geldiğinde muhtemelen yapılacak şey, benzeyen kısımları (avatar sistemi, skor, ses) olduğu gibi kullanmak, farklı olan kısımlar için de sıfırdan basit yeni kod yazmak olacak. Bu, "genel bir çatı kur" yaklaşımından çok daha kolay öğretilir ve anlaşılır.

---

## 🎨 Grafik Rehberindeki Basitleştirme Kod Tarafını da Kolaylaştırdı

Göz rengi özelleştirmesi kaldırıldığı ve gözler artık avatarın ana çizimine gömülü olduğu için, kod tarafında **ayrı göz katmanları, ayrı renklendirme mantığı yok.** Avatar sistemi artık sadece iki basit şeyi yapıyor:
1. Doğru hayvan + doğru poz (normal/mutlu/üzgün) resmini göster
2. O resme seçilen rengi uygula (`SpriteRenderer.color` — tek satır kod)

Bu, bir çocuğun ilk haftada rahatça anlayıp yazabileceği seviyede bir iş.

---

## 📁 Script Klasör Yapısı (Sade)

```
Assets/_Scripts/
├── Avatar/
│   ├── AvatarYoneticisi.cs      (hangi hayvan/renk/ifade seçili, kaydetme)
│   ├── AvatarGorunumu.cs        (seçileni ekranda gösterme: sprite + renk + aksesuar)
│   ├── AvatarSecimEkrani.cs     (hayvanlar arası ileri/geri gezinme)
│   ├── RenkSecici.cs            (renk butonlarına tıklama)
│   └── AksesuarSecici.cs        (aksesuar butonlarına tıklama)
├── Ortak/
│   ├── HarfObjeVerisi.cs        (hangi harfte hangi objeler var — veri kartı)
│   ├── SoruSecici.cs            (rastgele harf seçme + o harfin objelerini getirme)
│   ├── SkorYoneticisi.cs        (skoru tutma ve gösterme)
│   ├── GeriBildirimYoneticisi.cs (doğru/yanlış ikon+ses+avatar ifadesi değiştirme)
│   └── SesYoneticisi.cs         (fon müziği + ses efektleri)
├── SepetOyunu/
│   ├── SepetHareketi.cs         (balonu sürükleyerek/dokunarak hareket ettirme)
│   └── SepetOyunYoneticisi.cs   (obje düşürme, yakalama kontrolü, süre, harf rozeti)
└── HarfSecmeOyunu/
    ├── HarfSecmeYoneticisi.cs   (büyük harf gösterme, 4 seçenek oluşturma)
    └── SecenekBalonu.cs         (tek bir seçenek balonuna tıklanınca ne olacağı)
```

**Toplam: 13 script.** Önceki taslakta 24'ün üzerindeydi — yarıya indirdik. Her dosya, tek bir çocuğun oturup baştan sona okuyup anlayabileceği büyüklükte tutulmalı (bir dosya 150-200 satırı geçiyorsa, o iş muhtemelen iki dosyaya bölünmeli — ama şimdilik bu kadarı yeterli).

**Not:** "Veri kartı" dediğimiz `HarfObjeVerisi.cs`, Unity'de **ScriptableObject** denen bir özellik kullanır — kod yazmadan, Unity ekranında sürükle-bırak ile doldurulan bir dosya türü. Bunu "bir harfin hangi resimlerle eşleştiğini yazdığımız bir kutu" gibi düşünebilirsiniz. Tek zor kavram budur, tüm ekibe bir kere gösterilmesi yeterli.

---

## 🧩 Her Dosyanın Ne Yaptığı (Sade Anlatım)

### Avatar Sistemi

**`AvatarYoneticisi.cs`** — Oyuncunun seçtiği hayvanı, rengi ve şu anki ifadeyi (normal/mutlu/üzgün) hatırlayan basit bir "hafıza" scripti. Sahne değişse bile unutmaması gerekiyor (`DontDestroyOnLoad` — "bu objeyi silme" demenin Unity'deki yolu). Seçimleri `PlayerPrefs` ile cihaza kaydeder (oyunu kapatıp açsa bile hatırlar).

**`AvatarGorunumu.cs`** — Avatarın göründüğü her yerde (seçim ekranı, balonun içi) kullanılan script. Yaptığı iş:
- `AvatarYoneticisi`'ndeki seçime göre doğru resmi (`avatar_kedi_normal.png` gibi) göster
- O resme seçilen rengi uygula: `spriteRenderer.color = seciliRenk;`
- Aksesuar seçiliyse, onu ayrı bir resim olarak üstüne ekle (renklendirme yok, olduğu gibi)

**`AvatarSecimEkrani.cs`** — İleri/geri butonlarına basınca 4 hayvan arasında geçiş yapar.

**`RenkSecici.cs`** — Renk kutucuklarına tıklanınca `AvatarYoneticisi`'ndeki rengi değiştirir, `AvatarGorunumu` otomatik güncellenir.

**`AksesuarSecici.cs`** — Aksesuar kutucuklarına tıklanınca seçili aksesuarı değiştirir.

**Çocuklara anlatılması gerekenler:** Bir GameObject'e script ekleme, `public` değişkenlerle Inspector'dan resim/renk atama, buton `OnClick` bağlama, `if/else` ile "hangi hayvan seçili" kontrolü.

---

### Ortak Sistemler

**`HarfObjeVerisi.cs`** — Her harf için: harf karakteri (örn. 'A') + o harfle başlayan objelerin resim listesi + o harfle başlamayan (çeldirici) birkaç obje. Bu, Unity ekranında doldurulacak bir veri kartı — 8 harf için 8 tane oluşturulacak.

**`SoruSecici.cs`** — Oyun başında ve her doğru cevapta rastgele bir harf seçer (aynı harf arka arkaya gelmesin diye basit bir kontrol), o harfin doğru+yanlış objelerini `SepetOyunYoneticisi` veya `HarfSecmeYoneticisi`'ne verir. **Her iki oyun da bu tek scripti kullanır** — kod tekrarı yok.

**`SkorYoneticisi.cs`** — `skor` adında bir sayı tutar, `SkorEkle(int miktar)` diye bir fonksiyonu vardır, ekrandaki yazıyı günceller.

**`GeriBildirimYoneticisi.cs`** — Doğru/yanlış olduğunda: ikon gösterir, ses çalar, avatarın ifadesini (mutlu/üzgün) birkaç saniyeliğine değiştirir sonra normale döner. Parıltı efektini (Adım 8'deki görsel, Unity'nin `ParticleSystem`'iyle) tetikler.

**`SesYoneticisi.cs`** — Seçili hayvana göre fon müziği çalar, obje toplanınca "harf-kelime" seslendirmesini çalar (ses dosyaları hazır olduğunda eklenecek — şimdilik boş bırakılabilir, sonra doldurulur).

**Çocuklara anlatılması gerekenler:** `List<>` (bir liste tutma), `Random.Range` (rastgele seçim), `AudioSource.Play`, basit bir zamanlayıcı (`Invoke` veya `Coroutine` — "X saniye sonra şunu yap").

---

### Mini Oyun 1 — Sepetle Toplama

**`SepetHareketi.cs`** — Sadece hareket: parmakla/fare ile sürükleme, balonu yatayda sağa-sola götürme.

**`SepetOyunYoneticisi.cs`** — Bu oyunun "beyni":
- Süre sayacı (örn. 60-90 saniye — kesin sayıyı sen belirle)
- `SoruSecici`'den harf alır, balonun üstündeki boş rozet alanına o harfi yazar
- Ekranın üstünden karışık obje düşürür (doğru + yanlış karışık)
- Bir obje balona değince: doğru mu yanlış mı kontrol eder, `SkorYoneticisi` ve `GeriBildirimYoneticisi`'ni çağırır, yeni harf ister
- Süre bitince oyunu durdurur, sonuç ekranına geçer

**Çocuklara anlatılması gerekenler:** `Collider2D` + "trigger" modu (iki nesne birbirine değince haber alma), objeyi ekranda oluşturup yok etme (`Instantiate`/`Destroy`), basit bir zamanlayıcı.

---

### Mini Oyun 2 — Hangisinin Baş Harfi

**`HarfSecmeYoneticisi.cs`** — Bu oyunun "beyni":
- `SoruSecici`'den harf alır, ekranda büyük gösterir
- 4 balon oluşturur (1 doğru obje + 3 yanlış obje, karışık sırada)
- Süre sayacı (Mini Oyun 1'le aynı mantık)

**`SecenekBalonu.cs`** — Her balona eklenen küçük bir script: "bana tıklanınca, ben doğru muyum yanlış mıyım" kontrolü yapar ve sonucu `HarfSecmeYoneticisi`'ne bildirir. (Bunun güzel yanı: her balon kendi kontrolünü kendi yapıyor, `HarfSecmeYoneticisi`'nin 4 balonun hepsini tek tek kontrol etmesine gerek kalmıyor — daha kolay takip edilir.)

**Çocuklara anlatılması gerekenler:** Bir prefab'ı (hazır obje şablonu) `Instantiate` ile 4 kere oluşturma, `OnMouseDown` (bir nesneye tıklanınca ne olacağı), listeyi karıştırma (basit: `Random.Range` ile listeden sırayla rastgele çekme).

---

## ❓ Senin Netleştirmen Gereken Kararlar

1. Her iki oyunda da süre kaç saniye? (İkisinde de aynı olması, kod tekrarını azaltır.)
2. Yanlış cevapta can/hayat azalıyor mu, yoksa sınırsız deneme mi?
3. Mini Oyun 2'de yanlış cevapta soru değişiyor mu, aynı soru mu kalıyor?
4. Ses dosyaları (müzik, harf seslendirmeleri, efekt sesleri) ne zaman, kimden gelecek?
5. Skor/harf rozeti/büyük harf için hangi font kullanılacak? (Grafik ekibinin Adım 6'da seçeceği font.)

---

## 👥 6 Kişilik Görev Dağılımı

Paketler, birbirine mümkün olduğunca az bağımlı ve farklı sahnelerde çalışacak şekilde ayrıldı — çakışma riski düşük. Kime hangi paketi vereceğine sen karar ver.

### 📦 Paket 1 — Avatar Sistemi (Temel)
**Dosyalar:** `AvatarYoneticisi.cs`, `AvatarGorunumu.cs`
**Bağımlılık:** Yok, ilk başlar. Paket 2, 4 ve 5 buna bağımlı olduğu için erken bitmesi önemli.
**Bilmesi gerekenler:** `public` değişken, Inspector'dan resim/renk atama, `SpriteRenderer.color`, `DontDestroyOnLoad`, `PlayerPrefs`
**Kullanacağı asset:** Adım 1 (avatar), Adım 2 (aksesuar)

### 📦 Paket 2 — Avatar Seçim Ekranı
**Dosyalar:** `AvatarSecimEkrani.cs`, `RenkSecici.cs`, `AksesuarSecici.cs`
**Bağımlılık:** Paket 1 (en azından iskelet halinde — paralel başlanabilir)
**Bilmesi gerekenler:** UI Button, `OnClick`, `if/else`
**Kullanacağı asset:** Adım 3 (arayüz parçaları)

### 📦 Paket 3 — Ortak Sistemler
**Dosyalar:** `HarfObjeVerisi.cs`, `SoruSecici.cs`, `SkorYoneticisi.cs`, `GeriBildirimYoneticisi.cs`, `SesYoneticisi.cs`
**Bağımlılık:** Yok, Paket 1 ile paralel başlar. Paket 4 ve 5 buna bağımlı, öncelikli bitmeli.
**Bilmesi gerekenler:** ScriptableObject (veri kartı) kavramı, `List<>`, `Random.Range`, `AudioSource`
**Kullanacağı asset:** Adım 7 (objeler), Adım 8 (efekt)
**Not:** Bu paketi en sistemli düşünen çocuğa vermen faydalı olur — çünkü hem Mini Oyun 1 hem 2 buna dayanıyor.

### 📦 Paket 4 — Mini Oyun 1: Sepetle Toplama
**Dosyalar:** `SepetHareketi.cs`, `SepetOyunYoneticisi.cs`
**Bağımlılık:** Paket 1 (avatarın sepette görünmesi) ve Paket 3 (harf/obje verisi) bitmiş olmalı
**Bilmesi gerekenler:** Collider2D/trigger, `Instantiate`/`Destroy`, basit zamanlayıcı
**Kullanacağı asset:** Adım 4 (arkaplan), Adım 5 (balon), Adım 7 (objeler)

### 📦 Paket 5 — Mini Oyun 2: Hangisinin Baş Harfi
**Dosyalar:** `HarfSecmeYoneticisi.cs`, `SecenekBalonu.cs`
**Bağımlılık:** Paket 3 (harf/obje verisi) bitmiş olmalı
**Bilmesi gerekenler:** Prefab + `Instantiate`, `OnMouseDown`, liste karıştırma
**Kullanacağı asset:** Adım 7 (objeler) — arkaplan için Adım 4'ü paylaşabilir ya da ayrı istenebilir (netleştir)

### 📦 Paket 6 — UI + Ses/Efekt Bağlama + Test
**Kapsam:** Skor/harf yazılarının font ayarı (TextMeshPro kurulumu), efekt/ses dosyalarını sistemlere bağlama, sahneler arası geçiş butonları, genel test
**Bağımlılık:** Diğer paketlerin en azından iskelet halinde bitmiş olması — proje sonuna doğru yoğunlaşır, ama font kurulumuna en baştan başlanabilir
**Bilmesi gerekenler:** TextMeshPro temelleri, `SceneManager.LoadScene`, temel test (elle deneme, hata bulma)
**Kullanacağı asset:** Adım 6 (font, UI ikonları), Adım 8 (efekt)
**Not:** Bu kişi aynı zamanda "her şey birlikte çalışıyor mu" kontrolünü yapan kişi olacağından, detaylara dikkat eden birine verilmesi iyi olur.

---

## 🗓️ Önerilen Sıra

```
1. Hafta:  Paket 1 + Paket 3 paralel başlar
           Paket 6'nın font kurulumu paralel başlayabilir
2. Hafta:  Paket 1 ve 3 biter → Paket 2, 4, 5 başlar
3. Hafta:  Paket 2, 4, 5 devam eder
4. Hafta:  Paket 2, 4, 5 biter → Paket 6 tam entegrasyon + test
```

---

## 🔮 Bölüm 2 Geldiğinde Ne Olacak?

Basit kural: **Avatar sistemi (Paket 1), skor ve ses sistemleri (Paket 3'ün bir kısmı) olduğu gibi tekrar kullanılır.** Bölüm 2'nin mekaniği (eşleştirme, yap-boz, her ne olursa) tamamen farklıysa, o mekanik için **sıfırdan, yeni ve basit** scriptler yazılır — Bölüm 1'in mini oyun scriptlerini (Sepet/HarfSecme) zorla genişletmeye veya "her ihtimale uyacak" hale getirmeye çalışılmaz. Bu, her bölümü kendi başına anlaşılır tutar ve çocukların her seferinde "bu iş nasıl yapılır" diye küçük, somut bir problem çözmesini sağlar — büyük, soyut bir sistemi anlamaya çalışmak yerine.
