using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSwitch: MonoBehaviour {

   internal string layer;

   public void Activate() {

      HilbertMap.instance.ShowLayer( layer );
   }
}
