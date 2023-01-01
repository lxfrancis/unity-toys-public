using UnityEngine;

public class AutoMonoSingleton< T >: MonoBehaviour where T: AutoMonoSingleton< T > {

    static T _instance;
    
    public static T instance {
        get {
            _instance = _instance ? _instance : FindObjectOfType< T >( true );
            if (_instance && !_instance.gameObject.activeSelf) { _instance.gameObject.SetActive( true ); }
            if (!_instance) {
                _instance = new GameObject( typeof( T ).Name ).AddComponent< T >();
                if (Application.isPlaying) { DontDestroyOnLoad( _instance ); }
            }
            return _instance;
        }
    }
}