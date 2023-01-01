// Structs for 2D and 3D integer coordinates, and 2D area coordinates
// by Lexa Francis, 2014-2017

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;

namespace Lx {

   /// <summary>A two-dimensional integer position (x and y).</summary>
   [Serializable]
   public struct Coord2: IEquatable< Coord2 > {

      public int x, y;

      public Coord2( int x, int y ) => (this.x, this.y) = (x, y);

      public override bool Equals( object obj ) => obj is Coord2 coord && x == coord.x && y == coord.y;

      public bool Equals( Coord2 coord ) => x == coord.x && y == coord.y;

      public static implicit operator Coord2( CardinalDir dir ) => dir switch {
          CardinalDir.Up    => up,
          CardinalDir.Down  => down,
          CardinalDir.Left  => left,
          CardinalDir.Right => right,
          _                 => throw new ArgumentOutOfRangeException( nameof( dir ), dir, null )
      };

      public static implicit operator Coord2( (int x, int y) tuple ) => new Coord2( tuple.x, tuple.y );

      public static implicit operator (int x, int y) (Coord2 coord) => (coord.x, coord.y);

      public static explicit operator Coord2( Coord3 coord3 ) => new Coord2( coord3.x, coord3.y );

      public static explicit operator Coord2( Vector2 vector ) => new Coord2( (int) vector.x, (int) vector.y );

      public static Coord2 Parse( string s ) {

          if (s == null) { throw new ArgumentNullException(); }
          
          var tokens = s.Split( new [] {'x', '*', ',', 'b', 'y', ' ', ':'},
                                StringSplitOptions.RemoveEmptyEntries );
          
          if (tokens.Length < 2) { throw new FormatException( "Coord2.Parse could not find two coords in string" ); }
          
          return new Coord2( int.Parse( tokens[ 0 ].Trim() ),
                             int.Parse( tokens[ 1 ].Trim() ) );
      }

      public static bool TryParse( string s, out Coord2 coords ) {

          coords = default;
          if (s == null) { return false; }
          
          var tokens = s.Split( new [] {'x', '*', ',', 'b', 'y', ' ', ':'},
                                StringSplitOptions.RemoveEmptyEntries );
          
          if (tokens.Length < 2) { return false; }

          if (int.TryParse( tokens[ 0 ].Trim(), out int x ) && int.TryParse( tokens[ 1 ].Trim(), out int y )) {
              
              coords = new Coord2( x, y );
              return true;
          }
          
          return false;
      }

      public static bool   operator ==( Coord2 a, Coord2 b ) => a.x == b.x && a.y == b.y;

      public static bool   operator !=( Coord2 a, Coord2 b ) => !(a == b);
      
      /// <summary>Add two Coord2s together.</summary>
      public static Coord2 operator  +( Coord2 a, Coord2 b ) => new Coord2( a.x + b.x, a.y + b.y );
      
      /// <summary>Subtract one Coord2 from another.</summary>
      public static Coord2 operator  -( Coord2 a, Coord2 b ) => new Coord2( a.x - b.x, a.y - b.y );
      
      /// <summary>Negate this Coord2.</summary>
      public static Coord2 operator  -( Coord2 coord ) => new Coord2( -coord.x, -coord.y );
      
      /// <summary>Multiply a Coord2 by an integer scalar value.</summary>
      public static Coord2 operator  *( Coord2 coord, int scalar ) => new Coord2( coord.x * scalar, coord.y * scalar );
      
      /// <summary>Multiply a Coord2 by a floating-point scalar value, resulting in a Vector2.</summary>
      public static Vector2 operator *( Coord2 c, float s ) => new Vector2( c.x * s, c.y * s );
      
      /// <summary>Create a Coord2 by rounding a Vector2's components to the nearest integer.</summary>
      public static Coord2 Round( Vector2 v ) => new Coord2( Mathf.RoundToInt( v.x ), Mathf.RoundToInt( v.y ) );

      /// <summary>Create a Coord2 by taking the next lower integer of each component of a vector.</summary>
      public static Coord2 Floor( Vector2 v ) => new Coord2( Mathf.FloorToInt( v.x ), Mathf.FloorToInt( v.y ) );
      
      /// <summary>Create a Coord2 by taking the next higher integer of each component of a vector.</summary>
      public static Coord2 Ceil( Vector2 v ) => new Coord2( Mathf.CeilToInt( v.x ), Mathf.CeilToInt( v.y ) );

      /// <summary>Returns a Coord2 with x and y set to the minimums of the corresponding values in a and b.</summary>
      public static Coord2 Min( Coord2 a, Coord2 b ) => new Coord2( Mathf.Min( a.x, b.x ), Mathf.Min( a.y, b.y ) );

      /// <summary>Returns a Coord2 with x and y set to the maximums of the corresponding values in a and b.</summary>
      public static Coord2 Max( Coord2 a, Coord2 b ) => new Coord2( Mathf.Max( a.x, b.x ), Mathf.Max( a.y, b.y ) );

