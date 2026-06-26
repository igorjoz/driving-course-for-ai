# driving-course-for-ai

AI: "What is my purpose?"
<br>
Us: "You park cars."
<br>
AI: "OH MY GOD..."

## Overview

`driving-course-for-ai` is a Unity project created in 2024 for an Artificial Intelligence course. The goal is to train an ML-Agents driver to park a car in a parking lot whose layout can be randomized between episodes.

The agent controls steering, acceleration/reverse, and braking. The environment rewards useful parking behavior, such as entering the parking area and stopping inside an available parking spot, and penalizes collisions, leaving the parking area, and taking too long to finish an episode.

Authors:

- [Igor Józefowicz](https://github.com/igorjoz)
- [Adrian Ściepura](https://github.com/Adrian-Sciepura)

## Requirements

- Unity Hub.
- Unity Editor `6000.5.1f1`; this is the version recorded in `ProjectSettings/ProjectVersion.txt`.
- A platform supported by Unity 6.5.
- Internet access during the first project open, so Unity can restore packages.
- Optional: Python and the `mlagents` CLI if you want to train the agent from ML-Agents.

Important Unity packages:

- `com.unity.ml-agents` `4.0.3`
- `com.unity.ai.inference` `2.6.1`, installed as an ML-Agents dependency
- `com.unity.ugui` `2.5.0`

## Repository Layout

```text
.
+-- README.md
+-- requirements-mlagents.txt
+-- config/
|   +-- driver_ppo.yaml
|   +-- driver_ppo_long_training.yaml
+-- docs/
|   +-- _config.yml
|   +-- index.md
+-- driving-course-for-ai/
    +-- AI_LearningData.json
    +-- Assets/
    |   +-- Scenes/MainScene.unity
    |   +-- Scripts/
    |   +-- Prefab/
    +-- Packages/
    +-- ProjectSettings/
```

Key files:

- `Assets/Scenes/MainScene.unity` - main simulation scene.
- `Assets/Prefab/Sedan.prefab` - agent prefab with ML-Agents components.
- `Assets/Scripts/CarController.cs` - agent logic, car control, observations, actions, rewards, and episode reset.
- `Assets/Scripts/MapController.cs` - randomizes free and occupied parking spots.
- `Assets/Scripts/GameManager.cs` - loads learning and reward settings from JSON and exposes them through a scene-wide manager.
- `Assets/Scripts/DriverLearningData.cs` - serializable data model for learning configuration.
- `AI_LearningData.json` - current reward, episode, and randomization settings.

## Running In Unity

1. Clone the repository.
2. Open Unity Hub.
3. Add the `driving-course-for-ai/` folder as a project. This is the folder that contains `Assets/`, `Packages/`, and `ProjectSettings/`.
4. Open the project with Unity `6000.5.1f1`.
5. Open `Assets/Scenes/MainScene.unity`.
6. Press **Play**.

Unity may regenerate C# project files and import assets during the first launch.

## Manual Control

`CarController` implements the ML-Agents `Heuristic` method, so the car can be controlled manually:

- `Horizontal` - steering, usually left/right arrows or `A`/`D`.
- `Vertical` - acceleration and reverse, usually up/down arrows or `W`/`S`.
- `Space` - brake.
- `R` - reload `AI_LearningData.json` through `GameManager` and print the applied parameter changes in Unity Console.

If the car does not react while no trained model is assigned, set the `Behavior Parameters` component on the `Sedan` prefab to `Heuristic Only`.

## Agent Design

The agent is implemented by `CarController`, which derives from `Unity.MLAgents.Agent`.

Current `Sedan` behavior setup:

- Behavior Name: `Driver`
- Continuous actions: `3`
- Decision Period: `5`
- Child sensors enabled
- Ray Perception Sensor: `6` rays per direction, `180` degrees, length `15`

Actions:

- `ContinuousActions[0]` - steering.
- `ContinuousActions[1]` - forward or reverse motor input.
- `ContinuousActions[2]` - braking.

Code-level observations:

- Current car speed from `rigidbody.linearVelocity.magnitude`.
- The prefab also uses a Ray Perception Sensor to observe nearby scene objects.

Reward and episode behavior:

- Entering an available parking spot gives a positive reward.
- Stopping inside an available spot for long enough, at low speed, ends the episode successfully.
- Leaving the parking area, hitting a fence, or hitting an occupied parking spot gives a penalty and ends the episode.
- Exceeding the maximum episode time gives a penalty and ends the episode.
- A small per-timestep penalty encourages faster parking.

## Learning Configuration

Environment and reward parameters are stored in `driving-course-for-ai/AI_LearningData.json`. `GameManager` loads this file at runtime. If the file is missing, the project creates a default configuration from `DriverLearningData.CreateDefault()`.

Main configuration sections:

- `mapRandomizationData` - controls whether the parking layout is randomized every episode and how many free spots are required.
- `carRandomizationData` - controls randomized starting rotation for the car.
- `basicData` - controls maximum episode time and time-based rewards or penalties.
- `availableParkingSpaceData` - rewards for entering, leaving, staying in, and covering an available spot.
- `occupiedParkingSpaceData` - collision penalty for occupied spots.
- `areaData` - rewards and penalties for parking-area trigger zones.
- `fenceData` - collision penalty for fences.

During Play Mode, you can edit `AI_LearningData.json` and press `R` to reload it without restarting the editor. The reload does not show an on-screen notification, but Unity Console logs whether the file changed any learning parameters and lists each applied value change.

`GameManager` uses an early execution order so that ML-Agents can reset agents safely when Play Mode starts. `CarController` and `MapController` resolve the manager through `GameManager.TryGetInstance(...)`, which avoids initialization-order null references after the Unity 6.5 upgrade.

## Training With ML-Agents

The repository contains two PPO training presets. Both target the `Driver` behavior used by the `Sedan` agent:

- `config/driver_ppo.yaml` - a shorter run for quick iteration.
- `config/driver_ppo_long_training.yaml` - a long run intended to train for several hours and save evenly spaced checkpoints.

Use Python `3.10` for the ML-Agents trainer; the upstream ML-Agents documentation was tested with Python `3.10.12`. This project uses Unity ML-Agents `4.0.3`, which matches the Python package `mlagents==1.1.0`. If you still have an older virtual environment from ML-Agents `0.30.0`, recreate it before training.

```powershell
winget install --id Python.Python.3.10 --source winget
```

Create the virtual environment from the repository root, the directory that contains `README.md`, `config/`, and `requirements-mlagents.txt`:

```powershell
py -3.10 -m venv .venv
.\.venv\Scripts\python.exe -m pip install --upgrade pip
.\.venv\Scripts\python.exe -m pip install -r requirements-mlagents.txt
```

Start training from the repository root:

```powershell
.\.venv\Scripts\python.exe -m mlagents.trainers.learn config\driver_ppo.yaml --run-id driver-ppo
```

For a longer run intended to last several hours, use:

```powershell
.\.venv\Scripts\python.exe -m mlagents.trainers.learn config\driver_ppo_long_training.yaml --run-id driver-ppo-long
```

The long preset trains for `20,000,000` steps, writes TensorBoard summaries every `50,000` steps, saves a checkpoint every `500,000` steps, and keeps the last `40` checkpoints. Actual runtime depends on Unity simulation speed, the number of parallel map copies in the scene, and hardware.

ML-Agents stores run data in `results/<run-id>/`. If a directory for the same run id already exists, ML-Agents stops before training starts. Pick the command that matches what you want to do:

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

When the CLI waits for the Unity environment, open `driving-course-for-ai/Assets/Scenes/MainScene.unity` and enter Play Mode. ML-Agents will connect to the running scene and begin collecting experience.

If your terminal is inside the Unity project directory (`driving-course-for-ai/`), go back to the repository root first:

```powershell
cd ..
```

The included starter config:

```yaml
behaviors:
  Driver:
    trainer_type: ppo
    hyperparameters:
      batch_size: 1024
      buffer_size: 10240
      learning_rate: 0.0003
      beta: 0.005
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
      learning_rate_schedule: linear
    network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    keep_checkpoints: 10
    checkpoint_interval: 100000
    max_steps: 5000000
    time_horizon: 64
    summary_freq: 10000
```

The long preset changes only the training duration and logging/checkpoint cadence:

```yaml
    keep_checkpoints: 40
    checkpoint_interval: 500000
    max_steps: 20000000
    summary_freq: 50000
```

Useful training options:

- `--run-id driver-ppo` names the training run and creates `results/driver-ppo/`.
- `--resume` continues an existing run with the same run id.
- `--force` overwrites an existing run with the same run id.
- `--time-scale 20` can speed up simulation while training.

After training, assign the exported `.onnx` model in the `Behavior Parameters` component on the `Sedan` prefab if you want to run inference.

## Monitoring Training

ML-Agents writes TensorBoard summaries to the `results/` directory. Start TensorBoard from the repository root:

```powershell
.\.venv\Scripts\python.exe -m tensorboard.main --logdir results
```

Open the local URL printed by TensorBoard, usually `http://localhost:6006/`. The `TensorFlow installation not found - running with reduced feature set` message is expected here; ML-Agents logs still display correctly. Press `Ctrl+C` in the TensorBoard terminal to stop it.

The most useful charts while training are:

- `Environment/Cumulative Reward` - should trend upward when the agent improves.
- `Policy/Entropy` - shows how exploratory the policy is.
- `Losses/Policy Loss` and `Losses/Value Loss` - help spot unstable training.
- `Policy/Learning Rate` - confirms the learning-rate schedule.

## Build

`MainScene` is not currently listed in `EditorBuildSettings.asset`, so add it to **File -> Build Settings -> Scenes In Build** before creating a player build.

Basic editor build flow:

1. Open `Assets/Scenes/MainScene.unity`.
2. Open **File -> Build Settings**.
3. Add the open scene to the build.
4. Choose a target platform, such as Windows, macOS, Linux, or WebGL.
5. Build the project.

Project settings currently use `1920x1080` as the default resolution and `960x600` for WebGL.

## Current State

- The documentation reflects the current repository state.
- No trained `.onnx` model is currently assigned to the agent.
- The project includes Synty Studios assets; check the relevant asset licenses before redistributing packaged assets publicly.
