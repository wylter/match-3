using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointsController : MonoBehaviour{

    TextMeshProUGUI pointText;
    Animator animator;
    RectTransform rectTransform;

    public void SetUp(Vector2 position, int points, Color color) {
        pointText = GetComponentInChildren<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();
        rectTransform.position = position;
        pointText.SetText(points.ToString());
        pointText.color = color;
        animator.SetTrigger("SpawnPoints");
    }

    public void NotifyEndFloatPointsAnimation() {
        Destroy(gameObject);
    }
}
