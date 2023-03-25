## Rational type ##

(Not yet properly documented!)

This is a C# struct that represents rational numbers with numerators and denominators in the range of a 32-bit signed int. In practice it's an immutable struct but it does not have the readonly modifier in order to allow for Unity serialisation.

Features:
- Unity serialisation and custom property drawer that supports entering fractions, integers, decimals, etc.
- Automatic simplification to smallest common factor
- Proper handling of negatives, infinity, NaN, etc in line with behaviour of built-in types and conversions thereof
- IEquatable< Rational >, IComparable, IComparable< Rational >, and IConvertible implementations
- Implicit conversions from ints
- Explicit converstions to and from other numeric types (single, double, long, etc)
- Automatic finding of nearest matching fractions to floating point values (for casting) up to numerators and denominators of magnitude one million
- Random properties for full range and 0-1
- All expected arithmetic, comparison and equality operators with both Rational and other numeric types
- Nice ToString() which simplifies to integers and other special values where possible
