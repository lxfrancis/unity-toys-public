using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Lx;
using Random = UnityEngine.Random;

public enum SpanAlignment {

   None,
   Days,
   Weeks,
   Months,
   Years,
   Decades
}

public class HilbertMap: MonoBehaviour {

   public static HilbertMap instance;

   static Color randomColor {
      get {
         Color result = Color.black;
         while (result.r + result.g + result.b < 1.0f || result.r + result.g + result.b > 2.0f) {
            result = new Color( Random.value, Random.value, Random.value, 1.0f );
         }
         return result;
      }
   }

   static Color MaxDifferentColor( IEnumerable< Color > existing, int iterations ) {

      Color[] candidates = Enumerable.Range( 0, iterations ).Select( n => randomColor ).ToArray();
      return candidates.MinBy( c => existing.Max( ex => ((Vector4) c - (Vector4) ex).magnitude ) );
   }

   public TextAsset     source;
   public int           sideDivBits = 6;
   public float         divSize     = 0.01f;
   public SpanAlignment squareTimeSpan;
   public bool          allowTwoSquareAlignment, alignSquaresWithCalendar;
   public Material      baseMaterial;
   public LineRenderer  edgePrefab, decadeEdgePrefab, yearEdgePrefab, monthEdgePrefab;
   public RectTransform labelPrefab, decadeLabelPrefab, yearLabelPrefab, monthLabelPrefab,
                        labelParent, elementLabelContainer;
   public Transform     layerList, groupList, elementList;
   public Selectable    layerButtonPrefab, groupListingPrefab, elementListingPrefab;
   public Text          elementName, elementDateRange, elementTotalTime;
   public CanvasGroup   elementDetails;

   DateTime mapStartTime, lifeStartTime;
   bool     dirty, monthsVisible = true;
   Element  lifeDecadeGuide, calendarDecadeGuide, lifeYearGuide, calendarYearGuide, monthGuide;
   int      yearGuideSetting = 1, decadeGuideSetting = 1;
   float    elementLabelY, guideLabelY, elementEdgeY, guideEdgeY, elementMeshY;
   Color    baseGroupListingColor;

   public void SetSquareAlignment( int alignment ) {

      if ((SpanAlignment) alignment == squareTimeSpan) { return; }

      squareTimeSpan = (SpanAlignment) alignment;
      dirty          = true;
   }

   public void SetCalenderAlignment( bool on ) {

      if (on == alignSquaresWithCalendar) { return; }

      alignSquaresWithCalendar = on;
      dirty                    = true;
   }

   public void SetDecadeGuide( int setting ) {

      decadeGuideSetting = setting;
      lifeDecadeGuide.SetVisible( setting == 0 );
      calendarDecadeGuide.SetVisible( setting == 1 );
   }

   public void SetYearGuide( int setting ) {

      yearGuideSetting = setting;
      lifeYearGuide.SetVisible( setting == 0 );
      calendarYearGuide.SetVisible( setting == 1 );
   }

   public void SetMonthGuide( bool on ) {

      monthGuide.SetVisible( monthsVisible = !monthsVisible );
   }

   public void ShowElement( Element element ) {

      foreach (var e in elementsByName.Values) {
         
         if (e.layer == "guide") { continue; }
         e.SetVisible( e == element );
         e.listingText.color = e == element ? e.color.ShiftLuma( 0.25f ) : e.color * 0.5f;
      }

      elementDetails.gameObject.SetActive( true );

      elementName.text       = element.name;
      DateTime startTime     = element.blocks.Min( b => b.startTime );
      DateTime endTime       = element.blocks.Max( b => b.endTime   );
      elementDateRange.text  = startTime.ToShortDateString() + " - " + endTime.ToShortDateString();
      TimeSpan totalTime     = endTime - startTime;
      float    years         = totalTime.Days / 365.25f;
      float    monthDuration = 365.25f / 12.0f;
      float    months        = (totalTime.Days / monthDuration) % 12.0f;
      elementTotalTime.text  = (int) years + "y " + (int) months + "m " + (int) (totalTime.Days % monthDuration) + "d";
   }

   public void ShowAllElements() {

      elementDetails.gameObject.SetActive( false );

      foreach (var e in elementsByName.Values) {
         
         if (e.layer == "guide") { continue; }
         e.SetVisible( true );
         e.listingText.color = e.color;
      }

      foreach (var listing in groupListings) { listing.text.color = baseGroupListingColor; }
   }

