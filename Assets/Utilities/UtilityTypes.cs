using System;
using UnityEngine;
using static CoroutineUtils;
using static UnityEngine.Debug;



[Serializable]
public struct Motion {

    public static Motion smoothRampUp => new Motion {
        curve    = AnimationCurve.EaseInOut( 0.0f, 0.0f, 1.0f, 1.0f ),
        duration = 1.0f
    };

    public static Motion smoothRampDown => new Motion {
        curve    = AnimationCurve.EaseInOut( 0.0f, 1.0f, 1.0f, 0.0f ),
        duration = 1.0f
    };

    public AnimationCurve curve;
    public float          duration;

    
    public Coroutine Animate( Action< float > interpolation, MonoBehaviour behaviour = null, Action onCompletion = null,
                              float delay = 0.0f )
        => Interpolate( interpolation, duration, curve, onCompletion, behaviour, delay );

    
    public Coroutine Animate( DeltaInterpolation interpolation, MonoBehaviour behaviour = null,
                              Action onCompletion = null, float delay = 0.0f )
        => Interpolate( interpolation, duration, curve, onCompletion, behaviour, delay );
}


[Serializable]
public struct Pair< TLeft, TRight >: IEquatable< Pair< TLeft, TRight > > {

    public TLeft  leftVal;
    public TRight rightVal;

    // boilerplate
    public Pair( TLeft left, TRight right ) => (leftVal, rightVal) = (left, right);

    public bool Equals( Pair< TLeft, TRight > other )
        => leftVal.Equals( other.leftVal ) && rightVal.Equals( other.rightVal );

    public override bool Equals( object obj ) => obj is Pair< TLeft , TRight > other && Equals( other );

    public override int GetHashCode() => unchecked ( (leftVal.GetHashCode() * 397) ^ rightVal.GetHashCode() );

    public static bool operator ==( Pair< TLeft, TRight > left, Pair< TLeft, TRight > right ) =>  left.Equals( right );
    public static bool operator !=( Pair< TLeft, TRight > left, Pair< TLeft, TRight > right ) => !left.Equals( right );
}


[Serializable]
public struct InOutPair< T > { public T @in, @out; }


[Serializable]
public struct DownUpPair< T > { public T down, up; }


[Serializable]
public struct RectTransformPair {

    public struct Data {

        public float      t;
        public Vector2    sizeDelta, pivot,         anchorMin,  anchorMax, anchoredPosition;
        public Vector3    position,  localPosition, lossyScale, localScale;
        public Quaternion rotation,  localRotation;
        public Rect       rect;
    }

    
    public RectTransform a, b;

    
    public Data Interpolate( float t ) {

        return new Data {
            
            t = t,

            sizeDelta        = LerpSizeDelta( t ),
            anchoredPosition = LerpAnchoredPosition( t ),
            position         = LerpPosition( t ),
            localScale       = LerpLocalScale( t ),
            localRotation    = SlerpLocalRotation( t ),
            rect             = LerpRect( t ),

            pivot     = Vector2.Lerp( a.pivot,     b.pivot,     t ),
            anchorMin = Vector2.Lerp( a.anchorMin, b.anchorMin, t ),
            anchorMax = Vector2.Lerp( a.anchorMax, b.anchorMax, t ),

            localPosition = Vector3.Lerp( a.position,   b.position,   t ),
            lossyScale    = Vector3.Lerp( a.lossyScale, b.lossyScale, t ),

            rotation = Quaternion.Slerp( a.rotation, b.rotation, t )
        };
    }

    
    public Vector2 LerpSizeDelta( float t ) => Vector2.Lerp( a.sizeDelta, b.sizeDelta, t );

    public Vector3 LerpPosition( float t ) => Vector3.Lerp( a.position, b.position, t );

    public Vector2 LerpAnchoredPosition( float t ) => Vector2.Lerp( a.anchoredPosition, b.anchoredPosition, t );

    public Vector3 LerpLocalScale( float t ) => Vector3.Lerp( a.localScale, b.localScale, t );

    public Quaternion SlerpLocalRotation( float t ) => Quaternion.Slerp( a.localRotation, b.localRotation, t );

    public Rect LerpRect( float t ) => new Rect( Vector2.Lerp( a.rect.position, b.rect.position, t ),
                                                 Vector2.Lerp( a.rect.size,     b.rect.size,     t ) );
}


public struct RectTransformData {

    public Vector3 localScale;
    public Vector2 anchoredPosition, sizeDelta;

    
    public RectTransformData( RectTransform rtf ) {

        anchoredPosition = rtf.anchoredPosition;
        localScale       = rtf.localScale;
        sizeDelta        = rtf.sizeDelta;
    }

    
    public void SetRectTransform( RectTransform rtf ) {

        rtf.anchoredPosition = anchoredPosition;
        rtf.localScale       = localScale;
        rtf.sizeDelta        = sizeDelta;
    }

    
    public void LerpRectTransform( RectTransform rtf, RectTransform target, float t ) {

        rtf.anchoredPosition = Vector2.Lerp( anchoredPosition, target.anchoredPosition, t );
        rtf.localScale       = Vector3.Lerp( localScale, target.localScale, t );
        rtf.sizeDelta        = Vector2.Lerp( sizeDelta, target.sizeDelta, t );
    }

    
    public static bool operator ==( RectTransformData a, RectTransformData b ) => a.Equals( b );
    public static bool operator !=( RectTransformData a, RectTransformData b ) => !a.Equals( b );

    
    public bool Equals( RectTransformData other ) => localScale.Equals( other.localScale )
                                                  && anchoredPosition.Equals( other.anchoredPosition )
                                                  && sizeDelta.Equals( other.sizeDelta );

    
    public override bool Equals( object obj ) => obj is RectTransformData other && Equals( other );
    
    
    public override int GetHashCode() {
        unchecked {
            var hashCode = localScale.GetHashCode();
            hashCode = (hashCode * 397) ^ anchoredPosition.GetHashCode();
            hashCode = (hashCode * 397) ^ sizeDelta.GetHashCode();
            return hashCode;
        }
    }
}


[Serializable]
public struct RectTransformMotion {

    public RectTransform  rectTransform;
    public AnimationCurve curve;
    public float          duration;

    public RectTransformData homeState { get; private set; }

    
    public void Init() => homeState = new RectTransformData( rectTransform );

    
    public void Reset() => homeState.SetRectTransform( rectTransform );

    
    public Coroutine AnimateToTarget( RectTransform target, MonoBehaviour behaviour = null,
                                      Action onCompletion = null, float delay = 0.0f ) {

        if (homeState == default) {
            LogWarning( "Possible uninitialised RectTransformMotion!" );
        }
        
        RectTransformMotion rtm = this;
        
        return Interpolate( t => rtm.homeState.LerpRectTransform( rtm.rectTransform, target, t ),
                                                          rtm.duration, rtm.curve, onCompletion, behaviour, delay );
    }

    
    public Coroutine Animate( Action< float > interpolation, MonoBehaviour behaviour = null,
                              Action onCompletion = null, float delay = 0.0f )
        => Interpolate( interpolation, duration, curve, onCompletion, behaviour, delay );

    
    public Coroutine Animate( DeltaInterpolation interpolation, MonoBehaviour behaviour = null,
                              Action onCompletion = null, float delay = 0.0f )
        => Interpolate( interpolation, duration, curve, onCompletion, behaviour, delay );
}