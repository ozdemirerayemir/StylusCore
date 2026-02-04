---
description: StylusCore Project Guidelines and Rules (v2.0)
---

# 📏 StylusCore Development Guidelines

This document establishes the strict rules for contributing to the StylusCore project. **All future development must adhere to these standards** to maintain consistency in architecture, design, and code quality.

---

## 🏗️ 1. Architectural & Layout Rules

The application uses a specific layout structure based on **Grids** and **Features**.

### Global Layout (MainWindow)
* **Structure:** Grid-based layout.
* **Header:** Height **48px** (standard) or integrated into the Sidebar logic.
* **Sidebar (Rail):**
    * **Width (Collapsed):** `48px` or `64px` (Standard Rail width).
    * **Width (Expanded):** `240px` - `280px`.
* **Content Area:** Occupies the remaining space (`*`).

### Page Layouts (Views)
* **Root Element:** Must be a `Grid` (not `StackPanel`) to allow proper resizing.
* **Row Definitions:**
    * `Auto`: For Toolbars/Headers inside views.
    * `*`: For the main content (Lists, InkCanvas).
* **Margins:** Use `8px` or `16px` multiples (refer to `StandardControls.xaml` resources).

---

## 🖼️ 2. Iconography Rules

Icons must be **vector-based** and fully **theme-aware**.

* **Format:** SVG Path Data string only. **DO NOT** use PNG, JPG, or external `.svg` files.
* **Location:** All icons must be defined in `src/StylusCore.App/Shared/Themes/Icons.xaml`.
* **Naming Convention:** `Icon.[Name]` (e.g., `Icon.Pen`, `Icon.Settings`).
* **Structure (StreamGeometry Resource):**
    *(Preferred for performance)*
    ```xml
    <StreamGeometry x:Key="Icon.Pen">M10,20 L30,40...</StreamGeometry>
    ```
* **Usage:**
    ```xml
    <Path Data="{StaticResource Icon.Pen}" Fill="{DynamicResource PrimaryTextBrush}" Stretch="Uniform"/>
    ```

---

## 🎨 3. Theming, Typography & Colors

Hardcoded values are **STRICTLY PROHIBITED**.

### Colors
* **Rule:** Always use `DynamicResource` keys defined in `Shared/Themes/`.
    * ✅ Correct: `Background="{DynamicResource PrimaryBackgroundBrush}"`
    * ❌ Incorrect: `Background="#FFFFFF"` or `Background="White"`
* **Required Keys:** `PrimaryBackgroundBrush`, `PrimaryTextBrush`, `PrimaryAccentBrush`, `PrimaryBorderBrush`.

### Typography (NEW CRITICAL RULE)
* **Rule:** Do NOT use hardcoded FontSizes (e.g., `FontSize="14"`).
* **Usage:** Use styles from `TextStyles.xaml`.
    * ✅ Correct: `Style="{StaticResource Text.Body}"` or `Style="{StaticResource Text.Header1}"`

---

## 🌍 4. Localization & Strings (NEW CRITICAL RULE)

* **Rule:** Hardcoded text strings in XAML are **FORBIDDEN**.
    * ❌ Incorrect: `<TextBlock Text="Library Settings"/>`
    * ✅ Correct: `<TextBlock Text="{x:Static p:Resources.LibrarySettings_Title}"/>`
* **Why:** To ensure the application can support multiple languages (TR/EN) instantly without code refactoring.

---

## ⚡ 5. Performance Standards (NEW CRITICAL RULE)

* **Lists:** All `ListBox`, `ListView`, or `ItemsControl` displaying more than 10 items MUST use UI Virtualization.
    * `VirtualizingStackPanel.IsVirtualizing="True"` (Default in ListBox, but check Styles).
* **Images:** If user images are loaded, `DecodePixelWidth` must be set to prevent memory leaks.
* **Async:** Never block the UI thread. Use `async/await` for file I/O and database operations.

---

## 📝 6. Naming & Coding Standards

* **Namespaces:** Must follow the physical folder structure.
    * `StylusCore.App.Features.[FeatureName].Views`
* **Feature Isolation:** A feature (e.g., `Library`) should not directly depend on the internal logic of another feature. Use `EventAggregator` or `Core Services` to communicate.

---

## 📂 7. Hierarchy & File Placement Strategy

When creating a new file, follow this decision tree:

### 1. Is it a standalone Feature? (e.g., Export, CloudSync)
* **Path:** `src/StylusCore.App/Features/[FeatureName]/`
* **Contains:** `Views/` (XAML), `ViewModels/` (Logic)

### 2. Is it a reusable UI Component?
* **Path:** `src/StylusCore.App/Shared/Components/`

### 3. Is it a Pop-up / Dialog?
* **Path:** `src/StylusCore.App/Dialogs/`

### 4. Is it Core Logic?
* **Path:** `src/StylusCore.App/Core/Models/` or `Services/`

**CRITICAL:** Do NOT create new folders at the root of `src/StylusCore.App/`. Keep the root clean.

---

**⚠️ IMPORTANT:** Before generating code, review these rules. If a requested feature violates these rules (e.g., hardcoded color), **REJECT** the request and correct it.