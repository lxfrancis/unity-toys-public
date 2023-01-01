using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class WeightedListExtensions {

    /// <summary>
    /// Get a random item from a list, with the likelihood of each item being proportional to its weight
    /// </summary>
    public static T GetWeightedRandomItem< T >( this IList< T > list ) where T: IWeightedItem {

        float val = Random.Range( 0, list.Sum( item => Mathf.Max( 0.0f, item.Weight ) ) );
        
        foreach (var item in list) {
            
            if (val < item.Weight) { return item; }
            if (item.Weight > 0.0f) { val -= item.Weight; }
        }
        
        return default;
    }

    public static int GetWeightedRandomIndex< T >( this IList< T > list ) where T: IConvertible {

        float val = Random.Range( 0, list.Sum( v => Mathf.Max( 0.0f, Convert.ToSingle( v ) ) ) );
        
        for (int index = 0; index < list.Count; index++) {

            float weight = Convert.ToSingle( list[ 0 ] );
            if (val < weight) { return index; }
            if (val > 0.0f) { val -= weight; }
        }
        
        return -1;
    }

    public static T RandomOrDefault< T >( this IList< T > list ) => list switch {
        null                   => default,
        _ when list.Count == 0 => default,
        _                      => list[ Random.Range( 0, list.Count ) ]
    };

    public static T RandomElement< T >( this IList< T > list ) => list switch {
        null                   => throw new ArgumentNullException(),
        _ when list.Count == 0 => throw new ArgumentException( "RandomOrDefault() called with empty list" ),
        _                      => list[ Random.Range( 0, list.Count ) ]
    };

    public static (TKey key, TValue value) ToValuePair< TKey, TValue >
        ( this string str, Func< string, TKey > keyFunc, Func< string, TValue > valueFunc, TValue fallback=default ) {

        var values = str.Split( ':', '|' );
        return (keyFunc( values[ 0 ].Trim() ), values.Length > 1 ? valueFunc( values[ 1 ].Trim() ) : fallback);
    }

    public static List< T > ToValueList< T >( this string csv, Func< string, T > valueFunc, char divider=',' )
        => csv == "--" ? new List< T >() : csv.Split( divider ).Select( val => valueFunc( val.Trim() ) ).ToList();
}