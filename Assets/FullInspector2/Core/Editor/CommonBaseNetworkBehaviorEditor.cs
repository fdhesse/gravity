﻿#if !(UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0)
using UnityEditor;

namespace FullInspector.Internal {
    [CustomEditor(typeof(CommonBaseNetworkBehavior), true)]
    public class CommonBaseNetworkBehaviorEditor : FullInspectorCommonSerializedObjectEditor {
    }
}
#endif