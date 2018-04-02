## Unity utility code

A collection of static methods, extension methods and specialised classes I use across each of my projects.

The code is grouped into files by functionality rather than classes. All of it lives inside the `Lx` namespace, and many of the miscellaneous methods and extension methods live in a static class called `Utils`, split across several files.

#### Regarding code style

Some of this code is of dubious usefulness or performance, and was written while I was learning and experimenting with C#, seeing what was possible and making up for some of the limitations of C# 4. I prefer a functional programming style and make heavy use of LinQ extension methods while experimenting and beginning new projects, since I find this the most comfortable and expedient way of getting things done. As such, this utility code is geared towards ease-of-use and compactness in client code, rather than performance. In the later stages of a project, in places where performance matters, I usually alter the code to a more procedural, bare-bones style.

There are also probably better ways to achieve some of the functionality here; for example, I am not very well acquainted with attributes and much of Unity's editor API.

## The files

### CachedEvaluations.cs

This includes two generic classes, `ContingentEvaluation` and `PeriodicEvaluation`, that can be initialised with a function that yields a value of the given type, which is only re-evaluated either when a condition has changed or a certain period of time has elapsed since the last evaluation. Useful for expensive calculations that don't need to be updated very often.

### CardinalDir.cs

An enum for 2D cardinal directions (up, down, left, right) with supporting structs and extension methods, for converting to and from vectors, rotating, and so on. Useful in tile-based games.

### CollectionUtils.cs

Specialised collection classes, and miscellaneous collection extension methods.

#### Collection classes

* `LaxStringDict` - A dictionary with string keys that doesn't care about case or spaces in the keys.
* `CountTable` - A simple `<string, int>` dictionary for keeping a running tally of items, where items don't need to be explicitly added.
* `CappedQueue` - A queue with a fixed capacity that jettisons the oldest elements as new ones are added. Useful for situations like keeping a running average of some value.
* `ListSet` - A list that won't include duplicate items (or close-enough items, if a tolerace function is provided).
* `TimedList` - A sorted list, where elements are sorted based on the time they were added (according to Time.time), and covering only a fixed period of time into the past. Useful for situations like averaging values over the most recent block of time.
* `VariantRandomiser` - Spits out integers from `next` below a fixed value, and won't give the same value twice within some specified minimum number of calls. Useful for generating random sequences where you don't want repeated values close together.

#### Collection methods

* Several methods for destroying UnityEngine.Objects within collections
* `RemoveAll()` and `RemoveValue()` for removing items from SortedLists matching a given predicate or value
* `WeightedRandom()` returns an element from a collection where each element has an associated frequency (relative probability of occurring)
* `RandomSelection()` for a random subset of n items from a collection
* `MinBy()` and `MaxBy()` for returning the item that yields the lowest or highest result from a selector function
* Several methods for printing out lists of values for debug purposes
* A few other miscellaneous methods

### ConstantAreaFOV.cs

This component allows your cameras to cover (perceptually) the same 2D-area field of view at different aspect ratios, rather than having a fixed vertical FOV as usual. Set the base aspect ratio and vertical FOV (for example, 1.333f and 45 degrees) based on the aspect ratio for which you are testing/designing, and call UpdateFOV() at runtime to update the attached cameras' FOV based on the current actual screen aspect ratio.

### Coords.cs

Structs for 2D and 3D integer coordinates (`Coord2` and `Coord3`), and rectangular areas of 2D coordinates (`Coord2Range`), with conversions to and from vectors, and many other useful properties and manipulations. Useful in tile-based games or data structures, especially in conjunction with CardinalDir.

TODO: more comprehensive rotation methods for Coord3, and a Coord3Range struct.

### ElegantPair.cs

