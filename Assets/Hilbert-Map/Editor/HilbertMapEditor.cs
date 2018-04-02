using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Lx;
using Random = UnityEngine.Random;

[CustomEditor( typeof( HilbertMap ) )]
public class HilbertMapEditor: Editor {

   //const string timeFormat = "yyyy-MM-dd";

   //static Regex CSVParser = new Regex( ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))" );

   // static HilbertMap script;

   // static Color randomColor {
   //    get {
   //       Color result = Color.black;
   //       while (result.r + result.g + result.b < 1.0f || result.r + result.g + result.b > 2.0f) {
   //          result = new Color( Random.value, Random.value, Random.value, 1.0f );
   //       }
   //       return result;
   //    }
   // }

   // class Element {

   //    public struct TimeBlock {

   //       public DateTime startTime, endTime;

   //       public List< List< Coord2 > > lines;

   //       public TimeBlock( DateTime startTime, DateTime endTime ) {

   //          this.startTime = startTime;
   //          this.endTime   = endTime;
   //          lines          = new List< List< Coord2 > >();
   //       }
   //    }

   //    class LineSpan {

   //       public Coord2 start, end;

   //       public LineSpan( int startX, int startY, int endX, int endY ) {

   //          start = new Coord2( startX, startY );
   //          end   = new Coord2( endX,   endY   );
   //       }

   //       public override string ToString() {

   //          return "[" + start.ToString() + " -> " + end.ToString() + "]";
   //       }
   //    }

   //    public string            name;
   //    public string            group;
   //    public string            layer;
   //    public List< TimeBlock > blocks = new List< TimeBlock >();
   //    public Transform         shape;
   //    public bool[,]           points;
   //    public int               size;

   //    public Element( string name, string group, string layer, DateTime startTime, DateTime endTime, int size ) {

   //       this.name  = name;
   //       this.group = group;
   //       this.layer = layer;
   //       this.size  = size;
   //       points     = new bool[ size, size ];

   //       blocks.Add( new TimeBlock( startTime, endTime ) );
   //    }

   //    public void GenerateShape( float divDuration, float divSize, DateTime zeroTime, Transform parent,
   //                               Material baseMat ) {

   //       List< Vector3 > verts = new List< Vector3 >();
   //       List< int >     tris  = new List< int >();

   //       GameObject shapeObject       = new GameObject( name );
   //       shapeObject.transform.parent = parent;
   //       Color color                  = randomColor;
   //       // Debug.Log( "Generated color for " + name + ": " + color );

   //       foreach (var block in blocks) {

   //          int            startDiv    = (int) (block.startTime.Subtract( zeroTime ).TotalDays / divDuration);
   //          int            endDiv      = (int) (block.endTime.Subtract( zeroTime ).TotalDays   / divDuration);
   //          bool[,]        blockPoints = new bool[ size, size ];
   //          bool[,]        vertLines   = new bool[ size + 1, size + 1 ];
   //          bool[,]        horizLines  = new bool[ size + 1, size + 1 ];
   //          List< Coord2 > blockPointList = new List< Coord2 >();

   //          HashSet< Coord2 > coveredPoints = new HashSet< Coord2 >();
   //          Dictionary< Coord2, int > rectSizes = new Dictionary< Coord2, int >();

   //          /*
   //          bool debug = Random.value < 0.1f;
   //          /*/
   //          bool debug = false;
   //          //*/

   //          // Debug.Log( "Drawing shape for " + name + "; start: " + block.startTime + "; startDiv: " + startDiv
   //          //               + "; end: " + block.endTime + "; endDiv: " + endDiv );

   //          for (int i = startDiv; i < endDiv; i++) {

   //             // int     vertInd      = verts.Count;
   //             int[]   originCoords = HilbertCurve.IntToHilbert( i, 2 );
   //             //Vector3 origin       = new Vector3( originCoords[ 0 ], 0.0f, originCoords[ 1 ] ) * divSize;

   //             points     [ originCoords[ 0 ], originCoords[ 1 ] ] = true;
   //             blockPoints[ originCoords[ 0 ], originCoords[ 1 ] ] = true;
   //             blockPointList.Add( new Coord2( originCoords[ 0 ], originCoords[ 1 ] ) );

