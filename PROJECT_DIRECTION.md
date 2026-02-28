# StylusCore — Master Blueprint (V1.3 FINAL)
> **Tek kaynak konsept dosyası.** > Bu doküman, StylusCore’un vizyonunu, etkileşim modelini, içerik (object) sistemini, radial menü davranışlarını ve feature ekleme kurallarını tek bir yerde toplar.  
> Yeni bir AI veya geliştirici, bu dosyayı okuduktan sonra projeyi **A’dan Z’ye** anlamalıdır.

---

## 0) Kısa Tanım (One-liner)
**StylusCore**, klavye ile yapılandırılmış dokümantasyon ve grafik tablet/kalem ile serbest çizimi aynı editörde birleştiren, **tool-based** ve **deterministic** (öngörülebilir) davranışa sahip hibrit bir not/editör uygulamasıdır.

> Not: Kullanılan grafik tablet **touch/pinch gesture desteklemez**. Zoom/pan gibi navigasyonlar **mouse/keyboard + tablet tuşları/dial** üzerinden tasarlanır.

---

## 1) Ürün Kimliği ve Vizyon

### 1.1 Problem: Neyi çözmeye çalışıyoruz?
Klasik “doküman editörleri” (Word vb.) serbest çizim/yerleşim konusunda kısıtlıdır.  
Serbest “canvas” uygulamaları ise (çoğu not/çizim uygulaması) doküman düzeni ve yapısal yazımda zayıftır.
**Amaç:** Serbestlik sunarken yapısal netliği kaybetmemek.

### 1.2 Ürün İlkeleri
- **Deterministic behavior:** Gizli otomasyon yok. Her eylem açık ve öngörülebilir sonuç üretir.
- **Explicit tool switching:** Tool/mode değişimi daima kullanıcı tarafından yapılır ve görünürdür.
- **User-controlled state transitions:** Sistem “tahmin” yapmaz, kullanıcı “seçer”.
- **Object-type separation:** Text / ink / shapes / media türleri birbirine karışmadan aynı yüzeyde yaşar.
- **Modüler genişleyebilirlik:** Yeni tool ve object türleri eklenebilir olmalı.
- **Resource-driven UI:** Görsel stiller hardcode edilmez; merkezi resource’lardan gelir.

---

## 2) Global Hiyerarşi (En Üst Seviye)
Global hiyerarşi **sadece 3 seviyedir**:
Library
  └── Notebook
        └── Editor

### 2.1 Library
Konu/kategori bazlı üst konteyner. Örn: “Yazılım”, “Üniversite”, “Kişisel” vb.

### 2.2 Notebook
Library içindeki “kitap”. Konu bazlı içerik grubu. Örn: “C#”, “Python”, “Fizik” vb.

### 2.3 Editor
Notebook içeriğinin düzenlendiği ana çalışma alanı.
> **Not:** “Section” ve “Page” global hiyerarşi değildir; Editor içindedir.

---

## 3) Editor İç Yapısı (Editor-Internal Structure)
Editor şunları barındırır:
- **Sections (Sidebar Chapters):** Konu başlıkları / chapter’lar
- **Pages:** Section içindeki çalışma sayfaları
- **Active Surface:** Aktif page’in düzenlendiği yüzey (canvas)

---

## 4) UI Katmanları (User Interface Layers)
UI görsel olarak şu bölümlere ayrılır (modüler):
- Header
- Ribbon
- Sidebar (Sections/Pages)
- Canvas (Active Surface)
- Panels (properties, inspector, tools, AI vb.)

> **Amaç:** Bağımsız evrim. Bir panel değişirken canvas mimarisi çökmemelidir.

---

## 5) Etkileşim Modeli: Tool/Modes

### 5.1 Terminoloji
- **Mode (Kullanıcı dili):** Kullanıcının algıladığı durum (Ink Mode, Text Mode vb.)
- **Tool (Sistem dili):** Input’u yorumlayıp object üreten sistem implementasyonu (PenTool, TextTool vb.)

### 5.2 Core Modes (V1)
- Navigation Mode (pan/zoom/scroll)
- Ink Mode
- Text Mode
- Eraser Mode
- Shape Mode
- Selection Mode

### 5.3 Altın Kural
**Tool otomatik değişmez.** Kullanıcı tetikler.

---

## 6) Canvas ve Sayfa Boyutu
### 6.1 Desteklenenler
- Infinite Canvas
- Standard sizes: A2/A3/A4/A5/Letter vb.
- Orientation: Portrait / Landscape

### 6.2 Kural
Sayfa boyutu ve yönü **Page property**’dir. Aynı notebook içinde farklı sayfa boyutları desteklenebilir.

---

## 7) Object Model: “Her Şey Object”
Her şey object graph olarak yönetilir.

