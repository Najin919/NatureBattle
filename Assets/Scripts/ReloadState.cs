using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : StateMachineBehaviour
{
    public float m_Delay = 0.7f;
    public bool m_isReload = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_isReload)
            return;

        if (stateInfo.normalizedTime >= m_Delay)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGameScene")
            {
                WeaponCtrl[] a_Weapon = animator.GetComponentsInChildren<WeaponCtrl>();

                foreach (WeaponCtrl weaponCtrl in a_Weapon)
                {
                    if (GameManager.Inst.m_weapon ==Weapon.Assault57 &&  weaponCtrl.m_WeaponName == "AKM")
                        weaponCtrl.Reload();
                    else if(GameManager.Inst.m_weapon == Weapon.Pistol && weaponCtrl.m_WeaponName == "Pistol")
                        weaponCtrl.Reload();

                }


            }
            else
                animator.GetComponentInChildren<TrainWeaponCtrl>().Reload();

            m_isReload = true;
        }
       
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_isReload = false;
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
