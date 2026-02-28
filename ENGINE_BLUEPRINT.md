# StylusCore — Engine / Backend Blueprint (V1.0)
> Bu doküman, StylusCore’un konsept dokümanındaki davranışların (V1.3 FINAL) arkada **nasıl** uygulanacağını tanımlar.
> Amaç: Tek bir “engine planı” ile input → tool → command → object → render → persistence zincirini netleştirmek.

---

## 0) Hedefler ve Kapsam

### 0.1 Bu doküman neyi kapsar?
- Uygulamanın çekirdek veri modeli (object graph)
- Tool sistemi (input yorumlama)
- Command sistemi (undo/redo + atomic actions)
- Render pipeline (preview + commit)
- Hit-testing / selection / z-order
- Persistence (dosya formatı, autosave, snapshot)
- Threading modeli (UI thread kuralları)
- Radial menu’nin backend bağlanma şekli (command trigger)

### 0.2 Bu doküman neyi kapsamaz?
- XAML tema/stil detayları
- UI layout (panel yerleşimi vb.)
- Tek tek ekran tasarımları
- Tam export pipeline (V2+)

---

## 1) Katmanlar (Layered Architecture)

### 1.1 Katmanlar
- **UI Layer (WPF View / XAML)**  
  Sadece görselleştirme ve input event köprüsü (thin).
- **Interaction Layer (Tool System)**  
  Input’u state’e göre yorumlar, preview üretir, command üretir.
- **Command Layer (Undo/Redo)**  
  Tüm değişiklikler command ile modele uygulanır.
- **Model Layer (Document / Object Graph)**  
  Editor’in gerçek verisi. Single source of truth.
- **Rendering Layer (Renderer)**  
  Model’i ve preview’yu ekrana çizer.
- **Persistence Layer (Storage)**  
  Save/load, autosave, snapshot, recovery.

Kural: UI doğrudan model mutasyon yapmaz → her şey command’dan geçer.

---

## 2) Global Veri Modeli (Library → Notebook → Editor)

### 2.1 Domain model
- **Library**
  - Id, Name
  - Notebooks[]
- **Notebook**
  - Id, Name, Metadata
  - Document (editor content root)
- **Document (Editor Root)**
  - Sections[]
  - ActiveSectionId, ActivePageId

### 2.2 Editor internal model
- **Section**
  - Id, Name, Color/Metadata
  - Pages[]
- **Page**
  - Id, Title
  - PageSettings (size/orientation/infinite)
  - ObjectStore (all objects on page)
  - PageViewState (viewport/camera state) *(opsiyonel)*

Not: PageViewState persistence’e dahil edilebilir ama V1’de optional.

---

## 3) Object Graph Tasarımı (Her şey object)

### 3.1 Base Object
Her object ortak alanları taşır:
- ObjectId (Guid)
- Type (enum)
- ZIndex / Order
- Transform (Position, Scale [V2], Rotation [V2])
- Bounds (cached) → hit-test için
- CreatedAt / UpdatedAt (opsiyonel)
- StyleRef / PresetRef (opsiyonel)

### 3.2 Object türleri (V1)
- InkStroke
- FreeTextBlock
- DocumentText (flow root)
- Shape
- Image
- VoiceBlock (audio + transcript ref)

### 3.3 ObjectStore
Page içindeki tüm objeler tek yerde tutulur:
- `Dictionary<ObjectId, Object>`
- `List<ObjectId> ZOrderedIds` (render order için)
- Spatial index (V2) / basit bounds scan (V1)

Kural: “Render sırası” = ZOrderedIds; Object’ler ayrı listelere bölünmez (V1).

---

## 4) Input → Tool → Command Zinciri

### 4.1 Tool state machine
Editor’de aktif tool vardır:
- NavigationTool
- InkTool (preset + engine)
- TextTool (free/document alt modu)
- EraserTool (stroke/point/object)
- ShapeTool
- SelectionTool

Tool’lar:
- UI input eventlerini alır
- preview state üretir
- commit anında command üretir

### 4.2 Preview vs Commit
- Preview: UI thread’de hafif çizim, kalıcı değil
- Commit: command ile model güncellenir

Kural: Commit olmadan model değişmez.

---

## 5) Command Layer ve Undo/Redo

### 5.1 Command interface
Her command:
- Execute(model)
- Undo(model)
- CanMerge(prevCommand) / MergeWith(prev) (opsiyonel)

### 5.2 Core commands (V1)
- AddStrokeCommand
- AddTextBlockCommand
- UpdateTextCommand
- AddShapeCommand
- TransformObjectCommand (move/resize)
- DeleteObjectCommand
- ChangeZOrderCommand
- PasteImageCommand
- SetActivePageCommand (opsiyonel undo dışı olabilir)
- VoiceFinalizeCommand (transcript insert)

