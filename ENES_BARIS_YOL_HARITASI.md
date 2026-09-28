# Enes Barış — Yol Haritası

Kaynak: `DETAYLI GÖREV DAĞILIMI VE KILAVUZ.md`, Paket 4.  
Kişi: Enes Barış. Branch: `enes/level_1`.  
Oyun: Mini Oyun 1, sahne adı `SepetOyunu`.

Balon sabit durur. Objeler ekranın altında durur. Oyuncu objeyi sürükleyip balonun üzerine bırakır.

## Nasıl ilerlenir

Fazlar 0, 1, 2, 3, 4, 5, 6, 7 sırasıyladır. Bir fazın sondaki **Bitti** listesindeki her madde tamamlanmadan sonraki faza geçilmez. Agent da kullanıcı da bu sırayı bozmaz.

İş ikiye ayrılır:

- **Sen (Unity):** Hierarchy, sahne, collider, tag, prefab, Inspector sürükleme, Play testi.
- **Cursor:** Yalnızca o fazın scripti. Bir fazda bir script. Faz 5’te kod yok, yalnız plan var.

Başka paketlerin dosyalarına (avatar, harf verisi, skor, geri bildirim, ikinci mini oyun, menü) dokunulmaz. Onların sınıf adları Paket 4 scriptlerinde aynen kullanılır: `SoruSecici`, `SkorYoneticisi`, `GeriBildirimYoneticisi`, `HarfObjeVerisi`.

---

## Faz 0 — Sözleşmeyi kilitle

Amaç: Kod ve sahne kurmadan önce isimlerin ve sınırın net olması.

1. Bu dosyayı ve `AGENTS.md` dosyasını oku.
2. Şu isimleri bir yere not et; sonraki fazlarda bunlar değişmez:
   - Scriptler: `ObjeSurukleme`, `SepetOyunYoneticisi`
   - Sahne: `SepetOyunu`
   - Tag: `Balon`
   - Prefab yolu: `Assets/_Prefabs/Obje.prefab`
   - Pozisyon objeleri: `ObjePozisyon1`, `ObjePozisyon2`, `ObjePozisyon3`, `ObjePozisyon4`
   - Rozet: `HarfRozeti`
   - Yönetici objesi: `OyunYoneticisi`
3. Klasörleri oluştur: `Assets/_Scripts/Sepet/`, `Assets/_Prefabs/`, `Assets/_Scenes/`.
4. Paket 3 scriptlerinin bu branch’te olmadığını kabul et. Onları burada yazma. Faz 6’da derleme için o scriptlerin projede durması gerekir; yoksa Faz 7 testi “bitti” sayılmaz, eksik paket ismiyle durulur.

**Bitti**

- [ ] İsim listesi değişmeden duruyor.
- [ ] Üç klasör var.
- [ ] Başka pakete ait script eklenmedi.

Bu liste kapanmadan Faz 1 açılmaz.

---

## Faz 1 — Sahne ve arkaplan (Adım 4.1)

Amaç: Boş `SepetOyunu` sahnesi ve arkada duran zemin.

Unity’de, kod yok:

1. File > New Scene. Sahneyi `Assets/_Scenes/SepetOyunu.unity` olarak kaydet. Sahnenin adı `SepetOyunu` olsun.
2. Arkaplan görselini Hierarchy’ye sürükle.
3. Sprite Renderer içinde `Order in Layer` değerini `-10` yap. Oyun objeleri bunun önünde kalır.

**Bitti**

- [ ] Sahnede arkaplan görünüyor.
- [ ] `Order in Layer` değeri `-10`.
- [ ] Dosya `Assets/_Scenes/SepetOyunu.unity`.

Bu liste kapanmadan Faz 2 açılmaz.

---

## Faz 2 — Sabit balon ve harf rozeti (Adım 4.2)

Amaç: Balon hareket etmeyen bir hedef. Üzerinde harf yazılacak rozet var.

Unity’de, kod yok:

