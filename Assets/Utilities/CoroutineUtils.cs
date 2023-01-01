using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static CoroutineUtils;
using static UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif


// TODO: incorporate into utils
[SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" )]
[SuppressMessage( "ReSharper", "UnassignedReadonlyField" )]
[SuppressMessage( "ReSharper", "UnusedAutoPropertyAccessor.Global" )]
public class InterpolationRoutine {

    public readonly Coroutine     coroutine;
    public readonly MonoBehaviour owner;
    
    public Action             onCompletion;
    public Action< float >    interpolation;
    public DeltaInterpolation deltaInterpolation;
    
    public bool running { get; private set; }

    
    public InterpolationRoutine( MonoBehaviour owner, Action onCompletion ) {

        this.owner        = owner;
        this.onCompletion = onCompletion;
    }

    
    /// <summary>
    /// Halts the interpolation where it is
    /// </summary>
    /// <param name="runCompletionAction">Whether to run the explicit completion action, if there is one</param>
    public void Stop( bool runCompletionAction ) {

        owner.StopCoroutine( coroutine );
        running = false;
        
        if (runCompletionAction) { onCompletion?.Invoke(); }
    }

    
    /// <summary>
    /// Completes the interpolation instantly with a t-value of 1.0
    /// </summary>
    public void Complete() {

        owner.StopCoroutine( coroutine );
        interpolation( 1.0f );
        onCompletion?.Invoke();
        running = false;
    }
}


public class CoroutineUtils: AutoMonoSingleton< CoroutineUtils > {

    enum WaitType { Frame, FrameEnd, Seconds, Condition };

    
    public static Coroutine OnNextFrame( Action action, MonoBehaviour behaviour = null ) =>
        VerifyBehaviour( behaviour, "OnNextFrame" ).StartCoroutine( Wait( WaitType.Frame, action ) );

    
    public static Coroutine OnNextFrame< T >( Func< T > action, MonoBehaviour behaviour = null ) =>
        VerifyBehaviour( behaviour, "OnNextFrame" ).StartCoroutine( Wait( WaitType.Frame, action ) );

    
    public static Coroutine OnFrameEnd( Action action, MonoBehaviour behaviour = null ) =>
        VerifyBehaviour( behaviour, "OnFrameEnd" ).StartCoroutine( Wait( WaitType.FrameEnd, action ) );

    
    public static Coroutine OnFrameEnd< T >( Func< T > action, MonoBehaviour behaviour = null ) =>
        VerifyBehaviour( behaviour, "OnFrameEnd" ).StartCoroutine( Wait( WaitType.FrameEnd, action ) );

    
    public static Coroutine Delay( Action action, float delay, MonoBehaviour behaviour = null,
                                   bool realtime = false ) =>
        VerifyBehaviour( behaviour, "Delay" )
            .StartCoroutine( Wait( WaitType.Seconds, action, seconds: delay, unscaledTime: realtime ) );

    
    public static Coroutine Delay< T >( Func< T > action, float delay, MonoBehaviour behaviour = null,
                                        bool realtime = false ) =>
        VerifyBehaviour( behaviour, "Delay" )
            .StartCoroutine( Wait( WaitType.Seconds, action, seconds: delay, unscaledTime: realtime ) );

    
    public static Coroutine OnCondition( Func< bool > condition, Action action, MonoBehaviour behaviour = null ) =>
        VerifyBehaviour( behaviour, "OnCondition" ).StartCoroutine( Wait( WaitType.Condition, action, condition ) );

    
    public static Coroutine OnCondition< T >( Func< bool > condition, Func< T > action,
                                              MonoBehaviour behaviour = null ) =>
        VerifyBehaviour( behaviour, "OnCondition" ).StartCoroutine( Wait( WaitType.Condition, action, condition ) );


    [SuppressMessage( "ReSharper", "IteratorNeverReturns" )]
    public static Coroutine RunEveryFrame( Action action, MonoBehaviour behaviour=null, bool thisFrame=true,
                                           bool endOfFrame=false ) {
        
        return VerifyBehaviour( behaviour, "RunEveryFrame" ).StartCoroutine( Routine() );

        IEnumerator Routine() {

            if (!thisFrame) { yield return endOfFrame ? new WaitForEndOfFrame() : null; }

            while (true) {
                
                action();
                yield return null;
                if (endOfFrame) { yield return new WaitForEndOfFrame(); }
            }
        }
    }

    
    public static Coroutine StaggerActions( IEnumerable< Action > actions, MonoBehaviour behaviour = null ) {
        
        return VerifyBehaviour( behaviour, "StaggerActions" ).StartCoroutine( Routine() );

        IEnumerator Routine() {

            foreach (var action in actions) {
                
                yield return null;
                action?.Invoke();
            }
        }
    }

    
    public static Coroutine StaggerActions( int count, Action< int > repeatedAction,
                                            MonoBehaviour behaviour = null ) {
        
        return VerifyBehaviour( behaviour, "StaggerActions" ).StartCoroutine( Routine() );

        IEnumerator Routine() {
            
            for (int i = 0; i < count; i++) {
                
                yield return null;
                repeatedAction( i );
            }
        }
    }

    
    public delegate void StaggeredAction< in T >( T item, int index );

    
    public static Coroutine StaggerActions< T >( IEnumerable< T > collection, StaggeredAction< T > action,
                                                 MonoBehaviour behaviour = null ) {
        
        return VerifyBehaviour( behaviour, "StaggerActions" ).StartCoroutine( Routine() );

        IEnumerator Routine() {

            int index = 0;

            foreach (var item in collection) {
                
                yield return null;
                action( item, index );
                index++;
            }
        }
    }

    
    static IEnumerator Wait( WaitType type, Action action, Func< bool > condition = null, float seconds = -1,
                             bool unscaledTime = false ) {

        if (action == null) {

            LogError( "CoroutineUtils called with null action" );
            yield break;
        }

        if (type == WaitType.Condition) {

            if (condition == null) {
                
                LogError( "CoroutineUtils called with null condition" );
                yield break;
            }

            if (condition()) {
                
                action();
                yield break;
            }
        }

        if (type == WaitType.Seconds && seconds <= 0.0f) {

            action();
            yield break;
        }

        yield return type switch {
            WaitType.Frame                     => null,
            WaitType.FrameEnd                  => new WaitForEndOfFrame(),
            WaitType.Seconds when unscaledTime => new WaitForSecondsRealtime( seconds ),
            WaitType.Seconds                   => new WaitForSeconds( seconds ),
            WaitType.Condition                 => new WaitUntil( condition ),
            _                                  => throw new ArgumentOutOfRangeException( nameof( type ), type, null )
        };

        action();
    }

    
    static IEnumerator Wait< T >( WaitType type, Func< T > action, Func< bool > condition = null,
                                  float seconds = -1, bool unscaledTime = false ) {

        if (action == null) {

            LogError( "CoroutineUtils called with null action" );
            yield break;
        }

        if (type == WaitType.Condition) {

            if (condition == null) {
                LogError( "CoroutineUtils called with null condition" );
                yield break;
            }

            if (condition()) {
                
                action();
                yield break;
            }
        }

        if (type == WaitType.Seconds && seconds <= 0.0f) {

            action();
            yield break;
        }

        yield return type switch {
            WaitType.Frame                     => null,
            WaitType.FrameEnd                  => new WaitForEndOfFrame(),
            WaitType.Seconds when unscaledTime => new WaitForSecondsRealtime( seconds ),
            WaitType.Seconds                   => new WaitForSeconds( seconds ),
            WaitType.Condition                 => new WaitUntil( condition ),
            _                                  => throw new ArgumentOutOfRangeException( nameof( type ), type, null )
        };

        action();
    }


    static MonoBehaviour VerifyBehaviour( MonoBehaviour behaviour, string methodName ) {
        
        var useBehaviour = behaviour;

        if (!behaviour) {
            useBehaviour = instance;
        }
        else if (!behaviour.gameObject.activeInHierarchy) {
            
            LogWarning( $"CoroutineUtils.{methodName} called on inactive behaviour {behaviour}; " +
                        "using CoroutineUtils instance instead", behaviour );
            
            useBehaviour = instance;
        }

        if (useBehaviour == instance && !instance.isActiveAndEnabled) {
            
            instance.gameObject.SetActive( true );
            instance.enabled = true;
        }

        return useBehaviour;
    }

    
    public static Coroutine Interpolate( Action< float > interpolation, float duration, AnimationCurve curve = null,
                                         Action onCompletion = null, MonoBehaviour behaviour = null,
                                         float delay = 0.0f, bool unscaledTime = false ) {

        if (interpolation == null) {

            LogError( $"CoroutineUtils.Interpolate() called on {behaviour} with null interpolation callback",
                      behaviour );
            return null;
        }

        if (duration <= 0.0f) {

            LogWarning( $"CoroutineUtils.Interpolate() called on {behaviour} with non-positive duration",
                        behaviour );
            interpolation( curve?.Evaluate( 1.0f ) ?? 1.0f );
            onCompletion?.Invoke();
            return null;
        }

        return VerifyBehaviour( behaviour, "Interpolate" ).StartCoroutine( InterpolateRoutine() );
        
        IEnumerator InterpolateRoutine() {
            
            if (delay > 0.0f) {
                yield return unscaledTime ? new WaitForSecondsRealtime( delay ) : (object) new WaitForSeconds( delay );
            }

            float startTime = Time.time;

            while (true) {

                float t = Mathf.Clamp01( (Time.time - startTime) / duration );
                interpolation( curve?.Evaluate( t ) ?? t );

                if (t >= 1.0f) { break; }
                yield return null;
            }

            onCompletion?.Invoke();
        }
    }

    
    public delegate void DeltaInterpolation( float t, float delta );

    
    public static Coroutine Interpolate( DeltaInterpolation interpolation, float duration,
                                         AnimationCurve curve = null, Action onCompletion = null,
                                         MonoBehaviour behaviour = null, float delay = 0.0f,
                                         bool unscaledTime = false ) {
        
        if (interpolation == null) {

            LogError( $"CoroutineUtils.Interpolate() called on {behaviour} with null delta interpolation callback",
                      behaviour );
            return null;
        }

        if (duration <= 0.0f) {

            LogWarning( $"CoroutineUtils.Interpolate() called on {behaviour} with non-positive duration",
                        behaviour );
            float curved = curve?.Evaluate( 1.0f ) ?? 1.0f;
            interpolation( curved, 0.0f );
            onCompletion?.Invoke();
            return null;
        }

        return VerifyBehaviour( behaviour, "Interpolate" ).StartCoroutine( InterpolateDeltaRoutine() );
        
        IEnumerator InterpolateDeltaRoutine() {

            if (delay > 0.0f) {
                yield return unscaledTime ? new WaitForSecondsRealtime( delay ) : (object) new WaitForSeconds( delay );
            }

            float startTime = Time.time;
            float lastValue = 0.0f;

            while (true) {

                float t      = Mathf.Clamp01( (Time.time - startTime) / duration );
                float curved = curve?.Evaluate( t ) ?? t;
                interpolation( curved, curved - lastValue );
                lastValue = curved;

                if (t >= 1.0f) { break; }
                yield return null;
            }

            onCompletion?.Invoke();
        }
    }

    
    // TODO: stopping the coroutine that this method returns doesn't stop the routine that the user passes in
    // find some way to make it yield on the user's routine so it works as expected
    public static Coroutine SkippableRoutine( Func< IEnumerator > routine, Action skipAction,
                                              Func< bool > skipWhenTrue, MonoBehaviour behaviour = null,
                                              bool runSkipActionOnComplete = false,
                                              bool allowSkipOnInitialFrame = false ) {

        var         owner       = behaviour ? behaviour : instance;
        bool        finished    = false;
        IEnumerator mainRoutine = null;

        var skippable = owner.StartCoroutine( Skippable() );
        
        owner.StartCoroutine( WaitRoutine() );
        
        return skippable;

        IEnumerator WaitRoutine() {
            
            yield return mainRoutine = routine();
            finished = true;
        }

        IEnumerator Skippable() {

            while (!finished) {
                
                if (!allowSkipOnInitialFrame) { yield return null; }
                
                if (skipWhenTrue()) {
                
                    owner.StopCoroutine( mainRoutine );
                    skipAction?.Invoke();
                    yield break;
                }

                if (allowSkipOnInitialFrame) { yield return null; }
            }
            
            if (runSkipActionOnComplete) { skipAction?.Invoke(); }
        }
    }

    
    public static Coroutine CoroutineTest( Func< IEnumerator > routine, Action skipAction, Func< bool > skipWhenTrue,
                                           bool runSkipActionOnComplete = false,
                                           bool allowSkipOnInitialFrame = false ) {

        bool        finished    = false;
        IEnumerator mainRoutine = null;
        
        LogFrame( "CoroutineTest() start" );
        
        var skippable = instance.StartCoroutine( Skippable() );
        
        instance.StartCoroutine( WaitRoutine() );

        return skippable;

        IEnumerator WaitRoutine() {
            
            LogFrame( "CoroutineTest()::WaitRoutine() start, running routine parameter directly without StartCoroutine()" );
            yield return mainRoutine = routine();
            finished = true;
            LogFrame( "CoroutineTest()::WaitRoutine() finished" );
        }

        IEnumerator Skippable() {
        
            LogFrame( "CoroutineTest()::Skippable() start" );

            while (!finished) {
                
                LogFrame( "CoroutineTest()::Skippable() in loop" );
                
                if (!allowSkipOnInitialFrame) { yield return null; }
                
                if (skipWhenTrue()) {
                    
                    LogFrame( "CoroutineTest()::Skippable() about to stop mainRoutine" );
                    instance.StopCoroutine( mainRoutine );
                    LogFrame( "CoroutineTest()::Skippable() about to run skipAction" );
                    skipAction();
                    LogFrame( "CoroutineTest()::Skippable() has run skipAction" );
                    yield break;
                }

                if (allowSkipOnInitialFrame) { yield return null; }
            }

            if (runSkipActionOnComplete) {
                
                LogFrame( "CoroutineTest()::Skippable(); about to run skip action at natural end of routine" );
                skipAction();
            }
        
            LogFrame( "CoroutineTest()::Skippable() finish" );
        }
    }

    
    static void LogFrame( string message ) => Log( $"<color=red>[{Time.frameCount}]</color> {message}" );

    
#if UNITY_EDITOR
    [MenuItem( "MathCircus/Test Coroutines", isValidateFunction: true )]
    public static bool CanTestCoroutines() => Application.isPlaying;
    
    [MenuItem( "MathCircus/Test Coroutines")]
#endif
    public static void TestCoroutineMoo() {

        int startFrame = Time.frameCount;
        LogFrame( "TestCoroutineMoo()" );

        Coroutine moo = CoroutineTest( routine, SkipActionMethod, () => Time.frameCount == startFrame + 6, true );

        void SkipActionMethod() {

            LogFrame( "TestCoroutineMoo()::SkipActionMethod()" );
        }

        OnCondition( () => Time.frameCount == startFrame + 3, () => {
            LogFrame( "TestCoroutineMoo()::OnCondition action; about to stop moo here" );
            instance.StopCoroutine( moo );
        } );

        IEnumerator routine() {
            
            LogFrame( "TestCoroutineMoo()::routine start" );

            for (int i = 0; i < 10; i++) {
                
                LogFrame( $"moo: {i}; frame == startFrame + 6? " +
                          $"{Time.frameCount == startFrame + 6}" );
                yield return null;
            }
            
            LogFrame( "TestCoroutineMoo()::routine finished" );
        }
    }

    
    /// <summary>
    /// Repeats attempting action until it does not throw an exception.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="maxTries"></param>
    /// <param name="log"></param>
    /// <returns>Whether the action was ultimately successful.</returns>
    public static bool TryRepeatedly( Action action, int maxTries, bool log = false ) {

        int tries = 0;

        while (tries++ < maxTries) {

            try {
                action();
                if (log) { Log( $"Success after {tries} times" ); }
                return true;
            }
            catch (Exception e) {
                LogException( e );
            }
        }

        if (log) { LogError( $"Tried action {maxTries} times; failed" ); }
        return false;
    }

    
    /// <summary>
    /// Repeats attempting function, logging any exceptions caught, until it returns true.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="maxTries"></param>
    /// <param name="log"></param>
    /// <returns>Whether the action ultimately returned true.</returns>
    public static bool TryRepeatedly( Func< bool > function, int maxTries, bool log = false ) {

        int tries = 0;

        while (tries++ < maxTries) {

            try {
                if (!function()) { continue; }
                if (log) { Log( $"Success after {tries} times" ); }
                return true;
            }
            catch (Exception e) {
                LogWarning( e );
            }
        }

        if (log) { LogError( $"Tried function {maxTries} times; failed" ); }
        return false;
    }

    
    /// <summary>
    /// Repeats attempting function, logging any exceptions caught, until it does not return null.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="maxTries"></param>
    /// <param name="log"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The first non-null return value of function, otherwise null.</returns>
    public static T TryRepeatedly< T >( Func< T > function, int maxTries, bool log = false ) where T: class {

        int tries = 0;

        while (tries++ < maxTries) {

            try {
                T result;
                if ((result = function()) == null) { continue; }
                if (log) { Log( $"Success after {tries} times" ); }
                return result;
            }
            catch (Exception e) {
                LogException( e );
            }
        }

        if (log) { LogError( $"Tried function {maxTries} times; failed" ); }
        return null;
    }
}