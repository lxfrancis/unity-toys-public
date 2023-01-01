using System;
using UnityEngine;
using static UnityEngine.Mathf;

[RequireComponent( typeof( RectTransform ) )]
public class Oscillate: MonoBehaviour {

    public float   frequency, phase, rotationSpeed;
    public Vector2 amplitude;

    Vector2?      homePosition;
    RectTransform rtf;

    void Update() {
        
        rtf          ??= transform as RectTransform;
        homePosition ??= rtf!.anchoredPosition;

        rtf!.anchoredPosition
            = homePosition.Value + amplitude * Sin( (Time.time * frequency + phase) * PI * 2.0f );
        
        if (rotationSpeed != 0.0f) { rtf.Rotate( Vector3.forward, rotationSpeed * 360.0f * Time.deltaTime ); }
    }

    void OnDisable() {

        if (homePosition.HasValue) { rtf.anchoredPosition = homePosition.Value; }
    }
}
