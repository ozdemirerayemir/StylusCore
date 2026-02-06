---
description: StylusCore Product Vision, Core Philosophy & Design DNA
---

# 🌌 StylusCore — Product Vision & Core Philosophy

> **Bu belge StylusCore’un zihnidir.**  
> Mimari, UI, input sistemi, feature tasarımı ve gelecekte eklenecek her şey  
> **buradaki prensiplere uyumlu olmak zorundadır**.
>
> Bu dosya; geliştirici, tasarımcı ve yapay zekâ agent’lar için  
> **Tek Kaynak (Single Source of Truth)** olarak kabul edilir.

---

## 0. Amaç (Why)

StylusCore, klasik not alma uygulamalarının ötesine geçen,
**stylus-first**, **sonsuz tuval** temelli, **aşırı özelleştirilebilir**
bir üretim ve düşünme alanıdır.

Amaç:
- Kullanıcıyı uygulamaya değil, **uygulamayı kullanıcıya uydurmak**
- Grafik tablet kullanıcılarının (Wacom / Huion / XP-Pen)
  not alma, diagram çizme ve düşünce haritalama ihtiyaçlarını **tam verimle** karşılamak
- Power-user ihtiyaçlarını **kullanıcı dostu** bir şekilde sunmak

StylusCore:
- Sadece “not yazılan” bir uygulama değil
- **Düşünme, üretme ve organize etme alanıdır**

---

## 1. Temel Felsefe

### 1.1 Stylus First (Pen-Centric Design)

StylusCore, fare-klavye merkezli değil, **kalem merkezli** düşünür.

- Pen input bir ek özellik değil, **birincil etkileşim biçimidir**
- Input gecikmesi kabul edilemez
- Stylus butonları, basılı tutma ve gesture’lar birinci sınıf vatandaştır
- Grafik tablet kullanan profesyoneller (Blender, ZBrush vb.)
  aynı donanımla not alabilmelidir

#### Simultaneous Pen & Touch
- Kalem (Pen) yazı/çizim için kullanılırken
- El (Touch) **sadece navigasyon** (Pan / Zoom / Rotate) yapar
- Kusursuz **Palm Rejection** zorunludur
- Pen ve Touch aynı anda, çakışmadan çalışabilmelidir

---

### 1.2 Extreme Customization

> “Kullanıcıya uymayan özellik yoktur, ayarlanmamış özellik vardır.”

- UI, input, shortcut, mod, menü, ölçek, font… her şey ayarlanabilir olmalıdır
- Varsayılanlar sadece **başlangıç noktasıdır**
- Kullanıcı, uygulamayı kendi zihinsel modeline göre şekillendirebilmelidir

---

## 2. Canvas & Page Model

StylusCore, kullanıcının düşünme biçimini tek bir tuval türüyle sınırlandırmaz.

### 2.1 Hibrit Tuval Yapısı (Hybrid Canvas)

Kullanıcı proje bazlı olarak farklı tuval türleri seçebilir:

- **Infinite Canvas**
  - Sınırsız alan
  - Diagram, mind-map, serbest çizim
- **Fixed Page Formats**
  - A4, A5, Letter
  - **Portrait (Dikey) / Landscape (Yatay)** varyantları
  - Baskı ve export odaklı kullanım

Tuval sistemi:
- Sabit bir liste değil
- **Genişletilebilir bir model** olarak tasarlanmalıdır

---

### 2.2 Kağıt & Arka Plan Özelleştirme

Her sayfa aşağıdaki arka plan türlerinden birini kullanabilir:

- Grid (kareli)
- Lined (çizgili)
- Dotted (simetrik / asimetrik)
- Music staff (nota kağıdı)
- Düz (boş)

Ek özellikler:
- Çizgi yoğunluğu ayarlanabilir
- Arka plan rengi ayarlanabilir
- Göz konforu önceliklidir

---

### 2.3 Custom Page Templates & Background Import

StylusCore sadece hazır şablonlar sunmaz.
Kullanıcı **kendi arka planlarını** da sisteme ekleyebilmelidir.

- Tercih edilen format: **SVG**
- PNG desteklenebilir (yüksek çözünürlük şartıyla)

Import edilen şablonlar:
- Kullanıcıya özel saklanır
- “Page Template Library” içinde listelenir
- Yeni projelerde tekrar kullanılabilir

Arka plan ile sayfa rengi birlikte çalışmalıdır:
- Overlay rengi
- Opacity
- Temel sayfa rengi
kullanıcı tarafından ayarlanabilir

---

### 2.4 Export & Sharing Vision

StylusCore içindeki veri, uygulama içine **hapsedilmemelidir**.

- Fixed Page → PDF
- Infinite Canvas →
  - Yüksek çözünürlüklü PNG
  - Dilimlenmiş (Sliced) PDF
  - SVG (diagram odaklı)

---

## 3. UI, Scale & Görsel Tutarlılık

### 3.1 DPI & Scale Awareness

- 1080p / 2K / 4K
- Windows Scale (%100 / %125 / %150)

Kurallar:
- PNG ikon yasak
- Sadece vektörel ikonlar (Path / Geometry)
- Layout’lar deterministik olmalı (jitter yok)

---

### 3.2 Typography Freedom

- UI fontu kullanıcı tarafından seçilebilir
- UI font boyutu ve yoğunluğu ayarlanabilir
- Hiçbir font veya size hardcoded olamaz
- Tüm değerler `DynamicResource` ile gelmelidir