### 7.1 Object Türleri
- **InkStroke** (kalem/fırça vuruşları)
- **FreeTextBlock** (floating text)
- **DocumentText** (flow/structured text)
- **Shape** (parametrik şekiller)
- **Image** (görsel)
- **VoiceBlock** (audio + transcript)

### 7.2 Object-Type Separation
Object’ler aynı yüzeyde **üst üste durabilir** ama silme/düzenleme/seçme kuralları tür bazlı **izole** olmalıdır (edit kuralları birbirine karışmaz).

### 7.3 Z-Order
Sadece **render sırası** (öne getir/arkaya gönder) için kullanılır. Edit mantığını etkilemez.

---

## 8) Text Sistemi
İki ayrı text paradigması desteklenir:

### 8.1 Free Text (Floating Text Box)
Taşınabilir, serbest alan. Text Tool ile box çizilir, yazı o alan içinde (sabit genişlikte) akar.

### 8.2 Document Text (Structured)
Word benzeri yazım, paragraflar, structured layout.

### 8.3 Text Özellikleri & Profiller
- Font, Size, Bold/Italic/Underline, Color, Alignment.
- **Default Profiles:** Kullanıcı favori preset/preset tanımlayabilir.

---

## 9) Ink Sistemi
StylusCore bir "note-first editor"dür; ancak güçlü eskiz/diyagram desteği vardır.

### 9.1 Pen Engine vs Preset
- **Tool/Engine (Az):** Temel motor.
- **Preset (Çok):** Kullanıcı tercihleri (Örn: Ballpoint Blue 0.4).

### 9.2 Core Tool Set
1. **Ballpoint Pen:** Net, düşük jitter (basınç etkisi yok/az).
2. **Fountain Pen:** Basınçla kalınlık değişimi belirgin.
3. **Brush Pen:** Kalın vuruş, kaligrafi (yüksek basınç etkisi).
4. **Pencil:** Eskiz için texture/graphite hissi.
5. **Highlighter:** Yarı saydam, text üstüne highlight (object separation korunur).

### 9.3 Ortak Parametreler
Color, Thickness, Opacity, Smoothing, Stabilization, Pressure sensitivity (enable/disable), Pressure curve.
*Basınç, sistem tarafından normalize edilir (0..1 aralığı).*

### 9.4 Stroke Veri Modeli
Performans ve kalite için Stroke şunları tutar:
- Points: (x,y,time)
- Pressure: (0..1)
- ToolId (engine) ve PresetId
- BoundingBox (hızlı hit-test)
- Versioning

### 9.5 Çizim Şablonları (V2)
A4 yatay/dikey sayfa açılabilir. Çizim için grid, dotted gibi şablonlar ileriki versiyonlara kalır.

---

## 10) Eraser Sistemi
Text ve ink birbirini **otomatik silmez**.
- **Stroke Eraser:** Tüm stroke’u siler.
- **Point Eraser:** Stroke’un bir kısmını siler.
- **Object Eraser:** Seçili object’i siler.
- **Smart Eraser:** (V2+) Akıllı davranış.

---

## 11) Shape Sistemi
Shapes, stroke değildir; parametrik ve editable object'lerdir.

### 11.1 V1 Shape Set
Rectangle, Ellipse, Line, Arrow.

### 11.2 Shape Özellikleri
Fill color, border color, border thickness, opacity. (Resize/rotate edilebilir).

---

## 12) Radial Menu Sistemi (Workflow Engine)
Radial menu StylusCore’un hız ve workflow sistemidir. Business logic içermez, sadece Tool/Preset/Command tetikler.

### 12.1 Temel Mantık
Kullanıcı istediği kadar menü oluşturabilir. Her radial menu birden fazla trigger (keybind) destekler.

### 12.2 Multiple Keybind Desteği
Bir menü şunlarla tetiklenebilir: Keyboard, Mouse buttons, Tablet express keys, Pen buttons, Dial press. *(Conflict aynı profile içinde engellenir).*

### 12.3 Açılma Modları & İptal
- **Modlar:** Hold-to-select (basılı tut) veya Toggle.
- **Cancel Behavior:** Center deadzone (imleç merkeze gelirse iptal), Esc, Cancel slot.
- **Cursor:** Cursor warp edilmez; menü imlecin olduğu yerde açılır.

### 12.4 Menu İçeriği & Kurallar
- **Item Türleri:** Sadece ToolItem, PresetItem, CommandItem, SubmenuItem eklenebilir. Special-case hack yasaktır.
- **Nested Radial:** Maksimum depth 2'dir.
- **Önerilen Slot:** 6-8 slot idealdir.
- **Input Capture:** Radial açıkken çizim/text input kapalıdır (yanlışlıkla işlem yapılmasını önler).

---

## 13) Settings Sistemi
Ayarlar ekranı modülerdir ve profillere dayanır: `General`, `Input Profiles`, `Radial Menu`, `Tools & Presets`, `View & Navigation`.

