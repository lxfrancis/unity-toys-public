// Unity math and vector utility methods
// by Lexa Francis, 2014-2017

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PossibleMultipleEnumeration

namespace Lx {

    public readonly struct RingInt: IEquatable< RingInt > {

        public readonly int Size;

        readonly int _value;

        public RingInt( int size, int value = 0 ) {

            if (size < 1) { throw new ArgumentException( "RingInt size must be positive" ); }

            Size   = size;
            _value = value.Ring( Size );
        }

        public static implicit operator int( RingInt   ringInt ) => ringInt._value;
        public static implicit operator float( RingInt ringInt ) => ringInt._value;

        public static RingInt operator +( RingInt ring, int other ) => new RingInt( ring.Size, ring._value + other );
        public static RingInt operator -( RingInt ring, int other ) => new RingInt( ring.Size, ring._value - other );

        public bool Equals( RingInt other ) => _value == other._value;

        public override bool Equals( object obj )
            => obj is RingInt other && _value == other._value
               || obj is IConvertible number && number.ToInt32( CultureInfo.InvariantCulture ) == _value;

        public override int GetHashCode() {
            unchecked {
                return (Size * 397) ^ _value;
            }
        }
    }

    public static partial class Utils {

        public const float Tau = 2f * PI;

        public static Vector3 RotateAround( this Vector3 v, Vector3 pivot, Vector3 angles )
            => Quaternion.Euler( angles ) * (v - pivot) + pivot;

        public static Vector2 xy( this Vector3 v ) => new Vector2( v.x, v.y );
        public static Vector2 xz( this Vector3 v ) => new Vector2( v.x, v.z );
        public static Vector2 yz( this Vector3 v ) => new Vector2( v.y, v.z );

        public static Vector3 xPart( this Vector3 v ) => Vector3.right   * v.x;
        public static Vector3 yPart( this Vector3 v ) => Vector3.up      * v.y;
        public static Vector3 zPart( this Vector3 v ) => Vector3.forward * v.z;

        public static Vector3 ZeroX( this Vector3 v ) => new Vector3( 0.0f, v.y,  v.z  );
        public static Vector3 ZeroY( this Vector3 v ) => new Vector3( v.x,  0.0f, v.z  );
        public static Vector3 ZeroZ( this Vector3 v ) => new Vector3( v.x,  v.y,  0.0f );

        public static Vector3 MagnitudeAdjusted( this Vector3 v, float adjustment )
            => v.normalized * (v.magnitude + adjustment);

        /*  Zoom function:
         *  - 0 input = base FOV
         *  - each +1 to input should result in halving the rect height of FOV
         *  - fov -> rect height:
         *     rect_height = 2.0f * Mathf.Tan( Mathf.Deg2Rad * fov * 0.5f )
         *  - rect height -> fov:
         *     fov = 2.0f * Mathf.Rad2Deg * Mathf.Atan( rect_height * 0.5f );
         *  - rect height -> zoom value: (if base rect height is 10)
         *     10    -> 0
         *      5    -> 1
         *      2.5  -> 2
         *      1.25 -> 3
         *     zoom_value = Mathf.Log( base_rect_height / rect_height, 2.0f )
         *  - zoom value -> rect height: (if base rect height is 10)
         *     0 -> 10
         *     1 ->  5
         *     2 ->  2.5
         *     3 ->  1.25
         *     rect_height = base_rect_height / Mathf.Pow( 2.0f, zoom_value )
         */
        public static float FOVToZoomValue( float fov ) => Log( 10 / (2.0f * Tan( Deg2Rad * fov * 0.5f )), 2.0f );

        public static float ZoomValueToFOV( float zoomValue )
            => 2.0f * Rad2Deg * Atan( 10 / Mathf.Pow( 2.0f, zoomValue ) * 0.5f );

        public static float LerpPerceptualZoom( float fromFOV, float toFOV, float t )
            => ZoomValueToFOV( Lerp( FOVToZoomValue( fromFOV ), FOVToZoomValue( toFOV ), t ) );

