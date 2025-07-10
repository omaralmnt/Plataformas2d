using UnityEngine;

public class SuperJumpZone : MonoBehaviour
{
    [Header("Super Jump Settings")]
    [SerializeField] private float jumpForce = 25f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("SUPER JUMP!");
            
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Reproducir sonido
                if (jumpSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(jumpSound);
                }
                
                // SUPER SALTO
                playerRb.velocity = new Vector2(playerRb.velocity.x, 0f);
                playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }
}