1. Balon görselini sabit bir konuma yerleştir. Objeyi `Balon` diye adlandır.
2. Add Component > Box Collider 2D.
3. Collider’da **Is Trigger** işaretli olsun.
4. Tag listesine `Balon` tag’ini ekle ve bu objeye ata.
5. Balonun child’ı olarak 3D TextMeshPro ekle. Adı `HarfRozeti`.
6. Play’de balonun yerinde kaldığını kontrol et. Bu fazda sürükleme yok.

**Bitti**

- [ ] Obje adı `Balon`.
- [ ] Box Collider 2D var ve Is Trigger açık.
- [ ] Tag `Balon`.
- [ ] Child yazı objesinin adı `HarfRozeti`.

Bu liste kapanmadan Faz 3 açılmaz.

---

## Faz 3 — Obje prefab’ı ve dört sabit yer (Adım 4.3)

Amaç: Sürüklenecek objenin kalıbı ve ekranın altında duracakları dört nokta.

Unity’de, kod yok:

1. Hierarchy > Create Empty. Adı `Obje`.
2. Add Component > Sprite Renderer.
3. Add Component > Box Collider 2D. **Is Trigger kapalı** kalsın. Balonun collider’ı trigger, objeninki değil.
4. Bu objeyi `Assets/_Prefabs/Obje.prefab` olarak kaydet. Sahnedeki örnek prefab’dan üretildiyse Hierarchy’de kalabilir; asıl kalıp prefab dosyasıdır.
5. Ekranın alt kısmına dört boş obje koy:
   - `ObjePozisyon1`
   - `ObjePozisyon2`
   - `ObjePozisyon3`
   - `ObjePozisyon4`
6. Dördü de alt kenarda, birbirine girmeyecek aralıkta dursun.

**Bitti**

- [ ] `Assets/_Prefabs/Obje.prefab` var.
- [ ] Prefab’ta Sprite Renderer ve trigger olmayan Box Collider 2D var.
- [ ] Dört pozisyon objesi doğru isimlerle altta duruyor.

Bu liste kapanmadan Faz 4 açılmaz.

---

## Faz 4 — ObjeSurukleme scripti (Adım 4.4)

Amaç: Obje parmağı veya fareyi takip etsin. Balona yeterince yakın bırakılırsa yöneticiye haber versin. Uzaksa başladığı yere dönsün.

Bu fazda yalnız `ObjeSurukleme.cs` yazılır. `SepetOyunYoneticisi` bu fazda yazılmaz. Script, bırakma anında `SepetOyunYoneticisi.Instance.ObjeBirakildi(this)` çağırır. O sınıf Faz 6’da geleceği için bu fazın bitişi Play’de derlenme değildir; bitiş, scriptin kılavuzla aynı sözleşmeyi taşımasıdır.

Cursor’a verilecek tek prompt:

> Unity için ObjeSurukleme adında bir 2D sürükle-bırak C# script yaz. public string objeAdi ve public char dogruHarf alanları olsun. Start()'ta başlangıç pozisyonunu bir private değişkende sakla. OnMouseDown ile sürüklemeyi başlat, OnMouseDrag ile Camera.main.ScreenToWorldPoint(Input.mousePosition) kullanarak objeyi fareye/parmağa takip ettir (z ekseni 0 kalsın), OnMouseUp'ta sürüklemeyi bitir: 'Balon' tag'li objeyi GameObject.FindGameObjectWithTag ile bul, Vector3.Distance ile aradaki mesafeyi ölç, eğer 1.5 birimden yakınsa SepetOyunYoneticisi.Instance.ObjeBirakildi(this) çağır, değilse objeyi başlangıç pozisyonuna geri götür. Her satıra Türkçe yorum ekle.

Dosya: `Assets/_Scripts/Sepet/ObjeSurukleme.cs`.

Yazıldıktan sonra satır satır oku. Şunlar duruyorsa faz kapanır:

- `public string objeAdi`
- `public char dogruHarf`
- `OnMouseDown`, `OnMouseDrag`, `OnMouseUp`
- Mesafe eşiği 1.5
- Yakınsa `SepetOyunYoneticisi.Instance.ObjeBirakildi(this)`
- Değilse başlangıç pozisyonuna dönüş
- z ekseni 0

