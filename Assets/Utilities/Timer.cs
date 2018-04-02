// Timer.cs
// by Lexa Francis, 2014-2016
// last update: 2016-08-29: realtime timers now use Time.unscaledTime instead of Time.realtimeSinceStartup
// TODO:
// 1) Allow setting the behaviour for properties when timer is stopped:
//   a) count normally, without regard to manually stopping the timer (old behaviour)
//   b) jump the properties to their end-values and count from there (current behaviour)
// 2) Callbacks/events
// 3) Switch to using Time.unscaledTime for realtime timers, or allow it as an option?
// 4) Built-in lerping
// 5) Allow creating on-the-fly from static method, to use with callbacks
// 6) Allow specifying interval time for update callback

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Lx {

   /// <summary>
   /// A general-purpose timer class for Unity projects.
   /// For handling situations like timed powerups, countdowns, etc.<para/>
   /// All timers are checked and updated the first time any timer is accessed
   /// on a given frame.</summary>

   public class Timer {

      static List< WeakReference > gameTimers = new List< WeakReference >();
      static List< WeakReference > realTimers = new List< WeakReference >();
      static float                 lastGameUpdate;
      static int                   lastRealUpdate;

      /// <summary>
      /// If there are frames on which no timers are accessed, this should be
      /// called instead in Update() or FixedUpdate() to ensure that each timer's
      /// lastStopTime is correctly set when it stops.<para/>
      /// Not necessary if accessing (in any way) at least one timer each frame.</summary>
      public static void Update() {

         GameTimersUpdate();
         RealTimersUpdate();
      }

      public static void GameTimersUpdate() {

         if (Math.Abs( lastGameUpdate - Time.time ) < Mathf.Epsilon) { return; }
         lastGameUpdate = Time.time;

         int i = 0;
         while (i < gameTimers.Count) {

            if (gameTimers[i].Target != null) { ((Timer) gameTimers[i].Target).Tick(); i++; }
            else { gameTimers.RemoveAt( i ); }
         }
      }

      public static void RealTimersUpdate() {

         if (lastRealUpdate == Time.frameCount) { return; }
         lastRealUpdate = Time.frameCount;

         int i = 0;
         while (i < realTimers.Count) {

            if (realTimers[i].Target != null) { ((Timer) realTimers[i].Target).Tick(); i++; }
            else { realTimers.RemoveAt( i ); }
         }
      }

      bool  stopped = true;
      bool  paused;
      bool  silent;
      bool  manualStop;
      bool  clamp;
      float _period;
      float instancePeriod;
      float pauseTime;
  
      /// <summary>
      /// Whether or not the timer updates in real time or scaled game time.</summary>
      public bool realTime { get; private set; }

      /// <summary>
      /// The length of the timer's period in seconds.<para/>
      /// If this is reduced while the timer is on, such that the expiry time is
      /// before the current frame, and the timer's remaining time has NOT been
      /// manually changed, then the timer will stop on the next frame.</summary>
      public float period {
         get {
            return _period;
         }
         set {
            if (Math.Abs( instancePeriod - _period ) < Mathf.Epsilon) { instancePeriod = value; }
            _period = value;
         }
      }

      /// <summary>
      /// The time that the timer was last started.<para/>
      /// This will return either game time or real time since game start, depending on whether
      /// the timer is timescale-independent.<para/>
      /// Returns a negative value if the timer has not yet been used.</summary>
      public float lastStartTime;

      /// <summary>
      /// The time that the timer was last stopped.<para/>
      /// This will return either game time or real time since game start, depending on whether
      /// the timer is timescale-independent.<para/>
      /// Returns a negative value if the timer has not yet been used.</summary>
      public float lastStopTime = 0.0f;

      int lastStopFrame;

      /// <summary>
      /// Returns true if the timer is running,
      /// starting immediately on the same frame Begin() is called.</summary>
      public bool on {
         get {
            InternalUpdate();
            return (clock < lastStartTime + pauseTime + instancePeriod) && !manualStop && !stopped;
         }
      }

      /// <summary>
      /// Returns true if the timer is neither on nor stopping;
      /// i.e. has been off for at least one frame already.</summary>
      public bool dormant {
         get {
            InternalUpdate();
            return !on && !stopping;
         }
      }

      /// <summary>
      /// Returns true only on the last frame the timer stops.<para/>
      /// A given timer's stopping property should accessed from either Update() or FixedUpdate(),
      /// but not both.</summary>
      public bool stopping {
         get {
            InternalUpdate();
            return !on && !silent && lastStopFrame == Time.frameCount;
         }
      }

      /// <summary>
      /// Amount of time passed since timer was started.<para/>
      /// Continues accumulating after timer expires, unless clamping is on.</summary>
      public float elapsed {
         get {
            InternalUpdate();
            if (!on && clamp) { return instancePeriod; }
            return clock - lastStartTime - pauseTime;
         }
      }

      /// <summary>
      /// Returns a value between 0.0 (just started) and 1.0 (stopping)
      /// while the timer is running.<para/>
      /// Continues accumulating after the timer is stopped, unless clamping is on.
      /// Will jump to 1.0 if the timer is stopped manually.</summary>
      public float elapsedNormalized {
         get {
            InternalUpdate();
           if (instancePeriod == 0.0f && clamp) { return 1.0f; }
           /*MDebug.Log( "instance period: " + instancePeriod
                     + ", clock - lastStartTime - pauseTime: " + (clock - lastStartTime - pauseTime)
                     + ", returning: " + ((clock - lastStartTime - pauseTime) / instancePeriod));*/
            if (on) { return (clock - lastStartTime - pauseTime) / instancePeriod; }
            if (clamp) { return 1.0f; }
            float returnValue = (clock - lastStopTime) / instancePeriod + 1.0f;
            if (float.IsNaN( returnValue )) { return 1.0f; }
            return returnValue;
         }
      }

      /// <summary>
      /// Amount of time remaining before the timer stops. If set while running,
      /// this changes the timer's period only until it expires this time.<para/>
      /// Will jump to 0.0 if the timer is stopped manually.</summary>
      public float remaining {  
         get {
            InternalUpdate();
            if (on) { return lastStartTime + pauseTime + instancePeriod - clock; }
            if (clamp) { return 0.0f; }
            return lastStopTime - clock;
         }
         set {
            InternalUpdate();
            if (on) { instancePeriod = value + elapsed; }
         }
      }

      /// <summary>
      /// Returns a value between 1.0 (just started) and 0.0 (stopping) while timer is running.<para/>
      /// Continues decreasing below 0.0 after timer stops, unless clamping is on.
      /// Will jump to 0.0 if the timer is stopped manually.</summary>
      public float remainingNormalized {  
         get {
            InternalUpdate();
            if (on) { return (lastStartTime + pauseTime + instancePeriod - clock) / instancePeriod; }
            if (clamp) { return 0.0f; }
            return (lastStopTime - clock) / instancePeriod;
         }
      }

      float clock {
         get {
            return realTime ? Time.unscaledTime : Time.time;
         }
      }

      void InternalUpdate() {

         if (realTime) { RealTimersUpdate(); }
         else { GameTimersUpdate(); }
      }

      /// <summary>
      /// Class constructor.</summary>
      /// <param name="period">Sets the timer's period.</param>
      /// <param name="realTime">Controls whether the timer is affected by Time.timeScale.
      /// Set this to true if, for example, you want the timer to run while the game is paused.</param>
      /// <param name="clamp">Controls whether the timer sits at its end values after it stops.
      /// Useful in animations or other cases involving controlling other values.</param> 
      public Timer( float period, bool realTime=false, bool clamp=false ) {

         _period       = period;
         lastStartTime = -period * 2.0f;
         lastStopTime  = -period;
         this.realTime = realTime;
		   this.clamp    = clamp;
         if (realTime) { realTimers.Add( new WeakReference( this ) ); }
         else { gameTimers.Add( new WeakReference( this ) ); }
      }

      void Tick() {

         if (on && paused) { pauseTime += clock - (realTime ? lastRealUpdate : lastGameUpdate); }

         if (!on && paused) { paused = false; }

         if (!on && !stopped) {

            stopped       = true;
            lastStopTime  = clock;
            lastStopFrame = Time.frameCount;
         }
      }

      /// <summary>
      /// Start the timer.</summary>
      /// <param name="silent">Setting silent to true prevents Timer.stopping from
      /// returning true when the timer expires. Returns self.</param>
      public Timer Start( bool silent=false ) {

         InternalUpdate();
         stopped        = false;
         manualStop     = false;
         instancePeriod = period;
         lastStartTime  = clock;
         this.silent    = silent;
         paused         = false;
         pauseTime      = 0.0f;
         return this;
      }

      public void Pause() {

         InternalUpdate();
         paused = true;
      }

      public void Resume() {

         InternalUpdate();
         paused = false;
      }

      /// <summary>
      /// Manually stop the timer as though its clock expired.
      /// Timer.stopping will return true NEXT frame.</summary>
      /// <param name="silent">Setting silent to true prevents Timer.stopping from
      /// returning true when the timer expires.</param>
      public void Stop( bool silent=false ) {

         InternalUpdate();
         manualStop  = true;
         this.silent = silent;
      }

      /// <summary>
      /// Manually stop the timer instantly.
      /// Timer.stopping will return true for any subsequent checks on the CURRENT frame.</summary>
      /// <param name="silent">Setting silent to true prevents Timer.stopping from
      /// returning true when the timer expires.</param>
      public void Kill( bool silent=false ) {

         InternalUpdate();
         stopped       = true;
         lastStopTime  = clock;
         lastStopFrame = Time.frameCount;
         this.silent   = silent;
      }

      public static int numTimers {
         get {
            return gameTimers.Count + realTimers.Count;
         }
      }
   }
}