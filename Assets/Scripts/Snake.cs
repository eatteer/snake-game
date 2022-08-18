using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour
{
    public int xSize, ySize;

    public GameObject headPrefab;
    public GameObject tailPrefab;
    public GameObject edgePrefab;
    public GameObject foodPrefab;

    public Material headMaterial;
    public Material tailMaterialPrimary;
    public Material tailMaterialSecondary;
    public Material edgeMaterial;
    public Material foodMaterial;

    private float frameRate = 0.5f;

    private int score = 0;
    private Vector3 direction;
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
        // Set snake initial direction
        direction = Vector3.up;
        GenerateMap();
        GenerateSnake();
        GenerateFood();
        InvokeRepeating("MoveSnake", frameRate, frameRate);
    }

    // Update is called once per frame
    void Update()
    {
        bool isGameOver = DidSnakeCollidedEdge() || DidSnakeCollidedItself();
        if (isGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (DidSnakeAte())
        {
            score++;
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
            float xAxis = x - (xSize / 2);
            float yAxis = ySize / 2;

            GameObject topEdge = Instantiate(edgePrefab);
            PlaceGameObject(topEdge, xAxis, yAxis);
            SetMaterial(topEdge, edgeMaterial);

            GameObject bottomEdge = Instantiate(edgePrefab);
            PlaceGameObject(bottomEdge, xAxis, -yAxis);
            SetMaterial(bottomEdge, edgeMaterial);
        }

        for (int y = 0; y <= ySize; y++)
        {
            float xAxis = xSize / 2;
            float yAxis = y - (ySize / 2);

            GameObject leftEdge = Instantiate(edgePrefab);
            PlaceGameObject(leftEdge, -xAxis, yAxis);
            SetMaterial(leftEdge, edgeMaterial);

            GameObject rightEdge = Instantiate(edgePrefab);
            PlaceGameObject(rightEdge, xAxis, yAxis);
            SetMaterial(rightEdge, edgeMaterial);
        }
    }

    private void GenerateSnake()
    {
        // Config head
        head = Instantiate(headPrefab);
        head.transform.position = new Vector3(0, 0, 0);
        PlaceGameObject(head, 0, 0);
        SetMaterial(head, headMaterial);

        // Config empty tail
        tail = new List<GameObject>();
    }

    private void GenerateFood()
    {
        // Generate random positions for the food until one doesn't collide with the snake
        Vector3 newPosition;
        bool collisioned = false;
        do
        {
            newPosition = generateRandomPositionWithinMap();

            foreach (GameObject tailFragment in tail)
            {
                collisioned = newPosition.Equals(tailFragment.transform.position);
                if (collisioned) break;
            }

        } while (collisioned);


        // Config food
        food = Instantiate(foodPrefab);
        food.transform.position = newPosition;
        SetMaterial(food, foodMaterial);
    }

    private Vector3 generateRandomPositionWithinMap()
    {
        System.Random random = new System.Random();
        int x = random.Next(((xSize / 2) - 1) * -1, (xSize / 2 - 1));
        int y = random.Next(((ySize / 2) - 1) * -1, (ySize / 2 - 1));

        Vector3 position = new Vector3(x, y, 0);
        return position;
    }

    private bool DidSnakeCollidedEdge()
    {
        int leftEdge = (xSize / 2) * -1;
        int rightEdge = xSize / 2;
        int topEdge = ySize / 2;
        int bottomEdge = (ySize / 2) * -1;

        Vector3 snakePosition = head.transform.position;

        bool didCollision = snakePosition.x == leftEdge || snakePosition.x == rightEdge || snakePosition.y == bottomEdge || snakePosition.y == topEdge;
        return didCollision;
    }

    private bool DidSnakeCollidedItself()
    {
        bool collided = false;

        foreach (GameObject tailFragment in tail)
        {
            collided = tailFragment.transform.position.Equals(head.transform.position);
            if (collided) break;
        }

        return collided;
    }

    private bool DidSnakeAte()
    {
        Vector3 snakePosition = head.transform.position;
        Vector3 foodPosition = food.transform.position;

        bool didAte = snakePosition.Equals(foodPosition);
        return didAte;
    }

    private void GenerateTailFragment()
    {
        GameObject tailFragment = Instantiate(tailPrefab);
        Vector3 headPosition = head.transform.position;

        if (direction.Equals(new Vector3(1, 0, 0)))
        {
            tailFragment.transform.position = new Vector3(headPosition.x - 1, headPosition.y, 0);
        }

        if (direction.Equals(new Vector3(-1, 0, 0)))
        {
            tailFragment.transform.position = new Vector3(headPosition.x + 1, headPosition.y, 0);
        }

        if (direction.Equals(new Vector3(0, 1, 0)))
        {
            tailFragment.transform.position = new Vector3(headPosition.x, headPosition.y - 1, 0);
        }

        if (direction.Equals(new Vector3(0, -1, 0)))
        {
            tailFragment.transform.position = new Vector3(headPosition.x, headPosition.y + 1, 0);
        }

        // Set different material if tail fragment to be added is odd or even
        Material tailMaterial = tail.Count % 2 != 0 ? tailMaterialSecondary : tailMaterialPrimary;
        SetMaterial(tailFragment, tailMaterial);

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

    private void PlaceGameObject(GameObject gameObject, float x, float y)
    {
        gameObject.GetComponent<Transform>().position = new Vector3(x, y, 0);
    }

    private void SetMaterial(GameObject gameObject, Material material)
    {
        gameObject.GetComponent<MeshRenderer>().material = material;
    }
}
