using Lx;
using UnityEngine;
using UnityEngine.UI;

public class PulsingGlow: MonoBehaviour {
    
    public enum ScaleMode { LocalScale, SizeDelta }

    public float frequency = 0.5f, phase;
    
    [Tooltip( "Controls the oscillation range of the canvas group's alpha if there is a canvas group." )]
    public FloatRange alphaRange = new FloatRange( 0.75f, 1.0f );
    
    [Tooltip( "Controls the local scale range of scaleTransform, relative to the scale the transform already had." )]
    public FloatRange sizeRange  = new FloatRange( 1.0f,  1.25f );

    public Motion      startCurve = Motion.smoothRampUp,
                       endCurve   = Motion.smoothRampDown;
    public CanvasGroup canvasGroup;
    public bool        log;
    public Transform   scaleTransform;
    public Image       scaleImage;
    public bool        varySpritePixelDensity, setActiveWhenEnabled;
    public ScaleMode   scaleMode;

    Vector3       homeScale;
    float         amplitudeMultiple, homeAlpha, homeRectMinAxis, homePixelDensity;
    Vector3?      tempHomeScale;
    float?        tempHomeAlpha, tempHomePixelDensity;
    Coroutine     coroutine;
    int           lastAnimatingFrame;
    
    RectTransform _rtf;
    RectTransform rtf => _rtf ? _rtf : _rtf = scaleTransform as RectTransform;

    Vector3 currentHomeScale => tempHomeScale ?? homeScale;
    float   currentHomeAlpha => tempHomeAlpha ?? homeAlpha;

    [SerializeField] bool m_active;
    /// <summary>
    /// Whether the glow is active and animating.
    /// Will ramp up and down according to startCurve and endCurve settings.
    /// </summary>
    public bool active {
        get => m_active;
        set {
            if (value != m_active) {
                if (coroutine != null) { StopCoroutine( coroutine ); }
                if (value) {
                    if (scaleTransform) {
                        if (scaleMode == ScaleMode.SizeDelta) {
                            if (rtf) {
                                tempHomeScale = rtf.sizeDelta;
                            }
                            else {
                                Debug.LogError( $"PulsingGlow {name} is set to size delta mode but has no rect transform" );
                            }
                        }
                        else { tempHomeScale = scaleTransform.localScale; }
                    }
                    if (canvasGroup) { tempHomeAlpha = canvasGroup.alpha; }
                }
                if (gameObject.activeInHierarchy) {
                    coroutine = (value ? startCurve : endCurve).Animate( t => amplitudeMultiple = t,
                                                       enabled ? (MonoBehaviour) this : CoroutineUtils.instance, () => {
                        coroutine         = null;
                        tempHomeScale     = null;
                        tempHomeAlpha     = null;
                        amplitudeMultiple = value ? 1.0f : 0.0f;
                    } );
                }
            }
            m_active = value;
        }
    }

    void OnValidate() {

        if (!scaleTransform) { scaleTransform = transform;                     }
        if (!canvasGroup)    { canvasGroup    = GetComponent< CanvasGroup >(); }
        if (!scaleImage)     { scaleImage     = GetComponent< Image >();       }
    }

    void Start() {

        InitialiseHomeValues();
    }

    void OnEnable() {
        
        amplitudeMultiple = m_active ? 1.0f : 0.0f;
        if (!scaleTransform) { scaleTransform = transform; }

        if (setActiveWhenEnabled) { active = true; }
        // InitialiseHomeValues();
    }

    void InitialiseHomeValues() {
        
        if (scaleTransform) { homeScale = scaleTransform.localScale; }
        
        if (rtf && scaleMode == ScaleMode.SizeDelta) {
            
            homeScale = rtf.sizeDelta;
            var rect  = rtf.rect;
            homeRectMinAxis = Mathf.Min( rect.width, rect.height );

            if (scaleImage && varySpritePixelDensity) {
                homePixelDensity = scaleImage.pixelsPerUnitMultiplier;
            }
        }
        
        if (canvasGroup) { homeAlpha = canvasGroup.alpha; }
    }

    void Update() {

        if (!m_active && coroutine == null && Time.frameCount > lastAnimatingFrame) {
            
            // if (scaleTransform) { homeScale = scaleTransform.localScale; }
            // if (canvasGroup)    { homeAlpha = canvasGroup.alpha; }
            return;
        }
        
        float pulseValue = Mathf.Sin( (Time.time + phase) * Mathf.PI * 2.0f * frequency ) * 0.5f + 0.5f;

        if (scaleTransform) {
            
            if (scaleMode == ScaleMode.SizeDelta) {

                if (rtf) {

                    rtf.sizeDelta = Vector3.Lerp( currentHomeScale,
                                                  currentHomeScale + Vector3.one * sizeRange.Lerp( pulseValue ),
                                                  amplitudeMultiple );

                    if (scaleImage && varySpritePixelDensity) {

                        var rect        = rtf.rect;
                        var rectMinAxis = Mathf.Min( rect.width, rect.height );
                        var ratio       = homeRectMinAxis / rectMinAxis;
                        scaleImage.pixelsPerUnitMultiplier = ratio * homePixelDensity;
                    }
                }
            }
            else {
                scaleTransform.localScale = Vector3.Lerp( currentHomeScale,
                                                          currentHomeScale * sizeRange.Lerp( pulseValue ),
                                                          amplitudeMultiple );
            }
        }
        
        if (canvasGroup) {
            canvasGroup.alpha = Mathf.Lerp( currentHomeAlpha, alphaRange.Lerp( pulseValue ), amplitudeMultiple );
        }

        if (log) {
            Debug.Log( $"pulse value: {pulseValue}; current home scale: {currentHomeScale}; " +
                       $"local scale: {scaleTransform.localScale}; size range lerp: {sizeRange.Lerp( pulseValue )}; " +
                       $"alpha range lerp: {alphaRange.Lerp( pulseValue )}; alpha: {canvasGroup.alpha}; " +
                       $"amplitude multiple: {amplitudeMultiple}; " +
                       $"sine: {Mathf.Sin( (Time.time + phase) * Mathf.PI * 2.0f * frequency )}" );
        }

        if (m_active) { lastAnimatingFrame = Time.frameCount; }
    }

    void OnDisable() {

        if (active && setActiveWhenEnabled) {

            active = false;
            return;
        }

        if (coroutine != null) {
            
            StopCoroutine( coroutine );
            coroutine = null;
        }
        
        amplitudeMultiple = 0.0f;

        if (scaleTransform) {

            if (scaleMode == ScaleMode.SizeDelta) {

                if (rtf) {
                    rtf.sizeDelta = homeScale;
                    if (scaleImage && varySpritePixelDensity) {scaleImage.pixelsPerUnitMultiplier = homePixelDensity; }
                }
            }
            else {
                scaleTransform.localScale = homeScale;
            }
        }
        
        if (canvasGroup) { canvasGroup.alpha = homeAlpha; }
    }
}