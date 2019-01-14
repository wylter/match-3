using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [System.Serializable]
    public class CandyPrefab{
        public CandyColor color;
        public Candy candy;
    }

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
    private int test;
    [SerializeField]
    public CandyPrefab[] m_candyPrefabs;

    private Dictionary<CandyColor, Candy> m_candyPrefabsDictonary;

    private Candy[,] m_board;

    private Vector2 m_startSpawnPosition;

    private bool m_touchStarted = false;
    private bool m_canSwap = true;

    private Vector2Int m_startPosition;
    private Vector2Int m_endPosition;

    private int m_movingCandiesNumber = 0;
    private List<Candy> m_movedCandies;

    private void Awake(){
        m_candyPrefabsDictonary = new Dictionary<CandyColor, Candy>();
        foreach (CandyPrefab candyPrefab in m_candyPrefabs){
            m_candyPrefabsDictonary.Add(candyPrefab.color, candyPrefab.candy);
        }
    }

    // Start is called before the first frame update
    void Start(){
        m_startSpawnPosition = m_spawn.anchoredPosition;
        Destroy(m_spawn.gameObject);

        m_movedCandies = new List<Candy>();

        BoardSetup();
    }

    private void BoardSetup() {
        m_board = new Candy[m_horizontalSize, m_verticalSize];


        for (int i = 0; i < m_horizontalSize; i++) {
            for (int j = 0; j < m_verticalSize; j++) {
                //******Check the possible colors to add to the board
                List<int> possibleColors = new List<int>();
                possibleColors.AddRange(Enumerable.Range(0,m_candyPrefabs.Count()));
                if (i >= 2){
                    if (m_board[i - 1,j].m_color == m_board[i - 2, j].m_color){
                        possibleColors.Remove((int)m_board[i - 1, j].m_color);
                    }
                }
                if (j >= 2){
                    if (m_board[i, j - 1].m_color == m_board[i, j - 2].m_color){
                        possibleColors.Remove((int)m_board[i, j - 1].m_color);
                    }
                }

                //******Candy Assignement
                Candy toInstantiate = m_candyPrefabsDictonary[(CandyColor)possibleColors[Random.Range(0, possibleColors.Count)]];
                Candy candy = Instantiate(toInstantiate, m_startSpawnPosition, toInstantiate.transform.rotation) as Candy;
                candy.gameObject.transform.SetParent(gameObject.transform, false);
                candy.GetComponent<RectTransform>().anchoredPosition = m_startSpawnPosition + new Vector2(i * m_distanceUnits, j * m_distanceUnits);
                candy.SetUpPosition(new Vector2Int(i, j));
                m_board[i, j] = candy;
            }
        }
    }

    public void CandyStartTouch(Vector2Int candyPosition){
        m_startPosition = candyPosition;
        m_touchStarted = true;
    }

    public void CandyReleaseTouch()
    {
        m_touchStarted = false;
    }

    public void CandyOverTouch(Vector2Int candyPosition)
    {
        if (!m_touchStarted || !m_canSwap){
            return;
        }

        if ((m_startPosition - candyPosition).magnitude == 1){
            m_canSwap = false;
            m_touchStarted = false;
            m_endPosition = candyPosition;
            StartCoroutine(TryToSwapCandies());
        }
    }

    private IEnumerator TryToSwapCandies(){
        SwapCandies();

        m_movingCandiesNumber += 2;

        while (m_movingCandiesNumber > 0){
            yield return null;
        }

        if (!CheckAllStoppedCandies()){
            SwapCandies();
            m_movingCandiesNumber += 2;
        } else {
            RefillBoard();
        }

        while (m_movingCandiesNumber > 0) {
            yield return null;
        }
        m_canSwap = true;
    }

    private void SwapCandies(){
        

        Vector2 startRectPosition = m_board[m_startPosition.x, m_startPosition.y].GetComponent<RectTransform>().position;
        Vector2 endRectPosition = m_board[m_endPosition.x, m_endPosition.y].GetComponent<RectTransform>().position;

        Debug.Log("Swapping " + m_startPosition.x + ":" + m_startPosition.y + "  " + m_endPosition.x + ":" + m_endPosition.y);
        Debug.Log("---- " + startRectPosition.x + ":" + startRectPosition.y + "  " + endRectPosition.x + ":" + endRectPosition.y);

        m_board[m_startPosition.x, m_startPosition.y].MoveTo(m_endPosition, endRectPosition);
        m_board[m_endPosition.x, m_endPosition.y].MoveTo(m_startPosition, startRectPosition);

        Candy temp = m_board[m_startPosition.x, m_startPosition.y];
        m_board[m_startPosition.x, m_startPosition.y] = m_board[m_endPosition.x, m_endPosition.y];
        m_board[m_endPosition.x, m_endPosition.y] = temp;
    }

    public void candyStopped(Candy candy){
        m_movingCandiesNumber--;
        m_movedCandies.Add(candy);
    }

    private bool CheckAllStoppedCandies(){

        List<Candy> candiesToDestory = new List<Candy>();

        foreach (Candy candy in m_movedCandies){
            int combo = 1;
            Vector2Int candyPosition = candy.getBoardPosition();
            List<Candy> comboCandies = new List<Candy>();

            //Search in vertical
            for (int i = 1; !(candyPosition.y + i >= m_verticalSize || m_board[candyPosition.x, candyPosition.y + i].m_color != candy.m_color); i++){
 
                combo++;
                comboCandies.Add(m_board[candyPosition.x, candyPosition.y + i]);
            }
            for (int i = -1; !(candyPosition.y + i < 0 || m_board[candyPosition.x, candyPosition.y + i].m_color != candy.m_color); i--)
            {

                combo++;
                comboCandies.Add(m_board[candyPosition.x, candyPosition.y + i]);
            }

            //Check if there was a vertical combo
            if (combo >= 3){
                candiesToDestory.Add(candy);
                candiesToDestory.AddRange(comboCandies);
            }

            combo = 1;
            comboCandies.Clear();

            //Search in horizontal
            for (int i = 1; !(candyPosition.x + i >= m_horizontalSize || m_board[candyPosition.x + i, candyPosition.y].m_color != candy.m_color); i++)
            {

                combo++;
                comboCandies.Add(m_board[candyPosition.x + i, candyPosition.y]);
            }
            for (int i = -1; !(candyPosition.x + i < 0 || m_board[candyPosition.x + i, candyPosition.y].m_color != candy.m_color); i--)
            {

                combo++;
                comboCandies.Add(m_board[candyPosition.x + i, candyPosition.y]);
            }

            if (combo >= 3){
                candiesToDestory.Add(candy);
                candiesToDestory.AddRange(comboCandies);
            }
        }

        m_movedCandies.Clear();

        foreach (Candy candy in candiesToDestory) {
            Vector2Int candyPosition = candy.getBoardPosition();
            m_board[candyPosition.x, candyPosition.y] = null;
            Destroy(candy.gameObject);
        }

        return candiesToDestory.Count > 0;
    }

    private void RefillBoard(){

        int[,] refillMatrix = new int[m_horizontalSize, m_verticalSize];
        int[] candiesToSpawn = new int[m_horizontalSize];

        
        for (int i = 0; i < m_horizontalSize; i++) {
            for (int j = 0; j < m_verticalSize; j++) {
                if (m_board[i, j] == null) {
                    int lowerPosition = j;
                    int k = 1;
                    while (j + k < m_verticalSize && m_board[i, j + k] == null) {
                        k++;
                    }

                    for (int l = j + k; l < m_verticalSize; l++) {
                        refillMatrix[i, l] -= k;
                        candiesToSpawn[i] += k;
                    }

                    j += k - 1;
                }
            }
        }

        for (int i = 0; i < m_horizontalSize; i++) {
            for (int j = 0; j < m_verticalSize; j++) {
                if (refillMatrix[i, j] < 0) {
                    m_movingCandiesNumber++;
                    m_board[i, j].MoveDown(new Vector2Int(i, j + refillMatrix[i, j]), refillMatrix[i, j] * m_distanceUnits);
                    m_board[i, j + refillMatrix[i, j]] = m_board[i, j];
                }
            }
        }
    }
}
