// Structs for representing ranges of integers and floats
// by Lexa Francis, 2014-2017

using System;

namespace Lx {

   [Serializable]
   public struct IntRange {

      public int min, max;

      public IntRange( int val ) {

         min = val;
         max = val;
      }

      public IntRange( int min, int max ) {

         this.min = min;
         this.max = max;
      }

      public int random { get { return UnityEngine.Random.Range( min, max + 1 ); } }

      public bool Contains( int value ) {

         return value >= min && value <= max;
      }

      public override string ToString() {

         return "IntRange( " + min + " to " + max + " )";
      }

      public string ToShortString() {

         return min + "-" + max;
      }
   }
   
   [Serializable]
   public class NullableIntRange: SerializedNullable< IntRange > {

      public NullableIntRange( IntRange  range ): base( range ) { }
      public NullableIntRange( IntRange? range ): base( range ) { }
      public NullableIntRange(): base() { }
      public static implicit operator NullableIntRange( IntRange value ) { return new NullableIntRange( value ); }

      public override string ToString() {
         return "NullableIntRange( " + (HasValue ? ("" + Value.min + " to " + Value.max) : "null") + " )";
      }
   }

   [Serializable]
   public struct FloatRange {

      public float min, max;

      public FloatRange( float min, float max ) {

         this.min = min;
         this.max = max;
      }

      public float random { get { return UnityEngine.Random.Range( min, max ); } }

      public bool Contains( float value ) {
         return value >= min && value <= max;
      }

      public override string ToString() {

         return "FloatRange( " + min + " to " + max + " )";
      }

      public string ToShortString() {

         return min + "-" + max;
      }
   }

   [Serializable]
   public class NullableFloatRange: SerializedNullable< FloatRange > {

      public NullableFloatRange( FloatRange  range ): base( range ) { }
      public NullableFloatRange( FloatRange? range ): base( range ) { }
      public NullableFloatRange(): base() { }
      public static implicit operator NullableFloatRange( FloatRange value ) { return new NullableFloatRange( value ); }

      public override string ToString() {
         return "NullableFloatRange( " + (HasValue ? ("" + Value.min + " to " + Value.max) : "null") + " )";
      }
   }
}
