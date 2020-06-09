using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : Bolt.EntityEventListener<IBreakableObject>
{
    public const int MAX_HEALTH = 10;
    public const float RESPAWN_TIME = 5.0f;

    protected float respawnTimer;
    protected bool isDead;
    protected Vector3 cachedPos;
    
    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        
        if (entity.IsOwner)
        {
            state.Health = MAX_HEALTH;
        }
        state.AddCallback("Health", HealthChanged);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        isDead = false;
    }

    public override void SimulateOwner()
    {
        OVRGrabbable grabbable = gameObject.GetComponent<OVRGrabbable>();
        if ( grabbable != null && grabbable.isGrabbed )
        {
            Rigidbody rb = gameObject.GetComponentInChildren<Rigidbody>();
            if ( rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }
            
            Collider c = gameObject.GetComponentInChildren<Collider>();
            if ( c != null )
            {
                c.isTrigger = false;
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (respawnTimer > 0 && isDead)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0.0f)
            {
                Respawn();
            }
        }
    }

    public void Respawn()
    {
        if (entity.IsControllerOrOwner)
        {
            isDead = false;
            entity.transform.position = cachedPos;
            respawnTimer = 0;
            state.Health = MAX_HEALTH;
        }
    }
    
    public void OnHit(int damage)
    {
        if (entity.IsControllerOrOwner)
        {
            var health = state.Health;
            health -= damage;
            if (health < 0)
            {
                health = 0;
            }

            state.Health = health;
            if (health == 0)
            {
                isDead = true;
                cachedPos = gameObject.transform.position;
                entity.transform.position = cachedPos + new Vector3(0, -100, 0);
                // start respawn timer
                respawnTimer = RESPAWN_TIME;
            }
        }
    }
    
    protected void HealthChanged()
    {
        
    }
}
