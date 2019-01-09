using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Candy : MonoBehaviour
{
    [SerializeField]
    private CandyColor m_color;
    [SerializeField]
    private Sprite[] m_sprites;

    // Start is called before the first frame update
    void Start(){
        gameObject.GetComponent<Image>().sprite = m_sprites[Random.Range(0, m_sprites.Length)];
    }

}
