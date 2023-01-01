using System;
using Lx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using Object = UnityEngine.Object;

// features wanted:
// detect actual sprite rect when main sprite is using preserve aspect

[RequireComponent( typeof( Image ) )]
[ExecuteAlways]
public class SoftShadow: MonoBehaviour {

    internal static readonly bool log = false;

    internal static bool instantiating;

    public Sprite shadowSprite;

    [Range( 0f, 1f )] public float opacity = 0.8f;
    
    public float sizeByRadius     = 0.25f;
    public float offsetByRadius   = -0.1f;
    public float radiusMultiplier = 1.0f;
    public float widthOffsetByRadius;
    public bool  verboseLogs;
    
    // these shouldn't be altered directly unless something borks up
    public Image                shadowImage;
    public SoftShadowIdentifier identifier;

    RectTransform _shadowRtf;
    RectTransform shadowRtf => shadowImage.transform as RectTransform;

    RectTransform _mainRtf;
    RectTransform mainRtf => transform as RectTransform;

    Image _mainImage;
    Image mainImage => _mainImage ? _mainImage : _mainImage = GetComponent< Image >();
    
    Color shadowColor => new Color( 0.0f, 0.0f, 0.0f, opacity );

    float offsetInUnits => offsetByRadius * shadowImage.sprite.rect.width * 0.5f
                           / (shadowImage.sprite.pixelsPerUnit * shadowImage.pixelsPerUnitMultiplier);

    float widthOffsetInUnits => widthOffsetByRadius * shadowImage.sprite.rect.width * 0.5f
                                / (shadowImage.sprite.pixelsPerUnit * shadowImage.pixelsPerUnitMultiplier);
    
    float sizeInUnits => sizeByRadius * shadowImage.sprite.rect.width * 0.5f
                         / (shadowImage.sprite.pixelsPerUnit * shadowImage.pixelsPerUnitMultiplier);
    
