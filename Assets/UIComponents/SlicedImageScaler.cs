using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.RectTransform.Axis;

namespace Lx {
    
    [ExecuteAlways]
    [RequireComponent( typeof( Image ))]
    public class SlicedImageScaler: MonoBehaviour {
        
        public enum ScaleMode { ImagePPI, Transform }

        public ScaleMode                   scaleMode;
        public SerializedNullable< float > minPPIMultiplier;
        public bool                        collapseNineSliceCentre;

        Vector2 rectSize, spriteSize;

        Image _image;
        Image image => _image ? _image : _image = GetComponent< Image >();
        
        RectTransform _rtf;
        RectTransform rtf => _rtf ? _rtf : _rtf = GetComponent< RectTransform >();

        void OnValidate() {
            
            Update();
        }

        void Update() {

            if (!image.sprite) { return; }
            
            var rtfRect    = rtf.rect;
            var spriteRect = image.sprite.rect;
            
            // subtract the centre of the 9-slice from the rect size if necessary
            // border x = left, y = bottom, z = right, w = top
            if (collapseNineSliceCentre) {
                
                var border = image.sprite.border;
                // Debug.Log( $"SlicedImageScaler {name} image borders: {image.sprite.border}" );
                spriteRect.height = border.y + border.w;
                spriteRect.width  = border.x + border.z;
            }

            if (spriteRect.size == spriteSize && rtfRect.size == rectSize) { return; }

            spriteSize = spriteRect.size;
            rectSize   = rtfRect.size;

            // other modes not yet implemented
            if (scaleMode != ScaleMode.ImagePPI) { return; }

            var effectivePPI = image.pixelsPerUnit * image.pixelsPerUnitMultiplier;
            
            if (float.IsNaN( effectivePPI )) {
                
                Debug.LogWarning( $"SlicedImageScaler on '{name}' encountered a NaN current PPI; aborting'" );
                goto AbortLog;
            }
            
            var spriteAspect = spriteRect.width / spriteRect.height;
            
            if (float.IsNaN( spriteAspect )) {
                
                Debug.LogWarning( $"SlicedImageScaler on '{name}' encountered a NaN sprite aspect; aborting'" );
                goto AbortLog;
            }

            var rectAspect = rtfRect.width / rtfRect.height;
            
            if (float.IsNaN( rectAspect )) {
                
                Debug.LogWarning( $"SlicedImageScaler on '{name}' encountered a NaN rect transform aspect; aborting'" );
                goto AbortLog;
            }

            var shortAxis = rectAspect > spriteAspect ? Vertical : Horizontal;
            var targetPPI = shortAxis == Vertical ? spriteRect.height / rtfRect.height
                                                  : spriteRect.width  / rtfRect.width;
            
            if (float.IsNaN( targetPPI )) {
                
                Debug.LogWarning( $"SlicedImageScaler on '{name}' calculated a NaN target PPI; aborting'" );
                goto AbortLog;
            }

            var oldMultiplier = image.pixelsPerUnitMultiplier;
            
            image.pixelsPerUnitMultiplier *= targetPPI / effectivePPI;
            
            if (float.IsNaN( image.pixelsPerUnitMultiplier )) {
                
                Debug.LogWarning( $"SlicedImageScaler on '{name}' set a NaN PPI multiplier; reverting'" );
                image.pixelsPerUnitMultiplier = oldMultiplier;
                if (float.IsInfinity( image.pixelsPerUnitMultiplier )) { image.pixelsPerUnitMultiplier = 1f; }
                goto AbortLog;
            }

            if (minPPIMultiplier.HasValue && image.pixelsPerUnitMultiplier < minPPIMultiplier && minPPIMultiplier > 0.0f) {
                image.pixelsPerUnitMultiplier = minPPIMultiplier;
            }
            return;
            
            AbortLog:
            Debug.Log( $"SlicedImageScaler.Update(); name: {name}; rtf rect: {rtfRect}; sprite rect: {spriteRect}; base PPI: {image.pixelsPerUnit}; current multiplier: {image.pixelsPerUnitMultiplier}" );
        }
    }
}