      public Coord2 abs => new Coord2( Mathf.Abs( x ), Mathf.Abs( y ) );  // questionable
      
      /// <summary>Rotate the Coord2 around the origin by 90 degrees clockwise.</summary>
      public Coord2 rotatedClockwise   => new Coord2( y, -x );
      
      /// <summary>Sum of the absolute values of the X and Y coordinates.</summary>
      public int manhattanMagnitude => Mathf.Abs( x ) + Mathf.Abs( y );
      
      /// <summary>Sum of the absolute values of the X and Y coordinates.</summary>
      public int chebyshevMagnitude => Mathf.Max( Mathf.Abs( x ), Mathf.Abs( y ) );

      public static int ManhattanDistance( Coord2 a, Coord2 b ) => Mathf.Abs( a.x - b.x ) + Mathf.Abs( a.y - b.y );

      public static int ChebyshevDistance( Coord2 a, Coord2 b )
          => Mathf.Max( Mathf.Abs( a.x - b.x ), Mathf.Abs( a.y - b.y ) );

      public Coord3 xy0 => new Coord3( x, y, 0 );
      
      public Coord3 x0y => new Coord3( x, 0, y );
      
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
      public Coord2 Wrapped( Coord2 limit ) => new Coord2( x.Ring( limit.x ), y.Ring( limit.y ) );

      public bool InArrayBounds< T >( T[ , ] array ) =>
          x >= 0 && x < array.GetLength( 0 ) && y >= 0 && y < array.GetLength( 1 );

      Coord2[] GetRelativeCoords( IReadOnlyList< Coord2 > absolute ) {
          
          var relative = new Coord2[ absolute.Count ];
          for (int i = 0; i < relative.Length; i++) { relative[ i ] = this + absolute[ i ]; }
          return relative;
      }

      public Coord2[] adjacentCardinal => GetRelativeCoords( adjacentToOriginCardinal );
      public Coord2[] adjacentDiagonal => GetRelativeCoords( adjacentToOriginDiagonal );
      public Coord2[] adjacentSquare => GetRelativeCoords( adjacentToOriginSquare );

      public override int GetHashCode() => unchecked( x * 23 + y * 17 );

      public override string ToString() => $"Coord2( {x}, {y} )";
      
      /// <summary>Same as "new Coord2( 0, 0 )".</summary>
      public static Coord2 zero  => new Coord2(  0,  0 );
      /// <summary>Same as "new Coord2( 1, 1 )".</summary>
      public static Coord2 one   => new Coord2(  1,  1 );
      /// <summary>Same as "new Coord2( 0, 1 )".</summary>
      public static Coord2 up    => new Coord2(  0,  1 );
      /// <summary>Same as "new Coord2( 1, 0 )".</summary>
      public static Coord2 right => new Coord2(  1,  0 );
      /// <summary>Same as "new Coord2( 0, -1 )".</summary>
      public static Coord2 down  => new Coord2(  0, -1 );
      /// <summary>Same as "new Coord2( -1, 0 )".</summary>
      public static Coord2 left  => new Coord2( -1,  0 );

      public static readonly Coord2[] adjacentToOriginCardinal = {
         new Coord2( -1,  0 ),
         new Coord2(  1,  0 ),
         new Coord2(  0, -1 ),
         new Coord2(  0,  1 )
      };

      public static readonly Coord2[] adjacentToOriginDiagonal = {
         new Coord2( -1,  -1 ),
         new Coord2(  1,  -1 ),
         new Coord2( -1,   1 ),
         new Coord2(  1,   1 )
      };

