---
description: StylusCore Agent Entry Point (Routing + Precedence + Output Contract)
---

# 🤖 StylusCore — AGENT.md (Önce bunu oku)

Bu dosya, AI agent'ların (Antigravity/Cursor vb.) **hangi görev için hangi kural dosyalarını** okuyacağını belirleyen tek giriş kapısıdır.

> **Kullanım:** Her promptta → "`.agent/AGENT.md` oku ve uygula"

---

## 0) Precedence (Çelişki olursa hangisi kazanır?)

1) **`.agent/01_CONSTITUTION.md`** (RED LINES) — her şeyin üstünde
2) Göreve özel dosya (Canvas/Input/Persistence/XAML vb.)
3) Best practices (performans/UX önerileri)

Şüphede kalırsan: **RED LINE'ı bozma.** Uyumlu alternatif öner.

---

## 1) Her görevden önce ZORUNLU

1. ✅ `.agent/01_CONSTITUTION.md` oku
2. ✅ Bu dosyadan (AGENT.md) "Task Routing" bölümünden uygun kategoriyi seç
3. ✅ Kategoride listelenen dosyaları oku
4. ✅ Çıktını "Output Contract" formatında ver (aşağıda)

---

## 2) Task Routing — "Ne yapıyorsun?"

Aşağıdan en uygun kategoriyi seç. **Listelenen dosyalar okunmadan kod yazma.**

### A) Editor/Canvas Engine (kamera, zoom/pan, scene graph, quadtree, selection)

Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`
- `.agent/07_UNDO_REDO_COMMANDS.md` (canvas operasyonu ekliyorsan)

### B) Ink/Input/Threading (StylusPlugIn, buffer, wet/dry, palm rejection)

Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/04_INPUT_INK_THREADING.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`

### C) Mixed Media Objects (Text/Table/Code/Diagram + View-to-Edit)

Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`
- `.agent/07_UNDO_REDO_COMMANDS.md`

### D) Persistence/DB/File Format (SQLite, LZ4, autosave, recovery, chunking)

Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/06_PERSISTENCE_DB.md`
- `.agent/03_CANVAS_ENGINE.md` (bounds/chunk kontratı için)

### E) App Shell / Navigation / MVVM / Dialogs

Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/08_XAML_UI_HIERARCHY.md`

### F) XAML/UI (Sidebar, layout, components, theming, icons, localization)

Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/08_XAML_UI_HIERARCHY.md`
- `.agent/05_RENDERING_PERF_WPF.md` (virtualization, nested scroll ban)

### G) Undo/Redo (Command pattern, grouping, atomicity)

Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/07_UNDO_REDO_COMMANDS.md`
- `.agent/03_CANVAS_ENGINE.md` (quadtree/bounds update)
- `.agent/05_RENDERING_PERF_WPF.md` (dirty rect)

### H) Genel Refactoring / Kod Kalitesi / Mimari Değişiklik

Oku:
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- İlgili domain dosyası (hangi katmandaysa)

---

## 3) Dosya Haritası (Quick Reference)

| Dosya | Kapsam |
|-------|--------|
| `01_CONSTITUTION.md` | Master kurallar, RED LINES, tüm projeyi kapsar |
| `02_ARCHITECTURE.md` | Katman yapısı, bağımlılık yönü, klasör kuralları |
| `03_CANVAS_ENGINE.md` | Infinite canvas, camera, QuadTree, View-to-Edit |
| `04_INPUT_INK_THREADING.md` | StylusPlugIn, pen thread, wet/dry pipeline |
| `05_RENDERING_PERF_WPF.md` | DrawingVisual, dirty rect, virtualization |
| `06_PERSISTENCE_DB.md` | SQLite, R-Tree, LZ4, autosave, chunking |
| `07_UNDO_REDO_COMMANDS.md` | Command pattern, 50 step limit, grouping |
| `08_XAML_UI_HIERARCHY.md` | Shell UI, theming, icons, dialogs, a11y |

---

## 4) Output Contract (Agent çıktısı standardı)

Her görev için agent şu formatta cevap üretmeli:

1. **Okunan dokümanlar:** (dosya listesi)
2. **Kuralların etkisi:** Her dosyadan 1 somut kural → bu görevde neye zorladı?
3. **Plan:** 3–8 adım
4. **Değişecek dosyalar:** path listesi
5. **Riskler:** perf/thread/persistence/memory
6. **Test/Doğrulama:** en az 3 madde

---

## 5) Hard Stops (Yapılmayacaklar)

Agent aşağıdakileri uygulamaz; uyumlu alternatif önerir:

| Yasak | Neden | Alternatif |
|-------|-------|------------|
| Canvas'ta `ScrollViewer` | Pan/zoom için yanlış | `MatrixTransform` camera |
| Pen thread'de UI erişimi | Thread-safety | Buffer → UI thread pump |
| `ItemsControl` ile canvas render | Performans | `DrawingVisual` retained |
| Canvas'ta kalıcı `DataGrid`/`RichTextBox` | Memory | View-to-Edit overlay |
| Hardcoded string/color/font | Theming/i18n | `DynamicResource` |
| Icon `Path`'te `Width/Height` | Clipping | Grid wrapper + `Stretch` |
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
- **Sürüm:** v1.1
- **.NET:** 10.0 (LTS)
