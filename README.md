UnityTools
==========

A collection of scripts that continue to be useful to me, including:

* `CanvasHelper` will resize a child RectTransform named "SafeArea" to fit `Screen.safeArea`. You may also register events to `onOrientationChange` and `onResolutionChange`, as Unity itself doesn't have a way for you register to those events. This component should be attached to a Canvas GameObject in every scene in your game.
* `Inp` handles touch and mouse input the exact same way. Just add the file to your project (no need to add it to any gameobject in a scene) and use the `Inp.ut` singleton to access the stuff. Make sure the `Inp` class is executed before anything else by putting it at the top of the script execution order list: Edit > Project Settings > Script Execution Order.
* `ColliderToMesh` adds a mesh to Unity’s 2D polygon collision so that you can use Unity’s collider editor to quickly ‘draw’ a mesh.
* `DynamicPositionAnimation` is a component for an Animator to be able to interpolate between two positions that are set through code (rather than set in the Animator itself).
* `AccelerationInput` can be used to get the rough acceleration over X amount of frames, or to subscribe to an acceleration trigger. Use `AccelerationInput.Sample(sampleFrameAmount)` to sample a bunch of frames, or `AccelerationInput.SetTrigger(YourFunction, triggerForce)` to subscribe a trigger event when there is enough force. Note that my class uses `FixedUpdate()` and works with the `UnityEngine.Input.acceleration` variable, which doesn’t give you as much precision as `UnityEngine.Input.accelerationEvents`.
* `[HideWhenFalse]` and `[HideWhenTrue]` attributes hide public variables depending on the state of a bool in the same script. Usage: `public bool showSpeed = false; [HideWhenFalse("showSpeed")] public float speed = 2f;`
* `[InspectorButton]` attribute can be added to functions in your component and will show up as a button in the inspector, which on being pressed will run in the editor regardless of whether the game is running.
