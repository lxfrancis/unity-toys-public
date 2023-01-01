using System;
using UnityEngine;
using static System.Globalization.CultureInfo;

/// <summary>
/// An item in a collection that has an associated randomness weight, for biasing random selection
/// </summary>
public interface IWeightedItem {

    public float Weight { get; }
}

[Serializable]
public struct WeightedItem< T >: IWeightedItem {

    public T value;

    [SerializeField] float weight;

    public float Weight => weight;
        
    public WeightedItem( T item, float weight ) {
        
        (value, this.weight) = (item, weight);
        CheckType();
    }

    public WeightedItem( string cellData, Func< string, T > itemSelector ) {
                
        (value, weight) = cellData.ToValuePair( itemSelector, token => float.Parse( token, InvariantCulture ) );
        weight          = Math.Max( 0.0f, weight );
        
        CheckType();
    }

    void CheckType() {
        
        if (typeof( IWeightedItem ).IsAssignableFrom( typeof( T ) )) {
            
            Debug.LogWarning( $"Constructing WeightedItem< {typeof( T ).Name} > " +
                              $"but {typeof( T ).Name} already implements IWeightedItem" );
        }
    }
}