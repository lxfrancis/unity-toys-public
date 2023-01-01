using System;
using UnityEngine;
using UnityEngine.UI;
using static AnimatedFill.Mode;

public class AnimatedFill: MonoBehaviour {

    public enum Mode { AnimateFresh, SoftAnimate, SoftSnap, HardSnap };

    public Image          image;
    public float          duration;
    public AnimationCurve curve;
    public Mode           defaultMode;

    Coroutine routine;

    public bool  animating     => routine != null;
    public float animatedValue => image.fillAmount;

    public void SetFill( float value, Mode? mode =null ) {

        fill =   value;
        mode ??= defaultMode;

        switch (mode.Value) {
            
            case AnimateFresh:
                
                float startValue = image.fillAmount;
                
                if (!Application.isPlaying || enabled == false || !gameObject.activeInHierarchy) {
                    
                    if (!animating) { image.fillAmount = value; }
                    return;
                }
                
                StopAllCoroutines();
                routine = CoroutineUtils.Interpolate( t => image.fillAmount = Mathf.Lerp( startValue, fill, t ),
                                                      duration, curve, () => routine = null, this );
                break;
            
            case SoftAnimate:
                
                SetFill( value, animating ? SoftSnap : AnimateFresh );
                break;
            
            case SoftSnap:
                
                if (!animating) { image.fillAmount = value; }
                break;
            
            case HardSnap:
                
                StopAllCoroutines();
                routine          = null;
                image.fillAmount = value;
                break;
            
            default:
                throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
        }
    }

    [Range( 0.0f, 1.0f )]
    [SerializeField] float fill;
    public float Fill {
        get => fill;
        set => SetFill( value );
    }

    void OnValidate() {
        
        image ??= GetComponent< Image >();
        SetFill( fill );
    }
}
