// by Lexa Francis, 2014-2017

using UnityEngine;

namespace Lx {
   
   /*
    *  This component allows your cameras to cover (perceptually) the same 2D-area field of view at different aspect 
    *  ratios, rather than having a fixed vertical FOV as usual.
    *  Set the base aspect ratio and vertical FOV (for example, 1.333f and 45 degrees) based on the aspect ratio for
    *  which you are testing/designing, and call UpdateFOV() at runtime to update the attached cameras' FOVbased on the
    *  current actual screen aspect ratio.
    */
   public class ConstantAreaFOV: MonoBehaviour {

      public Camera[] cams;
      public float    baseVerticalFOV = 45.0f;
      public float    baseAspectRatio = 1.3333f;

      float baseRectHeight, sqrtBaseAspect, lastBaseFOV;

      void Start() {

         Initialise();
      }

      public void Initialise() {

         baseRectHeight = 2.0f * Mathf.Tan( Mathf.Deg2Rad * baseVerticalFOV * 0.5f );
         sqrtBaseAspect = baseAspectRatio.Sqrt();
         UpdateFOV();
      }

      public void UpdateFOV() {

         float currentAspect = (float) Screen.width / Screen.height;
         float newRectHeight = baseRectHeight * ((currentAspect.Sqrt() * sqrtBaseAspect) / currentAspect);
         float newFOV        = 2.0f * Mathf.Rad2Deg * Mathf.Atan( newRectHeight * 0.5f );

         foreach (Camera cam in cams) { cam.fieldOfView = newFOV; }
      }

      void Update() {

         if (lastBaseFOV != baseVerticalFOV) { Initialise(); }
         lastBaseFOV = baseVerticalFOV;
      }
   }
}