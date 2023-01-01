using UnityEngine;
using UnityEngine.UI;

namespace Lx {
    
    [DisallowMultipleComponent, RequireComponent( typeof( Mask ) )]
    public class HighlightGlint: MonoBehaviour {

        const string glintImageName = "[Glint Image]";

        public FloatRange     interval;
        public float          duration, angle, thickness, hackExtraSize, hackMotionMultiplier;
        public AnimationCurve curve;
        public Color          color = Color.white;
        public bool           glintOnEnable;

        Image         glintImage;
        float         nextGlintTime = Mathf.NegativeInfinity;
        Vector2       homePosition;
        RectTransform glintRtf;
        Coroutine     glintRoutine;

        
        void SetNextGlint() {
            
            nextGlintTime      = interval.max > 0.0f ? Time.time + interval.random : Mathf.NegativeInfinity;
            glintRoutine       = null;
            glintImage.enabled = false;
        }

        
        void OnEnable() {

            if (glintImage) { Destroy( glintImage.gameObject ); }

            if (hackMotionMultiplier == 0f) { hackMotionMultiplier = 1f; }

            glintImage               = new GameObject( glintImageName ).AddComponent< Image >();
            glintImage.color         = color;
            glintImage.raycastTarget = false;
            var rtf                  = transform as RectTransform;
            glintRtf                 = glintImage.transform as RectTransform;
            var rect                 = rtf!.rect;
            var diagonalSize         = Mathf.Sqrt( rect.width * rect.width + rect.height * rect.height );
            glintRtf!.sizeDelta       = new Vector2( diagonalSize, thickness );
            glintRtf.SetParent( rtf, false );
            glintRtf.Rotate( Vector3.forward, angle );
            glintRtf.anchoredPosition = glintRtf.localRotation * Vector2.up * (glintRtf.sizeDelta.y + thickness * 0.5f)
                                        * (1 + hackExtraSize) * hackMotionMultiplier;
            glintRtf.localScale       = Vector3.one * (1 + hackExtraSize);
            homePosition              = glintRtf.anchoredPosition;

            if (glintOnEnable) {
                Glint();
            }
            else {
                nextGlintTime      = Time.time + (interval.min > 0.0f ? Random.Range( 0.0f, interval.min ) : 1.0f);
                glintImage.enabled = false;
            }
        }

        
        void OnDisable() {
            
            StopAllCoroutines();
            glintRoutine = null;

            if (glintImage) { glintImage.gameObject.SetActive( false ); }
        }

        
        void Update() {

            if (nextGlintTime > 0.0f && Time.time > nextGlintTime) { Glint(); }

            if (glintRoutine == null && glintImage && glintImage.enabled) { glintImage.enabled = false; }
        }

        
        public void Glint() {

            if (!enabled || !gameObject.activeInHierarchy) { return; }

            StopAllCoroutines();
            nextGlintTime      = Mathf.NegativeInfinity;
            glintImage.enabled = true;

            glintRoutine = CoroutineUtils.Interpolate(
                t => glintRtf.anchoredPosition = Vector2.Lerp( homePosition, -homePosition, t ),
                duration, curve, SetNextGlint, this );
        }
    }
}