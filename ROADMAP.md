# StylusCore — ROADMAP

---

## Phase X: Architecture Hardening (Audit-Driven)

> Items below were identified by the System Audit Report (2026-02-12)
> and address documentation / architectural gaps.

- [ ] **Document GPU Acceleration Strategy** (DirectX tile rendering, `BitmapCache`, `RenderOptions.CachingHint`) — Ref: `.agent/05_RENDERING_PERF_WPF.md`
- [ ] **Implement .NET 9/10 Fluent Theme integration** — evaluate native WPF Fluent Theme alongside existing custom theming system
- [ ] **Formalize Dependency Injection (DI) strategy** in `.agent/02_ARCHITECTURE.md` — `Microsoft.Extensions.DependencyInjection`, service registration, lifetime scopes
- [ ] **Define global Telemetry & Error Handling policy** — structured logging (Serilog), crash reporting, user-facing error handling conventions
- [ ] **Expand Accessibility coverage** — UI Automation patterns, narrator/screen reader testing, high contrast mode verification