### 13.1 Input Profiles
İki ana profil vardır ve her biri tamamen bağımsız ayar/menü seti tutar:
1. Keyboard & Mouse Profile
2. Tablet & Pen Profile
*(Örn: Radial menüler ve favori preset'ler bu profillere göre ayrı ayrı saklanır).*

---

## 14) Context Menu (Sağ Tık)
Sağ tık menüleri seçilen object type’a göre değişir (Word/PowerPoint yaklaşımı):
- delete, duplicate, transform
- text styling shortcuts
- shape adjustments

---

## 15) Input Model
Mouse + Keyboard ve Graphic Tablet tam desteklenir. "Tool abstraction" kullanılır. Implicit (gizli) tool switching veya "tahmin" yoktur.

---

## 16) Selection & Transform
**Selection (Ayrı bir Tool'dur):**
- Default: Single select
- Shift: Multi-select
- Box select (Selection tool ile sürükleyerek)
- *Kural:* Tıklanan noktada **ZIndex’i en yüksek object** seçilir.

**Transform:**
- Move: Text, Shape, Image, Stroke
- Resize: Text, Shape, Image (V1'de stroke resize yoktur).

---

## 17) Clipboard / Paste
- **Ctrl+V:** Clipboard'da image varsa `Image object` olur. Text varsa aktif context'e veya `FreeTextBlock`'a yapışır.
- **Yerleşim:** İmleç konumu biliniyorsa oraya, bilinmiyorsa **viewport center**'a yapıştırılır.
- **Ctrl+Shift+V:** Plain text olarak yapıştır.

---

## 18) Voice / Whisper (STT)
İki mod:
1. **Canvas Voice:** Alana metin kutusu çizer, sesi metne çevirip oraya koyar (VoiceBlock + Text).
2. **Document Voice:** İmleç konumuna sesi metin olarak ekler.
*Kural:* STT sonucu her zaman metne dönüşür ve UI thread’i bloklamaz.

---

## 19) AI Desteği
AI bir "helper layer"dır (rewrite, summarize, improve).
*Kural:* AI doğrudan datayı değiştirmez, öneri sunar, kullanıcı uygular.

---

## 20) Kaydetme & Autosave
- 30–60 saniye aralıklı autosave.
- Crash recovery ve snapshot yaklaşımı hedeflenir.

---

## 21) Undo / Redo
Sistem **Command-based** tasarlanır. (AddStroke, MoveObject, PasteImage vb.)
Sürekli eylemler (drag move, ink stroke) tek komut olarak gruplanır (begin -> commit).

---

## 22) Navigasyon (Pan/Zoom)
- **Zoom:** Ctrl + MouseWheel, Tablet Dial, veya Ribbon/Panel üzerinden dropdown (50%, 100%, Fit to Page). Zoom, cursor konumunu anchor (merkez) alır.
- **Pan:** Space + Drag (Mouse) veya Middle Mouse Drag.

---

## 23) Tablet Özel Davranışları
- Pen pressure altyapısı mevcuttur.
- Pen Button 1: Radial trigger (varsayılan).
- Pen Button 2: Eraser modifier (opsiyon).
- Dial Rotate: Zoom veya brush size.
- Palm Rejection: Accidental input filtreleri.

---

## 24) Performans Kuralları
- **UI Thread asla bloklanmaz:** STT, AI istekleri, IO ve export background thread'de yapılır.
- Commit anında data state ve render state tutarlı olmalı, **single source of truth** hedeflenmelidir.

---

## 25) UI/XAML Kuralları (Görsel Disiplin)
- **Resource-driven UI:** Hardcoded renk yasaktır, merkezi DynamicResource kullanılır.
- Shadowing (kırık key / aynı key'in birden fazla yerde tanımlanması) yasaktır.
- Custom stiller key'li olmalıdır.
- Editor projesi olduğu için erken aşamada performans adına code-behind normaldir.

---

## 26) Feature Ekleme "Blueprint" Kuralı
Yeni özellik eklerken sorulur: Yeni Tool mu? Yeni Object mi? Yeni Command mi? Parametre mi?
*Kural:* "Şu sayfaya özel istisna" gibi special-case hack'ler kesinlikle yasaktır. Her özellik genel modele oturmalıdır.

---

## 27) Roadmap (V1 Odaklı)
**V1 Hedefleri:** Library/Notebook hiyerarşisi, temel tool set (Pen, Text, Shape), Radial Menu (çekirdek yapı), Basic STT, Autosave, Selection & Transform.
**V2+ Adayları:** Advanced brush engines, Smart eraser, Plugin architecture, Alignment/snapping, Lasso select.

---

## 28) Yönetim Notu
Detaylı mimari kurallar ve zorunluluklar `.agent/` altında bulunur.
Bu dosya StylusCore’un **tek kaynak konsept dokümanıdır**. Yeni bir feature eklemeden önce daima bu doküman referans alınır.