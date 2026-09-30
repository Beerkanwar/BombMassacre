using UnityEngine;

public class DestructibleBlock : MonoBehaviour {
    [Header("Stats")]
    [SerializeField] private int health = 2;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite intactSprite;
    [SerializeField] private Sprite crackedSprite;
    
    [SerializeField] private Color crackedColorTint = new Color(0.7f, 0.7f, 0.7f, 1f);

    void Start() {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    public void TakeDamage(int damageAmount) {
        health -= damageAmount;

        if (health <= 0) {
            Destroy(gameObject);
        } else {
            UpdateVisuals();
        }
    }

    private void UpdateVisuals() {
        if (health == 1) {
            if (crackedSprite != null) {
                spriteRenderer.sprite = crackedSprite;
            } else {
                spriteRenderer.color = crackedColorTint;
            }
        } else if (health >= 2) {
            if (intactSprite != null) spriteRenderer.sprite = intactSprite;
            spriteRenderer.color = Color.white;
        }
    }

    [ContextMenu("Test Take 1 Damage")]
    private void DebugTakeDamage() {
        TakeDamage(1);
    }
}