   //             // verts.Add( origin );
   //             // verts.Add( origin + Vector3.forward * divSize );
   //             // verts.Add( origin + Vector3.right   * divSize );
   //             // verts.Add( origin + new Vector3( divSize, 0.0f, divSize ) );

   //             // tris.Add( vertInd );
   //             // tris.Add( vertInd + 1 );
   //             // tris.Add( vertInd + 3 );
   //             // tris.Add( vertInd );
   //             // tris.Add( vertInd + 3 );
   //             // tris.Add( vertInd + 2 );
   //          }

   //          blockPointList = blockPointList.OrderByDescending( p => Enumerable.Range( 0, 8 ).Select( n => (int) Mathf.Pow( 2, n ) ).Where( n => p.x % n == 0 && p.y % n == 0 ).Max() ).ToList();

   //          if (debug) { Debug.Log( "points in block: " + Utils.PrintVals( blockPointList.ToArray(), false, true ) ); }

   //          foreach (var point in blockPointList) {

   //             if (coveredPoints.Contains( point )) { continue; }
   //             //coveredPoints.Add( point );
   //             int         rectSize = 1;
   //             Coord2Range range    = new Coord2Range( point );

   //             for (rectSize = 1; rectSize < 256; rectSize *= 2 ) {

   //                if (point.x % rectSize != 0   || point.y % rectSize != 0)   { break; }
   //                if (point.x + rectSize > size || point.y + rectSize > size) { break; }

   //                var newRange = new Coord2Range( point, point + Coord2.one * (rectSize - 1) );
   //                if (newRange.Any( p => !blockPoints[ p.x, p.y ] || coveredPoints.Contains( p ))) { break; }

   //                range = newRange;

   //                if (debug) {
   //                   Debug.Log( "rect size: " + rectSize + "; range: " + range + "; covered by range: "
   //                                 + Utils.PrintVals( range.ToArray(), false, true ) );
   //                }
   //             }
   //             rectSize /= 2;
   //             foreach (var covered in range ) { coveredPoints.Add( covered ); }
   //             if (debug) {
   //                Debug.Log( "covered points is now: " + Utils.PrintVals( coveredPoints.ToArray(), false, true ) );
   //             }
   //             rectSizes[ point ] = rectSize;
   //          }

   //          if (debug) {
   //             Debug.Log( "Rect sizes for block in " + name + ":\n"
   //                           + string.Join( "\n", rectSizes.Select( r => r.Key + ": " + r.Value ).ToArray() ) );
   //          }

   //          foreach (var rect in rectSizes ) {

   //             int     vertInd = verts.Count;
   //             Vector3 origin  = new Vector3( rect.Key.x, 0.0f, rect.Key.y ) * divSize;

   //             verts.Add( origin );
   //             verts.Add( origin + Vector3.forward * divSize * rect.Value );
   //             verts.Add( origin + Vector3.right   * divSize * rect.Value );
   //             verts.Add( origin + new Vector3( divSize, 0.0f, divSize ) * rect.Value );

   //             tris.Add( vertInd );
   //             tris.Add( vertInd + 1 );
   //             tris.Add( vertInd + 3 );
   //             tris.Add( vertInd );
   //             tris.Add( vertInd + 3 );
   //             tris.Add( vertInd + 2 );
   //          }

   //          // add individual edge segments
   //          for (int x = 0; x < size; x++) {

   //             for (int y = 0; y < size; y++) {

   //                if (!blockPoints[ x, y ]) { continue; }

   //                if (x == 0        || !blockPoints[ x - 1, y ]) { vertLines[ x,     y ] = true; }
   //                if (x == size - 1 || !blockPoints[ x + 1, y ]) { vertLines[ x + 1, y ] = true; }

   //                if (y == 0        || !blockPoints[ x, y - 1 ]) { horizLines[ x, y     ] = true; }
   //                if (y == size - 1 || !blockPoints[ x, y + 1 ]) { horizLines[ x, y + 1 ] = true; }
   //             }
   //          }

   //          // combine edge segments into lines
   //          List< LineSpan > spans = new List< LineSpan >();

   //          // vertical lines first
   //          for (int x = 0; x <= size; x++) {

