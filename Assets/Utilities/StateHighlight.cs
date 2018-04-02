using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Lx {

   [System.Serializable]
   public class HighlightSystem< TState, TConcreteStateHighlight >
                                   where TConcreteStateHighlight: StateHighlight< TState >, new() {

      public MaskableGraphic                 graphic;
      public CanvasGroup                     alphaGroup;
      public RectTransform                   transform;
      public float                           phase;
      public bool                            randomPhase;
      public float                           transitionTime;
      public AnimationCurve                  transitionCurve;
      public TState                          defaultState;
      public List< TConcreteStateHighlight > states;

      TConcreteStateHighlight curr, prev;
      Timer                   transition;
      Vector2                 basePosition;
      Vector3                 baseScale;
      Color                   baseColor = Color.white;
      bool                    initialised;

      void Initialise() {

         initialised = true;
         transition  = new Timer( transitionTime, false, true );
         curr        = states.First( s => s.state.Equals( defaultState ) );
         if (randomPhase) { phase = Random.Range( 0.0f, 360.0f ); }
         if (graphic) { baseColor = graphic.color; }

         if (transform) {

            basePosition = transform.anchoredPosition;
            baseScale    = transform.localScale;
         }

         foreach (var state in states) {

            state.phase = phase;

            if (graphic && (state.alphaA   == 0.0f && state.alphaB   == 0.0f)
                        || (state.colorA.a == 0.0f && state.colorB.a == 0.0f)) {

               state.invisible = true;
            }
         }
      }

      public void Update( TState state, Object context ) {

         if (!initialised) { Initialise(); }

         if (!states.Any( s => s.state.Equals( state ) )) {

            Debug.LogError( "Missing state '" + state + "' in highlight system '" + GetType().Name + "'", context );
            TConcreteStateHighlight missing = new TConcreteStateHighlight();
            missing.Initialise( state, baseColor );
            states.Add( missing );
         }

         if (!state.Equals( curr.state )) {

            prev = curr;
            curr = states.First( s => s.state.Equals( state ) );
            transition.Start();
            if (graphic && prev != null && prev.invisible && !curr.invisible) { graphic.enabled = true; }
         }

         if (transition.dormant) {

            if (graphic)    { graphic.color    = curr.color; }
            if (alphaGroup) { alphaGroup.alpha = curr.alpha; }

            if (transform) {

               transform.localScale       = baseScale * curr.scale;
               transform.anchoredPosition = basePosition + curr.position;
            }
         }
         else {

            float t = transitionCurve.Evaluate( transition.elapsedNormalized );
            if (graphic)    { graphic.color    = Color.Lerp( prev.color, curr.color, t ); }
            if (alphaGroup) { alphaGroup.alpha = Mathf.Lerp( prev.alpha, curr.alpha, t ); }

            if (transform) {

               transform.localScale       = baseScale    * Mathf  .Lerp( prev.scale,    curr.scale,    t );
               transform.anchoredPosition = basePosition + Vector2.Lerp( prev.position, curr.position, t );
            }
         }

         if (graphic && transition.stopping && curr.invisible) { graphic.enabled = false; }
      }
   }

   [System.Serializable]
   public class StateHighlight< T > {

      public T       state;
      public float   period = 1.0f;
      public Color   colorA,                colorB;
      public float   scaleA  = 1.0f,        scaleB  = 1.0f,
                     alphaA  = 1.0f,        alphaB  = 1.0f;
      public Vector2 extentA = Vector3.one, extentB = Vector3.one;

      internal float phase;
      internal bool  invisible;

      float   lastUpdateTime;
      Color   _color;
      float   _scale;
      float   _alpha;
      Vector2 _position;

      public void Initialise( T state, Color color ) {

         this.state = state;
         colorA = colorB = color;
      }

      void Update() {

         if (Time.time != lastUpdateTime) {

            lastUpdateTime = Time.time;
            float x        = period > 0 ? Utils.Cycle( period, 0.0f, phase ) : 0.0f;
            _color         = Color.Lerp( colorA, colorB, x );
            _position      = Vector2.Lerp( extentA, extentB, x );
            _scale         = Mathf.Lerp( scaleA, scaleB, x );
            _alpha         = Mathf.Lerp( alphaA, alphaB, x );
         }
      }

      public Color color {
         get {
            Update();
            return _color;
         }
      }

      public float alpha {
         get {
            Update();
            return _alpha;
         }
      }

      public float scale {
         get {
            Update();
            return _scale;
         }
      }

      public Vector2 position {
         get {
            Update();
            return _position;
         }
      }
   }
}