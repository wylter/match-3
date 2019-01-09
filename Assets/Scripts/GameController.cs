using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    [SerializeField]
    private GameObject m_pauseMenu;

    private Animator m_pauseAnimator;

    private void Start() {
        m_pauseAnimator = m_pauseMenu.GetComponent<Animator>();
    }

    public void Pause() {
        m_pauseMenu.SetActive(true);
        m_pauseAnimator.SetTrigger("Pause");
    }

    public void Resume() {
        m_pauseAnimator.SetTrigger("Resume");
    }

    public void DismissPause() {
        m_pauseMenu.SetActive(false);
    }
}
