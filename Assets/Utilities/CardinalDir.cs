// Enum for 2D cardinal directions, with supporting structs and extension methods
// by Lexa Francis, 2014-2017

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Lx {

   public enum CardinalAxis { Vertical, Horizontal }
   
   /// <summary>Represents 2D cardinal directions (up, down, left, right).</summary>
   public enum CardinalDir { Up, Right, Down, Left }
   
   /// <summary>A set of flags for each 2D cardinal direction.</summary>
   [Serializable]
   public struct DirectionFlags: IEnumerable< CardinalDir > {

      public bool up, right, down, left;
      
      /// <summary>All directions incuded in this DirectionFlags.</summary>
      public List< CardinalDir > directions {
         get {
            List< CardinalDir > list = new List< CardinalDir >();
            if (up)    { list.Add( CardinalDir.Up    ); }
            if (right) { list.Add( CardinalDir.Right ); }
            if (down)  { list.Add( CardinalDir.Down  ); }
            if (left)  { list.Add( CardinalDir.Left  ); }
            return list;
         }
      }

      public IEnumerator< CardinalDir > GetEnumerator() => directions.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => directions.GetEnumerator();

      /// <summary>Directions rotated 90 degrees clockwise.</summary>
      public DirectionFlags rotated90 => new DirectionFlags {
          right = up,
          down  = right,
          left  = down,
          up    = left
      };

      /// <summary>Flag for the given direction.</summary>
      public bool this[ CardinalDir dir ] {
         get => dir switch {
             CardinalDir.Up    =>    up,
             CardinalDir.Right => right,
             CardinalDir.Down  =>  down,
             CardinalDir.Left  =>  left,
             _                 => throw new ArgumentOutOfRangeException( nameof( dir ), dir, null )
         };
         set {
            switch (dir) {
               case CardinalDir.Up:    { up    = value; } break;
               case CardinalDir.Right: { right = value; } break;
               case CardinalDir.Down:  { down  = value; } break;
               case CardinalDir.Left:  { left  = value; } break;
               default: throw new ArgumentOutOfRangeException( nameof( dir ), dir, null );
            }
         }
      }
      
      /// <summary>Rotate by the given amount, relative to up.</summary>
      public DirectionFlags RotatedFromUpTo( CardinalDir dir ) {

         DirectionFlags rotated = this;
         for (int i = 0; i < (int) dir; i++) { rotated = rotated.rotated90; }
         return rotated;
      }

      public override bool Equals( object obj )
          => obj is DirectionFlags other && up   == other.up   && right == other.right
                                         && down == other.down && left  == other.left;

      public bool Equals( DirectionFlags other )
          => up == other.up && right == other.right && down == other.down && left == other.left;

      public static bool operator ==( DirectionFlags a, DirectionFlags b ) => a.Equals( b );

      public static bool operator !=( DirectionFlags a, DirectionFlags b ) => !(a == b);

      public override string ToString() => "DirectionFlags( " + stringEncoding + " )";

      public string stringEncoding
          => string.Join( ", ", directions.Select( d => d.ToString().Substring( 0, 1 ) ).ToArray() );

      public override int GetHashCode() => unchecked( (up ? 8 : 0) | (right ? 4 : 0) | (down ? 2 : 0) | (left ? 1 : 0) );
   }
   
   /// <summary>Utility methods for working with CardinalDir.</summary>
   public static class CardinalDirExtensions {
      
      /// <summary>Convert a vector roughly to a CardialDir.</summary>
      public static CardinalDir? InputToDirection( float x, float y ) {

         if (x >  0.5f) { return CardinalDir.Right; }
         if (x < -0.5f) { return CardinalDir.Left;  }
         if (y >  0.5f) { return CardinalDir.Up;    }
         if (y < -0.5f) { return CardinalDir.Down;  }
         return null;
      }

      /// <summary>Returns a rotation pointing in the given direction.</summary>
      public static Quaternion Rotation( this CardinalDir direction ) => direction switch {
          CardinalDir.Up    => Quaternion.Euler( 0.0f, 0.0f,   0.0f ),
          CardinalDir.Right => Quaternion.Euler( 0.0f, 90.0f,  0.0f ),
          CardinalDir.Down  => Quaternion.Euler( 0.0f, 180.0f, 0.0f ),
          CardinalDir.Left  => Quaternion.Euler( 0.0f, -90.0f, 0.0f ),
          _                 => Quaternion.identity
      };

      /// <summary>Returns a Coord2 pointing in the given direction.</summary>
      public static Coord2 RelativeCoord2( this CardinalDir direction ) => direction switch {
          CardinalDir.Up    => new Coord2(  0, 1 ),
          CardinalDir.Right => new Coord2(  1, 0 ),
          CardinalDir.Down  => new Coord2(  0, -1 ),
          CardinalDir.Left  => new Coord2( -1, 0 ),
          _                 => Coord2.zero
      };

      /// <summary>Returns the CardinalAxis of the given direction.</summary>
      public static CardinalAxis Axis( this CardinalDir direction )
          => direction == CardinalDir.Up || direction == CardinalDir.Down ? CardinalAxis.Vertical
                                                                          : CardinalAxis.Horizontal;

      /// <summary>Returns the opposite of the given direction.</summary>
      public static CardinalDir Reverse( this CardinalDir direction ) => (CardinalDir) (((int) direction + 2) % 4);

      /// <summary>Returns the next direction clockwise from the given direction.</summary>
      public static CardinalDir NextClockwise( this CardinalDir direction )
          => (CardinalDir) (((int) direction + 3) % 4);

      /// <summary>Returns the next direction anticlockwise from the given direction.</summary>
      public static CardinalDir NextAnticlockwise( this CardinalDir direction )
          => (CardinalDir) (((int) direction + 1) % 4);

      /// <summary>Returns this direction relative to the given forward direction.</summary>
      public static CardinalDir RelativeTo( this CardinalDir dir, CardinalDir forward )
          => (CardinalDir) Mathf.Repeat( (int) dir - (int) forward, 4 );

      /// <summary>Returns true if the other direction is immediately clockwise or counterclockwise of this one.</summary>
      public static bool IsAdjacent( this CardinalDir direction, CardinalDir other )
          => direction.Axis() != other.Axis();

      /// <summary>Returns true if the other direction is opposite to this one this one.</summary>
      public static bool IsOpposite( this CardinalDir direction, CardinalDir other )
          => direction != other && direction.Axis() == other.Axis();
   }
}