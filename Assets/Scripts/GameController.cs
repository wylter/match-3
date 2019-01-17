using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    
    [Header("UI Settings")]
    [SerializeField]
    private GameObject m_pauseMenu = null;
    [SerializeField]
    private TextMeshProUGUI m_timerText = null;
    [SerializeField]
    private GameObject m_freezeImage = null;

    private float m_remeiningTime;
    private Animator m_pauseAnimator;
    private bool m_timerIsActive = true;

    public float m_startTime = 60f;

    private void Awake() {
        Debug.Assert(m_timerText != null, "Timer text is null");
        Debug.Assert(m_pauseMenu != null, "Pause Menu is null");
        Debug.Assert(m_freezeImage != null, "Freeze Image is null");
    }

    private void Start() {
        m_pauseAnimator = m_pauseMenu.GetComponent<Animator>();
        m_remeiningTime = m_startTime + 1f;
    }

    private void Update() {
        if (m_timerIsActive) {
            UpdateTimer();
        }
    }


    public void Pause() {
        Time.timeScale = 0;
        m_pauseMenu.SetActive(true);
        m_pauseAnimator.SetTrigger("Pause");
    }

    public void Resume() {
        m_pauseAnimator.SetTrigger("Resume");
    }

    public void DismissPause() {
        Time.timeScale = 1;
        m_pauseMenu.SetActive(false);
    }

    private void UpdateTimer() {
        m_remeiningTime -= Time.deltaTime;
        m_timerText.SetText(((int)m_remeiningTime).ToString());
    }

    public void SetTimerActive(bool active) {
        m_timerIsActive = active;
        m_freezeImage.SetActive(!active);
    }
}
