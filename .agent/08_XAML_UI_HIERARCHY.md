---
description: StylusCore Technical Constitution — 08_UI_SHELL_XAML (v1.3 - Contextual Ribbon Update + App Root Hygiene Sync)
---

# 🧩 08 — UI SHELL & XAML HIERARCHY (WPF / .NET 10)

This document defines STRICT rules for StylusCore’s **Shell UI** (MainWindow, Header, Sidebar, Library, Settings, Dialogs) and its **XAML design system**.  
It does **not** redefine the Canvas Engine rendering (see `03_CANVAS_ENGINE.md`, `05_RENDERING_PERF_WPF.md`).

---

## 0) Normative Language
- **MUST**: required
- **MUST NOT / FORBIDDEN**: prohibited
- **SHOULD**: recommended
- **MAY / OPTIONAL**: allowed but not required

---

## 1) Scope & Non-Goals

### 1.1 Scope (Shell Only)
- **MUST:** Apply these rules to:
  - `MainWindow` shell layout
  - Header/Top bar (search, tool entrypoints)
  - Sidebar navigation
  - Library pages and lists
  - Settings pages
  - Dialogs/Popups/Overlays (non-canvas)
  - Shared components and templates

### 1.2 Non-Goals
- **MUST NOT:** Use XAML-heavy frameworks to render the editor canvas.
- **FORBIDDEN:** Any approach that turns the editor into an `ItemsControl` canvas.

---

## 2) Global Shell Layout (MainWindow Grid)

### 2.1 Required Layout (UPDATED v1.2)
- **MUST:** MainWindow root is `Grid` with:
  - **Row 0:** App Shell / Custom Window Chrome (Logo, Window Controls). Fixed height (`Shell.HeaderHeight`).
  - **Row 1:** Workspace (`*`) -> Contains Sidebar and Content.
- **MUST:** Workspace (Row 1) uses columns:
  - **Column 0:** Global Sidebar (`Shell.SidebarWidth`).
  - **Column 1:** Main Content (`*`) -> Hosts `LibraryView` or `EditorView`.

### 2.2 Contextual Ribbon Rule (CRITICAL CHANGE)
- **MUST:** The Functional Ribbon (Pens, Colors, Tools) is **NOT** a global control in MainWindow.
- **MUST:** The Ribbon lives inside **`EditorView.xaml`** (Row 0 of the Editor Grid).
- **FORBIDDEN:** Placing the Ribbon in MainWindow Row 0 or Row 1 (spanning above the sidebar). The Ribbon belongs to the Editor context.

> **V1 TRANSITION NOTE:** For the V1 full-width layout, the Ribbon Container
> MAY reside in `MainWindow.xaml` (Row 1) as specified in `09_UI_CONTEXT_MAP.md` §3.2.
> This is a sanctioned exception; logical ownership remains with the Editor context,
> and Visibility MUST be strictly bound to Editor Mode.
> See `01_CONSTITUTION.md` §9 for the corresponding exception clause.

### 2.3 Default Metrics (UI Density = Compact)
- **MUST:** Default preset values:
  - `Shell.HeaderHeight = 48`
  - `Shell.SidebarCollapsedWidth = 48`
  - `Shell.SidebarExpandedWidth = 160`

### 2.4 No-Jitter Sidebar Implementation

#### Basic Principles
- **MUST:** Sidebar UserControl `Width` property must be set directly.
- **MUST:** MainWindow Sidebar column must have a fixed `Width` value (no Auto).
- **MUST:** Updates happen via Toggle event in Code-behind or ViewModel state.

#### Sidebar.xaml Structure
```xml
UserControl (Width="48", SnapsToDevicePixels="True", UseLayoutRounding="True")
└── Border (BorderThickness="0,0,1,0" - right divider)
    └── Grid (3 Rows)
        ├── Row 0: Toggle Button (Icon: Chevron or Menu)
        ├── Row 1: Nav Items (Icon + Text)
        └── Row 2: Settings (Icon + Text)
```

---

## 3) UI Density (Compact / Comfortable / Large)

### 3.1 Density System Mandate
- **MUST:** Support user-selectable UI Density presets without using global ScaleTransform.
- **MUST:** Implement density by swapping a Metrics ResourceDictionary.
- **MUST:** Density changes affect shell UI only, not the editor canvas engine.

