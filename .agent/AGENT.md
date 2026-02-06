---
description: StylusCore Agent Entry Point (Routing + Precedence + Output Contract)
---

# 🤖 StylusCore — AGENT.md (Önce bunu oku)

Bu dosya, AI agent'ların (Antigravity/Cursor vb.) **hangi görev için hangi kural dosyalarını** okuyacağını belirleyen tek giriş kapısıdır.

> **Kullanım:** Her promptta → "`.agent/AGENT.md` oku ve uygula"

---

## 0) Precedence (Çelişki olursa hangisi kazanır?)

1) **`.agent/01_CONSTITUTION.md`** (RED LINES) — her şeyin üstünde
2) **`.agent/00_PRODUCT_VISION.md`** (Product/UX Intent) — ürün niyeti & etkileşim modeli
3) Göreve özel dosyalar (Architecture/Canvas/Input/Persistence/XAML/Undo vb.)
4) Best practices / öneriler (dokümanlarda aksi yazmıyorsa)

Şüphede kalırsan:
- **RED LINE'ı bozma.**
- Ürün davranışı/UX kararında tereddüt varsa: **00_PRODUCT_VISION.md** kazanır.
- Uyumlu alternatif öner ve çatışmayı açıkça yaz.

---

## 1) Her görevden önce ZORUNLU

1. ✅ `.agent/00_PRODUCT_VISION.md` oku (özellikle UX/Mode/Text/Voice/Radial işleri için)
2. ✅ `.agent/01_CONSTITUTION.md` oku (RED LINES)
3. ✅ Bu dosyadan (AGENT.md) "Task Routing" bölümünden uygun kategoriyi seç
4. ✅ Kategoride listelenen dosyaları oku
5. ✅ Çıktını "Output Contract" formatında ver (aşağıda)

---

## 2) Task Routing — "Ne yapıyorsun?"

Aşağıdan en uygun kategoriyi seç. **Listelenen dosyalar okunmadan kod yazma.**

### A) Product/UX / Interaction Tasarımı (Mode, Radial, Text/Voice davranışı, akış)
Oku:
- `.agent/00_PRODUCT_VISION.md`
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md` (UI/Editor ayrımı, bağımlılıklar)
- `.agent/08_XAML_UI_HIERARCHY.md` (Shell UI kuralları)

> Not: Bu kategori “kod yazmadan önce doğru davranışı tanımlama” içindir.

### B) Editor/Canvas Engine (kamera, zoom/pan, scene graph, quadtree, selection)
Oku:
- `.agent/00_PRODUCT_VISION.md` (editor davranışı & intent)
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`
- `.agent/07_UNDO_REDO_COMMANDS.md` (canvas operasyonu ekliyorsan)

### C) Ink/Input/Threading (StylusPlugIn, buffer, wet/dry, palm rejection)
Oku:
- `.agent/00_PRODUCT_VISION.md` (pen+touch intent, latency hedefi)
- `.agent/01_CONSTITUTION.md`
- `.agent/04_INPUT_INK_THREADING.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`

### D) Text / Tables / Mixed Media (Free Text vs Flow Text, View-to-Edit, text formatting)
Oku:
- `.agent/00_PRODUCT_VISION.md` (Text Mode, Writing Models, Input Methods)
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/03_CANVAS_ENGINE.md` (View-to-Edit overlay kontratı)
- `.agent/05_RENDERING_PERF_WPF.md`
- `.agent/07_UNDO_REDO_COMMANDS.md`

### E) Voice / Whisper / Microphone (STT, inline bar, input device, settings)
Oku:
- `.agent/00_PRODUCT_VISION.md` (Voice intent + behavior)
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/06_PERSISTENCE_DB.md` (ayar/preset saklama gerekiyorsa)
- `.agent/07_UNDO_REDO_COMMANDS.md` (voice → text insertion undo/redo)
- `.agent/08_XAML_UI_HIERARCHY.md` (UI component & theming)

### F) Persistence/DB/File Format (SQLite, LZ4, autosave, recovery, chunking, template import)
Oku:
- `.agent/00_PRODUCT_VISION.md` (Zero Data Loss intent)
- `.agent/01_CONSTITUTION.md`
- `.agent/06_PERSISTENCE_DB.md`
- `.agent/03_CANVAS_ENGINE.md` (bounds/chunk kontratı için)
- `.agent/07_UNDO_REDO_COMMANDS.md` (persisted operations)

