using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadHighScore : MonoBehaviour
{

    void Start(){
        GetComponent<TextMeshProUGUI>().SetText("HIGHSCORE: " + PlayerPrefs.GetInt("highscore"));
    }

}