### 3.2 Metric Tokens (Required)
- **MUST:** Define and use metric tokens:
  - `Shell.HeaderHeight`
  - `Shell.SidebarCollapsedWidth`
  - `Shell.SidebarExpandedWidth`
  - `Shell.IconSlotWidth`
  - `Shell.IconSize`
  - `Shell.PaddingS/M/L`
  - `Shell.CornerRadiusS/M/L`
  - `Typography.*`
- **MUST:** All shell components bind sizes to these tokens.

### 3.3 Preset Guidance
- **MUST:** Provide at least Compact (default) and Large presets.

---

## 4) ResourceDictionaries, Tokens, and Merge Order

### 4.1 Dictionary Organization
- **MUST:** Organize dictionaries by responsibility:
  - `00_Base.xaml`
  - `01_Metrics.xaml`
  - `02_Colors.xaml`
  - `03_Brushes.xaml`
  - `04_Typography.xaml`
  - `05_Icons.xaml`
  - `06_Components.xaml`
  - `07_Views.xaml`

> **NOTE:** These dictionaries are typically stored under `Shared/Themes/` (or an equivalent themes root).  
> Theme and Density swapping must only swap their respective dictionaries (see 4.2).

### 4.2 Merge Order (STRICT)
- **MUST:** Merge in this order:
  1. Base
  2. Metrics
  3. Colors
  4. Brushes
  5. Typography
  6. Icons
  7. Components
  8. Views (optional)

- **MUST:** Theme changes swap Colors/Brushes dictionaries.
- **MUST:** Density changes swap Metrics dictionary.
- **FORBIDDEN:** Mixing Metrics into theme dictionaries (keep concerns separate).

### 4.3 DynamicResource Boundary
- **MUST:** Shell UI (buttons, text, panels) uses DynamicResource for theme/localization.
- **MUST NOT:** Use DynamicResource lookups inside engine hot render loops (covered elsewhere).

### 4.4 Static vs Dynamic Decision Matrix
- **Use `StaticResource` ONLY for:**
  - Geometry / path data (`Icon.*`) — immutable shape data, never changes
  - `BooleanToVisibilityConverter` and other value converters — logic objects, not visual
  - Resources referenced within the **same ResourceDictionary** (e.g., a Color inside a Brush definition in the same theme file)
- **Use `DynamicResource` for EVERYTHING else:**
  - All Brushes (colors, backgrounds, foregrounds, borders)
  - All Font properties (FontSize, FontFamily, FontWeight)
  - All Sizing tokens (Width, Height, Margin, Padding, CornerRadius)
  - All Styles (including `IconBase`, `SidebarButtonStyle`, button styles)
  - All Localization strings (`Str_*`)
- **FORBIDDEN:** Using `StaticResource` for any Brush, Font, or Size value in Shell UI XAML.
- **FORBIDDEN (HARDCODED SIZES):** Writing raw numbers for `Margin`, `Padding`, `Width`, `Height`, `CornerRadius`, or `FontSize` (e.g. `Margin="10"`, `Width="200"`).
- **MUST:** All measurements MUST be bound using `{DynamicResource [TokenName]}` (e.g., `{DynamicResource Spacing16}`, `{DynamicResource ControlHeightMD}`).
- **Rationale:** `DynamicResource` enables runtime theme switching, UI density changes, and future preset customization. `StaticResource` freezes the lookup and prevents updates. Hitting raw numbers prevents runtime density swaps entirely.

---

## 5) Theming

### 5.1 Baseline Themes
- **MUST:** Support Light and Dark themes.
- **SHOULD:** Allow adding additional themes by adding dictionaries (no control rewrites).

### 5.2 Required Brush Keys
- **MUST:** Provide and use standard keys:
  - `PrimaryBackgroundBrush`
  - `PrimaryTextBrush`
  - `PrimaryAccentBrush`
  - `PrimaryBorderBrush`
  - `SidebarHoverBrush` — sidebar item hover background (theme-dependent)
  - `SidebarBackgroundBrush` — sidebar panel background
  - `IconStrokeBrush` — default icon stroke color (defaults to `PrimaryTextColor`, can be overridden for accent icon customization)

