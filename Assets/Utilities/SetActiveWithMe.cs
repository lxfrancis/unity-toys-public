using UnityEngine;

namespace Lx {

    public class SetActiveWithMe: MonoBehaviour {

        public GameObject[] objectsToSet;

        void OnEnable() {
            
            foreach (var go in objectsToSet) { go.SetActive( true ); }
        }

        void OnDisable() {
            
            foreach (var go in objectsToSet) { go.SetActive( false ); }
        }
    }

}