using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip wallJumpSound;
    
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float wallJumpCooldown;
    private float horizontalInput;
    
    // Para detectar aterrizaje
    private bool wasGrounded;
    private bool isCurrentlyGrounded;
    
    private void Awake()
    {
        //Grab references for rigidbody and animator from object
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Inicializar estado de suelo
        wasGrounded = isGrounded();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        
        // Verificar estado del suelo para detectar aterrizaje
        isCurrentlyGrounded = isGrounded();
        
        // Detectar aterrizaje (no estaba en suelo y ahora sí)
        if (!wasGrounded && isCurrentlyGrounded)
        {
            OnLanded();
        }
        
        // Actualizar estado anterior
        wasGrounded = isCurrentlyGrounded;

        //Flip player when moving left-right
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        //Set animator parameters
        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", isCurrentlyGrounded);

        //Wall jump logic
        if (wallJumpCooldown > 0.2f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            if (onWall() && !isCurrentlyGrounded)
            {
                body.gravityScale = 0;
                body.velocity = Vector2.zero;
            }
            else
                body.gravityScale = 7;

            if (Input.GetKey(KeyCode.Space))
                Jump();
        }
        else
            wallJumpCooldown += Time.deltaTime;
    }

    private void Jump()
    {
        if (isCurrentlyGrounded)
        {
            // Salto normal
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            anim.SetTrigger("jump");
            
            // Reproducir sonido de salto
            if (jumpSound != null)
            {
                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            }
        }
        else if (onWall() && !isCurrentlyGrounded)
        {
            // Wall jump
            if (horizontalInput == 0)
            {
                body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 10, 0);
                transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
                body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 3, 6);

            wallJumpCooldown = 0;
            
            // Reproducir sonido de wall jump (o usar el mismo de salto)
            if (wallJumpSound != null)
            {
                AudioSource.PlayClipAtPoint(wallJumpSound, transform.position);
            }
            else if (jumpSound != null)
            {
                // Si no hay sonido específico de wall jump, usar el de salto normal
                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            }
        }
    }
    
    private void OnLanded()
    {
        // Método llamado cuando el player aterriza
        if (landSound != null)
        {
            AudioSource.PlayClipAtPoint(landSound, transform.position);
        }
        
        Debug.Log("Player landed!");
    }

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    
    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
    
    public bool canAttack()
    {
        return horizontalInput == 0 && isCurrentlyGrounded && !onWall();
    }
}