        public static int LogNearest( float value, List< float > vals ) {

            int   closestInd      = 0;
            float closestDistance = Infinity;
            float logValue        = Log( value, 2.0f );

            for (int i = 0; i < vals.Count; i++) {

                float distance = Abs( Log( vals[ i ], 2.0f ) - logValue );

                if (!(distance < closestDistance)) { continue; }

                closestInd      = i;
                closestDistance = distance;
            }
            return closestInd;
        }

        public static float FlatToLinearRamp( float value, bool reverse = false, float mix = 1.0f )
            => value.Pow( reverse ? 2.0f : 0.5f ) * mix + value * (1.0f - mix);

        public static string PreciseVectorString( Vector2 vector )
            => "(" + vector.x.ToString( "0.000" ) + ", " + vector.y.ToString( "0.000" ) + ")";

        public static Vector2 ApplyDeadZone( Vector2 input, float deadZone )
            => input.sqrMagnitude > 1.0f ? input
                   : (input - Vector2.ClampMagnitude( input, deadZone )) / (1.0f - deadZone);

        public static float ApplyDeadZone( float input, float deadZone )
            => input > 1.0f ? input : (input - Clamp( input, -deadZone, deadZone )) / (1.0f - deadZone);

        public static int FractionalChoiceProbability( int choices, float min, float max )
            => Clamp( (int) (Random.Range( min, max ) * choices), 0, choices - 1 );

        public static bool InRange( int value, int min, int max ) => value >= min && value < max;

        public static uint RotateLeft( this uint value, int count ) => (value << count) | (value >> (32 - count));

        public static int DiceRoll( int numDice, int dieMax, int baseResult = 0 )
            => baseResult + Enumerable.Range( 0, numDice - 1 ).Sum( _ => Random.Range( 1, dieMax + 1 ) );

        public static float Cycle( float period, bool centerOnZero ) => Cycle( period, 0.0f, 0.0f, centerOnZero );

        public static float Cycle( float period, float startTime=0.0f, float phase=0.0f, bool centerOnZero=false ) {

            float raw = Sin( (Time.time - startTime) * 2.0f * PI / period + phase * Deg2Rad );
            return centerOnZero ? raw : raw * 0.5f + 0.5f;
        }

        /// <summary>
        /// Parametric equation for an ellipse centred on the origin rotated by an arbitrary angle
        /// </summary>
        /// <param name="radius">Major and minor radius, in units</param>
        /// <param name="theta">Ellipse rotation, in radians</param>
        /// <param name="t">From zero to 2pi</param>
        /// <returns>Point coordinates</returns>
        public static Vector2 RotatedEllipse( Vector2 radius, float theta, float t )
            => new Vector2( radius.x * Cos( t ) * Cos( theta ) - radius.y * Sin( t ) * Sin( theta ),
                            radius.x * Cos( t ) * Sin( theta ) - radius.y * Sin( t ) * Cos( theta ) );

        public static float SignedProjectionMagnitute( Vector3 vector, Vector3 onNormal,
                                                       bool    proportionalToNormal = false )
            => Vector3.Project( vector, onNormal ).magnitude * Sign( Vector3.Dot( vector, onNormal ) )
               / (proportionalToNormal ? onNormal.magnitude : 1.0f);

        public static float Sqrt( this float f ) => Mathf.Sqrt( f );

        public static float Pow( this float f, float p ) => Mathf.Pow( f, p );

        public static float IntPow( float x, int pow ) {

            float result = x;
            for (int i = 0; i < pow; i++) { result *= pow; }
            return result;
        }

        public static int Ring( this int n, int size ) {

            while (n < 0) { n     += size; }
            while (n >= size) { n -= size; }
            return n;
        }

        public static float Ring( this float n, float size ) {

            while (n < 0) { n     += size; }
            while (n >= size) { n -= size; }
            return n;
        }

        // Should only be used on sets of similar quaternions
        public static Quaternion Average( this IEnumerable< Quaternion > quats ) {

            int        n   = 0;
            Quaternion avg = Quaternion.identity;

            foreach (Quaternion q in quats) {

                avg = Quaternion.Slerp( avg, q, 1.0f / n );
                n++;
            }
            return avg;
        }

        public static float Smoothstep( float x ) => x * x * (3f - 2f * x);
        
        public static float Smootherstep( float x ) => x * x * x * (x * (x * 6f - 15f) + 10f);

        public static float SeamlessRamp   ( float x ) => x - Sin( Tau * x ) / Tau;

