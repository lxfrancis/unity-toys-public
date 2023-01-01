using System;
using System.Collections.Generic;
using Lx;
using UnityEngine;


[Serializable]
public struct PlainStructs {
    
    public Coord2            coord2b;
    public Coord3            coord3b;
    public Coord2Range       range2b;
    public Coord3Range       range3b;
    public IntRange          intRangeB;
    public FloatRange        floatRangeB;
    public Motion            motionB;
    public RectTransformPair rtfPairB;
}


[Serializable]
public struct InOutPairs {
   
    public InOutPair< Coord2 >             coord2b;
    public InOutPair< Coord3 >             coord3b;
    public InOutPair< Coord2Range >        range2b;
    public InOutPair< Coord3Range >        range3b;
    public DownUpPair< IntRange >          intRangeB;
    public DownUpPair< FloatRange >        floatRangeB;
    public DownUpPair< Motion >            motionB;
    public DownUpPair< RectTransformPair > rtfPairB;
}


[Serializable]
public struct Lists {
   
    public List< Coord2 >            coord2b;
    public List< Coord3 >            coord3b;
    public List< Coord2Range >       range2b;
    public List< Coord3Range >       range3b;
    public List< IntRange >          intRangeB;
    public List< FloatRange >        floatRangeB;
    public List< Motion >            motionB;
    public List< RectTransformPair > rtfPairB;
}


[Serializable]
public struct SerializedNullables {
   
    public SerializedNullable< Coord2 >            coord2b;
    public SerializedNullable< Coord3 >            coord3b;
    public SerializedNullable< Coord2Range >       range2b;
    public SerializedNullable< Coord3Range >       range3b;
    public SerializedNullable< IntRange >          intRangeB;
    public SerializedNullable< FloatRange >        floatRangeB;
    public SerializedNullable< Motion >            motionB;
    public SerializedNullable< RectTransformPair > rtfPairB;
}


[Serializable]
public struct EnumSets {
   
    public EnumSet< CardinalDir, Coord2 >            coord2b;
    public EnumSet< CardinalDir, Coord3 >            coord3b;
    public EnumSet< CardinalDir, Coord2Range >       range2b;
    public EnumSet< CardinalDir, Coord3Range >       range3b;
    public EnumSet< CardinalDir, IntRange >          intRangeB;
    public EnumSet< CardinalDir, FloatRange >        floatRangeB;
    public EnumSet< CardinalDir, Motion >            motionB;
    public EnumSet< CardinalDir, RectTransformPair > rtfPairB;
}


public class LxPropertyDrawerTest: MonoBehaviour {

    public bool indentMe;

    public Coord2            coord2b;
    public Coord3            coord3b;
    public Coord2Range       range2b;
    public Coord3Range       range3b;
    public IntRange          intRangeB;
    public FloatRange        floatRangeB;
    public Motion            motionB;
    public RectTransformPair rtfPairB;

    public PlainStructs        testStruct;
    public InOutPairs          inOutPairs;
    public SerializedNullables serializedNullables;
    public Lists               lists;
    public EnumSets            enumSets;
    
    public EnumSet< CardinalDir, Coord2 >            enumSetcoord2b;
    public EnumSet< CardinalDir, Coord3 >            enumSetcoord3b;
    public EnumSet< CardinalDir, Coord2Range >       enumSetrange2b;
    public EnumSet< CardinalDir, Coord3Range >       enumSetrange3b;
    public EnumSet< CardinalDir, IntRange >          enumSetintRangeB;
    public EnumSet< CardinalDir, FloatRange >        enumSetfloatRangeB;
    public EnumSet< CardinalDir, Motion >            enumSetmotionB;
    public EnumSet< CardinalDir, RectTransformPair > enumSetrtfPairB;
}
