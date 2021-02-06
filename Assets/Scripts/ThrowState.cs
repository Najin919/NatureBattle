using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowState : StateMachineBehaviour
{
    float m_Delay = 0.63f;
    bool m_isThrow = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= 0.15f && stateInfo.normalizedTime <= 0.2f)//무기 render 꺼주기
        {
            animator.GetComponentInParent<PlayerCtrl>().SetAKM(false);
            animator.GetComponentInParent<PlayerCtrl>().SetPistol(false);
        }

        else if (!m_isThrow && stateInfo.normalizedTime >= m_Delay)
        {
            animator.GetComponentInParent<PlayerCtrl>().CreateGrenadePrefab();
            m_isThrow = true;
        }
        else if (stateInfo.normalizedTime > 0.85f && stateInfo.normalizedTime <= 0.9f)//무기 render 켜주기
        {
            if (animator.GetBool("Assault") == true)
                animator.GetComponentInParent<PlayerCtrl>().SetAKM(true);
            else if (animator.GetBool("Pistol") == true)
                animator.GetComponentInParent<PlayerCtrl>().SetPistol(true);

        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_isThrow = false;
    }

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
