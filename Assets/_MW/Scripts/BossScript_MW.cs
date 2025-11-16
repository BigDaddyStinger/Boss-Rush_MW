using UnityEditor.Build;
using UnityEngine;

namespace WalshScripts
{
    public class BossScript_MW : MonoBehaviour
    {
        [Header("Script References")]
            [SerializeField] PlayerLogic _pLogic;

        [Header("Boss Movement Variables")]
            [SerializeField] bool bossGrounded;
            [SerializeField] float bossSpeed;
            

        [Header("Boss Attack Variables")]
            [SerializeField] bool bossAttacking = false;
            [SerializeField] bool playerWithinAttackDistance = false;
            [SerializeField] float playerAcknowledgementDistance = 100f;
            [SerializeField] float bossLongProjectileDistance = 70f;
            [SerializeField] float bossShortProjectileDistance = 40f;
            [SerializeField] float bossPersuitDistance = 50f;
            [SerializeField] float bossAttackDistance = 5f;

        [Header("Boss Misc Variables")]
            [SerializeField] bool bossDead;

        [Header("Player Variables")]
            [SerializeField] bool playerDead;
            [SerializeField] float playerDistance;

        private void Awake()
        {
            _pLogic = _pLogic.GetComponent<PlayerLogic>();
        }





        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}
