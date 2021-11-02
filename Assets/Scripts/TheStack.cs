using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TheStack : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public Color32[] gameColors = new Color32[4];
    public Material stackMat;
    public GameObject endPanel;
    public GameObject gameMusic;
    public GameObject loseMusic;

    private const float BoundsSize = 3.5f;
    private const float StackMovingSpeed = 5.0f;
    private const float ErrorMargin = 0.25f;
    private const float StackBoundsGain = 0.25f;
    private const int ComboStartGain = 3;


    private GameObject[] _theStack;
    private Vector2 _stackBounds = new Vector2(BoundsSize, BoundsSize);

    private int _stackIndex;
    private int _scoreCount = 0;
    private int _combo = 0;

    private float _tileTransition = 0.0f;
    private float tileSpeed = 2.5f;
    private float _secondayPosition;

    private bool _isMovingOnX = true;
    private bool _gameOver = false;

    private Vector3 _desiredPosition;
    private Vector3 _lastTilePostition;

    private void Start()
    {
        _theStack = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            _theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(_theStack[i].GetComponent<MeshFilter>().mesh);
        }

        _stackIndex = transform.childCount - 1;

        loseMusic.SetActive(false);
    }


    private void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        go.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(go.GetComponent<MeshFilter>().mesh);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                _scoreCount++;
                scoreText.text = _scoreCount.ToString();
            }
            else
            {
                EndGame();
            }
        }

        MoveTile();

        //Move to Stack

        transform.position = Vector3.Lerp(transform.position, _desiredPosition, StackMovingSpeed * Time.deltaTime);
    }

    private void MoveTile()
    {
        if (_gameOver)
            return;

        _tileTransition += Time.deltaTime * tileSpeed;
        if (_isMovingOnX)
            _theStack[_stackIndex].transform.localPosition =
                new Vector3(Mathf.Sin(_tileTransition) * BoundsSize, _scoreCount, _secondayPosition);
        else
            _theStack[_stackIndex].transform.localPosition =
                new Vector3(_secondayPosition, _scoreCount, Mathf.Sin(_tileTransition) * BoundsSize);
    }

    private void SpawnTile()
    {
        _lastTilePostition = _theStack[_stackIndex].transform.localPosition;
        _stackIndex--;
        if (_stackIndex < 0)
            _stackIndex = transform.childCount - 1;

        _desiredPosition = (Vector3.down) * _scoreCount;
        _theStack[_stackIndex].transform.localPosition = new Vector3(0, _scoreCount, 0);
        _theStack[_stackIndex].transform.localScale = new Vector3(_stackBounds.x, 1, _stackBounds.y);

        ColorMesh(_theStack[_stackIndex].GetComponent<MeshFilter>().mesh);
    }

    private bool PlaceTile()
    {
        Transform t = _theStack[_stackIndex].transform;

        if (_isMovingOnX)
        {
            float deltaX = _lastTilePostition.x - t.position.x;
            if (Mathf.Abs(deltaX) > ErrorMargin)
            {
                //CUT THE TILE
                _combo = 0;
                _stackBounds.x -= Mathf.Abs(deltaX);
                if (_stackBounds.x <= 0)
                    return false;

                float middle = _lastTilePostition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(_stackBounds.x, 1, _stackBounds.y);

                CreateRubble
                (new Vector3((t.position.x > 0)
                            ? t.position.x + (t.localScale.x / 2)
                            : t.position.x - (t.localPosition.x / 2)
                        , t.position.y
                        , t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z)
                );

                t.localPosition = new Vector3(middle - (_lastTilePostition.x / 2), _scoreCount, _lastTilePostition.z);
            }
            else
            {
                if (_combo > ComboStartGain)
                {
                    _stackBounds.x += StackBoundsGain;
                    if (_stackBounds.x > BoundsSize)
                        _stackBounds.x = BoundsSize;

                    float middle = _lastTilePostition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(_stackBounds.x, 1, _stackBounds.y);
                    t.localPosition = new Vector3(middle - (_lastTilePostition.x / 2), _scoreCount,
                        _lastTilePostition.z);
                }

                _combo++;
                t.localPosition = new Vector3(_lastTilePostition.x, _scoreCount, _lastTilePostition.z);
            }
        }
        else
        {
            float deltaZ = _lastTilePostition.z - t.position.z;
            if (Mathf.Abs(deltaZ) > ErrorMargin)
            {
                //CUT THE TILE
                _combo = 0;
                _stackBounds.y -= Mathf.Abs(deltaZ);
                if (_stackBounds.y <= 0)
                    return false;

                float middle = _lastTilePostition.z + t.localPosition.z / 2;
                t.localScale = new Vector3(_stackBounds.x, 1, _stackBounds.y);

                CreateRubble
                (new Vector3(t.position.x
                        , t.position.y
                        , (t.position.z > 0)
                            ? t.position.z + (t.localScale.z / 2)
                            : t.position.z - (t.localPosition.z / 2)),
                    new Vector3(Mathf.Abs(deltaZ), 1, t.localScale.z)
                );

                t.localPosition = new Vector3(_lastTilePostition.x, _scoreCount, middle - (_lastTilePostition.z / 2));
            }
            else
            {
                if (_combo > ComboStartGain)
                {
                    if (_stackBounds.y > BoundsSize)
                        _stackBounds.y = BoundsSize;

                    _stackBounds.y += StackBoundsGain;
                    float middle = _lastTilePostition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(_stackBounds.x, 1, _stackBounds.y);
                    t.localPosition = new Vector3(_lastTilePostition.x, _scoreCount,
                        middle - (_lastTilePostition.z / 2));
                }

                _combo++;
                t.localPosition = new Vector3(_lastTilePostition.x, _scoreCount, _lastTilePostition.z);
            }
        }

        _secondayPosition = (_isMovingOnX)
            ? t.localPosition.x
            : t.localPosition.z;
        _isMovingOnX = !_isMovingOnX;

        return true;
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float f = Mathf.Sin(_scoreCount * 0.25f);

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);

        mesh.colors32 = colors;
    }

    private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
            return Color.Lerp(a, b, t / 0.33f);
        else if (t < 0.66f)
            return Color32.Lerp(b, c, (t - 0.33f) / 0.33f);
        else
            return Color.Lerp(c, d, (t - 0.66f) / 0.33f);
    }

    private void EndGame()
    {
        Debug.Log("LOSE");
        _gameOver = true;
        endPanel.SetActive(true);
        gameMusic.SetActive(false);
        loseMusic.SetActive(true);
        _theStack[_stackIndex].AddComponent<Rigidbody>();
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}