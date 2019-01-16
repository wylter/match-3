using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Candy : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    public CandyColor m_color;
    [SerializeField]
    private Sprite[] m_sprites = null;
    [SerializeField]
    private float m_swapTime = 0.3f;

    protected BoardController m_board;

    private Vector2Int m_boardPosition;

    private RectTransform m_rectTranform;

    private Animator m_animator;

    private void Awake() {
        Debug.Assert(m_sprites != null, "Sprites are all null");
    }


    // Start is called before the first frame update
    void Start(){
        SetUp();
    }

    public void SetUp() {
        GetComponent<Image>().sprite = m_sprites[Random.Range(0, m_sprites.Length)];
        m_board = transform.parent.GetComponent<BoardController>();
        m_rectTranform = GetComponent<RectTransform>();
        m_animator = GetComponent<Animator>();
    }

    public void SetUpPosition(Vector2Int boardPosition){
        this.m_boardPosition = boardPosition;
    }

    public Vector2Int getBoardPosition(){
        return m_boardPosition;
    }

    public void OnPointerDown(PointerEventData eventData) {
        m_board.CandyStartTouch(m_boardPosition);
    }

    public void OnPointerUp(PointerEventData eventData){
        m_board.CandyReleaseTouch();
    }

    public void OnPointerEnter(PointerEventData data){
        m_board.CandyOverTouch(m_boardPosition);
    }

    public void MoveTo(Vector2Int newBoardPosition, Vector2 newRectTransformPosition){
        m_board.startedMoving();
        m_boardPosition = newBoardPosition;
        StartCoroutine(Move(newRectTransformPosition));
    }

    public void MoveDown(Vector2Int newBoardPosition, float downDistance) {
        MoveTo(newBoardPosition, new Vector2(m_rectTranform.position.x, m_rectTranform.position.y + downDistance));
    }

    private IEnumerator Move(Vector2 newRectTransformPosition){
        float step = Time.fixedDeltaTime / m_swapTime;
        float t = 0;

        Vector2 start = m_rectTranform.position;

        while (t <= 1.0f){
            t += step; // Goes from 0 to 1, incrementing by step each time
            m_rectTranform.position = Vector2.Lerp(start, newRectTransformPosition, t); // Move objectToMove closer to b
            yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
        }
        m_rectTranform.position = newRectTransformPosition;

        m_board.candyStopped(this);
    }

    public virtual void Die() {
        m_animator.SetTrigger("Die");
        m_board.candyStartedDying();
    }

    public void NotifyEndDeathAnimation() {
        m_board.candyFinishedDying();
        Destroy(gameObject);
    }

}
