// Structs for representing ranges of integers and floats
// by Lexa Francis, 2014-2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lx {

    [Serializable]
    public struct Range< T > where T: IComparable, IComparable< T >, IEquatable< T > {

        public T min, max;

        public Range( T val )        => (min, max)           = (val, val);
        public Range( T min, T max ) => (this.min, this.max) = (min, max);

        public          bool   Contains( T value ) => value.CompareTo( min ) >= 0 && value.CompareTo( max ) <= 0;
        public override string ToString()          => $"Range< {typeof( T ).Name} >( {min} to {max} )";
        public          string ToShortString()     => $"{min} to {max}";

        public override bool Equals( object obj )
            => obj is Range< T > other && other.min.Equals( min ) && other.max.Equals( max );

        public bool Equals( Range< T > other ) => other.min.Equals( min ) && other.max.Equals( max );

        public override int GetHashCode() => unchecked( min.GetHashCode() + (31 + max.GetHashCode()) * 17 );
    }


    public static class RangeExtensions {

        public static IntRange LengthRange( this Array array ) => new IntRange( 0, array.Length - 1 );

        public static IntRange CountRange< T >( this IEnumerable< T > enumerable )
            => new IntRange( 0, enumerable.Count() - 1 );
    }


    [Serializable]
    public struct IntRange: IEquatable< IntRange >, IEnumerable< int > {

        // fields
        public int min, max;

        // constructors
        public IntRange( int val ) => (min, max) = (val, val);

        public IntRange( int min, int max ) => (this.min, this.max) = max >= min ? (min, max)
            : throw new ArgumentException( $"IntRange max ({max}) is less than min ({min})" );

        public IntRange( IList< int > values ) => (min, max) = (values.Min(), values.Max());

        // implicit tuple conversions
        public static implicit operator IntRange( (int min, int max) tuple ) => new IntRange( tuple.min, tuple.max );

        public static implicit operator (int min, int max)( IntRange range ) => (range.min, range.max);

        public static explicit operator FloatRange( IntRange intRange ) => new FloatRange( intRange.min, intRange.max );

        // useful properties
        public readonly int size => max - min + 1;

        public readonly int random => Random.Range( min, max + 1 );

        // methods
        public readonly bool Contains( int value ) => value >= min && value <= max;

        public readonly int Clamp( int num ) => num > max ? max : num < min ? min : num;

        // union of multiple ranges
        public static IntRange Union( IntRange a, IntRange b ) {

            if (a.max < b.min - 1 || b.max < a.min - 1) {

                Debug.LogWarning( "Union of two non-overlapping and non-adjacent int ranges " +
                                  "covers possibly unwanted values" );
            }

            return new IntRange( Mathf.Min( a.min, b.min ), Mathf.Max( a.max, b.max ) );
        }

        public static IntRange Union( params IntRange[] ranges ) =>
            new IntRange( ranges.Min( r => r.min ), ranges.Max( r => r.max ) );

        // parsing from string
        public static IntRange Parse( string str ) {

            try {
                if (str.Contains( '+' )) {
                    return new IntRange( int.Parse( str.Substring( 0, str.IndexOf( '+' ) ) ), int.MaxValue );
                }

                var tokens = str.Trim().ToLower().Split( new[] { "to" }, default );
                if (tokens.Length != 2) {
                    tokens = str.Split( new[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
                }
                return new IntRange( int.Parse( tokens[ 0 ] ), int.Parse( tokens[ 1 ] ) );
            }
            catch (Exception e) {
                throw new ArgumentException( $"Could not parse IntRange from string {str}", e );
            }
        }

        // enumeration
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator< int > GetEnumerator() {
            for (int i = min; i <= max; i++) { yield return i; }
        }

        // boilerplate
        public readonly override int GetHashCode() => unchecked( min + (31 + max) * 17 );

        public readonly override string ToString()      => $"IntRange( {min} to {max} )";
        public readonly          string ToShortString() => $"{min} to {max}";

        // equality
        public          bool Equals( IntRange other ) => other.min == min && other.max == max;
        public override bool Equals( object   obj )   => obj is IntRange other && other.min == min && other.max == max;

        public static bool operator ==( IntRange a, IntRange b ) => a.min == b.min && a.max == b.max;
        public static bool operator !=( IntRange a, IntRange b ) => a.min != b.min || a.max != b.max;
    }


    [Serializable]
    [SuppressMessage( "ReSharper", "CompareOfFloatsByEqualityOperator" )]
    public struct FloatRange: IEquatable< FloatRange > {

        public float min, max;

        public FloatRange( float val )            => (min, max)           = (val, val);
        public FloatRange( float min, float max ) => (this.min, this.max) = (min, max);

        public readonly float random => Random.Range( min, max );

        public readonly float Lerp( float t ) => min + t * (max - min);

        public readonly float Clamp( float t ) => t > max ? max : t < min ? min : t;

        public readonly bool Contains( float value ) => value >= min && value <= max;

        public static explicit operator IntRange( FloatRange intRange )
            => new IntRange( (int) intRange.min, (int) intRange.max );

        public static FloatRange Parse( string str ) {

            try {
                if (str.Contains( '+' )) {
                    return new FloatRange( float.Parse( str.Substring( 0, str.IndexOf( '+' ) ) ),
                                           float.PositiveInfinity );
                }

                var tokens = str.Trim().ToLower().Split( new[] { "to" }, default );
                return new FloatRange( float.Parse( tokens[ 0 ] ), float.Parse( tokens[ 1 ] ) );
            }
            catch (Exception e) {
                throw new ArgumentException( $"Could not parse FloatRange from string {str}", e );
            }
        }

        // boilerplate
        public readonly override string ToString() => $"FloatRange( {min} to {max} )";

        public readonly string ToShortString() => $"{min} to {max}";

        public static bool operator ==( FloatRange a, FloatRange b ) => a.min == b.min && a.max == b.max;

        public static bool operator !=( FloatRange a, FloatRange b ) => a.min != b.min || a.max != b.max;

        public readonly bool Equals( FloatRange other ) => other.min == min && other.max == max;

        public readonly override bool Equals( object obj )
            => obj is FloatRange other && other.min == min && other.max == max;

        public readonly override int GetHashCode() => min.GetHashCode() ^ (max.GetHashCode() << 3);
    }
}