### 5.3 No Hardcoding
- **FORBIDDEN:** Hardcoded colors, brushes, or FontFamily in XAML.
- **MUST:** Font family comes from DynamicResource `App.MainFont`.

### 5.4 Icon Accent Color Architecture
- **MUST:** Icon stroke color comes from `DynamicResource IconStrokeBrush`, NOT directly from `PrimaryTextBrush`.
- **MUST:** `IconStrokeBrush` defaults to `PrimaryTextColor` in both Light and Dark themes.
- **SHOULD:** When user-selectable icon accent colors are implemented, swap `IconStrokeBrush` value at runtime (e.g., purple, orange, teal).
- **MUST NOT:** Change text colors (`PrimaryTextBrush`) to match icon accent — text stays black/white per theme.
- **Rationale:** Separating icon stroke from text color enables per-user icon color customization without affecting readability.

### 5.5 Shared Style Location
- **MUST:** Reusable component styles (e.g., `SidebarButtonStyle`) go in `GeneralResources.xaml`, referencing dynamic brushes.
- **FORBIDDEN:** Defining component styles locally inside UserControl.Resources — this prevents theming and causes duplicated code.
- **MUST:** Styles reference brush keys via `DynamicResource` so theme switches apply automatically.

---

## 6) Localization (Runtime Switching)

### 6.1 No Hardcoded Strings
- **FORBIDDEN:** Hardcoded user-facing strings in XAML/code.
- **MUST:** Use DynamicResource string keys for runtime language switching.

### 6.2 Formatting Strategy
- **SHOULD:** For formatted strings, use:
  - MultiBinding + StringFormat, or
  - a dedicated converter that formats using localized templates.
- **MUST:** Keep localization keys stable and versioned.

---

## 7) Icons (StreamGeometry) — Clip Protection

### 7.1 Storage & Format
- **MUST:** Icons are StreamGeometry (path data) stored in `Shared/Themes/Icons.xaml`.

### 7.2 Fit-to-Box Rule
- **FORBIDDEN:** Setting explicit Width/Height on the Path itself.
- **MUST:** Parent container defines size; Path uses:
  - `Stretch="Uniform"`
  - centered alignment

### 7.3 Reference Pattern
- **MUST:** Use this pattern (size on wrapper, not Path):
```xml
<Grid Width="{DynamicResource Shell.IconSize}" Height="{DynamicResource Shell.IconSize}">
  <Path Data="{StaticResource Icon.Example}"
        Fill="{DynamicResource PrimaryTextBrush}"
        Stretch="Uniform"
        HorizontalAlignment="Center"
        VerticalAlignment="Center"/>
</Grid>
```

---

## 8) Component Architecture Strategy (Templates vs Controls)

### 8.1 ControlTemplate vs UserControl
- **MUST:** Use ControlTemplate when:
  - you need restyling via theme/tokens
  - you need consistent states (hover/pressed/disabled)
  - you want minimal visual tree (template reuse)
- **MUST:** Use UserControl only when:
  - composition is complex and reused as a composite view
  - the visual tree remains shallow and predictable
- **FORBIDDEN:** Using UserControl as a “styling shortcut” for simple buttons/rows.

### 8.2 Visual States
- **SHOULD:** Prefer simple triggers for basic states.
- **SHOULD:** Use VisualStateManager for controls requiring multiple interactive states or stylus/touch nuance.
- **FORBIDDEN:** Trigger storms (many nested triggers) inside item templates for large lists.

### 8.3 Naming Conventions
- **MUST:** Use stable style keys:
  - `Shell.*` for shell styles
  - `Sidebar.*` for sidebar templates/styles
  - `Button.*`, `TextBox.*`, `ListItem.*`, `Dialog.*`
- **MUST:** Keep keys consistent across dictionaries.

---

## 9) Sidebar Specification (Shell Navigation)

### 9.1 Width & Toggle
- **MUST:** Sidebar supports collapsed/expanded modes via `Shell.SidebarWidth`.
- **MUST:** Collapsed shows icons only; expanded shows icons + text labels.

