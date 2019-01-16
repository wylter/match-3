using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [System.Serializable]
    public class CandyPrefab{
        public Candy candy = null;
        public Candy bombPowerUp = null;
        public Candy freezePowerUp = null;
    }
    [System.Serializable]
    public class ColorAssignment {
        public CandyColor color;
        public Color rgbaColor;
    }

    [Header("Board Settings")]
    [SerializeField]
    private int m_horizontalSize = 8;
    [SerializeField]
    private int m_verticalSize = 8;
    [SerializeField]
    private RectTransform m_spawn = null;
    [SerializeField]
    private int m_distanceUnits = 100;
    [SerializeField]
    private GameController m_gameController = null;

    [Space]
    [Header("Candy Prefabs Settings")]
    [SerializeField]
    public CandyPrefab[] m_candyPrefabs = null;
    [SerializeField]
    public ColorAssignment[] m_candyColor2RGBAColor = null;

    [Space]
    [Header("PowerUp Settings")]
    public int m_selectedPowerUp = 0;
    [SerializeField]
    public int m_powerUpPercentualChance;
    [SerializeField]
    public float m_freezePowerUpSeconds = 5f;

    [Space]
    [Header("Points Settings")]
    [SerializeField]
    private PointsController m_pointsController = null;

    private Dictionary<CandyColor, List<Candy>> m_candyPrefabsDictonary;
    private Dictionary<CandyColor, Color> m_candyColor2RGBAColorDictonary;

    private HashSet<Candy> m_candiesDestroyedByBombs;
    private bool candiesAreExploding = false;

    private Candy[,] m_board;

    private Vector2 m_startSpawnPosition;

    private bool m_touchStarted = false;
    private bool m_canSwap = true;

    private Vector2Int m_startPosition;
    private Vector2Int m_endPosition;

    public int m_movingCandiesNumber = 0;
    private List<Candy> m_movedCandies;
    public int m_dyingCandiesNumber = 0;

    public float m_remeiningPausedTime = -1f;

    private void Awake(){
        Debug.Assert(m_spawn != null, "Spawn is null");
        Debug.Assert(m_candyPrefabs != null, "All candy prefabs are null");
        Debug.Assert(m_candyColor2RGBAColor.Length == System.Enum.GetNames(typeof(CandyColor)).Length, "Please define a color for each CandyColor");
        Debug.Assert(m_pointsController != null, "The points controller is null");
        Debug.Assert(m_gameController != null, "GameController is null");

        m_candyPrefabsDictonary = new Dictionary<CandyColor, List<Candy>>();
        m_candyColor2RGBAColorDictonary = new Dictionary<CandyColor, Color>();
        foreach (CandyPrefab candyPrefab in m_candyPrefabs){
            Debug.Assert(candyPrefab.candy != null, "Candy in prefab vector is null");
            Debug.Assert(candyPrefab.bombPowerUp != null, "Bomb in prefab vector is null");
            Debug.Assert(candyPrefab.freezePowerUp != null, "Freeze in prefab vector is null");

            Debug.Assert(candyPrefab.candy.m_color == candyPrefab.bombPowerUp.m_color && candyPrefab.candy.m_color == candyPrefab.freezePowerUp.m_color, "All prefabs in the same vector index must be the same color");

            List<Candy> candies = new List<Candy>();
            candies.Add(candyPrefab.candy);
            candies.Add(candyPrefab.bombPowerUp);
            candies.Add(candyPrefab.freezePowerUp);
            m_candyPrefabsDictonary.Add(candyPrefab.candy.m_color, candies);
        }
        foreach (ColorAssignment colorAssigned in m_candyColor2RGBAColor) {
            m_candyColor2RGBAColorDictonary.Add(colorAssigned.color, colorAssigned.rgbaColor);
        }
    }

    // Start is called before the first frame update
    void Start(){
        m_startSpawnPosition = m_spawn.anchoredPosition;
        Destroy(m_spawn.gameObject);

        m_movedCandies = new List<Candy>();
        m_candiesDestroyedByBombs = new HashSet<Candy>();

        BoardSetup();
    }

    private int RandomCandyTypeIndex() {
        return (Random.Range(0, 100) > m_powerUpPercentualChance) ? 0 : m_selectedPowerUp;
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
                Candy prefabToInstantiate = m_candyPrefabsDictonary[(CandyColor)possibleColors[Random.Range(0, possibleColors.Count)]][RandomCandyTypeIndex()];
                Candy candy = InstatiateCandyAtPosition(prefabToInstantiate, m_startSpawnPosition + new Vector2(i * m_distanceUnits, j * m_distanceUnits));
                candy.SetUpPosition(new Vector2Int(i, j));

                m_board[i, j] = candy;
            }
        }
    }

    private Candy InstatiateCandyAtPosition(Candy prefabToInstatiate, Vector2 position) {
        Candy candy = Instantiate(prefabToInstatiate, m_startSpawnPosition, prefabToInstatiate.transform.rotation) as Candy;
        candy.gameObject.transform.SetParent(gameObject.transform, false);
        candy.GetComponent<RectTransform>().anchoredPosition = position;

        return candy;
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

        while (m_movingCandiesNumber > 0){
            yield return null;
        }

        HashSet<Candy> candiesToDestory = CheckAllStoppedCandies();

        if (candiesToDestory.Count == 0) {
            SwapCandies();
        } else {
            do {

                DestoryCandies(candiesToDestory);

                while (m_dyingCandiesNumber > 0) {
                    yield return null;
                }

                RefillBoard();

                while (m_movingCandiesNumber > 0) {
                    yield return null;
                }
                candiesToDestory = CheckAllStoppedCandies();
            } while (candiesToDestory.Count > 0);
        }

        while (m_movingCandiesNumber > 0) {
            yield return null;
        }
        m_canSwap = true;
    }

    private void SwapCandies(){
        

        Vector2 startRectPosition = m_board[m_startPosition.x, m_startPosition.y].GetComponent<RectTransform>().position;
        Vector2 endRectPosition = m_board[m_endPosition.x, m_endPosition.y].GetComponent<RectTransform>().position;

        m_board[m_startPosition.x, m_startPosition.y].MoveTo(m_endPosition, endRectPosition);
        m_board[m_endPosition.x, m_endPosition.y].MoveTo(m_startPosition, startRectPosition);

        Candy temp = m_board[m_startPosition.x, m_startPosition.y];
        m_board[m_startPosition.x, m_startPosition.y] = m_board[m_endPosition.x, m_endPosition.y];
        m_board[m_endPosition.x, m_endPosition.y] = temp;
    }

    public void startedMoving() {
        m_movingCandiesNumber++;
    }

    public void candyStopped(Candy candy){
        m_movingCandiesNumber--;
        m_movedCandies.Add(candy);
    }

    public void candyStartedDying() {
        m_dyingCandiesNumber++;
    }

    public void candyFinishedDying() {
        m_dyingCandiesNumber--;
    }

    private HashSet<Candy> CheckAllStoppedCandies(){

        HashSet<Candy> candiesToDestory = new HashSet<Candy>();

        foreach (Candy candy in m_movedCandies){
            int combo = 1;
            int points = 0;
            Vector2Int candyPosition = candy.getBoardPosition();
            List<Candy> comboCandies = new List<Candy>();

            //Search in vertical
            for (int i = 1; candyPosition.y + i < m_verticalSize && m_board[candyPosition.x, candyPosition.y + i].m_color == candy.m_color; i++){
 
                combo++;
                comboCandies.Add(m_board[candyPosition.x, candyPosition.y + i]);
            }
            for (int i = -1; candyPosition.y + i >= 0 && m_board[candyPosition.x, candyPosition.y + i].m_color == candy.m_color; i--)
            {

                combo++;
                comboCandies.Add(m_board[candyPosition.x, candyPosition.y + i]);
            }

            //Check if there was a vertical combo
            if (combo >= 3){
                candiesToDestory.Add(candy);
                candiesToDestory.UnionWith(comboCandies);
                points = 60;
            }

            combo = 1;
            comboCandies.Clear();

            //Search in horizontal
            for (int i = 1; candyPosition.x + i < m_horizontalSize && m_board[candyPosition.x + i, candyPosition.y].m_color == candy.m_color; i++)
            {

                combo++;
                comboCandies.Add(m_board[candyPosition.x + i, candyPosition.y]);
            }
            for (int i = -1; candyPosition.x + i >= 0 && m_board[candyPosition.x + i, candyPosition.y].m_color == candy.m_color; i--)
            {

                combo++;
                comboCandies.Add(m_board[candyPosition.x + i, candyPosition.y]);
            }

            if (combo >= 3){
                candiesToDestory.Add(candy);
                candiesToDestory.UnionWith(comboCandies);
                points = 60;
            }
            if (points > 0) {
                SpawnPoints(candy.GetComponent<RectTransform>().position, 60, candy.m_color);
            }
        }

        m_movedCandies.Clear();



        return candiesToDestory;
    }

    private void DestoryCandies(HashSet<Candy> candiesToDestory) {
        foreach (Candy candy in candiesToDestory) {
            Vector2Int candyPosition = candy.getBoardPosition();
            m_board[candyPosition.x, candyPosition.y] = null;
            candy.Die();
        }

        if (m_candiesDestroyedByBombs.Count > 0) {
            candiesAreExploding = true;
            m_candiesDestroyedByBombs.ExceptWith(candiesToDestory);
            foreach (Candy candy in m_candiesDestroyedByBombs) {
                Vector2Int candyPosition = candy.getBoardPosition();
                m_board[candyPosition.x, candyPosition.y] = null;
                candy.Die();
            }
            m_candiesDestroyedByBombs.Clear();
            candiesAreExploding = false;
        }
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
                    }
                    candiesToSpawn[i] += k;

                    j += k - 1;
                }
            }
        }

        for (int i = 0; i < m_horizontalSize; i++) {
            for (int j = 0; j < m_verticalSize; j++) {
                if (refillMatrix[i, j] < 0 && m_board[i,j]) {
                    m_board[i, j].MoveDown(new Vector2Int(i, j + refillMatrix[i, j]), refillMatrix[i, j] * m_distanceUnits);
                    m_board[i, j + refillMatrix[i, j]] = m_board[i, j];
                }
            }
           
            for (int j = 0; j < candiesToSpawn[i]; j++) {
                Candy prefabToInstantiate = m_candyPrefabsDictonary.Values.ToList()[Random.Range(0, m_candyPrefabsDictonary.Count())][RandomCandyTypeIndex()];
                Candy candy = InstatiateCandyAtPosition(prefabToInstantiate, m_startSpawnPosition + new Vector2(i * m_distanceUnits, (m_verticalSize + j) * m_distanceUnits));

                int verticalIndex = m_verticalSize - candiesToSpawn[i] + j;

                candy.SetUp();
                m_board[i, verticalIndex] = candy;
                candy.MoveDown(new Vector2Int(i, verticalIndex), -(candiesToSpawn[i]) * m_distanceUnits);
            }

        }
    }

    private void SpawnPoints(Vector2 position, int points, CandyColor candyColor) {
        PointsController point = Instantiate(m_pointsController, position, m_pointsController.transform.rotation) as PointsController;
        point.gameObject.transform.SetParent(gameObject.transform, false);
        point.SetUp(position, points, m_candyColor2RGBAColorDictonary[candyColor]);
    }

    public void ActivateFreezePowerUp() {
        if (m_remeiningPausedTime < 0f) {
            m_remeiningPausedTime = m_freezePowerUpSeconds;
            m_gameController.SetTimerActive(false);
            StartCoroutine(CountDownPauseTime());
        } else {
            m_remeiningPausedTime = m_freezePowerUpSeconds;
        }
    }

    private IEnumerator CountDownPauseTime() {
        float timeStep = 0.1f;
        while (m_remeiningPausedTime >= 0) {
            yield return new WaitForSeconds(timeStep);
            m_remeiningPausedTime -= timeStep;
        }
        m_gameController.SetTimerActive(true);
        
    }

    public void ActivateBombPowerUp(Vector2Int bombPosition) {
        if (!candiesAreExploding) {
            for (int i = bombPosition.x - 1; i < bombPosition.x + 2; i++) {
                for (int j = bombPosition.y - 1; j < bombPosition.y + 2; j++) {
                    if (i >= 0 && i < m_verticalSize && j >= 0 && j < m_horizontalSize && m_board[i, j]) {
                        m_candiesDestroyedByBombs.Add(m_board[i, j]);
                    }
                }
            }
        }
        
    }
}
