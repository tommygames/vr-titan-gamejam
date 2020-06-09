using UnityEngine;

public class PlayerBullet  : Bolt.EntityEventListener<IPlayerBasicBullet>
{
    public int damage;
    public float speed;
    public Vector3 autoTarget;

    private float timer;
    private float deathDelay = 0;
    
    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
    }

    public void Init(Vector3 target, int dmg, float spd)
    {
        autoTarget = target;
        damage = dmg;
        speed = spd;
//        var dir = autoTarget - entity.transform.position;
//        entity.transform.rotation = Quaternion.LookRotation(dir);
    }

    void Start()
    {
        GameObject hitFx = Resources.Load<GameObject>("VFX/Prefabs/vfx_muzzleFlash_01_pre");
        var vfx = GameObject.Instantiate(hitFx, transform.position, Quaternion.LookRotation(-transform.position));
        vfx.AddComponent<VFXAutoDestroy>();
    }

    void Update()
    {
        if (entity.IsControllerOrOwner)
        {
            if (deathDelay > 0)
            {
                deathDelay -= Time.deltaTime;
                if (deathDelay <= 0)
                {
                    BoltNetwork.Destroy(entity);
                }
            }
            else
            {
                timer += Time.deltaTime;
                entity.transform.position += entity.transform.forward * Time.deltaTime * speed;

                if (timer > 5.0f)
                {
                    BoltNetwork.Destroy(entity);
                }
            }
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        var collideWithVRPlayer = other.collider.gameObject.layer == LayerMask.NameToLayer("VRPlayer") ||
                                  other.collider.gameObject.layer == LayerMask.NameToLayer("WorldObject");
        var collideWithOther = other.collider.gameObject.layer == LayerMask.NameToLayer("VRProjectile");
        if (BoltNetwork.IsServer && collideWithVRPlayer)
        {
            var hitEntity = other.collider.gameObject.GetComponent<BoltEntity>();
            var damageEvent = DamageEvent.Create();
            damageEvent.Damage = damage;
            damageEvent.HitEntity = hitEntity;
            damageEvent.Send();
        }

        if (collideWithVRPlayer && GameStateManager.Instance.CanPlayVFX())
        {
            GameObject hitFx = Resources.Load<GameObject>("VFX/Prefabs/vfx_explosion_pre");
            var vfx = GameObject.Instantiate(hitFx, other.GetContact(0).point, Quaternion.LookRotation(other.GetContact(0).normal));
            vfx.AddComponent<VFXAutoDestroy>();
        }
        
        if (entity.IsControllerOrOwner && (collideWithVRPlayer || collideWithOther))
        {
            deathDelay = 0.25f;
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }
}