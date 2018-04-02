// Classes for caching the results of expensive calculations, and re-evaluating when necessary
// by Lexa Francis, 2014-2017

using UnityEngine;
using System;

namespace Lx {

   /// <summary>Caches the result of a calculation, and returns new values only after the period has elapsed.</summary>
   public class PeriodicEvaluation< T > {

      public float period;

      Func< T > calculation;
      float     last_evaluation_time;
      T         last_value;

      /// <summary>Construct a new PeriodicEvaluation with the given calculation and period.</summary>
      public PeriodicEvaluation( Func< T > calculation, float period = 0.0f ) {

         this.calculation = calculation;
         this.period = period;
      }

      /// <summary>Calculates, caches and returns a new value if the period has elapsed, otherwise returns the cached value.</summary>
      public T Value {
         get {
            if (Time.time != last_evaluation_time && Time.time >= last_evaluation_time + period) {

               last_evaluation_time = Time.time;
               last_value           = calculation();
            }
            return last_value;
         }
      }

      public void SetToReevaluate() {
         last_evaluation_time = Time.time - period * 2.0f;
      }

      public static implicit operator T( PeriodicEvaluation< T > evaluation ) { return evaluation.Value; }
   }

   /// <summary>Caches the result of a calculation, and returns new values only after the condition has changed.</summary>
   public class ContingentEvaluation< T > {

      public Func< object > condition;

      Func< T > calculation;
      object    last_condition_evaluation;
      T         last_value;
      bool      re_evaluate;

      /// <summary>Construct a new ContingentEvaluation with the given calculation and condition.</summary>
      public ContingentEvaluation( Func< T > calculation, Func< object > condition ) {

         this.calculation = calculation;
         this.condition = condition;
      }

      /// <summary>Calculates, caches and returns a new value if the condition has changed, otherwise returns the cached value.</summary>
      public T Value {
         get {
            object eval  = condition();

            bool   equal = ((last_condition_evaluation != null) && last_condition_evaluation.GetType().IsValueType)
                              ? last_condition_evaluation.Equals( eval )
                              : (last_condition_evaluation == eval);

            if (re_evaluate || !equal) {

               last_condition_evaluation = eval;
               last_value                = calculation();
               re_evaluate               = false;
            }
            return last_value;
         }
      }

      public void SetToReevaluate() {
         re_evaluate = true;
      }

      public static implicit operator T( ContingentEvaluation< T > evaluation ) { return evaluation.Value; }
   }
}
