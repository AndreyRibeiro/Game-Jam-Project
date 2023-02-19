using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;

    private bool canDash;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    private bool canDoubleJump;
    private bool doubleJump;

    public Animator animator;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource dashSound;
    [SerializeField] private AudioSource powerupSound;


    private void Start() {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        //Animações do personagem
        if (Input.GetAxis("Horizontal") != 0)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            jumpSound.Play();
            animator.SetBool(("isJumping"), true);
        }
        else
        {
            animator.SetBool(("isJumping"), false);
        }
        if (isDashing)
        {
            animator.SetBool(("isDashing"), true);
            return;
        }
        else
        {
            animator.SetBool(("isDashing"), false);
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        //Movimentação e PowerUps do Personagem
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            dashSound.Play();
            StartCoroutine(Dash());
        }

        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }
        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || doubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

                if (!canDoubleJump)
                {
                    return;
                }
                doubleJump = !doubleJump;
            }
        }
        Flip();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }


    private IEnumerator Dash()
    {
        //Código pra fazer o dash funcionar
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Fazer com que os PowerUps funcionem quando forem acionados
        if (collision.CompareTag("DoubleJump"))
        {
            powerupSound.Play();
            canDoubleJump = true;
        }
        if (collision.CompareTag("Dash"))
        {
            powerupSound.Play();
            canDash = true;
        }
        if (collision.CompareTag("GameOver"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