   public void ShowGroup( string group ) {

      elementDetails.gameObject.SetActive( false );

      foreach (var e in elementsByName.Values) {
         
         if (e.layer == "guide") { continue; }
         e.SetVisible( e.group == group );
         e.listingText.color = e.group == group ? e.color.ShiftLuma( 0.25f ) : e.color * 0.5f;
      }

      foreach (var listing in groupListings) {

         listing.text.color = listing.group == group ? baseGroupListingColor.ShiftLuma( 0.25f )
                                                     : baseGroupListingColor * 0.5f;
      }
   }

   public void ShowLayer( string layer ) {

      foreach (var kvp in shapeLayersByName) {

         if (kvp.Key == "guide") { continue; }
         kvp.Value.gameObject.SetActive( kvp.Key == layer );
      }

      foreach (var kvp in labelLayersByName) {

         if (kvp.Key == "guide") { continue; }
         kvp.Value.gameObject.SetActive( kvp.Key == layer );
      }

      foreach (var e in elementsByName.Values) {

         if (!e.listing) { continue; }
         e.listing.gameObject.SetActive( e.layer == layer );
      }

      foreach (var gel in groupListings) { gel.gameObject.SetActive( gel.layer == layer ); }
   }

   public enum CornerDir { None, UpperLeft, UpperRight, LowerLeft, LowerRight };

   [Serializable]
   public class Element {

      [Serializable]
      public class TimeBlock {

         public DateTime startTime, endTime;
         public string   labelString;
         public RectTransform label;

         public TimeBlock( DateTime startTime, DateTime endTime, string labelString=null ) {

            this.startTime   = startTime;
            this.endTime     = endTime;
            this.labelString = labelString;
         }
      }

      class LineSpan {

         public Coord2    start, end;
         //public CornerDir startCorner, endCorner;

         public LineSpan( int startX, int startY, int endX, int endY/*, CornerDir startCorner, CornerDir endCorner*/ ) {

            start = new Coord2( startX, startY );
            end   = new Coord2( endX,   endY   );
            // this.startCorner = startCorner;
            // this.endCorner   = endCorner;
         }

         public override string ToString() {

            return "[" + start.ToString() + " -> " + end.ToString() + "]";
         }
      }

      public string            name;
      public string            group;
      public string            layer;
      public List< TimeBlock > blocks = new List< TimeBlock >();
      public Transform         shape;
      public bool[,]           points;
      public int               size;
      public SpanAlignment     guideSpan = SpanAlignment.None;
      public bool              calendarAligned = false;
      public List< Coord2 >    pointList;
      public Selectable        listing;
      public Text              listingText;
      public RectTransform     label;
      public List< RectTransform > labels = new List< RectTransform >();
      public GameObject        labelParent;
      public Color             color;
      public RectTransform     labelPrefabToUse;

      public Element( string name, string group, string layer, DateTime startTime, DateTime endTime, int size,
                      SpanAlignment guideSpan=SpanAlignment.None, bool calendarAligned=false, Color? setColor=null ) {

         this.name            = name;
         this.group           = group;
         this.layer           = layer;
         this.size            = size;
         points               = new bool[ size, size ];
         this.guideSpan       = guideSpan;
         this.calendarAligned = calendarAligned;
         instance.elementsPerLayer[ layer ].Add( this );

         RectTransform labelParentRTF
            = Instantiate( instance.elementLabelContainer, instance.labelLayersByName[ layer ] );
         //Debug.Log( "label parent rtf: " + labelParentRTF + "; label parent: " + labelParentRTF?.gameObject );
         labelParentRTF.name          = name;
         labelParent                  = labelParentRTF.gameObject;

         labelPrefabToUse = instance.labelPrefab;
         if (guideSpan == SpanAlignment.Months)  { labelPrefabToUse = instance.monthLabelPrefab;  }
         if (guideSpan == SpanAlignment.Years)   { labelPrefabToUse = instance.yearLabelPrefab;   }
         if (guideSpan == SpanAlignment.Decades) { labelPrefabToUse = instance.decadeLabelPrefab; }

         if (guideSpan == SpanAlignment.None) {

            this.color = setColor ?? MaxDifferentColor( instance.elementsPerLayer[ layer ].Select( e => e.color ), 10 );

            listing           = Instantiate( instance.elementListingPrefab, instance.elementList );
            listingText       = listing.GetComponentInChildren< Text >();
            listingText.text  = name;
            listingText.color = color;
            instance.elementsByListing[ listing ] = this;
            GroupElementListing gel = listing.GetComponentInChildren< GroupElementListing >();
            gel.element             = this;
            gel.layer               = layer;

            if (!instance.groups.Contains( group ) && group != "--") {

               instance.groups.Add( group );

               var groupListing = Instantiate( instance.groupListingPrefab, instance.groupList );
               groupListing.GetComponentInChildren< Text >().text = group;
               gel       = groupListing.GetComponentInChildren< GroupElementListing >();
               gel.group = group;
               gel.layer = layer;
               instance.groupListings.Add( gel );
            }

            label              = Instantiate( labelPrefabToUse, labelParent.transform as RectTransform );
            Text labelText     = label.GetComponentInChildren< Text >();
            labelText.text     = name;
            labelText.color    = color.ShiftLuma( 0.25f );
            Color backingColor = color * 0.4f;
            backingColor.a     = 0.4f;
            label.GetComponentInChildren< Image >().color = backingColor;
            labels.Add( label );

            blocks.Add( new TimeBlock( startTime, endTime ) );
         }
      }

