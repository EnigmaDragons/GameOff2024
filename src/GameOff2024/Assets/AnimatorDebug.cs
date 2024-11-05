using UnityEngine;

public class AnimatorDebug : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            Debug.Log("Jump");
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("FallingBlend"))
        {
            Debug.Log("FallingBlend");
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
        {
            Debug.Log("Landing");
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("WalkCrouchBlend"))
        {
            Debug.Log("WalkCrouchBlend");
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("SprintBlend"))
        {
            Debug.Log("SprintBlend");
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
