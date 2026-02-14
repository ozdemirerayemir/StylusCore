# Sidebar Component — Golden Reference (v1.0)

> **FROZEN SPECIFICATION** — This document describes the verified, working sidebar.
> Any AI model modifying the sidebar **MUST** follow these rules exactly.
> Last verified: 2026-02-12

---

## 1) File Ownership

| Concern | File | Notes |
|---------|------|-------|
| XAML layout | `Shared/Components/Sidebar.xaml` | **No local styles or resources** |
| Code-behind | `Shared/Components/Sidebar.xaml.cs` | Toggle logic + navigation events |
| Button style | `Resources/GeneralResources.xaml` | `SidebarButtonStyle` — **single source of truth** |
| Metric tokens | `Resources/GeneralResources.xaml` | `Shell.Sidebar*` and `Shell.Icon*` tokens |
| Hover brush | `Shared/Themes/LightTheme.xaml` / `DarkTheme.xaml` | `SidebarHoverBrush` |
| Icon stroke | `Shared/Themes/LightTheme.xaml` / `DarkTheme.xaml` | `IconStrokeBrush` |

> [!CAUTION]
> **FORBIDDEN:** Defining `SidebarButtonStyle` in any file other than `GeneralResources.xaml`.
> `StandardControls.xaml` loads AFTER `GeneralResources.xaml` in App.xaml and would silently override it.

---

## 2) Metric Tokens (GeneralResources.xaml)

```xml
<sys:Double x:Key="Shell.SidebarCollapsedWidth">48</sys:Double>
<sys:Double x:Key="Shell.SidebarExpandedWidth">160</sys:Double>
<sys:Double x:Key="Shell.SidebarItemHeight">48</sys:Double>
<sys:Double x:Key="Shell.IconSlotWidth">48</sys:Double>
<sys:Double x:Key="Shell.IconSize">20</sys:Double>
<CornerRadius x:Key="Shell.SidebarCornerRadius">6</CornerRadius>
```

---

## 3) SidebarButtonStyle (GeneralResources.xaml)

**Critical properties that MUST NOT be changed:**

| Property | Value | Why |
|----------|-------|-----|
| `ContentPresenter.HorizontalAlignment` | **`Left`** | Icons stay left-aligned when sidebar expands |
| `Background` (default) | `Transparent` | No background in idle state |
| `BorderThickness` | `0` | No borders |
| `Height` | `{DynamicResource Shell.SidebarItemHeight}` | Dynamic, not hardcoded |
| No `Width` setter | — | Button stretches to fill sidebar width (critical for expand/collapse) |
| No `HorizontalAlignment` setter | — | Defaults to `Stretch` (do NOT set to `Center`) |
| Hover trigger | `{DynamicResource SidebarHoverBrush}` | Theme-dependent |
| `CornerRadius` | `{DynamicResource Shell.SidebarCornerRadius}` | Dynamic token |

---

## 4) Sidebar Item Pattern (mandatory for all items)

Every sidebar button MUST follow this exact XAML pattern:

```xml
<Button Style="{DynamicResource SidebarButtonStyle}"
        Click="[Handler]"
        ToolTip="[Label]"
        AutomationProperties.Name="[Accessibility Label]">
    <StackPanel Orientation="Horizontal">
        <!-- Icon Slot: fixed-size container (does NOT move when sidebar expands) -->
        <Grid Width="{DynamicResource Shell.IconSlotWidth}"
              Height="{DynamicResource Shell.IconSlotWidth}">
            <!-- Icon: centered inside slot -->
            <Grid Width="{DynamicResource Shell.IconSize}"
                  Height="{DynamicResource Shell.IconSize}"
                  HorizontalAlignment="Center"
                  VerticalAlignment="Center">
                <Path Data="{StaticResource Icon.[Name]}" 
                      Style="{DynamicResource IconBase}"
                      Stretch="Uniform"
                      HorizontalAlignment="Center" VerticalAlignment="Center"/>
            </Grid>
        </Grid>
        <!-- Text Label: appears only when expanded -->
        <TextBlock x:Name="[Name]Text"
                   Text="{DynamicResource Str_[Key]}"
                   VerticalAlignment="Center"
                   FontSize="{DynamicResource FontSize_Body}"
                   Foreground="{DynamicResource PrimaryTextBrush}"
                   Visibility="Collapsed"
                   Margin="0,0,16,0"/>
    </StackPanel>
</Button>
```

### Rules enforced by this pattern:

| Rule | Rationale |
|------|-----------|
| `StackPanel Orientation="Horizontal"` | Icon on left, text on right — horizontal flow |
| Icon slot is **outer Grid** with `Shell.IconSlotWidth` | Fixed anchor point — icon position NEVER moves |
| Icon is **inner Grid** with `Shell.IconSize` | Path sizing on parent, NOT on Path element (§7.2) |
| Path has NO `Width`/`Height` | **FORBIDDEN** — uses `Stretch="Uniform"` inside sized Grid |
| TextBlock `Visibility="Collapsed"` default | Code-behind toggles to `Visible` on expand |
| `TextBlock.Margin="0,0,16,0"` | Right padding for text, prevents text touching sidebar edge |
| All resource refs are `DynamicResource` | Except `Icon.*` geometry (`StaticResource`) and converters |
| `AutomationProperties.Name` required | Accessibility compliance |

---

## 5) Grid Layout (Sidebar.xaml)

```
┌─────────────────────────┐
│ Row 0: Toggle (48px)    │  ← RowDefinition Height="48"
├─────────────────────────┤
│ Row 1: Nav Items (*)    │  ← RowDefinition Height="*"
│   • Library             │
│   • Books (conditional) │
├─────────────────────────┤
│ Row 2: Settings (48px)  │  ← RowDefinition Height="48"
└─────────────────────────┘
```

> [!WARNING]
> `RowDefinition.Height` MUST be literal `48`, NOT `{DynamicResource ...}`.
> WPF `GridLength` type cannot accept `DynamicResource` binding to `sys:Double`.

---

## 6) Code-Behind (Sidebar.xaml.cs)

```csharp
// Width values ALWAYS from resource tokens — never hardcode
private double CollapsedWidth => (double)FindResource("Shell.SidebarCollapsedWidth");
private double ExpandedWidth => (double)FindResource("Shell.SidebarExpandedWidth");

private void ApplyState()
{
    this.Width = _isExpanded ? ExpandedWidth : CollapsedWidth;
    LibraryText.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
    SettingsText.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
}
```

**When adding a new item:**
1. Add `[Name]Text.Visibility` toggle in `ApplyState()`
2. Add `Click` or `Command` handler
3. Add `NavigationRequested?.Invoke(this, "[target]")` if needed

---

## 7) Adding a New Sidebar Item — Checklist

1. [ ] Define icon geometry in `Icons.xaml` (e.g., `Icon.NewFeature`)
2. [ ] Add localization string in language files (e.g., `Str_NewFeature`)
3. [ ] Copy the exact pattern from §4 into the appropriate Grid row in `Sidebar.xaml`
4. [ ] Add `x:Name="[Name]Text"` to the TextBlock
5. [ ] Add visibility toggle in `Sidebar.xaml.cs` `ApplyState()`
6. [ ] Add click handler or command binding
7. [ ] **DO NOT** create any local styles
8. [ ] **DO NOT** change `SidebarButtonStyle`
9. [ ] **DO NOT** add `Width` or `Height` to the `Path` element
10. [ ] **DO NOT** change `ContentPresenter.HorizontalAlignment` to `Center`
