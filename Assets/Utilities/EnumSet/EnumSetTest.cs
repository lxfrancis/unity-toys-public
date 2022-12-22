using System.Collections;
using System.Collections.Generic;
using Lx;
using UnityEngine;

public class EnumSetTest: MonoBehaviour {
    
    public enum Placement { Gold, Silver, Bronze, Loser }

    public EnumSet< Placement, Color > placementColors;
}
