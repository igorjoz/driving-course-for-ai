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

Install the ML-Agents CLI in a Python environment:

```bash
python -m pip install mlagents
```

Start training from the repository root:

```bash
mlagents-learn config/driver_ppo.yaml --run-id driver-ppo
```

When ML-Agents waits for the Unity environment, open `Assets/Scenes/MainScene.unity` and enter Play Mode. Training summaries are written to `results/`.

To monitor training in TensorBoard:

```bash
tensorboard --logdir results
```

Then open the local TensorBoard URL printed in the terminal.
