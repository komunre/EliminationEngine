# Camera

Camera is absolutely necessary for enging work. Due to lack of multiple null checks, the engine will or might crash if no camera exists on the scene.

# Lighthing

Lighting is absolutely necessary for any objects that use `textured.frag` shader.

Without any object with `LightComponent` component in a reach of object that is supposed to be visible, object will be completely invisible on a default black background.

# Physics

Currently physics initialize in `OnLoad`, long before game systems. That means init should be invoked manually after being changed.

Should be fixed by adding `PreInit` method to `EntitySystem` or moving init invokation.

# Engine Under The Hood Work and Optimizations

Currently, systems and objects are stored in C# standard library classes of `List` and `Dictionary`, which might be not very well optimized for such tasks. It probably will be preferred to manually allocate memory using `Marshal`
