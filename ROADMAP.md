# 🗺️ StylusCore Yol Haritası (Roadmap)

> **Son Güncelleme:** 03.02.2026

## 🚀 Mevcut Durum: Faz 2 - Mimari Refactoring (TAMAMLANDI)

Projenin altyapısı, ölçeklenebilir ve sürdürülebilir bir yapıya kavuşturulmak üzere tamamen yenilenmiştir.

### ✅ Tamamlanan Görevler
- [x] **Feature-Based Architecture Geçişi:**
    - Proje `Features`, `Shared` ve `Core` olarak yeniden yapılandırıldı.
    - Tüm View ve ViewModel'ler ilgili özellik klasörlerine taşındı.
- [x] **Global Namespace Refactoring:**
    - `StylusCore.App.Features.*` ve `StylusCore.App.Shared.*` namespace'leri güncellendi.
- [x] **Dialog Sistemi Düzenlemesi:**
    - `InputDialog` ve `CreateNotebookDialog` özel bir `Dialogs` klasörüne taşındı.
- [x] **Tema ve Kaynak Yönetimi:**
    - `ThemeService` yeni klasör yapısına (`Shared/Themes/`) göre güncellendi.
    - Kayıp referans hataları giderildi.
- [x] **Build & Run:**
    - Proje hatasız derleniyor ve çalışıyor.

---

## 🚧 Faz 3: Fonksiyonel Derinleşme (SIRADAKİ ADIMLAR)

Mimari iskelet sağlamlaştıktan sonra odak noktamız özelliklerin içini doldurmak ve kullanıcı deneyimini zenginleştirmektir.

### 🎯 Öncelikli Hedefler
- [ ] **Kalıcı Veri Katmanı (Persistence):**
    - SQLite entegrasyonu ile notların ve kütüphane yapısının diskte saklanması.
    - Şu anki "Mock Data" (Geçici Veri) yapısının veritabanı ile değiştirilmesi.
- [ ] **Gelişmiş Çizim Araçları:**
    - `InkCanvas` üzerinde kalem basınç hassasiyetinin (Pressure Sensitivity) optimize edilmesi.
    - Silgi ve Vurgulayıcı araçlarının geliştirilmesi.
- [ ] **Kullanıcı Ayarları:**
    - Seçilen temanın ve son açılan sayfanın uygulama yeniden başlatıldığında hatırlanması.

---

## 🔮 Gelecek Vizyonu (Backlog)

- [ ] **Cross-Platform Hazırlığı:** Yapının gelecekte MAUI veya Avalonia'ya geçişe uygun tutulması.
- [ ] **Cloud Sync:** Notların bulut (Google Drive/OneDrive) ile senkronizasyonu.
- [ ] **Plugin Sistemi:** Dışarıdan eklenti (yeni kalem uçları, şablonlar) desteği.
