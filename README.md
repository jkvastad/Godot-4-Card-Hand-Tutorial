# Card Hand
A hand of cards, commonly seen in various card games. The cards can be grabbed with the mouse and dropped on top of other cards to take their place:

![Card Hand demonstration](https://github.com/jkvastad/Godot-4-Card-Hand-Tutorial/assets/9295196/c684527c-338c-4374-8566-2eafc4f5b84f)

The idea is to have a small simple project where cards can be dragged and dropped between hands to show some basic Godot 4 features. Open up the project, play around in the main scene, and then compare with the code to see how it works.

The features are mainly:

## Object Picking 
A.K.A. clicking on stuff with the mouse.
* [The last consumer of an input in the input pipeline](https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html#how-does-it-work) is an inbuilt raycast.
  * see [_input_event](https://docs.godotengine.org/en/stable/classes/class_collisionobject2d.html#class-collisionobject2d-method-input-event) on e.g. CollisionObject2D

## Node Basics - 3D, 2D or Control?
In this project I used Node2D based nodes.
* It turns out Control is pretty niche for main menus and similar, e.g. one problem is the input event [_gui_input](https://docs.godotengine.org/en/stable/classes/class_control.html#class-control-method-gui-input) for Control which does not trigger if the node is obscured by another Control. Thus it is impossible to do "raycast on my mouse click; how many cards are in the stack? Is there something behind the cards?" (in fact Control does not even have raycast support).

## [Raycasting](https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html)
A very useful operation with a few different implementations.
* In this tutorial [intersect_point](https://docs.godotengine.org/en/stable/classes/class_physicsdirectspacestate2d.html#class-physicsdirectspacestate2d-method-intersect-point) is used.
* For some ray casting basics I made [another example project](https://github.com/jkvastad/godot-4-raycast-event-vs-polling/tree/master)
  
* [There is a caveat to raycasting](https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html#accessing-space):
  
> Godot physics runs by default in the same thread as game logic, but may be set to run on a separate thread to work more efficiently. Due to this, the only time accessing space is safe is during the Node._physics_process() callback. Accessing it from outside this function may result in an error due to space being locked.
  
  The mentioned space is the physics space on which we raycast, thus we can only raycast reliably inside the [_physics_process](https://docs.godotengine.org/en/stable/classes/class_node.html#class-node-method-physics-process) method, forcing us to use [polling instead of events](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html#events-versus-polling). There are workarounds where we don't poll for everything all the time, which leads us to the next point of the project (if excessive polling is not your bottleneck it is probably better to just go with polling).

# [async/await](https://learn.microsoft.com/en-US/dotnet/csharp/asynchronous-programming/)
In C# we can create an asynchronous task, postpone the execution of an async method and await the result of the task.
* In this project we use async/await to ask _physics_process to do raycasting for us.
* Using async/await in a nice clean manner can be quite tricky. Knowledge of events, delegates, lambda functions, closures, Task and TaskCompletionSource can be required. The code for CardHand contains comments with the details.
