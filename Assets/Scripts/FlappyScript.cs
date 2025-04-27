using UnityEngine;
using System.Collections;

public class FlappyScript : MonoBehaviour
{
    public AudioClip FlyAudioClip, DeathAudioClip, ScoredAudioClip;
    public Sprite GetReadySprite;
    public float RotateUpSpeed = 1.5f, RotateDownSpeed = 2.5f;
    public GameObject IntroGUI, DeathGUI;
    public Collider2D restartButtonGameCollider;
    public float BaseVelocityPerJump = 3f;
    public float XSpeed = 1f;
    public float MaxStamina = 3; // Limit number of boosts per flight
    private float currentStamina;

    private FlappyYAxisTravelState flappyYAxisTravelState;
    private Vector3 birdRotation = Vector3.zero;
    private float speedIncrementTimer = 0f;

    enum FlappyYAxisTravelState
    {
        GoingUp, GoingDown
    }

    void Start()
    {
        currentStamina = MaxStamina;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (GameStateManager.GameState == GameState.Intro)
        {
            MoveBirdOnXAxis();
            if (WasTouchedOrClicked())
            {
                BoostOnYAxis();
                GameStateManager.GameState = GameState.Playing;
                IntroGUI.SetActive(false);
                ScoreManagerScript.Score = 0;
                currentStamina = MaxStamina;
            }
        }

        else if (GameStateManager.GameState == GameState.Playing)
        {
            MoveBirdOnXAxis();

            if (WasTouchedOrClicked() && currentStamina > 0)
            {
                BoostOnYAxis();
                currentStamina--;
            }
        }

        else if (GameStateManager.GameState == GameState.Dead)
        {
            Vector2 contactPoint = Vector2.zero;

            if (Input.touchCount > 0)
                contactPoint = Input.touches[0].position;
            if (Input.GetMouseButtonDown(0))
                contactPoint = Input.mousePosition;

            if (restartButtonGameCollider == Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(contactPoint)))
            {
                GameStateManager.GameState = GameState.Intro;
                Application.LoadLevel(Application.loadedLevelName);
            }
        }
    }

    void FixedUpdate()
    {
        if (GameStateManager.GameState == GameState.Intro)
        {
            if (GetComponent<Rigidbody2D>().velocity.y < -1)
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, GetComponent<Rigidbody2D>().mass * 6000 * Time.deltaTime));
        }
        else if (GameStateManager.GameState == GameState.Playing || GameStateManager.GameState == GameState.Dead)
        {
            FixFlappyRotation();
            IncreaseSpeedOverTime();

            // Recover stamina slowly while falling
            if (GetComponent<Rigidbody2D>().velocity.y < -1 && currentStamina < MaxStamina)
                currentStamina += Time.deltaTime;
        }
    }

    bool WasTouchedOrClicked()
    {
        return Input.GetMouseButtonDown(0) || 
               Input.GetKeyDown(KeyCode.Space) || 
               (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began);
    }

    void MoveBirdOnXAxis()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0, 0);
    }

    void BoostOnYAxis()
    {
        float randomizedBoost = BaseVelocityPerJump + Random.Range(-0.2f, 0.3f);
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, randomizedBoost);
        GetComponent<AudioSource>().PlayOneShot(FlyAudioClip);
    }

    private void FixFlappyRotation()
    {
        float verticalVelocity = GetComponent<Rigidbody2D>().velocity.y;

        flappyYAxisTravelState = verticalVelocity > 0 ? 
            FlappyYAxisTravelState.GoingUp : 
            FlappyYAxisTravelState.GoingDown;

        float degreesToAdd = 0;

        switch (flappyYAxisTravelState)
        {
            case FlappyYAxisTravelState.GoingUp:
                degreesToAdd = 7 * RotateUpSpeed;
                break;
            case FlappyYAxisTravelState.GoingDown:
                degreesToAdd = -5 * RotateDownSpeed * Mathf.Clamp01(-verticalVelocity / 3f); // more downward velocity = faster tilt
                break;
        }

        birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z + degreesToAdd, -90, 45));
        transform.eulerAngles = birdRotation;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.CompareTag("Pipeblank"))
            {
                GetComponent<AudioSource>().PlayOneShot(ScoredAudioClip);
                ScoreManagerScript.Score++;
                currentStamina = Mathf.Min(MaxStamina, currentStamina + 1); // Regain stamina on score
            }
            else if (col.gameObject.CompareTag("Pipe"))
            {
                FlappyDies();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState == GameState.Playing && col.gameObject.CompareTag("Floor"))
        {
            FlappyDies();
        }
    }

    void FlappyDies()
    {
        GameStateManager.GameState = GameState.Dead;
        DeathGUI.SetActive(true);
        GetComponent<AudioSource>().PlayOneShot(DeathAudioClip);
    }

    void IncreaseSpeedOverTime()
    {
        speedIncrementTimer += Time.deltaTime;

        if (speedIncrementTimer >= 10f)
        {
            XSpeed += 0.1f;
            speedIncrementTimer = 0f;
        }
    }
}
