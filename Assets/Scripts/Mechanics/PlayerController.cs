using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics {
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 10;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/
        public Collider2D collider2d;
        public Collider2D bombCollider;
        /*internal new*/
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;
        private InputAction m_BombAction;

        [Header("Bomb Mechanics")]
        [SerializeField] private GameObject bombPrefab;
        public Transform bombCarryPosition;
        [SerializeField] private float throwSpeed = 12f;
        public GameObject currentBomb;
        private Rigidbody2D currentBombRb;
        private CircleCollider2D currentBombCollider;
        public Bounds Bounds => collider2d.bounds;

        //[System.Serializable]
        //public class Pool
        //{
        //    public string tag;
        //    public GameObject prefab;
        //    public int size;
        //}

        //public List<Pool> pools;
        //private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Queue<GameObject> bombPool;
        private int maxBombs = 3;

        void Awake() {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            m_BombAction = InputSystem.actions.FindAction("Player/Bomb");

            m_MoveAction?.Enable();
            m_JumpAction?.Enable();
            m_BombAction?.Enable();
            //poolDictionary = new Dictionary<string, Queue<GameObject>>();
            //foreach (Pool bomb in pools)
            //{
            //    Queue<GameObject> objectPool = new Queue<GameObject>();

            //    for (int i = 0; i < bomb.size; i++)
            //    {
            //        GameObject Bomb = Instantiate(bomb.prefab);
            //        Bomb.SetActive(false);
            //        objectPool.Enqueue(Bomb);
            //    }
            //    poolDictionary.Add(bomb.tag, objectPool);
            //}
            bombPool = new Queue<GameObject>();
            for (int i = 0; i < maxBombs; i++) {
                GameObject bomb = Instantiate(bombPrefab);
                bomb.SetActive(false);
                bombPool.Enqueue(bomb);
            }
        }

        protected override void Update() {
            if (controlEnabled) {
                move.x = m_MoveAction.ReadValue<Vector2>().x;
                if (jumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;
                else if (m_JumpAction.WasReleasedThisFrame()) {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }

                if (m_BombAction != null && m_BombAction.WasPressedThisFrame()) {
                    HandleBombInput();
                }

            } else {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }
        //private void HandleBombInput() {
        //    if (currentBomb == null) {
        //        // 1. Spawn the bomb
        //        if (bombPrefab == null || bombCarryPosition == null) return;
        //        if (poolDictionary.ContainsKey("Bomb"))

        //        currentBomb = poolDictionary["Bomb"].Dequeue();
        //        currentBomb.SetActive(true);
        //        currentBomb.transform.position = bombCarryPosition.position;
        //        currentBomb.transform.rotation = Quaternion.identity;
        //        currentBomb.transform.SetParent(bombCarryPosition);

        //        currentBombRb = currentBomb.GetComponent<Rigidbody2D>();
        //        currentBombCollider = currentBomb.GetComponent<CircleCollider2D>();
        //        if (currentBombRb != null) {
        //            currentBombRb.bodyType = RigidbodyType2D.Kinematic; // Disable physics while carrying
        //            currentBombRb.linearVelocity = Vector2.zero; // Unity 6 uses linearVelocity
        //        }

        //        if (currentBombCollider != null)
        //        {
        //            currentBombCollider.enabled = false;
        //        }
        //    } else {
        //        // 2. Throw the bomb
        //        currentBomb.transform.SetParent(null);

        //        if (currentBombRb != null) {
        //            currentBombRb.bodyType = RigidbodyType2D.Dynamic; // Re-enable physics

        //            // Calculate 45-degree throw direction based on facing direction
        //            float facingDirection = spriteRenderer.flipX ? -1f : 1f;
        //            Vector2 throwDirection = new Vector2(facingDirection, 1f).normalized;

        //            currentBombRb.linearVelocity = throwDirection * throwSpeed;
        //        }

        //        if (currentBombCollider != null)
        //        {
        //            currentBombCollider.enabled = true;
        //        }

        //        // Clear references so player can spawn a new one later
        //        currentBomb = null;
        //        currentBombRb = null;
        //        currentBombCollider = null;
        //    }
        //}
        private void HandleBombInput() {
            if (currentBomb == null) {
                // 1. Spawn the bomb (Only if we have bombs available in the pool)
                if (bombPrefab == null || bombCarryPosition == null || bombPool.Count == 0) return;

                currentBomb = bombPool.Dequeue();
                currentBomb.SetActive(true);
                currentBomb.transform.position = bombCarryPosition.position;
                currentBomb.transform.rotation = Quaternion.identity;
                currentBomb.transform.SetParent(bombCarryPosition);

                currentBombRb = currentBomb.GetComponent<Rigidbody2D>();
                currentBombCollider = currentBomb.GetComponent<CircleCollider2D>();

                if (currentBombRb != null) {
                    currentBombRb.bodyType = RigidbodyType2D.Kinematic;
                    currentBombRb.linearVelocity = Vector2.zero;
                }

                if (currentBombCollider != null) {
                    currentBombCollider.enabled = false;
                }
            } else {
                // 2. Throw the bomb
                currentBomb.transform.SetParent(null);

                if (currentBombRb != null) {
                    currentBombRb.bodyType = RigidbodyType2D.Dynamic;

                    float facingDirection = spriteRenderer.flipX ? -1f : 1f;
                    Vector2 throwDirection = new Vector2(facingDirection, 0.5f).normalized;

                    currentBombRb.linearVelocity = throwDirection * throwSpeed;
                }

                if (currentBombCollider != null) {
                    currentBombCollider.enabled = true;
                }

                currentBomb = null;
                currentBombRb = null;
                currentBombCollider = null;
            }
        }

        public void ReturnBombToPool(GameObject bomb) {
            bomb.SetActive(false);
            bombPool.Enqueue(bomb);
        }

        void UpdateJumpState() {
            jump = false;
            switch (jumpState) {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded) {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded) {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity() {
            if (jump && IsGrounded) {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            } else if (stopJump) {
                stopJump = false;
                if (velocity.y > 0) {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }



        
        public enum JumpState {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}