### G) App Shell / Navigation / MVVM / Dialogs
Oku:
- `.agent/00_PRODUCT_VISION.md` (Shell intent)
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/08_XAML_UI_HIERARCHY.md`

### H) XAML/UI (Sidebar, layout, components, theming, icons, localization, a11y)
Oku:
- `.agent/00_PRODUCT_VISION.md` (UI scale, typography freedom)
- `.agent/01_CONSTITUTION.md`
- `.agent/08_XAML_UI_HIERARCHY.md`
- `.agent/05_RENDERING_PERF_WPF.md` (virtualization, nested scroll ban)

### I) Undo/Redo (Command pattern, grouping, atomicity)
Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/07_UNDO_REDO_COMMANDS.md`
- `.agent/03_CANVAS_ENGINE.md` (quadtree/bounds update)
- `.agent/05_RENDERING_PERF_WPF.md` (dirty rect)
- `.agent/00_PRODUCT_VISION.md` (UX: user expectation, zero data loss)

### J) Genel Refactoring / Kod Kalitesi / Mimari Değişiklik
Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- İlgili domain dosyası (hangi katmandaysa)
- `.agent/00_PRODUCT_VISION.md` (davranış/UX etkisi varsa)

---

## 3) Dosya Haritası (Quick Reference)

| Dosya | Kapsam |
|-------|--------|
| `00_PRODUCT_VISION.md` | Ürün niyeti, UX, modlar, radial, text/voice, non-goals |
| `01_CONSTITUTION.md` | Master kurallar, RED LINES, tüm projeyi kapsar |
| `02_ARCHITECTURE.md` | Katman yapısı, bağımlılık yönü, klasör kuralları |
| `03_CANVAS_ENGINE.md` | Infinite canvas, camera, QuadTree, View-to-Edit |
| `04_INPUT_INK_THREADING.md` | StylusPlugIn, pen thread, wet/dry pipeline |
| `05_RENDERING_PERF_WPF.md` | DrawingVisual, dirty rect, virtualization |
| `06_PERSISTENCE_DB.md` | SQLite, R-Tree, LZ4, autosave, chunking, template storage |
| `07_UNDO_REDO_COMMANDS.md` | Command pattern, 50 step limit, grouping |
| `08_XAML_UI_HIERARCHY.md` | Shell UI, theming, icons, dialogs, a11y |

---

## 4) Output Contract (Agent çıktısı standardı)

Her görev için agent şu formatta cevap üretmeli:

1. **Okunan dokümanlar:** (dosya listesi)
2. **Vision alignment:** (00_PRODUCT_VISION ile uyum kontrolü: Mode/Text/Voice/Radial/Scale/Non-goals)
3. **Kuralların etkisi:** Her dosyadan 1 somut kural → bu görevde neye zorladı?
4. **Plan:** 3–8 adım
5. **Değişecek dosyalar:** path listesi
6. **Riskler:** perf/thread/persistence/memory/UX regressions
7. **Test/Doğrulama:** en az 3 madde (ölçüm/visual check dahil)

---

## 5) Hard Stops (Yapılmayacaklar)

Agent aşağıdakileri uygulamaz; uyumlu alternatif önerir:

| Yasak | Neden | Alternatif |
|-------|------|------------|
| Canvas'ta `ScrollViewer` | Pan/zoom için yanlış | `MatrixTransform` camera |
| Pen thread'de UI erişimi | Thread-safety | Buffer → UI thread pump |
| `ItemsControl` ile canvas render | Performans | `DrawingVisual` retained |
| Canvas'ta kalıcı `DataGrid`/`RichTextBox` | Memory | View-to-Edit overlay |
| Hardcoded string/color/font | Theming/i18n | `DynamicResource` |
| Icon `Path`'te fixed `Width/Height` ile yanlış ölçekleme | DPI/layout tutarsız | Wrapper + `Stretch` / geometry-first yaklaşım |
| UI thread'de sync I/O | Freeze | Background + snapshot |
| Core'da `System.Windows.*` | Headless | Primitives only |

---

## 6) Reference Integrity (Dosya taşındığında)

- **MUST:** `.agent/` klasörü tek "source of truth" dokümantasyon konumudur.
- **MUST:** Her move/rename işleminden sonra `.agent/*.md` içinde eski path araması yap ve güncelle.
- **MUST:** Her migration sonunda "Migration Map" üret: `old path -> new path`
- **FORBIDDEN:** "see above / previous doc" gibi path'siz referans.

---

## 7) Versiyon

- **Tarih:** 2026-02-06
- **Sürüm:** v1.2
- **.NET:** 10.0 (target)
