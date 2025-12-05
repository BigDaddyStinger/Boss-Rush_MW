using UnityEngine;

namespace WalshScripts
{
    public class BossPhaseVisuals : MonoBehaviour
    {
        [SerializeField] private ParticleSystem auraParticles;

        [Header("Emission rates")]
        [SerializeField] private float phase1Rate = 0f;   // no glow
        [SerializeField] private float phase2Rate = 30f;  // light glow
        [SerializeField] private float phase3Rate = 80f;  // strong glow

        void SetAuraRate(float rate)
        {
            if (auraParticles == null) return;

            var emission = auraParticles.emission;
            emission.rateOverTime = rate;
        }

        public void SetPhase1()
        {
            SetAuraRate(phase1Rate);
        }

        public void SetPhase2()
        {
            SetAuraRate(phase2Rate);
        }

        public void SetPhase3()
        {
            SetAuraRate(phase3Rate);
        }
    }
}
