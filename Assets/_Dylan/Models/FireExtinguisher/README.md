# Fire Extinguisher

- Place `Assets/_Dylan/Prefabs/FireExtinguisher.prefab` and `FireExtinguisherHolder.prefab` independently under `DylanSceneRoot`.
- The extinguisher uses the package XR Grab Interactable, a bottle Capsule Collider plus a handle Box Collider, and a dynamic Rigidbody with gravity. GrabAttach sets the grip position.
- The holder has a solid Mesh Collider and no Rigidbody. Its Socket child uses the package XR Socket Interactor and a Box Collider trigger. Release the extinguisher inside the trigger to dock it; grab it again to remove it.
- Socket/SocketAttach controls the docking position and rotation. It is calibrated to seat the bottle on the base with its outer +Z axis pointing toward the backplate (holder local -Z).
- Use the scene's XR Interaction Manager and XR controllers with compatible interaction layers. The socket uses the default interaction layer and can accept other compatible grab interactables too.
- Interaction uses package components only. Headset grabbing and docking still need a Play-mode check.

Source: Workshop Tools - Free Low Poly Asset Pack by Nebula Forms, version 1.0.
https://assetstore.unity.com/packages/3d/props/industrial/workshop-tools-free-low-poly-asset-pack-326488
License: Standard Unity Asset Store EULA.
Uses the original model, textures, and URP shader with unique asset GUIDs.

## Dry powder visual effect

`DryPowder` emits along its own local +Z axis. Its manually adjusted position and rotation are preserved. `PressureJet` is the narrow fast stream; `PowderCloud` slows down, expands, and drifts. Both are built-in Particle Systems using world-space simulation, velocity limiting/damping, noise, size growth, and smooth alpha fading. Collision is disabled; no runtime control script is included.

The effect loops automatically (`Play On Awake`). To preview, select either particle child in Prefab Mode and use the Particle System playback controls. Move or rotate the prefab to see the world-space trail. Turn off Play On Awake on both systems if automatic spraying is unwanted.

The pressure jet lifetime is 0.06-0.09 seconds (1.5 times the previous setting). The cloud grows gradually over 0.5-0.65 seconds and keeps moving as it fades, creating an elongated cone instead of collecting into a round puff. Particle travel is tuned through the modules, not a hard clipping boundary; existing smoke stays in world space when the extinguisher moves. Changing lifetime, speed, noise, size, or scale changes the effective range.

`DryPowder.mat` uses the built-in URP Particles/Unlit shader with Alpha blending, no emission, and Soft Particles. `PowderSoft.png` reuses the project's XR sample particle texture with import channel swizzling for white RGB and a soft alpha mask. Both Performance and Quality URP Config have Depth Texture enabled with approval. Cameras must use the pipeline depth setting (or explicitly request depth); opaque ground surfaces supply the depth used for intersection fading.

## Fire detection setup

PowderCloud has Trigger enabled: Enter and Inside = Callback, Outside and Exit = Ignore, Collider Query Mode = All, Radius Scale = 1. PressureJet stays visual-only and the Collision module remains disabled.

The Trigger collider list is intentionally empty. On a scene instance, add each fire source Collider to PowderCloud > Trigger > Colliders; the prefab asset cannot reference scene objects. No runtime script or extinguishing/deactivation behavior is included. Future code should read OnParticleTrigger on PowderCloud and use the Inside particles to track sustained exposure; a normal OnTriggerEnter on the fire source does not receive these particle callbacks.

## Prefab orientation

The outer `FireExtinguisher` is an identity Transform. Its `Body` child is rotated +90 degrees around Y, so `Body/DryPowder` emits along the outer +Z axis in the saved pose. The Rigidbody and XR Grab Interactable are on the outer root, so grabbing and docking move the entire prefab. Colliders, GrabAttach, and particles remain under Body. SocketAttach accounts for the Body rotation and GrabAttach offset. The bottle uses a capsule and the handle uses a box to avoid the old convex hull filling the hose gap and intersecting the holder.