      public void SetVisible( bool visible ) {

         labelParent.SetActive( visible );
         shape.gameObject.SetActive( visible );
      }

      public void GenerateShape( float divDuration, float divSize, DateTime zeroTime, Transform parent,
                                 Material baseMat ) {

         DateTime now = DateTime.Now;

         List< Vector3 > verts = new List< Vector3 >();
         List< int >     tris  = new List< int >();
         pointList             = new List< Coord2 >();

         if (shape) { Destroy( shape.gameObject ); }

         if (guideSpan != SpanAlignment.None) { labels.DestroyAll(); }

         GameObject shapeObject       = new GameObject( name );
         shape                        = shapeObject.transform;
         shapeObject.transform.parent = parent;
         // Debug.Log( "Generated color for " + name + ": " + color );

         LineRenderer edgePrefabToUse = instance.edgePrefab;
         if (guideSpan == SpanAlignment.Months)  { edgePrefabToUse = instance.monthEdgePrefab;  }
         if (guideSpan == SpanAlignment.Years)   { edgePrefabToUse = instance.yearEdgePrefab;   }
         if (guideSpan == SpanAlignment.Decades) { edgePrefabToUse = instance.decadeEdgePrefab; }

         if (guideSpan != SpanAlignment.None) { blocks.Clear(); }

         switch (guideSpan) {

            case SpanAlignment.Months: {

               DateTime currStartDate = instance.mapStartTime;

               while (currStartDate <= now) {

                  DateTime endDate = currStartDate.AddMonths( 1 ).AddDays( 1 - currStartDate.Day );
                  if (endDate > now) { endDate = now; }
                  blocks.Add( new TimeBlock( currStartDate, endDate, currStartDate.Month.ToString() ) );
                  if (endDate == now) { break; }
                  currStartDate = endDate;
               }
            } break;

            case SpanAlignment.Years: {
               
               DateTime currStartDate = calendarAligned ? instance.mapStartTime : instance.lifeStartTime;
               int      age           = 0;
               
               while (currStartDate <= now) {

                  DateTime endDate = currStartDate.AddYears( 1 );
                  if (calendarAligned) { endDate = endDate.AddDays( 1 - endDate.DayOfYear ); }
                  if (endDate > now) { endDate = now; }
                  blocks.Add( new TimeBlock( currStartDate, endDate,
                                             (calendarAligned ? currStartDate.Year : age).ToString() ) );
                  if (endDate == now) { break; }
                  currStartDate = endDate;
                  age++;
               }
            } break;

            case SpanAlignment.Decades: {

               DateTime currStartDate = calendarAligned ? instance.mapStartTime : instance.lifeStartTime;
               int      age           = 0;

               while (currStartDate <= now) {

                  DateTime endDate = currStartDate.AddYears( 10 );
                  if (calendarAligned) { endDate = new DateTime( endDate.Year - endDate.Year % 10, 1, 1 ); }
                  if (endDate > now) { endDate = now; }
                  blocks.Add( new TimeBlock( currStartDate, endDate,
                                       (calendarAligned ? currStartDate.Year - currStartDate.Year % 10 : age) + "s" ) );
                  if (endDate == now) { break; }
                  currStartDate = endDate;
                  age += 10;
               }
            } break;
         }

         foreach (var block in blocks) {

            int                       startDiv       = (int) (block.startTime.Subtract( zeroTime ).TotalDays / divDuration);
            int                       endDiv         = (int) (block.endTime.Subtract( zeroTime ).TotalDays   / divDuration);
            bool[,]                   blockPoints    = new bool[ size, size ];
            bool[,]                   vertLines      = new bool[ size + 1, size + 1 ];
            bool[,]                   horizLines     = new bool[ size + 1, size + 1 ];
            List< Coord2 >            blockPointList = new List< Coord2 >();
            HashSet< Coord2 >         coveredPoints  = new HashSet< Coord2 >();
            Dictionary< Coord2, int > rectSizes      = new Dictionary< Coord2, int >();

            /*
            bool debug = Random.value < 0.1f;
            /*/
            bool debug = false;
            //*/

            // Debug.Log( "Drawing shape for " + name + "; start: " + block.startTime + "; startDiv: " + startDiv
            //               + "; end: " + block.endTime + "; endDiv: " + endDiv );

            for (int i = startDiv; i < endDiv; i++) {

               int[]   originCoords = HilbertCurve.IntToHilbert( i, 2 );

               points     [ originCoords[ 0 ], originCoords[ 1 ] ] = true;
               blockPoints[ originCoords[ 0 ], originCoords[ 1 ] ] = true;
               blockPointList.Add( new Coord2( originCoords[ 0 ], originCoords[ 1 ] ) );
               pointList.Add( new Coord2( originCoords[ 0 ], originCoords[ 1 ] ) );
            }

            blockPointList = blockPointList.OrderByDescending( p =>
                                Enumerable.Range( 0, 8 )
                                          .Select( n => (int) Mathf.Pow( 2, n ) )
                                          .Where( n => p.x % n == 0 && p.y % n == 0 )
                                          .Max() ).ToList();

            if (debug) { Debug.Log( "points in block: " + Utils.PrintVals( blockPointList.ToArray(), false, true ) ); }

            foreach (var point in blockPointList) {

               if (coveredPoints.Contains( point )) { continue; }
               //coveredPoints.Add( point );
               int         rectSize = 1;
               Coord2Range range    = new Coord2Range( point );

               for (rectSize = 1; rectSize < 256; rectSize *= 2 ) {

                  if (point.x % rectSize != 0   || point.y % rectSize != 0)   { break; }
                  if (point.x + rectSize > size || point.y + rectSize > size) { break; }

                  var newRange = new Coord2Range( point, point + Coord2.one * (rectSize - 1) );
                  if (newRange.Any( p => !blockPoints[ p.x, p.y ] || coveredPoints.Contains( p ))) { break; }

                  range = newRange;

                  if (debug) {
                     Debug.Log( "rect size: " + rectSize + "; range: " + range + "; covered by range: "
                                   + Utils.PrintVals( range.ToArray(), false, true ) );
                  }
               }
               rectSize /= 2;
               foreach (var covered in range ) { coveredPoints.Add( covered ); }
               if (debug) {
                  Debug.Log( "covered points is now: " + Utils.PrintVals( coveredPoints.ToArray(), false, true ) );
               }
               rectSizes[ point ] = rectSize;
            }

            if (debug) {
               Debug.Log( "Rect sizes for block in " + name + ":\n"
                             + string.Join( "\n", rectSizes.Select( r => r.Key + ": " + r.Value ).ToArray() ) );
            }

            foreach (var rect in rectSizes ) {

               int     vertInd = verts.Count;
               Vector3 origin  = new Vector3( rect.Key.x, instance.elementMeshY, rect.Key.y ) * divSize;

               verts.Add( origin );
               verts.Add( origin + Vector3.forward * divSize * rect.Value );
               verts.Add( origin + Vector3.right   * divSize * rect.Value );
               verts.Add( origin + new Vector3( divSize, 0.0f, divSize ) * rect.Value );

               tris.Add( vertInd );
               tris.Add( vertInd + 1 );
               tris.Add( vertInd + 3 );
               tris.Add( vertInd );
               tris.Add( vertInd + 3 );
               tris.Add( vertInd + 2 );
            }

            // add corners
            CornerDir[,] corners = new CornerDir[ size + 1, size + 1 ];

            // add individual edge segments
            for (int x = 0; x < size; x++) {

               for (int y = 0; y < size; y++) {

                  if (!blockPoints[ x, y ]) { continue; }

                  if (((x == 0 || !blockPoints[ x-1, y ]) && (y == 0 || !blockPoints[ x, y-1 ]))
                   || ((x >  0 &&  blockPoints[ x-1, y ]) && (y >  0 &&  blockPoints[ x, y-1 ]))) {

                     corners[ x, y ] = CornerDir.LowerLeft;
                  }

                  if (((x == 0 || !blockPoints[ x-1, y ]) && (y == size-1 || !blockPoints[ x, y+1 ]))
                   || ((x >  0 &&  blockPoints[ x-1, y ]) && (y <  size-1 &&  blockPoints[ x, y+1 ]))) {

                     corners[ x, y+1 ] = CornerDir.UpperLeft;
                  }

                  if (((x == size-1 || !blockPoints[ x+1, y ]) && (y == size-1 || !blockPoints[ x, y+1 ]))
                   || ((x <  size-1 &&  blockPoints[ x+1, y ]) && (y <  size-1 &&  blockPoints[ x, y+1 ]))) {

                     corners[ x+1, y+1 ] = CornerDir.UpperRight;
                  }

                  if (((x == size-1 || !blockPoints[ x+1, y ]) && (y == 0 || !blockPoints[ x, y-1 ]))
                   || ((x <  size-1 &&  blockPoints[ x+1, y ]) && (y >  0 &&  blockPoints[ x, y-1 ]))) {

                     corners[ x+1, y ] = CornerDir.LowerRight;
                  }

                  if (x == 0        || !blockPoints[ x - 1, y ]) { vertLines[ x,     y ] = true; }
                  if (x == size - 1 || !blockPoints[ x + 1, y ]) { vertLines[ x + 1, y ] = true; }

                  if (y == 0        || !blockPoints[ x, y - 1 ]) { horizLines[ x, y     ] = true; }
                  if (y == size - 1 || !blockPoints[ x, y + 1 ]) { horizLines[ x, y + 1 ] = true; }
               }
            }

            // combine edge segments into lines
            List< LineSpan > spans = new List< LineSpan >();

            // vertical lines first
            for (int x = 0; x <= size; x++) {

               for (int y = 0; y <= size; y++) {

                  if (!vertLines[ x, y ]) { continue; }

                  int startY = y;
                  while (vertLines[ x, y ]) { y++; }

                  spans.Add( new LineSpan( x, startY, x, y ) );
               }
            }

            for (int y = 0; y <= size; y++) {

               for (int x = 0; x <= size; x++) {

                  if (!horizLines[ x, y ]) { continue; }

                  int startX = x;
                  while (horizLines[ x, y ]) { x++; }

                  spans.Add( new LineSpan( startX, y, x, y ) );
               }
            }

            //Debug.Log( name + " block has " + spans.Count + " line spans" );

            if (spans.Count == 0) { continue; }
            List< Coord2 > lineCoords = new List< Coord2 >();
            LineSpan       currSpan   = spans.Random();
            spans.Remove( currSpan );
            LineSpan nextSpan = null;
            Coord2   currEnd  = currSpan.end;
            lineCoords.Add( currSpan.start );
            lineCoords.Add( currSpan.end );

            // Debug.Log( "currSpan is " + currEnd + "; all other spans:\n"
            //               + Utils.PrintVals( spans.ToArray(), false, true ) );

            // foreach (var span in spans) {

            //    Debug.Log( "Match " + span + ": " + (span.start == currEnd || span.end == currEnd) );
            // }

            while ((nextSpan = spans.FirstOrDefault( s => s.start == currEnd || s.end == currEnd )) != null) {

               currEnd = nextSpan.start == currEnd ? nextSpan.end : nextSpan.start;
               spans.Remove( nextSpan );
               currSpan = nextSpan;
               lineCoords.Add( currEnd );
            }

            LineRenderer lineRenderer  = Instantiate( edgePrefabToUse, shapeObject.transform );
            lineRenderer.positionCount = lineCoords.Count;
            float yPosition = guideSpan == SpanAlignment.None ? instance.elementEdgeY : instance.guideEdgeY;
            lineRenderer.widthMultiplier *= divSize * 0.333f;
            float cornerShift = lineRenderer.widthMultiplier * 0.5f;
            // Vector3[] shunkLineVerts = instance.ShrinkPath( lineVerts.Select( c => ((Vector3) (Vector2) c) * divSize ).ToArray(),
            //                                                 lineRenderer.widthMultiplier * 0.5f );
            Vector3[] lineVerts = lineCoords.Select( coord => {
               var     cornerDir = corners[ coord.x, coord.y ];
               Vector3 vert      = new Vector3( coord.x * divSize, yPosition, coord.y * divSize );
               if (guideSpan != SpanAlignment.None) { return vert; }
               if (cornerDir == CornerDir.UpperLeft || cornerDir == CornerDir.LowerLeft) { vert.x += cornerShift; }
               else { vert.x -= cornerShift; }
               if (cornerDir == CornerDir.UpperLeft || cornerDir == CornerDir.UpperRight) { vert.z -= cornerShift; }
               else { vert.z += cornerShift; }
               return vert;
            } ).ToArray();
            lineRenderer.SetPositions( lineVerts );

            if (guideSpan == SpanAlignment.None) {

               Color edgeColor = color.ShiftLuma( 0.1f ); 
               edgeColor.a     = 1.0f;
               lineRenderer.startColor = lineRenderer.endColor = edgeColor;
            }
            // Debug.Log( "Generated edge color for " + name + ": " + edgeColor );

            if (guideSpan != SpanAlignment.None) {

               Vector3 position = blockPointList.Select( v => new Vector3( v.x, 0.0f, v.y ) ).Average() * divSize
                                     + Vector3.up * instance.guideLabelY;

               if (!block.label) {

                  block.label = Instantiate( labelPrefabToUse, labelParent.transform as RectTransform );
                  //label.transform.position = position;
                  block.label.localPosition = Utils.WorldToCanvasSpace( position, labelParent.transform as RectTransform,
                                                                        block.label );
                  Text labelText = block.label.GetComponentInChildren< Text >();
                  labelText.text = block.labelString;
                  labels.Add( block.label );
               }

               block.label.localPosition
                  = Utils.WorldToCanvasSpace( position, labelParent.transform as RectTransform,
                                              block.label.transform as RectTransform );
            }
         }

         if (guideSpan == SpanAlignment.None) {

            Mesh mesh      = new Mesh();
            mesh.vertices  = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            MeshFilter meshFilter = shapeObject.AddComponent< MeshFilter >();
            meshFilter.sharedMesh = mesh;
            Material   mat        = new Material( baseMat );
            mat.color             = color * 0.6f;
            // Debug.Log( "Applying mesh material color for " + name + ": " + color );
            MeshRenderer meshRenderer   = shapeObject.AddComponent< MeshRenderer >();
            meshRenderer.sharedMaterial = mat;

            Vector3 position = pointList.Select( v => new Vector3( v.x, 0.0f, v.y ) ).Average() * divSize
                                  + Vector3.up * instance.elementLabelY;
            //Debug.Log( name + " average point position: " + position );
               //label.transform.position = position;
            label.localPosition = Utils.WorldToCanvasSpace( position, labelParent.transform as RectTransform,
                                                            label.transform as RectTransform );
         }
      }
   }

