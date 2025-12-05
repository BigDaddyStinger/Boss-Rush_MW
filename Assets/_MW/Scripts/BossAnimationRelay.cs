using UnityEngine;


namespace WalshScripts
{
    public class BossAnimationRelay : MonoBehaviour
    {
        private BossScript_MW _boss;

        void Awake()
        {
            _boss = GetComponentInParent<BossScript_MW>();
        }

        public void EnableMeleeHitbox() => _boss?.EnableMeleeHitbox();
        public void DisableMeleeHitbox() => _boss?.DisableMeleeHitbox();
        public void EnableShockwaveHitbox() => _boss?.EnableShockwaveHitbox();
        public void DisableShockwaveHitbox() => _boss?.DisableShockwaveHitbox();
        public void InstantiateProjectile() => _boss?.InstantiateProjectile();
    }
}
