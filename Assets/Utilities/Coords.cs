// Structs for 2D and 3D integer coordinates, and 2D area coordinates
// by Lexa Francis, 2014-2017

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Lx {

   /// <summary>A two-dimensional integer position (x and y).</summary>
   [Serializable]
   public struct Coord2: IEquatable< Coord2 > {

      public int x, y;

      public Coord2( int x, int y ) {

         this.x = x;
         this.y = y;
      }

      public override bool Equals( object obj ) {

         if (obj is Coord2) {

            Coord2 coord = (Coord2) obj;
            return x == coord.x && y == coord.y;
         }
         return false;
      }

      public bool Equals( Coord2 coord ) { return x == coord.x && y == coord.y; }

      public static bool   operator ==( Coord2 a, Coord2 b ) { return a.x == b.x && a.y == b.y; }

      public static bool   operator !=( Coord2 a, Coord2 b ) { return !(a == b); }
      
      /// <summary>Add two Coord2s together.</summary>
      public static Coord2 operator  +( Coord2 a, Coord2 b ) { return new Coord2( a.x + b.x, a.y + b.y ); }
      
      /// <summary>Subtract one Coord2 from another.</summary>
      public static Coord2 operator  -( Coord2 a, Coord2 b ) { return new Coord2( a.x - b.x, a.y - b.y ); }
      
      /// <summary>Negate this Coord2.</summary>
      public static Coord2 operator  -( Coord2 coord ) { return new Coord2( -coord.x, -coord.y ); }
      
      /// <summary>Multiply a Coord2 by an integer scalar value.</summary>
      public static Coord2 operator  *( Coord2 coord, int scalar ) { return new Coord2( coord.x * scalar, coord.y * scalar ); }
      
      /// <summary>Multiply a Coord2 by a floating-point scalar value, resulting in a Vector2.</summary>
      public static Vector2 operator *( Coord2 coord, float scalar ) {

         return new Vector2( coord.x * scalar, coord.y * scalar );
      }
      
      /// <summary>Create a Coord2 by rounding a Vector2's components to the nearest integer.</summary>
       public static Coord2 Round( Vector2 vector ) {

           return new Coord2( Mathf.RoundToInt( vector.x ), Mathf.RoundToInt( vector.y ) );
       }
      
      /// <summary>Rotate the Coord2 around the origin by 90 degrees clockwise.</summary>
      public Coord2 rotatedClockwise   { get { return new Coord2( y, -x ); } }
      
      /// <summary>Sum of the absolute values of the X and Y coordinates.</summary>
      public int    manhattanMagnitude { get { return Mathf.Abs( x ) + Mathf.Abs( y ); } }
      
      /// <summary>Change a Coord2 that is relative to some direction into a global Coord2 relative to 'up'.</summary>
      public Coord2 RelativeToGlobal( CardinalDir facing ) {

         Coord2 rotated = this;
         for (int i = 0; i < (int) facing; i++) { rotated = rotated.rotatedClockwise; }
         return rotated;
      }
      
      /// <summary>Change a Coord2 that is relative to 'up' into a local Coord2 relative to the given direction.</summary>
      public Coord2 GlobalToRelative( CardinalDir facing ) {

         Coord2 rotated = this;
         for (int i = 4; i > (int) facing; i--) { rotated = rotated.rotatedClockwise; }
         return rotated;
      }
      
      /// <summary>Wrap the X and Y coordinates between zero and the given limits.</summary>
      public Coord2 Wrapped( Coord2 limit ) {

         return new Coord2( x.Ring( limit.x, 0 ), y.Ring( limit.y, 0 ) );
      }

      public override int GetHashCode() { return (x * 23) + (y * 17); }

      public override string ToString() { return "Coord( " + x + ", " + y + " )"; }
      
      /// <summary>Same as "new Coord2( 0, 0 )".</summary>
      public static Coord2 zero  { get { return new Coord2(  0,  0 ); } }
      /// <summary>Same as "new Coord2( 1, 1 )".</summary>
      public static Coord2 one   { get { return new Coord2(  1,  1 ); } }
      /// <summary>Same as "new Coord2( 0, 1 )".</summary>
      public static Coord2 up    { get { return new Coord2(  0,  1 ); } }
      /// <summary>Same as "new Coord2( 1, 0 )".</summary>
      public static Coord2 right { get { return new Coord2(  1,  0 ); } }
      /// <summary>Same as "new Coord2( 0, -1 )".</summary>
      public static Coord2 down  { get { return new Coord2(  0, -1 ); } }
      /// <summary>Same as "new Coord2( -1, 0 )".</summary>
      public static Coord2 left  { get { return new Coord2( -1,  0 ); } }

      public static readonly Coord2[] adjacentCardinal = {
         new Coord2( -1,  0 ),
         new Coord2(  1,  0 ),
         new Coord2(  0, -1 ),
         new Coord2(  0,  1 )
      };

      public static readonly Coord2[] adjacentDiagonal = {
         new Coord2( -1,  -1 ),
         new Coord2(  1,  -1 ),
         new Coord2( -1,   1 ),
         new Coord2(  1,   1 )
      };

      public static readonly Coord2[] adjacentSquare = {
         new Coord2( -1, -1 ),
         new Coord2(  0, -1 ),
         new Coord2(  1, -1 ),
         new Coord2( -1,  0 ),
         new Coord2(  1,  0 ),
         new Coord2( -1,  1 ),
         new Coord2(  0,  1 ),
         new Coord2(  1,  1 )
      };

      public static implicit operator Vector2( Coord2 coord ) { return new Vector2( coord.x, coord.y ); }
   }
   
   /// <summary>A three-dimensional integer position (x, y and z).</summary>
   [Serializable]
   public struct Coord3: IEquatable< Coord3 > {

       public int x, y, z;

       public Coord3( int x, int y, int z ) {

           this.x = x;
           this.y = y;
           this.z = z;
       }

       public override bool Equals( object obj ) {

           if (obj is Coord3) {

               Coord3 coord = (Coord3) obj;
               return x == coord.x && y == coord.y && z == coord.z;
           }
           return false;
       }

       public bool Equals( Coord3 coord ) { return x == coord.x && y == coord.y && z == coord.z; }

       public static bool   operator ==( Coord3 a, Coord3 b ) { return a.x == b.x && a.y == b.y && a.z == b.z; }

       public static bool   operator !=( Coord3 a, Coord3 b ) { return !(a == b); }
      
      /// <summary>Add two Coord3s together.</summary>
       public static Coord3 operator  +( Coord3 a, Coord3 b ) { return new Coord3( a.x + b.x, a.y + b.y, a.z + b.z ); }
      
      /// <summary>Subtract one Coord3 from another.</summary>
       public static Coord3 operator  -( Coord3 a, Coord3 b ) { return new Coord3( a.x - b.x, a.y - b.y, a.z - b.z ); }
      
      /// <summary>Negate this Coord3.</summary>
       public static Coord3 operator  -( Coord3 coord ) { return new Coord3( -coord.x, -coord.y, -coord.z ); }
      
      /// <summary>Multiply a Coord3 by an integer scalar value.</summary>
       public static Coord3 operator  *( Coord3 coord, int scalar ) {

           return new Coord3( coord.x * scalar, coord.y * scalar, coord.z * scalar );
       }
      
      /// <summary>Multiply a Coord3 by a floating-point scalar value, resulting in a Vector3.</summary>
       public static Vector3 operator *( Coord3 coord, float scalar ) {

           return new Vector3( coord.x * scalar, coord.y * scalar, coord.z * scalar );
       }
      
      /// <summary>Create a Coord3 by rounding a Vector3's components to the nearest integer.</summary>
       public static Coord3 Round( Vector3 vector ) {

           return new Coord3( Mathf.RoundToInt( vector.x ), Mathf.RoundToInt( vector.y ), Mathf.RoundToInt( vector.z ) );
       }
      
      /// <summary>Create a Coord3 by taking the floor of a Vector3's components.</summary>
       public static Coord3 Floor( Vector3 vector ) {

           return new Coord3( Mathf.FloorToInt( vector.x ), Mathf.FloorToInt( vector.y ), Mathf.FloorToInt( vector.z ) );
       }
      
      /// <summary>Create a Coord3 by taking the ceiling of a Vector3's components.</summary>
       public static Coord3 Ceil( Vector3 vector ) {

           return new Coord3( Mathf.CeilToInt( vector.x ), Mathf.CeilToInt( vector.y ), Mathf.CeilToInt( vector.z ) );
       }
      
      /// <summary>Rotate the Coord3 around the Z-axis by 90 degrees clockwise.</summary>
       public Coord3 rotatedClockwise   { get { return new Coord3( y, -x, z ); } }
      
      /// <summary>Sum of the absolute values of the X, Y and Z coordinates.</summary>
       public int    manhattanMagnitude { get { return Mathf.Abs( x ) + Mathf.Abs( y ) + Mathf.Abs( z ); } }
      
      /// <summary><para>Change a Coord3 that is relative to some cardinal X-Y direction into a global Coord3 relative to 'up'.</para>
      /// Note that this operation ignores the Z axis.</summary>
       public Coord3 RelativeToGlobal( CardinalDir facing ) {

           Coord3 rotated = this;
           for (int i = 0; i < (int) facing; i++) { rotated = rotated.rotatedClockwise; }
           return rotated;
       }
      
      /// <summary><para>Change a Coord3 that is relative to 'up' into a local Coord3 relative to the given cardinal X-Y direction.</para>
      /// Note that this operation ignores the Z axis.</summary>
       public Coord3 GlobalToRelative( CardinalDir facing ) {

           Coord3 rotated = this;
           for (int i = 4; i > (int) facing; i--) { rotated = rotated.rotatedClockwise; }
           return rotated;
       }
      
      /// <summary>Wrap the X, Y and Z coordinates between zero and the given limits.</summary>
      public Coord3 Wrapped( Coord3 limit ) {

         return new Coord3( x.Ring( limit.x, 0 ), y.Ring( limit.y, 0 ), z.Ring( limit.z, 0 ) );
      }
      
      /// <summary>Access the X, Y and Z components by indices 0, 1 and 2.</summary>
       public int this[ int index ] {
           get {
               if (index < 0 || index > 2) { throw new ArgumentOutOfRangeException(); }
               return index == 0 ? x : index == 1 ? y : z;
           }
           set {
               if (index < 0 || index > 2) { throw new ArgumentOutOfRangeException(); }
               if (index == 0) { x = value; return; }
               if (index == 1) { y = value; return; }
               z = value;
           }
       }

       public override int GetHashCode() { return (x * 23) + (y * 17) + (z * 37); }

       public override string ToString() { return "Coord3( " + x + ", " + y + ", " + z + " )"; }
      
      /// <summary>Same as "new Coord3( 0, 0, 0 )".</summary>
       public static Coord3 zero    { get { return new Coord3(  0,  0,  0 ); } }
      /// <summary>Same as "new Coord3( 1, 1, 1 )".</summary>
       public static Coord3 one     { get { return new Coord3(  1,  1,  1 ); } }
      /// <summary>Same as "new Coord3( 0, 1, 0 )".</summary>
       public static Coord3 up      { get { return new Coord3(  0,  1,  0 ); } }
      /// <summary>Same as "new Coord3( 1, 0, 0 )".</summary>
       public static Coord3 right   { get { return new Coord3(  1,  0,  0 ); } }
      /// <summary>Same as "new Coord3( 0, -1, 0 )".</summary>
       public static Coord3 down    { get { return new Coord3(  0, -1,  0 ); } }
      /// <summary>Same as "new Coord3( -1, 0, 0 )".</summary>
       public static Coord3 left    { get { return new Coord3( -1,  0,  0 ); } }
      /// <summary>Same as "new Coord3( 0, 0, 1 )".</summary>
       public static Coord3 forward { get { return new Coord3(  0,  0,  1 ); } }
      /// <summary>Same as "new Coord3( 0, 0, -1 )".</summary>
       public static Coord3 back    { get { return new Coord3(  0,  0, -1 ); } }

      public static readonly Coord3[] adjacentCardinal = {
         new Coord3( -1,  0,  0 ),
         new Coord3(  1,  0,  0 ),
         new Coord3(  0, -1,  0 ),
         new Coord3(  0,  1,  0 ),
         new Coord3(  0,  0, -1 ),
         new Coord3(  0,  0,  1 )
      };

      public static readonly Coord3[] adjacentCube = {
         new Coord3( -1, -1, -1 ),
         new Coord3(  0, -1, -1 ),
         new Coord3(  1, -1, -1 ),
         new Coord3( -1,  0, -1 ),
         new Coord3(  0,  0, -1 ),
         new Coord3(  1,  0, -1 ),
         new Coord3( -1,  1, -1 ),
         new Coord3(  0,  1, -1 ),
         new Coord3(  1,  1, -1 ),
         new Coord3( -1, -1,  0 ),
         new Coord3(  0, -1,  0 ),
         new Coord3(  1, -1,  0 ),
         new Coord3( -1,  0,  0 ),
         new Coord3(  1,  0,  0 ),
         new Coord3( -1,  1,  0 ),
         new Coord3(  0,  1,  0 ),
         new Coord3(  1,  1,  0 ),
         new Coord3( -1, -1,  1 ),
         new Coord3(  0, -1,  1 ),
         new Coord3(  1, -1,  1 ),
         new Coord3( -1,  0,  1 ),
         new Coord3(  0,  0,  1 ),
         new Coord3(  1,  0,  1 ),
         new Coord3( -1,  1,  1 ),
         new Coord3(  0,  1,  1 ),
         new Coord3(  1,  1,  1 )
      };

      public static implicit operator Vector3( Coord3 coord ) { return new Vector3( coord.x, coord.y, coord.z ); }
   }

   /// <summary>A two-dimensional range of integer positions (min and max X, and min and max Y). Ranges are inclusive.</summary>
   [Serializable]
   public struct Coord2Range: IEnumerable< Coord2 >, IEquatable< Coord2Range > {

      public int xMin, xMax, yMin, yMax;  // square in which attack source might be randomly positioned

      public Coord2Range( int xMin, int xMax, int yMin, int yMax ) {

         this.xMin = xMin;
         this.xMax = xMax;
         this.yMin = yMin;
         this.yMax = yMax;
      }

      /// <summary>Construct a Coord2Range covering a single Coord2 at the given position.</summary>
      public Coord2Range( int x, int y ) {

         xMin = xMax = x;
         yMin = yMax = y;
      }

      /// <summary>Construct a Coord2Range covering the single given Coord2.</summary>
      public Coord2Range( Coord2 coord ) {

         xMin = xMax = coord.x;
         yMin = yMax = coord.y;
      }
      
      /// <summary>Construct a Coord2Range exactly large enough to cover the given Coord2s.</summary>
      public Coord2Range( Coord2 a, Coord2 b ) {

         bool aXMin = a.x < b.x;
         bool aYMin = a.y < b.y;

         xMin = aXMin ? a.x : b.x;
         xMax = aXMin ? b.x : a.x;
         yMin = aYMin ? a.y : b.y;
         yMax = aYMin ? b.y : a.y;
      }

      /// <summary>Construct a Coord2Range exactly large enough to cover the given Coord2s.</summary>
      public Coord2Range( params Coord2[] coords ) {

         xMin = coords.Min( c => c.x );
         xMax = coords.Max( c => c.x );
         yMin = coords.Min( c => c.y );
         yMax = coords.Max( c => c.y );
      }
      
      /// <summary>Rotate the Coord2Range around the origin by 90 degrees clockwise.</summary>
      Coord2Range rotatedClockwise { get { return new Coord2Range( yMin, yMax, -xMax, -xMin ); } }
      
      /// <summary>The Coord2 corresponding to the upper-left corner of the Coord2Range.</summary>
      public Coord2 upperLeft  { get { return new Coord2( xMin, yMax ); } }
      /// <summary>The Coord2 corresponding to the upper-right corner of the Coord2Range.</summary>
      public Coord2 upperRight { get { return new Coord2( xMax, yMax ); } }
      /// <summary>The Coord2 corresponding to the lower-left corner of the Coord2Range.</summary>
      public Coord2 lowerLeft  { get { return new Coord2( xMin, yMin ); } }
      /// <summary>The Coord2 corresponding to the lower-right corner of the Coord2Range.</summary>
      public Coord2 lowerRight { get { return new Coord2( xMax, yMin ); } }
      
      /// <summary>Horizontal size of the Coord2Range.</summary>
      public int width  { get { return xMax - xMin + 1; } }
      /// <summary>Vertical size of the Coord2Range.</summary>
      public int height { get { return yMax - yMin + 1; } }
      
      /// <summary>Random Coord2 within the area of the Coord2Range.</summary>
      public Coord2 randomCoord2 {
         get { return new Coord2( UnityEngine.Random.Range( xMin, xMax ), UnityEngine.Random.Range( yMin, yMax ) ); }
      }

      /// <summary>A list of all Coord2s covered by this Coord2Range.</summary>
      public List< Coord2 > coords {
         get {
            List< Coord2 > list = new List< Coord2 >();
            for (int x = xMin; x <= xMax; x++) for (int y = yMin; y <= yMax; y++) { list.Add( new Coord2( x, y ) ); }
            return list;
         }
      }
      
      /// <summary>A list of all Coord2s immediately within the perimiter of this Coord2Range.</summary>
      public List< Coord2 > edge {
         get {
            Coord2Range range = this;
            return Where( coord => coord.x == range.xMin || coord.x == range.xMax
                                || coord.y == range.yMin || coord.y == range.xMax );
         }
      }

      /// <summary>A list of all Coord2s covered by this Coord2Range that satisfy the given condition.</summary>
      public List< Coord2 > Where( Func< Coord2, bool > condition ) {

         List< Coord2 > list = new List< Coord2 >();

         for (int x = xMin; x <= xMax; x++) for (int y = yMin; y <= yMax; y++) {

               Coord2 coord = new Coord2( x, y );
               if (!condition( coord )) { continue; }
               list.Add( new Coord2( x, y ) );
            }
         return list;
      }
      
      /// <summary>Change a Coord2Range that is relative to some direction into a global Coord2Range relative to 'up'.</summary>
      public Coord2Range RelativeToGlobal( CardinalDir forDirection ) {

         Coord2Range rotated = this;
         for (int i = 0; i < (int) forDirection; i++) { rotated = rotated.rotatedClockwise; }
         return rotated;
      }
      
      /// <summary>Shift the Coord2Range by the given displacements horizontally and vertically.</summary>
      public Coord2Range Shifted( int x, int y ) {

         return new Coord2Range( xMin + x, xMax + x, yMin + y, yMax + y );
      }

      /// <summary>Returns a Coord2Range whose minimums are increased and maximums decreased by another Coord2Range.</summary>
      public Coord2Range Shrink( Coord2Range other ) {

         return new Coord2Range( xMin + other.xMin, xMax - other.xMax, yMin + other.yMin, yMax - other.yMax );
      }

      /// <summary>Returns the intersection of two Coord2Ranges.</summary>
      public static Coord2Range Intersection( Coord2Range a, Coord2Range b ) {

         return new Coord2Range( Mathf.Max( a.xMin, b.xMin ), Mathf.Min( a.xMax, b.xMax ),
                                 Mathf.Max( a.yMin, b.yMin ), Mathf.Min( a.yMax, b.yMax ) );
      }

      /// <summary>Returns the union of two Coord2Ranges as a list of all covered Coord2s.</summary>
      public static ListSet< Coord2 > Union( Coord2Range a, Coord2Range b ) {

         ListSet< Coord2 > union = new ListSet< Coord2 >();

         union.AddRange( a.coords );
         union.AddRange( b.coords );

         return union;
      }

      /// <summary>Add corresponding values of two Coord2Ranges.</summary>
      public static Coord2Range operator +( Coord2Range a, Coord2Range b ) {
         return new Coord2Range( a.xMin + b.xMin, a.xMax + b.xMax, a.yMin + b.yMin, a.yMax + b.yMax );
      }

      /// <summary>Subtract corresponding values of two Coord2Ranges.</summary>
      public static Coord2Range operator -( Coord2Range a, Coord2Range b ) {
         return new Coord2Range( a.xMin - b.xMin, a.xMax - b.xMax, a.yMin - b.yMin, a.yMax - b.yMax );
      }

      /// <summary>Shift a Coord2Range by the displacement given by a Coord2.</summary>
      public static Coord2Range operator +( Coord2Range a, Coord2 b ) {
         return new Coord2Range( a.xMin + b.x, a.xMax + b.x, a.yMin + b.y, a.yMax + b.y );
      }
      
      /// <summary>Shift a Coord2Range by the displacement given by a negated Coord2.</summary>
      public static Coord2Range operator -( Coord2Range a, Coord2 b ) {
         return new Coord2Range( a.xMin - b.x, a.xMax - b.x, a.yMin - b.y, a.yMax - b.y );
      }
      
      /// <summary>Returns true if the Coord2 is covered by this Coord2Range.</summary>
      public bool Contains( Coord2 coord ) {
         return coord.x >= xMin && coord.x <= xMax && coord.y >= yMin && coord.y <= yMax;
      }

      public override string ToString() {
         return "CoordRange( " + xMin + " - " + xMax + ", " + yMin + " - " + yMax + " )";
      }

      public override bool Equals( object obj ) {

         if (obj is Coord2Range) {

            Coord2Range coord_range = (Coord2Range) obj;
            return xMin == coord_range.xMin && xMax == coord_range.xMax
                && yMin == coord_range.yMin && yMax == coord_range.yMax;
         }
         return false;
      }

      public bool Equals( Coord2Range coord_range ) {

         return xMin == coord_range.xMin && xMax == coord_range.xMax
             && yMin == coord_range.yMin && yMax == coord_range.yMax;
      }

      public static bool operator ==( Coord2Range a, Coord2Range b ) {

         return a.xMin == b.xMin && a.xMax == b.xMax && a.yMin == b.yMin && a.yMax == b.yMax;
      }

      public static bool operator !=( Coord2Range a, Coord2Range b ) { return !(a == b); }

      public override int GetHashCode() {

         int hash = 17;

         hash = hash * 31 + xMin;
         hash = hash * 31 + xMax;
         hash = hash * 31 + yMin;
         hash = hash * 31 + yMax;

         return hash;
      }

      public IEnumerator< Coord2 > GetEnumerator() { return coords.GetEnumerator(); }

      IEnumerator IEnumerable.GetEnumerator() { return coords.GetEnumerator(); }
   }

   public static class CoordExtensions {

      public static Vector2 Average( this IEnumerable< Coord2 > coords ) {

         return coords.Cast< Vector2 >().Average();
      }

       public static Vector3 Average( this IEnumerable< Coord3 > coords ) {

           return coords.Cast< Vector3 >().Average();
       }
   }

   [Serializable]
   public class NullableCoord2: SerializedNullable< Coord2 > {
   
      public NullableCoord2( Coord2  coord ): base( coord ) { }
      public NullableCoord2( Coord2? coord ): base( coord ) { }
      public NullableCoord2(): base() { }
      public static implicit operator NullableCoord2( Coord2 coord ) { return new NullableCoord2( coord ); }
   }
   
   [Serializable]
   public class NullableCoord3: SerializedNullable< Coord3 > {
      
       public NullableCoord3( Coord3  coord ): base( coord ) { }
       public NullableCoord3( Coord3? coord ): base( coord ) { }
       public NullableCoord3(): base() { }
      public static implicit operator NullableCoord3( Coord3 coord ) { return new NullableCoord3( coord ); }
   }

   [Serializable]
   public class NullableCoord2Range: SerializedNullable< Coord2Range > {
   
      public NullableCoord2Range( Coord2Range  coordRange ): base( coordRange ) { }
      public NullableCoord2Range( Coord2Range? coordRange ): base( coordRange ) { }
      public NullableCoord2Range(): base() { }

      public static implicit operator NullableCoord2Range( Coord2Range coordRange ) {
         return new NullableCoord2Range( coordRange );
      }
   }
}