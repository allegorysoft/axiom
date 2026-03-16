---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
  name: "Axiom Framework"
  text: "Open Source .NET Application Framework"
  tagline: "Modular by design. Extensible by default. Clean architecture without the boilerplate."
  actions:
    - theme: brand
      text: Get Started →
      link: /get-started/overview

features:
  - icon: 🔍
    title: Reflection-Based Dependency Injection
    details: Automatically discover and register services by scanning assemblies. Use marker interfaces or attributes — no manual wiring needed.

  - icon: 🧩
    title: Built-in Modularity & Plugin System
    details: Compose your application from self-contained modules with ordered lifecycle hooks. Drop in plugin assemblies at runtime without recompiling.

  - icon: 🔗
    title: Interception for Cross-Cutting Concerns
    details: Apply logging, caching, transactions, and authorization transparently via a Castle DynamicProxy-backed AOP pipeline — zero changes to your business logic.
---

::: warning 🚧 Work in Progress
Documentation is actively being written. Some pages may be incomplete or missing.
Follow us on [GitHub](https://github.com/allegorysoft/axiom) for updates.
:::