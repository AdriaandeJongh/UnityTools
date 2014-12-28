UnityTools
==========

A collection of scripts that I've written for my silly prototypes, including:

* `Inp` to handle touch and mouse input the exact same way. Use the `Inp.ut` singleton to access the stuff, and make sure to put the `Inp` class before anything else in Edit > Project Settings > Script Execution Order.
* `ColliderToMesh` to add a mesh to Unity’s 2D polygon collision so that you can use Unity’s collider editor to quickly ‘draw’ a mesh.
* `DynamicPositionAnimation` for when you want to animate the position of something through code rather than the set values that Unity’s Mechanim uses.
* `AccelerationTrigger` to receive a function call when the acceleration of the device is larger than X. My script calls the `OnAccelTrigger` function on a script attached to the same GameObject.
* `AcceleratorInput` to get the rough acceleration over an X amount of frames. Note that this uses `FixedUpdate` and works with the `UnityEngine.Input.acceleration` variable, which doesn’t give you as much precision as `UnityEngine.Input.accelerationEvents`.