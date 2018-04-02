// Unity math and vector utility methods
// by Lexa Francis, 2014-2017

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lx {

   public static partial class Utils {
      
      public static Vector3 RotateAround( this Vector3 v, Vector3 pivot, Vector3 angles ) {
         return Quaternion.Euler( angles ) * (v - pivot) + pivot;
      }

      public static Vector2 xy( this Vector3 v ) { return new Vector2( v.x, v.y ); }
      public static Vector2 xz( this Vector3 v ) { return new Vector2( v.x, v.z ); }
      public static Vector2 yz( this Vector3 v ) { return new Vector2( v.y, v.z ); }

      public static Vector3 xPart( this Vector3 v ) { return Vector3.right   * v.x; }
      public static Vector3 yPart( this Vector3 v ) { return Vector3.up      * v.y; }
      public static Vector3 zPart( this Vector3 v ) { return Vector3.forward * v.z; }

      public static Vector3 ZeroX( this Vector3 v ) { return new Vector3( 0.0f, v.y,  v.z  ); }
      public static Vector3 ZeroY( this Vector3 v ) { return new Vector3( v.x,  0.0f, v.z  ); }
      public static Vector3 ZeroZ( this Vector3 v ) { return new Vector3( v.x,  v.y,  0.0f ); }

      public static Vector3 MagnitudeAdjusted( this Vector3 v, float adjustment ) {
         return v.normalized * (v.magnitude + adjustment);
      }

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
      public static float FOVToZoomValue( float fov ) {

         float rectHeight = 2.0f * Mathf.Tan( Mathf.Deg2Rad * fov * 0.5f );
         return Mathf.Log( 10 / rectHeight, 2.0f );
      }

      public static float ZoomValueToFOV( float zoomValue ) {

         float rectHeight = 10 / Mathf.Pow( 2.0f, zoomValue );
         return 2.0f * Mathf.Rad2Deg * Mathf.Atan( rectHeight * 0.5f );
      }

      public static float LerpPerceptualZoom( float fromFOV, float toFOV, float t ) {

         return ZoomValueToFOV( Mathf.Lerp( FOVToZoomValue( fromFOV ), FOVToZoomValue( toFOV ), t ) );
      }

      public static int LogNearest( float value, List< float > vals ) {

         int   closestInd      = 0;
         float closestDistance = Mathf.Infinity;
         float logValue        = Mathf.Log( value, 2.0f );

         for (int i = 0; i < vals.Count; i++) {

            float distance = Mathf.Abs( Mathf.Log( vals[ i ], 2.0f ) - logValue );

            if (distance < closestDistance) {

               closestInd      = i;
               closestDistance = distance;
            }
         }
         return closestInd;
      }

      public static float FlatToLinearRamp( float value, bool reverse = false, float mix = 1.0f ) {

         return value.Pow( reverse ? 2.0f : 0.5f ) * mix + value * (1.0f - mix);
      }

      static string PreciseVectorString( Vector2 vector ) {

         return "(" + vector.x.ToString( "0.000" ) + ", " + vector.y.ToString( "0.000" ) + ")";
      }

      public static Vector2 ApplyDeadZone( Vector2 input, float deadZone ) {

         if (input.sqrMagnitude > 1.0f) { return input; }
         return (input - Vector2.ClampMagnitude( input, deadZone )) / (1.0f - deadZone);
      }

      public static float ApplyDeadZone( float input, float deadZone ) {

         if (input > 1.0f) { return input; }
         return (input - Mathf.Clamp( input, -deadZone, deadZone )) / (1.0f - deadZone);
      }

      public static int FractionalChoiceProbability( int choices, float min, float max ) {

         int val = Mathf.Clamp( (int) (UnityEngine.Random.Range( min, max ) * choices), 0, choices - 1 );
         return val;
      }

      public static bool InRange( int value, int min, int max ) {

         if (value >= min && value < max) { return true; }
         return false;
      }

      public static uint RotateLeft( this uint value, int count ) {
         return (value << count) | (value >> (32 - count));
      }

      public static int DiceRoll( int numDice, int dieMax, int baseResult = 0 ) {
         
         return baseResult + Enumerable.Range( 0, numDice - 1 ).Sum( n => UnityEngine.Random.Range( 1, dieMax + 1 ) );
      }

      public static float Cycle( float period, bool centerOnZero ) {
         return Cycle( period, 0.0f, 0.0f, centerOnZero );
      }

      public static float Cycle( float period, float startTime = 0.0f, float phase = 0.0f, bool centerOnZero = false ) {

         float raw = Mathf.Sin( ((Time.time - startTime) * 2.0f * Mathf.PI) / period + (phase * Mathf.Deg2Rad) );
         return centerOnZero ? raw : raw * 0.5f + 0.5f;
      }

      public static float SignedProjectionMagnitute( Vector3 vector, Vector3 onNormal, bool proportionalToNormal = false ) {

         return (Vector3.Project( vector, onNormal ).magnitude
                   * Mathf.Sign( Vector3.Dot( vector, onNormal ) ))
                 / (proportionalToNormal ? onNormal.magnitude : 1.0f);
      }

      public static float Sqrt( this float f ) { return Mathf.Sqrt( f ); }

      public static float Pow( this float f, float p ) { return Mathf.Pow( f, p ); }

      public static float IntPow( float x, int pow ) {

         float result = x;
         for (int i = 0; i < pow; i++) { result *= pow; }
         return result;
      }

      public static int Ring( this int n, int size, int change ) {

         n += change;
         while (n < 0) { n += size; }
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

      public static float SeamlessRamp( float x ) { return x - (0.5f / Mathf.PI) * Mathf.Sin( x * 2.0f * Mathf.PI ); }

      public static float SinRamp( float x ) { return 0.5f + 0.5f * Mathf.Sin( (x - 0.5f) * Mathf.PI ); }

      public static float ProportionalVariationRange( float x ) { return -((2.0f * x) / (x - 2.0f)); }

      public static float ReversibleClamp( float value, float bound1, float bound2 ) {
         return Mathf.Clamp( value, Mathf.Min( bound1, bound2 ), Mathf.Max( bound1, bound2 ) );
      }

      public static float ValueMap( float input, float inMin, float inMax, float outMin, float outMax ) {

         float normalisedInput = (input - inMin) / (inMax - inMin);
         float result          = normalisedInput * (outMax - outMin) + outMin;

         if (Input.GetKey( KeyCode.V )) {

            PrintVals( "input", input )
                 .Add( "inMin", inMin )
                 .Add( "inMax", inMax )
                 .Add( "outMin", outMin )
                 .Add( "outMax", outMax )
                 .Add( "result", result ).Log();
         }
         return result;
      }

      public static float SignedPow( float x, float pow ) { return Mathf.Sign( x ) * Mathf.Pow( Mathf.Abs( x ), pow ); }

      public static float Median( this IEnumerable< float > vals ) {

         int n = vals.Count();
         if (n < 1) { throw new ArgumentException( "Collection is empty" ); }
         List< float > sorted = new List< float >( vals );
         sorted.Sort();
         if (n % 2 == 1) { return sorted[ n / 2 ]; } else { return (sorted[ n / 2 - 1 ] + sorted[ n / 2 ]) / 2.0f; }
      }

      public static Vector3 Sum( this IEnumerable< Vector3 > vecs ) {

         Vector3 sum = Vector3.zero;
         foreach (Vector3 vec in vecs) { sum += vec; }
         return sum;
      }

      public static Vector2 Sum( this IEnumerable< Vector2 > vecs ) {

         Vector2 sum = Vector2.zero;
         foreach (Vector2 vec in vecs) { sum += vec; }
         return sum;
      }

      public static Vector3 Average( this IEnumerable< Vector3 > vecs ) {

         if (vecs.Count() < 1) { throw new ArgumentException( "Collection is empty" ); }
         return vecs.Sum() / vecs.Count();
      }

      public static Vector2 Average( this IEnumerable< Vector2 > vecs ) {

         if (vecs.Count() < 1) { throw new ArgumentException( "Collection is empty" ); }
         return vecs.Sum() / vecs.Count();
      }

      public static string MatrixToString< T >( this IList< IList< T > > matrix ) {

         return string.Join( "\n", matrix.Select( vec => string.Join( ", ", vec.Select( v => v.ToString() )
                                                                             .ToArray() ) )
                                         .ToArray() );
      }

      public static T[][] Transpose< T >( this IList< IList< T > > matrix ) {

         return Enumerable.Range( 0, matrix.First().Count() )
                          .Select( i => matrix.Select( v => v[ i ] )
                                              .ToArray() )
                          .ToArray();
      }

      // Vector median generalised to lists of floats, getting the median of corresponding entries. Filthy.
      public static IEnumerable< float > Median( this IEnumerable< IList< float > > matrix ) {

         return Enumerable.Range( 0, matrix.First().Count() )
                          .Select( i => matrix.Select( v => v[ i ] )
                                              .Median() );
      }

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

      public static float AbsoluteSum( this IEnumerable< float > vals ) {

         return vals.Select( v => Mathf.Abs( v ) ).Sum();
      }

      public static Vector3 RandomVector3 {
         get {
            return new Vector3( UnityEngine.Random.value,
                                UnityEngine.Random.value,
                                UnityEngine.Random.value ) * 2.0f - Vector3.one;
         }
      }

      public static Vector2 RandomVector2 {
         get {
            return new Vector2( UnityEngine.Random.value,
                                UnityEngine.Random.value ) * 2.0f - Vector2.one;
         }
      }
   
      public static Vector3 RayIntersect( this Plane plane, Vector3 point, Vector3 normal ) {
         
         Ray   ray = new Ray( point + normal * 1000.0f, -normal );
         float distance;
         plane.Raycast( ray, out distance ); return ray.GetPoint( distance );
      }
    
      public static Vector3 RotateTowardsOvershoot( Vector3 from, Vector3 to, float angle ) {

         if (Vector3.Angle( from, to ) < angle) { return Vector3.RotateTowards( from, to, angle, Mathf.Infinity ); }
         return Vector3.RotateTowards( -from, to, 180.0f - angle, Mathf.Infinity );
      }

      public static Vector2 Rotate90( this Vector2 v ) {

         return new Vector2( v.y, -v.x );
      }
      
      public static AnimationCurve HistogramToEqualisationCurve( IEnumerable< int > histogram ) {

         int[] cumulative = new int[ histogram.Count() + 1 ];
         float total      = histogram.Sum();

         histogram.Select( (next, ind) => cumulative[ ind + 1 ] = cumulative[ ind ] + next ).ToArray();

         // I regret nothing.
         return new AnimationCurve( cumulative.Select( (val, ind)
            => new[] { ind / (cumulative.Length - 1.0f), val / total,
                       (cumulative[ ind + (ind < cumulative.Length - 1 ? 1 : 0) ] - val)
                          / (total / (cumulative.Length - 1)), 0.0f } )
            .Select( vals => new Keyframe( vals[ 0 ], vals[ 1 ], vals[ 2 ], vals[ 2 ] ) )
            .ToArray() );
      }

      public static Vector3 Barycentric( Vector2 a, Vector2 b, Vector2 c, Vector2 point ) {
            
         Vector2 v0 = b - a, v1 = c - a, v2 = point - a;
         float   d00      = Vector3.Dot( v0, v0 );
         float   d01      = Vector3.Dot( v0, v1 );
         float   d11      = Vector3.Dot( v1, v1 );
         float   d20      = Vector3.Dot( v2, v0 );
         float   d21      = Vector3.Dot( v2, v1 );
         float   invDenom = 1.0f / (d00 * d11 - d01 * d01);
         float   v        = (d11 * d20 - d01 * d21) * invDenom;
         float   w        = (d00 * d21 - d01 * d20) * invDenom;
         float   u        = 1.0f - v - w;
         
         return new Vector3( u, v, w );
      }

      public static Vector3 Barycentric( Vector3 a, Vector3 b, Vector3 c, Vector3 point ) {
            
         Vector3 v0 = b - a, v1 = c - a, v2 = point - a;
         float   d00      = Vector3.Dot( v0, v0 );
         float   d01      = Vector3.Dot( v0, v1 );
         float   d11      = Vector3.Dot( v1, v1 );
         float   d20      = Vector3.Dot( v2, v0 );
         float   d21      = Vector3.Dot( v2, v1 );
         float   invDenom = 1.0f / (d00 * d11 - d01 * d01);
         float   v        = (d11 * d20 - d01 * d21) * invDenom;
         float   w        = (d00 * d21 - d01 * d20) * invDenom;
         float   u        = 1.0f - v - w;
         
         return new Vector3( u, v, w );
      }

      public static Vector3 ApplyFunction( this Vector3 vec, Func< float, float > function ) {

         return new Vector3( function( vec.x ), function( vec.y ), function( vec.z ) );
      }

      public static Vector3 WithSumOne( this Vector3 v ) {

         return v / (v.x + v.y + v.z);
      }

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
