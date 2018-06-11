using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities{

    public static float getAnimationLengthByNameInAnimator(string name, Animator animator)
    {
        foreach(AnimationClip anim in animator.runtimeAnimatorController.animationClips)
        {
            if (anim.name == name)
                return anim.length;
        }
        return 0f;
    }

}
