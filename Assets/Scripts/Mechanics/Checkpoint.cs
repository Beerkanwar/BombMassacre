using UnityEngine;
using Platformer.Core;
using Platformer.Model;

public class Checkpoint : MonoBehaviour {
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private ParticleSystem activationParticles;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activateSound;

    private bool isActivated = false;

    void Start() {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (spriteRenderer != null && inactiveSprite != null) {
            spriteRenderer.sprite = inactiveSprite;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (isActivated) return;

        if (other.CompareTag("Player")) {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint() {
        isActivated = true;

        var model = Simulation.GetModel<PlatformerModel>();
        if (model != null) {
            model.spawnPoint = this.transform;
        }

        if (spriteRenderer != null && activeSprite != null) {
            spriteRenderer.sprite = activeSprite;
        }

        if (audioSource != null && activateSound != null) {
            audioSource.PlayOneShot(activateSound);
        }

        if (activationParticles != null) activationParticles.Play();
    }
}