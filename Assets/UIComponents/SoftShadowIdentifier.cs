using System;
using System.Collections;
using System.Linq;
using Lx;
using UnityEngine;
using Object = UnityEngine.Object;

// TODO: re-enable all logs and look at why these things destroy themselves more often than intended
// seems to be something related to being in a prefab instance
[ExecuteAlways]
public class SoftShadowIdentifier: MonoBehaviour {

    internal SoftShadow mainComponent;

    bool beingDestroyed;

    
    void Log( string message, Object context=null ) {

        if (!SoftShadow.log) { return; }
        
        Debug.Log( message, context );
    }

    
    void NextFrameCheck() {

        if (beingDestroyed
            || (mainComponent && mainComponent.identifier == this)
            || SoftShadow.instantiating) {
            
            return;
        }
        
        var owner = FindObjectsOfType< SoftShadow >().FirstOrDefault( ss => ss.identifier == this );

        if (owner) {
                
            mainComponent = owner;
                
            if (!Application.isPlaying) {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty( gameObject );
#endif
            }
            
            return;
        }

        bool alreadyDestroyed = true;
        var  destroyLog       = "SoftShadowIdentifier already destroyed; pointing to owner";

        if (this) {
            alreadyDestroyed = false;
            destroyLog = $"SoftShadowIdentifier destroying self. path: {transform.Path()}; " +
                         $"main component? {mainComponent}; " +
                         $"main component identifier is this? {mainComponent && mainComponent.identifier == this}; " +
                         $"instantiating? {SoftShadow.instantiating}";
        }

        if (alreadyDestroyed) {
            
            Log( destroyLog, owner );
            return;
        }


        if (!Application.isPlaying) {
#if UNITY_EDITOR
            if (!UnityEditor.PrefabUtility.IsPartOfPrefabInstance( gameObject )) {
                    
                Log( destroyLog );
                if (gameObject) { DestroyImmediate( gameObject ); }
            }
#endif
        }
        else if (gameObject) {
                    
            // Debug.Log( destroyLog );
            Destroy( gameObject );
        }
    }

    
    static IEnumerator WaitOneFrame( Action action ) {

        yield return null;
        action();
    }

    
    public void RunNextFrameCheck() {

        if (!Application.isPlaying) {
            
#if UNITY_EDITOR
            Unity.EditorCoroutines.Editor.EditorCoroutineUtility
                 .StartCoroutineOwnerless( WaitOneFrame( NextFrameCheck ) );
            return;
#endif
        }

        CoroutineUtils.OnNextFrame( NextFrameCheck );
    }

    public void SelfDestruct() {

        if (beingDestroyed) { return; }
        
        if (!Application.isPlaying) { DestroyImmediate( gameObject ); }
        else { Destroy( gameObject ); }
    }


    void OnEnable() => RunNextFrameCheck();

    
    void OnDestroy() => beingDestroyed = true;
}
