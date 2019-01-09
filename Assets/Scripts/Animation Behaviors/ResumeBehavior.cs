using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  ResumeBehavior : StateMachineBehaviour
{
    private GameController m_gameController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        if (m_gameController == null) {
            m_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        m_gameController.DismissPause();
    }
}