---

## 4. Shell UI vs Editor Layer

### 4.1 Shell UI
- Sidebar
- Library
- Settings
- Navigation
- Radial Menu konfigürasyonu

Shell UI:
- Editor performansını asla etkilememelidir
- Hafif ve modüler olmalıdır

### 4.2 Editor Layer
- Canvas
- Ink engine
- Page layout
- Drawing / writing tools

Editor:
- Düşük gecikmeli
- Büyük veri setlerine dayanıklı
- Uzun süre açık kalabilmelidir

---

## 5. Interaction Modes (Mental Model)

StylusCore, **donanım değil davranış** temelli modlar kullanır.

- **Text Mode** → Metin üretimi
- **Ink Mode** → Kalemle yazma/çizme
- **Shape / Diagram Mode**
- **Navigation Mode**

Mode:
- Bir UI değil
- **Input’un nasıl yorumlandığını belirleyen davranış setidir**

---

## 6. Radial Menu System (Mode-Specific)

Radial Menu:
- Sadece UI değil
- **Bir etkileşim motorudur**

Her Mode:
- Kendi radyal menülerine sahiptir
- Bir modda **birden fazla radial** olabilir

Kullanıcı:
- İstediği kadar radial oluşturabilir
- Her radial için:
  - Eleman sayısı
  - Sıra
  - Boyut
  - Hold / Toggle davranışı
  - Atanan komutlar
ayarlar üzerinden belirlenir

---

## 7. Performance, Stability & Data Safety

Performans bir optimizasyon değil, **tasarım gereksinimidir**.

- Low latency input
- UI thread bloklanmaz
- RAM kontrollü kullanılır

### Zero Data Loss
- Kullanıcı bir çizgi çizdiği an veri güvendedir
- Crash durumunda **son stroke’a kadar recovery**
- “Save” butonu psikolojiktir
- Sistem arka planda sürekli kayıt alır

---

## 8. Text Mode, Writing Models & Input Methods

### 8.1 Writing Models

Text Mode iki yazım modeline sahiptir:

#### Free Text
- Tuvalin herhangi bir yerine tıklanır
- Bağımsız text container oluşur
- Infinite Canvas için varsayılandır

#### Flow / Linear Text
- Sadece sabit sayfa formatlarında (A4 / A5 / Letter)
- Word / OneNote benzeri davranış
- Satır akışı, paragraph, alignment desteklenir

Infinite Canvas’ta Flow Mode **bilinçli olarak devre dışıdır**.

---

### 8.2 Text Input Methods

Text üretimi **Input Method** katmanı ile belirlenir:

- Physical Keyboard
- On-Screen Keyboard
- **Voice (Speech-to-Text)**

Input Method:
- Writing Model’dan bağımsızdır
- Aynı yazım modeli farklı inputlarla çalışabilir

---

### 8.3 Inline Input Bar

Kullanıcı insertion point oluşturduğunda (caret):

Caret yakınında küçük bir **Inline Input Bar** görünür:

- 🎙️ Voice input
- ⌨️ Keyboard input

Voice aktifken:
- Konuşma **anlık olarak** seçili noktaya yazılır
- Geçici metin (partial) farklı stil ile gösterilebilir
- Final metin normal forma döner

---

## 9. Voice Input & Whisper Integration

### 9.1 Whisper Model Kullanımı

StylusCore, ses → metin için **Whisper** tabanlı bir sistem kullanmayı hedefler.

- Çoklu dil desteği
- Otomatik dil algılama
- Türkçe + İngilizce + karışık terimler desteklenir

Varsayılan davranış:
- Manuel dil seçimi gerekmez
- Whisper otomatik algılar

---

### 9.2 Voice Recording Modes

İki kayıt davranışı desteklenir:

- **Push-to-Talk** (varsayılan)
- **Toggle Recording**

Bu davranış ayarlardan değiştirilebilir.

---

## 10. Settings Philosophy (Input & Voice)

Ayarlar ekranı:
- Sadece “genel ayarlar” değil
- Kullanıcının **çalışma biçimini** tanımladığı yerdir

Voice & Input ayarları:
- Varsayılan input method
- Kullanılacak mikrofon cihazı
- Push / Toggle tercihi
- Auto-stop silence (opsiyonel)

Bu ayarlar:
- Text Mode
- Radial Menu
- Inline Input Bar
ile uyumlu çalışır

---

## 11. Localization & Accessibility

- Çoklu dil desteği zorunludur
- UI string’leri hardcoded olamaz
- Font, scale, contrast ayarlanabilir olmalıdır

---

## 12. Non-Goals

StylusCore:
- Mobile-first değildir
- Zorunlu cloud bağımlı değildir
- Sosyal ağ değildir
- Real-time collaborative editör değildir

---

## 13. Agent’lar İçin Altın Kural

> **Bu belgeye aykırı hiçbir tasarım veya kod kararı kabul edilemez.**

Her agent:
- Feature eklerken
- UI tasarlarken
- Mimari önerirken

**önce bu belgeyi okumak zorundadır.**

---

## Sonuç

Bu belge:
- StylusCore’un karakteridir
- Geliştirme pusulasıdır
- Yapay zekâlar için bağlamdır

Bu belge güncellenebilir,
ama **temel felsefe korunmalıdır**.
