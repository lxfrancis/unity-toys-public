// Miscellaneous C# (Unity-compatible) utility methods
// by Lexa Francis, 2014-2017

using System.Linq;
using System.Reflection;
using System;

namespace Lx {

   /// <summary>
   /// A giant mass of utility methods and extension methods.</summary>
   public static partial class Utils {

      public static string NullChainCheck( string n, object obj, params string[] invocations ) {

         string str     = $"{n.Bold()} invocation chain:\nstarting with:{obj}";
         object current = obj;

         foreach (string invocation in invocations) {

            if (current == null) {

               str += $"\n at invocation '{invocation}'; current is null; breaking...";
               break;
            }

            Type               currType     = current.GetType();
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic
                                            | BindingFlags.Instance | BindingFlags.Static;

            FieldInfo field = currType.GetField( invocation, bindingFlags );

            if (field != null) {

               current =  field.GetValue( current );
               str     += $"\n{(invocation + " (field): ").Bold()}{(current != null ? current.ToString() : "null")}";
               continue;
            }

            MethodInfo method = currType.GetMethod( invocation, bindingFlags );

            if (method != null) {

               current =  method.Invoke( current, new object[] { } );
               str     += $"\n{(invocation + " (method): ").Bold()}{(current != null ? current.ToString() : "null")}";
               continue;
            }

            PropertyInfo property = currType.GetProperty( invocation, bindingFlags );

            if (property != null) {

               current =  property.GetValue( current, null );
               str     += $"\n{(invocation + " (property): ").Bold()}{(current != null ? current.ToString() : "null")}";
               continue;
            }
            
            str += $"\n at invocation '{invocation}'; no method or property found on type '{currType}'; breaking...";
            break;
         }
         
         return str;
      }

      public class SwitchMap< Tkey, Tresult > {

         Tkey    input;

         internal SwitchMap( Tkey input ) { this.input = input; }

         public static implicit operator Tresult( SwitchMap< Tkey, Tresult > map ) { return map.Value; }

         public SwitchMap< Tkey, Tresult > Map( Tkey key, Tresult output ) {

            if (input.Equals( key )) { Value = output; }
            return this;
         }

         public Tresult Value { get; private set; }
      }

      public static SwitchMap< T, U > Map< T, U >( this T input, T key, U output ) {

         var dict = new SwitchMap< T, U >( input );
         return dict.Map( key, output );
      }

      public static T[] EnumValues< T >() {
         return Enum.GetValues( typeof( T ) ).Cast< T >().ToArray();
      }

      public static TResult To< TSource, TResult >( this TSource source, Func< TSource, TResult > operation ) {

         return operation( source );
      }

      static readonly int[] primes = { 17, 23, 31, 37, 43, 19, 29, 41, 59, 73 };

      public static int IntHash( params int[] values ) {

         int hash = 73;
         for (int i = 0; i < values.Length; i++) {
            hash = hash * primes[ (i * 2) % 10 ] + values[ i ] + primes[ (i * 2 + 1) % 10 ];
         }
         return hash;
      }
   }
}
