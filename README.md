# RagePit

RagePit is a fast-paced, 5v5 third-person multiplayer arena combat game built in Unity. Players engage in intense sword fights with fluid animations and responsive controls. Designed for browser-based gameplay, it emphasizes accessibility and adrenaline-pumping action.

## Features

- Third-person combat system with smooth animations  
- AI-ready design for future social deduction mechanics 
- Locomotion blending (idle, walk, run)  
- Context-sensitive jump animations (idle→jump, walk→jump, run→jump)   

## Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download)
- Unity Editor version `2022.3 LTS` or newer (URP recommended)
- Git

### Installation

1. **Clone the repository:**

```bash
git clone https://github.com/your-username/RagePit.git
```

2. **Open with Unity Hub:**

- Launch Unity Hub

- Click on Add project

- Select the cloned RagePit folder

3. **Open the project:**

- Select the project in Unity Hub and hit Open

4. **Set input system support (if prompted):**

- Accept Unity's prompt to enable both the legacy and new input system

### Project Structure

```bash
RagePit/
├── Assets/
│   ├── Animations/          # All animation FBX and clips
│   ├── Scripts/             # Game logic and input handling
│   ├── Models/              # Character models and weapons
│   ├── Prefabs/             # Reusable game objects
│   └── Scenes/              # Main and test scenes
└── ProjectSettings/         # Unity project settings
```

### Controls (Default)

| Action       | Key            | Description                     |
|--------------|----------------|---------------------------------|
| Move         | W / A / S / D  | Basic movement                  |
| Run          | Left Shift     | Sprint while moving             |
| Jump         | Space          | Context-based jump animation    |

### Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you'd like to change.

### Licensing

This project is licensed under the [MIT License](https://opensource.org/license/MIT).
