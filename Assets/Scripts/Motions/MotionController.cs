using UnityEngine;

[CreateAssetMenu]
public class MotionController : ScriptableObject {
    [HideInInspector] public AnimatedMotion[] AnimatedMotions = new AnimatedMotion[0];
}
