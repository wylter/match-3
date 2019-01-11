﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{

    [SerializeField]
    private GameObject m_pauseMenu;
    [SerializeField]
    private float m_startTime;
    [SerializeField]
    private TextMeshProUGUI m_timerText;

    private float m_remeiningTime;
    private Animator m_pauseAnimator;

    private void Start() {
        m_pauseAnimator = m_pauseMenu.GetComponent<Animator>();
        m_remeiningTime = m_startTime + 1f;
    }

    private void Update() {
        UpdateTimer();
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
        m_timerText.SetText("TIME: " + (int) m_remeiningTime);
    }
}