   Vector3[] ShrinkPath( Vector3[] rawPoints, float amount ) {

      int n = rawPoints.Length;

      Vector3[] newPoints = new Vector3[ n ];
      
      for (int i = 0; i < n; i++) {

         Vector3 dirToPrev = (rawPoints[ i.Ring( n, -1 ) ] - rawPoints[ i ]).normalized * amount;
         Vector3 dirToNext = (rawPoints[ i.Ring( n,  1 ) ] - rawPoints[ i ]).normalized * amount;

         newPoints[ i ] = rawPoints[ i ] + (dirToPrev + dirToNext);
      }

      // Debug.Log( Utils.PrintVals( Enumerable.Range( 0, n ).Select( i => PreciseVectorString( rawPoints[ i ] ) + " -> " + PreciseVectorString( newPoints[ i ] ) ).ToArray(), false, true ) );

      return newPoints;
   }

   string PreciseVectorString( Vector3 v ) {
      return "(" + v.x.ToString( "0.000000" ) + ", " + v.y.ToString( "0.000000" ) + ", " + v.z.ToString( "0.000000" ) + ")";
   }

   DateTime StringToDate( string input ) {

      try {
         return new DateTime( int.Parse( input.Substring( 0, 4 ) ),
                              int.Parse( input.Substring( 5, 2 ) ),
                              int.Parse( input.Substring( 8, 2 ) ) );
      } catch (Exception e) {
         // Debug.Log( "Couldn't parse date: '" + input + "'" );
         throw e;
      }
   }

