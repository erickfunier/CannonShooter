using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CannonSoul : MonoBehaviour {

    const float TweenTime = 0.25f;
    const float Sqrt3Div2 = 0.86602540378443864676372317075294f;
    const float Sqrt3 = 1.7320508075688772935274463415059f;
    const float BoundFrameWidth = 0.1f;

    class BallCell {
        public int x;
        public int y;
        public GameObject go;
        public Material mat;
        public int color;
    }

    struct NPlane {
        public Vector3 point;
        public Vector3 normal;
    }

    struct ballCoord {
        public int x;
        public int y;
        public ballCoord(int _x, int _y) { x = _x; y = _y; }
    }

    class FlyingBall {
        public bool stopped;
        public ballCoord destResult;
        public Transform transform;
        public float radius;
        public Vector3 movedir;
        public float speed;
        public int color;
        public void DestroyBall() {
            if (transform != null) {
                Destroy(transform.gameObject);
            }
        }
    }

    class CellLine {
        public BallCell[] cells;
        public Transform root;
    }

    // Levels txt file
    public TextAsset levelData = null;      

    // UI variables
    public GameObject canvasPanelOnGame = null;
    public GameObject canvasMenuOnGame = null;
    public GameObject canvasPanelEndGame = null;
    public GameObject canvasPanelGameOver = null;
    public GameObject canvasPanelConfirm = null;
    public GameObject canvasMenuEndGame = null;
    public GameObject canvasMenuGameOver = null;
    public GameObject canvasMenuConfirm = null;
    public GameObject canvasBtnSettings = null;
    public GameObject menuOnGame = null;    
    public GameObject menuEndGame = null;    
    public GameObject menuGameOver = null;    
    public GameObject menuConfirm = null;
    public GameObject btnSettings = null;
    public GameObject txtHurryUp = null;
    public GameObject txtMenuEndGame = null;
    public GameObject txtMenuGameOver = null;
    public GameObject txtTitleScore = null;
    public GameObject txtUserScore1 = null;
    public GameObject txtUserScore2 = null;
    public GameObject txtUserScore3 = null;
    public GameObject txtUserScore4 = null;
    public GameObject txtUserScore5 = null;
    public Toggle toggleHelpPath = null;
    public Toggle toggleHurryUp = null;
    public Toggle togglePlayMusic = null;
    public Toggle togglePlaySounds = null;

    // Assets GameObject
    public GameObject ballGO = null;
    public GameObject remainsGO = null;

    // Scene Gameplay
    public GameObject cylinder = null;
    public Transform cylinderAxis = null;
    public Transform ring = null;
    public Transform cannon = null;
    public Transform wheel = null;

    // Materials
    public Material transpMaterial = null;

    // Audio Variables
    float volume = 0.5f;
    public AudioSource audioSource;
    public AudioSource audioSourceBG;
    public AudioClip leftCannonMove;
    public AudioClip rightCannonMove;
    public AudioClip leftCannonMove2;
    public AudioClip rightCannonMove2;
    public AudioClip cannon0;
    public AudioClip cannon1;
    public AudioClip cannon2;
    public AudioClip ballSweep;
    public AudioClip ballExplode;
    public AudioClip ballCollision;
    public AudioClip ballRemains;
    public AudioClip audioGate;
    public AudioClip bgMainMenu;

    // Background images
    public GameObject bgGO = null;
    public GameObject gateGO = null;
    public Sprite bgCave = null;
    public Sprite gateCave = null;
    public Sprite bgWall = null;
    public Sprite gateWall = null;
    public Sprite bgBuild = null;
    public Sprite gateBuild = null;
    public Sprite bgFuture = null;
    public Sprite gateFuture = null;

    // Control variables
    private int maxLine = 10;
    private int deadLine = 15;
    private int initLineCount = 5;
    private float initialSpeed = 20;
    private float aimOffsetBg = 0;

    // Dockers
    Camera mainCamera = null;
    Transform cellRoot = null;
    Transform aimer = null;
    Transform currentTransform = null;
    Transform bounds = null;
    Transform gate = null;
    
    // Level metrics
    float boundsHeight = 0;
    float lineHeight = 0;
    float ballRadius = 0;
    static float bgScaleDefault = 0.52f;
    static int aspectYDefault = 16;
    static float sphereRadiusDefault = 0.625f;
    static float gateOffsetDefault = 0.5413f;
    static int frameWidthDefault = 5;
    static int scaleFactorDefault = 1;

    // Level data
    float gateOffset = 0.5413f;
    int screenOffset = 0;
    int level = -1;
    int shootCount = 0;
    int totalLines = 0;
    int firstLineOddLeading = 0;
    int[][] ballLevelData = null;
    int ballDataOffset = -1;
    static bool levelUpdating = false;
    List<CellLine> buffer = null;
    NPlane[] planes = null;
    int actualLine = 0;
    int activeBalls = 0;
    List<int> activeColors = new List<int>();
    private System.Diagnostics.Stopwatch zeit = new System.Diagnostics.Stopwatch();
    private System.Diagnostics.Stopwatch hurryTime = new System.Diagnostics.Stopwatch();
    int levelParity = 0;
    private int removedBalls = 0;
    int cannonId = 0;

    // Ball State
    int shootBallState = 0;
    int shootBallColor = 0;
    FlyingBall flyingBall = null;
    int flagTest = 0;

    // use to check ball eliminating
    List<ballCoord> openTable = new List<ballCoord>();
    List<BallCell> eraseList = new List<BallCell>();
    HashSet<ballCoord> closedTable = new HashSet<ballCoord>();

    static Color[] ColorPalette = new Color[] {
        Color.grey,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.red,
        Color.cyan,
        Color.black,
        Color.white,
    };

    static Color GetColor(int colorIndex) {
        return ColorPalette[(colorIndex - 1) % ColorPalette.Length];
    }

    static void SetColor(GameObject go, Material mat, int colorIndex) {
        if (mat != null) {
            mat.color = GetColor(colorIndex);
        }
    }

    static void FadeInBall(Material mat, int colorIndex, Action callback = null, float alpha = 0.4f) {
        if (mat != null) {
            var color = ColorPalette[(colorIndex - 1) % ColorPalette.Length];
            color.a = 0.0f;
            mat.color = color;
            color.a = 1;
            var t = mat.DOColor(color, TweenTime);
            if (callback != null) {
                t.onComplete += () => callback();
            }
        }
    }

    static void FadeOutBall(Material mat, Action callback) {
        if (mat != null) {
            var color = mat.color;
            color.a = 0;
            var t = mat.DOColor(color, TweenTime);
            if (callback != null) {
                t.onComplete += () => callback();
            }
        }
    }

    static void FadeOutDestroy(BallCell b, GameObject remains) {
        var go = b.go;
        var mat = b.mat;
        var color = b.color;
     
        //var remains = Resources.Load<GameObject>("remains");
        b.color = 0;
        b.go = null;
        b.mat = null;

        if (go != null) {
            var scale = go.transform.localScale;
            
            go.transform.DOScale(scale * 2, TweenTime);
            
        }
        
        Instantiate(remains, new Vector3(go.transform.position.x, go.transform.position.y, -2), go.transform.rotation);
        Component[] renderers = remains.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer childRenderer in renderers) {
            childRenderer.sharedMaterial.color = ColorPalette[color - 1];
        }
        FadeOutBall(
            mat,
            () => {
                
                if (go != null) {
                    var parent = go.transform.parent;
                    
                    go.transform.parent = null;

    
                    Destroy(go);
                    go = null;
                    if (parent != null && parent.childCount == 0) {
                        Destroy(parent.gameObject);
                    }
                }
            }
        );
        
    }

    static void DropDestroy(BallCell b) {
        levelUpdating = true;
        var go = b.go;
        var mat = b.mat;
        b.color = 0;
        b.go = null;
        b.mat = null;
        if (go != null) {
            var t = go.transform.DOShakePosition(1.0f, new Vector3(0.25f, 0.35f, 0), 20);
            t.onComplete += () => {
                if (go != null) {
                    var pos = go.transform.localPosition;
                    pos.y += 4.0f;
                    go.transform.DOLocalMove(pos, TweenTime * 2);
                    FadeOutBall(
                        mat,
                        () => {
                            if (go != null) {
                                var parent = go.transform.parent;
                                go.transform.parent = null;
                                Destroy(go);
                                go = null;
                                if (parent != null && parent.childCount == 0) {
                                    Destroy(parent.gameObject);
                                }
                            }
                        }
                    );
                }
            };
        }
    }

    public static Vector2 GetAspectRatio(int x, int y) {
        float f = (float)x / (float)y;
        int i = 0;
        while (true)
        {
            i++;
            if (Math.Round(f * i, 2) == Mathf.RoundToInt(f * i))
                break;
        }
        return new Vector2((float)Math.Round(f * i, 2), i);
    }
    
    void UpdateColors() {
        activeColors = new List<int>();

        for (int j = actualLine; j <= totalLines; j++) {
            var cells = GetLine(j);

            if (cells != null) {
                for (int i = 0; i < cells.Length; i++)
                {
                    var bi = cells[i];
                    if (bi != null && bi.go != null && bi.color != 9) {
                        if (activeColors == null || !activeColors.Contains(bi.color))
                        {
                            activeColors.Add(bi.color);
                            //Debug.Log("i: " + i + " j: " + j + " Color: " + bi.color);
                        }
                    }
                }
            }
        }
        
        
        /*for(int i = 0; i < active_colors.Count; i++)
        {
            //Debug.Log("i: " + i);
            Debug.Log(active_colors[i]);
        }*/
    }

    BallCell StickToBuffer(ref FlyingBall ball) {
        if (!ball.stopped) {
            return null;
        }
        var cell = ball.destResult;

        CellLine line;
        if (cell.y >= buffer.Count) {
            line = new CellLine {
                cells = new BallCell[ballLevelData[0].Length],
                root = null
            };
            buffer.Insert(0, line);
        } else {
            line = buffer[buffer.Count - 1 - cell.y];
        }
        var slot = line.cells[cell.x] ?? new BallCell();
        if (slot != null && slot.go != null) {
            return null;
        }
        line.cells[cell.x] = slot;
        var x = ballRadius * cell.x + ballRadius;
        var y = (cell.y * lineHeight + ballRadius);
        slot.x = cell.x;
        slot.y = (cell.y);
        GameObject go = Instantiate(ballGO);
        go.name = String.Format("cell+{0}", cell.x);
        if (line.root == null) {
            var lineRoot = new GameObject(String.Format("line+{0}", buffer.Count)).transform;
            lineRoot.parent = cellRoot;
            lineRoot.localScale = Vector3.one;
            line.root = lineRoot;
        }
        go.transform.parent = line.root;
        go.transform.localPosition = ball.transform.localPosition;
        go.transform.DOLocalMove(new Vector3(x, y, 0), TweenTime * 0.5f);

        Material mat = go.GetComponent<Renderer>().material;
        slot.go = go;
        slot.mat = mat;
        slot.color = ball.color;
        SetColor(go, mat, ball.color);
        return line.cells[cell.x];
    }

    void MoveToNextLine(BallCell[] line, bool withAnimation = true) {
        for (int i = 0; i < line.Length; ++i) {
            var info = line[i];
            if (info != null) {
                info.y = info.y + 1;
                line[i] = info;
                var go = info.go;
                if (go != null) {
                    var new_y = info.y * lineHeight + ballRadius;
                    var new_x = ballRadius * info.x + ballRadius;
                    if (go != null) {
                        var new_pos = go.transform.localPosition;
                        new_pos.x = new_x;
                        new_pos.y = new_y;
                        if (withAnimation) {
                            go.transform.DOLocalMove(new_pos, TweenTime);
                        } else {
                            go.transform.localPosition = new_pos;
                        }
                    }
                }
            }
        }
    }

    bool CheckDeadline() {
        int count = 0;
        //Debug.Log("Mbuffer: " + buffer.Count);
        //Debug.Log("Actual Line: " + actualLine);
        //Debug.Log("Dead Line: " + deadLine);
        if ((buffer.Count+actualLine) > deadLine) {

            playOneShot(4);
            playOneShot(5);

            while (buffer.Count > deadLine) {
                //Debug.Log("Removed");
                var curLine = buffer[0];
                for (int i = 0; i < curLine.cells.Length; ++i) {
                    var b = curLine.cells[i];
                    if (b != null && b.go != null) {
                        FadeOutDestroy(b, remainsGO);
                        activeBalls--;
                        removedBalls++;
                    }
                }
                buffer.RemoveAt(0);
                ++count;
            }
            
        }
        return count != 0;
    }

    ballCoord ToNearestCell(Vector3 pos) {
        var cellWidth = ballRadius * 2;
        var cellHeight = ballRadius * Sqrt3;
        var y = pos.y;
        var cy = (Mathf.FloorToInt(y / cellHeight));
        var padding = (cy + totalLines + firstLineOddLeading) & 1;

        var x = pos.x - padding * ballRadius;
        //Debug.Log("Start Point X: " + x + " Y: " + pos.y);
        var cx = Mathf.FloorToInt(x / cellWidth) * 2 + padding;
        //Debug.Log("Final Point X: " + cx + " Y: " + cy);
        return new ballCoord { x = cx, y = cy };
    }

    void NewLine(bool withAnimation = true) {
        // Add all ball for specific line
        if (ballDataOffset >= 0) {
            var curOffsetY = ballDataOffset--;
            var srcLine = ballLevelData[curOffsetY];
            if (ballDataOffset < 0) {
                ballDataOffset = ballLevelData.Length - 1;
            }
            for (int i = 0; i < buffer.Count; ++i) {
                MoveToNextLine(buffer[i].cells, withAnimation);
            }
            var lineRoot = new GameObject(String.Format("line-{0}", totalLines)).transform;
            lineRoot.parent = cellRoot;
            //lineRoot.position = new Vector3(0.6f, 0, 0);
            var local_pos = lineRoot.localPosition;
            //local_pos.y = local_pos.y + 0.1f;
            //lineRoot.localPosition = local_pos;
            //Debug.Log("Local pos x: " + local_pos.x);
            //Debug.Log("Local pos y: " + local_pos.y);
            lineRoot.localScale = Vector3.one;
            var newLine = new CellLine();
            var newCells = new BallCell[srcLine.Length];
            for (int i = 0; i < srcLine.Length; ++i) {
                var dx = ballRadius * i + ballRadius;
                var tag = ballLevelData[curOffsetY][i];
                GameObject go = null;
                Material mat = null;
                if (tag != 0) {
                    go = Instantiate(ballGO);
                    go.name = String.Format("cell-{0}", i);
                    go.transform.parent = lineRoot;
                    go.tag = "ball";
                    var dstPos = new Vector3(dx, ballRadius, 0);
                    if (withAnimation) {
                        var srcPos = new Vector3(dx, -ballRadius, 0);
                        
                        go.transform.localPosition = srcPos;
                        var t = go.transform.DOLocalMove(dstPos, TweenTime);
                        levelUpdating = true;
                        t.onComplete += () => {
                            levelUpdating = false;
                        };
                    } else {
                        go.transform.localPosition = dstPos;
                    }
                    mat = go.GetComponent<Renderer>().material;
                    FadeInBall(mat, tag);
                    activeBalls++;
                }
                var bi = new BallCell {
                    x = i,
                    y = 0,
                    go = go,
                    mat = mat,
                    color = tag,
                };               
                
                newCells[i] = bi;
            }
            newLine.cells = newCells;
            newLine.root = lineRoot;
            buffer.Add(newLine);
            ++totalLines;
        }
    }

    void NewBlankLine(bool withAnimation = true) {
        // Add all ball for specific line invisible
        if (ballDataOffset >= 0) {
            var curOffsetY = ballDataOffset--;
            int[] srcLine = { 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0 };
            int[] srcLine2 = { 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 0 };

            if (ballDataOffset < 0) {
                ballDataOffset = ballLevelData.Length - 1;
            }
            for (int i = 0; i < buffer.Count; ++i) {
                MoveToNextLine(buffer[i].cells, withAnimation);
            }
            var lineRoot = new GameObject(String.Format("line-{0}", totalLines)).transform;
            lineRoot.parent = cellRoot;
            lineRoot.localScale = Vector3.one;
            var newLine = new CellLine();
            var newCells = new BallCell[srcLine.Length];
            //Debug.Log("Length: " + srcLine.Length);

            //Debug.Log("Parity: " + levelParity);
            for (int i = 0; i < srcLine.Length; ++i) {                
                var dx = ballRadius * i + ballRadius;
                var tag = srcLine[i];
                
                if (levelParity == 1 && actualLine % 2 == 0) {
                    tag = srcLine2[i];
                }
                
                GameObject go = null;
                Material mat = null;
                if (tag != 0) {
                    go = Instantiate(ballGO);
                    go.name = String.Format("cell-{0}", i);
                    go.transform.parent = lineRoot;
                    go.SetActive(true);
                    go.GetComponent<Renderer>().enabled = false;
                    var dstPos = new Vector3(dx, ballRadius, 0);
                    if (withAnimation) {
                        var srcPos = new Vector3(dx, -ballRadius, 0);
                        go.transform.localPosition = srcPos;
                        var t = go.transform.DOLocalMove(dstPos, TweenTime);
                        levelUpdating = true;
                        t.onComplete += () => {
                            levelUpdating = false;

                        };                        
                    } else {
                        go.transform.localPosition = dstPos;
                    }
                    mat = go.GetComponent<Renderer>().material;
                    FadeInBall(mat, tag);
                }
                var bi = new BallCell {
                    x = i,
                    y = 0,
                    go = go,
                    mat = mat,
                    color = tag,
                };
                newCells[i] = bi;
                
            }
            newLine.cells = newCells;
            newLine.root = lineRoot;
            buffer.Add(newLine);
            ++totalLines;            
        }

        if (withAnimation) {
            playOneShot(7);
            var dstPos2 = new Vector3(gate.localPosition.x, gate.localPosition.y + gateOffset, -1f);
            var t2 = gate.transform.DOLocalMove(dstPos2, TweenTime);
        }
    }

    static float CollisionTest_Ball_Wall(Vector3 ori, Vector3 dir, float step, float dt, float radius, NPlane plane) {
        var offset = dir * step;
        var s = ori - plane.point;
        var dis = Vector3.Dot(s, plane.normal);

        if (dis <= radius) {
            //Debug.Log("Dis: " + dis);
            // already collided: ignore...
            return -1;
        }
        var dst = ori + offset;
        var s2 = dst - plane.point;
        var dis2 = Vector3.Dot(s2, plane.normal);
        if (dis2 < radius) {
            var speed = Vector3.Dot(offset / dt, -plane.normal);
            var t = (dis - radius) / speed;
            if (t >= 0) {
                return t;
            }
        }
        return -1;
    }

    static float CollisionTest_Ball_Ball(Vector3 c0, Vector3 dir, float speed, float r0, Vector3 c1, float r1) {
        var r = r0 + r1;
        var r2 = r * r;
        var l = c1 - c0;
        // center distance square
        var disSq = l.sqrMagnitude;
        if (disSq <= r2) {
            // already collided: ignore...
            return -1;
        }
        var s = Vector3.Dot(l, dir);
        if (s < 0) {
            return -1;
        }
        var p = c0 + s * dir;
        var n = c1 - p;
        var nLenSq = n.sqrMagnitude;
        if (nLenSq > r2) {
            return -1;
        }

        var h = Mathf.Sqrt(r2 - nLenSq);
        var hit = p - dir * h;
        return (hit - c0).magnitude / speed;
    }

    static Vector3 Reflect(Vector3 i, Vector3 n) {
        return n * Vector3.Dot(-i, n) * 2 + i;
    }

    // collision test every ball move
    void HitTest(FlyingBall ball) {
        var time = Time.deltaTime;
        var curPos = ball.transform.localPosition;

        var hitObjType = 0;
        for (; time > 0 && ball.speed > 0;) {
            var hitTime = time;
            var movedir = ball.movedir;
            for (int i = 0; i < planes.Length; ++i) {
                var step = ball.speed * time;
                var t = CollisionTest_Ball_Wall(curPos, ball.movedir, step, time, ball.radius, planes[i]);

                if (t >= 0 && t < hitTime) {
                    hitTime = t;
                    if (i == 2) {
                        hitObjType = 4;
                    } else {
                        movedir = Reflect(ball.movedir, planes[i].normal);
                        hitObjType = 1;
                    }


                }
            }
            //Debug.Log("Buff count: " + buffer.Count);
            for (int j = 0; j < buffer.Count; ++j) {
                var line = buffer[j];
                for (int i = 0; i < line.cells.Length; ++i) {
                    //Debug.Log("Length: " + line.cells.Length);
                    var bi = line.cells[i];
                    if (bi != null && bi.go != null && bi.color != 0) {
                        var c1 = bi.go.transform.localPosition;
                        var c0 = curPos;
                        var t = CollisionTest_Ball_Ball(c0, ball.movedir, ball.speed, ball.radius, c1, ball.radius * 0.95f);

                        if (t >= 0 && t < hitTime) {
                            hitTime = t;
                            movedir = Reflect(ball.movedir, (c0 - c1).normalized);
                            hitObjType = 2;
                        }
                    }
                }
            }

            var offset = ball.movedir * ball.speed * hitTime;
            curPos += offset;
            time -= hitTime;
            ball.movedir = movedir;
            if (time > 0) {
                if (hitObjType == 2 || hitObjType == 4) {
                    ball.speed = 0;
                    ball.stopped = true;
                    ball.destResult = ToNearestCell(curPos);
                }
            }
        }
        ball.transform.localPosition = curPos;
    }

    BallCell[] GetLine(int y) {
        if (y < 0 || y >= buffer.Count) {
            return null;
        }
        var line = buffer[buffer.Count - 1 - y];
        if (line == null || line.cells == null) {
            return null;
        }
        return line.cells;
    }

    BallCell GetBall(int x, int y) {
        if (y < 0 || y >= buffer.Count || x < 0) {
            return null;
        }

        var line = buffer[buffer.Count - 1 - y];
        if (line != null && x < line.cells.Length && line.cells[x] != null)
        {
            if (line.cells[x].go != null)
            {
                //Debug.Log("Line Ball Name: " + line.cells[x].go.name);
            } else
            {
                //Debug.Log("Sem GO");
            }

        }

        if (line == null || line.cells == null || x >= line.cells.Length || line.cells[x] == null) {
            return null;
        }
        return line.cells[x];
    }

    // Used for Debug
    void PrintBuffer() {
        for (int j = 0; j < buffer.Count; ++j)
        {
            var _cells = GetLine(j);
            for (int i = 0; i < _cells.Length; ++i)
            {
                var b = _cells[i];
                if (b != null && b.go != null && b.color != 0)
                {
                    Debug.Log("X: " + b.x + " Y: " + b.y + " Name: " + b.go.gameObject.name);
                }
            }
        }
    }

    int GetBallColor(int x, int y) {
        if (y < 0 || y >= buffer.Count || x < 0) {
            return -1;
        }
        var line = buffer[buffer.Count - 1 - y];
        if (line == null || line.cells == null || x >= line.cells.Length || line.cells[x] == null) {
            return -1;
        }
        return line.cells[x].color;
    }

    void CheckEliminate(ballCoord pt, int color, List<BallCell> eraseList, int threshold) {
        openTable.Clear();
        closedTable.Clear();
        var count = 1;
        try {
            openTable.Add(new ballCoord(pt.x, pt.y));
            ////Debug.Log("Open Table Count: " + openTable.Count);
            while (openTable.Count > 0) {
                int last = openTable.Count - 1;
                ballCoord cur = openTable[last];
                openTable.RemoveAt(last);
                closedTable.Add(cur);
                var a = new ballCoord(cur.x + 2, cur.y);
                ////Debug.Log("Color: " + color);
                ////Debug.Log("Color a: " + GetBallColor(a.x, a.y));
                if (GetBallColor(a.x, a.y) == color) {
                    if (!closedTable.Contains(a)) {
                        ////Debug.Log("Add a");
                        openTable.Add(a);
                        ++count;
                    }
                }
                var b = new ballCoord(cur.x - 2, cur.y);
                ////Debug.Log("Color b: " + GetBallColor(b.x, b.y));
                if (GetBallColor(b.x, b.y) == color) {
                    if (!closedTable.Contains(b)) {
                        ////Debug.Log("Add b");
                        openTable.Add(b);
                        ++count;
                    }
                }
                var c = new ballCoord(cur.x - 1, cur.y - 1);
                ////Debug.Log("Color c: " + GetBallColor(c.x, c.y));
                if (GetBallColor(c.x, c.y) == color) {
                    if (!closedTable.Contains(c)) {
                        ////Debug.Log("Add c");
                        openTable.Add(c);
                        ++count;
                    }
                }
                var d = new ballCoord(cur.x + 1, cur.y + 1);
                ////Debug.Log("Color d: " + GetBallColor(d.x, d.y));
                if (GetBallColor(d.x, d.y) == color) {
                    if (!closedTable.Contains(d)) {
                        ////Debug.Log("Add d");
                        openTable.Add(d);
                        ++count;
                    }
                }
                var e = new ballCoord(cur.x - 1, cur.y + 1);
                ////Debug.Log("Color e: " + GetBallColor(e.x, e.y));
                if (GetBallColor(e.x, e.y) == color) {
                    if (!closedTable.Contains(e)) {
                        ////Debug.Log("Add e");
                        openTable.Add(e);
                        ++count;
                    }
                }
                var f = new ballCoord(cur.x + 1, cur.y - 1);
                ////Debug.Log("Color f: " + GetBallColor(f.x, f.y));
                if (GetBallColor(f.x, f.y) == color) {
                    if (!closedTable.Contains(f)) {
                        ////Debug.Log("Add f");
                        openTable.Add(f);
                        ++count;
                    }
                }
            }
        } finally {
            if (eraseList != null) {

                if (closedTable.Count >= threshold) {
                    foreach (ballCoord p in closedTable) {
                        var b = GetBall(p.x, p.y);
                        if (b != null && b.go != null) {
                            this.eraseList.Add(b);
                        }
                    }
                }
            }
            openTable.Clear();
            closedTable.Clear();
        }
    }

    void DoEliminateBalls(List<BallCell> eraseList) {
        if (eraseList.Count > 0) {
            playOneShot(4);
            playOneShot(5);
        }
        for (int i = 0; i < eraseList.Count; ++i) {
            var b = eraseList[i];
            if (b.go != null) {
                FadeOutDestroy(b, remainsGO);
                activeBalls--;
            }
        }

    }

    private static string GetSerializedString(List<int> data) {
        if (data.Count == 0) return string.Empty;

        string result = data[0].ToString();
        for (int i = 1; i < data.Count && i < 12; i++) {
            result += ("|" + data[i]);
        }
        return result;
    }

    public static List<int> GetHighScores(string playerPrefsData) {
        string[] data = playerPrefsData.Split('|');
        List<int> val = new List<int>();
        for (int i = 0; i < data.Length; i++) {
            val.Add(int.Parse(data[i]));
        }
        return val;
    }

    void DoDropDestroyBalls(List<BallCell> eraseList) {
        if (eraseList.Count > 0) {
            playOneShot(6);
        }
        for (int i = 0; i < eraseList.Count; ++i) {
            var b = eraseList[i];
            if (b.go != null) {
                DropDestroy(b);
                activeBalls--;
            }
        }
        if (activeBalls == 0)
        {
            TimeSpan ts = zeit.Elapsed;
            zeit.Stop();

            Color color;
            if (!cylinder.GetComponent<MeshRenderer>()) {
                color = cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
            } else {
                color = cylinder.GetComponent<MeshRenderer>().material.color;
            }
            color.a = 0;
            transpMaterial.color = color;
            if (!cylinder.GetComponent<MeshRenderer>()) {
                cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material = transpMaterial;
                cylinder.transform.GetChild(2).GetComponent<MeshRenderer>().material = transpMaterial;
            } else {
                cylinder.GetComponent<MeshRenderer>().material = transpMaterial;
            }
            if (ring.GetComponent<MeshRenderer>() == null) {
                ring.transform.GetChild(0).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                ring.transform.GetChild(1).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                ring.transform.GetChild(2).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
            } else {
                ring.GetComponent<MeshRenderer>().material = transpMaterial;
            }
            //txtMenuEndGame.GetComponent<RectTransform>().localPosition = new Vector3(55, -527, 0);
            //txtMenuEndGame.GetComponent<Text>().text = "You Win!";

            List<int> highscores = new List<int>();

            txtTitleScore.GetComponent<Text>().text = "Highscore for Level " + (level + 1);

            if (!PlayerPrefs.HasKey("highscoreLevel" + (level + 1))) {
                highscores.Add(shootCount);
                highscores.Add(ts.Minutes);
                highscores.Add(ts.Seconds);
                PlayerPrefs.SetString("highscoreLevel" + (level + 1), GetSerializedString(highscores));

                if (highscores[2] > 60) {
                    txtUserScore1.GetComponent<Text>().text = 1 + " - " + highscores[0] + " Shots " + highscores[1] + " Min " + highscores[2] + " Sec";
                } else {
                    txtUserScore1.GetComponent<Text>().text = 1 + " - " + highscores[0] + " Shots " + highscores[2] + " Sec";
                }

                txtUserScore1.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
            } else {
                //Debug.Log("Serialized: " + PlayerPrefs.GetString("highscoreLevel"+(level+1)));
                highscores = GetHighScores(PlayerPrefs.GetString("highscoreLevel" + (level + 1)));
                int i = 0;

                for (i = 0; i < (highscores.Count / 3); i++) {
                    if (highscores[i * 3] > shootCount) {
                        highscores.Insert(i * 3, shootCount);
                        highscores.Insert((i * 3) + 1, ts.Minutes);
                        highscores.Insert((i * 3) + 2, ts.Seconds);
                        break;
                    } else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] > ts.Minutes) {
                        highscores.Insert(i * 3, shootCount);
                        highscores.Insert((i * 3) + 1, ts.Minutes);
                        highscores.Insert((i * 3) + 2, ts.Seconds);
                        break;
                    } else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] == ts.Minutes && highscores[(i * 3) + 2] > ts.Seconds) {
                        highscores.Insert(i * 3, shootCount);
                        highscores.Insert((i * 3) + 1, ts.Minutes);
                        highscores.Insert((i * 3) + 2, ts.Seconds);
                        break;
                    } else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] == ts.Minutes && highscores[(i * 3) + 2] == ts.Seconds) {
                        highscores[i * 3] = shootCount;
                        highscores[(i * 3) + 1] = ts.Minutes;
                        highscores[(i * 3) + 2] = ts.Seconds;
                        break;
                    }
                }
                if (i == (highscores.Count / 3))
                {
                    //Debug.Log("Here");
                    highscores.Add(shootCount);
                    highscores.Add(ts.Minutes);
                    highscores.Add(ts.Seconds);
                }
                //Debug.Log("Highscore Serialized: " + GetSerializedString(highscores));
                PlayerPrefs.SetString("highscoreLevel" + (level + 1), GetSerializedString(highscores));

                for (int j = 0; j < highscores.Count / 3 && j < 12; j++) {
                    if (j == 0) {
                        if (highscores[(j * 3) + 2] > 60) {
                            txtUserScore1.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                        } else {
                            txtUserScore1.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                        }
                    } else if (j == 1) {
                        txtUserScore2.SetActive(true);
                        if (highscores[(j * 3) + 2] > 60) {
                            txtUserScore2.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                        } else {
                            txtUserScore2.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                        }
                    } else if (j == 2) {
                        txtUserScore3.SetActive(true);
                        if (highscores[(j * 3) + 2] > 60) {
                            txtUserScore3.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                        } else { 
                            txtUserScore3.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                        }
                    } else if (j == 3) {
                        txtUserScore4.SetActive(true);
                        if (highscores[(j * 3) + 2] > 60) {
                            txtUserScore4.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                        } else {
                            txtUserScore4.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                        }
                    } else if (j == 4) {
                        txtUserScore5.SetActive(true);
                        if (highscores[(j * 3) + 2] > 60) {
                            txtUserScore5.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                        } else {
                            txtUserScore5.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                        }
                    }
                }

                switch (i)
                {
                    case 0:
                        txtUserScore1.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                        break;
                    case 1:
                        txtUserScore2.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                        break;
                    case 2:
                        txtUserScore3.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                        break;
                    case 3:
                        txtUserScore4.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                        break;
                    case 4:
                        txtUserScore5.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                        break;
                }
            }
            canvasMenuEndGame.SetActive(true);

            if (level + 1 != 100) {
                if (PlayerPrefs.HasKey("savedLevel")) {
                    if (level == PlayerPrefs.GetInt("savedLevel")) {
                        PlayerPrefs.SetInt("savedLevel", (level + 1));
                    }
                } else {
                    PlayerPrefs.SetInt("savedLevel", (level + 1));
                }

                PlayerPrefs.SetInt("level", level + 1);

                PlayerPrefs.Save();
            }            
        }
    }

    void Sweep(List<BallCell> eraseList) {
        eraseList.Clear();
        openTable.Clear();
        closedTable.Clear();
        
        var cells = GetLine(0);

        if (cells != null) {
            Debug.Log("Actual Line: " + actualLine);
            cells = GetLine(actualLine);
            //Debug.Log("Color: " + cells[0].color);
            for (int i = 0; i < cells.Length; ++i) {
                //Debug.Log("Actual Line: " + actualLine);
                var b = GetBall(i, actualLine);
                if (b != null && b.go != null) {
                    var pt = new ballCoord(b.x, b.y);
                    closedTable.Add(pt);
                    openTable.Add(pt);
                    //if (step_count > 0)
                    //Debug.Log("BX: " + b.x + " BY: " + b.y);
                }
            }
        }
        //Debug.Log("Line 0 Count: " + closedTable.Count);
        var count = 1;
        try {
            //Debug.Log("Open Table: " + openTable.Count);
            while (openTable.Count > 0) {
                int last = openTable.Count - 1;
                ballCoord cur = openTable[last];
                openTable.RemoveAt(last);
                closedTable.Add(cur);

                var a = new ballCoord(cur.x + 2, cur.y);
                var aa = GetBall(a.x, a.y);
                //Debug.Log("CURX: " + cur.x + " CURY: " + cur.y + " Color: " + GetBall(cur.x, cur.y).color);

                if (aa != null && aa.go != null) {
                    //Debug.Log("AA: " + aa);
                    if (!closedTable.Contains(a)) {
                        //Debug.Log("Add aa");
                        openTable.Add(a);
                        ++count;
                    }
                }
                var b = new ballCoord(cur.x - 2, cur.y);
                var bb = GetBall(b.x, b.y);
                /*if (step_count > 0)
                {
                    Debug.Log("BX: " + b.x + " BY: " + b.y);
                    Debug.Log("Color: " + bb.color);
                }*/

                if (bb != null && bb.go != null) {
                    if (!closedTable.Contains(b)) {
                        openTable.Add(b);
                        ++count;
                    }
                }
                var c = new ballCoord(cur.x - 1, cur.y - 1);
                var cc = GetBall(c.x, c.y);
                /*if (step_count > 0 && cc != null && cc.go != null)
                {
                    Debug.Log("CX: " + c.x + " CY: " + c.y);
                    Debug.Log("Color: " + cc.color);
                }*/
                if (cc != null && cc.go != null) {
                    if (!closedTable.Contains(c)) {
                        openTable.Add(c);
                        ++count;
                    }
                }
                var d = new ballCoord(cur.x + 1, cur.y + 1);
                var dd = GetBall(d.x, d.y);

                //Debug.Log("DX: " + d.x + " DY: " + d.y);
                if (dd != null && dd.go != null) {
                    if (!closedTable.Contains(d)) {
                        //Debug.Log("Add dd");
                        openTable.Add(d);
                        ++count;
                    }
                }
                var e = new ballCoord(cur.x - 1, cur.y + 1);
                var ee = GetBall(e.x, e.y);
                if (ee != null && ee.go != null) {
                    if (!closedTable.Contains(e)) {
                        //Debug.Log("Add ee");
                        openTable.Add(e);
                        ++count;
                    }
                }
                var f = new ballCoord(cur.x + 1, cur.y - 1);
                var ff = GetBall(f.x, f.y);
                if (ff != null && ff.go != null) {
                    if (!closedTable.Contains(f)) {
                        //Debug.Log("Add ff");
                        openTable.Add(f);
                        ++count;
                    }
                }
            }
        } finally {
            if (eraseList != null) {
                //Debug.Log("Closed Table Count: " + closedTable.Count);
                for (int j = 0; j < buffer.Count; ++j) {
                    var _cells = GetLine(j);
                    for (int i = 0; i < _cells.Length; ++i) {
                        var b = _cells[i];
                        if (b != null && b.go != null && b.color != 0 && b.color != 9) {
                            if (!closedTable.Contains(new ballCoord(b.x, b.y))) {
                                //Debug.Log("Erase BX: " + b.x + " BY: " + (b.y - step_count));
                                this.eraseList.Add(b);
                            }
                        }
                    }
                }
                //Debug.Log("Erase List Count: " + eraseList.Count);
            }
            openTable.Clear();
            closedTable.Clear();
        }
    }

    void BallMove(ref FlyingBall ball) {
        if (!ball.stopped) {
            
            HitTest(ball);
            if (ball.stopped) {
                playOneShot(3);
                //Debug.Log("Aqui 4");
                //Debug.Log("Count 1: " + buffer.Count);

                var slot = StickToBuffer(ref ball);
                
                if (slot != null) {
                    if (!CheckDeadline()) {
                        try {
                            CheckEliminate(new ballCoord(slot.x, slot.y), slot.color, eraseList, 3);
                            //Debug.Log("Erase List Count: " + eraseList.Count);

                            DoEliminateBalls(eraseList);

                            Sweep(eraseList);
                                
                            //Debug.Log("Erase List Count: " + eraseList.Count);
                            DoDropDestroyBalls(eraseList);
                        } finally {
                            eraseList.Clear();
                            UpdateColors();
                            
                            if (shootCount % 8 == 0 && shootCount > 0) {
                                NewBlankLine();
                                actualLine++;
                                CheckDeadline();
                                if (removedBalls > 0) {
                                    removedBalls = 0;
                                    Debug.Log("Game Over!");

                                    TimeSpan ts = zeit.Elapsed;
                                    zeit.Stop();
                                    Color color;
                                    if (!cylinder.GetComponent<MeshRenderer>()) {
                                        color = cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
                                    } else {
                                        color = cylinder.GetComponent<MeshRenderer>().material.color;
                                    }
                                    color.a = 0;
                                    transpMaterial.color = color;
                                    if (!cylinder.GetComponent<MeshRenderer>()) {
                                        cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material = transpMaterial;
                                        cylinder.transform.GetChild(2).GetComponent<MeshRenderer>().material = transpMaterial;
                                    } else {
                                        cylinder.GetComponent<MeshRenderer>().material = transpMaterial;
                                    }

                                    if (ring.GetComponent<MeshRenderer>()) {
                                        ring.GetComponent<MeshRenderer>().material = transpMaterial;
                                    }

                                    List<int> highscores = new List<int>();

                                    if (!PlayerPrefs.HasKey("highscoreLevel" + (level + 1)))
                                    {
                                        highscores.Add(shootCount);
                                        highscores.Add(ts.Minutes);
                                        highscores.Add(ts.Seconds);
                                        PlayerPrefs.SetString("highscoreLevel" + (level + 1), GetSerializedString(highscores));

                                        if (highscores[2] > 60)
                                        {
                                            txtUserScore1.GetComponent<Text>().text = 1 + " - " + highscores[0] + " Shots " + highscores[1] + " Min " + highscores[2] + " Sec";
                                        }
                                        else
                                        {
                                            txtUserScore1.GetComponent<Text>().text = 1 + " - " + highscores[0] + " Shots " + highscores[2] + " Sec";
                                        }

                                        txtUserScore1.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                    }
                                    else
                                    {
                                        Debug.Log("Serialized: " + PlayerPrefs.GetString("highscoreLevel" + (level + 1)));
                                        highscores = GetHighScores(PlayerPrefs.GetString("highscoreLevel" + (level + 1)));
                                        int i = 0;

                                        for (i = 0; i < (highscores.Count / 3); i++)
                                        {
                                            if (highscores[i * 3] > shootCount)
                                            {
                                                highscores.Insert(i * 3, shootCount);
                                                highscores.Insert((i * 3) + 1, ts.Minutes);
                                                highscores.Insert((i * 3) + 2, ts.Seconds);
                                                break;
                                            }
                                            else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] > ts.Minutes)
                                            {
                                                highscores.Insert(i * 3, shootCount);
                                                highscores.Insert((i * 3) + 1, ts.Minutes);
                                                highscores.Insert((i * 3) + 2, ts.Seconds);
                                                break;
                                            }
                                            else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] == ts.Minutes && highscores[(i * 3) + 2] > ts.Seconds)
                                            {
                                                highscores.Insert(i * 3, shootCount);
                                                highscores.Insert((i * 3) + 1, ts.Minutes);
                                                highscores.Insert((i * 3) + 2, ts.Seconds);
                                                break;
                                            }
                                            else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] == ts.Minutes && highscores[(i * 3) + 2] == ts.Seconds)
                                            {
                                                highscores[i * 3] = shootCount;
                                                highscores[(i * 3) + 1] = ts.Minutes;
                                                highscores[(i * 3) + 2] = ts.Seconds;
                                                break;
                                            }

                                        }
                                        if (i == (highscores.Count / 3))
                                        {
                                            //Debug.Log("Here");
                                            highscores.Add(shootCount);
                                            highscores.Add(ts.Minutes);
                                            highscores.Add(ts.Seconds);
                                        }
                                        //Debug.Log("Highscore Serialized: " + GetSerializedString(highscores));
                                        PlayerPrefs.SetString("highscoreLevel" + (level + 1), GetSerializedString(highscores));

                                        for (int j = 0; j < highscores.Count / 3 && j < 12; j++)
                                        {
                                            if (j == 0)
                                            {
                                                if (highscores[(j * 3) + 2] > 60)
                                                {
                                                    txtUserScore1.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                                else
                                                {
                                                    txtUserScore1.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                            }
                                            else if (j == 1)
                                            {
                                                txtUserScore2.SetActive(true);
                                                if (highscores[(j * 3) + 2] > 60)
                                                {
                                                    txtUserScore2.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                                else
                                                {
                                                    txtUserScore2.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                            }
                                            else if (j == 2)
                                            {
                                                txtUserScore3.SetActive(true);
                                                if (highscores[(j * 3) + 2] > 60)
                                                {
                                                    txtUserScore3.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                                else
                                                {
                                                    txtUserScore3.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                            }
                                            else if (j == 3)
                                            {
                                                txtUserScore4.SetActive(true);
                                                if (highscores[(j * 3) + 2] > 60)
                                                {
                                                    txtUserScore4.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                                else
                                                {
                                                    txtUserScore4.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                            }
                                            else if (j == 4)
                                            {
                                                txtUserScore5.SetActive(true);
                                                if (highscores[(j * 3) + 2] > 60)
                                                {
                                                    txtUserScore5.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                                else
                                                {
                                                    txtUserScore5.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                                }
                                            }
                                        }

                                        switch (i)
                                        {
                                            case 0:
                                                txtUserScore1.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                                break;
                                            case 1:
                                                txtUserScore2.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                                break;
                                            case 2:
                                                txtUserScore3.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                                break;
                                            case 3:
                                                txtUserScore4.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                                break;
                                            case 4:
                                                txtUserScore5.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                                break;
                                        }
                                    }
                                    canvasMenuGameOver.SetActive(true);

                                }
                            }
                        }
                    } else {
                        Debug.Log("Game Over 2!");
                        TimeSpan ts = zeit.Elapsed;
                        zeit.Stop();
                        Color color;
                        if (!cylinder.GetComponent<MeshRenderer>()) {
                            color = cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
                        } else {
                            color = cylinder.GetComponent<MeshRenderer>().material.color;
                        }
                        color.a = 0;
                        transpMaterial.color = color;
                        if (!cylinder.GetComponent<MeshRenderer>()) {
                            cylinder.transform.GetChild(0).GetComponent<MeshRenderer>().material = transpMaterial;
                            cylinder.transform.GetChild(2).GetComponent<MeshRenderer>().material = transpMaterial;
                        } else {
                            cylinder.GetComponent<MeshRenderer>().material = transpMaterial;
                        }
                        if (ring.GetComponent<MeshRenderer>() == null) {
                            ring.transform.GetChild(0).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                            ring.transform.GetChild(1).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                            ring.transform.GetChild(2).GetComponentInChildren<MeshRenderer>().material = transpMaterial;
                        } else {
                            ring.GetComponent<MeshRenderer>().material = transpMaterial;
                        }

                        List<int> highscores = new List<int>();

                        if (!PlayerPrefs.HasKey("highscoreLevel" + (level + 1)))
                        {
                            highscores.Add(shootCount);
                            highscores.Add(ts.Minutes);
                            highscores.Add(ts.Seconds);
                            PlayerPrefs.SetString("highscoreLevel" + (level + 1), GetSerializedString(highscores));

                            if (highscores[2] > 60)
                            {
                                txtUserScore1.GetComponent<Text>().text = 1 + " - " + highscores[0] + " Shots " + highscores[1] + " Min " + highscores[2] + " Sec";
                            }
                            else
                            {
                                txtUserScore1.GetComponent<Text>().text = 1 + " - " + highscores[0] + " Shots " + highscores[2] + " Sec";
                            }

                            txtUserScore1.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                        }
                        else
                        {
                            Debug.Log("Serialized: " + PlayerPrefs.GetString("highscoreLevel" + (level + 1)));
                            highscores = GetHighScores(PlayerPrefs.GetString("highscoreLevel" + (level + 1)));
                            int i = 0;

                            for (i = 0; i < (highscores.Count / 3); i++)
                            {
                                if (highscores[i * 3] > shootCount)
                                {
                                    highscores.Insert(i * 3, shootCount);
                                    highscores.Insert((i * 3) + 1, ts.Minutes);
                                    highscores.Insert((i * 3) + 2, ts.Seconds);
                                    break;
                                }
                                else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] > ts.Minutes)
                                {
                                    highscores.Insert(i * 3, shootCount);
                                    highscores.Insert((i * 3) + 1, ts.Minutes);
                                    highscores.Insert((i * 3) + 2, ts.Seconds);
                                    break;
                                }
                                else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] == ts.Minutes && highscores[(i * 3) + 2] > ts.Seconds)
                                {
                                    highscores.Insert(i * 3, shootCount);
                                    highscores.Insert((i * 3) + 1, ts.Minutes);
                                    highscores.Insert((i * 3) + 2, ts.Seconds);
                                    break;
                                }
                                else if (highscores[i * 3] == shootCount && highscores[(i * 3) + 1] == ts.Minutes && highscores[(i * 3) + 2] == ts.Seconds)
                                {
                                    highscores[i * 3] = shootCount;
                                    highscores[(i * 3) + 1] = ts.Minutes;
                                    highscores[(i * 3) + 2] = ts.Seconds;
                                    break;
                                }

                            }
                            if (i == (highscores.Count / 3))
                            {
                                //Debug.Log("Here");
                                highscores.Add(shootCount);
                                highscores.Add(ts.Minutes);
                                highscores.Add(ts.Seconds);
                            }
                            //Debug.Log("Highscore Serialized: " + GetSerializedString(highscores));
                            PlayerPrefs.SetString("highscoreLevel" + (level + 1), GetSerializedString(highscores));

                            for (int j = 0; j < highscores.Count / 3 && j < 12; j++)
                            {
                                if (j == 0)
                                {
                                    if (highscores[(j * 3) + 2] > 60)
                                    {
                                        txtUserScore1.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                    else
                                    {
                                        txtUserScore1.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                }
                                else if (j == 1)
                                {
                                    txtUserScore2.SetActive(true);
                                    if (highscores[(j * 3) + 2] > 60)
                                    {
                                        txtUserScore2.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                    else
                                    {
                                        txtUserScore2.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                }
                                else if (j == 2)
                                {
                                    txtUserScore3.SetActive(true);
                                    if (highscores[(j * 3) + 2] > 60)
                                    {
                                        txtUserScore3.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                    else
                                    {
                                        txtUserScore3.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                }
                                else if (j == 3)
                                {
                                    txtUserScore4.SetActive(true);
                                    if (highscores[(j * 3) + 2] > 60)
                                    {
                                        txtUserScore4.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                    else
                                    {
                                        txtUserScore4.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                }
                                else if (j == 4)
                                {
                                    txtUserScore5.SetActive(true);
                                    if (highscores[(j * 3) + 2] > 60)
                                    {
                                        txtUserScore5.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                    else
                                    {
                                        txtUserScore5.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                                    }
                                }
                            }

                            switch (i)
                            {
                                case 0:
                                    txtUserScore1.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                    break;
                                case 1:
                                    txtUserScore2.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                    break;
                                case 2:
                                    txtUserScore3.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                    break;
                                case 3:
                                    txtUserScore4.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                    break;
                                case 4:
                                    txtUserScore5.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                                    break;
                            }
                        }
                        canvasMenuGameOver.SetActive(true);
                    }
                }
                ball.DestroyBall();
                ball = null;
                //Debug.Log("Count 2: " + buffer.Count);


            }

        }

    }

    void playOneShot(int option) {
        if (PlayerPrefs.HasKey("playSounds")) {
            if (PlayerPrefs.GetInt("playSounds") == 1) {
                switch(option) {
                    case 0: // Left Cannon Move
                        switch (cannonId) {
                            case 2:
                                audioSource.PlayOneShot(leftCannonMove2, volume / 16);
                                break;
                            default:
                                audioSource.PlayOneShot(leftCannonMove, volume / 8);
                                break;
                        }
                        break;
                    case 1: // Right Cannon Move
                        switch (cannonId) {
                            case 2:
                                audioSource.PlayOneShot(rightCannonMove2, volume / 16);
                                break;
                            default:
                                audioSource.PlayOneShot(rightCannonMove, volume / 16);
                                break;
                        }

                        break;
                    case 2: // Cannon Shoot
                        switch(cannonId) {
                            case 0:
                                audioSource.PlayOneShot(cannon0, volume);
                                break;
                            case 1:
                                audioSource.PlayOneShot(cannon1, volume);
                                break;
                            case 2:
                                audioSource.PlayOneShot(cannon2, volume*0.75f);
                                break;
                            default:
                                audioSource.PlayOneShot(cannon0, volume);
                                break;
                        }
                        break;
                    case 3: // Ball Collision
                        audioSource.PlayOneShot(ballCollision, volume);
                        break;
                    case 4: // Ball Explode
                        audioSource.PlayOneShot(ballExplode, volume);
                        break;
                    case 5: // Ball Remains
                        audioSource.PlayOneShot(ballRemains, volume);
                        break;
                    case 6: // Ball Sweep
                        audioSource.PlayOneShot(ballSweep, volume);
                        break;
                    case 7: // Gate
                        audioSource.PlayOneShot(audioGate, volume);
                        break;
                }
            }
        }
    }

    void playLoop() {
        if (PlayerPrefs.HasKey("playMusic")) {
            if (PlayerPrefs.GetInt("playMusic") == 1) {
                audioSourceBG.loop = true;
                audioSourceBG.PlayOneShot(bgMainMenu, volume);
            }
        }
    }

    void Start() {
        // Disable Multitouch
        Input.multiTouchEnabled = false;

        // Get Level from player prefs
        if (PlayerPrefs.HasKey("level")) {
            if (level == -1) {
                level = PlayerPrefs.GetInt("level");
            }

            Debug.Log("Level " + level);
            if (level >= 69) {
                bgGO.GetComponent<SpriteRenderer>().sprite = bgFuture;
                gateGO.GetComponent<SpriteRenderer>().sprite = gateFuture;
            } else if (level >= 39) {
                bgGO.GetComponent<SpriteRenderer>().sprite = bgBuild;
                gateGO.GetComponent<SpriteRenderer>().sprite = gateBuild;
                //cannon.localPosition = new Vector2(cannon.position.x, cannon.position.y + 0.15f);
                //aimOffsetBg = 0.20f;
            } else if (level >= 19) {
                bgGO.GetComponent<SpriteRenderer>().sprite = bgWall;
                gateGO.GetComponent<SpriteRenderer>().sprite = gateWall;
                cannon.localPosition = new Vector2(cannon.position.x, cannon.position.y + 0.05f);
                //aimer.localPosition = new Vector2(aimer.position.x, aimer.position.y + 0.05f);
                aimOffsetBg = 0.10f;
            } else {
                //backgroundImg.GetComponent<SpriteRenderer>().sprite = bgCave;
                //gate.GetComponent<SpriteRenderer>().sprite = gateCave;
                //backgroundImg.GetComponent<SpriteRenderer>().sprite = bgWood;
                //gate.GetComponent<SpriteRenderer>().sprite = gateWood;
                bgGO.GetComponent<SpriteRenderer>().sprite = bgCave;
                gateGO.GetComponent<SpriteRenderer>().sprite = gateCave;
            }
        }

        if (PlayerPrefs.HasKey("cannonId")) {
            cannonId = PlayerPrefs.GetInt("cannonId");
            //Debug.Log("Level " + level);
        }


        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        int tempPlaySounds = 0;
        if (PlayerPrefs.HasKey("playSounds")) {
            switch (PlayerPrefs.GetInt("playSounds")) {
                case 0:
                    togglePlaySounds.isOn = false;
                    break;
                case 1:
                    togglePlaySounds.isOn = true;
                    tempPlaySounds = 1;
                    break;
            }
        } else {
            //PlayerPrefs.SetInt("playSounds", 1);
            togglePlaySounds.isOn = true;
            tempPlaySounds = 1;
        }

        PlayerPrefs.SetInt("playSounds", 0);

        if (PlayerPrefs.HasKey("helpPath")) {
            switch (PlayerPrefs.GetInt("helpPath")) {
                case 0:
                    toggleHelpPath.isOn = false;
                    break;
                case 1:
                    toggleHelpPath.isOn = true;
                    break;
            }
        } else {
            PlayerPrefs.SetInt("helpPath", 1);
            toggleHelpPath.isOn = true;
        }

        if (PlayerPrefs.HasKey("hurryUp")) {
            switch (PlayerPrefs.GetInt("hurryUp")) {
                case 0:
                    toggleHurryUp.isOn = false;
                    break;
                case 1:
                    toggleHurryUp.isOn = true;
                    break;
            }
        } else {
            PlayerPrefs.SetInt("hurryUp", 1);
            toggleHurryUp.isOn = true;
        }

        if (PlayerPrefs.HasKey("playMusic")) {
            switch (PlayerPrefs.GetInt("playMusic")) {
                case 0:
                    togglePlayMusic.isOn = false;
                    break;
                case 1:
                    togglePlayMusic.isOn = true;
                    audioSourceBG.Play();
                    break;
            }
        } else {
            PlayerPrefs.SetInt("playMusic", 1);
            togglePlayMusic.isOn = true;
            audioSourceBG.Play();
        }

        PlayerPrefs.SetInt("playSounds", tempPlaySounds);

        //playLoop();

        // Ball Buffer List
        buffer = new List<CellLine>();

        // Find Cell to put new lines with balls
        cellRoot = transform.Find("Cell");

        // Find airmer
        aimer = transform.Find("Aimer");

        // Find gate
        gate = transform.Find("Gate");

        currentTransform = transform;

        // Find bounds/ walls
        bounds = transform.Find("Bounds");

        // Set walls to active
        bounds.gameObject.SetActive(true);

        // Set aimer to active
        aimer.gameObject.SetActive(true);

        // Check if txt cell file is loaded
        if (levelData != null) {
            // txt cell file
            var src = levelData.text;

            // split txt cell file in lines
            var lines = src.Split('\n');
            List<string> lines2 = new List<string>();
            //Debug.Log("Level 2 " + level);

            for (int i = (level * 10); i < (level * 10) + 10; i++) {
                lines2.Add(lines[i]);
                //Debug.Log("Line " + i + ": " + lines2[i]);
            }


            // create list to string lines
            var slines = new List<string>();

            // var width to set wall distance to put all balls
            var width = int.MaxValue;
            //Debug.Log("Max Lines: " + lines.Length);

            for (int i = 0; i < maxLine; ++i) {
                //var l = lines[i].Trim();
                var l = lines2[i].Trim();
                //Debug.Log("Length: " + l.Length);
                if (!string.IsNullOrEmpty(l)) {
                    // Add string line to slines
                    slines.Add(l);
                    // set width to max line length
                    if (l.Length < width) {
                        width = l.Length;
                    }
                }
            }
            //Debug.Log("S Lines: " +slines.Count);
            initLineCount = slines.Count;


            // ball data Offset
            ballDataOffset = slines.Count - 1;
            //Debug.Log("Data Offset: " + ballDataOffset);

            ballLevelData = new int[slines.Count][];

            for (int j = 0; j < slines.Count; ++j) {
                // create one ball level data with line width
                ballLevelData[j] = new int[width];
                var s = slines[j];
                for (int i = 0; i < width; ++i) {
                    var c = s[i];
                    if (Char.IsNumber(c)) {
                        ballLevelData[j][i] = s[i] - '0';

                    }
                }
            }
            int temp_count = 0;

            for (int i = 0; i < 16; i++) {
                //Debug.Log("Color: " + ballLevelData[0][i]);
                if (ballLevelData[0][i] == 0) {
                    temp_count++;
                } else {
                    temp_count++;
                    break;
                }
            }

            //temp_count++;
            if (temp_count % 2 != 0) {
                levelParity = 1;
            }

            firstLineOddLeading = initLineCount % 2;

            // get component SphereCollider
            //var sphere = ballGO.GetComponent<SphereCollider>();
            var sphere = ballGO.GetComponent<CircleCollider2D>();

            Vector2 aspect = GetAspectRatio(Screen.width, Screen.height);
            Debug.Log("Aspect before: " + aspect);

            if (aspect.x < 9) {
                aspect.y = 9 / aspect.x * aspect.y;
                aspect.x = 9;
            } else if (aspect.x > 9) {
                aspect.y /= (aspect.x / 9);
                aspect.x = 9;
            }

            Debug.Log("Aspect after: " + aspect);

            var bgScale = aspectYDefault * bgScaleDefault / aspect.y;
            var sphereRadius = bgScale * sphereRadiusDefault / bgScaleDefault;
            sphere.radius = sphereRadius;
            ballGO.transform.localScale = new Vector3(sphereRadius, sphereRadius, sphereRadius);
            gateOffset = (sphereRadius * gateOffsetDefault / sphereRadiusDefault) - 0.000003f;

            var frameWidth = 8 * sphereRadius;
            var scaleFactor = frameWidth * scaleFactorDefault / frameWidthDefault;

            var frameHeight = 9.75f;

            bgGO.transform.localPosition = new Vector2(frameWidth / 2, bgGO.transform.localPosition.y);
            bgGO.transform.localScale = new Vector2(bgGO.transform.localScale.x * scaleFactor, bgGO.transform.localScale.y);

            screenOffset = (int)aspect.y - aspectYDefault;
            if (screenOffset < 0) {
                screenOffset = 0;
            }

            gate.transform.localPosition = new Vector3(frameWidth / 2, gate.transform.localPosition.y + (screenOffset * gateOffset), -1);
            gate.transform.localScale = new Vector2(gate.transform.localScale.x * scaleFactor, gate.transform.localScale.y);

            canvasBtnSettings.transform.localPosition = new Vector3(frameWidth / 2, frameHeight / 2, 90);
            btnSettings.transform.localPosition = new Vector2(btnSettings.transform.localPosition.x * scaleFactor, btnSettings.transform.localPosition.y);
            btnSettings.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            txtHurryUp.transform.localPosition = new Vector2(txtHurryUp.transform.localPosition.x * scaleFactor, txtHurryUp.transform.localPosition.y);
            txtHurryUp.transform.localScale = new Vector3(scaleFactor, -scaleFactor, scaleFactor);

            cylinderAxis.localPosition = new Vector2(frameWidth / 2, -cylinderAxis.position.y);
            cylinderAxis.localScale = new Vector3(cylinderAxis.localScale.x * scaleFactor, cylinderAxis.localScale.y * scaleFactor, cylinderAxis.localScale.z * scaleFactor);

            ring.localPosition = new Vector2(frameWidth / 2, -ring.position.y);
            ring.localScale = new Vector3(ring.localScale.x * scaleFactor, ring.localScale.y * scaleFactor, ring.localScale.z * scaleFactor);

            wheel.position = new Vector3(frameWidth / 2, wheel.position.y, -1);
            wheel.localScale = new Vector3(wheel.localScale.x * scaleFactor, wheel.localScale.y * scaleFactor, wheel.localScale.z * scaleFactor);

            canvasPanelOnGame.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 2600);
            menuOnGame.GetComponent<RectTransform>().offsetMin = new Vector2(menuOnGame.GetComponent<RectTransform>().offsetMin.x, 690);
            menuOnGame.GetComponent<RectTransform>().offsetMax = new Vector2(menuOnGame.GetComponent<RectTransform>().offsetMax.x, -810);

            canvasPanelConfirm.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 2600);
            menuConfirm.GetComponent<RectTransform>().offsetMin = new Vector2(menuConfirm.GetComponent<RectTransform>().offsetMin.x, 1000);
            menuConfirm.GetComponent<RectTransform>().offsetMax = new Vector2(menuConfirm.GetComponent<RectTransform>().offsetMax.x, -820);

            canvasPanelEndGame.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 2600);
            menuEndGame.GetComponent<RectTransform>().offsetMin = new Vector2(menuEndGame.GetComponent<RectTransform>().offsetMin.x, 950);
            menuEndGame.GetComponent<RectTransform>().offsetMax = new Vector2(menuEndGame.GetComponent<RectTransform>().offsetMax.x, -690);

            canvasPanelGameOver.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 2600);
            menuGameOver.GetComponent<RectTransform>().offsetMin = new Vector2(menuGameOver.GetComponent<RectTransform>().offsetMin.x, 950);
            menuGameOver.GetComponent<RectTransform>().offsetMax = new Vector2(menuGameOver.GetComponent<RectTransform>().offsetMax.x, -690);

            // get sphere radius
            ballRadius = sphere.radius / 2;
            //Debug.Log("Ball Radius: " + ballRadius);

            // set line height based on ball radius
            lineHeight = (ballRadius * 2.0f) * Sqrt3Div2;
            //Debug.Log("Line Height: " + lineHeight);

            // check if bounds/ wall exists
            if (bounds != null) {
                // get scale from Bounds Object
                var frameScale = bounds.localScale;

                // set frame scale z with ball radius
                frameScale.z = ballRadius * 2;

                // set bounds scale with frame scale
                bounds.localScale = frameScale;

                // create planes for walls
                planes = new NPlane[4];

                // get bounds one by one
                var l = bounds.Find("Left");
                var r = bounds.Find("Right");
                var t = bounds.Find("Top");
                var b = bounds.Find("Bottom");

                // get left bound position
                var lpos = l.localPosition;
                // get left bound scale
                var lscale = l.localScale;
                //Debug.Log("Left scale y: " + lscale.y);
                // set left bound x position
                lpos.x = (-BoundFrameWidth * 0.5f);
                // set left bound y position
                lpos.y = -frameHeight * 0.5f;

                //Debug.Log("Left pos x: " + lpos.x);
                //Debug.Log("Left pos y: " + lpos.y);
                // set left bound position
                l.localPosition = lpos;
                // set left bound y scale
                lscale.y = frameHeight + BoundFrameWidth * 2;
                //Debug.Log("Left scale y: " + lscale.y);
                // set left bound scale
                l.localScale = lscale;
                // set left plane
                planes[0] = new NPlane {
                    point = new Vector3(lpos.x + BoundFrameWidth * 0.5f, lpos.y, lpos.z),
                    //point = new Vector3(lpos.x, lpos.y, lpos.z),
                    normal = new Vector3(1, 0, 0)
                };

                var rpos = r.localPosition;
                var rscale = l.localScale;
                rpos.x = (frameWidth + BoundFrameWidth * 0.5f);
                rpos.y = -frameHeight * 0.5f;
                //Debug.Log("Right X: " + rpos.x);
                //Debug.Log("Right Y: " + rpos.y);
                r.localPosition = rpos;
                rscale.y = frameHeight + BoundFrameWidth * 2;
                r.localScale = rscale;
                //Debug.Log("Right scale x: " + r.localScale.x);
                //Debug.Log("Right scale y: " + r.localScale.y);
                planes[1] = new NPlane {
                    point = new Vector3(rpos.x - BoundFrameWidth * 0.5f, rpos.y, rpos.z),
                    normal = new Vector3(-1, 0, 0)
                };

                var tpos = t.localPosition;
                var tscale = t.localScale;
                tpos.x = frameWidth * 0.5f;
                tpos.y = (-BoundFrameWidth * 0.5f) + 0.125f;
                //Debug.Log("Top X: " + tpos.x);
                //Debug.Log("Top Y: " + tpos.y);
                t.localPosition = tpos;
                tscale.x = frameWidth;
                t.localScale = tscale;
                //Debug.Log("Top scale x: " + t.localScale.x);
                //Debug.Log("Top scale y: " + t.localScale.y);
                planes[2] = new NPlane {
                    point = new Vector3(tpos.x, (tpos.y + BoundFrameWidth * 0.5f) - 0.125f, tpos.z),
                    normal = new Vector3(0, 1, 0)
                };

                var bpos = b.localPosition;
                var bscale = t.localScale;
                bpos.x = frameWidth * 0.5f;
                bpos.y = (-frameHeight + BoundFrameWidth * 0.5f) - 0.125f;
                //Debug.Log("Bottom X: " + bpos.x);
                //Debug.Log("Bottom Y: " + bpos.y);
                b.localPosition = bpos;
                bscale.x = frameWidth;
                b.localScale = bscale;
                //Debug.Log("Bottom scale x: " + b.localScale.x);
                //Debug.Log("Bottom scale y: " + b.localScale.y);
                planes[3] = new NPlane {
                    point = new Vector3(bpos.x, bpos.y - BoundFrameWidth * 0.5f, bpos.z),
                    normal = new Vector3(0, -1, 0)
                };

                boundsHeight = frameHeight;
                if (aimer != null) {
                    // set aimer position based on frame size and ball radius
                    aimer.localPosition = new Vector3(
                        frameWidth * 0.5f,
                        (frameHeight - ballRadius - BoundFrameWidth * 0.5f) - 0.05f + aimOffsetBg,
                        0
                    );
                    //Debug.Log("Aim pos x: " + frameWidth * 0.5f);
                    //Debug.Log("Aim pos y: " + (frameHeight - ballRadius - BoundFrameWidth * 0.5f));
                }

                if (mainCamera == null) {
                    // set camera position based on frame size and ball radius
                    mainCamera = GetComponentInChildren<Camera>();
                    if (mainCamera != null) {
                        var pos = mainCamera.transform.localPosition;
                        pos.x = frameWidth * 0.5f;
                        pos.y = frameHeight * 0.5f;
                        //Debug.Log("Pos X: " + pos.x);
                        //Debug.Log("Pos Y: " + pos.y);
                        mainCamera.transform.localPosition = pos;
                    }
                }

                for (int i = 0; i < initLineCount && i < maxLine; ++i) {
                    NewLine(false);
                }

                // Update current colors
                UpdateColors();
            }
        }
        for (int i = 0; i < screenOffset; i++) {
            NewBlankLine(false);
            actualLine++;
            deadLine++;
        }
    }

    void Update() {
        // if press space bar add new line to game (DEBUG)
        /*if (Input.GetKeyDown(KeyCode.Space) && flyingBall == null && levelUpdating == false) {
            //NewLine();
            NewBlankLine();
            //_CheckDeadline();
             if (CheckDeadline()) {
                TimeSpan ts = zeit.Elapsed;
                zeit.Stop();
                Color color = cylinder.GetComponent<MeshRenderer>().material.color;
                color.a = 0;
                transpMaterial.color = color;
                cylinder.GetComponent<MeshRenderer>().material = transpMaterial;
                ring.GetComponent<MeshRenderer>().material = transpMaterial;

                List<int> highscores = new List<int>();

                if (!PlayerPrefs.HasKey("highscoreLevel"+(level+1))) {
                    highscores.Add(shootCount);
                    highscores.Add(ts.Minutes);
                    highscores.Add(ts.Seconds);
                    PlayerPrefs.SetString("highscoreLevel" + (level + 1), GetSerializedString(highscores));

                    if (highscores[2] > 60) {
                        txtUserScore1.GetComponent<Text>().text = 1 + " - " + highscores[0] + " Shots " + highscores[1] + " Min " + highscores[2] + " Sec";
                    } else {
                        txtUserScore1.GetComponent<Text>().text = 1 + " - " + highscores[0] + " Shots " + highscores[2] + " Sec";
                    }

                    txtUserScore1.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                } else {
                    //Debug.Log("Serialized: " + PlayerPrefs.GetString("highscoreLevel"+(level+1)));
                    highscores = GetHighScores(PlayerPrefs.GetString("highscoreLevel" + (level + 1)));
                    int i = 0;

                    for (i = 0; i < (highscores.Count / 3); i++) {
                        if (highscores[i*3] > shootCount) {
                            highscores.Insert(i*3, shootCount);
                            highscores.Insert((i*3)+1, ts.Minutes);
                            highscores.Insert((i*3)+2, ts.Seconds);
                            break;
                        } else if (highscores[i * 3] == shootCount && highscores[(i * 3)+1] > ts.Minutes) {
                            highscores.Insert(i * 3, shootCount);
                            highscores.Insert((i * 3) + 1, ts.Minutes);
                            highscores.Insert((i * 3) + 2, ts.Seconds);
                            break;
                        } else if (highscores[i * 3] == shootCount && highscores[(i * 3)+1] == ts.Minutes && highscores[(i * 3)+2] > ts.Seconds) {
                            highscores.Insert(i * 3, shootCount);
                            highscores.Insert((i * 3) + 1, ts.Minutes);
                            highscores.Insert((i * 3) + 2, ts.Seconds);
                            break;
                        } else if (highscores[i * 3] == shootCount && highscores[(i * 3)+1] == ts.Minutes && highscores[(i * 3)+2] == ts.Seconds) {
                            highscores[i * 3] = shootCount;
                            highscores[(i * 3) + 1] = ts.Minutes;
                            highscores[(i * 3) + 2] = ts.Seconds;
                            break;
                        }

                    }
                    if (i == (highscores.Count/3)) {
                        highscores.Add(shootCount);
                        highscores.Add(ts.Minutes);
                        highscores.Add(ts.Seconds);
                    }
                    //Debug.Log("Highscore Serialized: " + GetSerializedString(highscores));
                    PlayerPrefs.SetString("highscoreLevel" + (level + 1), GetSerializedString(highscores));

                    for (int j = 0; j < highscores.Count/3 && j < 12; j++) {
                        if (j == 0) {
                            if (highscores[(j * 3) + 2] > 60) {
                                txtUserScore1.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                            } else {
                                txtUserScore1.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                            }
                        } else if (j == 1) {
                            txtUserScore2.SetActive(true);
                            if (highscores[(j * 3) + 2] > 60) {
                                txtUserScore2.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                            } else {
                                txtUserScore2.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                            }
                        } else if (j == 2) {
                            txtUserScore3.SetActive(true);
                            if (highscores[(j * 3) + 2] > 60) {
                                txtUserScore3.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                            } else {
                                txtUserScore3.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                            }
                        } else if (j == 3) {
                            txtUserScore4.SetActive(true);
                            if (highscores[(j * 3) + 2] > 60) {
                                txtUserScore4.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                            } else {
                                txtUserScore4.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                            }
                        } else if (j == 4) {
                            txtUserScore5.SetActive(true);
                            if (highscores[(j * 3) + 2] > 60) {
                                txtUserScore5.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 1] + " Min " + highscores[(j * 3) + 2] + " Sec";
                            } else {
                                txtUserScore5.GetComponent<Text>().text = j + 1 + " - " + highscores[j * 3] + " Shots " + highscores[(j * 3) + 2] + " Sec";
                            }
                        }
                    }

                    switch (i) {
                        case 0:
                            txtUserScore1.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                            break;
                        case 1:
                            txtUserScore2.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                            break;
                        case 2:
                            txtUserScore3.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                            break;
                        case 3:
                            txtUserScore4.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                            break;
                        case 4:
                            txtUserScore5.GetComponent<Text>().fontStyle = FontStyle.BoldAndItalic;
                            break;
                    }
                }
                canvasMenuGameOver.SetActive(true);
             }
        }*/

        // if mouse left click set aimer rotation
        if (Input.GetMouseButton(0) && !canvasMenuOnGame.activeSelf && !canvasMenuEndGame.activeSelf && !canvasMenuGameOver.activeSelf && !canvasMenuConfirm.activeSelf) {
            var pt = Input.mousePosition;
            //Debug.Log("Point x: " + pt.x + " y: " + pt.y);
            if ( mainCamera != null && pt.y > 280) {
                var o = mainCamera.WorldToScreenPoint( aimer.position );
                //Debug.Log("Bound Height: " + boundsHeight);
                o.y = Mathf.Clamp( o.y, 0, boundsHeight );
                o.z = 0;
                pt.z = 0;
                var dir = pt - o;
                if ( dir.sqrMagnitude > Vector3.kEpsilon ) {
                    var rangle = Mathf.Atan2( dir.y, dir.x );
                    var angle = Mathf.Rad2Deg * rangle;
                    angle = Mathf.Clamp( angle, 30, 150 );
                    angle -= 90;

                    //Debug.Log(aimer.rotation.eulerAngles.z - angle);
                    if (aimer.rotation.eulerAngles.z - angle > 360) {
                        if(aimer.rotation.eulerAngles.z - angle-360 > 2) {
                            playOneShot(1); // Left Cannon Move
                        }
                    } else if (aimer.rotation.eulerAngles.z - angle < 359 && aimer.rotation.eulerAngles.z - angle > 350) {
                        if(aimer.rotation.eulerAngles.z - angle-360 < 2) {
                            playOneShot(0); // Right Cannon Move
                        }
                    } else if (aimer.rotation.eulerAngles.z - angle > 0 && aimer.rotation.eulerAngles.z - angle < 10) {
                        if(aimer.rotation.eulerAngles.z - angle > 2) {
                            playOneShot(1); // Left Cannon Move
                        }
                    } else if (aimer.rotation.eulerAngles.z - angle < -1 && aimer.rotation.eulerAngles.z - angle > -10) {
                        if(aimer.rotation.eulerAngles.z - angle < 2) {
                            playOneShot(0); // Right Cannon Move
                        }
                    }

                    aimer.rotation = Quaternion.Euler( 0, 0, angle );

                    cylinderAxis.transform.rotation = Quaternion.Euler( angle-90, -90, 90 );
                    ring.transform.rotation = Quaternion.Euler(angle-90, -90, 90);

                    
                }
                if (!zeit.IsRunning) {
                    zeit.Start();
                }               
                    
            }
        }

        // prepare ball to shoot
        if ( shootBallState == 0 && !canvasMenuOnGame.activeSelf && !canvasMenuEndGame.activeSelf && !canvasMenuGameOver.activeSelf && !canvasMenuConfirm.activeSelf) {
            shootBallState = 1;
            
            //Debug.Log("Active Colors: "+ active_colors.Count);
            var rnd = UnityEngine.Random.Range(0, activeColors.Count);
            //Debug.Log(active_colors[rnd]);
            shootBallColor = activeColors[rnd];
            var ball = Instantiate( ballGO );
            ball.name = "[ball]";
            ball.SetActive(false);
            ball.transform.parent = aimer;
            ball.transform.localPosition = Vector3.zero;
            if (!cylinder.GetComponent<Renderer>()) {
                cylinder.transform.GetChild(0).GetComponent<Renderer>().material.color = ColorPalette[shootBallColor - 1];
                cylinder.transform.GetChild(2).GetComponent<Renderer>().material.color = ColorPalette[shootBallColor - 1];
            } else {
                cylinder.GetComponent<Renderer>().material.color = ColorPalette[shootBallColor - 1];
            }
            

            FadeInBall( 
                ball.GetComponent<Renderer>().material,
                shootBallColor,
                () => {
                    shootBallState = 2;
                }
            );
        }

        // shoot when mouse up and ball is prepared
        if (shootBallState == 2 && (Input.GetMouseButtonUp( 0 ) || flagTest == 2) && !canvasMenuGameOver.activeSelf && !canvasMenuOnGame.activeSelf && !canvasMenuEndGame.activeSelf && !canvasMenuConfirm.activeSelf && flyingBall == null) {
            var pt = Input.mousePosition;
            if (pt.y < 280)
                return;
            shootBallState = 0;
            activeBalls++;
            //Debug.Log("Shoot 1");
            var b = aimer.Find( "[ball]" );
            b.gameObject.SetActive(true);
            if ( b != null ) {
                b.transform.parent = currentTransform;
                flyingBall = new FlyingBall {
                    radius = ballRadius,
                    transform = b.transform,
                    movedir = aimer.parent.TransformVector(aimer.up),
                    speed = initialSpeed,
                    color = shootBallColor
                };
            }
            playOneShot(2);
            shootCount++;
            flagTest = 0;
            txtHurryUp.SetActive(false);
            if (toggleHurryUp.isOn) {
                hurryTime.Restart();
            }
        }

        // move ball
        if ( flyingBall != null ) {
            //Debug.Log("Shoot 2");
            BallMove( ref flyingBall );
            
        }

        if (hurryTime.ElapsedMilliseconds > 20000 && toggleHurryUp.isOn) {
            //Debug.Log("Hurry Up Shoot");
            flagTest = 2;
            txtHurryUp.SetActive(false);
            hurryTime.Restart();
        } else if (hurryTime.ElapsedMilliseconds > 10000 && toggleHurryUp.isOn) {
            if (flagTest == 0) {
                //Debug.Log("Hurry Up");
                flagTest = 1;
                txtHurryUp.SetActive(true);
            }
        }

        if (canvasMenuOnGame.activeSelf && zeit.IsRunning) {
            //TimeSpan ts = zeit.Elapsed;
            zeit.Stop();
            flagTest = 0;
            txtHurryUp.SetActive(false);
            
            hurryTime.Restart();
            hurryTime.Stop();
            //Debug.Log("Stopped " + ts);
        } else if (!canvasMenuOnGame.activeSelf && !zeit.IsRunning && shootCount > 0 && !canvasMenuEndGame.activeSelf && !canvasMenuGameOver.activeSelf && !canvasMenuConfirm.activeSelf) {
            //TimeSpan ts = zeit.Elapsed;
            zeit.Start();
            if (toggleHurryUp.isOn) {
                hurryTime.Restart();
            }
            //Debug.Log("Started " + ts);
        }
    }
}
