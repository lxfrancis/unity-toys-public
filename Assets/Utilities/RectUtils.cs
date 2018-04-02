// Utility methods for Unity rects, recttransforms and canvases
// by Lexa Francis, 2014-2017

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lx {

   public static partial class Utils {

      public static bool Overlap( RectTransform a, RectTransform b, RectTransform canvas ) {

         Rect aRect = RectInCanvasSpace( a, canvas );
         Rect bRect = RectInCanvasSpace( b, canvas );

         Vector2 offset  = aRect.center - bRect.center;
         bool    overlap = Mathf.Abs( offset.x ) < (aRect.width  + bRect.width ) * 0.5f
                        && Mathf.Abs( offset.y ) < (aRect.height + bRect.height) * 0.5f;

         return overlap;
      }

      public static Rect[] SplitRectHorizontal( Rect rect, float[] divisionPoints, bool proportional = true,
                                                float gapSize = 0.0f ) {

         Rect[] rects = new Rect[ divisionPoints.Length + 1 ];
         int    i;

         if (proportional) {

            rects[ 0 ] = new Rect( rect.x, rect.y, rect.width * divisionPoints[ 0 ] - gapSize * 0.5f, rect.height );
            for (i = 0; i < divisionPoints.Length - 1; i++) {
               rects[ i + 1 ] = new Rect( rect.x + rect.width * divisionPoints[ i ] + gapSize * 0.5f, rect.y,
                                          rect.width * (divisionPoints[ i + 1 ] - divisionPoints[ i ]) - gapSize, rect.height );
            }
            rects[ i + 1 ] = new Rect( rect.x + rect.width * divisionPoints[ i ] + gapSize * 0.5f, rect.y,
                                       rect.width * (1.0f - divisionPoints[ i ]) - gapSize * 0.5f, rect.height );
         } else {
            rects[ 0 ] = new Rect( rect.x, rect.y, divisionPoints[ 0 ] - gapSize * 0.5f, rect.height );
            for (i = 0; i < divisionPoints.Length - 1; i++) {
               rects[ i + 1 ] = new Rect( rect.x + divisionPoints[ i ] + gapSize * 0.5f, rect.y,
                                          divisionPoints[ i + 1 ] - divisionPoints[ i ] - gapSize, rect.height );
            }
            rects[ i + 1 ] = new Rect( rect.x + divisionPoints[ i ] + gapSize * 0.5f, rect.y,
                                       rect.width - divisionPoints[ i ] - gapSize * 0.5f, rect.height );
         }
         return rects;
      }

      public static Rect[][] SplitRectLabels( Rect rect, float[] divisionPoints, float labelWidth ) {

         Rect[]   sections = SplitRectHorizontal( rect, divisionPoints );
         Rect[][] rects    = new Rect[ divisionPoints.Length + 1 ][];

         for (int i = 0; i < divisionPoints.Length + 1; i++) {
            rects[ i ] = SplitRectHorizontal( sections[ i ], new[] { labelWidth }, false );
         }
         return rects;
      }

      public static Rect RectInCanvasSpace( RectTransform r, RectTransform canvas ) {

         Vector3[] cornersInCanvasSpace = CornersInCanvasSpace( r, canvas );

         return new Rect( cornersInCanvasSpace[ 0 ].x,  cornersInCanvasSpace[ 0 ].y,
                          cornersInCanvasSpace[ 2 ].x - cornersInCanvasSpace[ 0 ].x,
                          cornersInCanvasSpace[ 1 ].y - cornersInCanvasSpace[ 0 ].y );
      }

      public static Vector3[] CornersInCanvasSpace( RectTransform r, RectTransform canvas ) {

         Vector3[] worldCorners = new Vector3[ 4 ];
         r.GetWorldCorners( worldCorners );
         return worldCorners.Select( v => canvas.InverseTransformPoint( v )
                                             + Vector3.up * canvas.rect.height ).ToArray();
      }
      
      public static Vector3 WorldToCanvasSpace( Vector3 worldTarget, RectTransform canvasRect,
                                                RectTransform objectTransform, Camera cam=null ) {
      
         if (!cam) { cam = Camera.main; }
         if (!cam) {
            Debug.LogError( "No cam given/found" );
            return Vector3.zero;
         }
         Vector3 canvasPoint = cam.WorldToViewportPoint( worldTarget );
         canvasPoint.Scale( canvasRect.rect.size );
         return canvasPoint - Vector3.up    * canvasRect.rect.height * objectTransform.anchorMax.y
                            - Vector3.right * canvasRect.rect.width  * objectTransform.anchorMin.x;
      }

      public static Vector2 DistanceFromRectEdge( RectTransform rectTransform, Vector2 localPoint ) {

         Vector2 distance = localPoint - (Vector2) rectTransform.localPosition;

         for (int i = 0; i < 2; i++) {
            if (distance[ i ] > 0) {
               distance[ i ] = Mathf.Max( 0.0f, distance[ i ] - rectTransform.rect.size[ i ] * (1.0f - rectTransform.pivot[ i ]) );
            } else {
               distance[ i ] = Mathf.Min( 0.0f, distance[ i ] + rectTransform.rect.size[ i ] * rectTransform.pivot[ i ] );
            }
         }
         return distance;
      }

      public static float VerticalScrollDeltaToCoverElement( ScrollRect scrollRect, RectTransform element ) {

         if (scrollRect.content != element.parent) {
            throw new ArgumentException( "Element must be a direct child of the scroll rect content pane." );
         }

         RectTransform scrollRtf    = scrollRect.transform as RectTransform;
         float  scrollWindow        = scrollRect.content.rect.height - scrollRtf.rect.height;
         Bounds relativeChildBounds = RectTransformUtility.CalculateRelativeRectTransformBounds( scrollRtf, element );

         if (scrollWindow <= 0.0f) { return 0.0f; }

         float aboveDelta = relativeChildBounds.center.y + relativeChildBounds.extents.y
                               - scrollRtf.rect.height / 2.0f;
         float belowDelta = relativeChildBounds.center.y - relativeChildBounds.extents.y
                               + scrollRtf.rect.height / 2.0f;

         if      (aboveDelta > 0.0f) { return aboveDelta / scrollWindow; }
         else if (belowDelta < 0.0f) { return belowDelta / scrollWindow; }

         return 0.0f;
      }
	}
}
