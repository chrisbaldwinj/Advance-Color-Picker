# 🎨 AdvancedColorPickerControl (WPF)

A fully customizable, modern **WPF Color Picker Control** with support for:

- 🎯 Primary & Secondary colors
- 🌈 HSVA color model
- 🔍 Hex input/output
- 🎨 Swatches (default + dynamic/custom)
- 🧭 Smart popup placement
- 🧩 External trigger support (MVVM-friendly)
- 🌙 Dark mode compatibility

Designed for flexibility, extensibility, and smooth UX.

---

## 📦 Features

- **Two-way binding** for selected colors
- **Primary / Secondary color switching**
- **Hex editing** with validation
- **Alpha (transparency) support**
- **Custom swatch palettes**
- **Popup positioning system**
- **External control of opening behavior**
- **INotifyPropertyChanged compliant**

---

## 🚀 Getting Started

### 1. Add the Control

Include the control in your project:

```xml
xmlns:controls="clr-namespace:Noveller.Controls"
```

---

### 2. Basic Usage

```xml
<controls:AdvancedColorPickerControl
    SelectedColor="{Binding MyColor, Mode=TwoWay}" />
```

---

### 3. With Secondary Color

```xml
<controls:AdvancedColorPickerControl
    SelectedColor="{Binding PrimaryColor}"
    SecondaryColor="{Binding SecondaryColor}"
    EnableSecondary="True" />
```

---

### 4. External Trigger (MVVM-friendly popup control)

```xml
<controls:AdvancedColorPickerControl
    SelectedColor="{Binding MyColor}"
    ExternalTrigger="{Binding IsPickerOpen, Mode=TwoWay}" />
```

---

### 5. Custom Swatches

```xml
<controls:AdvancedColorPickerControl
    Swatches="{Binding CustomColors}" />
```

Supports `ObservableCollection<Color>` for dynamic updates.

---

## ⚙️ Dependency Properties

### 🎨 Color Properties

#### `SelectedColor` (Color)
- Primary color
- Two-way bindable
- Default: `White`

#### `SecondaryColor` (Color)
- Secondary color (optional)
- Used when `EnableSecondary = true`

#### `EnableSecondary` (bool)
- Enables dual-color mode

---

### 🧭 Layout & Positioning

#### `Placement` (PickerPlacement)
Controls popup direction:

```csharp
Top, Bottom, Left, Right
```

#### `CenterAlign` (bool)
Centers popup relative to trigger

#### `HorizontalOffset` / `VerticalOffset` (double)
Fine-tune popup position

#### `Offset` (double)
Shortcut to set both offsets

---

### 🔘 Behavior

#### `ExternalTrigger` (bool)
- Opens popup externally (MVVM)
- Automatically resets to `false` when closed

#### `HideButton` (bool, internal)
- Automatically hides built-in trigger if external trigger is used

---

### 🎨 Swatches

#### `Swatches` (IList<Color>)
- Custom color palette
- Falls back to default palette if null/empty
- Supports dynamic updates via `INotifyCollectionChanged`

---

### 🌙 Theme

#### `IsDarkMode` (bool)
- Enables dark mode styling (handled in XAML)

---

## 🧠 Internal Properties

### `ActiveColor`
- Returns current working color (Primary or Secondary)

### HSVA Components

| Property     | Description              |
|--------------|--------------------------|
| `Hue`        | 0–360                    |
| `Saturation` | 0–1                      |
| `Value`      | 0–1                      |
| `Alpha`      | 0–1                      |

Updating any of these updates the color in real time.

---

### `HexValue` (string)

- Format: `#AARRGGBB` or `#RRGGBB`
- Automatically updates color when set
- Safe parsing with validation

---

## 🧩 Methods Breakdown

---

### 🔄 Color Synchronization

#### `UpdateColorFromComponents()`
- Converts HSVA → Color
- Updates `SelectedColor` or `SecondaryColor`
- Prevents recursive updates via `_isUpdatingFromColor`

---

#### `UpdateComponentsFromColor(Color color)`
- Converts Color → HSVA
- Updates sliders and UI state

---

---

### 🎯 SV (Saturation/Value) Handling

#### `UpdateSV(Point position)`
- Converts mouse position → Saturation & Value
- Updates active color

---

#### `UpdateSVSelectorPosition()`
- Moves selector UI to match current SV values

---

---

### 🎨 Swatches

#### `PopulateSwatches()`
- Builds swatch UI dynamically
- Uses:
  - `Swatches` if provided
  - otherwise default palette

---

#### `Swatches_CollectionChanged(...)`
- Refreshes UI when bound collection changes

---

---

### 🎯 Popup Behavior

#### `CustomPopupPlacementCallback(...)`
- Calculates popup position based on:
  - `Placement`
  - `Offsets`
  - `CenterAlign`

---

#### `ColorPickerPopup_Opened(...)`
- Syncs UI when popup opens

---

#### `ColorPickerPopup_Closed(...)`
- Resets `ExternalTrigger` to allow reopening

---

---

### 🎮 Interaction Handlers

#### `PrimaryButton_Click(...)`
- Activates primary color editing

#### `SecondaryButton_Click(...)`
- Activates secondary color editing

#### `Swatch_Click(...)`
- Applies selected swatch color

#### `Slider_PreviewMouseDown(...)`
- Enables click-to-set slider values

---

---

## 🎨 Color Conversion (Core Logic)

---

### `FromHSVA(double h, double s, double v, double a)`

- Converts HSVA → `Color`

---

### `ToHSVA(Color color, out h, out s, out v, out a)`

- Converts `Color` → HSVA

---

### `DarkenColor(Color color, float factor)`
- Used for swatch borders

---

---

## 🔧 Converters Included

- ColorToBrushConverter
- CenteredOffsetConverter
- BooleanToVisibilityConverter
- OpaqueColorConverter

---

## 🧪 Tips & Best Practices

- Use `ObservableCollection<Color>` for dynamic swatches
- Use `ExternalTrigger` in MVVM scenarios
- Bind `SelectedColor` for main usage

---

## 📄 License

MIT (or your preferred license)
