using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ValueAnimator: MonoBehaviour {

    [Serializable]
    public class Target {

        public UnityEvent< float > action;
        public float               amplitude, baseline, endValue;
    }

    public AnimationCurve curve;
    public float          duration;
    public List< Target > targets;

    public void Stop() {

        StopAllCoroutines();

        foreach (var target in targets) { target.action.Invoke( target.endValue ); }
    }
    
    public void Animate() {

        StopAllCoroutines();

        CoroutineUtils.Interpolate( t => {
            foreach (var target in targets) { target.action.Invoke( target.baseline + t * target.amplitude ); }
        }, duration, curve, () => {
            foreach (var target in targets) { target.action.Invoke( target.endValue ); }
        }, this );
    }
}