### 5.3 Command grouping
Sürekli aksiyonlar:
- ink stroke = tek command
- drag move = begin/commit ile tek command

Kural: UI her pointer move’da command push etmez.

---

## 6) Selection / Hit Testing

### 6.1 Hit test pipeline
- Input point → candidates by bounds
- ZIndex’e göre en üst object seçilir
- Shift ile multi-select set güncellenir

### 6.2 Selection state
SelectionTool model’e yazmaz; EditorSessionState içinde tutulur:
- SelectedObjectIds[]
- ActiveObjectId

Command üretimi:
- Delete selection → DeleteObjectCommand(list)
- Move selection → TransformObjectCommand(list)

---

## 7) Rendering Pipeline

### 7.1 Renderer sorumluluğu
- Page model → draw
- Active tool preview → draw overlay

### 7.2 Render scheduling
- UI thread sadece invalidate/compose
- Ağır işler background’da hazırlanır (örn. stroke geometry preprocess)

### 7.3 Single source of truth
- Model = gerçek veri
- Preview = geçici overlay
- Commit sonrası preview temizlenir

---

## 8) Ink Engine (Backend)

### 8.1 Ink input sampling
- pointer events → points (x,y,time,pressure)
- smoothing/stabilization tool seviyesinde uygulanır
- commit’te InkStroke oluşturulur

### 8.2 Preset sistemi
Preset:
- ToolEngineId
- Color, thickness, opacity
- smoothing, pressure curve
- metadata

Kural: Preset switch undo stack’e girmez.

---

## 9) Text Engine (Backend)

### 9.1 FreeTextBlock
- Rect (creation box)
- Text runs + formatting
- caret/selection runtime state (UI/session)

### 9.2 DocumentText
- Flow root (paragraphs/runs)
- Layout engine (V1 basit)
- Cursor insertion commands ile mutasyon

Kural: Text değişimleri “UpdateTextCommand” ile yapılır.

---

## 10) Shape Engine (Backend)

Shape object:
- geometry params (rect, ellipse, line, arrow)
- stroke/fill style
- handles (UI) → transform command üretir

V1: rotate yok (opsiyon).

---

## 11) Voice / Whisper Pipeline

### 11.1 Asenkron çalışma
- Record → file/stream
- STT background thread/task
- result UI thread’e dispatch
- commit: VoiceFinalizeCommand (text insert + voice block)

Kural: UI thread bloklanmaz.

---

## 12) Clipboard / Paste Backend

Paste resolver:
- clipboard içerik tipi detect (image/text/internal object)
- target location = cursor veya viewport center
- commit: PasteImageCommand / AddTextBlockCommand

---

## 13) Persistence / Autosave / Recovery

### 13.1 Dosya formatı (V1 öneri)
- tek bir “.styluscore” paket:
  - document.json (metadata + object graph)
  - blobs/ (images, audio)
  - optional: incremental snapshots

### 13.2 Autosave
- timer (30–60s)
- dirty flag + debounce
- background serialize
- atomic write (temp → replace)

### 13.3 Crash recovery
- last snapshot
- journal (opsiyonel V2)

---

## 14) Threading Kuralları

UI thread:
- input dispatch
- render scheduling
- lightweight preview

Background:
- serialization
- STT/AI calls
- heavy geometry preprocess
- export pipeline

Kural: UI thread’de IO yok.

---

## 15) Radial Menu Backend Bağlantısı

Radial menu:
- UI elemanıdır
- backend’de sadece “Command/Tool/Preset trigger” eder

Radial item resolution:
- ToolItem → SetActiveTool(action)
- PresetItem → SetActivePreset(action)
- CommandItem → execute command (undo/redo/delete etc.)
- SubmenuItem → UI navigation (backend yok)

Kural: radial menu business logic içermez.

---

## 16) “Ne eklerken nereye koyacağız?” Kural Seti

Yeni feature eklerken:
1) Yeni object türü mü?
   - Evet → Model Layer + renderer + commands + hit-test
2) Yeni tool mu?
   - Evet → Interaction Layer + preview + command production
3) Yeni command mı?
   - Evet → Command Layer + undo/redo rules
4) Yeni IO mı?
   - Evet → Persistence/async threading rules

---

## 17) V1 Uygulama Sırası (Pragmatik Plan)

1) ObjectStore + basic renderer
2) Selection + move + delete commands
3) InkTool (single engine) + AddStrokeCommand
4) FreeTextBlock + AddTextBlockCommand
5) Shapes + AddShapeCommand
6) Undo/Redo
7) Autosave snapshot
8) Voice pipeline (basic)

---

## Son
Bu doküman StylusCore için backend/engine uygulama planıdır.
Konsept dokümanı (V1.3 FINAL) ile çelişemez; onu uygular.