    static readonly Color gizmoColor = Color.cyan;

    
    void Log( string message, Object context=null ) {

        if (!log && !verboseLogs) { return; }
        
        Debug.Log( message, context );
    }

    
    void DrawLineInLocalSpace( Vector3 from, Vector3 to ) {

        from = shadowRtf.TransformPoint( from );
        to   = shadowRtf.TransformPoint( to );
        
        Gizmos.DrawLine( from, to );
    }

    
    void DrawArcInLocalSpace( Vector3 centre, float radius, float fromAngle, float toAngle ) {

        for (int i = 0; i < 10; i++) {

            float segmentStartAngle = Deg2Rad * LerpAngle( fromAngle, toAngle, i / 10f );
            float segmentEndAngle   = Deg2Rad * LerpAngle( fromAngle, toAngle, (i + 1) / 10f );

            var fromPoint = shadowRtf.TransformPoint( centre + radius * new Vector3( Cos( segmentStartAngle ),
                                                                                     Sin( segmentStartAngle ) ) );
            var toPoint = shadowRtf.TransformPoint( centre + radius * new Vector3( Cos( segmentEndAngle ),
                                                                                   Sin( segmentEndAngle ) ) );
            
            Gizmos.DrawLine( fromPoint, toPoint );
        }
    }

    
    void OnDrawGizmosSelected() {

        Gizmos.color = gizmoColor;
        // bottom left, top left, top right, bottom right
        Vector3[] worldCorners = new Vector3[ 4 ];
        shadowRtf.GetWorldCorners( worldCorners );
        Vector3[] outerCorners = new Vector3[ 4 ];
        shadowRtf.GetLocalCorners( outerCorners );
        float localRadius = shadowImage.sprite.rect.width * 0.5f / mainRtf.localScale.x
                            / shadowImage.pixelsPerUnit / shadowImage.pixelsPerUnitMultiplier;  // use parent scale
        
        Vector3[] innerCorners = {
            outerCorners[ 0 ] + new Vector3(  localRadius,  localRadius ),
            outerCorners[ 1 ] + new Vector3(  localRadius, -localRadius ),
            outerCorners[ 2 ] + new Vector3( -localRadius, -localRadius ),
            outerCorners[ 3 ] + new Vector3( -localRadius,  localRadius )
        };
        
        // bottom left
        DrawLineInLocalSpace( innerCorners[ 0 ], outerCorners[ 0 ] + Vector3.right * localRadius );
        DrawLineInLocalSpace( innerCorners[ 0 ], outerCorners[ 0 ] + Vector3.up * localRadius );
        DrawArcInLocalSpace( innerCorners[ 0 ], localRadius, 180f, 270f );
        
        // top left
        DrawLineInLocalSpace( innerCorners[ 1 ], outerCorners[ 1 ] + Vector3.right * localRadius );
        DrawLineInLocalSpace( innerCorners[ 1 ], outerCorners[ 1 ] + Vector3.down * localRadius );
        DrawArcInLocalSpace( innerCorners[ 1 ], localRadius, 180f, 90f );
        
        // top right
        DrawLineInLocalSpace( innerCorners[ 2 ], outerCorners[ 2 ] + Vector3.left * localRadius );
        DrawLineInLocalSpace( innerCorners[ 2 ], outerCorners[ 2 ] + Vector3.down * localRadius );
        DrawArcInLocalSpace( innerCorners[ 2 ], localRadius, 0f, 90f );
        
        // bottom right
        DrawLineInLocalSpace( innerCorners[ 3 ], outerCorners[ 3 ] + Vector3.left * localRadius );
        DrawLineInLocalSpace( innerCorners[ 3 ], outerCorners[ 3 ] + Vector3.up * localRadius );
        DrawArcInLocalSpace( innerCorners[ 3 ], localRadius, 0f, 270f );
    }

    
    void CreateShadow() {
        
#if UNITY_EDITOR
        bool prefabMode     = UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene( gameObject.scene );
        bool prefabInstance = UnityEditor.PrefabUtility.IsPartOfPrefabInstance( gameObject );
        Log( $"SoftShadow.CreateShadow(); prefab mode: {prefabMode}; prefab instance: {prefabInstance};" +
             $"already instantiating: {instantiating}; object path: {mainRtf.Path()}", this );
        if (!prefabMode && prefabInstance) { return; }
#endif

        if (instantiating) { return; }

        try {
            instantiating = true;

            shadowImage = new GameObject( $"{name} [Shadow]" ).AddComponent< Image >();
            shadowRtf.SetParent( mainRtf.parent );
            shadowRtf.SetSiblingIndex( mainRtf.GetSiblingIndex() );
            shadowRtf.anchorMin          = mainRtf.anchorMin;
            shadowRtf.anchorMax          = mainRtf.anchorMax;
            shadowRtf.pivot              = mainRtf.pivot;
            shadowImage.sprite           = shadowSprite;
            shadowImage.color            = shadowColor;
            shadowImage.type             = Image.Type.Sliced;
            shadowImage.gameObject.layer = gameObject.layer;

            if (mainImage.type == Image.Type.Sliced) {
                shadowImage.pixelsPerUnitMultiplier = mainImage.pixelsPerUnitMultiplier
                                                      * (shadowImage.sprite.rect.width / mainImage.sprite.rect.width)
                                                      / radiusMultiplier;
            }
            else {
                shadowImage.pixelsPerUnitMultiplier = shadowImage.sprite.rect.width * 0.5f / radiusMultiplier;
            }

            shadowRtf.anchoredPosition = mainRtf.anchoredPosition + Vector2.up * offsetInUnits;
            shadowRtf.sizeDelta        = mainRtf.sizeDelta + Vector2.one * sizeInUnits + Vector2.right * widthOffsetInUnits;

            identifier               = shadowImage.gameObject.AddComponent< SoftShadowIdentifier >();
            identifier.mainComponent = this;

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications( shadowImage.gameObject );
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( SceneManager.GetActiveScene() );
            }
#endif
        }
        catch (Exception e) {
            
            Debug.LogWarning( "SoftShadow.CreateShadow() failed to create shadow", this );
            Debug.LogException( e );
        }
        finally {
            instantiating = false;
        }
        
