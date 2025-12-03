using UnityEngine;

namespace WalshScripts
{
    public class BossPlayerDamager : MonoBehaviour
    {
        [SerializeField] int damageAmount = 10;
        [SerializeField] float knockbackForce = 0f;

        private void OnTriggerEnter(Collider other)
        {
            // Look for a Damageable on this object OR its parents (works with Player/Cube setup)
            Damageable dmg = other.GetComponentInParent<Damageable>();
            if (dmg == null)
                return;

            // Direction from this hitbox toward the thing we hit
            Vector3 dir = (other.transform.position - transform.position).normalized;

            Damage damage = new Damage();
            damage.amount = damageAmount;
            damage.direction = dir;
            damage.knockbackForce = knockbackForce;

            dmg.Hit(damage);
        }
    }
}
