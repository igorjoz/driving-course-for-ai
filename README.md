# driving-course-for-ai

AI: "What is my purpose?"
<br> 
Us: "You park cars."
<br>
AI: "OH MY GOD..."

## About

`driving-course-for-ai` is a Unity project where an ML-Agents driver learns how to park a car in a randomized parking lot. The project was created in 2024 for an Artificial Intelligence course at Gdańsk University of Technology. And for fun :D

Authors:

- [Igor Józefowicz](https://github.com/igorjoz)
- [Adrian Ściepura](https://github.com/Adrian-Sciepura)

## Project At A Glance

- Engine: Unity `2022.3.62f2`
- Main scene: `driving-course-for-ai/Assets/Scenes/MainScene.unity`
- AI package: Unity ML-Agents `2.0.1`
- Agent behavior name: `Driver`
- Reward and randomization config: `driving-course-for-ai/AI_LearningData.json`
- Project scripts: `driving-course-for-ai/Assets/Scripts/`

## Running The Project

1. Install Unity Hub and Unity Editor `2022.3.62f2`, or a compatible `2022.3` LTS version.
2. In Unity Hub, add the `driving-course-for-ai/` folder as a project.
3. Wait for Unity to restore packages from `Packages/manifest.json`.
4. Open `Assets/Scenes/MainScene.unity`.
5. Enter Play Mode.

Manual control is available through the ML-Agents heuristic path: `Horizontal`, `Vertical`, and `Space` for braking. If the car does not respond while no trained model is assigned, set the `Behavior Parameters` component to `Heuristic Only`.

## Documentation

Full documentation is available in [`docs/index.md`](docs/index.md).

## Training

Use Python 3.9 for the ML-Agents trainer. This project uses Unity ML-Agents `2.0.1`, which matches the Python package `mlagents==0.30.0`; that old trainer stack is fragile on newer Python versions.

```powershell
winget install --id Python.Python.3.9 --source winget
```

Create the virtual environment from the repository root, the directory that contains `README.md`, `config/`, and `requirements-mlagents.txt`:

```powershell
& "$env:LocalAppData\Programs\Python\Python39\python.exe" -m venv .venv
.\.venv\Scripts\python.exe -m pip install --upgrade pip
.\.venv\Scripts\python.exe -m pip install -r requirements-mlagents.txt
```

Start training from the repository root:

```powershell
.\.venv\Scripts\python.exe -m mlagents.trainers.learn config\driver_ppo.yaml --run-id driver-ppo
```

For a longer run intended to last several hours, use the long-training preset:

```powershell
.\.venv\Scripts\python.exe -m mlagents.trainers.learn config\driver_ppo_long_training.yaml --run-id driver-ppo-long
```

This preset trains for `20,000,000` steps, writes TensorBoard summaries every `50,000` steps, saves a checkpoint every `500,000` steps, and keeps the last `40` checkpoints. On the machine used to verify the setup, `500,000` steps took about 11 minutes, so the full run should be in the range of several hours. Actual runtime depends on Unity simulation speed, the number of parallel map copies in the scene, and hardware.

If `results/driver-ppo` already exists, choose one of these options:

```powershell
# Continue the existing run
.\.venv\Scripts\python.exe -m mlagents.trainers.learn config\driver_ppo.yaml --run-id driver-ppo --resume

# Start over and overwrite the existing run
.\.venv\Scripts\python.exe -m mlagents.trainers.learn config\driver_ppo.yaml --run-id driver-ppo --force

# Keep the old run and create a new one
.\.venv\Scripts\python.exe -m mlagents.trainers.learn config\driver_ppo.yaml --run-id driver-ppo-2
```

The trainer should print:

```text
Listening on port 5004. Start training by pressing the Play button in the Unity Editor.
```

Then open `driving-course-for-ai/Assets/Scenes/MainScene.unity` in Unity and enter Play Mode. Training summaries are written to `results/`.

To monitor training in TensorBoard:

```powershell
.\.venv\Scripts\python.exe -m tensorboard.main --logdir results
```

Then open the local TensorBoard URL printed in the terminal, usually `http://localhost:6006/`. The `TensorFlow installation not found - running with reduced feature set` message is expected here; ML-Agents logs still display correctly. Press `Ctrl+C` in the TensorBoard terminal to stop it.
