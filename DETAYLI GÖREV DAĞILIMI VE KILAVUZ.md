# Detaylı Uygulama Kılavuzu — Bölüm 1 (Cursor Destekli Sürüm)

Bu kılavuz, Unity editöründe **elle yapılması gereken** adımları (sahne kurulumu, buton oluşturma, obje yerleştirme — bunları Cursor yapamaz, bunlar editörde fiziksel tıklama gerektirir) ile, **Cursor'a yaptırılacak kod işlerini** ayırıyor.

Her script için sana kod vermiyorum — bunun yerine **net bir görev tanımı**, hangi değişken/fonksiyonların olması gerektiği, ve çocukların Cursor'a verebileceği **hazır bir prompt örneği** veriyorum. Kod, çocuğun Cursor ile üretip anlayarak entegre edeceği kısım.

---

## 📖 Hızlı Sözlük

| Terim                            | Basit Anlamı                                                                                        |
| -------------------------------- | --------------------------------------------------------------------------------------------------- |
| **GameObject**                   | Sahnedeki her şey. Boş bir tane: Hierarchy'de sağ tık > Create Empty                                |
| **Component**                    | Bir GameObject'e eklenen özellik (SpriteRenderer, Collider2D, script). Inspector'da "Add Component" |
| **Prefab**                       | Bir GameObject şablonu, kod ile çoğaltılabilir (Instantiate)                                        |
| **Canvas**                       | UI elemanlarının (buton, yazı) içinde durduğu kapsayıcı. GameObject > UI > Canvas                   |
| **Script oluşturma**             | Project'te ilgili klasöre sağ tık > Create > C# Script                                              |
| **OnClick bağlama**              | Butonun Inspector'ındaki "On Click ()" kutusuna GameObject + fonksiyon sürükleme                    |
| **Inspector'dan referans atama** | `public` değişkene Inspector'dan dosya/obje sürükleme                                               |

---

## 🎯 Cursor'a Prompt Verirken Dikkat Edilecekler (Ekibe Baştan Anlat)

