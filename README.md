# StylusCore 🖊️

**Modern, Feature-Based (Özellik Tabanlı) Mimariye Sahip Grafik Tablet Not Alma Uygulaması.**

StylusCore, grafik tablet kullanıcıları için geliştirilmiş, yüksek performanslı ve özelleştirilebilir bir not alma uygulamasıdır. Proje, modern yazılım geliştirme prensiplerine (Clean Code, MVVM, Modular Design) sıkı sıkıya bağlı kalınarak geliştirilmektedir.

## 🌟 Öne Çıkan Özellikler

- **Feature-Based Architecture:** Modüler yapı sayesinde kolay geliştirilebilir ve bakımı yapılabilir kod tabanı.
- **Dinamik Tema Sistemi:** 🌙 Dark / ☀️ Light mod desteği (Anlık geçiş).
- **Gelişmiş Klasörleme:** Kütüphane -> Defter -> Bölüm -> Sayfa hiyerarşisi.
- **Modern UI:** WPF ve XAML ile hazırlanmış akıcı kullanıcı arayüzü.

## 📂 Proje Yapısı

Proje, özellik tabanlı (Feature-Based) bir mimariye sahiptir. Detaylı teknik bilgi ve klasör yapısı için lütfen **[ARCHITECTURE.md](ARCHITECTURE.md)** dosyasını inceleyin.

Kısaca yapı:
- `Features/`: Library, Editor, Settings gibi ana modüller.
- `Shared/`: Ortak bileşenler ve temalar.
- `Core/`: Global modeller ve servisler.

## 🚀 Kurulum ve Çalıştırma

1.  Gereksinimler: **.NET 8 SDK**
2.  Projeyi klonlayın.
3.  Terminali `src/StylusCore.App` klasöründe açın.
4.  Çalıştırın:
    ```powershell
    dotnet run
    ```

## 📜 Lisans

MIT License.
