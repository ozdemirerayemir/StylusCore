# StylusCore - Proje Mimarisi ve Kapsamlı Rehber

> **Bu dosya, herhangi bir yapay zeka veya geliştiricinin projeyi hızlıca anlaması ve geliştirmeye devam etmesi için hazırlanmıştır.**

---

## 🎯 Proje Amacı

**StylusCore**, grafik tablet kullanıcıları için özel olarak tasarlanmış, **Feature-Based (Özellik Tabanlı)** mimariye sahip, modern ve yüksek performanslı bir not alma uygulamasıdır. 

**Temel Farklar:**
- ✅ **Feature-Based Architecture:** Modüler, genişletilebilir ve temiz kod yapısı.
- ✅ **Stylus First:** Grafik tablet girişlerini (basınç, eğim) birinci sınıf vatandaş olarak ele alır.
- ✅ **Modern UI:** WPF + MVVM ile modern, akıcı ve temalandırılabilir arayüz.

---

## 🛠️ Teknoloji Yığını

| Teknoloji | Kullanım Amacı |
|-----------|----------------|
| **C# 10 / .NET 8** | Ana Geliştirme Platformu |
| **WPF** | Kullanıcı Arayüzü (UI) |
| **MVVM Pattern** | Mimari Desen (Model-View-ViewModel) |
| **Feature-Based** | Proje Organizasyon Yapısı |
| **Wintab API** | Grafik Tablet Entegrasyonu (Planlanan) |
| **SQLite** | Veri Kalıcılığı (Planlanan) |

---

## 🏗️ Proje Klasör Yapısı (Feature-Based Architecture)

Proje, dikey dilimleme (vertical slicing) mantığıyla özellik bazlı klasörlenmiştir. Bu sayede her özellik kendi içinde (View, ViewModel, Model) bağımsızdır.

```
src/StylusCore.App/
│
├── 📁 Core/                      # === ÇEKİRDEK KATMANI ===
│   │                             # Tüm uygulama tarafından kullanılan ortak iş mantığı.
│   ├── 📁 Models/                # Temel veri yapıları (Notebook, Page, AppTheme vb.)
│   ├── 📁 Services/              # Global servisler (ThemeService, StorageService vb.)
│   └── 📁 ViewModels/            # Global ViewModel'ler (MainViewModel vb.)
│
├── 📁 Features/                  # === ÖZELLİK KATMANI (FEATURE MODULES) ===
│   │                             # Uygulamanın ana fonksiyonel parçaları.
│   ├── 📁 Library/               # Kütüphane Yönetimi Modülü
│   │   ├── 📁 Views/             # (LibraryView.xaml)
│   │   └── 📁 ViewModels/        # (LibraryViewModel.cs)
│   │
│   ├── 📁 Editor/                # Not Alma / Editör Modülü
│   │   ├── 📁 Views/             # (NotebookView.xaml)
│   │   └── 📁 ViewModels/        # (NotebookViewModel.cs)
│   │
│   └── 📁 Settings/              # Ayarlar Modülü
│       ├── 📁 Views/             # (SettingsView.xaml)
│       └── 📁 ViewModels/        # (SettingsViewModel.cs)
│
├── 📁 Shared/                    # === PAYLAŞILAN KATMAN ===
│   │                             # Birden fazla özellik tarafından kullanılan UI bileşenleri.
│   ├── 📁 Components/            # Ortak Kontroller
│   │   ├── Sidebar.xaml          # Sol Navigasyon Menüsü
│   │   └── HeaderControl.xaml    # Üst Bilgi/Arama Çubuğu
│   │
│   └── 📁 Themes/                # Renk ve Stil Tanımları
│       ├── Icons.xaml            # SVG tabanlı ikon setleri
│       ├── LightTheme.xaml       # Açık Tema Renkleri
│       ├── DarkTheme.xaml        # Koyu Tema Renkleri
│       └── StandardControls.xaml # Buton, Textbox vb. genel stiller
│
├── 📁 Dialogs/                   # === DİYALOGLAR ===
│   │                             # Pop-up pencereler ve modallar.
│   ├── InputDialog.cs            # Basit metin girişi
│   └── CreateNotebookDialog.xaml # Yeni defter oluşturma penceresi
│
└── 📁 Views/                     # === KÖK GÖRÜNÜMLER ===
    └── MainWindow.xaml           # Ana Pencere (Shell)
```

---

## 🧩 Uygulama Modülleri ve Sorumlulukları

### 1. `Core` (Çekirdek)
Uygulamanın "beyni" burasıdır. UI'dan bağımsız modeller ve servisler burada bulunur.
- **Models:** Veritabanı veya bellek içi veri yapıları (`Library`, `Notebook`, `Page`).
- **Services:** `ThemeService` (Tema değişimi), `StorageService` (Dosya işlemleri).

### 2. `Features` (Özellikler)
Her özellik kendi klasöründe izole edilmiştir.
- **Library:** Kullanıcının kütüphanelerini listeler, defter ekler/siler.
- **Editor:** `InkCanvas` kontrolünü barındırır, çizim işlemlerini yönetir.
- **Settings:** Uygulama ayarlarını (Tema, Dil) yönetir.

### 3. `Shared` (Paylaşılanlar)
Uygulama genelinde tekrar eden UI parçaları.
- **Themes:** `App.xaml` üzerinden yüklenen renk paletleri. `DynamicResource` kullanarak runtime'da tema değişimine izin verir.
- **Components:** `Sidebar` gibi her sayfada kullanılan bileşenler.

---

## 🎨 Tema Sistemi Nasıl Çalışır?

Renkler **ResourceDictionary** dosyalarında tutulur ve `DynamicResource` anahtarları ile erişilir.

1.  **Tanımlama:** `Shared/Themes/DarkTheme.xaml` içinde renkler tanımlanır:
    ```xml
    <Color x:Key="PrimaryBackgroundColor">#1E1E1E</Color>
    <SolidColorBrush x:Key="PrimaryBackgroundBrush" Color="{StaticResource PrimaryBackgroundColor}"/>
    ```
2.  **Kullanım:** UI tarafında bu anahtara bağlanılır:
    ```xml
    <Grid Background="{DynamicResource PrimaryBackgroundBrush}"> ... </Grid>
    ```
3.  **Değişim:** `ThemeService.cs`, eski sözlüğü (Dictionary) kaldırıp yenisini (örneğin `LightTheme.xaml`) `App.Resources.MergedDictionaries` koleksiyonuna ekler. Böylece tüm UI anında renk değiştirir.

---

## 🚀 Geliştirici Kılavuzu (Yeni Özellik Ekleme)

Eğer projeye yeni bir özellik (örneğin "Export") ekleyecekseniz:

1.  `src/StylusCore.App/Features/` altında **Export** klasörü oluşturun.
2.  İçine `Views` ve `ViewModels` klasörlerini açın.
3.  Gerekli XAML ve CS dosyalarını oluşturun.
4.  Eğer ortak bir bileşen lazımsa, `Shared/Components` altına bakın veya ekleyin.
5.  Eğer global bir veri lazımsa, `Core/Models` kullanın.

---

## 📞 İletişim & Durum

Bu dosya **03.02.2026** tarihinde, projenin Feature-Based mimariye başarıyla geçirilmesinin ardından güncellenmiştir.
Tüm modüller test edilmiş ve çalışır durumdadır.
