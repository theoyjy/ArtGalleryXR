# Instruction to Compile and Deploy

## Prerequisites

Before you begin building the project, ensure the following steps are completed:

### Install Unity Hub
- Download and install Unity Hub.
- Launch Unity Hub and sign in with your Unity account.

### Install Unity Editor Version 6000.0.26f1
- In Unity Hub, go to the **Installs** tab and click **Install Editor**.
- Select version **6000.0.26f1**.
- Ensure that **Windows Build Support (IL2CPP)** is selected during installation.

> ⚠️ **Troubleshooting**: If the build support module was not selected initially, you can add it later via the three dots next to the installed version → **Add Modules**.

### Open the Project
- Navigate to the **Projects** tab and click **Open Project**.
- Select the `ArtGallery` folder and open it using the Unity version you just installed.
- Unity will auto-import and compile the assets and packages.

### Verify
- No console errors after opening the project.
- All packages are resolved: **Window → Package Manager**.
- **Scripting Backend** is set correctly:  
  `Edit → Project Settings → Player → Configuration → Scripting Backend = IL2CPP`.

> If errors show up:
> - Try **Assets → Reimport All**.
> - Delete the **Library** folder and re-open the project to force a clean reimport.

---

## Setting Up Stability AI API

The ArtGallery project uses the StabilityAI API to generate AI-driven content during runtime.

### Create a StabilityAI Account and Generate an API Key
1. Go to [https://platform.stability.ai](https://platform.stability.ai)
2. Click **Sign Up** (or **Log In** if you already have an account).
3. Once logged in, go to the **API Keys** section:
   - Find it under your profile → **API Access**
   - Or directly: [https://platform.stability.ai/account/keys](https://platform.stability.ai/account/keys)
4. Click **Generate API Key**.
5. Copy the newly created key.

### Set the Environment Variable

#### For Windows:
Open Command Prompt and run:
```bash
setx STABILITY_API_KEY=YOUR_API_KEY_HERE
```

#### For Android:
- Open `StabilityAIImage.cs` under:
  `Assets/CustomPlayerPrefabsAndScripts/AIGeneration`
- On **line 58**, hardcode your API key before building for Android.

---

## Building the Project for Windows

### Select the Build Profile
- File → **Build Settings**
- Select the **Windows** build target.
- If not activated, click **Switch Platform**.

### Verify Build Settings
Ensure the following scenes are listed under **Open Scenes** in this order:
1. Init
2. Login
3. LoginUI
4. Lobby
5. Gallery

If not:
- Click **Add Open Scenes** to include missing ones.
- Drag scenes to reorder as needed.

Set **Build Location** to a known folder, e.g., `Builds/Windows/`.

### Check Player Settings
- File → **Build Settings → Player Settings**
- Confirm:
  - Company Name and Product Name are correct.
  - Resolution and presentation settings are appropriate.

### Build the Project
- In **Build Settings**, click **Build** (not *Build and Run* unless you want to launch immediately).
- Choose an output folder (e.g., `Builds/Windows/`).
- Wait for Unity to compile and generate the executable.

> ⚠️ **Build issues may occur if:**
> - Platform modules are not installed (e.g., IL2CPP for Windows).
> - There are script compilation errors.
> - Asset GUIDs have changed or are missing.

> 🧪 **If errors occur:**
> - Check the **Console** for specific errors.
> - Try restarting Unity and rebuilding.
> - Clear **Library** and rebuild if persistent.

### Verify the Output
Navigate to the build output folder. You should see:
- `ArtGallery.exe` (or specified name)
- `ArtGallery_Data` folder
- Other required files (e.g., `UnityPlayer.dll`)

---

## Building the Project for MetaQuest 2

### Open Build Settings
In Unity: **File → Build Settings**

### Select the Target Platform
- Set target platform to **Android** (Meta Quest 2 runs on Android).
- Add scenes to the build if not already included.
- Change **Run Device** to Meta Quest 2.

### Build the Project
- Click **Build** (not *Build and Run* unless you want to launch immediately).
- Choose an output folder, e.g., `Builds/Android/`.
- Wait for Unity to compile and generate the APK file.

