using Lx;
using UnityEngine;

public class ColorSetGenerator: MonoBehaviour {

    [Range(    0f,   1.0f )] public float luma;
    [Range(    0f,   0.5f )] public float chroma;
    [Range( -360f, 360.0f )] public float hueMin, hueMax;

    public Color[] colors;

    [HideInInspector] public bool outOfGamut;

    void OnValidate() {

        var redYUV = new ColorYUV( Color.red );

        var redPhase = Mathf.Atan2( redYUV.v, redYUV.u );

        outOfGamut = false;

        for (int i = 0; i < colors?.Length; i++) {

            float hue = Mathf.Lerp( hueMin, hueMax, (float) i / (colors.Length - 1) ) * Mathf.Deg2Rad + redPhase;

            var yuv = new ColorYUV( luma, Mathf.Cos( hue ) * chroma, Mathf.Sin( hue ) * chroma, 1.0f );

            colors[ i ] = yuv.ToRGB( zeroAlphaOutOfGamut: true );

            if (colors[ i ].a == 0.0f) {

                outOfGamut    = true;
                colors[ i ].a = 1.0f;
            }
        }
    }
}
