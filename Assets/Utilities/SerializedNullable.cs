// Generic class for a struct value and flag, convertible to and from Nullable< T >
// Concrete subclasses can be serialized by Unity
// by Lexa Francis, 2014-2017

using UnityEngine;
using System;

namespace Lx {

    [Serializable]
    public struct SerializedNullable< T >: IEquatable< SerializedNullable< T > > where T: struct {

        [SerializeField] T    value;
        [SerializeField] bool hasValue;

        public bool HasValue     => hasValue;
        public T    Value        => hasValue ? value : throw new InvalidOperationException();
        public T    BackingValue => value;

        public SerializedNullable( T argument ) => (hasValue, value) = (true, argument);

        public SerializedNullable( T? argument ) {

            hasValue = argument.HasValue;
            value    = argument ?? default;
        }

        public static implicit operator T( SerializedNullable< T > nullable )
            => nullable.hasValue ? nullable.value : throw new InvalidOperationException();

        public static implicit operator SerializedNullable< T >( T argument )
            => new SerializedNullable< T >( argument );

        public static implicit operator T? ( SerializedNullable< T > nullable )
            => nullable.HasValue ? nullable.value : (T?) null;

        public override string ToString() => $"SerializedNullable< {typeof( T ).Name} >" +
                                             $"( {(hasValue ? value.ToString() : "null")} )";

        public bool Equals( SerializedNullable< T > other )
            => (!hasValue && !other.hasValue) || value.Equals( other.value );

        public override bool Equals( object obj ) => obj switch {
            SerializedNullable< T > null_t when !hasValue => !null_t.hasValue,
            SerializedNullable< T > null_t                => value.Equals( null_t.value ),
            T _ when !hasValue                            => false,
            T t                                           => value.Equals( t ),
            _                                             => false
        };

        public override int GetHashCode() => hasValue ? value.GetHashCode() : -int.MaxValue;
    }
}