1. **Bağlam ver:** Cursor'a projenin `AGENTS.md` dosyasını (klasör yapısı, isimlendirme kuralları) okutup, sonra görev tanımını isteyin.
2. **Tek script iste:** Bir seferde tek bir scriptin görevini verin, birden fazlasını karıştırmayın.
3. **`public` alan isimlerini siz belirleyin, Cursor'a değiştirtmeyin:** Bu kılavuzdaki değişken isimlerini prompt'a aynen yazın — böylece Inspector'da bağlantı kurarken herkesin script'i aynı isimlerde olur, karışıklık olmaz.
4. **Kodu üretince hemen çalıştırmayın, önce okuyun:** "Bu satır ne yapıyor?" diye birbirinize sorun — amaç sadece çalışan kod değil, anlaşılan kod.
5. **Plan Mode kullanın:** Karmaşık scriptlerde (Paket 4/5'teki oyun yöneticileri gibi) önce Cursor'dan bir plan istetin, planı siz onaylayın, sonra kodu yazdırın.

---

## 🔄 ÖNEMLİ MEKANİK HATIRLATMA

Balon **sabit duruyor**. Objeler ekranın alt kısmında sabit duruyor, oyuncu istediğini **sürükleyip balonun üzerine bırakıyor.**

---

## 📦 PAKET 1 — MUSTAFA SAİD BAYRAM — Avatar Sistemi ve Seçim Ekranı

Bu paket eski **Paket 1 (Avatar Sistemi)** ile **Paket 2 (Avatar Seçim Ekranı)** görevlerini birleştirir. Önce temel avatar hafıza/görünüm sistemi (Adım 1.x), ardından seçim ekranı UI ve butonlar (Adım 2.x) yapılır.

### Adım 1.1 — Yönetici Objesini Oluştur (Unity Editöründe)

1. Hierarchy'de sağ tık > Create Empty, adı `AvatarYoneticisi`.
2. `Assets/_Scripts/Avatar/` klasörüne sağ tık > Create > C# Script > `AvatarYoneticisi`.
3. Scripti `AvatarYoneticisi` objesine ekle.

### Adım 1.2 — AvatarYoneticisi.cs Görev Tanımı

**Ne yapmalı:**

- Oyuncunun hangi hayvanı, hangi rengi ve hangi aksesuarı seçtiğini hafızada tutmalı.
- Sahne değişse bile bu bilgi kaybolmamalı (tek bir kopyası olmalı, her yerden erişilebilmeli).
- Seçimler cihaza kaydedilmeli, oyun kapanıp açılınca hatırlanmalı.

**Olması gereken public alanlar:**

- `int seciliHayvanIndex` (0=Kedi, 1=Tavşan, 2=Kuş, 3=Rakun)
- `Color seciliRenk`
- `int seciliAksesuarIndex` (-1 = aksesuar yok)

**Olması gereken fonksiyonlar:**

- `HayvanSec(int index)`
- `RenkSec(Color renk)`
- `AksesuarSec(int index)`

**Kullanılması beklenen Unity/C# kavramları:** `static` ile tek-obje erişimi (Singleton), `Awake()`, `DontDestroyOnLoad()`, `PlayerPrefs`

**Cursor'a örnek prompt:**

> "Unity için AvatarYoneticisi adında bir C# script yaz. Bu script bir Singleton olmalı (static Instance değişkeni ile, Awake() içinde ayarlanmalı, DontDestroyOnLoad kullanmalı, sahne değişse bile silinmemeli). Şu public alanları içermeli: `int seciliHayvanIndex` (varsayılan 0), `Color seciliRenk` (varsayılan beyaz), `int seciliAksesuarIndex` (varsayılan -1). Şu fonksiyonları içermeli: `HayvanSec(int index)`, `RenkSec(Color renk)`, `AksesuarSec(int index)` — her biri ilgili değişkeni günceller ve PlayerPrefs ile kaydeder. Kodun her satırına kısa Türkçe yorum ekle ki yeni öğrenen biri anlayabilsin."

### Adım 1.3 — Avatar Görselini Sahneye Yerleştir (Unity Editöründe)

1. Hierarchy'de boş obje `Avatar`, altına iki child: `Govde`, `Aksesuar`.
2. İkisine de **Add Component > Sprite Renderer** ekle.
3. `Aksesuar`ın `Order in Layer` değeri `Govde`'den 1 fazla olsun (üstte görünsün).

### Adım 1.4 — AvatarGorunumu.cs Görev Tanımı

**Ne yapmalı:**

- Ekranda o an seçili olan hayvanı, doğru pozda (normal/mutlu/üzgün) ve doğru renkte göstermeli.
- Aksesuar seçiliyse onu da üstüne eklemeli.
- `AvatarYoneticisi`'ndeki seçim her değiştiğinde, dışarıdan çağrılabilecek bir "yenile" fonksiyonu olmalı.

**Olması gereken public alanlar:**

- Her hayvan için ayrı ayrı: hayvan adı + normal/mutlu/üzgün sprite'ları tutan bir yapı, 4 elemanlı bir liste/dizi olarak (Inspector'da doldurulacak)
- `SpriteRenderer govdeRenderer`
- `SpriteRenderer aksesuarRenderer`

**Olması gereken fonksiyonlar:**

- `Guncelle()` — aktif seçimlere göre gövde ve aksesuar sprite'larını ve gövde rengini ayarlar

**Kullanılması beklenen kavramlar:** `[System.Serializable]` ile Inspector'da görünen özel veri yapısı, `SpriteRenderer.sprite`, `SpriteRenderer.color`

**Cursor'a örnek prompt:**

