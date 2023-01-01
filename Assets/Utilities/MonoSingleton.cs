using System;
using UnityEngine;

public abstract class MonoSingleton< T >: MonoBehaviour where T: MonoSingleton< T > {

    static T _instance;

    public static T instance => _instance = _instance ? _instance : FindObjectOfType< T >( false )
                                                                    ?? FindObjectOfType< T >( true );
}


public abstract class InitialisedSingleton< T >: MonoBehaviour where T: InitialisedSingleton< T > {

    static T       _instance;
    public static T instance => _instance ? _instance.needsInitialising ? _instance.Init()
                                                                        : _instance
                                          : _instance = FindObjectOfType< T >()?.Init();

    protected static T uncheckedInstance;

    public static void Uninitialise() => _instance = uncheckedInstance = null;

    protected bool initialising { get; private set; }

    protected bool initialised { get; private set; }

    protected abstract void OnInitialise();

    protected abstract bool InitialisationCheck();

    protected bool needsInitialising {
        get {
            if (initialising) {
                throw new InvalidOperationException( $"Cannot access instance of {typeof( T ).Name} " +
                                                     "during instance initialisation" );
            }
            if (!InitialisationCheck()) { initialised = false; }
            return !initialised;
        }
    }

    protected void OnValidate() {

        initialising = false;
    }

    protected void Awake() {

        Debug.Log( $"InitialisedSingleton.Awake(); _instance: {(_instance ? "exists" : "null")}; " +
                   // $"current active scene: {Scene.active}" +
                   (_instance ? $"_instance == this? {_instance == this}; " +
                                $"_instance scene name: {_instance.gameObject.scene.name}" : ""), _instance );

        // allow for initialised singletons to set themselves to DontDestroyOnLoad and persist,
        // avoiding reinitialisation in every scene
        if (_instance && _instance != this && _instance.gameObject.scene.name == "DontDestroyOnLoad") {
            
            Destroy( gameObject );
            return;
        }

        if (needsInitialising) { Init(); }
    }

    protected T Init() {

        if (!needsInitialising) { return (T) this; }

        if (initialising) { throw new InvalidOperationException( "Recursive singleton initialisation" ); }

        initialised       = true;
        initialising      = true; // protects against recursive call causing a stack overflow
        uncheckedInstance = (T) this;

        try {
            OnInitialise();
        }
        catch (Exception) {

            Debug.LogError( $"Could not initialise {typeof( T ).Name} singleton; rethrowing exception." );
            throw;
        }
        finally {
            initialising = false;
        }

        _instance = (T) this;
        
        return (T) this;
    }
}
