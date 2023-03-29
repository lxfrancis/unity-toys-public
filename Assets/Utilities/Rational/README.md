## Rational type ##

(Not yet properly documented!)

This is a C# struct that represents rational numbers with numerators and denominators in the range of a 32-bit signed int. In practice it's an immutable struct but it does not have the readonly modifier in order to allow for Unity serialisation.

Should be considered early/experimental; updates to the implementation may not be backwards-compatible with previously stored values.

Requires C# 9.0 (Available in Unity 2021.2 and up; Unity is *not* required for the struct itself)


### Updates ###

- 2023-03-28: Fixed edge case error in equality logic with infinite values where the internal values of the struct were non-standard (e.g. through bit fiddling or deserialisation)


### Features ###

- Unity serialisation and custom property drawer that supports entering fractions, integers, decimals, and special values.
- Automatic simplification to smallest representation (by greatest common factor)
- Proper handling of negatives, infinity, NaN, etc in line with behaviour of built-in types and conversions thereof
- `IEquatable< Rational >`, `IComparable`, `IComparable< Rational >`, and `IConvertible` implementations
- Implicit conversions from ints
- Explicit converstions to and from other numeric types (single, double, long, etc)
- Automatic finding of nearest matching fractions to floating point values (for casting) up to numerators and denominators of magnitude one million
- Random value properties for full range and 0-1
- All expected arithmetic, comparison and equality operators with both Rational and other numeric types
- Extension method on ints for slightly nicer / more readable construction
- String parsing to Rational that allows all parseable int and float values as well as directly specified fractions (e.g. `"7/9"`)
- Nice `ToString()` which simplifies to integers and other special values where possible


Examples of constructing rational 3/5:

```cs
// conversion from int and overloaded arithmetic operator
Rational r1 = (Rational) 3 / 5; 

// constructor
Rational r2 = new Rational( 3, 5 );  // or new( 3, 5 );

// int extension method
Rational r3 = 3.Over( 5 );

// best fit conversion from floating point - can be considerably slower depending on value
Rational r4 = 0.6f; 
```

Not yet implemented:
- More robust overflow handling
- Various planned improvements to conversions from floating point values
- String parsing improvement - using the number of decimal places in the string as the basis of the required fraction fit accuracy
- Uninitialised value is currently equal to NaN, but it should be zero - this will change
- Documentation
- Long version (128-bit with 64-bit components)
- Typical math library functions (Pow, Abs, etc)
- Certain convenience properties (max and min value, etc)
- Support for c# 8
