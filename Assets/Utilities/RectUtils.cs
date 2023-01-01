// Utility methods for Unity rects, recttransforms and canvases
// by Lexa Francis, 2014-2017

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lx {

   public static partial class Utils {

      public static bool Overlap( RectTransform a, RectTransform b, RectTransform canvas ) {

         Rect    aRect  = RectInCanvasSpace( a, canvas );
         Rect    bRect  = RectInCanvasSpace( b, canvas );
         Vector2 offset = aRect.center - bRect.center;

         return Mathf.Abs( offset.x ) < (aRect.width  + bRect.width ) * 0.5f
             && Mathf.Abs( offset.y ) < (aRect.height + bRect.height) * 0.5f;
      }

      public static (Rect left, Rect right) SplitRectHorizontal( Rect rect, float divisionPoint,
                                                                 bool proportional = false, float gapSize = 0.0f,
                                                                 float? assumedWidth = null, bool ignoreFinalGap = false,
                                                                 bool log = false ) {

         var rects = SplitRectHorizontal( rect, new[] { divisionPoint }, proportional, gapSize, assumedWidth,
                                          ignoreFinalGap, log: log );

         return (rects[ 0 ], rects[ 1 ]);
      }

      /// <summary>
      /// Split a rect into multiple sub-rects horizontally.
      /// </summary>
      /// <param name="rect">Input rect to be divided.</param>
      /// <param name="divisionPoints">Set of at least one point to divide the rect with, specified either in pixels from the left rect edge or as a proportion of the rect width between 0 and 1.</param>
      /// <param name="proportional">Whether or not the division points are interpreted as proportional or as pixel positions.</param>
      /// <param name="gapSize">Size of gap between sub-rects, in pixels.
      /// Rect subdivision is according to the total size minus the gaps.</param>
      /// <param name="assumedWidth">If proportional, the sub-rects will not shrink to a total size narrower than this.
      /// If not proportional, the sub-rects will begin shrinking when the total size is narrower than this.</param>
      /// <param name="ignoreFinalGap">Set to true if this is a fixed width set of rects where the final sub-rect (filling up to the right hand edge of the rect) will be unused.</param>
      /// <param name="log">Verbose logging of non-proportional rect details.</param>
      /// <param name="segmentSize">If specified, divisionPoints will be ignored, and numSegments number of rects will be created, each as wide as divisionSize.</param>
      /// <param name="numSegments">If specified, divisionPoints will be ignored, and this number of rects will be created, either uniformly sized (if segmentSize is unspecified) or as big as segmentSize.</param>
      /// <returns>Array of sub-rects.</returns>
      public static Rect[] SplitRectHorizontal( Rect rect, float[] divisionPoints=null, bool proportional=true,
                                                float gapSize=0.0f, float? assumedWidth=null, bool ignoreFinalGap=false,
                                                float segmentSize=0f, int numSegments=0, bool log=false ) {
         
         var originalRect=rect;

         rect.position = Vector2.zero;

         var divs = divisionPoints;

         if (numSegments > 0) { divs = new float[ numSegments ]; }

         if (divs == null) {
            
            Debug.LogError( "No division points or number of segments specified for Utils.SplitRectHorizontal()" );
            return null;
         }
         
         float contentWidth = rect.width - gapSize * divs.Length;
         if (ignoreFinalGap) { contentWidth += gapSize; }
         
         if (numSegments > 0) {

            if (segmentSize <= 0f) {

               segmentSize = 1f / numSegments;

               // if (log) {
               //    Debug.Log( $"rect width: {rect.width}, gap size: {gapSize}; divs length: {divs.Length}; " +
               //               $"num segments: {numSegments}; content width: {contentWidth}; " +
               //               $"using width: {usingWidth}; calculated segment size: {segmentSize}; " +
               //               $"frame num: {Time.frameCount}" );
               // }
            }

            for (int d = 0; d < divs.Length; d++) {

               divs[ d ] = segmentSize * (d + 1);
            }
         }

         Rect[] rects = new Rect[ divs.Length + 1 ];
         int    i;

         if (proportional) {

            if (assumedWidth.HasValue && rect.width < assumedWidth) { contentWidth += assumedWidth.Value - rect.width; }

            rects[ 0 ] = new Rect( rect.x, rect.y, contentWidth * divs[ 0 ], rect.height );
            
            for (i = 1; i < divs.Length; i++) {
               
               rects[ i ] = new Rect( rect.x + gapSize * i + contentWidth * divs[ i-1 ], rect.y,
                                        contentWidth * (divs[ i ] - divs[ i-1 ]), rect.height );
            }
            
            rects[ i ] = new Rect( rect.x + gapSize * i + contentWidth * divs[ i-1 ], rect.y,
                                     contentWidth * (1.0f - divs[ i-1 ]), rect.height );
         }
         else {
            float shrink = 0f;

            if (assumedWidth.HasValue) {
               
               shrink = Mathf.Max( 0, (assumedWidth.Value - contentWidth) / (divs.Length + 1) );
               if (ignoreFinalGap) { shrink = Mathf.Max( 0, (assumedWidth.Value - contentWidth) / divs.Length ); }
            }
         
            if (log) {
               Debug.Log( $"original rect: {rect}; max: {rect.max}, content width: {contentWidth}; shrink: {shrink}" );
            }
            
            rects[ 0 ] = new Rect( rect.x, rect.y, divs[ 0 ] - shrink, rect.height );
            
            LogRect( rects[ 0 ], 0 );
            
            for (i = 1; i < divs.Length; i++) {

               rects[ i ] = new Rect( rect.x + divs[ i-1 ] + gapSize * i - shrink * i, rect.y,
                                      divs[ i ] - divs[ i-1 ] - shrink, rect.height );
            
               LogRect( rects[ i ], i );
            }

            if (ignoreFinalGap) {
               rects[ i ] = new Rect( rect.x + divs[ i-1 ] + gapSize * i - shrink * i, rect.y,
                                      contentWidth - (divs[ i-1 ] + gapSize * i) - shrink, rect.height );
            }
            else {
               rects[ i ] = new Rect( rect.x + divs[ i-1 ] + gapSize * i - shrink * i, rect.y,
                                      rect.width - (divs[ i-1 ] + gapSize * i) - shrink, rect.height );
            }

            LogRect( rects[ i ], i );
         }

         for (int x = 0; x < rects.Length; x++) { rects[ x ].position += originalRect.position; }
         
         return rects;

         void LogRect( Rect rectToLog, int num ) {

            if (!log) { return; }
            
            Debug.Log( $"rect #{num}: {rectToLog}; max: {rectToLog.max}" );
         }
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
         var rect = canvasRect.rect;
         
         return canvasPoint - Vector3.up    * rect.height * objectTransform.anchorMax.y
                            - Vector3.right * rect.width  * objectTransform.anchorMin.x;
      }

      public static Vector2 DistanceFromRectEdge( RectTransform rectTransform, Vector2 localPoint ) {

         Vector2 distance = localPoint - (Vector2) rectTransform.localPosition;

         for (int i = 0; i < 2; i++) {
            
            if (distance[ i ] > 0) {
               distance[ i ] = Mathf.Max( 0.0f, distance[ i ]
                                                   - rectTransform.rect.size[ i ] * (1.0f - rectTransform.pivot[ i ]) );
            }
            else {
               distance[ i ] = Mathf.Min( 0.0f,
                                          distance[ i ] + rectTransform.rect.size[ i ] * rectTransform.pivot[ i ] );
            }
         }
         return distance;
      }

      public static float VerticalScrollDeltaToCoverElement( ScrollRect scrollRect, RectTransform element ) {

         if (scrollRect.content != element.parent) {
            throw new ArgumentException( "Element must be a direct child of the scroll rect content pane." );
         }

         var    scrollRtf           = scrollRect.transform as RectTransform;
         var    rect                = scrollRtf!.rect;
         float  scrollWindow        = scrollRect.content.rect.height - rect.height;
         Bounds relativeChildBounds = RectTransformUtility.CalculateRelativeRectTransformBounds( scrollRtf, element );

         if (scrollWindow <= 0.0f) { return 0.0f; }

         float aboveDelta = relativeChildBounds.center.y + relativeChildBounds.extents.y - rect.height / 2.0f;
         float belowDelta = relativeChildBounds.center.y - relativeChildBounds.extents.y + rect.height / 2.0f;

         if      (aboveDelta > 0.0f) { return aboveDelta / scrollWindow; }
         else if (belowDelta < 0.0f) { return belowDelta / scrollWindow; }

         return 0.0f;
      }
	}
}
