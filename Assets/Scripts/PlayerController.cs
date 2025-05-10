using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")] // Declaración de variables para salto, velocidad y correr
    public float jumpForce = 10f;
    public float moveSpeed = 2f;
    public float runSpeed = 5f;
    public float runDuration = 3f;
    public float runCooldown = 30f;

    [Header("UI")] // Elementos visuales: texto de cuenta regresiva e instrucciones
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI instructionText;

    [Header("Físico")] // Material que evita el rebote del jugador al caer
    public PhysicsMaterial2D noBounceMaterial;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private CanvasGroup countdownCanvas;
    private RectTransform countdownRect;

    private bool isGrounded;
    private bool jumpRequested;
    private bool runReady = true;
    private bool isRunning;

    private static bool countdownShown;

    void Start()
    {
        // Inicialización de componentes físicos y visuales
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        // Asignación de material antirrebote
        // Mostrar instrucciones breves al jugador
        // Preparación para cuenta regresiva de inicio
        if (countdownText != null)
        {
            countdownCanvas = countdownText.GetComponent<CanvasGroup>();
            countdownRect = countdownText.GetComponent<RectTransform>();
        }

        if (noBounceMaterial != null)
            GetComponent<Collider2D>().sharedMaterial = noBounceMaterial;

        if (cooldownText != null) cooldownText.text = "";
        if (instructionText != null)
        {
            instructionText.text = "Usa ← → para moverte\n↑ o ESPACIO para saltar";
            instructionText.gameObject.SetActive(true);
            Invoke(nameof(HideInstructions), 5f);
        }

        sprite.enabled = false;
        rb.gravityScale = 0f;

        if (!countdownShown)
        {
            countdownShown = true;
            StartCoroutine(StartCountdown());
        }
        else
        {
            sprite.enabled = true;
            rb.gravityScale = 3f;
            if (countdownText != null) countdownText.gameObject.SetActive(false);
        }
    }

    void HideInstructions()
    {
        if (instructionText != null)
            instructionText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Detecta si el jugador quiere saltar
        // Activa el sprint si se presiona Shift
        // Muestra el estado del sprint en pantalla
        if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)) && isGrounded)
            jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.LeftShift) && runReady)
            StartCoroutine(ActivateRun());

        if (runReady && !isRunning && cooldownText != null)
            cooldownText.text = "Presiona SHIFT para más velocidad";
    }

    void FixedUpdate()
    {
        // Movimiento horizontal y salto según entrada del jugador
        float speed = isRunning ? runSpeed : moveSpeed;
        float dirInput = Input.GetKey(KeyCode.RightArrow) ? 1f :
                         Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f;
        rb.linearVelocity = new Vector2(dirInput * speed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica si el jugador toca el suelo para permitir salto
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ObstacleSide"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator ActivateRun()
    {
        // Activa el sprint por tiempo limitado
        // Inicia cuenta regresiva de recarga antes de volver a correr
        runReady = false;
        isRunning = true;
        if (cooldownText != null) cooldownText.text = "";

        yield return new WaitForSeconds(runDuration);

        isRunning = false;
        float timer = runCooldown;
        while (timer > 0f)
        {
            if (cooldownText != null)
                cooldownText.text = $"Recargando: {timer:F0}s";
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        runReady = true;
        if (cooldownText != null)
            cooldownText.text = "Presiona SHIFT para más velocidad";
    }

    IEnumerator StartCountdown()
    {
        // Muestra cuenta regresiva de 3, 2, 1... ¡Vamos!
        // Activa sprite y gravedad después del conteo
        if (countdownText == null) yield break;

        yield return ShowAndWait("3");
        yield return ShowAndWait("2");
        yield return ShowAndWait("1");
        yield return ShowAndWait("¡Vamos!");

        countdownText.gameObject.SetActive(false);
        sprite.enabled = true;
        yield return new WaitForSeconds(0.1f);
        rb.gravityScale = 3f;
    }

    IEnumerator ShowAndWait(string msg)
    {
        // Anima visualmente cada número de la cuenta regresiva
        countdownText.text = msg;
        countdownText.gameObject.SetActive(true);
        countdownCanvas.alpha = 1f;
        countdownText.transform.localScale = Vector3.zero;

        float duration = 0.3f, t = 0f;
        while (t < duration)
        {
            float s = Mathf.Lerp(0f, 1.2f, t / duration);
            countdownText.transform.localScale = Vector3.one * s;
            t += Time.deltaTime;
            yield return null;
        }
        countdownText.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(0.7f);
    }
}