**Bitti**

- [ ] Yalnızca `ObjeSurukleme.cs` eklendi.
- [ ] Alan adları prompt’taki gibi.
- [ ] Yakın / uzak bırakma davranışı kodda okunuyor.
- [ ] `SepetOyunYoneticisi.cs` henüz yok.

Bu liste kapanmadan Faz 5 açılmaz.

---

## Faz 5 — SepetOyunYoneticisi planı (Adım 4.5, kod yok)

Amaç: Projenin en karmaşık scriptine geçmeden önce yapıyı görmek ve onaylamak.

Bu fazda C# dosyası oluşturulmaz. Cursor Plan Mode’da yalnız plan yazar. Sen planı okuyup onaylamadan Faz 6 başlamaz.

Planın kapsaması gereken davranış:

- Süre tutulur (başlangıç `kalanSure`, örnek 75 saniye) ve ekranda gösterilir. Süre bitince oyun durur.
- Rastgele bir harf seçilir ve `HarfRozeti` üzerine yazılır.
- Dört sabit pozisyona, o harfin doğru objeleri ve başka harflerden çeldiriciler prefab’tan üretilir.
- Obje balona yakın bırakılınca: doğruysa puan eklenir ve yeni tur başlar; yanlışsa yalnız geri bildirim gösterilir. Obje kendi scriptiyle eski yerine döner.

Planda şu alanlar ve fonksiyonlar adlarıyla geçmeli:

- Alanlar: `soruSecici`, `skorYoneticisi`, `geriBildirim`, `objePrefab`, `objePozisyonlari`, `harfRozeti`, `sureYazisi`, `kalanSure`
- Fonksiyonlar: `YeniTurBaslat()`, `ObjeleriOlustur()`, `ObjeBirakildi(ObjeSurukleme obje)`, `OyunuBitir()`
- `Update` içinde `Time.deltaTime` ile geri sayım, `Instantiate`, `Destroy`, `static Instance`

Cursor’a verilecek plan promptu:

> Unity için SepetOyunYoneticisi adında bir oyun yöneticisi scripti yazacağım. Görevi: Oyun süresini (örn. 75 saniye) tutmalı ve ekranda göstermeli, süre bitince oyunu durdurmalı. Rastgele bir harf seçip balonun rozetine yazmalı. Ekrandaki 4 sabit pozisyona, o harfe ait doğru objeler + başka harflerden çeldirici objeler yerleştirmeli (prefab'dan Instantiate ile). Bir obje balona doğru bırakıldığında: doğruysa puan ekleyip yeni tur başlatmalı, yanlışsa sadece geri bildirim göstermeli (obje zaten kendi kendine eski yerine dönüyor). Kod yazmadan önce, bu script için nasıl bir yapı kurman gerektiğini adım adım plan olarak anlat. Public alanlar şunlar olacak: SoruSecici soruSecici, SkorYoneticisi skorYoneticisi, GeriBildirimYoneticisi geriBildirim, GameObject objePrefab, Transform[] objePozisyonlari, TextMeshPro harfRozeti, TextMeshProUGUI sureYazisi, float kalanSure. Fonksiyonlar: YeniTurBaslat, ObjeleriOlustur, ObjeBirakildi, OyunuBitir.

**Bitti**

- [ ] Plan yazıldı, kod dosyası yok.
- [ ] Plan, yukarıdaki alan ve fonksiyon adlarını kullanıyor.
- [ ] Enes Barış planı açıkça onayladı.

Onay cümlesi gelmeden Faz 6 açılmaz.

---

## Faz 6 — SepetOyunYoneticisi kodu (Adım 4.5, onaydan sonra)

Amaç: Onaylanan planı tek script olarak yazmak.

Dosya: `Assets/_Scripts/Sepet/SepetOyunYoneticisi.cs`.

Cursor’a verilecek kod promptu (plan onayından sonra):

> Planı onayladım, şimdi bu C# kodunu yaz. Şu public alanları kullan: SoruSecici soruSecici, SkorYoneticisi skorYoneticisi, GeriBildirimYoneticisi geriBildirim, GameObject objePrefab, Transform[] objePozisyonlari, TextMeshPro harfRozeti, TextMeshProUGUI sureYazisi, float kalanSure (başlangıç 75). ObjeleriOlustur() fonksiyonunda: aktif harfin dogruObjeler listesinden 1-2 tanesini, başka rastgele harflerin objelerinden 2-3 çeldirici seç, hepsini karıştır, objePozisyonlari dizisindeki pozisyonlara objePrefab'ı Instantiate et, her birinin ObjeSurukleme bileşenine doğru sprite/objeAdi/dogruHarf değerlerini ata. YeniTurBaslat, ObjeBirakildi ve OyunuBitir fonksiyonlarını da yaz. Singleton Instance kullan. Her satıra Türkçe yorum ekle.

Kod okunurken kontrol:

- Doğru bırakmada skor artar ve `YeniTurBaslat()` çağrılır.
- Yanlış bırakmada yalnız geri bildirim çalışır; pozisyonu `ObjeSurukleme` düzeltir.
- Süre `Update` içinde azalır, bitince `OyunuBitir()` yeni tur açmaz.
- Çeldiriciler ile doğru objeler karışık dört pozisyona dağılır.
- Alan adları Faz 5 listesiyle aynıdır.

Paket 3 scriptleri projede yoksa kod yine bu isimlerle yazılır ve faz burada durur. Eksik tip uydurulmaz, boş sahte sınıf açılmaz.

**Bitti**

- [ ] Yalnızca `SepetOyunYoneticisi.cs` eklendi.
- [ ] Faz 5’teki adlar kodda aynı.
- [ ] `ObjeSurukleme` hâlâ `ObjeBirakildi(this)` çağırıyor.
- [ ] Paket 3 yoksa bu durum not edildi ve Faz 7’ye geçilmedi.

Bu liste kapanmadan Faz 7 açılmaz.

---

## Faz 7 — Bağlama ve Play testi (Adım 4.6)

Amaç: Scriptleri sahneye bağlayıp sürükle-bırak turunu gerçekten oynamak.

Unity’de:

1. Hierarchy > Create Empty. Adı `OyunYoneticisi`.
2. `SepetOyunYoneticisi` scriptini bu objeye ekle.
3. Inspector’dan doldur:
   - `soruSecici`, `skorYoneticisi`, `geriBildirim` (Paket 3 objeleri)
   - `objePrefab` → `Obje.prefab`
   - `objePozisyonlari` → dört pozisyon, sıra 1’den 4’e
   - `harfRozeti` → balondaki `HarfRozeti`
   - `sureYazisi` → süre yazısı
   - `kalanSure` → 75
4. `Obje` prefab’ına `ObjeSurukleme` ekli olsun. `objeAdi` ve `dogruHarf` oyun yöneticisi tur başlarken doldurur.
5. Play.

Test listesi, hepsi geçmeden faz kapanmaz:

- [ ] Oyun açılınca balonda bir harf görünüyor.
- [ ] Altta dört obje dört pozisyonda duruyor.
- [ ] Obje sürükleniyor, z sapmıyor.
- [ ] Balondan uzağa bırakınca obje eski yerine dönüyor.
- [ ] Doğru obje balona yakın bırakılınca puan artıyor ve yeni tur geliyor.
- [ ] Yanlış obje balona yakın bırakılınca puan artmıyor, geri bildirim geliyor, obje eski yerine dönüyor.
- [ ] Süre azalıyor. Süre bitince yeni tur açılmıyor, oyun duruyor.
- [ ] Balon hiç hareket etmiyor.

Bir madde takılırsa düzeltme yalnız o maddeyi ilgilendiren scriptte yapılır. Yeni özellik, ikinci mini oyun veya menü bu fazın parçası değildir.

**Bitti:** yukarıdaki sekiz madde geçti. Paket 4 burada kapanır.