        Log( "SoftShadow.CreateShadow() created shadow", shadowImage );
        // UpdateTriggerElement();
    }

    
    void CheckState() {

        if (instantiating || !shadowImage) { return; }

        if (!identifier) {
            
            var foundIdentifier = shadowImage.GetComponent< SoftShadowIdentifier >();
            
            if (foundIdentifier) {
                
                identifier               = foundIdentifier;
                identifier.mainComponent = this;
            }
        }

        if (shadowImage.gameObject.layer != gameObject.layer) { shadowImage.gameObject.layer = gameObject.layer; }

        if (shadowImage.color != shadowColor) { shadowImage.color = shadowColor; }

        if ((shadowRtf.anchoredPosition
             - Vector2.up * offsetInUnits - mainRtf.anchoredPosition).sqrMagnitude > 0.01f) {
            
            shadowRtf.anchoredPosition = mainRtf.anchoredPosition + Vector2.up * offsetInUnits;

            if (verboseLogs) {
                Log( "Updating soft shadow anchored position; " +
                     $"main: {mainRtf.anchoredPosition}; shadow: {shadowRtf.anchoredPosition}", this );
            }
        }

        if ((shadowRtf.sizeDelta
             - mainRtf.sizeDelta - Vector2.one * sizeInUnits - Vector2.right * widthOffsetInUnits).sqrMagnitude > 0.01f) {

            shadowRtf.sizeDelta = mainRtf.sizeDelta + Vector2.one * sizeInUnits + Vector2.right * widthOffsetInUnits;

            if (verboseLogs) {
                Log( $"Updating soft shadow size delta; main: {mainRtf.sizeDelta}; shadow: {shadowRtf.sizeDelta}" );
            }
        }

        if ((shadowRtf.anchorMin - mainRtf.anchorMin ).sqrMagnitude > 0.0001f
            || (shadowRtf.anchorMax - mainRtf.anchorMax ).sqrMagnitude > 0.0001f) {

            shadowRtf.anchorMin = mainRtf.anchorMin;
            shadowRtf.anchorMax = mainRtf.anchorMax;

            if (verboseLogs) {
                Log( $"Updating soft shadow anchors; main: {mainRtf.anchorMin}, {mainRtf.anchorMax}; " +
                     $"shadow: {shadowRtf.anchorMin}, {shadowRtf.anchorMax}" );
            }
        }

        if (shadowRtf.GetSiblingIndex() != mainRtf.GetSiblingIndex() - 1) {
            shadowRtf.SetSiblingIndex( mainRtf.GetSiblingIndex() - 1 );
        }

        if (shadowRtf.localScale    != mainRtf.localScale)    { shadowRtf.localScale    = mainRtf.localScale;    }
        if (shadowRtf.localRotation != mainRtf.localRotation) { shadowRtf.localRotation = mainRtf.localRotation; }
        
        var targetPPUM = shadowImage.sprite.rect.width * 0.5f / radiusMultiplier;
        
        if (mainImage.type == Image.Type.Sliced) {
            
            targetPPUM = mainImage.pixelsPerUnitMultiplier
                         * (shadowImage.sprite.rect.width / mainImage.sprite.rect.width) / radiusMultiplier;
        }
        
        if (Abs( targetPPUM - shadowImage.pixelsPerUnitMultiplier ) > 0.001f) {
            shadowImage.pixelsPerUnitMultiplier = targetPPUM;
        }

        if (Abs( shadowRtf.localPosition.z - mainRtf.localPosition.z ) > 0.01f) {
            
            var localPosition       = shadowRtf.localPosition;
            localPosition           = new Vector3( localPosition.x, localPosition.y, mainRtf.localPosition.z );
            shadowRtf.localPosition = localPosition;
        }
    }

    
    void Update() {

        if (!shadowImage) { CreateShadow(); }
        
        CheckState();
    }

    
    // void UpdateTriggerElement() {
    //     
    //     if (GetComponentInParent< TriggerElement >() is { } te) {
    //             
    //         te.RebuiltRendersNoDeactivate();
    //         shadowImage.enabled = te.rendersEnabled || !Application.isPlaying;
    //         Log( $"SoftShadow {name} is calling RebuildRenders() on TriggerElement {te.name}", this );
    //     }
    // }

    
    void OnEnable() {

        if (instantiating || !shadowImage) { return; }
            
        shadowImage.gameObject.SetActive( true );
    }

    
    void OnDisable() {

        if (instantiating || !shadowImage) { return; }
        
        shadowImage.gameObject.SetActive( false );
    }

    
    void OnDestroy() {
        
        if (identifier) { identifier.SelfDestruct(); }
    }
}