   Color ParseColor( string data ) {

      // Debug.Log( "Parsing color from data: " + data );
      string[] tokens = data.Split( ',' ).Select( t => t.Trim() ).ToArray();
      // Debug.Log( "Tokens: " + Utils.PrintVals( tokens, false ) );
      return new Color( float.Parse( tokens[ 0 ] ), float.Parse( tokens[ 1 ] ), float.Parse( tokens[ 2 ] ), 1.0f );
   }

   void Awake() {

      instance = this;
   }

   void Start() {

      Parse();
      Generate();
      ShowLayer( labelLayersByName.Keys.Where( layer => layer != "guide" ).First() );
   }

   void Update() {

      if (dirty) {

         dirty = false;
         Generate();
      }
   }

   Dictionary< string, Element >     elementsByName;
   Dictionary< string, Transform >   shapeLayersByName;
   Dictionary< string, Transform >   labelLayersByName;
   Dictionary< Selectable, Element > elementsByListing;
   Dictionary< string, List< Element > > elementsPerLayer;
   HashSet< string > groups = new HashSet< string >();
   List< GroupElementListing > groupListings = new List< GroupElementListing >();

   void Parse() {

      // Debug.Log( "Parsing..." );

      elementsByName    = new Dictionary< string, Element >();
      shapeLayersByName = new Dictionary< string, Transform >();
      labelLayersByName = new Dictionary< string, Transform >();
      elementsByListing = new Dictionary< Selectable, Element >();
      elementsPerLayer  = new Dictionary< string, List< Element > >();

      bool foundStartTime = false;

      //lifeStartTime = StringToDate( startDate );

      string[] fileLines;

      if (Application.isEditor) {
         fileLines = source.text.Split( '\n' );
      }
      else {
         string path = Application.dataPath;

         if (Application.platform == RuntimePlatform.OSXPlayer) {

            var tokens = path.Split( '/' ).ToList();
            tokens.RemoveAt( tokens.Count - 1 );
            tokens.RemoveAt( tokens.Count - 1 );
            path = string.Join( "/", tokens.ToArray() );
         }

         if (Application.platform == RuntimePlatform.WindowsPlayer) {

            var tokens = path.Split( '/' ).ToList();
            tokens.RemoveAt( tokens.Count - 1 );
            path = string.Join( "/", tokens.ToArray() );
         }
         
         var reader = new System.IO.StreamReader( path + "/input.txt" );
         fileLines = reader.ReadToEnd().Split( '\n' );
      }

      Transform parent = Utils.RefreshCollectionChild( transform, "Layers" );

      int size = Mathf.RoundToInt( Mathf.Pow( 2, sideDivBits ) );

      Utils.DestroyAllChildren( layerList );
      Utils.DestroyAllChildren( elementList );
      Utils.DestroyAllChildren( groupList );

      baseGroupListingColor = groupListingPrefab.GetComponentInChildren< Text >().color;

      AddLayer( "guide", parent );

      for (int fl = 0; fl < fileLines.Length; fl++) {

         //Debug.Log( "Parsing line: " + fileLines[ fl ] );

         string[] split = fileLines[ fl ].Split( '\t' );

         DateTime startTime = default( DateTime );

         if (split[ 3 ] != "--" && split[ 3 ].Length == 10) {

            startTime = StringToDate( split[ 3 ] );

            if (!foundStartTime || lifeStartTime > startTime) {

               foundStartTime = true;
               lifeStartTime  = startTime;
            }
         }

         DateTime endTime = DateTime.Now;
         //Debug.Log( "Parsing end date: " + split[ 4 ] + "; length: " + split[ 4 ].Length );
         if (split[ 4 ] != "--" && split[ 4 ].Length >= 10) { endTime = StringToDate( split[ 4 ] ).AddDays( 1 ); }
         
         if (!shapeLayersByName.ContainsKey( split[ 0 ] )) { AddLayer( split[ 0 ], parent ); }

         Color? color = split.Length > 5 && split[ 5 ].Length >= 5 ? ParseColor( split[ 5 ] ) : (Color?) null;

         if (elementsByName.ContainsKey( split[ 2 ] )) {
            elementsByName[ split[ 2 ] ].blocks.Add( new Element.TimeBlock( startTime, endTime ) );
         } else {
            elementsByName.Add( split[ 2 ], new Element( split[ 2 ], split[ 1 ], split[ 0 ], startTime, endTime,
                                                         size, setColor: color ) );
         }
      }

      DateTime nonTime = default( DateTime );

      calendarDecadeGuide = new Element( "calendarDecades", "decades", "guide", nonTime, nonTime,
                                         size, SpanAlignment.Decades, true );

      lifeDecadeGuide = new Element( "lifeDecades", "decades", "guide", nonTime, nonTime,
                                     size, SpanAlignment.Decades, false );

      calendarYearGuide = new Element( "calendarYears", "years", "guide", nonTime, nonTime,
                                       size, SpanAlignment.Years, true );

      lifeYearGuide = new Element( "lifeYears", "years", "guide", nonTime, nonTime,
                                   size, SpanAlignment.Years, false );

      monthGuide = new Element( "months", "months", "guide", nonTime, nonTime,
                                size, SpanAlignment.Months );

      foreach (Element e in new[] { calendarDecadeGuide, lifeDecadeGuide, calendarYearGuide, lifeYearGuide, monthGuide }) {

         // Debug.Log( "adding guide element: " + e.name );
         elementsByName[ e.name ] = e;
      }
   }