        public static float SeamlessEaseIn ( float x ) => x - Sin( PI  * x ) / PI;

        public static float SeamlessEaseOut( float x ) => x + Sin( PI  * x ) / PI;

        public static float SignedToUnsignedUnitRange( float x ) => (x + 1f) * .5f;

        public static float UnsignedToSignedUnitRange( float x ) => x * 2f - 1f;

        public static float SinRamp( float x ) => .5f + .5f * Sin( (x - .5f) * PI );

        public static float SinEaseIn( float x ) => 1f + Sin( (x - 1f) * PI * .5f );

        public static float SinEaseOut( float x ) => Sin( x * PI * .5f );

        public static float ProportionalVariationRange( float x ) => -(2f * x / (x - 2f));

        public static float ReversibleClamp( float value, float bound1, float bound2 )
            => Clamp( value, Min( bound1, bound2 ), Max( bound1, bound2 ) );

        public static float ValueMap( float input, float inMin, float inMax, float outMin, float outMax ) {

            float normalisedInput = (input - inMin) / (inMax - inMin);
            float result          = normalisedInput * (outMax - outMin) + outMin;

            return result;
        }

        public static float SignedPow( float x, float pow ) => Sign( x ) * Mathf.Pow( Abs( x ), pow );

        public static float Median( this IEnumerable< float > vals ) {

            int n = vals.Count();
            if (n < 1) { throw new ArgumentException( "Collection is empty" ); }
            List< float > sorted = new List< float >( vals );
            sorted.Sort();
            return n % 2 == 1 ? sorted[ n / 2 ] : (sorted[ n / 2 - 1 ] + sorted[ n / 2 ]) / 2.0f;
        }

        public static Vector3 Sum( this IEnumerable< Vector3 > vecs )
            => vecs.Aggregate( Vector3.zero, ( current, vec ) => current + vec );

        public static Vector2 Sum( this IEnumerable< Vector2 > vecs )
            => vecs.Aggregate( Vector2.zero, ( current, vec ) => current + vec );

        public static Vector3 Average( this IEnumerable< Vector3 > vecs )
            => !vecs.Any() ? throw new ArgumentException( "Collection is empty" ) : vecs.Sum() / vecs.Count();

        public static Vector2 Average( this IEnumerable< Vector2 > vecs )
            => !vecs.Any() ? throw new ArgumentException( "Collection is empty" ) : vecs.Sum() / vecs.Count();

        public static string MatrixToString< T >( this IList< IList< T > > matrix )
            => string.Join( "\n", matrix.Select( vec => string.Join( ", ", vec.Select( v => v.ToString() ).ToArray() ) )
                                        .ToArray() );

        public static T[][] Transpose< T >( this IList< IList< T > > matrix ) =>
            Enumerable.Range( 0, matrix.First().Count() )
                      .Select( i => matrix.Select( v => v[ i ] ).ToArray() )
                      .ToArray();

        // Vector median generalised to lists of floats, getting the median of corresponding entries. Filthy.
        public static IEnumerable< float > Median( this IEnumerable< IList< float > > matrix )
            => Enumerable.Range( 0, matrix.First().Count() ).Select( i => matrix.Select( v => v[ i ] ).Median() );

        public static Vector3 Median( this IEnumerable< Vector3 > vecs ) {

            Vector3 vec = Vector3.zero;
            for (int i = 0; i < 3; i++) { vec[ i ] = vecs.Select( v => v[ i ] ).Median(); }
            return vec;
        }

        public static Vector3 Center( this IEnumerable< Vector3 > vecs ) {

            Vector3 vec = Vector3.zero;

            for (int i = 0; i < 3; i++) {
                vec[ i ] = (vecs.Select( v => v[ i ] ).Min()
                            + vecs.Select( v => v[ i ] ).Max()) * 0.5f;
            }

            return vec;
        }

        public static float AbsoluteSum( this IEnumerable< float > vals ) => vals.Select( Abs ).Sum();

        public static Vector3 RandomVector3 => new Vector3( Random.value, Random.value,
                                                            Random.value ) * 2.0f - Vector3.one;

        public static Vector2 RandomVector2 => new Vector2( Random.value, Random.value ) * 2.0f - Vector2.one;

