using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GroupElementListing: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

   internal HilbertMap.Element element;
   internal string             group, layer;

   Text _text;
   public Text text { get { return _text = _text ?? GetComponentInChildren< Text >(); } } 

   public void OnPointerEnter( PointerEventData pointerData ) {

      if (element != null) { HilbertMap.instance.ShowElement( element ); }
      else { HilbertMap.instance.ShowGroup( group ); }
   }

   public void OnPointerExit( PointerEventData pointerData ) {

      HilbertMap.instance.ShowAllElements();
   }
}
