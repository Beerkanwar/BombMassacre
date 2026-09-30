using UnityEngine;
using Platformer.Mechanics;

public class Bomb : MonoBehaviour {
    [Header("Fuse Settings")]
    [SerializeField] private float fuseDuration = 3f;
    private float fuseTimer;

    [Header("Explosion Settings")]
    [SerializeField] private float minRadius = 1.5f;
    [SerializeField] private float maxRadius = 4.0f;
    [SerializeField] private float playerKnockbackForce = 15f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color startColor = Color.black;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private GameObject explosionVisualPrefab;

    private bool hasExploded = false;

    void Start() {
        fuseTimer = fuseDuration;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (hasExploded) return;

        fuseTimer -= Time.deltaTime;

        float chargePercent = 1f - (fuseTimer / fuseDuration);

        Color currentColor = Color.Lerp(startColor, endColor, chargePercent);
        if (fuseTimer < 0.5f) {
            currentColor = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? Color.white : endColor;
        }
        spriteRenderer.color = currentColor;

        if (fuseTimer <= 0f) {
            Explode(chargePercent);
        }
    }

    private void Explode(float finalChargePercent) {
        hasExploded = true;

        float currentRadius = Mathf.Lerp(minRadius, maxRadius, finalChargePercent);

        if (explosionVisualPrefab != null) {
            GameObject visual = Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);
            visual.transform.localScale = new Vector3(currentRadius * 2, currentRadius * 2, 1f);
            Destroy(visual, 0.2f);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, currentRadius);

        int blockDamage = finalChargePercent > 0.5f ? 2 : 1;

        foreach (Collider2D hit in hitColliders) {
            if (hit.CompareTag("Player")) {
                HitPlayer(hit.gameObject);
            }

            DestructibleBlock block = hit.GetComponent<DestructibleBlock>();
            if (block != null) block.TakeDamage(blockDamage);
        }

        Destroy(gameObject);
    }

    private void HitPlayer(GameObject playerObj) {
        Health playerHealth = playerObj.GetComponent<Health>();
        if (playerHealth != null && playerHealth.IsAlive) {
            playerHealth.Decrement();
        }

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        if (playerController != null) {
            Vector2 knockbackDir = (playerObj.transform.position - transform.position).normalized;

            knockbackDir.y += 0.5f;
            knockbackDir = knockbackDir.normalized;

            playerController.velocity = knockbackDir * playerKnockbackForce;
            playerController.jumpState = PlayerController.JumpState.InFlight;
        }
    }
}