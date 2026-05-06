---
name: unity-playable-ads
description: "Use when working with Unity 2D, Playable Ads, gameplay systems, clean C# architecture, animation without external libraries, performance optimization, and debugging Unity code. Keywords: Unity, MonoBehaviour, gameplay, drag, throw, animation, optimization, playable ads, Luna"
---

## Purpose

    git add .github/skills/unity-playable-ads-expert

git commit -m "feat: add Unity 2D Playable Ads Expert skill"
This skill provides guidelines for developing Unity 2D Playable Ads with clean, optimized, and production-ready code. It focuses on core gameplay implementation, performance constraints, and best practices for Playable Ads environments (such as Luna).

---

## Core Rules

- Use MonoBehaviour structure
- Keep code clean and optimized
- Avoid GC allocation
- Do not break existing flow
- Focus on completing the core gameplay first, avoid unnecessary features or over-engineering
- Always prefer compatibility and stability over advanced Unity features

---

## Coding Guidelines

- Apply SOLID principles (focus on Single Responsibility, Open/Closed, Dependency Inversion)
- Provide full script
- Use proper naming conventions
- Explain briefly if needed
- Prioritize simplicity and fast implementation over scalability

---

## Communication

- Always ask clarifying questions in Vietnamese if any requirement is ambiguous. Do not assume.

---

## Performance Rules

- Avoid heavy logic inside Update()
- Cache references, do not use Find() at runtime
- Minimize per-frame allocations
- Avoid frequent Instantiate/Destroy
- Reuse objects when possible (simple pooling if needed)

---

## Animation Rules

- Do NOT use DOTween or any external tweening libraries
- Use Unity built-in systems (Coroutine, Lerp, Animation, Animator)
- Keep animations lightweight and optimized for Playable Ads
- Avoid complex Animator Controllers

---

## Physics Rules

- Avoid complex physics simulation
- Prefer simple collision checks (Overlap, Raycast)
- Do not rely on precise physics behavior

---

## Rendering Rules

- Use a single camera
- Avoid post-processing effects

---

## Input Rules

- Use simple input handling (Input.GetMouseButton / Touch)
- Avoid new Input System package

---

## Audio Rules

- Use simple audio playback
- Avoid dynamic audio loading
- Ensure audio is preloaded and referenced

---

## Determinism

- Use controlled randomness
- Avoid unpredictable gameplay outcomes

---

## Time Rules

- Do not rely on Time.timeScale for core gameplay logic

---

## Avoid Using

- Resources.Load(...)
- Loading assets via string/path at runtime
- System.IO (file read/write)
- Reflection (GetType, Invoke, etc.)
- Complex threading / async tasks
- FontEngine.LoadFontFace
- StreamingAssets
- External animation libraries (e.g., DOTween)

---

## Prefer Using

- [SerializeField] to reference assets directly
- Drag & drop assets in the Inspector
- Coroutines for simple animations
- Mathf.Lerp / Vector3.Lerp for transitions

---

## Example Use Cases

- Create core gameplay systems (drag, throw, spin, interaction)
- Implement lightweight animations without external libraries
- Optimize Unity scripts for Playable Ads platforms
- Debug unexpected Unity behavior
- Refactor code to follow clean architecture and performance best practices