### 9.2 Item Template (No-Jitter)
- **MUST:** Sidebar items follow the fixed icon slot rule (`Shell.IconSlotWidth`).
- **MUST:** Item height equals `Shell.HeaderHeight` (48 in Compact).
- **SHOULD:** Hover/Pressed states are full-width rectangles (no per-child hover artifacts).

### 9.3 Context Switching
- **MUST:** Sidebar content adapts by context:
  - Library context: notebook lists / favorites / library actions
  - Editor context: back-to-library + page/section list + editor tools entrypoints
- **MUST:** When navigating back, clear old selection to avoid “ghost data”.

---

## 10) Lists, Virtualization, and Data Templates

### 10.1 Virtualization (STRICT)
- **MUST:** Enable virtualization for list-heavy UI:
  - `VirtualizingPanel.IsVirtualizing="True"`
  - `VirtualizingPanel.VirtualizationMode="Recycling"`
- **FORBIDDEN:** Placing a ListBox/ListView inside a ScrollViewer (kills virtualization).
- **SHOULD:** Keep item templates shallow (few borders/grids).

### 10.2 Search/Filter Inputs
- **MUST:** Use:
  - `UpdateSourceTrigger=PropertyChanged`
  - `Delay=300` (default) to prevent UI lag
- **SHOULD:** Debounce expensive filtering on background thread and update UI safely.

### 10.3 DataGrid Guidance
- **FORBIDDEN:** Width="Auto" on large datasets.
- **SHOULD:** Prefer star widths or fixed widths.

---

## 11) MVVM Performance Rules (Shell)

### 11.1 Binding Storm Avoidance
- **MUST:** Avoid converters and multi-binding in large repeating templates unless justified.
- **SHOULD:** Precompute display strings/icons in view models for list rows.
- **SHOULD:** Prefer INotifyPropertyChanged updates that are batched/debounced.

### 11.2 Visual Tree Discipline
- **MUST:** Keep visual tree flat.
- **FORBIDDEN:** “StackPanel hell” as primary layout.
- **MUST:** Use Grid for major layouts; Border for wrapper styling only.

---

## 12) Input UX (Shell) & Hit Targets

### 12.1 Hit Target Size
- **MUST:** Interactive hit targets meet a minimum of 40–48 DIP in both dimensions for touch/stylus friendliness.

### 12.2 Hover/Press Semantics
- **SHOULD:** Provide consistent hover/pressed visuals.
- **MUST:** Ensure stylus/touch works without hover reliance.
- **MAY:** Add stylus proximity/contact visual states if hardware supports it.

---

## 13) Accessibility (STRICT)

### 13.1 Automation Properties
- **MUST:** All interactive controls MUST set AutomationProperties.Name.

### 13.2 Keyboard Navigation
- **MUST:** Full shell is keyboard navigable (Tab/Arrow).
- **MUST:** Visible focus indicators.
- **SHOULD:** Define predictable tab order and arrow navigation in lists.

### 13.3 High Contrast
- **MUST:** Ensure text/icons remain visible in Windows High Contrast mode.
- **SHOULD:** Avoid relying solely on subtle opacity changes for state indication.

---

## 14) DPI & Crispness

### 14.1 DPI Robustness
- **MUST:** Shell UI must remain usable and crisp under Windows DPI scaling and per-monitor DPI changes.
- **SHOULD:** Enable:
  - `UseLayoutRounding="True"`
  - `SnapsToDevicePixels="True"` where appropriate on shell roots or key containers.
- **MUST:** Icons remain crisp via vector rendering and proper sizing wrappers.

---

## 15) Dialogs & Overlays (Shell)

### 15.1 Standard Dialog Template
- **MUST:** Use a consistent dialog layout:
  - Title
  - Content
  - Button row (primary/secondary)
- **MUST:** Provide ESC to cancel, Enter to confirm (where safe).

### 15.2 Focus Management
- **MUST:** Implement focus trap for modal dialogs.
- **MUST:** Initial focus goes to first actionable control.
- **MUST:** Return focus to invoker on close.

### 15.3 Overlay Layering
- **MUST:** Overlays are above shell content but separate from the editor canvas visuals.
- **SHOULD:** Prefer a centralized overlay host region in MainWindow.