> "Unity için AvatarGorunumu adında bir C# script yaz. [System.Serializable] ile HayvanGorseli adında bir class tanımla: string hayvanAdi, Sprite normalPoz, Sprite mutluPoz, Sprite uzgunPoz alanları olsun. AvatarGorunumu script'inde public HayvanGorseli[] hayvanlar (4 elemanlı), public SpriteRenderer govdeRenderer, public SpriteRenderer aksesuarRenderer olsun. Guncelle() adında bir fonksiyon yaz: AvatarYoneticisi.Instance.seciliHayvanIndex'e göre hayvanlar dizisinden doğru hayvanı bulsun, govdeRenderer.sprite'a onun normalPoz'unu atasın, govdeRenderer.color'ı AvatarYoneticisi.Instance.seciliRenk yapsın. Start() içinde Guncelle()'yi çağırsın. Yorumlarla açıkla."

### Adım 1.5 — Bağlama ve Test (Unity Editöründe)

`Avatar` objesini seç, script alanlarını doldur (Govde/Aksesuar renderer'ları sürükle, `hayvanlar` listesine 4 hayvanın resimlerini `Assets/_Art/Avatar/Karakterler/`'den sürükle). Play'e bas, doğru resim/renk görünüyor mu kontrol et.

### Adım 2.1-2.2 — Canvas ve Butonlar (Unity Editöründe)

1. Hierarchy > sağ tık > UI > Canvas. Canvas Scaler: `Scale With Screen Size`, Reference Resolution belirle.
2. Canvas'a UI > Button - TextMeshPro ekle: `GeriButon` (sol), `IleriButon` (sağ). Source Image'lara grafik ekibinin ok görsellerini koy, yazıları sil.

### Adım 2.3 — AvatarSecimEkrani.cs Görev Tanımı

**Ne yapmalı:**

- İleri butonuna basınca sıradaki hayvana geçmeli (3'ten sonra 0'a dönmeli).
- Geri butonuna basınca öncekine geçmeli (0'dan önce 3'e dönmeli).
- Her geçişte hem `AvatarYoneticisi`'ndeki seçim güncellenmeli hem de `AvatarGorunumu.Guncelle()` çağrılmalı.

**Olması gereken public alanlar:**

- `AvatarGorunumu avatarGorunumu`

**Olması gereken fonksiyonlar:**

- `SonrakiHayvan()`
- `OncekiHayvan()`

**Kullanılması beklenen kavramlar:** `%` (mod alma) ile döngüsel sayaç

**Cursor'a örnek prompt:**

> "Unity için AvatarSecimEkrani adında bir C# script yaz. public AvatarGorunumu avatarGorunumu alanı olsun. SonrakiHayvan() fonksiyonu: AvatarYoneticisi.Instance.seciliHayvanIndex'i 1 artırsın, 4'e ulaşınca 0'a dönsün (mod alma kullan), AvatarYoneticisi.Instance.HayvanSec() ile kaydetsin, avatarGorunumu.Guncelle()'yi çağırsın. OncekiHayvan() aynı mantığı tersine yapsın (0'dan önce 3'e dönsün). Yorumla açıkla."

**Butonlara bağlama (Unity Editöründe):** `IleriButon` > On Click () > script objesini sürükle > `AvatarSecimEkrani.SonrakiHayvan()` seç. `GeriButon` için aynısı `OncekiHayvan()` ile.

### Adım 2.4 — Renk Seçici

**Unity Editöründe:** `RenkPaneli` (Horizontal Layout Group ile), içine 4 renk butonu.

**Görev tanımı:**

- Her renk butonuna tıklanınca o rengi `AvatarYoneticisi`'ne bildirmeli ve `AvatarGorunumu.Guncelle()` çağrılmalı.

**⚠️ Bilinmesi gereken teknik sınır:** Unity'nin buton `On Click()` sistemi `Color` gibi parametreleri kabul etmiyor. Bunu Cursor'a prompt yazarken belirtin, iki çözümden birini üretmesini isteyin: (a) her renk için ayrı parametresiz fonksiyon (`TuruncuSec()`, `PembeSec()` gibi), ya da (b) her buton üzerinde script'in kendi `public Color builtinRenk` alanını Inspector'dan ayarlayıp parametresiz `BuButonunRenginiSec()` çağırması. İkinci yöntem daha az kod tekrarı içerir — Cursor'a hangisini istediğinizi net söyleyin.

