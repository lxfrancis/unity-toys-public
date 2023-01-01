using TMPro;
using UnityEngine;

[RequireComponent( typeof( TextMeshProUGUI ))]
[ExecuteAlways]
public class TextShadow: MonoBehaviour {

    static bool instantiating;

    public TextMeshProUGUI shadowText;
    public TMP_FontAsset   fontAsset;
    public Vector2         offset  = new Vector2( -2.0f, -10.0f );
    
    [Range( 0f, 1f )] public float opacity = 1.0f / 3.0f;

    TextMeshProUGUI _mainText;
    TextMeshProUGUI mainText => _mainText ? _mainText : _mainText = GetComponent< TextMeshProUGUI >();

    RectTransform _shadowRtf;
    RectTransform shadowRtf => _shadowRtf ? _shadowRtf : _shadowRtf = shadowText.transform as RectTransform;

    RectTransform _mainRtf;
    RectTransform mainRtf => _mainRtf ? _mainRtf : _mainRtf = transform as RectTransform;
    
    Color shadowColor => new Color( 0.0f, 0.0f, 0.0f, opacity );

    static void MatchTextProperties( TextMeshProUGUI source, TextMeshProUGUI dest ) {

        if (dest.text             != source.text)             { dest.text             = source.text;             }
        if (dest.fontSize         != source.fontSize)         { dest.fontSize         = source.fontSize;         }
        if (dest.lineSpacing      != source.lineSpacing)      { dest.lineSpacing      = source.lineSpacing;      }
        if (dest.characterSpacing != source.characterSpacing) { dest.characterSpacing = source.characterSpacing; }
        if (dest.wordSpacing      != source.wordSpacing)      { dest.wordSpacing      = source.wordSpacing;      }
        if (dest.alignment        != source.alignment)        { dest.alignment        = source.alignment;        }
        if (dest.fontStyle        != source.fontStyle)        { dest.fontStyle        = source.fontStyle;        }

        if (dest.maxVisibleCharacters != source.maxVisibleCharacters) {
            dest.maxVisibleCharacters = source.maxVisibleCharacters;
        }
    }

    void CreateShadow() {

        instantiating = true;

        shadowText = Instantiate( gameObject, mainRtf.parent ).GetComponent< TextMeshProUGUI >();
        
        if (!Application.isPlaying) { DestroyImmediate( shadowText.GetComponent< TextShadow >() ); }
        else { Destroy( shadowText.GetComponent< TextShadow >() ); }
        
        shadowRtf.SetSiblingIndex( mainRtf.GetSiblingIndex() );
        shadowText.font = fontAsset;
        shadowText.name = $"{name} [Shadow]";

        instantiating = false;
    }

    public void CheckState() {

        if (instantiating || !shadowText) { return; }
        
        if (shadowText.color    != shadowColor)       { shadowText.color    = shadowColor;       }
        if (shadowRtf.sizeDelta != mainRtf.sizeDelta) { shadowRtf.sizeDelta = mainRtf.sizeDelta; }

        if ((shadowRtf.anchoredPosition - offset - mainRtf.anchoredPosition).magnitude > 0.001f) {
            shadowRtf.anchoredPosition = mainRtf.anchoredPosition + offset;
        }

        if (shadowRtf.GetSiblingIndex() != mainRtf.GetSiblingIndex() - 1) {
            shadowRtf.SetSiblingIndex( mainRtf.GetSiblingIndex() - 1 );
        }

        if (shadowRtf.localScale    != mainRtf.localScale)    { shadowRtf.localScale    = mainRtf.localScale;    }
        if (shadowRtf.localRotation != mainRtf.localRotation) { shadowRtf.localRotation = mainRtf.localRotation; }

        MatchTextProperties( mainText, shadowText );
    }

    void Update() {

        if (!shadowText) { CreateShadow(); }
        
        CheckState();
    }

    void OnEnable() {

        if (instantiating) { return; }
        
        if (shadowText) { shadowText.gameObject.SetActive( true ); }
    }

    void OnDisable() {

        if (instantiating) { return; }
        
        if (shadowText) { shadowText.gameObject.SetActive( false ); }
    }
}
