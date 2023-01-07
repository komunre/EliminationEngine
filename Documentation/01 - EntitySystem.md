# EntitySystem documentation

To create new system, inherit your class from `EliminationEngine.GameObject.EntitySystem`.

Override `public void` methods `OnLoad`, `PostLoad`, `OnUpdate` and `OnDraw` to "link" to according events of an engine.

`EntitySystem` has `Engine` variable, that contains instance of an engine, `Elimination` object.

`Elimination` instance has useful methods, such as:
* GetSystem - get instance of another system.
* TryGetSystem - try to get instance of another system.
* GetObjectsOfType - gets objects with certain component type.
* LobckCursor - locks the cursor.
* UnlockCursor - unlocks the cursor.
* GetCursorLockState - returns the locked/unlocked state of the cursosr.

`Elimination` instance has useful variables, such as;
* DeltaTime - time in seconds since last frame.
* Elapsed - total engine running time.
* KeyState - state of keyboard. Can be used to receive user input.
* MouseState - state of mouse. Can be used to receive user input.
* OnObjectCreate - event function that is called when object is added into the world.
* ProgramArgs - console arguments.
* Headless - headless mode.