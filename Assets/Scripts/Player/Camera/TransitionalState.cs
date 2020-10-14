using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionalState : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isQ", false);
        animator.SetBool("isE", false);
        animator.SetBool("backToLeft", false);
        animator.SetBool("backToRight", false);
        animator.SetBool("backToUp", false);
        animator.SetBool("backToBottom", false);
    }
}