An implementation of the [elegant pairing function](http://szudzik.com/ElegantPairing.pdf). Supports negative values, unlike the canonical version of the function, so long as the same bounds are used when extracting values. Default value bounds (±23169) were chosen to avoid integer overflow in the intermediate calculations. I am not sure if this algorithm is useful for anything at all in Unity, but the elegance appeals to me.

### FlatMDArray.cs

A generic container that behaves like a multidimensional array but uses a flat array internally, allowing for use in situations where a flat array is required (for example, Unity's `Texture2D.SetPixels()`). There are subclasses for 2D and 3D cases (`Flat2DArray` and `Flat3DArray`) that provide width, height and depth convenience properties, and may also be faster when accessing elements although I have not tested this.

The 2D and 3D subclasses also add an `Indexer()` method, which takes a function and applies it to each valid set of coordinates within the array. This saves having to write nested loops for operating on each element, while still having access to the coordinates in your loop code (unlike a simple iteration over the flat backing array).

### GameUtils.cs

Unity-specific utility methods and extension methods.

* `RotationDamper` - give an instance of this class a transform and feed it target rotations each frame, and it will set the transform's rotation to the average of the most recent rotations.
* `TransformData` - simple struct for holding position, rotation and scale if you want to reset transforms to prior values, for example. Can be initialised from a transform, and set a target transform's data.
* `DrawDebugGraph()` - draws simple graphs using Debug.DrawLine in the scene view, based on provided int or float values.
* `SetResourcesDirectories()` and `FindResource()` - use the former in editor code to get a list of all subdirectories in your project's resources, and pass that list to the latter at runtime to find any resource by filename alone, regardless of path.
* `AddEventCallback()` - for adding Unity event handlers to gameobjects from code. Adds an EventTrigger component if there isn't one, and a corresponding EventTrigger.Entry to that component if necessary.
* `RendererBounds()` - returns a Bounds that covers all the child renderers of the given transform.
* `TimeLog()` - simple but useful! Call this around expensive calculations to print out a debug line reporting how long since the last call. Returns the current time, which you can pass in to the next call.
* `DoWhenTrue()` - pass in a function that checks for a condition, and an action to perform when the condition is true.
* `TrueAfterSeconds()` - returns a function which returns true after the given period of time has elapsed.
* `FindComponent()` - find a child component (at any depth) of a given type with the given name.
* `HighestAncestorComponent()` - finds the highest ancestor of a given type.
* `SetAlpha()` and `SetColorExceptAlpha()` - change the alpha, or non-alpha color, of a Graphic or Material.
* Convenience methods for converting Color and Color32 arrays to textures, and saving textures to PNGs, since this is something I find myself doing a lot in proc gen work.
* Several more miscellaneous little things

### HilbertCurve.cs

For translating an index to coordinates, and vice versa, on a [Hilbert curve](https://en.wikipedia.org/wiki/Hilbert_curve) of any number of dimensions. Used by the RGB Image toy.

TODO: implementing a modified version of the curve that forms a loop.

### MathUtils.cs

For dealing with numbers and vectors, including:

* averaging quaternions together (as long as they aren't _too_ different)
* converting between FOV and a perceptual zoom level
* finding nearest value to a given value in a set of values, according to the base-2 logarithmic difference rather than absolute difference
* applying a dead zone to input vectors
* simple sine-wave cycles with various parameters
* the signed magnitude of a vector projection
* ring modulus clamped between 0 and a maximum (good for wrapping index values within range)
* a low-jolt 'seamless ramp', similar to a sine-wave ramp between 0 and 1 but with no discontinuities in the first derivative - really nice for animations
* ValueMap and ColorMap which combine a lerp and inverse lerp in one, for floats and colours
* finding the sum, average, and axis-dependent median and center of collections of vectors
* messing around with matrices

### MiscUtils.cs

Non-Unity-specific stuff. I find `SwitchMap` particularly useful. This adds an extension method `Map()` to, well, _everything_, allowing you to select a value switching on some other value. Like a compact switch statement for assignments, including implicit conversion. It looks like this:

    int    foo = 2;
    string bar = foo.Map( 1, "one"   )
                    .Map( 2, "two"   )
                    .Map( 3, "three" );
    // bar is now "two"

### NavigationUtils.cs

For dealing with setting up navigation between Unity UI selectables. For control I usually use explicit navigation rather than automatic, so I wrote these to deal with UI layouts that are generated at runtime.

* `SetNavigationSequence()` takes either a parent transform or collection of selectables, and will set up horizontal or vertical navigation between them.
* `JoinNavigationSequences()` takes two horizontal or two vertical navigation sequences (which should be visually adjacent but not necessarily with the same number of elements) and sets up navigation between them, so that the first and last elements line up.
* `SetNavigationGrid()` sets up navigation between elements in a grid, for example if GridLayoutGroup is used.
* `SetNavigation()` and `SetNavigationMode()` set properties on the Navigation struct of a Selectable.
* `SetFreeformNavigation()` Takes a dictionary of Selectables and RectTransforms and sets up navigation between the selectables based on the RectTransform positions (these are separated in case the selectables are children of the transforms whose position we need). Useful for navigating between elements anchored in world space - for example, on a map.
* `TargetForMoveDir()` returns the selectable to which navigation is set for a given direction.

### Partial3DArray.cs

This behaves like an infinitely-sized 3D array, but in fact only covers (with corresponding memory usage) a small movable window over the 3D coordinate space. For accesses to elements that are outside the window, it can either behave like a tiling space (cyclic), or throw an exception. Useful for 3D tile-based worlds that are generated on the fly as you move around.

### Ranges.cs

Simple structs for representing ranges of ints or floats.

### RectUtils.cs

For dealing with rects and RectTransforms.

* `SplitRectHorizontal()` and `SplitRectLabels()` are handy for laying out property drawers in the inspector.
* `RectInCanvasSpace()` and `CornersInCanvasSpace()` can help with aligning UI elements to other elements that are elsewhere in the UI hierarchy.
* `Overlap()` returns true if two RectTransforms under the same canvas overlap each other, regardless of hierarchy relationship.
* `WorldToCanvasSpace()` is good for aligning UI elements in a screen-space canvas to objects in world space.
* `DistanceFromRectEdge()` works like a signed distance field for RectTransforms.
* `VerticalScrollDeltaToCoverElement()` returns the normalized vertical scroll delta necessary for a ScrollRect's viewport to cover a given child RectTransform. Useful for auto-scrolling to items in a scrolling list, and for making the viewport move to cover items as the user navigates up and down.

### SerializedNullable.cs

A generic class that holds a struct value and flag, convertible to and from T and Nullable< T >. Concrete subclasses can be serialized by Unity, and the custom property drawer will show the normal property drawer for T alongside a checkbox for HasValue. Includes concrete subclasses for a few basic types. Other structs in the utility library also have their own SerializedNullable variants.

### SphereLUT.cs

Includes two methods, `CircleLUT` and `SphereLUT`, that create linear lookup tables of Coord2s and Coord3s respectively. The coordinates expand outwards in concentric rings or shells. Useful for procedural content and other algorithms that involve searching outward in an indexed space. Used by the Worley noise and RGB Image toys.

### StateHighlight.cs

A somewhat cumbersome tool that I've included here for completeness. Can be used to set up highlight schemes for UI elements that change based on some state (usually an enum value), including changing RectTransform scale, graphic color and opacity etc, with AnimationCurves for state transitions and optional cycling/pulsating of values. You need to create concrete subclasses based on the state value type in order for Unity to serialize instances. If writing a similar system today, I would almost certainly make it component-based instead.

### StringUtils.cs

Methods and extension methods for dealing with strings, including parsing numbers, adding commas and articles, adding Bold and Color tags to rich text, stripping style tags, and splitting delimited strings to new objects based on a provided function.

### Timer.cs

A general-purpose timer class. Can be configured to run on scaled or unscaled time. All timers are held in a WeakReference collection for automatic updating whenever any timer is accessed. I used to use this class extensively for timed gameplay elements (performing actions in `Update()` depending on the state of various timers), and it seems quite robust and clean, but over time I've shifted to using coroutines for most things.

### Editor/EditorUtils.cs

Convenience methods for drawing simple property values and range properties (i.e. that have a minimum and maximum value), and for accessing assets of a given type matching a given name regardless of path.