        public static Vector3 RayIntersect( this Plane plane, Vector3 point, Vector3 normal ) {

            Ray ray = new Ray( point + normal * 1000.0f, -normal );
            plane.Raycast( ray, out var distance );
            return ray.GetPoint( distance );
        }

        public static Vector3 RotateTowardsOvershoot( Vector3 from, Vector3 to, float angle )
            => Vector3.Angle( @from, to ) < angle ? Vector3.RotateTowards( @from, to, angle,          Infinity )
                   : Vector3.RotateTowards( -@from,                               to, 180.0f - angle, Infinity );

        public static Vector2 Rotate90( this Vector2 v ) => new Vector2( v.y, -v.x );

        public static AnimationCurve HistogramToEqualisationCurve( IEnumerable< int > histogram ) {

            int[] cumulative = new int[ histogram.Count() + 1 ];
            float total      = histogram.Sum();

            histogram.Select( ( next, ind ) => cumulative[ ind + 1 ] = cumulative[ ind ] + next );

            // I regret nothing.
            return new AnimationCurve( cumulative.Select( ( val, ind ) => new[] {
                ind / (cumulative.Length - 1.0f), val / total,
                (cumulative[ ind + (ind < cumulative.Length - 1 ? 1 : 0) ] - val) / (total / (cumulative.Length - 1)),
                0.0f
            } ).Select( vals => new Keyframe( vals[ 0 ], vals[ 1 ], vals[ 2 ], vals[ 2 ] ) ).ToArray() );
        }

        // TODO: why does this calculate and return a vec3
        public static Vector3 Barycentric( Vector2 a, Vector2 b, Vector2 c, Vector2 point ) {

            Vector2 v0       = b - a, v1 = c - a, v2 = point - a;
            float   d00      = Vector3.Dot( v0, v0 );
            float   d01      = Vector3.Dot( v0, v1 );
            float   d11      = Vector3.Dot( v1, v1 );
            float   d20      = Vector3.Dot( v2, v0 );
            float   d21      = Vector3.Dot( v2, v1 );
            float   invDenom = 1.0f / (d00 * d11 - d01 * d01);
            float   v        = (d11 * d20 - d01 * d21) * invDenom;
            float   w        = (d00 * d21 - d01 * d20) * invDenom;

            return new Vector3( 1.0f - v - w, v, w );
        }

        public static Vector3 Barycentric( Vector3 a, Vector3 b, Vector3 c, Vector3 point ) {

            Vector3 v0       = b - a, v1 = c - a, v2 = point - a;
            float   d00      = Vector3.Dot( v0, v0 );
            float   d01      = Vector3.Dot( v0, v1 );
            float   d11      = Vector3.Dot( v1, v1 );
            float   d20      = Vector3.Dot( v2, v0 );
            float   d21      = Vector3.Dot( v2, v1 );
            float   invDenom = 1.0f / (d00 * d11 - d01 * d01);
            float   v        = (d11 * d20 - d01 * d21) * invDenom;
            float   w        = (d00 * d21 - d01 * d20) * invDenom;

            return new Vector3( 1.0f - v - w, v, w );
        }

        public static Vector3 ApplyFunction( this Vector3 vec, Func< float, float > function )
            => new Vector3( function( vec.x ), function( vec.y ), function( vec.z ) );

        public static Vector3 WithSumOne( this Vector3 v ) => v / (v.x + v.y + v.z);

        public static Vector3 WeightedAverage( this IEnumerable< Vector3 > vecs, IEnumerable< float > weights ) {

            float   weightTotal = 0.0f;
            Vector3 sum         = Vector3.zero;

            for (int i = 0; i < vecs.Count(); i++) {

                float weight = weights.ElementAt( i );
                sum         += vecs.ElementAt( i ) * weight;
                weightTotal += weight;
            }
            return sum / weightTotal;
        }

        public static float WeightedAverage( this IEnumerable< float > values, IEnumerable< float > weights ) {

            float weightTotal = 0.0f;
            float sum         = 0.0f;

            for (int i = 0; i < values.Count(); i++) {

                float weight = weights.ElementAt( i );
                sum         += values.ElementAt( i ) * weight;
                weightTotal += weight;
            }
            return sum / weightTotal;
        }
    }
}
