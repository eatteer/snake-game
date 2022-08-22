using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Snake : MonoBehaviour
{
    public int xSize, ySize;

    public GameObject headPrefab;
    public GameObject tailPrefab;
    public GameObject edgePrefab;
    public GameObject foodPrefab;

    public Material headMaterial;
    public Material tailPrimaryMaterial;
    public Material tailSecondaryMaterial;
    public Material edgeMaterial;
    public Material foodMaterial;

    public GameObject UIScoreText;
    public GameObject UIRestartButton;
    public GameObject UIGameoverText;

    private float frameRate = 0.5f;

    private int score;
    private Vector3 direction;
    private bool isGameOver;
    private GameObject head;
    private List<GameObject> tail;
    private GameObject food;

    private Dictionary<KeyCode, Vector3> directionDictionary = new Dictionary<KeyCode, Vector3>
    {
        [KeyCode.LeftArrow] = Vector3.left,
        [KeyCode.RightArrow] = Vector3.right,
        [KeyCode.UpArrow] = Vector3.up,
        [KeyCode.DownArrow] = Vector3.down,
    };

    // Start is called before the first frame update
    void Start()
    {
        // Set UI text score
        UIScoreText.GetComponent<Text>().text = $"Score: {0}";
        UIGameoverText.SetActive(false);

        // Config button events
        UIRestartButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        // Disable restart button until gameover
        UIRestartButton.SetActive(false);


        Time.timeScale = 1;
        isGameOver = false;
        score = 0;
        direction = Vector3.up;

        GenerateMap();
        GenerateSnake();
        GenerateFood();
        InvokeRepeating("MoveSnake", frameRate, frameRate);
    }

    // Update is called once per frame
    void Update()
    {
        isGameOver = DidSnakeCollideEdge() || DidSnakeCollideItself();
        if (isGameOver)
        {
            // Stop game
            Time.timeScale = 0;
            UIRestartButton.SetActive(true);
            UIGameoverText.SetActive(true);
        }

        if (DidSnakeEat())
        {
            UIScoreText.GetComponent<Text>().text = $"Score: {++score}";

            Destroy(food);
            GenerateFood();
            GenerateTailFragment();
        }

        ChangeSnakeDirectionIfKeyPressed();
    }

    void MoveSnake()
    {
        // Move head
        Vector3 savedPosition = head.transform.position;
        head.transform.Translate(direction);

        // Move tail
        foreach (GameObject tailFragment in tail)
        {
            // Save the position of the tail fragment that is about to move (successor)
            // This way, its predecessor can take its position.
            Vector3 successorPosition = tailFragment.transform.position;
            tailFragment.transform.position = savedPosition; // In the first iteration, the successor is the head
            savedPosition = successorPosition;
        }
    }

    private void GenerateMap()
    {
        for (int x = 0; x <= xSize; x++)
        {
            float xPoint = x - (xSize / 2);
            float yPoint = ySize / 2;

            GameObject topEdge = Instantiate(edgePrefab);
            topEdge.transform.position = new Vector3(xPoint, yPoint, 0);
            topEdge.GetComponent<MeshRenderer>().material = edgeMaterial;

            GameObject bottomEdge = Instantiate(edgePrefab);
            bottomEdge.transform.position = new Vector3(xPoint, -yPoint, 0);
            bottomEdge.GetComponent<MeshRenderer>().material = edgeMaterial;
        }

        for (int y = 0; y <= ySize; y++)
        {
            float xPoint = xSize / 2;
            float yPoint = y - (ySize / 2);

            GameObject leftEdge = Instantiate(edgePrefab);
            leftEdge.transform.position = new Vector3(-xPoint, yPoint, 0);
            leftEdge.GetComponent<MeshRenderer>().material = edgeMaterial;

            GameObject rightEdge = Instantiate(edgePrefab);
            rightEdge.transform.position = new Vector3(xPoint, yPoint, 0);
            rightEdge.GetComponent<MeshRenderer>().material = edgeMaterial;
        }
    }

    private void GenerateSnake()
    {
        // Config head
        head = Instantiate(headPrefab);
        head.transform.position = new Vector3(0, 0, 0);
        head.GetComponent<MeshRenderer>().material = headMaterial;

        // Config empty tail
        tail = new List<GameObject>();
    }

    private void GenerateFood()
    {
        // Generate random positions for the food until one doesn't collide with the snake
        Vector3 position;
        bool didCollide = false;
        do
        {
            position = generateRandomPositionWithinMap();

            foreach (GameObject tailFragment in tail)
            {
                didCollide = position.Equals(tailFragment.transform.position);
                if (didCollide) break;
            }

        } while (didCollide);


        // Config food
        food = Instantiate(foodPrefab);
        food.transform.position = position;
        food.GetComponent<MeshRenderer>().material = foodMaterial;
    }

    private Vector3 generateRandomPositionWithinMap()
    {
        System.Random random = new System.Random();
        int x = random.Next(((xSize / 2) - 1) * -1, (xSize / 2 - 1));
        int y = random.Next(((ySize / 2) - 1) * -1, (ySize / 2 - 1));

        Vector3 position = new Vector3(x, y, 0);
        return position;
    }

    private bool DidSnakeCollideEdge()
    {
        int leftEdge = (xSize / 2) * -1;
        int rightEdge = xSize / 2;
        int topEdge = ySize / 2;
        int bottomEdge = (ySize / 2) * -1;

        Vector3 snakePosition = head.transform.position;

        bool didCollide = snakePosition.x == leftEdge || snakePosition.x == rightEdge || snakePosition.y == bottomEdge || snakePosition.y == topEdge;
        return didCollide;
    }

    private bool DidSnakeCollideItself()
    {
        bool didCollide = false;

        foreach (GameObject tailFragment in tail)
        {
            didCollide = tailFragment.transform.position.Equals(head.transform.position);
            if (didCollide) break;
        }

        return didCollide;
    }

    private bool DidSnakeEat()
    {
        Vector3 snakePosition = head.transform.position;
        Vector3 foodPosition = food.transform.position;

        bool didEat = snakePosition.Equals(foodPosition);
        return didEat;
    }

    private void GenerateTailFragment()
    {
        GameObject tailFragment = Instantiate(tailPrefab);
        Vector3 headPosition = head.transform.position; ;


        if (direction.Equals(Vector3.right))
        {
            tailFragment.transform.position = new Vector3(headPosition.x - 1, headPosition.y, 0);
        }

        if (direction.Equals(Vector3.left))
        {
            tailFragment.transform.position = new Vector3(headPosition.x + 1, headPosition.y, 0);
        }

        if (direction.Equals(Vector3.up))
        {
            tailFragment.transform.position = new Vector3(headPosition.x, headPosition.y - 1, 0);
        }

        if (direction.Equals(Vector3.down))
        {
            tailFragment.transform.position = new Vector3(headPosition.x, headPosition.y + 1, 0);
        }


        // Set different material if the tail fragment to be added is odd or even
        Material tailMaterial = tail.Count % 2 != 0 ? tailSecondaryMaterial : tailPrimaryMaterial;
        tailFragment.GetComponent<MeshRenderer>().material = tailMaterial;
        tail.Add(tailFragment);
    }

    private void ChangeSnakeDirectionIfKeyPressed()
    {
        // Get every key of direction dictionary and determinate if was pressed
        foreach (var directionPair in directionDictionary)
        {
            if (Input.GetKeyDown(directionPair.Key))
            {
                Vector3 newDirection = directionPair.Value;
                // If the new direction to which snake wants to move is the opposite direction
                // then direction is not changed
                direction =
                    doesSnakeHaveTail() && isOppositeDirection(newDirection)
                    ? direction // No change of direction
                    : newDirection; // Change of direction
            }
        }
    }

    private bool isOppositeDirection(Vector3 newDirection)
    {
        return Vector3.zero.Equals((direction + newDirection));
    }

    private bool doesSnakeHaveTail()
    {
        return tail.Count > 0;
    }
}
