using System.Collections;
using System.IO;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AI;

namespace WalshScripts
{
    public class BossScript_MW : MonoBehaviour
    {
        #region Animator Parameter Hashes

        //Bool / Float Parameters
        static readonly int HashPlayerDetected = Animator.StringToHash("PlayerDetected");
        static readonly int HashBossGrounded = Animator.StringToHash("BossGrounded");
        static readonly int HashBossAttack = Animator.StringToHash("BossAttack");
        static readonly int HashBossDied = Animator.StringToHash("BossDied");
        static readonly int HashPlayerDistance = Animator.StringToHash("PlayerDistance");
        static readonly int HashBossMove = Animator.StringToHash("BossMove");

        //Trigger Parameters
        static readonly int HashNormalSwing = Animator.StringToHash("NormalSwing");
        static readonly int HashKick = Animator.StringToHash("Kick");
        static readonly int HashSpinSwing = Animator.StringToHash("SpinSwing");
        static readonly int HashShoot = Animator.StringToHash("Shoot");
        static readonly int HashSlamAttack = Animator.StringToHash("SlamAttack");
        static readonly int HashFastCombo = Animator.StringToHash("FastCombo");
        static readonly int HashSwingCombo = Animator.StringToHash("SwingCombo");
        static readonly int HashCombo2 = Animator.StringToHash("Combo2");

        #endregion


        #region Serialized References

        [Header("Script References")]
        [SerializeField] PlayerLogic _player;
        [SerializeField] Damageable _damageable;
        [SerializeField] Animator _anim;
        [SerializeField] NavMeshAgent _agent;

        [Header("Boss Health Threshold")]
        [Range(0.1f, 0.99f)]
        [SerializeField] float phase2Threshold = 0.66f;
        [Range(0.1f, 0.99f)]
        [SerializeField] float phase3Threshold = 0.33f;

        [Header("Boss Movement Variables")]
        [SerializeField] bool bossGrounded;
        [SerializeField] float bossSpeed;
        [SerializeField] float phase1Speed = 3.5f;
        [SerializeField] float phase2Speed = 4.5f;
        [SerializeField] float phase3Speed = 6.0f;

        [Header("Boss Melee Variables")]
        [SerializeField] float meleeRange = 4f;
        [SerializeField] float normalSwingDuration = 1.0f;
        [SerializeField] float kickDuration = 0.8f;
        [SerializeField] float slamDuration = 1.4f;
        [SerializeField] float spinDuration = 2.5f;
        [SerializeField] float shootDuration = 0.9f;
        [SerializeField] float fastComboDuration = 1.4f;
        [SerializeField] float swingComboDuration = 0.9f;
        [SerializeField] float combo2Duration = 0.9f;

        [Header("Boss Defense")]
        [SerializeField] float bigHitFractionForDefense = 0.2f;
        [SerializeField] float defensiveRange = 3f;
        [SerializeField] float defensiveCooldown = 5f;

        [Header("Boss Projectile Variables")]
        [SerializeField] GameObject stapleProjectilePrefab;
        [SerializeField] Transform stapleMuzzle;
        [SerializeField] float acknowledgeDistance = 30f;
        [SerializeField] float longProjectileRange = 25f;
        [SerializeField] float shortProjectileRange = 15f;
        [SerializeField] float hailstormWindup = 0.8f;
        [SerializeField] float hailstormInterval = 0.15f;
        [SerializeField] int hailstormShotCount = 6;
        [SerializeField] float hailstormSpreadAngle = 20f;

        [Header("Boss Shockwave Attack")]
        [SerializeField] GameObject shockwavePrefab;
        [SerializeField] Transform shockwaveSpawnPoint;

        [Header("Boss Misc Variables")]
        [SerializeField] bool bossDead;
        [SerializeField] float phase1AtkCooldown = 1.5f;
        [SerializeField] float phase2AtkCooldown = 1.2f;
        [SerializeField] float phase3AtkCooldown = 0.9f;

        [Header("Player Variables")]
        [SerializeField] bool playerDead;
        [SerializeField] float playerDistance;

        [Header("Debug")]
        [SerializeField] bool drawDebugGizmos;

        #endregion


