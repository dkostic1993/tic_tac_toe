## WebGL Build (playable)

### 1) Open project
- Open the repo root in Unity Hub (Unity 2022.3 LTS).

### 2) Add scenes to Build Settings
- `File -> Build Settings...`
- Add:
  - `Assets/Scenes/Play.unity`
  - `Assets/Scenes/Game.unity`
- Ensure `Play` is **Scene 0**.

### 3) Switch platform
- Select **WebGL** and click **Switch Platform**.

### 4) Player settings (recommended defaults)
- `Project Settings -> Player -> Resolution and Presentation`:
  - Enable “Run In Background” if desired
  - Leave orientation as “Auto Rotation” (UI handles both portrait/landscape)

### 5) Build
- In Build Settings click **Build** (or **Build And Run**).
- Upload the build folder to any static hosting (GitHub Pages, itch.io, Netlify, etc.) and submit the public link.
