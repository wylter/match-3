using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField]
    private int m_horizontalSize;
    [SerializeField]
    private int m_verticalSize;
    [SerializeField]
    private RectTransform m_spawn;
    [SerializeField]
    private int m_distanceUnits = 100;

    [Space]
    [Header("Candy Prefabs Settings")]
    [SerializeField]
    private Candy[] m_candyPrefabs;

    private Candy[,] m_board;

    private Vector2 m_startSpawnPosition;

    // Start is called before the first frame update
    void Start(){
        m_startSpawnPosition = m_spawn.anchoredPosition;
        Destroy(m_spawn.gameObject);

        BoardSetup();
    }

    private void BoardSetup() {
        m_board = new Candy[m_horizontalSize, m_verticalSize];

        for (int i = 0; i < m_horizontalSize; i++) {
            for (int j = 0; j < m_verticalSize; j++) {
                Candy toInstantiate = m_candyPrefabs[Random.Range(0, m_candyPrefabs.Length)];
                Candy candy = Instantiate(toInstantiate, m_startSpawnPosition, toInstantiate.transform.rotation) as Candy;
                candy.gameObject.transform.SetParent(gameObject.transform, false);
                candy.GetComponent<RectTransform>().anchoredPosition = m_startSpawnPosition + new Vector2(i * m_distanceUnits, j * m_distanceUnits);
                m_board[i, j] = candy;
            }
        }
    }
}
