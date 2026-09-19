# Smash Jeeps — Multiplayer Battle Arena

Server-authoritative multiplayer vehicle combat game with physics-driven gameplay, real-time state synchronization, and latency-compensated networking. Built with **Unity 6** and **Netcode for GameObjects (NGO)**.

---

## Gameplay

https://github.com/user-attachments/assets/b8f703b7-7459-475e-ac21-5b90083acb72

https://github.com/user-attachments/assets/df083833-54dc-4d94-9ab7-02373d4590f0

https://github.com/user-attachments/assets/443a8c23-dea3-47d3-9d46-f2a5624f6255

https://github.com/user-attachments/assets/c7800255-76b1-47d6-995a-cac7c51a4999

---

## Technical Implementation

### Networking

- **Server-Authoritative Architecture** — All gameplay-critical logic (damage resolution, kill validation, win conditions) executes on the server. Clients send inputs and render validated state. No client-side trust.
- **RPC Pipeline** — `ServerRpc` for player actions (fire, boost, ability). `ClientRpc` for broadcasting validated outcomes (hit confirmation, death events). Payloads kept minimal to reduce per-tick bandwidth.
- **Reactive State Sync** — Server-owned `NetworkVariable<T>` for health, score, and game phase. UI and gameplay systems subscribe to `OnValueChanged` callbacks — zero polling, zero redundant traffic on unchanged state.
- **Custom Serialization** — Struct-based `INetworkSerializable` implementations for damage packets and vehicle loadouts. Avoids heap allocation from default serializer boxing, keeps GC pressure low during combat.
- **Object Lifecycle** — Server-controlled `NetworkObject` spawning with explicit ownership. Projectiles and VFX use object pooling to eliminate runtime `Instantiate()` / `Destroy()` overhead.

### Physics & Combat

- **Server-Owned Simulation** — `NetworkRigidbody` with server authority for deterministic collision resolution. Clients interpolate transforms only — no local physics simulation on networked objects.
- **Damage Pipeline** — Server `OnCollisionEnter` → force magnitude and hit angle evaluation → damage calculation against target stats → `NetworkVariable<float>` health update → targeted `ClientRpc` for hit feedback on the affected client.
- **Latency Handling** — `NetworkTransform` with tuned interpolation buffers and extrapolation limits. Vehicles remain visually consistent at 80–120ms RTT without rubber-banding.
- **Physics Layer Optimization** — Collision layers configured to minimize broadphase overlap checks. Networked projectiles interact only with relevant layers.

### Architecture

- **Finite State Machine** — Session flow driven by discrete states (`Lobby → Countdown → InGame → GameOver`). Transitions are server-initiated and broadcast via `NetworkVariable<GamePhase>` for lockstep client synchronization.
- **Observer / Event-Driven UI** — HUD and menu systems fully decoupled from network and gameplay layers. All UI updates are reactive through C# events and `NetworkVariable` callbacks.
- **Factory + Data-Driven Config** — Vehicle spawning through a centralized factory mapped to `ScriptableObject` definitions. All vehicle parameters (speed, acceleration, health, mass, drag) are asset-driven — new variants require zero code changes.
- **Separation of Concerns** — Network transport, input handling, gameplay logic, physics response, and UI isolated into single-responsibility systems.

### Performance

- **Struct-first serialization** for all network data types — zero GC allocations on the hot path.
- **Object pooling** for projectiles and particles — no runtime instantiation during gameplay.
- **Bandwidth discipline** — RPC payloads under 64 bytes. `NetworkVariable` delta compression via NGO's built-in dirty-checking.
- **GPU instancing-friendly shaders** via Shader Graph for repeated vehicle variants.

---

## Tech Stack

| Domain | Technology |
|---|---|
| Engine | Unity 6 (6000.0.31f1) |
| Language | C# |
| Networking | Netcode for GameObjects |
| Physics | Unity Physics + NetworkRigidbody |
| Rendering | URP + Shader Graph |
| Animation | DOTween |

---

## Credits

Based on the multiplayer game development course by **SkinnyDev**. The course provided the networking foundation which was then extended with the architecture and optimization decisions described above.

- **YouTube:** [@skinnydev](https://www.youtube.com/@skinnydev)
- **Discord:** [SkinnyDev Community](https://discord.gg/WMaqkSUHaU)
