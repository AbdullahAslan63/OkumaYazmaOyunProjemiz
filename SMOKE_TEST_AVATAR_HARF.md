# Play smoke test — Avatar + Level + Sepet + HarfSecme

Unity’de bir kez (isteğe bağlı polish): **Oyun > Bolum1 Kurulumu (Avatar + HarfSecme)**  
Play için runtime kurucular yeterli: `AvatarSahneKurucu`, `LevelEkraniKurucu`, `SepetDemoKurucu`, `HarfSecmeSahneKurucu`.

Ortak sözleşme: 60 sn `GunesSayac`, harf+obje sesi (doğru), TekrarOyna, `dogruPuan=1`.

## AvatarOlusturmaEkrani

- [ ] Play: Kedi/Tavşan/Kuş/Rakun ileri-geri
- [ ] Renk / aksesuar / Devam → LevelEkranı
- [ ] PlayerPrefs kalıcılığı

## LevelEkranı

- [ ] Seviye 1 → SepetOyunu; Seviye 2 → HarfSecmeOyunu
- [ ] Yıldızlar önceki oyunlardan görünür

## SepetOyunu (Mini Oyun 1)

- [ ] Sağ üstte `GunesSayac` (ışınlar sönüyor)
- [ ] Yeni turda harf sesi duyulur
- [ ] Doğru bırakmada obje adı sesi + tik; yanlışta çarpı, obje yuvaya
- [ ] Geri bildirim sırasında güneş durur
- [ ] Bitiş: tebrik + yıldız + Doğru:N + Tekrar Dene + Ana Menü
- [ ] Tekrar Dene skoru/güneşi sıfırlar

## HarfSecmeOyunu (Mini Oyun 2)

- [ ] “Hangisinin baş harfi” prompt → harf sesi
- [ ] Sağ üstte `GunesSayac`; HUD’da `Doğru: N`
- [ ] 4 UI seçenek + yükseliş; yanlışta aynı soru; doğruda obje sesi + yeni soru
- [ ] Bitiş: Tekrar Dene (Mavi) + Ana Menü
- [ ] `Resources/Harfler` 8 SO yüklü

## Build Settings

- [ ] AvatarOlusturmaEkrani, LevelEkranı, SepetOyunu, HarfSecmeOyunu