**Cursor'a örnek prompt:**

> "Unity için RenkSecici adında bir C# script yaz. public AvatarGorunumu avatarGorunumu ve public Color builtinRenk alanları olsun. BuButonunRenginiSec() adında parametresiz bir fonksiyon yaz: AvatarYoneticisi.Instance.RenkSec(builtinRenk) çağırsın, sonra avatarGorunumu.Guncelle() çağırsın. Bu script her renk butonuna ayrı ayrı eklenip builtinRenk alanı Inspector'dan farklı renklere ayarlanacak. Yorumla açıkla."

**Unity Editöründe:** Bu script'i her 4 renk butonuna da ekle, her birinin `builtinRenk` alanını Inspector'dan farklı bir renge ayarla, `On Click()`'e `BuButonunRenginiSec()`'i bağla.

### Adım 2.5 — Aksesuar Seçici

Aynı mantık — Cursor'a benzer bir prompt verilecek, `AksesuarSec(int index)` çağıran bir script. Bunu ekibe bir önceki adımı örnek göstererek kendilerinin yazmasını isteyebilirsin — iyi bir tekrar alıştırması.

### Adım 2.6 — Test

Play'e bas, tüm butonları dene.

---

## 📦 PAKET 3 — MUSTAFA YİĞİT AVAN — Ortak Sistemler

### Adım 3.1 — Harf Verisi (ScriptableObject)

**Unity Editöründe script oluşturma yolu aynı**, ama içerik Cursor'a yazdırılacak:

**Görev tanımı:**

- Her harf için: harf karakteri + o harfle başlayan objelerin resim listesi tutan bir veri kartı olmalı.
- Unity menüsünden (sağ tık > Create > ...) kolayca yeni veri kartı oluşturulabilmeli.

**Cursor'a örnek prompt:**

> "Unity için HarfObjeVerisi adında bir ScriptableObject C# scripti yaz. [CreateAssetMenu(fileName = \"YeniHarf\", menuName = \"Oyun/Harf Verisi\")] attribute'u kullan. public char harf ve public Sprite[] dogruObjeler alanları olsun. Yorumla açıkla."

**Unity Editöründe (script yazıldıktan sonra):**

1. `Assets/_Data/Harfler/` klasörü oluştur.
2. Sağ tık > Create > Oyun > Harf Verisi — 8 kere tekrarla (A, E, I, İ, O, Ö, U, Ü).
3. Her birine harf karakterini yaz, objeleri sürükle.

### Adım 3.2 — SoruSecici.cs

**Görev tanımı:**