        #region Boss Info

        BossState _state;
        Coroutine _attackRoutine;
        bool _hasSpottedPlayer;
        bool _isAttacking;
        bool _grounded = true;

        int _maxHealth;
        int _currentHealth;

        float _atkTimer;
        float _lastDefenseTime = -999f;

        public bool IsAttacking => _isAttacking;

        #endregion


        #region Unity Methods

        void Awake()
        {
            if (_player == null)
            {
                _player = FindAnyObjectByType<PlayerLogic>();
            }

            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }

            if (_anim == null)
            {
                _anim = GetComponentInChildren<Animator>();
            }

            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }

            if (_damageable != null)
            {
                _damageable.OnInitialize.AddListener(OnHealthInit);
                _damageable.OnHealthChanged.AddListener(OnHealthChanged);
                _damageable.OnDeath.AddListener(OnDeath);
            }
        }

        void Start()
        {
            SwitchState(new IdleState(this));
        }

        void Update()
        {
            if (_state == null)
            {
                return;
            }

            if (!_isAttacking)
            {
                _atkTimer -= Time.deltaTime;
            }

            _state.Tick();
            UpdateAnimatorParameters();

            if (!(_state is DeadState))
            {
                EvaluatePhaseSwitch();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, acknowledgeDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, shortProjectileRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, longProjectileRange);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, defensiveRange);
        }

        #endregion


        #region Health & Phase Logic

        void OnHealthInit(int max)
        {
            _maxHealth = max;
            _currentHealth = max;
        }

        void OnHealthChanged(int damageAmount, int newHealth)
        {
            _currentHealth = newHealth;

            if (!_hasSpottedPlayer)
            {
                _hasSpottedPlayer = true;
            }

            bool bigHit = _maxHealth > 0 && damageAmount >= Mathf.CeilToInt(_maxHealth * bigHitFractionForDefense);
            bool playerClose = DistanceToPlayer() <= defensiveRange;

            if (Time.time - _lastDefenseTime > defensiveCooldown && (bigHit || playerClose) && !(_state is DefensiveState) && !(_state is DeadState))
            {
                _lastDefenseTime = Time.time;
                SwitchState(new DefensiveState(this));
            }
        }

        void OnDeath()
        {
            SwitchState(new DeadState(this));

            if (GameManager.instance != null)
            {
                GameManager.instance.GoToNextLevel();
            }
        }

        float NormalizedHealth
        {
            get
            {
                if(_maxHealth <= 0)
                {
                    return 1f;
                }

                return Mathf.Clamp01((float) _currentHealth /  _maxHealth);
            }
        }

        void EvaluatePhaseSwitch()
        {
            if(!_hasSpottedPlayer)
            {
                return;
            }

            float hp = NormalizedHealth;

            BossState target = null;

            if(hp > phase2Threshold)
            {
                target = _state is Phase1State ? null : new Phase1State(this);
            }

            else if (hp > phase3Threshold)
            {
                target = _state is Phase2State ? null : new Phase2State(this);
            }

            else
            {
                target = _state is Phase3State ? null : new Phase3State(this);
            }

            if (target != null)
            {
                SwitchState(target);
            }
        }

        #endregion


        #region Animator Helpers

        void UpdateAnimatorParameters()
        {
            if (_anim == null)
            {
                return;
            }

            float dist = DistanceToPlayer();

            _anim.SetFloat(HashPlayerDistance, (HasPlayer ? dist : 0f));   
            
            float moveSpeed = (_agent != null) ? _agent.velocity.magnitude : 0f;

            _anim.SetFloat(HashBossMove, moveSpeed);

            _anim.SetBool(HashPlayerDetected, _hasSpottedPlayer);
            _anim.SetBool(HashBossGrounded, _grounded);
            _anim.SetBool(HashBossAttack, _isAttacking);
        }

        void Trigger(int hash)
        {
            if(_anim == null)
            {
                return;
            }
            _anim.SetTrigger(hash);
        }

        #endregion


        #region Movement & Helpers

        public bool HasPlayer => _player != null;
        public Vector3 PlayerPosition => HasPlayer ? _player.transform.position : transform.position;

        public float DistanceToPlayer()
        {
            if (!HasPlayer)
            {
                return Mathf.Infinity;
            }

            return Vector3.Distance(transform.position, PlayerPosition);
        }

        public bool PlayerInAcknowledgeRange()
        {
            return DistanceToPlayer() <= acknowledgeDistance;
        }

        public void LookAtPlayer()
        {
            if (!HasPlayer)
            {
                return;
            }

            Vector3 dir = PlayerPosition - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);

        }

        public void SetMoveSpeed(float speed)
        {
            if (_agent == null)
            {
                return;
            }

            _agent.speed = speed;
        }

        public void MoveTowardsPlayer()
        {
            if (_agent == null || !HasPlayer)
            {
                return;
            }

            _agent.isStopped = false;

            _agent.SetDestination(PlayerPosition);
        }

        public void StopMoving()
        {
            if(_agent == null)
            {
                return;
            }

            _agent.isStopped = true;

            _agent.ResetPath();
        }

        public void BeginAttack(IEnumerator routine, float phaseCooldown)
        {
            if (_isAttacking || routine == null)
            {
                return;
            }

            _isAttacking = true;

            StopMoving();

            _attackRoutine = StartCoroutine(AttackWrapper(routine, phaseCooldown));
        }

        IEnumerator AttackWrapper(IEnumerator routine, float phaseCooldown)
        {
            yield return routine;

            _isAttacking = false;

            _atkTimer = phaseCooldown;

            _attackRoutine = null;
        }

        void SwitchState(BossState newState)
        {
            if (_state == newState)
            {
                return; 
            }

            if(_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }

            _isAttacking = false;

            _atkTimer = 0f;

            _state?.Exit();

            _state = newState;

            _state?.Enter();
        }

        #endregion


        #region Attack Routines

        public IEnumerator NormalSwingRoutine()
        {
            LookAtPlayer();

            Trigger(HashNormalSwing);
            
            yield return new WaitForSeconds(normalSwingDuration);
        }

        public IEnumerator KickRoutine()
        {
            LookAtPlayer();
            Trigger(HashKick);
            yield return new WaitForSeconds(kickDuration);
        }

        public IEnumerator SlamRoutine()
        {
            LookAtPlayer();

            Trigger(HashSlamAttack);

            float half = slamDuration * 0.5f;

            yield return new WaitForSeconds(half);

            if (shockwavePrefab != null && shockwaveSpawnPoint != null)
            {
                Instantiate(shockwavePrefab, shockwaveSpawnPoint.position, shockwaveSpawnPoint.rotation);
            }

            float remaining = Mathf.Max(0f, slamDuration - half);

            yield return new WaitForSeconds(remaining);
        }

        public IEnumerator SpinRoutine()
        {
            LookAtPlayer();

            Trigger(HashSpinSwing);

            float elapsed = 0f;

            float duration = spinDuration;

            while (elapsed < duration)
            {
                MoveTowardsPlayer();

                elapsed += Time.deltaTime;

                yield return null;
            }

            StopMoving();
        }

        public IEnumerator ShootRoutine(bool longRange)
        {
            LookAtPlayer();

            Trigger(HashShoot);

            float half = shootDuration * 0.5f;

            yield return new WaitForSeconds(half);

            if (stapleProjectilePrefab != null && stapleMuzzle != null)
            {
                Vector3 dir = PlayerPosition - stapleMuzzle.position;

                dir.y = 0f;

                if (dir.sqrMagnitude < 0.01f)
                {
                    dir = transform.forward;
                }

                Quaternion rotation = Quaternion.LookRotation(dir.normalized);

                GameObject proj = Instantiate(stapleProjectilePrefab, stapleMuzzle.position, rotation);

                var rb = proj.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    float speed = longRange ? 35f : 25f;

                    rb.linearVelocity = rotation * Vector3.forward * speed;
                }
            }

            float remaining = Mathf.Max(0f, shootDuration - half);

            yield return new WaitForSeconds(remaining);
        }

        public IEnumerator FastComboRoutine()
        {
            LookAtPlayer();

            Trigger(HashFastCombo);

            yield return new WaitForSeconds(fastComboDuration);
        }

        public IEnumerator ComboChainRoutine()
        {
            LookAtPlayer();

            Trigger(HashSwingCombo);

            yield return new WaitForSeconds(swingComboDuration);

            LookAtPlayer();

            Trigger(HashCombo2);

            yield return new WaitForSeconds(combo2Duration);
        }

        IEnumerator HailstormStapleRoutine()
        {
            LookAtPlayer();

            Trigger(HashShoot);

            yield return new WaitForSeconds(hailstormWindup);

            if (stapleProjectilePrefab == null || stapleMuzzle == null)
            {
                yield return ShootRoutine(true);

                yield break;
            }

            Vector3 baseDir = PlayerPosition - stapleMuzzle.position;

            baseDir.y = 0f;

            if (baseDir.sqrMagnitude < 0.01f)
            {
                baseDir = transform.forward;
            }

            Quaternion baseRot = Quaternion.LookRotation(baseDir.normalized);

            int count = Mathf.Max(1, hailstormShotCount);

            for (int i = 0; i < count; i++)
            {
                float t = (count == 1) ? 0.5f : (float)i / (count - 1);

                float angle = Mathf.Lerp(-hailstormSpreadAngle, hailstormSpreadAngle, t);

                Quaternion rot = baseRot * Quaternion.Euler(0f, angle, 0f);

                GameObject proj = Instantiate(stapleProjectilePrefab, stapleMuzzle.position, rot);

                var rb = proj.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    float speed = 32f;

                    rb.linearVelocity = rot * Vector3.forward * speed;
                }

                yield return new WaitForSeconds(hailstormInterval);
            }

            yield return new WaitForSeconds(0.2f);
        }

        public IEnumerator DefensiveBraceRoutine()
        {
            LookAtPlayer();
            Trigger(HashKick);
            yield return new WaitForSeconds(kickDuration * 0.5f);

            yield return SlamRoutine();
        }

        #endregion


        #region Boss States

        abstract class BossState
        {
            protected readonly BossScript_MW boss;
            protected BossState(BossScript_MW boss) { this.boss = boss; }
            public virtual void Enter() { }
            public virtual void Tick() { }
            public virtual void Exit() { }
        }

        class IdleState : BossState
        {
            public IdleState(BossScript_MW boss) : base(boss) { }

            public override void Enter()
            {
                boss.StopMoving();
            }

            public override void Tick()
            {
                if (!boss.HasPlayer)
                {
                    return;
                }

                boss.LookAtPlayer();

                if(boss.PlayerInAcknowledgeRange())
                {
                    boss._hasSpottedPlayer = true;
                }
            }
        }

        class Phase1State : BossState
        {
            public Phase1State(BossScript_MW boss) : base(boss) { }

            public override void Enter()
            {
                boss.SetMoveSpeed(boss.phase1Speed);
                boss._atkTimer = 0.5f;
            }

            public override void Tick()
            {
                if (!boss.HasPlayer)
                {
                    return;
                }

                if(boss.IsAttacking)
                {
                    return;
                }

                float dist = boss.DistanceToPlayer();

                if (dist > boss.meleeRange)
                {
                    boss.MoveTowardsPlayer();
                }
                else
                {
                    boss.StopMoving();
                }

                boss.LookAtPlayer();

                if(boss._atkTimer > 0)
                {
                    return;
                }

                if(dist <= boss.meleeRange * 0.8f)
                {
                    float roll = Random.value;

                    if(roll < 0.5f)
                    {
                        boss.BeginAttack(boss.KickRoutine(), boss.phase1AtkCooldown);
                    }
                    else
                    {
                        boss.BeginAttack(boss.NormalSwingRoutine(), boss.phase1AtkCooldown);
                    }
                }
                else if (dist <= boss.meleeRange * 1.2f)
                {
                    boss.BeginAttack(boss.NormalSwingRoutine(), boss.phase1AtkCooldown);
                }
                else if (dist <= boss.shortProjectileRange)
                {
                    boss.BeginAttack(boss.ShootRoutine(false), boss.phase1AtkCooldown);
                }
                else
                {
                    boss._atkTimer = 0.25f;
                }
            }

            public override void Exit()
            {
                boss.StopMoving();
            }
        }

        class Phase2State : BossState
        {
            public Phase2State(BossScript_MW boss) : base(boss) { }

            public override void Enter()
            {
                boss.SetMoveSpeed(boss.phase2Speed);
                boss._atkTimer = 0.5f;
            }

            public override void Tick()
            {
                if(!boss.HasPlayer)
                {
                    return;
                }

                if(boss.IsAttacking)
                {
                    return;
                }

                float dist = boss.DistanceToPlayer();

                if(dist > boss.meleeRange)
                {
                    boss.MoveTowardsPlayer();
                }
                else
                {
                    boss.StopMoving();
                }

                boss.LookAtPlayer();

                if(boss._atkTimer > 0)
                {
                    return;
                }

                float roll = Random.value;

                if(dist <= boss.meleeRange * 1.2f)
                {
                    boss.BeginAttack(boss.ComboChainRoutine(), boss.phase2AtkCooldown);
                }
                else if(dist <= boss.shortProjectileRange *1.3f)
                {
                    if(roll < 0.5f)
                    {
                        boss.BeginAttack(boss.SpinRoutine(), boss.phase2AtkCooldown);
                    }
                    else
                    {
                        boss.BeginAttack(boss.ComboChainRoutine(), boss.phase2AtkCooldown);
                    }
                }
                else if(dist <= boss.longProjectileRange)
                {
                    boss.BeginAttack(boss.HailstormStapleRoutine(), boss.phase2AtkCooldown);
                }
                else
                {
                    boss._atkTimer = 0.25f;
                }
            }

            public override void Exit()
            {
                boss.StopMoving();
            }
        }

        class Phase3State : BossState
        {
            public Phase3State(BossScript_MW boss) : base(boss) { }

            public override void Enter()
            {
                boss.SetMoveSpeed(boss.phase3Speed * 1.25f);
                boss._atkTimer = 0.25f;
            }

            public override void Tick()
            {
                if(!boss.HasPlayer)
                {
                    return;
                }

                if(boss.IsAttacking)
                {
                    return;
                }

                float dist = boss.DistanceToPlayer();

                if(dist > boss.meleeRange * 0.9f)
                {
                    boss.MoveTowardsPlayer();
                }
                else
                {
                    boss.StopMoving();
                }

                boss.LookAtPlayer();

                if(boss._atkTimer > 0)
                {
                    return;
                }

                float roll = Random.value;

                if(dist > boss.longProjectileRange)
                {
                    boss.BeginAttack(boss.HailstormStapleRoutine(), boss.phase3AtkCooldown);
                }
                else if (dist > boss.meleeRange * 1.25f)
                {
                    if (roll < 0.5f)
                    {
                        boss.BeginAttack(boss.HailstormStapleRoutine(), boss.phase3AtkCooldown);
                    }
                    else
                    {
                        boss.BeginAttack(boss.SpinRoutine(), boss.phase3AtkCooldown);
                    }
                }
                else
                {
                    if (roll < 0.4f)
                    {
                        boss.BeginAttack(boss.ComboChainRoutine(), boss.phase3AtkCooldown);
                    }
                    else if(roll < 0.7f)
                    {
                        boss.BeginAttack(boss.SlamRoutine(), boss.phase3AtkCooldown);
                    }
                    else
                    {
                        boss.BeginAttack(boss.FastComboRoutine(), boss.phase3AtkCooldown);
                    }
                }

            }

            public override void Exit()
            {
                boss.StopMoving();
            }
        }

        class DefensiveState : BossState
        {
            public DefensiveState(BossScript_MW boss) : base(boss) { }

            public override void Enter()
            {
                boss.StopMoving();
                boss.BeginAttack(boss.DefensiveBraceRoutine(), boss.phase2AtkCooldown);
            }

            public override void Tick()
            {
                if(!boss.IsAttacking)
                {
                    boss.EvaluatePhaseSwitch();
                }
            }

            public override void Exit()
            {
                boss.StopMoving();
            }
        }

        class DeadState : BossState
        {
            public DeadState(BossScript_MW boss) : base(boss) { }

            public override void Enter()
            {
                boss.StopMoving();
            }
        }

        #endregion
    }
}
