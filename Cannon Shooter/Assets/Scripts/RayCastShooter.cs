using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class RayCastShooter : MonoBehaviour {

    public GameObject dotPrefab;
    public GameObject m_menuOnGame = null;
    public GameObject m_menuEndGame = null;
    public GameObject m_menuGameOver = null;
    public Toggle m_helpPath = null;

    private bool mouseDown = false;
    private List<Vector2> dots;
    private List<GameObject> dotsPool;
    private int maxDots = 26;

    private float dotGap = 0.32f;

    private bool started = false;


    // Use this for initialization
    void Start() {

        if (PlayerPrefs.HasKey("helpPath")) {
            if (PlayerPrefs.GetInt("helpPath") == 1) {
                m_helpPath.isOn = true;
            } else {
                m_helpPath.isOn = false;
            }
        }

        if (m_helpPath.isOn) {

            dots = new List<Vector2>();
            dotsPool = new List<GameObject>();

            var i = 0;
            var alpha = 1.0f / maxDots;
            var startAlpha = 1.0f;
            while (i < maxDots) {
                var dot = Instantiate(dotPrefab) as GameObject;
                dot.GetComponent<Renderer>().material.color = Color.white;

                startAlpha -= alpha;

                dot.SetActive(false);
                dotsPool.Add(dot);
                i++;
            }
            started = true;
        }
    }

    void HandleTouchDown(Vector2 touch) {
    }

    void HandleTouchUp(Vector2 touch) {
        if (touch.y < 280 || m_menuOnGame.activeSelf || m_menuEndGame.activeSelf || m_menuGameOver.activeSelf)
            return;

        if (dots == null || dots.Count < 2)
            return;

        foreach (var d in dotsPool)
            d.SetActive(false);

        InitPath();
    }

    void HandleTouchMove(Vector2 touch) {
        if (touch.y < 280 || m_menuOnGame.activeSelf || m_menuEndGame.activeSelf || m_menuGameOver.activeSelf || !Input.GetMouseButton(0))
            return;

        if (dots == null) {
            return;
        }

        dots.Clear();

        foreach (var d in dotsPool)
            d.SetActive(false);

        //Vector2 point = Camera.main.ScreenToWorldPoint(touch);
        
        //Debug.Log("Point: " + point);
        

        //var direction = new Vector2(point.x - transform.position.x, point.y - transform.position.y);
        var pt = Input.mousePosition;
        var o = Camera.main.WorldToScreenPoint( transform.position );
        o.y = Mathf.Clamp( o.y, 0, 9.75f );
        o.z = 0;
        pt.z = 0;
        var direction = pt - o;

        Debug.Log("Direction: " + direction);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction);
        Debug.Log("Hit: " + hit.point);
        Debug.Log("Hit results: " + hit);
        if (hit.collider != null) {
            dots.Add(transform.position);
            Debug.Log("Hit Collider Tag: " + hit.collider.tag);
            if (hit.collider.tag == "SideWall") {
                DoRayCast(hit, direction);
            } else {
                dots.Add(hit.point);
                
                DrawPaths();
            }
        }
    }

    void DoRayCast(RaycastHit2D previousHit, Vector2 directionIn) {

        dots.Add(previousHit.point);

        var normal = Mathf.Atan2(previousHit.normal.y, previousHit.normal.x);
        var newDirection = normal + (normal - Mathf.Atan2(directionIn.y, directionIn.x));
        var reflection = new Vector2(-Mathf.Cos(newDirection), -Mathf.Sin(newDirection));
        var newCastPoint = previousHit.point + (2 * reflection);

        var hit2 = Physics2D.Raycast(newCastPoint, reflection);
        if (hit2.collider != null) {

            if (hit2.collider.CompareTag("SideWall")) {
                //Debug.Log("Wall");
                //shoot another cast
                DoRayCast(hit2, reflection);
            } else {
                dots.Add(hit2.point);
                DrawPaths();
            }
        } else {
            DrawPaths();
        }
    }


    // Update is called once per frame
    void Update() {
        if (!started && m_helpPath.isOn) {
            Start();
        } else if (started && !m_helpPath.isOn) {
            for (int i = 0; i < maxDots; i++) {
                Destroy(dotsPool[i]);
            }
            started = false;
            dots = null;
        }

        if (dots == null)
            return;
        if (gameObject.transform.rotation.z > -0.5f && gameObject.transform.rotation.z < 0.5f) {
            if (Input.touches.Length > 0) {

                Touch touch = Input.touches[0];

                if (touch.phase == TouchPhase.Began) {
                    HandleTouchDown(touch.position);
                } else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) {
                    HandleTouchUp(touch.position);
                } else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
                    HandleTouchMove(touch.position);
                }
                HandleTouchMove(touch.position);
                return;
            } else if (Input.GetMouseButtonDown(0)) {
                mouseDown = true;
                HandleTouchDown(Input.mousePosition);
            } else if (Input.GetMouseButtonUp(0)) {
                mouseDown = false;
                HandleTouchUp(Input.mousePosition);
            } else if (mouseDown) {
                HandleTouchMove(Input.mousePosition);
            }
        }
    }

    void DrawPaths() {

        if (dots.Count > 1) {

            foreach (var d in dotsPool)
                d.SetActive(false);

            int index = 0;

            for (var i = 1; i < dots.Count; i++) {
                DrawSubPath(i - 1, i, ref index);
            }
        }
    }

    void DrawSubPath(int start, int end, ref int index) {
        var pathLength = Vector2.Distance(dots[start], dots[end]);

        int numDots = Mathf.RoundToInt((float)pathLength / dotGap);
        float dotProgress = 1.0f / numDots;

        var p = 0.0f;

        while (p < 1) {
            var px = dots[start].x + p * (dots[end].x - dots[start].x);
            var py = dots[start].y + p * (dots[end].y - dots[start].y);

            if (index < maxDots) {
                var d = dotsPool[index];
                d.transform.position = new Vector2(px, py);
                d.SetActive(true);
                index++;
            }

            p += dotProgress;
        }
    }

    void InitPath() {
        var start = dots[0];
        var end = dots[1];
        var length = Vector2.Distance(start, end);
        var iterations = length / 0.15f;
    }

}