   void AddLayer( string layerName, Transform shapeParent ) {
            
      shapeLayersByName[ layerName ] = Utils.RefreshCollectionChild( shapeParent, layerName );
      elementsPerLayer[ layerName ] = new List< Element >();

      RectTransform labelLayerParentRTF = Instantiate( instance.elementLabelContainer, instance.labelParent );
      labelLayerParentRTF.name          = layerName;
      labelLayersByName[ layerName ]    = labelLayerParentRTF;
      
      if (layerName != "guide") {

         var listing = Instantiate( layerButtonPrefab, layerList );
         listing.GetComponentInChildren< Text >().text = layerName;
         listing.GetComponentInChildren< LayerSwitch >().layer = layerName;
      }
   }

   void Generate() {

      //Debug.Log( "Generating..." );

      mapStartTime  = lifeStartTime;

      if (alignSquaresWithCalendar) {

         switch (squareTimeSpan) {

            case SpanAlignment.Months: {
               mapStartTime = new DateTime( mapStartTime.Year, mapStartTime.Month, 1 );
            } break;

            case SpanAlignment.Years: {
               mapStartTime = new DateTime( mapStartTime.Year, 1, 1 );
            } break;

            case SpanAlignment.Decades: {
               mapStartTime = new DateTime( mapStartTime.Year - mapStartTime.Year % 10, 1, 1 );
            } break;
         }
      }

      float numDivs = Mathf.Round( Mathf.Pow( Mathf.Pow( 2, sideDivBits ), 2 ) );
      int   numDays = (int) DateTime.Now.Subtract( mapStartTime ).TotalDays + 1;
      float divDurationMultiplier = 1.0f;

      float squareSpanDivisor = squareTimeSpan.Map( SpanAlignment.None,       1.0f    )
                                              .Map( SpanAlignment.Days,       1.0f    )
                                              .Map( SpanAlignment.Weeks,      7.0f    )
                                              .Map( SpanAlignment.Months,    30.4375f )
                                              .Map( SpanAlignment.Years,    365.25f   )
                                              .Map( SpanAlignment.Decades, 3652.5f    );

      if (squareTimeSpan != SpanAlignment.None) {

         float numUnits = numDays / squareSpanDivisor;
         //Debug.Log( "num units [" + squareTimeSpan + "]: " + numUnits );
         float squaredNumUnits = Mathf.NextPowerOfTwo( (int) numUnits );
         // must be an even-numbered power of 2 if two-square units are disallowed
         if (!allowTwoSquareAlignment && Mathf.Log( squaredNumUnits, 2 ) % 2 == 1) { squaredNumUnits *= 2; }
         //Debug.Log( "squared num units: " + squaredNumUnits );
         divDurationMultiplier = squaredNumUnits / numUnits;
      }

      float divDuration = ((DateTime.Now.Subtract( mapStartTime ).Days + 1) / numDivs) * divDurationMultiplier;
      //Debug.Log( "Division duration: " + divDuration + " days" );

      elementMeshY  = 0.0f;
      guideEdgeY    = divSize * 0.1f;
      elementEdgeY  = divSize * 0.2f;
      guideLabelY   = divSize * 0.3f;
      elementLabelY = divSize * 0.4f;

      foreach (var element in elementsByName.Values) {
         element.GenerateShape( divDuration, divSize, mapStartTime, shapeLayersByName[ element.layer ], baseMaterial );
      }

      SetDecadeGuide( decadeGuideSetting );
      SetYearGuide( yearGuideSetting );
      monthGuide.SetVisible( monthsVisible );
   }
}
