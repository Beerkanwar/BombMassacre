using System.Collections;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombHandler : MonoBehaviour {
    [SerializeField] private float bombDestroyAfter = 5f;
    [SerializeField] private float bombDestroyTimer = 0f;
    [SerializeField] private int maxHoldTime = 5;
    [SerializeField] private float maxHoldTimer = 0;
    private int nextSecond = 1;

    [Header("Explosion Settings")]
    [SerializeField] private float minExplosionRadius = 1f;
    [SerializeField] private float maxExplosionRadius = 3f;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private float explosionVisualDuration = 0.25f;
    [SerializeField] private Color explosionVisualColor = Color.red;
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private GameObject explosionVfxPrefab;
    //[SerializeField] private float damage = 1f;
    //[SerializeField] private float knockbackForce = 10f;

    [Header("Animation")]
    [SerializeField] private Sprite[] bombSprites;
    private SpriteRenderer spriteRenderer;

    void OnEnable() {
        bombDestroyTimer = 0f;
        maxHoldTimer = 0f;
        nextSecond = 1;
        explosionRadius = minExplosionRadius;
        explodeNextFrame = false;

        if (spriteRenderer != null && bombSprites != null && bombSprites.Length > 0) {
            spriteRenderer.sprite = bombSprites[0];
        }
    }

    private bool explodeNextFrame = false;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) {
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    void Update() {
        if (gameObject == null || playerController == null || playerController.bombCarryPosition == null) return;

        if (explodeNextFrame) {
            explodeNextFrame = false;
            Explode();
            return;
        }

        // If the bomb is NOT being held
        if (transform.parent != playerController.bombCarryPosition) {
            bombDestroyTimer += Time.deltaTime;
            if (bombDestroyTimer >= bombDestroyAfter) {
                Explode();
            }
        } else { // If the bomb IS being held
            maxHoldTimer += Time.deltaTime;

            if (bombSprites != null && bombSprites.Length > 0) {
                float frameDuration = maxHoldTime / (float)bombSprites.Length;
                int frameIndex = Mathf.Min(
                    Mathf.FloorToInt(maxHoldTimer / frameDuration),
                    bombSprites.Length - 1);

                spriteRenderer.sprite = bombSprites[frameIndex];
            }

            if (maxHoldTimer >= nextSecond && nextSecond <= maxHoldTime) {
                explosionRadius = Mathf.Min(explosionRadius + 1f, maxExplosionRadius);
                nextSecond += 1;
            } else if (maxHoldTimer >= maxHoldTime) {
                Explode();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        int hitLayer = collision.gameObject.layer;
        if (hitLayer == LayerMask.NameToLayer("Destructible") ||
            collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Enemy")) {
            explodeNextFrame = true;
        }
    }

    void Explode() {
        if (explosionVfxPrefab != null) {
            GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
            vfx.transform.localScale = new Vector3(explosionRadius, explosionRadius, 1f);
            Destroy(vfx, 2f);
        } else {
            CreateExplosionVisual();
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        bool playerWasHitByMask = false;

        foreach (Collider2D hit in hits) {
            if (hit.CompareTag("Player")) playerWasHitByMask = true;

            if (hit.TryGetComponent<Tilemap>(out Tilemap tilemap)) {
                DestroyTilesInRadius(tilemap, transform.position, explosionRadius);
            } else if (hit.TryGetComponent<DestructibleBlock>(out var destructibleBlock)) {
                destructibleBlock.TakeDamage(1);
            } else if (hit.TryGetComponent<Health>(out var health)) {
                //health.Decrement(); // Damages Enemies and Player
                health.Die();
            }
        }

        if (!playerWasHitByMask && playerController != null) {
            float distToPlayer = Vector2.Distance(transform.position, playerController.transform.position);
            if (distToPlayer <= explosionRadius) {
                if (playerController.TryGetComponent<Health>(out var playerHealth)) {
                    playerHealth.Decrement();
                }
            }
        }

        if (playerController != null) {
            playerController.ReturnBombToPool(gameObject);
        } else {
            gameObject.SetActive(false);
        }
    }

    private void DestroyTilesInRadius(Tilemap map, Vector2 center, float radius) {
        Vector3Int centerCell = map.WorldToCell(center);
        int cellRadius = Mathf.CeilToInt(radius);

        for (int x = -cellRadius; x <= cellRadius; x++) {
            for (int y = -cellRadius; y <= cellRadius; y++) {
                Vector3Int cellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);
                Vector3 cellWorldPos = map.GetCellCenterWorld(cellPos);

                if (Vector2.Distance(center, cellWorldPos) <= radius) {
                    map.SetTile(cellPos, null);
                }
            }
        }
    }

    private void CreateExplosionVisual() {
        GameObject visual = new GameObject("ExplosionVisual");
        visual.transform.position = transform.position;

        SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 100;
        spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));

        Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        Color[] colors = new Color[64 * 64];

        for (int y = 0; y < 64; y++) {
            for (int x = 0; x < 64; x++) {
                Vector2 sample = new Vector2((x + 0.5f) / 64f - 0.5f, (y + 0.5f) / 64f - 0.5f);
                bool inside = sample.magnitude <= 0.5f;
                colors[y * 64 + x] = inside ? explosionVisualColor : new Color(0f, 0f, 0f, 0f);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 32f);
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = explosionVisualColor;
        visual.transform.localScale = Vector3.one * explosionRadius;

        StartCoroutine(FadeExplosionVisual(spriteRenderer, visual));
        Destroy(visual, 1.0f);
    }

    private IEnumerator FadeExplosionVisual(SpriteRenderer spriteRenderer, GameObject visual) {
        float elapsed = 0f;
        Color startColor = explosionVisualColor;

        while (elapsed < explosionVisualDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / explosionVisualDuration);
            Color color = startColor;
            color.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = color;
            yield return null;
        }
    }
}