   //             for (int y = 0; y <= size; y++) {

   //                if (!vertLines[ x, y ]) { continue; }

   //                int startY = y;
   //                while (vertLines[ x, y ]) { y++; }

   //                spans.Add( new LineSpan( x, startY, x, y ) );
   //             }
   //          }

   //          for (int y = 0; y <= size; y++) {

   //             for (int x = 0; x <= size; x++) {

   //                if (!horizLines[ x, y ]) { continue; }

   //                int startX = x;
   //                while (horizLines[ x, y ]) { x++; }

   //                spans.Add( new LineSpan( startX, y, x, y ) );
   //             }
   //          }

   //          // Debug.Log( name + " block has " + spans.Count + " line spans" );

   //          List< Coord2 > lineVerts = new List< Coord2 >();
   //          LineSpan       currSpan  = spans.Random();
   //          spans.Remove( currSpan );
   //          LineSpan nextSpan = null;
   //          Coord2   currEnd  = currSpan.end;
   //          lineVerts.Add( currSpan.start );
   //          lineVerts.Add( currSpan.end );

   //          // Debug.Log( "currSpan is " + currEnd + "; all other spans:\n"
   //          //               + Utils.PrintVals( spans.ToArray(), false, true ) );

   //          // foreach (var span in spans) {

   //          //    Debug.Log( "Match " + span + ": " + (span.start == currEnd || span.end == currEnd) );
   //          // }

   //          while ((nextSpan = spans.FirstOrDefault( s => s.start == currEnd || s.end == currEnd )) != null) {

   //             currEnd = nextSpan.start == currEnd ? nextSpan.end : nextSpan.start;
   //             spans.Remove( nextSpan );
   //             currSpan = nextSpan;
   //             lineVerts.Add( currEnd );
   //          }

   //          LineRenderer lineRenderer  = Instantiate( script.edgePrefab, shapeObject.transform );
   //          lineRenderer.positionCount = lineVerts.Count;
   //          lineRenderer.SetPositions( lineVerts.Select( c => new Vector3( c.x, divSize * 0.167f, c.y ) * divSize )
   //                                              .ToArray() );
   //          lineRenderer.widthMultiplier = divSize * 0.333f;
   //          Color edgeColor = color * 1.5f; 
   //          edgeColor.a = 1.0f;
   //          lineRenderer.startColor = lineRenderer.endColor = edgeColor;
   //          // Debug.Log( "Generated edge color for " + name + ": " + edgeColor );
   //       }

   //       Mesh mesh                   = new Mesh();
   //       mesh.vertices               = verts.ToArray();
   //       mesh.triangles              = tris.ToArray();
   //       mesh.RecalculateBounds();
   //       mesh.RecalculateNormals();
   //       MeshFilter meshFilter       = shapeObject.AddComponent< MeshFilter >();
   //       meshFilter.sharedMesh       = mesh;
   //       Material   mat              = new Material( baseMat );
   //       mat.color                   = color * 0.75f;
   //       // Debug.Log( "Applying mesh material color for " + name + ": " + color );
   //       MeshRenderer meshRenderer   = shapeObject.AddComponent< MeshRenderer >();
   //       meshRenderer.sharedMaterial = mat;
   //    }
   // }

   // DateTime StringToDate( string input ) {

   //    try {
   //       return new DateTime( int.Parse( input.Substring( 0, 4 ) ),
   //                            int.Parse( input.Substring( 5, 2 ) ),
   //                            int.Parse( input.Substring( 8, 2 ) ) );
   //    } catch (Exception e) {
   //       Debug.Log( "Couldn't parse date: '" + input + "'" );
   //       throw e;
   //    }
   // }

   // public override void OnInspectorGUI() {

   //    if (!script) { script = target as HilbertMap; }

   //    base.OnInspectorGUI();

   //    if (GUILayout.Button( "Generate shapes" )) {

   //       DateTime mapStartTime  = StringToDate( script.startDate );
   //       DateTime lifeStartTime = mapStartTime;

   //       if (script.alignSquaresWithCalendar) {

   //          switch (script.squareTimeSpan) {

