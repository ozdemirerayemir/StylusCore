---
description: StylusCore Agent Entry Point (Routing + Precedence + Output Contract)
---

# 🤖 StylusCore — AGENT.md (Önce bunu oku)

Bu dosya, AI agent’ların (Antigravity vb.) **hangi görev için hangi kural dosyalarını** okuyacağını belirleyen tek giriş kapısıdır.

## 0) Precedence (Çelişki olursa hangisi kazanır?)
1) **01_CONSTITUTION.md** (RED LINES) — her şeyin üstünde
2) Göreve özel dosya (Canvas/Input/Persistence/XAML vb.)
3) Best practices (performans/UX önerileri)

Şüphede kalırsan: **RED LINE’ı bozma.** Uyumlu alternatif öner.

---

## 1) Her görevden önce ZORUNLU
- ✅ `01_CONSTITUTION.md` oku
- ✅ Bu dosyadan (AGENT.md) “Task Routing” seç
- ✅ İlgili dosyaları oku
- ✅ Çıktını “Output Contract” formatında ver (aşağıda)

---

## 2) Task Routing — “Ne yapıyorsun?”
Aşağıdan en uygun kategoriyi seç. Listelenen dosyalar okunmadan kod yazma.

### A) Editor/Canvas Engine (kamera, zoom/pan, scene graph, quadtree, selection)
Oku:
- 01_CONSTITUTION.md
- 03_CANVAS_ENGINE.md
- 05_RENDERING_PERF_WPF.md
- 07_UNDO_REDO_COMMANDS.md (canvas operasyonu ekliyorsan)

### B) Ink/Input/Threading (StylusPlugIn, buffer, wet/dry, palm rejection)
Oku:
- 01_CONSTITUTION.md
- 04_INPUT_INK_THREADING.md
- 03_CANVAS_ENGINE.md
- 05_RENDERING_PERF_WPF.md

### C) Mixed Media Objects (Text/Table/Code/Diagram + View-to-Edit)
Oku:
- 01_CONSTITUTION.md
- 03_CANVAS_ENGINE.md
- 05_RENDERING_PERF_WPF.md
- 07_UNDO_REDO_COMMANDS.md

### D) Persistence/DB/File Format (SQLite, LZ4, autosave, recovery, chunking)
Oku:
- 01_CONSTITUTION.md
- 06_PERSISTENCE_DB.md
- 03_CANVAS_ENGINE.md (bounds/chunk kontratı için)

### E) App Shell / Navigation / MVVM / Dialogs
Oku:
- 01_CONSTITUTION.md
- 02_ARCHITECTURE.md
- 08_XAML_UI_HIERARCHY.md

### F) XAML/UI (Sidebar, layout, components, theming, icons, localization)
Oku:
- 01_CONSTITUTION.md
- 08_XAML_UI_HIERARCHY.md
- 05_RENDERING_PERF_WPF.md (virtualization, nested scroll ban gerekiyorsa)

### G) Undo/Redo (Ctrl+Z tetikleyici değil; engine kontratı)
Oku:
- 01_CONSTITUTION.md
- 07_UNDO_REDO_COMMANDS.md
- 03_CANVAS_ENGINE.md (quadtree/bounds update)
- 05_RENDERING_PERF_WPF.md (dirty rect vs.)

---

## 3) Output Contract (Agent çıktısı standardı)
Her görev için agent şu formatta cevap üretmeli:

1) **Okunan dokümanlar:** (dosya listesi)
2) **Kuralların etkisi:** Her dosyadan 1 somut kural → bu görevde neye zorladı?
3) **Plan:** 3–8 adım
4) **Değişecek dosyalar:** path listesi
5) **Riskler:** perf/thread/persistence/memory
6) **Test/Doğrulama:** en az 3 madde

---

## 4) Hard Stops (Yapılmayacaklar)
Agent aşağıdakileri uygulamaz; uyumlu alternatif önerir:

- Canvas yüzeyinde ScrollViewer ile pan/zoom
- Pen thread içinden UI/DependencyObject erişimi
- Stroke’ları ObservableCollection bind edip ItemsControl ile render
- Canvas üzerinde kalıcı DataGrid/RichTextBox (View-to-Edit yerine)
- Hardcoded string veya hardcoded FontFamily
- Icon Path üzerinde Width/Height vererek clipping
- UI thread’i bloklayan save/load

---

## 5) Reference Integrity (RED LINE) — Dosya taşındıysa linkleri kırma
- MUST: `.agent/` klasörü tek “source of truth” dokümantasyon konumudur.
- MUST: Her move/rename işleminden sonra `.agent/*.md` içinde eski path araması yap ve güncelle:
  - `docs/constitution/`, eski proje adları, eski klasörler, eski dosya adları
- MUST: Her migration sonunda “Migration Map” üret:
  - old path -> new path (liste)
- FORBIDDEN: “see above / previous doc” gibi path’siz referans.
WHY: Agent’lar UI bağlamına güvenemez; explicit path şarttır.
FAILURE: “location missing / reference lost” uyarıları ve yanlış doküman okuma.