      public static readonly Coord2[] adjacentToOriginSquare = {
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

      public Coord3( int x, int y, int z ) => (this.x, this.y, this.z) = (x, y, z);

      public static implicit operator Coord3( (int x, int y, int z) tuple ) => new Coord3( tuple.x, tuple.y, tuple.z );

      public static implicit operator (int x, int y, int z) (Coord3 coord) => (coord.x, coord.y, coord.z);

      public static implicit operator Coord3( Coord2 coord2 ) => new Coord3( coord2.x, coord2.y, 0 );

      public static explicit operator Coord3( Vector3 vector )
          => new Coord3( (int) vector.x, (int) vector.y, (int) vector.z );

      public static implicit operator Vector3( Coord3 coord ) => new Vector3( coord.x, coord.y, coord.z );

      public static Coord3 Parse( string s ) {

          if (s == null) { throw new ArgumentNullException(); }
          
          var tokens = s.Split( new [] {'x', '*', ',', 'b', 'y', ' ', ':'},
                                StringSplitOptions.RemoveEmptyEntries );
          
          if (tokens.Length < 3) { throw new FormatException( "Coord3.Parse could not find three coords in string" ); }
          
          return new Coord3( int.Parse( tokens[ 0 ].Trim() ),
                             int.Parse( tokens[ 1 ].Trim() ),
                             int.Parse( tokens[ 2 ].Trim() ) );
      }

      public static bool TryParse( string s, out Coord3 coords ) {

          coords = default;
          if (s == null) { return false; }
          
          var tokens = s.Split( new [] {'x', '*', ',', 'b', 'y', ' ', ':'},
                                StringSplitOptions.RemoveEmptyEntries );
          
          if (tokens.Length < 3) { return false; }

          if (int.TryParse( tokens[ 0 ].Trim(), out int x ) && int.TryParse( tokens[ 1 ].Trim(), out int y )
              && int.TryParse( tokens[ 2 ].Trim(),                                                                                out int z )) {
              
              coords = new Coord3( x, y, z );
              return true;
          }
          
          return false;
      }

      public override bool Equals( object obj ) => obj is Coord3 coord && x == coord.x && y == coord.y && z == coord.z;

      public bool Equals( Coord3 coord ) => x == coord.x && y == coord.y && z == coord.z;

      public static bool   operator ==( Coord3 a, Coord3 b ) => a.x == b.x && a.y == b.y && a.z == b.z;

      public static bool   operator !=( Coord3 a, Coord3 b ) => !(a == b);
      
      /// <summary>Add two Coord3s together.</summary>
      public static Coord3 operator  +( Coord3 a, Coord3 b ) => new Coord3( a.x + b.x, a.y + b.y, a.z + b.z );
      
      /// <summary>Subtract one Coord3 from another.</summary>
      public static Coord3 operator  -( Coord3 a, Coord3 b ) => new Coord3( a.x - b.x, a.y - b.y, a.z - b.z );
      
      /// <summary>Negate this Coord3.</summary>
      public static Coord3 operator  -( Coord3 coord ) => new Coord3( -coord.x, -coord.y, -coord.z );
      
      /// <summary>Multiply a Coord3 by an integer scalar value.</summary>
      public static Coord3 operator  *( Coord3 c, int s ) => new Coord3( c.x * s, c.y * s, c.z * s );
      
      /// <summary>Multiply a Coord3 by a floating-point scalar value, resulting in a Vector3.</summary>
      public static Vector3 operator *( Coord3 c, float s ) => new Vector3( c.x * s, c.y * s, c.z * s );
      
      /// <summary>Divide a Coord3 by a floating-point scalar value, resulting in a Vector3.</summary>
      public static Vector3 operator /( Coord3 c, float s ) => new Vector3( c.x / s, c.y / s, c.z / s );
      
      /// <summary>Create a Coord3 by rounding a Vector3's components to the nearest integer.</summary>
      public static Coord3 Round( Vector3 vector ) =>
         new Coord3( Mathf.RoundToInt( vector.x ), Mathf.RoundToInt( vector.y ), Mathf.RoundToInt( vector.z ) );
      
      /// <summary>Create a Coord3 by taking the floor of a Vector3's components.</summary>
      public static Coord3 Floor( Vector3 vector ) =>
         new Coord3( Mathf.FloorToInt( vector.x ), Mathf.FloorToInt( vector.y ), Mathf.FloorToInt( vector.z ) );
      
      /// <summary>Create a Coord3 by taking the ceiling of a Vector3's components.</summary>
      public static Coord3 Ceil( Vector3 vector ) =>
         new Coord3( Mathf.CeilToInt( vector.x ), Mathf.CeilToInt( vector.y ), Mathf.CeilToInt( vector.z ) );

      /// <summary>Returns a Coord3 with components set to the minimums of the corresponding components in a and b.</summary>
      public static Coord3 Min( Coord3 a, Coord3 b )
          => new Coord3( Mathf.Min( a.x, b.x ), Mathf.Min( a.y, b.y ), Mathf.Min( a.z, b.z ) );

      /// <summary>Returns a Coord3 with components set to the maximums of the corresponding values in a and b.</summary>
      public static Coord3 Max( Coord3 a, Coord3 b )
          => new Coord3( Mathf.Max( a.x, b.x ), Mathf.Max( a.y, b.y ), Mathf.Max( a.z, b.z ) );

      /// <summary>Rotate the Coord3 around the Z-axis by 90 degrees clockwise.</summary>
      public Coord3 clockwiseAroundZ => new Coord3( y, -x, z );
      
      /// <summary>Rotate the Coord3 around the Y-axis by 90 degrees clockwise.</summary>
      public Coord3 clockwiseAroundY => new Coord3( z, y, -x );
      
      /// <summary>Sum of the absolute values of the X, Y and Z coordinates.</summary>
      public int    manhattanMagnitude => Mathf.Abs( x ) + Mathf.Abs( y ) + Mathf.Abs( z );

      public Coord2 xy => new Coord2( x, y );
      
      public Coord2 xz => new Coord2( x, z );
      
      /// <summary><para>Change a Coord3 that is relative to some cardinal X-Y direction into a global Coord3 relative to 'up'.</para>
      /// Note that this operation ignores the Z axis.</summary>
      public Coord3 RelativeToGlobal( CardinalDir facing ) {

         Coord3 rotated = this;
         for (int i = 0; i < (int) facing; i++) { rotated = rotated.clockwiseAroundZ; }
         return rotated;
      }
      
      /// <summary><para>Change a Coord3 that is relative to 'up' into a local Coord3 relative to the given cardinal X-Y direction.</para>
      /// Note that this operation ignores the Z axis.</summary>
      public Coord3 GlobalToRelative( CardinalDir facing ) {

         Coord3 rotated = this;
         for (int i = 4; i > (int) facing; i--) { rotated = rotated.clockwiseAroundZ; }
         return rotated;
      }
      
      /// <summary>Wrap the X, Y and Z coordinates between zero and the given limits.</summary>
      public Coord3 Wrapped( Coord3 limit ) => new Coord3( x.Ring( limit.x ), y.Ring( limit.y ), z.Ring( limit.z ) );
      
      /// <summary>Access the X, Y and Z components by indices 0, 1 and 2.</summary>
      public int this[ int index ] {
         get => index switch {
             0 => x,
             1 => y,
             2 => z,
             _ => throw new ArgumentOutOfRangeException( nameof( index ), index, null )
         };
         set {
            switch (index) {
                case 0:  x = value; return;
                case 1:  y = value; return;
                case 2:  z = value; return;
                default: throw new ArgumentOutOfRangeException( nameof( index ), index, null );
            }
         }
      }

      public override int GetHashCode() => unchecked( x * 23 + y * 17 + z * 37 );

      public override string ToString() => $"Coord3( {x}, {y}, {z} )";
      
      /// <summary>Same as "new Coord3( 0, 0, 0 )".</summary>
      public static Coord3 zero    => new Coord3(  0,  0,  0 );
      /// <summary>Same as "new Coord3( 1, 1, 1 )".</summary>
      public static Coord3 one     => new Coord3(  1,  1,  1 );
      /// <summary>Same as "new Coord3( 0, 1, 0 )".</summary>
      public static Coord3 up      => new Coord3(  0,  1,  0 );
      /// <summary>Same as "new Coord3( 1, 0, 0 )".</summary>
      public static Coord3 right   => new Coord3(  1,  0,  0 );
      /// <summary>Same as "new Coord3( 0, -1, 0 )"".</summary>
      public static Coord3 down    => new Coord3(  0, -1,  0 );
      /// <summary>Same as "new Coord3( -1, 0, 0 )"".</summary>
      public static Coord3 left    => new Coord3( -1,  0,  0 );
      /// <summary>Same as "new Coord3( 0, 0, 1 )".</summary>
      public static Coord3 forward => new Coord3(  0,  0,  1 );
      /// <summary>Same as "new Coord3( 0, 0, -1 )"".</summary>
      public static Coord3 back    => new Coord3(  0,  0, -1 );

      public static readonly Coord3[] adjacentToOriginCardinal = {
         new Coord3( -1,  0,  0 ),
         new Coord3(  1,  0,  0 ),
         new Coord3(  0, -1,  0 ),
         new Coord3(  0,  1,  0 ),
         new Coord3(  0,  0, -1 ),
         new Coord3(  0,  0,  1 )
      };

      public static readonly Coord3[] adjacentToOriginCube = {
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
   }

   /// <summary>A two-dimensional range of integer positions (min and max X, and min and max Y).
   /// Ranges are inclusive.</summary>
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

      /// <summary>Construct a Coord2Range from two IntRanges.</summary>
      public Coord2Range( IntRange x, IntRange y ) {

          xMin = x.min;
          xMax = x.max;
          yMin = y.min;
          yMax = y.max;
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

      public static implicit operator Rect( Coord2Range range ) =>
         new Rect( range.xMin, range.yMin, range.xMax - range.xMin + 1, range.yMax - range.yMin + 1 );
      
      /// <summary>Rotate the Coord2Range around the origin by 90 degrees clockwise.</summary>
      Coord2Range rotatedClockwise => new Coord2Range( yMin, yMax, -xMax, -xMin );
      
      /// <summary>The Coord2 corresponding to the lower-left corner of the Coord2Range.</summary>
      public Coord2 min => new Coord2( xMin, yMin );
      
      /// <summary>The Coord2 corresponding to the upper-right corner of the Coord2Range.</summary>
      public Coord2 max => new Coord2( xMax, yMax );
      
      /// <summary>The Coord2 corresponding to the upper-left corner of the Coord2Range.</summary>
      public Coord2 upperLeft => new Coord2( xMin, yMax );
      
      /// <summary>The Coord2 corresponding to the upper-right corner of the Coord2Range.</summary>
      public Coord2 upperRight => new Coord2( xMax, yMax );
      
      /// <summary>The Coord2 corresponding to the lower-left corner of the Coord2Range.</summary>
      public Coord2 lowerLeft => new Coord2( xMin, yMin );
      
      /// <summary>The Coord2 corresponding to the lower-right corner of the Coord2Range.</summary>
      public Coord2 lowerRight => new Coord2( xMax, yMin );
      
      /// <summary>Horizontal size of the Coord2Range.</summary>
      public int width => xMax - xMin + 1;
      
      /// <summary>Vertical size of the Coord2Range.</summary>
      public int height => yMax - yMin + 1;

      public IntRange x {
          get => new IntRange( xMin, xMax );
          set {
              xMin = value.min;
              xMax = value.max;
          }
      }
      
      public IntRange y {
          get => new IntRange( yMin, yMax );
          set {
              yMin = value.min;
              yMax = value.max;
          }
      }
      
      /// <summary>Random Coord2 within the area of the Coord2Range.</summary>
      public Coord2 random =>
         new Coord2( Random.Range( xMin, xMax + 1 ), Random.Range( yMin, yMax + 1 ) );

      /// <summary>A list of all Coord2s covered by this Coord2Range.</summary>
      public List< Coord2 > coords {
         get {
            List< Coord2 > list = new List< Coord2 >();
            for (int i = xMin; i <= xMax; i++) for (int j = yMin; j <= yMax; j++) { list.Add( new Coord2( i, j ) ); }
            return list;
         }
      }
      
      /// <summary>A list of all Coord2s immediately within the perimiter of this Coord2Range.</summary>
      public List< Coord2 > edge {
         get {
            Coord2Range range = this;
            return Where( c => c.x == range.xMin || c.x == range.xMax || c.y == range.yMin || c.y == range.yMax );
         }
      }

      /// <summary>A list of all Coord2s covered by this Coord2Range that satisfy the given condition.</summary>
      public List< Coord2 > Where( Func< Coord2, bool > condition ) {

         List< Coord2 > list = new List< Coord2 >();

         for (int i = xMin; i <= xMax; i++) for (int j = yMin; j <= yMax; j++) {

            Coord2 coord = new Coord2( i, j );
            if (!condition( coord )) { continue; }
            list.Add( new Coord2( i, j ) );
         }

         return list;
      }
      
      /// <summary>Change a Coord2Range that is relative to some direction into a global Coord2Range relative to 'up'.</summary>
      public Coord2Range RelativeToGlobal( CardinalDir forDirection ) {

         Coord2Range rotated = this;
         for (int i = 0; i < (int) forDirection; i++) { rotated = rotated.rotatedClockwise; }
         return rotated;
      }

      /// <summary>Returns a Coord2Range whose minimums are increased and maximums decreased by another Coord2Range.</summary>
      public Coord2Range Shrink( Coord2Range other ) =>
         new Coord2Range( xMin + other.xMin, xMax - other.xMax, yMin + other.yMin, yMax - other.yMax );

      /// <summary>Returns the intersection of two Coord2Ranges.</summary>
      public static Coord2Range Intersection( Coord2Range a, Coord2Range b ) =>

         new Coord2Range( Mathf.Max( a.xMin, b.xMin ), Mathf.Min( a.xMax, b.xMax ),
                          Mathf.Max( a.yMin, b.yMin ), Mathf.Min( a.yMax, b.yMax ) );

      /// <summary>Returns the smallest Coord2Range that covers both a and b.</summary>
      public static Coord2Range Union( Coord2Range a, Coord2Range b )
          => new Coord2Range( Coord2.Min( a.min, b.min ), Coord2.Max( a.max, b.max ) );

      /// <summary>Add corresponding values of two Coord2Ranges.</summary>
      public static Coord2Range operator +( Coord2Range a, Coord2Range b ) =>
         new Coord2Range( a.xMin + b.xMin, a.xMax + b.xMax, a.yMin + b.yMin, a.yMax + b.yMax );

      /// <summary>Subtract corresponding values of two Coord2Ranges.</summary>
      public static Coord2Range operator -( Coord2Range a, Coord2Range b ) =>
         new Coord2Range( a.xMin - b.xMin, a.xMax - b.xMax, a.yMin - b.yMin, a.yMax - b.yMax );

      /// <summary>Shift a Coord2Range by the displacement given by a Coord2.</summary>
      public static Coord2Range operator +( Coord2Range a, Coord2 b ) =>
         new Coord2Range( a.xMin + b.x, a.xMax + b.x, a.yMin + b.y, a.yMax + b.y );
      
      /// <summary>Shift a Coord2Range by the displacement given by a negated Coord2.</summary>
      public static Coord2Range operator -( Coord2Range a, Coord2 b ) =>
         new Coord2Range( a.xMin - b.x, a.xMax - b.x, a.yMin - b.y, a.yMax - b.y );
      
      /// <summary>Returns true if the Coord2 is covered by this Coord2Range.</summary>
      public bool Contains( Coord2 coord )
          => coord.x >= xMin && coord.x <= xMax && coord.y >= yMin && coord.y <= yMax;

      public override string ToString() => $"CoordRange( {xMin} to {xMax}, {yMin} to {yMax} )";

      public override bool Equals( object obj )
          => obj is Coord2Range range && xMin == range.xMin && xMax == range.xMax
                                      && yMin == range.yMin && yMax == range.yMax;

      public bool Equals( Coord2Range range ) =>
         xMin == range.xMin && xMax == range.xMax && yMin == range.yMin && yMax == range.yMax;

      public static bool operator ==( Coord2Range a, Coord2Range b ) =>
         a.xMin == b.xMin && a.xMax == b.xMax && a.yMin == b.yMin && a.yMax == b.yMax;

      public static bool operator !=( Coord2Range a, Coord2Range b ) => !(a == b);

      public override int GetHashCode() {

          unchecked {
              
             int hash = 17;
 
             hash = hash * 31 + xMin;
             hash = hash * 31 + xMax;
             hash = hash * 31 + yMin;
             hash = hash * 31 + yMax;

             return hash;
          }
      }

      public IEnumerator< Coord2 > GetEnumerator() => coords.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => coords.GetEnumerator();
   }

   /// <summary>A three-dimensional range of integer positions (min and max X, Y and Z). Ranges are inclusive.</summary>
   [Serializable]
   public struct Coord3Range: IEnumerable< Coord3 >, IEquatable< Coord3Range > {

      public int xMin, xMax, yMin, yMax, zMin, zMax;

      public Coord3Range( int xMin, int xMax, int yMin, int yMax, int zMin, int zMax ) {

         this.xMin = xMin;
         this.xMax = xMax;
         this.yMin = yMin;
         this.yMax = yMax;
         this.zMin = zMin;
         this.zMax = zMax;
      }

      /// <summary>Construct a Coord3Range covering a single Coord3 at the given position.</summary>
      public Coord3Range( int x, int y, int z ) {

         xMin = xMax = x;
         yMin = yMax = y;
         zMin = zMax = z;
      }

      /// <summary>Construct a Coord3Range from three IntRanges.</summary>
      public Coord3Range( IntRange x, IntRange y, IntRange z ) {

          xMin = x.min;
          xMax = x.max;
          yMin = y.min;
          yMax = y.max;
          zMin = z.min;
          zMax = z.max;
      }

      /// <summary>Construct a Coord3Range covering the single given Coord3.</summary>
      public Coord3Range( Coord3 coord ) {

         xMin = xMax = coord.x;
         yMin = yMax = coord.y;
         zMin = zMax = coord.z;
      }
      
      /// <summary>Construct a Coord3Range exactly large enough to cover the given Coord3s.</summary>
      public Coord3Range( Coord3 a, Coord3 b ) {

         bool aXMin = a.x < b.x;
         bool aYMin = a.y < b.y;
         bool aZMin = a.z < b.z;

         xMin = aXMin ? a.x : b.x;
         xMax = aXMin ? b.x : a.x;
         yMin = aYMin ? a.y : b.y;
         yMax = aYMin ? b.y : a.y;
         zMin = aZMin ? a.z : b.z;
         zMax = aZMin ? b.z : a.z;
      }

      /// <summary>Construct a Coord3Range exactly large enough to cover the given Coord3s.</summary>
      public Coord3Range( params Coord3[] coords ) {

         xMin = coords.Min( c => c.x );
         xMax = coords.Max( c => c.x );
         yMin = coords.Min( c => c.y );
         yMax = coords.Max( c => c.y );
         zMin = coords.Min( c => c.z );
         zMax = coords.Max( c => c.z );
      }

      public static implicit operator Bounds( Coord3Range range ) =>
         new Bounds( (Vector3) (range.upperRightFront + range.lowerLeftRear + Coord3.one) * 0.5f,
                                range.upperRightFront - range.lowerLeftRear + Coord3.one );
      
      /// <summary>The Coord3 corresponding to the lower-left rear corner of the Coord3Range.</summary>
      public Coord3 min => new Coord3( xMin, yMin, zMin );
      
      /// <summary>The Coord3 corresponding to the upper-right front corner of the Coord3Range.</summary>
      public Coord3 max => new Coord3( xMax, yMax, zMax );
      
      /// <summary>Rotate the Coord3Range around the origin by 90 degrees clockwise in the Z axis.</summary>
      Coord3Range rotatedClockwiseZ => new Coord3Range( yMin, yMax, -xMax, -xMin, zMin, zMax );
      
      /// <summary>Rotate the Coord3Range around the origin by 90 degrees clockwise in the Y axis.</summary>
      Coord3Range rotatedClockwiseY => new Coord3Range( zMin, zMax, yMin, yMax, -xMax, -xMin );
      
      /// <summary>The Coord3 corresponding to the upper-left rear corner of the Coord3Range.</summary>
      public Coord3 upperLeftRear   => new Coord3( xMin, yMax, zMin );
      
      /// <summary>The Coord3 corresponding to the upper-right rear corner of the Coord3Range.</summary>
      public Coord3 upperRightRear  => new Coord3( xMax, yMax, zMin );
      
      /// <summary>The Coord3 corresponding to the lower-left rear corner of the Coord3Range.</summary>
      public Coord3 lowerLeftRear   => new Coord3( xMin, yMin, zMin );
      
      /// <summary>The Coord3 corresponding to the lower-right rear corner of the Coord3Range.</summary>
      public Coord3 lowerRightRear  => new Coord3( xMax, yMin, zMin );
      
      /// <summary>The Coord3 corresponding to the upper-left front corner of the Coord3Range.</summary>
      public Coord3 upperLeftFront  => new Coord3( xMin, yMax, zMax );
      
      /// <summary>The Coord3 corresponding to the upper-right front corner of the Coord3Range.</summary>
      public Coord3 upperRightFront => new Coord3( xMax, yMax, zMax );
      
      /// <summary>The Coord3 corresponding to the lower-left front corner of the Coord3Range.</summary>
      public Coord3 lowerLeftFront  => new Coord3( xMin, yMin, zMax );
      
      /// <summary>The Coord3 corresponding to the lower-right front corner of the Coord3Range.</summary>
      public Coord3 lowerRightFront => new Coord3( xMax, yMin, zMax );
      
      /// <summary>Horizontal size of the Coord3Range.</summary>
      public int width  => xMax - xMin + 1;
      
      /// <summary>Vertical size of the Coord3Range.</summary>
      public int height => yMax - yMin + 1;
      
      /// <summary>Rear-to-font size of the Coord3Range.</summary>
      public int depth  => zMax - zMin + 1;

      public IntRange x {
          get => new IntRange( xMin, xMax );
          set {
              xMin = value.min;
              xMax = value.max;
          }
      }
      
      public IntRange y {
          get => new IntRange( yMin, yMax );
          set {
              yMin = value.min;
              yMax = value.max;
          }
      }
      
      public IntRange z {
          get => new IntRange( zMin, zMax );
          set {
              zMin = value.min;
              zMax = value.max;
          }
      }
      
      /// <summary>Random Coord3 within the area of the Coord3Range.</summary>
      public Coord3 random =>
         new Coord3( Random.Range( xMin, xMax + 1 ), Random.Range( yMin, yMax + 1 ), Random.Range( zMin, zMax + 1 ) );

      /// <summary>A list of all Coord3s covered by this Coord3Range.</summary>
      public List< Coord3 > coords {
         get {
            List< Coord3 > list = new List< Coord3 >();
            for (int i = xMin; i <= xMax; i++) for (int j = yMin; j <= yMax; j++) for (int k = zMin; k <= zMax; k++) {
               list.Add( new Coord3( i, j, k ) );
            }
            return list;
         }
      }
      
      /// <summary>A list of all Coord3s immediately within the perimiter of this Coord3Range.</summary>
      public List< Coord3 > faceCoords {
         get {
            Coord3Range range = this;
            return Where( c => c.x == range.xMin || c.x == range.xMax || c.y == range.yMin || c.y == range.yMax
                            || c.z == range.zMin || c.z == range.zMax );
         }
      }
      
      /// <summary>A list of the eight corner coords of the Coord3Range.</summary>
      public Coord3[] corners => new[] {
         lowerLeftRear,  lowerRightRear,  upperLeftRear,  upperRightRear,
         lowerLeftFront, lowerRightFront, upperLeftFront, upperRightFront
      };

      /// <summary>A list of all Coord3s covered by this Coord3Range that satisfy the given condition.</summary>
      public List< Coord3 > Where( Func< Coord3, bool > condition ) {

         List< Coord3 > list = new List< Coord3 >();

         for (int i = xMin; i <= xMax; i++) for (int j = yMin; j <= yMax; j++) for (int k = zMin; k <= zMax; k++) {

            Coord3 coord = new Coord3( i, j, k );
            if (!condition( coord )) { continue; }
            list.Add( coord );
         }

         return list;
      }
      
      /// <summary>Change a Coord3Range that is relative to some direction into a global Coord3Range relative to 'up'.</summary>
      public Coord3Range RelativeToGlobalZ( CardinalDir forDirection ) {

         Coord3Range rotated = this;
         for (int i = 0; i < (int) forDirection; i++) { rotated = rotated.rotatedClockwiseZ; }
         return rotated;
      }
      
      /// <summary>Change a Coord3Range that is relative to some direction into a global Coord3Range relative to 'up'.</summary>
      public Coord3Range RelativeToGlobalY( CardinalDir forDirection ) {

         Coord3Range rotated = this;
         for (int i = 0; i < (int) forDirection; i++) { rotated = rotated.rotatedClockwiseY; }
         return rotated;
      }
      
      /// <summary>Shift the Coord3Range by the given displacements horizontally and vertically.</summary>
      public Coord3Range Shifted( int xShift, int yShift, int zShift ) =>
         new Coord3Range( xMin + xShift, xMax + xShift, yMin + yShift, yMax + yShift, zMin + zShift, zMax + zShift );

      /// <summary>Returns a Coord3Range whose minimums are increased and maximums decreased by another Coord3Range.</summary>
      public Coord3Range Shrink( Coord3Range r ) =>
         new Coord3Range( xMin + r.xMin, xMax - r.xMax, yMin + r.yMin, yMax - r.yMax, zMin + r.zMin, zMax - r.zMax );

      /// <summary>Returns the intersection of two Coord3Ranges.</summary>
      public static Coord3Range Intersection( Coord3Range a, Coord3Range b ) =>

         new Coord3Range( Mathf.Max( a.xMin, b.xMin ), Mathf.Min( a.xMax, b.xMax ),
                          Mathf.Max( a.yMin, b.yMin ), Mathf.Min( a.yMax, b.yMax ),
                          Mathf.Max( a.zMin, b.zMin ), Mathf.Min( a.zMax, b.zMax ) );

      /// <summary>Returns the smallest Coord3Range that covers both a and b.</summary>
      public static Coord3Range Union( Coord3Range a, Coord3Range b )
          => new Coord3Range( Coord3.Min( a.min, b.min ), Coord3.Max( a.max, b.max ) );

      /// <summary>Add corresponding values of two Coord3Ranges.</summary>
      public static Coord3Range operator +( Coord3Range a, Coord3Range b ) =>
         new Coord3Range( a.xMin + b.xMin, a.xMax + b.xMax, a.yMin + b.yMin, a.yMax + b.yMax,
                          a.zMin + b.zMin, a.zMax + b.zMax );

      /// <summary>Subtract corresponding values of two Coord3Ranges.</summary>
      public static Coord3Range operator -( Coord3Range a, Coord3Range b ) =>
         new Coord3Range( a.xMin - b.xMin, a.xMax - b.xMax, a.yMin - b.yMin, a.yMax - b.yMax,
                          a.zMin - b.zMin, a.zMax - b.zMax );

      /// <summary>Shift a Coord3Range by the displacement given by a Coord3.</summary>
      public static Coord3Range operator +( Coord3Range a, Coord3 b ) =>
         new Coord3Range( a.xMin + b.x, a.xMax + b.x, a.yMin + b.y, a.yMax + b.y, a.zMin + b.z, a.zMax + b.z );
      
      /// <summary>Shift a Coord3Range by the displacement given by a negated Coord3.</summary>
      public static Coord3Range operator -( Coord3Range a, Coord3 b ) =>
         new Coord3Range( a.xMin - b.x, a.xMax - b.x, a.yMin - b.y, a.yMax - b.y, a.zMin - b.z, a.zMax + b.z );
      
      /// <summary>Returns true if the Coord3 is covered by this Coord3Range.</summary>
      public bool Contains( Coord3 coord ) =>
         coord.x >= xMin && coord.x <= xMax && coord.y >= yMin && coord.y <= yMax && coord.z >= zMin && coord.z <= zMax;

      public override string ToString() => $"CoordRange( {xMin} to {xMax}, {yMin} to {yMax}, {zMin} to {zMax} )";

      public override bool Equals( object obj )
         => obj is Coord3Range range && xMin == range.xMin && xMax == range.xMax && yMin == range.yMin
                                     && yMax == range.yMax && zMin == range.zMin && zMax == range.zMax;

      public bool Equals( Coord3Range coord_range ) =>

         xMin == coord_range.xMin && xMax == coord_range.xMax
         && yMin == coord_range.yMin && yMax == coord_range.yMax
         && zMin == coord_range.zMin && zMax == coord_range.zMax;

      public static bool operator ==( Coord3Range a, Coord3Range b ) =>
         a.xMin == b.xMin && a.xMax == b.xMax && a.yMin == b.yMin && a.yMax == b.yMax
         && a.zMin == b.zMin && a.zMax == b.zMax;

      public static bool operator !=( Coord3Range a, Coord3Range b ) => !(a == b);

      public override int GetHashCode() {

          unchecked {
              
              int hash = 17;

              hash = hash * 31 + xMin;
              hash = hash * 31 + xMax;
              hash = hash * 31 + yMin;
              hash = hash * 31 + yMax;
              hash = hash * 31 + zMin;
              hash = hash * 31 + zMax;

              return hash;
          }
      }

      public IEnumerator< Coord3 > GetEnumerator() => coords.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => coords.GetEnumerator();
   }

   public static class CoordExtensions {

      public static Vector2 Average( this IEnumerable< Coord2 > coords ) => coords.Select( c => (Vector2) c ).Average();

      public static Vector3 Average( this IEnumerable< Coord3 > coords ) => coords.Select( c => (Vector3) c ).Average();

      public static T Get< T >( this T[,]  array, Coord2 coords ) => array[ coords.x, coords.y ];

      public static T Get< T >( this T[,,] array, Coord3 coords ) => array[ coords.x, coords.y, coords.z ];
   }
}