---

## 16) Navigation & State Preservation

### 16.1 Navigation Implementation
- **SHOULD:** Prefer ContentControl + DataTemplates (ViewModel-driven navigation).
- **FORBIDDEN (by default):** Using Frame navigation unless explicitly justified.
- **MUST:** NotebookView is renamed to EditorView to reflect its role as the editing workspace.

### 16.2 Ghost Data Rule
- **MUST:** On navigation away from a context, clear stale selections and transient state.
- **SHOULD:** Preserve only what is explicitly desired (camera, last page).

---

## 17) Anti-Patterns Checklist (Shell)

- **FORBIDDEN:** Hardcoded strings, colors, fonts.
- **FORBIDDEN:** ListBox/ListView inside ScrollViewer.
- **FORBIDDEN:** Deep nested borders/grids without need.
- **FORBIDDEN:** Excessive triggers in list item templates.
- **FORBIDDEN:** Heavy bitmap effects (blur/drop shadow everywhere) on large repeating UI.
- **FORBIDDEN:** Frame navigation unless explicitly justified.

### 17.1 App Root Folder Anti-Pattern (REVISED – SYNC WITH 02_ARCHITECTURE)
- **FORBIDDEN:** creating ad-hoc, undocumented new root folders under `src/StylusCore.App/`.
- **MUST:** only use the approved root folder categories defined in `02_ARCHITECTURE.md` (App Root whitelist).

- **FORBIDDEN:** Placing Ribbon in MainWindow (Ribbon MUST be inside EditorView).

---

## 18) Reference Templates (Copy-Pasteable)

### 18.1 MainWindow Skeleton (App Shell)
```xml
<Grid UseLayoutRounding="True" SnapsToDevicePixels="True">
  <Grid.RowDefinitions>
    <RowDefinition Height="{DynamicResource Shell.HeaderHeight}"/>
    <RowDefinition Height="*"/>
  </Grid.RowDefinitions>

  <ContentControl Grid.Row="0" Content="{Binding HeaderVM}"/>

  <Grid Grid.Row="1">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="{DynamicResource Shell.SidebarWidth}"/>
      <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <ContentControl Grid.Column="0" Content="{Binding SidebarVM}"/>

    <ContentControl Grid.Column="1" Content="{Binding ActivePageVM}"/>
  </Grid>
</Grid>
```

### 18.2 EditorView Skeleton (Contextual Ribbon)
```xml
<Grid>
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
  </Grid.RowDefinitions>

  <controls:RibbonToolbar Grid.Row="0" DataContext="{Binding}"/>

  <Grid Grid.Row="1">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="Auto"/>
      <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <Border Grid.Column="0" x:Name="SectionsPanel"/>
    <controls:CanvasHostControl Grid.Column="1"/>
  </Grid>
</Grid>
```

### 18.3 Standard Dialog Layout
```xml
<Grid Margin="{DynamicResource Shell.PaddingL}">
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
    <RowDefinition Height="Auto"/>
  </Grid.RowDefinitions>

  <TextBlock Grid.Row="0"
             Text="{DynamicResource String.DialogTitle}"
             Style="{StaticResource Typography.Title}"/>

  <ContentPresenter Grid.Row="1" Margin="{DynamicResource Shell.PaddingM}"/>

  <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right">
    <Button Content="{DynamicResource String.Cancel}" Style="{StaticResource Button.Secondary}"/>
    <Button Content="{DynamicResource String.OK}" Style="{StaticResource Button.Primary}" Margin="12,0,0,0"/>
  </StackPanel>
</Grid>
```

---

## 19) Required Deliverables (Project Hygiene)

- **MUST:** Place reusable shell components in `Shared/Components/`.
- **MUST:** Place theme dictionaries in `Shared/Themes/`.
- **MUST:** Keep folder hierarchy clean:
  - **FORBIDDEN:** creating ad-hoc, undocumented new root folders under `src/StylusCore.App/`
  - **MUST:** follow the App Root whitelist in `02_ARCHITECTURE.md`
- **MUST:** Every new shell component must ship with:
  - a style key
  - token usage (no hardcoded sizes/colors/fonts)
  - accessibility name
  - keyboard navigation behavior
