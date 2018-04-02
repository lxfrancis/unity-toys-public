// Generic class for a struct value and flag, convertible to and from Nullable< T >
// Concrete subclasses can be serialized by Unity
// by Lexa Francis, 2014-2017

using UnityEngine;
using System;

namespace Lx {

   [Serializable]
   public class SerializedNullable< T > where T: struct {

      [SerializeField] protected T    value;
      [SerializeField] protected bool hasValue;

      public bool HasValue { get { return hasValue; } }

      public T Value {
         get {
            if (hasValue) { return value; }
            throw new InvalidOperationException();
         }
      }

      public T BackingValue { get { return value; } }

      public SerializedNullable() { hasValue = false; }

      public SerializedNullable( T argument ) {

         hasValue = true;
         value    = argument;
      }

      public SerializedNullable( T? argument ) {

         hasValue = argument.HasValue;
         if (hasValue) { value = argument.Value; }
      }

      public static implicit operator T( SerializedNullable< T > nullable ) {

         if (nullable.hasValue) { return nullable.value; }
         throw new InvalidOperationException();
      }

      public static implicit operator SerializedNullable< T >( T argument ) {
         return new SerializedNullable< T >( argument );
      }

      public static implicit operator T? ( SerializedNullable< T > nullable ) {
         return nullable.HasValue ? nullable.value : (T?) null;
      }

      public override string ToString() {

         if (!HasValue) { return "null"; }
         return value.ToString();
      }
   }

   [Serializable]
   public class NullableInt: SerializedNullable< int > {

      public NullableInt( int  val ): base( val ) { }
      public NullableInt( int? val ): base( val ) { }
      public NullableInt(): base() { }
      public static implicit operator NullableInt( int value ) { return new NullableInt( value ); }
   }

   [Serializable]
   public class NullableFloat: SerializedNullable< float > {

      public NullableFloat( float  val ): base( val ) { }
      public NullableFloat( float? val ): base( val ) { }
      public NullableFloat(): base() { }
      public static implicit operator NullableFloat( float value ) { return new NullableFloat( value ); }
   }

   [Serializable]
   public class NullableColor: SerializedNullable< Color > {

      public NullableColor( Color  color ): base( color ) { }
      public NullableColor( Color? color ): base( color ) { }
      public NullableColor(): base() { }
      public static implicit operator NullableColor( Color value ) { return new NullableColor( value ); }

      public override string ToString() {
         return "NullableColor( " + (HasValue ? value.ToString() : "null") + " )";
      }
   }
}