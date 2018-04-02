// Unity UI navigation utility methods
// by Lexa Francis, 2014-2017

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace Lx {

   public static partial class Utils {

      public static void SetNavigationSequence( Transform parent, bool horizontal, bool keepCurrent = true,
                                                bool includeInactive = false ) {

         Selectable prev = null;

         foreach (Transform child in parent) {

            Selectable s = child.GetComponent< Selectable >();
            if (!s || !s.gameObject.activeSelf) { continue; }
            if (!keepCurrent) { s.SetNavigation( null, null, null, null ); }
            Navigation sNavigation = s.navigation;

            if (prev) {

               Navigation prevNavigation = prev.navigation;

               if (horizontal) {

                  prevNavigation.selectOnRight = s;
                  sNavigation.selectOnLeft     = prev;
               }
               else {
                  prevNavigation.selectOnDown = s;
                  sNavigation.selectOnUp      = prev;
               }

               prev.navigation = prevNavigation;
               s.navigation    = sNavigation;
            }
            prev = s;
         }
      }

      public static void SetNavigationSequence( IEnumerable< Selectable > selectables, bool horizontal,
                                                bool keepCurrent = true ) {

         Selectable prev = null;

         foreach (Selectable s in selectables) {

            if (!keepCurrent) { s.SetNavigation( null, null, null, null ); }
            Navigation sNavigation = s.navigation;

            if (prev) {

               Navigation prevNavigation = prev.navigation;

               if (horizontal) {

                  prevNavigation.selectOnRight = s;
                  sNavigation.selectOnLeft     = prev;
               }
               else {
                  prevNavigation.selectOnDown = s;
                  sNavigation.selectOnUp      = prev;
               }

               prev.navigation = prevNavigation;
               s.navigation    = sNavigation;
            }
            prev = s;
         }
      }

      public static void JoinNavigationSequences( IEnumerable< Selectable > selectablesA,
                                                  IEnumerable< Selectable > selectablesB, bool horizontal ) {

         int highestA = selectablesA.Count() - 1,
             highestB = selectablesB.Count() - 1;

         for (int i = 0; i <= highestA; i++) {

            Selectable target
               = selectablesB.ElementAt( Mathf.Clamp( Mathf.RoundToInt( i * (highestB / (float) highestA) ),
                                                      0, highestB ) );
            selectablesA.ElementAt( i ).SetNavigation( null, horizontal ? null : target, null, horizontal ? target : null );
         }

         for (int i = 0; i <= highestB; i++) {

            Selectable target
               = selectablesA.ElementAt( Mathf.Clamp( Mathf.RoundToInt( i * (highestA / (float) highestB) ),
                                                      0, highestA ) );
            selectablesB.ElementAt( i ).SetNavigation( horizontal ? null : target, null, horizontal ? target : null, null );
         }
      }

      public static void SetNavigationGrid( Transform parent, int numColumns ) {

         for (int i = 0; i < parent.childCount; i++) {

            Selectable selectable = parent.GetChild( i ).GetComponentInChildren< Selectable >();

            if (i > 0) {

               Selectable prevButton = parent.GetChild( i - 1 ).GetComponentInChildren< Selectable >();
               prevButton.SetNavigation( null, null, null, selectable );
               selectable.SetNavigation( null, null, prevButton, null );
            }
            if (i >= numColumns) {

               Selectable aboveButton = parent.GetChild( i - numColumns ).GetComponentInChildren< Selectable >();
               aboveButton.SetNavigation( null, selectable, null, null );
               selectable.SetNavigation( aboveButton, null, null, null );
            }
         }
      }

      public static void SetNavigation( this Selectable selectable,
                                        Selectable up, Selectable down, Selectable left, Selectable right,
                                        bool keepCurrent = true ) {

         Navigation navigation = selectable.navigation;

         if (up    != null || !keepCurrent) { navigation.selectOnUp    = up;    }
         if (down  != null || !keepCurrent) { navigation.selectOnDown  = down;  }
         if (left  != null || !keepCurrent) { navigation.selectOnLeft  = left;  }
         if (right != null || !keepCurrent) { navigation.selectOnRight = right; }

         selectable.navigation = navigation;
      }

      public static void SetNavigationMode( this Selectable selectable, Navigation.Mode mode ) {

         Navigation navigation = selectable.navigation;
         navigation.mode       = mode;
         selectable.navigation = navigation;
      }

      static Selectable ClosestSelectableForDirection( Vector2 origin, Vector2 direction,
                                                       IEnumerable< KeyValuePair< Selectable, RectTransform > > others ) {

         var currentClosest = others.First();

         foreach (var other in others) {
            if (Vector2.Angle( other.Value.anchoredPosition - origin, direction )
                < Vector2.Angle( currentClosest.Value.anchoredPosition - origin, direction )) {
               currentClosest = other;
            }
         }
         return Vector2.Angle( currentClosest.Value.anchoredPosition - origin, direction ) < 90.0f ? currentClosest.Key
                                                                                                   : null;
      }

      public static void SetFreeformNavigation( Dictionary< Selectable, RectTransform > elements ) {

         if (elements.Count < 2) {

            Debug.LogWarning( "Can't set freeform navigation for less than two elements." );
            return;
         }

         foreach (var element in elements) {

            Navigation navigation    = element.Key.navigation;
            Vector2    thisPos       = element.Value.anchoredPosition;
            var        otherElements = elements.Where( e => e.Key != element.Key );
            navigation.selectOnUp    = ClosestSelectableForDirection( thisPos, Vector2.up,    otherElements );
            navigation.selectOnRight = ClosestSelectableForDirection( thisPos, Vector2.right, otherElements );
            navigation.selectOnDown  = ClosestSelectableForDirection( thisPos, Vector2.down,  otherElements );
            navigation.selectOnLeft  = ClosestSelectableForDirection( thisPos, Vector2.left,  otherElements );
            element.Key.navigation   = navigation;
         }
      }

      public static bool CanNavigateTo( this Selectable self, Selectable other ) {

         if (self.navigation.selectOnUp    == other) { return true; }
         if (self.navigation.selectOnRight == other) { return true; }
         if (self.navigation.selectOnDown  == other) { return true; }
         if (self.navigation.selectOnLeft  == other) { return true; }
         return false;
      }

      public static Selectable TargetForMoveDir( this Navigation navigation, MoveDirection direction ) {

         return direction.Map( MoveDirection.Up,    navigation.selectOnUp )
                         .Map( MoveDirection.Right, navigation.selectOnRight )
                         .Map( MoveDirection.Down,  navigation.selectOnDown )
                         .Map( MoveDirection.Left,  navigation.selectOnLeft );
      }

   }
}