- Tüm harflerin listesini tutmalı (Inspector'dan doldurulacak).
- İstenildiğinde rastgele bir harf seçip döndürmeli.
- Aynı harf arka arkaya iki kere seçilmemeli.

**Olması gereken public alanlar:**

- `HarfObjeVerisi[] tumHarfler`

**Olması gereken fonksiyonlar:**

- `HarfObjeVerisi YeniHarfSec()`

**Kullanılması beklenen kavramlar:** `Random.Range`, `do-while` döngüsü (ya da eşdeğeri) ile tekrar kontrolü

**Cursor'a örnek prompt:**

> "Unity için SoruSecici adında bir C# script yaz. public HarfObjeVerisi[] tumHarfler alanı olsun. YeniHarfSec() fonksiyonu tumHarfler'den rastgele bir eleman seçip döndürsün, ama bir önceki seçilenle aynı olmasın (bir önceki seçimi private bir değişkende tut). Yorumla açıkla."

### Adım 3.3 — SkorYoneticisi.cs

**Unity Editöründe:** Canvas'a UI > Text - TextMeshPro ekle, `SkorYazisi`.

**Görev tanımı:**

- Bir skor sayısı tutmalı, dışarıdan puan eklenebilmeli, ekrandaki yazı otomatik güncellenmeli.

**Cursor'a örnek prompt:**

> "Unity için SkorYoneticisi adında bir C# script yaz. TextMeshPro kullan (using TMPro). public TextMeshProUGUI skorYazisi alanı ve private int skor (0'dan başlasın) olsun. SkorEkle(int miktar) fonksiyonu skoru artırsın ve skorYazisi.text'i güncellesin. Yorumla açıkla."

### Adım 3.4 — GeriBildirimYoneticisi.cs

**Unity Editöründe:** İki UI Image (`DogruIkon`, `YanlisIkon`, başta kapalı), bir Particle System (`ParlıltıEfekti`).

**Görev tanımı:**

- Doğru cevapta: ikon 1 saniyeliğine göstermeli, parıltı efektini oynatmalı, doğru sesini çalmalı.
- Yanlış cevapta: ikon 1 saniyeliğine göstermeli, yanlış sesini çalmalı.

**Olması gereken public alanlar:**

- `GameObject dogruIkon`, `GameObject yanlisIkon`, `ParticleSystem parlıltıEfekti`, `AudioSource sesKaynagi`, `AudioClip dogruSesi`, `AudioClip yanlisSesi`

**Olması gereken fonksiyonlar:**

- `DogruGoster()`, `YanlisGoster()`

**Kullanılması beklenen kavramlar:** `Coroutine` (`IEnumerator`, `yield return new WaitForSeconds`), `PlayOneShot`

**Cursor'a örnek prompt:**

> "Unity için GeriBildirimYoneticisi adında bir C# script yaz. public GameObject dogruIkon, public GameObject yanlisIkon, public ParticleSystem parlıltıEfekti, public AudioSource sesKaynagi, public AudioClip dogruSesi, public AudioClip yanlisSesi alanları olsun. DogruGoster() fonksiyonu: dogruIkon'u 1 saniyeliğine aç-kapat (Coroutine kullan), parlıltıEfekti.Play() çağırsın, sesKaynagi.PlayOneShot(dogruSesi) çalsın. YanlisGoster() benzer şekilde yanlisIkon ve yanlisSesi ile çalışsın. Yorumla açıkla, Coroutine'in ne işe yaradığını kod içinde yorum olarak belirt."

### Adım 3.5 — SesYoneticisi.cs

**Görev tanımı:**

- Seçili hayvana göre 4 fon müziğinden birini çalmalı.
- Obje toplanınca ilgili "harf-kelime" seslendirmesini çalabilmeli (ses dosyaları geldiğinde).

**Cursor'a örnek prompt:**

> "Unity için SesYoneticisi adında bir C# script yaz. public AudioSource muzikKaynagi, public AudioClip[] hayvanMuzikleri (4 elemanlı) alanları olsun. Start() içinde AvatarYoneticisi.Instance.seciliHayvanIndex'e göre hayvanMuzikleri dizisinden doğru klibi seçip muzikKaynagi.clip'e atasın ve Play() çağırsın, loop = true olsun. Yorumla açıkla."

---

## 📦 PAKET 4 — ENES BARIŞ — Mini Oyun 1: Sepetle Toplama

### Adım 4.1-4.3 — Sahne Kurulumu (Unity Editöründe, kod gerektirmez)

1. Yeni sahne `SepetOyunu`. Arkaplanı sürükle (`Order in Layer -10`).
2. Balonu sabit bir konuma yerleştir, **Box Collider 2D** ekle, **Is Trigger** işaretle, `Balon` tag'i oluşturup ata.
3. Balonun üzerine 3D Text Mesh Pro ekle: `HarfRozeti`.
4. Boş obje `Obje`: Sprite Renderer + Box Collider 2D (trigger DEĞİL) ekle. Bunu prefab yap (`Assets/_Prefabs/Obje.prefab`).
5. Ekranın alt kısmına 4 tane boş obje (`Create Empty`) koy, isimlerini `ObjePozisyon1`, `ObjePozisyon2` diye sırala — bunlar objelerin duracağı sabit yerler olacak.

### Adım 4.4 — ObjeSurukleme.cs Görev Tanımı

**Ne yapmalı:**

- Fareyle/parmakla tutulduğunda objeyi takip etmeli (sürükleme).
- Bırakıldığında, balona yeterince yakınsa "bırakıldı" bilgisini oyun yöneticisine bildirmeli.
- Balona yakın değilse, objeyi başladığı yere geri döndürmeli.

**Olması gereken public alanlar:**

- `string objeAdi`, `char dogruHarf`

**Kullanılması beklenen kavramlar:** `OnMouseDown`, `OnMouseDrag`, `OnMouseUp`, `Camera.main.ScreenToWorldPoint`, `Vector3.Distance`, `GameObject.FindGameObjectWithTag`

**Cursor'a örnek prompt:**

> "Unity için ObjeSurukleme adında bir 2D sürükle-bırak C# script yaz. public string objeAdi ve public char dogruHarf alanları olsun. Start()'ta başlangıç pozisyonunu bir private değişkende sakla. OnMouseDown ile sürüklemeyi başlat, OnMouseDrag ile Camera.main.ScreenToWorldPoint(Input.mousePosition) kullanarak objeyi fareye/parmağa takip ettir (z ekseni 0 kalsın), OnMouseUp'ta sürüklemeyi bitir: 'Balon' tag'li objeyi GameObject.FindGameObjectWithTag ile bul, Vector3.Distance ile aradaki mesafeyi ölç, eğer 1.5 birimden yakınsa SepetOyunYoneticisi.Instance.ObjeBirakildi(this) çağır, değilse objeyi başlangıç pozisyonuna geri götür. Her satıra Türkçe yorum ekle."

### Adım 4.5 — SepetOyunYoneticisi.cs Görev Tanımı

**Ne yapmalı:**

- Oyun süresini (örn. 75 saniye) tutmalı ve ekranda göstermeli, süre bitince oyunu durdurmalı.
- Rastgele bir harf seçip balonun rozetine yazmalı.
- Ekrandaki 4 sabit pozisyona, o harfe ait doğru objeler + başka harflerden çeldirici objeler yerleştirmeli (prefab'dan `Instantiate` ile).
- Bir obje balona doğru bırakıldığında: doğruysa puan ekleyip yeni tur başlatmalı, yanlışsa sadece geri bildirim göstermeli (obje zaten kendi kendine eski yerine dönüyor).

**Olması gereken public alanlar:**

- `SoruSecici soruSecici`, `SkorYoneticisi skorYoneticisi`, `GeriBildirimYoneticisi geriBildirim`, `GameObject objePrefab`, `Transform[] objePozisyonlari`, `TextMeshPro harfRozeti`, `TextMeshProUGUI sureYazisi`, `float kalanSure` (başlangıç değeri örn. 75)

**Olması gereken fonksiyonlar:**

- `YeniTurBaslat()`, `ObjeleriOlustur()`, `ObjeBirakildi(ObjeSurukleme obje)`, `OyunuBitir()`

**Kullanılması beklenen kavramlar:** `Update()` içinde `Time.deltaTime` ile geri sayım, `Instantiate`, `Destroy`, Singleton (`static Instance`)

**Bu, projenin en karmaşık scripti — Cursor'a prompt vermeden önce Plan Mode ile önce bir plan çıkarttırın, planı birlikte gözden geçirin, sonra kodu yazdırın.**

**Cursor'a örnek prompt (ilk aşama — plan):**

> "Unity için SepetOyunYoneticisi adında bir oyun yöneticisi scripti yazacağım. Görevi: [yukarıdaki 'Ne yapmalı' listesini buraya yapıştır]. Kod yazmadan önce, bu script için nasıl bir yapı kurman gerektiğini adım adım plan olarak anlat."

**Cursor'a örnek prompt (ikinci aşama — kod, plan onaylandıktan sonra):**

> "Planı onayladım, şimdi bu C# kodunu yaz. Şu public alanları kullan: [yukarıdaki alan listesini yapıştır]. ObjeleriOlustur() fonksiyonunda: aktif harfin dogruObjeler listesinden 1-2 tanesini, başka rastgele harflerin objelerinden 2-3 çeldirici seç, hepsini karıştır, objePozisyonlari dizisindeki pozisyonlara objePrefab'ı Instantiate et, her birinin ObjeSurukleme bileşenine doğru sprite/objeAdi/dogruHarf değerlerini ata. Her satıra Türkçe yorum ekle."

### Adım 4.6 — Bağlama ve Test (Unity Editöründe)

Boş obje `OyunYoneticisi` oluştur, scripti ekle, tüm `public` alanları Inspector'dan doldur (Paket 3'teki objeler, Adım 4.3'teki prefab/pozisyonlar, Adım 4.2'deki rozet). Play'e bas, sürükle-bırağı test et.

---

## 📦 PAKET 5 — MUSTAFA ÜZ — Mini Oyun 2: Hangisinin Baş Harfi

**Basitleştirme:** Seçenekler için ayrı bir "balon" görseli istenmedi — Adım 2.4'teki renk seçim çerçevesi görseli tekrar kullanılacak, içine obje resmi konacak.

### Adım 5.1-5.2 — Sahne Kurulumu (Unity Editöründe)

1. Yeni sahne `HarfSecmeOyunu`. Canvas oluştur, büyük punto TextMeshPro (`BuyukHarf`).
2. UI > Button - TextMeshPro: `SecenekBalonu`. Source Image'a çerçeve görseli, altına child UI > Image: `ObjeResmi`. Yazı objesini sil. Prefab yap.

### Adım 5.3 — SecenekBalonu.cs Görev Tanımı

**Ne yapmalı:**

- Dışarıdan (oyun yöneticisinden) "bu obje resmi, bu harf, sorulan harf bu" bilgisini alıp kendini ayarlayabilmeli.
- Tıklandığında, kendi objesinin doğru olup olmadığını oyun yöneticisine bildirmeli.

**Olması gereken public alanlar:**

- `Image objeResmi`

**Olması gereken fonksiyonlar:**

- `Ayarla(Sprite resim, char objeHarfi, char soruHarfi)`
- `Tiklandi()`

**Cursor'a örnek prompt:**

> "Unity için SecenekBalonu adında bir C# script yaz (using UnityEngine.UI). public Image objeResmi alanı, private char dogruHarf ve private char aktifSoruHarfi alanları olsun. Ayarla(Sprite resim, char objeHarfi, char soruHarfi) fonksiyonu bu değerleri atasın ve objeResmi.sprite'ı ayarlasın. Tiklandi() fonksiyonu dogruHarf == aktifSoruHarfi kontrolü yapıp sonucu HarfSecmeYoneticisi.Instance.SecenekSecildi(bool) ile bildirsin. Yorumla açıkla."

### Adım 5.4 — HarfSecmeYoneticisi.cs Görev Tanımı

**Ne yapmalı:**

- Rastgele harf seçip büyük göstermeli.
- 1 doğru + 3 yanlış obje seçip karıştırıp 4 sabit pozisyona seçenek balonları oluşturmalı.
- Her balonun tıklama olayını kendi fonksiyonuna bağlamalı (`button.onClick.AddListener(...)`).
- Doğru seçilirse puan verip yeni soru sormalı, yanlış seçilirse geri bildirim göstermeli.

**Olması gereken public alanlar:**

- `SoruSecici soruSecici`, `SkorYoneticisi skorYoneticisi`, `GeriBildirimYoneticisi geriBildirim`, `GameObject secenekPrefab`, `Transform[] secenekPozisyonlari`, `TextMeshProUGUI buyukHarf`

**Olması gereken fonksiyonlar:**

- `YeniSoru()`, `SecenekSecildi(bool dogruMu)`

**Bu da karmaşık bir script — Plan Mode kullanın.**

**Cursor'a örnek prompt (plan aşaması):**

> "Unity için HarfSecmeYoneticisi adında bir oyun yöneticisi scripti yazacağım. Görevi: [yukarıdaki 'Ne yapmalı' listesini yapıştır]. Kod yazmadan önce plan olarak anlat."

**Cursor'a örnek prompt (kod aşaması):**

> "Planı onayladım, kodu yaz. YeniSoru() içinde: aktif harfin dogruObjeler'inden 1 tane, başka harflerden 3 çeldirici seç, 4 elemanlı listeyi karıştır (Fisher-Yates ya da basit Random.Range tabanlı karıştırma kullan), secenekPozisyonlari dizisindeki her pozisyona secenekPrefab'ı Instantiate et, her birinin SecenekBalonu bileşenine Ayarla() ile bilgileri ver, ve o butonun Button bileşenine onClick.AddListener(secenekBalonu.Tiklandi) ile tıklama olayını bağla. SecenekSecildi(bool dogruMu) fonksiyonu doğruysa skor ekleyip YeniSoru() çağırsın, yanlışsa sadece geri bildirim göstersin. Yorumla açıkla."

### Adım 5.5 — Bağlama ve Test

Boş obje oluştur, scripti ekle, alanları doldur, Play'e bas.

---

## 📦 PAKET 6 — SÜLEYMAN ÖZ — UI, Font, Efekt Bağlama ve Test

### Adım 6.1 — Font Kurulumu (Unity Editöründe, kod gerektirmez)

1. `.ttf` dosyasını `Assets/_Art/Fonts/` klasörüne koy.
2. Window > TextMeshPro > Font Asset Creator > Source Font File'a sürükle > Generate Font Atlas > Save.
3. Sahnedeki her TextMeshPro objesinde Font Asset alanına yeni fontu ata.

### Adım 6.2 — Efekt/Ses Bağlama

Paket 3'teki boş `AudioClip`/görsel alanlarını diğer ekiplerden gelen dosyalarla doldur (kod değişikliği gerekmez, sadece Inspector'da sürükleme).

