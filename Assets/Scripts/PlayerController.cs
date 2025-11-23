using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    [Header("Student ID")]
    public int studentID = 22120061;

    [Header("GameObject A")]
    [Range(0f, 5f)] public float speed = 1f;
    bool isObjectARotating = false;
    bool isObjectAMoving = false;
    bool isHoveringObjectA = false;
    Color originalColor;
    int points = 0;

    [Header("GameObject B")]
    public GameObject prefabObjectB;
    public float force = 100f;
    bool isObjectBRotating = false;

    void Start()
    {
        float randomY = Random.Range(10, 20);
        transform.position = new Vector3(0, randomY, 0);

        originalColor = GetComponent<Renderer>().material.color;
    }

    void Update()
    {
        float HorizontalInput = Input.GetAxis("Horizontal");
        float VerticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(HorizontalInput, VerticalInput, 0) * speed * Time.deltaTime;
        transform.Translate(movement);

        if (Input.GetKeyDown(KeyCode.Space)) SpawnObjectB();
        if (Input.GetKeyDown(KeyCode.Q)) StartDestroySequence();

        if (Input.GetKeyDown(KeyCode.R))
        {
            isObjectARotating = !isObjectARotating;
            if (isObjectARotating) StartRotatingObjectA();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            isObjectBRotating = !isObjectBRotating;
            StartRotatingObjectB();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            isObjectAMoving = !isObjectAMoving;
            if (isObjectAMoving) StartMovingObjectA();
        }

        // Hovering logic for Object A
        ChangeColorOnHoverObjectA();

        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeColorOnAllObjectB();
        }

        if (Input.GetMouseButtonDown(0))
        {
            DestroyObjectBOnClick();
        }
    }

    // ----------------------------------------------------------
    // SPAWN OBJECT B
    // ----------------------------------------------------------
    void SpawnObjectB()
    {
        float randomPosX = Random.Range(-5f, 5f);
        float randomPosZ = Random.Range(5f, 15f);
        float randomPosY = Random.Range(10f, 10f + studentID % 10);

        Vector3 spawnPosition = new Vector3(randomPosX, randomPosY, randomPosZ);

        GameObject newObjectB = Instantiate(prefabObjectB, spawnPosition, Quaternion.identity);

        Rigidbody rb = newObjectB.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forceDirection = new Vector3(
                Random.Range(-1f, 1f),
                1,
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(forceDirection * force);
        }
    }

    // ----------------------------------------------------------
    // DESTROY OBJECT B
    // ----------------------------------------------------------
    void StartDestroySequence()
    {
        GameObject[] ObjectB = GameObject.FindGameObjectsWithTag("ObjectB");

        if (ObjectB.Length > 0)
        {
            StartCoroutine(DestroyAllObjectBCoroutine(ObjectB));
        }
    }

    IEnumerator DestroyAllObjectBCoroutine(GameObject[] objects)
    {
        // Fisher-Yates Shuffle 
        for (int i = objects.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            GameObject temp = objects[i];
            objects[i] = objects[randomIndex];
            objects[randomIndex] = temp;
        }

        foreach (GameObject obj in objects)
        {
            yield return new WaitForSeconds(2f);
            Destroy(obj);
        }
    }

    // ----------------------------------------------------------
    // ROTATE OBJECT A
    // ----------------------------------------------------------
    void StartRotatingObjectA()
    {
        StartCoroutine(RotateObjectACoroutine());
    }

    IEnumerator RotateObjectACoroutine()
    {
        while (isObjectARotating)
        {
            transform.Rotate(Vector3.up * 100f * Time.deltaTime);
            yield return null;
        }
    }

    // ----------------------------------------------------------
    // ROTATE OBJECT B
    // ----------------------------------------------------------
    void StartRotatingObjectB()
    {
        GameObject[] ObjectB = GameObject.FindGameObjectsWithTag("ObjectB");
        if (ObjectB.Length > 0 && isObjectBRotating)
        {
            StartCoroutine(RotateObjectBCoroutine(ObjectB));
        }
        else
        {
            isObjectBRotating = false;
        }
    }

    IEnumerator RotateObjectBCoroutine(GameObject[] objects)
    {
        float rotationSpeed = 100f;
        float newHeight = 15;

        Vector3 randomRotateDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                obj.transform.position = new Vector3(obj.transform.position.x, newHeight, obj.transform.position.z);
            }
        }

        while (isObjectBRotating)
        {
            foreach (GameObject obj in objects)
            {
                if (obj != null)
                {
                    obj.transform.Rotate(randomRotateDirection * rotationSpeed * Time.deltaTime);
                }
            }
            yield return null;
        }

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = true;
                }
            }
        }
    }

    // ----------------------------------------------------------
    // MOVE OBJECT A
    // ----------------------------------------------------------
    void StartMovingObjectA()
    {
        StartCoroutine(MoveObjectACoroutine());
    }

    IEnumerator MoveObjectACoroutine()
    {
        float moveDistance = 5f;
        float moveSpeed = 2f + studentID % 10;

        Vector3[] axes = new Vector3[]
        {
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };
        Vector3 axis = axes[Random.Range(0, axes.Length)];

        Vector3 center = transform.position;
        Vector3 maxPos = center + axis * moveDistance;
        Vector3 minPos = center - axis * moveDistance;

        bool goingPositive = Random.Range(0, 2) == 0;

        Vector3 from = center;
        Vector3 to = goingPositive ? maxPos : minPos;

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(from, to);

        while (isObjectAMoving)
        {
            float distCovered = (Time.time - startTime) * moveSpeed;
            float t = distCovered / journeyLength;

            transform.position = Vector3.Lerp(from, to, t);

            if (t >= 1f)
            {
                if (to == maxPos)
                    to = minPos;
                else if (to == center)
                    to = goingPositive ? maxPos : minPos;
                else if (to == minPos)
                    to = maxPos;

                from = transform.position;
                startTime = Time.time;
                journeyLength = Vector3.Distance(from, to);
            }

            yield return null;
        }
    }

    void ChangeColorOnHoverObjectA()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
                isHoveringObjectA = true;
            else
                isHoveringObjectA = false;
        }
        else
            isHoveringObjectA = false;

        if (isHoveringObjectA)
            GetComponent<Renderer>().material.color = Color.yellow;
        else GetComponent<Renderer>().material.color = originalColor;
    }

    void ChangeColorOnAllObjectB()
    {
        GameObject[] ObjectB = GameObject.FindGameObjectsWithTag("ObjectB");
        foreach (GameObject obj in ObjectB)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f)
                );
            }
        }
    }

    void DestroyObjectBOnClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("ObjectB"))
            {
                Destroy(hit.transform.gameObject);
                points += 1;
            }
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 150;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 300, 60), "Points: " + points, style);
    }
}
