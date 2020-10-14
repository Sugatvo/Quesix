using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class OnChangeState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        animator.SetBool("backToLeft", false);
        animator.SetBool("backToRight", false);
        animator.SetBool("backToUp", false);
        animator.SetBool("backToBottom", false);
    }

    // OnStateExit is called when a transition ends and the state 
    //machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isQ", false);
        animator.SetBool("isE", false);

        if (stateInfo.IsName("Left")){
            UnityEngine.Debug.Log("Left");
        }

        if (stateInfo.IsName("Bottom"))
        {
            UnityEngine.Debug.Log("Bottom");
        }

        if (stateInfo.IsName("Right"))
        {
            UnityEngine.Debug.Log("Right");
        }

        if (stateInfo.IsName("Up"))
        {
            UnityEngine.Debug.Log("Up");
        }
    }

}
