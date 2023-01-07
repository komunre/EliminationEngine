# Resources and Models

It is recommended to store every game resource in `res/` folder. Optional `Shader` folder is also recommended.

To copy resource file upon compile in visual studio, right click on the file, go to properties, and set `Copy to Output Directory` to `Copy Always`.

Files can be accessed directly by filesystem methods and functions of C#.

To load GLTF model, use `EliminationEngine.ModelParser` class, `ParseGLTFExternal` method.

To add GLTF model to the object, in the `EliminationEngine.ModelHelper` class, use `AddGLTFMeshToObject` or `PostParseMeshes` methods.

To use shaders, use `Shader` class.

Use `EliminationEngine.Systems.SoundSystem` to play sounds and music. Dynamic sound generation is possible.