### Adım 6.3 — SahneGecisi.cs Görev Tanımı

**Ne yapmalı:**

- Verilen isimdeki sahneye geçiş yapabilmeli.

**Olması gereken fonksiyonlar:**

- `SahneyeGit(string sahneAdi)`

**Cursor'a örnek prompt:**

> "Unity için SahneGecisi adında çok basit bir C# script yaz (using UnityEngine.SceneManagement). SahneyeGit(string sahneAdi) fonksiyonu SceneManager.LoadScene(sahneAdi) çağırsın. Yorumla açıkla."

**⚠️ Unutulmaması gereken adım:** File > Build Settings açılıp kullanılan tüm sahnelerin (`AvatarOlusturmaEkrani`, `SepetOyunu`, `HarfSecmeOyunu`) listeye eklenmesi gerekiyor — yoksa `LoadScene` çalışmaz. Bu en sık unutulan adım, ekibe özellikle hatırlat.

### Adım 6.4 — Genel Test Listesi

- [ ] Avatar seçimi kaydediliyor mu?
- [ ] Mini Oyun 1'de sürükle-bırak akıcı çalışıyor mu?
- [ ] Mini Oyun 2'de 4 seçenek doğru oluşuyor mu, aynı obje 2 kere gelmiyor mu?
- [ ] Süre bitince oyun düzgün duruyor mu?
- [ ] Skor doğru artıyor mu?
- [ ] Build Settings'te tüm sahneler ekli mi?

---

## 🗓️ Uygulama Sırası

```
1. Hafta:  Paket 1 (avatar temeli, Adım 1.x) + Paket 3 paralel başlar
2. Hafta:  Paket 1 devam (seçim ekranı, Adım 2.x) + Paket 4, 5 başlar
3. Hafta:  Paket 4, 5 devam — oyun yöneticisi scriptleri
           en çok zaman alacak kısım (Plan Mode kullanmayı unutmayın)
4. Hafta:  Paket 6 tam entegrasyon + test
```
