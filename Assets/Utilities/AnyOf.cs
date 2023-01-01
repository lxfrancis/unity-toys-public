// Lexa Francis, 2022

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lx {


    public static class Any {

        public static Any< T >.AnyOf Of< T >( params T[] possibilities ) => Any< T >.Of( possibilities );

        public static Any< T >.AnyOf Of< T >( IEnumerable< T > possibilities ) => Any< T >.Of( possibilities );
    }
    
    
    public static class Any< T > {

        static Any() => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( AnyOf ).TypeHandle );

        internal static AnyOf Of( params T[]       possibilities ) => paramsConstructorFunc    ( possibilities );
        internal static AnyOf Of( IEnumerable< T > possibilities ) => enumerableConstructorFunc( possibilities );

        static Func< T[], AnyOf >              paramsConstructorFunc;
        static Func< IEnumerable< T >, AnyOf > enumerableConstructorFunc;

        
        public readonly struct AnyOf: IEquatable< T >, IEquatable< AnyOf > {

            static AnyOf() => paramsConstructorFunc = possibilities => new AnyOf( possibilities );

            readonly T[] array;

            AnyOf( params T[] possibilities ) => array = possibilities;

            public bool Equals( AnyOf anyOther )
                => array.Any( thing => anyOther.array.Any( otherThing => thing.Equals( otherThing ) ) );

            public bool Equals( T otherThing ) => array.Any( thing => thing.Equals( otherThing ) );

            public override bool Equals( object obj ) => obj switch {
                T thing     => Equals( thing ),
                AnyOf other => Equals( other ),
                _           => false
            };


            public static bool operator ==( AnyOf left, AnyOf right )
                => left.array.Any( thing => right.array.Any( otherThing => thing.Equals( otherThing ) ) );
         
            public static bool operator !=( AnyOf left, AnyOf right )
                => !left.array.Any( thing => right.array.Any( otherThing => thing.Equals( otherThing ) ) );

            public static bool operator ==( T thing, AnyOf any ) =>  any.array.Any( t => t.Equals( thing ) );
            public static bool operator !=( T thing, AnyOf any ) => !any.array.Any( t => t.Equals( thing ) );

            public static bool operator ==( AnyOf any, T thing ) =>  any.array.Any( t => t.Equals( thing ) );
            public static bool operator !=( AnyOf any, T thing ) => !any.array.Any( t => t.Equals( thing ) );
            
            public override int GetHashCode() => array?.GetHashCode() ?? 0;
        }
    }
}
