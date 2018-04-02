// Miscellaneous Unity utility methods and classes
// by Lexa Francis, 2014-2017

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Lx {

   /// <summary>For damping rotations on a given transform.</summary>
   public class RotationDamper {

      Transform               tf;
      TimedList< Quaternion > recent;

      /// <summary>Construct a new RotationDamper.</summary>
      /// <param name="transform">Transform to rotate.</param>
      /// <param name="dampTime">Period of time over which to dampen rotations.</param>
      public RotationDamper( Transform transform, float dampTime ) {

         tf     = transform;
         recent = new TimedList< Quaternion >( dampTime );
      }

      /// <summary>Set a new target rotation to damp towards.</summary>
      /// <param name="quaternion">Target rotation.</param>
      public void Target( Quaternion quaternion ) {

         recent.Add( quaternion );
         tf.rotation = recent.Values.Average();
      }

      /// <summary>Reset damping and snap to next target rotation.</summary>
      public void Reset() {

         recent.Clear();
      }
   }

   /// <summary>Simple struct for holding position, rotation and scale data.</summary>
   public struct TransformData {

      public Vector3    position;
      public Quaternion rotation;
      public Vector3    scale;

      /// <summary>Initialise a TransformData with a given transform's data.</summary>
      public TransformData( Transform transform, Space space=Space.World ) {

         position = space == Space.World ? transform.position : transform.localPosition;
         rotation = space == Space.World ? transform.rotation : transform.localRotation;
         scale = transform.localScale;
      }

      /// <summary>Set a Transform's position, rotation and scale to match this TransformData.</summary>
      public void SetTransform( Transform transform, Space space=Space.World ) {

         if (space == Space.Self) {

            transform.localPosition = position;
            transform.localRotation = rotation;
         } else {
            transform.position = position;
            transform.rotation = rotation;
         }
         scale = transform.localScale;
      }

      public override int GetHashCode() {
         
         return position.GetHashCode() ^ rotation.GetHashCode() << 8 ^ scale.GetHashCode() << 16;
      }

      public override string ToString() {

         return "TransformData { position: " + position + ", rotation: " + rotation + ", scale: " + scale + " }";
      }

      public override bool Equals( object obj ) {
         
         if (!(obj is TransformData)) { return false; }
         TransformData other = (TransformData) obj;
         return position == other.position && rotation == other.rotation && scale == other.scale;
      }

      public static bool operator ==( TransformData a, TransformData b ) { return a.Equals( b ); }

      public static bool operator !=( TransformData a, TransformData b ) { return !a.Equals( b ); }
   }

   public static partial class Utils {

      /// <summary>The longest startLifetime.constantMax of all child particle systems.</summary>
      public static float LongestChildParticleEffect( Transform transform, bool include_inactive_children=true ) {

         return transform.GetComponentsInChildren< ParticleSystem >( include_inactive_children )
                         .Max( ps => ps.main.startLifetime.constantMax );
      }

      public static void ShowCachedParticleEffect( GameObject effect, Vector3 position, Quaternion rotation,
                                                         MonoBehaviour script ) {

         effect.SetActive( true );
         effect.transform.position = position;
         effect.transform.rotation = rotation;

         foreach (ParticleSystem ps in effect.GetComponentsInChildren< ParticleSystem >()) { ps.Play(); }

         Lerp( script, effect.GetComponentsInChildren< ParticleSystem >().Max( ps => ps.main.startLifetime.constantMax ), null,
               () => effect.SetActive( false ) );
      }

      static IEnumerator LerpCoroutine( float duration, Action< float > lerpAction, Action completion, bool realTime ) {

         float startTime = realTime ? Time.unscaledTime : Time.time;
         yield return null;

         while (true) {
            
            float time = realTime ? Time.unscaledTime : Time.time;
            if (lerpAction != null) { lerpAction( Mathf.Clamp01( time / duration ) ); }

            if (startTime + duration < time) {
               
               if (completion != null) { completion(); }
               yield break;
            }
            yield return null;
         }
      }

      public static void Lerp( MonoBehaviour behaviour, float duration, Action< float > lerpAction, Action completion,
                              bool realTime=false ) {

         behaviour.StartCoroutine( LerpCoroutine( duration, lerpAction, completion, realTime ) );
      }

      public static void BulletTime( MonoBehaviour behaviour, float slow_timescale, float duration, bool relative=false,
                                    bool revert_to_1=true ) {
         
         float prev_timescale = Time.timeScale;
         Time.timeScale       = relative ? Time.timeScale * slow_timescale : slow_timescale;
         Lerp( behaviour, duration, null, () => Time.timeScale = revert_to_1 ? 1.0f : prev_timescale, true );
      }
      
      /// <summary>Perform an action on all materials on the given renderer matching the given name.</summary>
      public static bool ChangeMaterial( this Renderer renderer, string name, Action< Material > change ) {

         return renderer.ChangeMaterial( m => m.name == name, change );
      }
      
      /// <summary>Perform an action on all materials on the given renderer for which the selector returns true.</summary>
      public static bool ChangeMaterial( this Renderer renderer, Func< Material, bool > selector,
                                         Action< Material > change ) {

         Material[] mats    = renderer.materials;
         bool       changed = false;

         for (int i = 0; i < mats.Length; i++) {

            if (selector( mats[ i ] )) {

               change( mats[ i ] );
               changed = true;
            }
         }
         renderer.materials = mats;
         return changed;
      }
      
      /// <summary>Draw a simple graph in the scene view with the given values.</summary>
      public static void DrawDebugGraph( IEnumerable< float > vals, Color color, float vertical_scale=1.0f,
                                         float interval=1.0f, Vector3? origin_point=null, float duration=0.0f ) {

         Vector3 last   = Vector3.zero;
         Vector3 origin = origin_point ?? Vector3.zero;
         int     i      = 0;

         foreach (float val in vals) {

            Vector3 current = origin + new Vector3( i * interval, val * vertical_scale, 0.0f );
            if (i > 0) { Debug.DrawLine( last, current, color, duration ); }
            last = current;
            i++;
         }
      }
      
      /// <summary>Draw a simple graph in the scene view with the given values.</summary>
      public static void DrawDebugGraph( IEnumerable< int > vals, Color color, float vertical_scale=1.0f,
                                         float interval = 1.0f, Vector3? origin_point=null, float duration=0.0f ) {

         Vector3 last   = Vector3.zero;
         Vector3 origin = origin_point ?? Vector3.zero;
         int     i      = 0;

         foreach (int val in vals) {

            Vector3 current = origin + new Vector3( i * interval, val * vertical_scale, 0.0f );
            if (i > 0) { Debug.DrawLine( last, current, color, duration ); }
            last = current;
            i++;
         }
      }

      public static bool Down   ( this KeyCode key ) { return Input.GetKeyDown( key ); }
      public static bool Pressed( this KeyCode key ) { return Input.GetKey    ( key ); }
      public static bool Up     ( this KeyCode key ) { return Input.GetKeyUp  ( key ); }
      
      /// <summary>Returns the UnityEngine.Object in the collection matching the given name.</summary>
      /// <param name="keyify">Ignore whitespace and case in object names.</param>
      public static T MatchingName< T >( this IEnumerable< T > items, string name, bool keyify=true )
                              where T: UnityEngine.Object {

         if (items == null || items.Count() == 0) { return null; }
         if (keyify) { return items.FirstOrDefault( item => item && item.name.ToKey() == name.ToKey() ); }
         return items.FirstOrDefault( item => item && item.name == name );
      }

      /// <summary>Returns a list of all subdirectories in your project's resources directory.</summary>
      public static string[] SetResourcesDirectories() {

         string   resources_path = Application.dataPath + "/Resources";
         string[] directories    = Directory.GetDirectories( resources_path, "*", SearchOption.AllDirectories );
         return directories.Select( s => s.Substring( resources_path.Length + 1 ).Replace( '\\', '/' ) ).ToArray();
      }
      
      /// <summary>Pass in a list of resources subdirectories and find any resource by name in all paths.</summary>
      public static T FindResource< T >( string name, string[] subdirectories, bool verbose=false ) where T: UnityEngine.Object {

         if (subdirectories == null || subdirectories.Length == 0) {

            Debug.LogError( "No resources subdirectories have been specified!" );
            return null;
         }

         T result = null;

         foreach (string subdirectory in subdirectories) {

            if (verbose) {
               Debug.Log( "Attempting to load " + typeof( T ).Name + " at " + subdirectory + "/" + name );
            }
            result = Resources.Load< T >( subdirectory + "/" + name );
            if (result != null) {
               if (verbose) { Debug.Log( "Found " + typeof( T ).Name + " at " + subdirectory + "/" + name ); }
               return result;
            }
         }
         if (verbose) { Debug.LogWarning( "No " + typeof( T ).Name + " found in resources" ); }
         return null;
      }
      
      /// <summary>Adds a Unity event handler to gameobjects from code. Adds an EventTrigger component if there isn't one, and a corresponding EventTrigger.Entry to that component if necessary.</summary>
      public static void AddEventCallback( Component c, EventTriggerType type,
                                           UnityEngine.Events.UnityAction< BaseEventData > action ) {

         EventTrigger eventTrigger = c.GetComponent< EventTrigger >();
         if (!eventTrigger) { eventTrigger = c.gameObject.AddComponent< EventTrigger >(); }
         EventTrigger.Entry entry = eventTrigger.triggers.FirstOrDefault( e => e.eventID == type )
                                    ?? new EventTrigger.Entry();
         entry.eventID = type;
         entry.callback.AddListener( action );
         if (!eventTrigger.triggers.Contains( entry )) { eventTrigger.triggers.Add( entry ); }
      }
      
      /// <summary>Returns a Bounds that covers all the child renderers of the given transform.</summary>
      public static Bounds RendererBounds( Transform transform ) {

         Bounds combinedBounds = new Bounds();
         bool   initialised    = false;
         var    renderers      = transform.GetComponentsInChildren< Renderer >();

         foreach (Renderer renderer in renderers) {

            if (!initialised) {
               combinedBounds = renderer.bounds;
               initialised    = true;
            } else {
               combinedBounds.Encapsulate( renderer.bounds );
            }
         }
         return combinedBounds;
      }
      
      /// <summary>Call this around expensive calculations to print out a debug line reporting how long since the last call.
      /// Returns the current time, which you can pass in to the next call.</summary>
      public static float TimeLog( float start_time=-1, string procedure=null ) {

         if (start_time >= 0 && procedure != null) {
            Debug.Log( "Time taken for <b>" + procedure + "</b>: " + (Time.realtimeSinceStartup - start_time) );
         }
         return Time.realtimeSinceStartup;
      }
      
      /// <summary>Performs the given action when condition returns true.</summary>
      public static void DoWhenTrue( MonoBehaviour behaviour, Func< bool > condition, Action action,
                                     Func< bool > abort_condition=null ) {
         
         behaviour.StartCoroutine( WaitForCondition( action, condition, abort_condition ) );
      }
      
      static IEnumerator WaitForCondition( Action action, Func< bool > condition,
                                           Func< bool > abort_condition=null ) {

         while (true) {

            if (abort_condition != null && abort_condition()) { yield break; }
            if (!condition()) { yield return null; } else { break; }
         }
         action();
      }
      
      /// <summary>Returns a function which returns true after the given period of time has elapsed.</summary>
      public static Func< bool > TrueAfterSeconds( float seconds, bool unscaled_time=false ) {

         float start_time = unscaled_time ? Time.unscaledTime : Time.time;
         return () => (unscaled_time ? Time.unscaledTime : Time.time) > start_time + seconds;
      }
      
      /// <summary>Find a child component at any depth of a given type with the given name.</summary>
      public static T FindComponent< T >( this Component component, string name ) where T: Component {

         foreach (T c in component.GetComponentsInChildren< T >( true )) {
            if (c.name.ToKey() == name.ToKey()) { return c; }
         }
         return null;
      }
      
      /// <summary>Finds the highest ancestor of the given type.</summary>
      public static T HighestAncestorComponent< T >( this Component component ) where T: Component {

         Transform parent  = component.transform.parent;
         T         highest = null;

         while (parent) {

            T canvas = parent.GetComponent< T >();
            if (canvas) { highest = canvas; }
            parent = parent.parent;
         }

         return highest;
      }

      public static string HierarchyString( this Transform transform ) {

         if (!transform.parent) { return transform.name; }
         return transform.parent.HierarchyString() + "/" + transform.name;
      }
      
      /// <summary>Destroy the associated GameObject if the object is a component, or else just destroy the object.</summary>
      public static void DestroyGameObject( this UnityEngine.Object o ) {

         if (!o) { return; }
         if (o.GetType().IsSubclassOf( typeof( Component ) )) {
            UnityEngine.Object.Destroy( (o as Component).gameObject );
         } else {
            UnityEngine.Object.Destroy( o );
         }
      }

      public static Transform RefreshCollectionChild( Transform transform, string childName ) {
         
         Transform child = transform.Find( childName );

         if (!child) {

            child        = new GameObject().transform;
            child.name   = childName;
            child.parent = transform;
         }
         
         DestroyAllChildren( child );

         return child;
      }

      public static void DestroyAllChildren( Transform transform ) {

         List< Transform > children = new List< Transform >();
         foreach (Transform child in transform) { children.Add( child ); }
         foreach (Transform child in children) { UnityEngine.Object.DestroyImmediate( child.gameObject ); }
      }
      
      /// <summary>Set the alpha value of this graphic's color.</summary>
      public static void SetAlpha( this Graphic graphic, float alpha ) {

         if (!graphic) { return; }
         Color color   = graphic.color;
         color.a       = alpha;
         graphic.color = color;
      }
      
      /// <summary>Set the alpha value of this material's color.</summary>
      public static void SetAlpha( this Material material, float alpha ) {

         Color color    = material.color;
         color.a        = alpha;
         material.color = color;
      }
      
      /// <summary>Set this graphic's color without changing the alpha value.</summary>
      public static void SetColorExceptAlpha( this Graphic graphic, Color color ) {

         if (!graphic) { return; }
         Color original = graphic.color;
         color.a        = original.a;
         graphic.color  = color;
      }
      
      /// <summary>Create a Texture2D from a Color array.</summary>
      public static Texture2D PixelsToTexture( Color[] colors, int width, int height,
                                               TextureFormat format=TextureFormat.ARGB32, bool mipmap=true ) {
         
         Texture2D texture = new Texture2D( width, height, format, mipmap );
         texture.SetPixels( colors );
         texture.Apply();
         return texture;
      }
      
      /// <summary>Create a Texture2D from a Color32 array.</summary>
      public static Texture2D Pixels32ToTexture( Color32[] colors, int width, int height,
                                                 TextureFormat format=TextureFormat.ARGB32, bool mipmap=true ) {
         
         Texture2D texture = new Texture2D( width, height, format, mipmap );
         texture.SetPixels32( colors );
         texture.Apply();
         return texture;
      }
      
      /// <summary>Save a texture to a PNG file, optionally destroying the texture.</summary>
      public static void SaveTextureToPNG( Texture2D texture, string relativePath, bool destroy=false ) {
         
         byte[] bytes = texture.EncodeToPNG();
         if (!relativePath.EndsWith( ".png" )) { relativePath += ".png"; }
         File.WriteAllBytes( Application.dataPath + "/" + relativePath, bytes );

         if (destroy) {

            if (Application.isEditor && !Application.isPlaying) { UnityEngine.Object.DestroyImmediate( texture ); }
            else { UnityEngine.Object.Destroy( texture ); }
         }
      }
      
      /// <summary>Save a PNG image based on a Color array.</summary>
      public static void SavePixelsToPNG( Color[] colors, int width, int height, string relativePath ) {

         SaveTextureToPNG( PixelsToTexture( colors, width, height ), relativePath, true );
      }
      
      /// <summary>Save a PNG image based on a Color32 array.</summary>
      public static void SavePixels32ToPNG( Color32[] colors, int width, int height, string relativePath ) {

         SaveTextureToPNG( Pixels32ToTexture( colors, width, height ), relativePath, true );
      }
   }
}