   //             case SpanAlignment.Months: {
   //                mapStartTime = new DateTime( mapStartTime.Year, mapStartTime.Month, 1 );
   //             } break;

   //             case SpanAlignment.Years: {
   //                mapStartTime = new DateTime( mapStartTime.Year, 1, 1 );
   //             } break;

   //             case SpanAlignment.Decades: {
   //                mapStartTime = new DateTime( mapStartTime.Year - mapStartTime.Year % 10, 1, 1 );
   //             } break;
   //          }
   //       }

   //       Dictionary< string, Element >   elementsByName = new Dictionary< string, Element >();
   //       Dictionary< string, Transform > layersByName   = new Dictionary< string, Transform >();

   //       string[] fileLines = script.source.text.Split( '\n' );

   //       Transform parent = Utils.RefreshCollectionChild( script.transform, "Layers" );

   //       for (int fl = 0; fl < fileLines.Length; fl++) {

   //          //Debug.Log( "Parsing line: " + fileLines[ fl ] );

   //          string[] split = fileLines[ fl ].Split( '\t' );

   //          DateTime startTime = lifeStartTime;
   //          if (split[ 3 ] != "--" && split[ 3 ].Length == 10) { startTime = StringToDate( split[ 3 ] ); }

   //          DateTime endTime = DateTime.Now;
   //          //Debug.Log( "Parsing end date: " + split[ 4 ] + "; length: " + split[ 4 ].Length );
   //          if (split[ 4 ] != "--" && split[ 4 ].Length >= 10) { endTime = StringToDate( split[ 4 ] ).AddDays( 1 ); }

   //          if (elementsByName.ContainsKey( split[ 2 ] )) {
   //             elementsByName[ split[ 2 ] ].blocks.Add( new Element.TimeBlock( startTime, endTime ) );
   //          } else {
   //             elementsByName.Add( split[ 2 ], new Element( split[ 2 ], split[ 1 ], split[ 0 ], startTime, endTime, Mathf.RoundToInt( Mathf.Pow( 2, script.sideDivBits ) ) ) );
   //          }
            
   //          if (!layersByName.ContainsKey( split[ 0 ] )) {
   //             layersByName[ split[ 0 ] ] = Utils.RefreshCollectionChild( parent, split[ 0 ] );
   //          }
   //       }

   //       float numDivs = Mathf.Round( Mathf.Pow( Mathf.Pow( 2, script.sideDivBits ), 2 ) );
   //       int   numDays = (int) DateTime.Now.Subtract( mapStartTime ).TotalDays + 1;
   //       float divDurationMultiplier = 1.0f;

   //       float squareSpanDivisor = script.squareTimeSpan.Map( SpanAlignment.None, 1.0f )
   //                                                      .Map( SpanAlignment.Days, 1.0f )
   //                                                      .Map( SpanAlignment.Weeks, 7.0f )
   //                                                      .Map( SpanAlignment.Months, 30.4375f )
   //                                                      .Map( SpanAlignment.Years, 365.25f )
   //                                                      .Map( SpanAlignment.Decades, 3652.5f );

   //       if (script.squareTimeSpan != SpanAlignment.None) {

   //          float numUnits = numDays / squareSpanDivisor;
   //          Debug.Log( "num units [" + script.squareTimeSpan + "]: " + numUnits );
   //          float squaredNumUnits = Mathf.NextPowerOfTwo( (int) numUnits );
   //          // must be an even-numbered power of 2 if two-square units are disallowed
   //          if (!script.allowTwoSquareAlignment && Mathf.Log( squaredNumUnits, 2 ) % 2 == 1) { squaredNumUnits *= 2; }
   //          Debug.Log( "squared num units: " + squaredNumUnits );
   //          divDurationMultiplier = squaredNumUnits / numUnits;
   //       }

   //       float divDuration = ((DateTime.Now.Subtract( mapStartTime ).Days + 1) / numDivs) * divDurationMultiplier;
   //       Debug.Log( "Division duration: " + divDuration + " days" );

   //       foreach (var element in elementsByName.Values) {
   //          element.GenerateShape( divDuration, script.divSize, mapStartTime, layersByName[ element.layer ],
   //                                 script.baseMaterial );
   //       }
   //    }
   // }
}
