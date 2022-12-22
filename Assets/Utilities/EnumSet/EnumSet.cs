/***************************************
 *                                     *
 *   ,_,          EnumSet type         *
 *  (o,o)    by Lexa Francis, 2022     *
 *  {`"'}     Uploaded 2022-12-22      *
 *  -"-"- Tested in 2020.3 and 2022.1  *
 *                                     *
 ***************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Lx {

    
    [Serializable]
    public class EnumSet< TKey, TValue >: IDictionary< TKey, TValue > where TKey: struct, Enum {

        // keys
        TKey[] m_keys;

        protected TKey[] keys {
            get {
                if (m_keys == null || m_keys.Length == 0) { m_keys = (TKey[]) Enum.GetValues( typeof( TKey ) ); }
                return m_keys;
            }
        }
        
        public ICollection< TKey > Keys => Array.AsReadOnly( keys );

        
        // values
#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        TValue[] m_values;

        protected virtual TValue[] values {
            get {
                m_values ??= new TValue[ keys.Length ];
                if (m_values.Length >= keys.Length) { return m_values; }
                var newArray = new TValue[ keys.Length ];
                Array.Copy( m_values, newArray, m_values.Length );
                return m_values = newArray;
            }
        }
        
        public ICollection< TValue > Values => Array.AsReadOnly( values );
        
        
        // other properties
        public bool IsReadOnly { get; }

        public int Count => keys.Length;
        
        
        // indexer
        public TValue this[ TKey key ] {
            get {
                if (!keys.Contains( key )) { throw new ArgumentOutOfRangeException(); }
                return values[ Array.IndexOf( keys, key ) ];
            }
            set {
                if (!keys.Contains( key )) { throw new ArgumentOutOfRangeException(); }
                values[ Array.IndexOf( keys, key ) ] = value;
            }
        }

        
        // static methods
        public static EnumSet< TKey, TValue > Parse( string str, Func< string, TValue > valueParser,
                                                     Func< string, TKey > keyParser=null ) {

            var set = new EnumSet< TKey, TValue >();
            set.Populate( str, valueParser, keyParser );
            return set;
        }

        
        // constructor
        public EnumSet() => IsReadOnly = false;

        
        // other methods
        public void Populate( string str, Func< string, TValue > valueParser, Func< string, TKey > keyParser = null ) {

            foreach (var pairStr in str.Split( ',', ';' )) {

                var  kvp = pairStr.Split( ':', '|', '=' );
                TKey key = keyParser?.Invoke( kvp[ 0 ].Trim() ) ?? (TKey) Enum.Parse( typeof( TKey ), kvp[ 0 ].Trim() );
                values[ Array.IndexOf( keys, key ) ] = valueParser( kvp[ 1 ].Trim() );
            }
        }

        
        // implemented so we can use collection initialiser syntax
        public void Add( TKey key, TValue value ) => this[ key ] = value;

        
        public void Add( KeyValuePair< TKey, TValue > item ) => Add( item.Key, item.Value );

        
        public bool ContainsKey( TKey key ) => keys.Contains( key );
        
        
        public bool Contains( KeyValuePair< TKey, TValue > item )
            => keys.Contains( item.Key ) && this[ item.Key ].Equals( item.Value );

        
        public bool Remove( TKey key ) => throw new ReadOnlyException();

        
        public bool Remove( KeyValuePair< TKey, TValue > item ) => throw new ReadOnlyException();

        
        public void Clear() {
            for (int i = 0; i < keys.Length; i++) { values[ i ] = default; }
        }


        public void CopyTo( KeyValuePair< TKey, TValue >[] array, int index ) {
            
            if (array == null) { throw new ArgumentNullException( nameof( array ) ); }
            if (index < 0 || index > array.Length) { throw new ArgumentOutOfRangeException( nameof( index ) ); }
            if (array.Length - index < Count) { throw new ArgumentException( "Array is too small" ); }
            
            for (int i = 0; i < Count; ++i) {
                array[ index++ ] = new KeyValuePair< TKey, TValue >( keys[ i ], values[ i ] );
            }
        }


        public bool TryGetValue( TKey key, out TValue value ) {
            
            if (keys.Contains( key )) {
                
                value = this[ key ];
                return true; 
            }
            
            value = default;
            return false;
        }

        
        public IEnumerator< KeyValuePair< TKey, TValue > > GetEnumerator()
            => keys.Select( (t, i) => new KeyValuePair< TKey, TValue >( t, values[ i ] ) ).GetEnumerator();

        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    
    // literally just a passthrough for another list since Unity apparently can't serialise SomeGeneric< List< T > > 
    [Serializable]
    public class EnumSetList< TValue >: IList< TValue > {

        [SerializeField] List< TValue > items = new List< TValue >();
        
        // IList implementation
        public int Count => items.Count;

        public TValue this[ int index ] {
            get => items[ index ];
            set => items[ index ] = value;
        }
        
        public void Add( TValue item )                  => items.Add( item );
        public void Clear()                             => items.Clear();
        public bool Contains( TValue item )             => items.Contains( item );
        public void CopyTo( TValue[] array, int index ) => items.CopyTo( array, index );
        public bool Remove( TValue item )               => items.Remove( item );
        public int  IndexOf( TValue item )              => items.IndexOf( item );
        public void Insert( int index, TValue item )    => items.Insert( index, item );
        public void RemoveAt( int index )               => items.RemoveAt( index );

        bool ICollection< TValue >.IsReadOnly => false;
        
        public IEnumerator< TValue > GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.     GetEnumerator() => GetEnumerator();
    }
}