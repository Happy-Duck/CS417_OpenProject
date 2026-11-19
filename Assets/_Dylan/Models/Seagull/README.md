# Seagull

Drag `Assets/_Dylan/Prefabs/Seagull.prefab` into your scene under `DylanSceneRoot`.
Its root Animator plays the looping `Fly` wing-flap animation automatically in Play mode.
The animation stays in place: move the prefab root with your flight/path script to make it travel.
Adjust the root scale to fit your scene. The FBX was exported at 5% of the source scale.

The prefab uses the exported FBX and four URP Lit materials, so Blender is not required to use it.
The original `Assets/_Dylan/Seagullboyo.blend` is retained as the source; Unity's direct `.blend`
import requires a locally installed Blender, but the prefab does not reference that file.
The former Visual object is now the prefab root. A Box Collider approximates the bird; it does not deform with the wings. A kinematic Rigidbody without gravity is included, using Continuous Speculative collision detection.

## Attribution

"Low-Poly Seagull (with Animation & Rigged)" by **simonaskLDE**.

- Source: https://sketchfab.com/3d-models/low-poly-seagull-with-animation-rigged-985024328902444c8270c0f09acc897e
- Author: https://sketchfab.com/simonaskLDE
- License: Creative Commons Attribution 4.0 International (CC BY 4.0)
- License terms: https://creativecommons.org/licenses/by/4.0/
- Changes: converted the supplied Blender model to FBX, adjusted export scale, recreated colors as URP materials, and configured a looping Unity Animator and prefab.

Keep this credit with the project and include it in distributed game credits.

The outer Seagull pivot is aligned with Armature's saved position while keeping root rotation at (0, 0, 0). Armature's local position is zero; its rig orientation is retained. Child, collider, and animation position offsets preserve the model placement and animation without adding a wrapper. `FlyCentered.anim` contains the corresponding animation offsets.
