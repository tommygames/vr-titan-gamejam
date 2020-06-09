using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class Player : Bolt.EntityEventListener<IMobilePlayer>
{
    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        if (entity.IsOwner)
        {
            state.Health = MAX_HEALTH;
        }
        BoltManager.Instance.RegisterEntity(entity);
        state.AddCallback("Health", HealthChanged);
        state.AddCallback( "State", SetStateCallback);
    }

    public const float gravity = 8.5f;
    
    public const int MAX_HEALTH = 1000;
    public const float RESPAWN_TIME = 5.0f;
    public System.Action<int, int> OnHealthChanged;
    public System.Action<float> OnDeath;
    public float playerMoveSpeed = 1.5f;

    
    protected Vector3 _velocity = Vector3.zero;
    protected Vector3 _rotation;
    protected CharacterController _character;
    protected State _state;
    protected bool isMoving;

    protected Item currentWeapon;
    protected Item currentPowerUp;
    protected float fireDelay;

    protected bool isDead;
    protected float respawnDelay;

	protected Animator _animator;
    
    protected enum State
    {
        Shoot,
        Run,
        Fall
    };

    // Use this for initialization
    void Start()
    {
        currentWeapon = Main.Instance.GetItemManager().GetItem(ItemsManager.Items.Pistol);
        currentPowerUp = null;
        fireDelay = 0.0f;
        _character = gameObject.GetComponent<CharacterController>();
        _animator = gameObject.GetComponentInChildren<Animator>();
//        ChangeState(State.Fall);
    }

    void ChangeState(State newState)
    {
        switch (newState)
        {
            case State.Shoot:
                _velocity.y = -gravity;
                break;
            case State.Run:
                _velocity.y = -gravity;
                break;
            case State.Fall:
                _velocity.y = 0.0f;
                break;
        }

        _state = newState;
        state.State = (int)_state;
    }
    
    void SetStateCallback()
    {
        switch ((State)state.State )
        {
            case State.Shoot:
                _animator.SetBool("Shoot", true );
                _animator.SetBool("Run", false );
                break;
            case State.Run:
                _animator.SetBool("Shoot", false );
                _animator.SetBool("Run", true );
                break;
            case State.Fall:
//                _animator.SetBool("Death", true );
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (entity.IsControllerOrOwner && GameStateManager.Instance != null && GameStateManager.Instance.GetGameState() == GameStateManager.GameState.Running)
        {
            if (entity.transform.position.y < 0.9f)
            {
                SetDead();
            }
            
            Vector3 dir = new Vector3();
            if (isDead)
            {
                respawnDelay -= Time.deltaTime;
                if (respawnDelay <= 0.0f)
                {
                    Respawn();
                }

                return;
            }

#if UNITY_IOS
            var virtualStick = TouchManager.Instance.virtualStick;
            var virtualButton = TouchManager.Instance.virtualButton;
            dir.x = virtualStick.stickDir.x;
            dir.z = virtualStick.stickDir.y;
#else
            if (Input.GetKey(KeyCode.W))
            {
                dir.z = 1.0f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                dir.x = -1.0f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir.z = -1.0f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir.x = 1.0f;
            } 
#endif
            
            var forward = _character.transform.position;
            forward = -forward;
            forward.Normalize();
            dir = Quaternion.LookRotation(forward, Vector3.up) * dir;

            isMoving = !dir.Equals(Vector3.zero);
            
            switch (_state)
            {
                case State.Shoot:
                    if ( isMoving )
                    {
                        ChangeState(State.Run);
                    }
                    else
                    {
                        Vector3 lookDir = new Vector3(0,transform.position.y, 0) - transform.position; 
                        Quaternion rot = Quaternion.LookRotation( lookDir, Vector3.up);
                        transform.rotation = Quaternion.SlerpUnclamped( transform.rotation, rot, 0.2f);
                    }
                    break;
                case State.Run:
                    if (_character.isGrounded == false)
                    {
                        ChangeState(State.Fall);
                    }
                    else if ( CanFire() )
                    {
                        ChangeState(State.Shoot);
                    }
                    else
                    {
                        Quaternion rot = Quaternion.LookRotation( dir, Vector3.up);
                        transform.rotation = Quaternion.SlerpUnclamped( transform.rotation, rot, 0.2f);
//                        Quaternion rot = Quaternion.LookRotation( dir, Vector3.up);
//                        Debug.DrawLine( transform.position, rot * Vector3.forward * 2.5f);
                    }
                    break;
                case State.Fall:
                    _velocity.y -= gravity * Time.deltaTime;
                    if (_character.isGrounded == true)
                    {
                        ChangeState(State.Run);
                    }
                    break;
            }
            
            dir *= GetMoveSpeed();
            dir.y = _velocity.y;
            _character.Move(dir * Time.deltaTime);
            entity.transform.position = _character.transform.position;
            
            // auto fire with current weapon
            if (CanFire())
            {
                fireDelay -= Time.deltaTime;
                if (fireDelay <= 0.0f)
                {
                    fireDelay = GetFireRate();
                    currentWeapon.UseItem();
                }
            }
            else
            {
                fireDelay = GetFireRate();

            }
        }
    }

    public bool CanFire()
    {
        return !isMoving && !isDead && currentWeapon != null;
    }

    private float GetMoveSpeed()
    {
        return playerMoveSpeed;
    }

    private float GetFireRate()
    {
        if (currentWeapon != null)
        {
            return currentWeapon.fireRate;
        }

        return 0;
    }

//    private int GetWeaponDamage()
//    {
//        return itemArray[(int) currentWeapon].damage;
//    }
//    
//    private float GetWeaponProjectileSpeed()
//    {
//        return itemArray[(int) currentWeapon].speed;
//    }

    public Item GetCurrentWeapon()
    {
        return currentWeapon;
    }
    
    public Item GetCurrentPowerUp()
    {
        return currentPowerUp;
    }

    public void SetCurrentWeapon(Item weapon)
    {
        var itemEvent = CreateItemEvent.Create();
        itemEvent.Entity = entity;
        itemEvent.Item = Main.Instance.GetItemManager().GetEnumForItem(currentWeapon);
        itemEvent.Send();
        
        currentWeapon = weapon;
    }
    
    public void SetCurrentPowerUp(Item powerup)
    {
        if (currentPowerUp != null && powerup != null)
        {
            var itemEvent = CreateItemEvent.Create();
            itemEvent.Entity = entity;
            itemEvent.Item = Main.Instance.GetItemManager().GetEnumForItem(currentPowerUp);
            itemEvent.Send();
        }
        currentPowerUp = powerup;
    }
    
    public void UseCurrentPowerUp()
    {
        if (currentPowerUp != null)
        {
            currentPowerUp.UseItem();
            currentPowerUp.DecrementUses();

            if (currentPowerUp.IsOutOfUses())
            {
                var itemEvent = ItemChanged.Create();
                itemEvent.Entity = entity;
                itemEvent.Item = (int) ItemsManager.Items.None;
                itemEvent.Send();
            }
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
                SetDead();
            }
        }
    }

    public void SetDead()
    {
        // start respawn timer
        respawnDelay = RESPAWN_TIME;
        isDead = true;
        entity.transform.position = entity.transform.position + new Vector3(0, 100, 0);
        _animator.SetBool("Death", true );
        
        if (entity.isOwner && OnDeath != null)
        {
            OnDeath(respawnDelay);
        }
    }

    public void Respawn()
    {
        if (entity.IsControllerOrOwner)
        {
            state.Health = MAX_HEALTH;
            isDead = false;
            var spawn = BoltManager.Instance.GetSpawnLocation(2.5f);
            spawn.y = 1.08f;
            _velocity = Vector3.zero;
            _character.enabled = false;
            entity.transform.position = spawn;
            _character.transform.position = spawn;
            _character.enabled = true;
            respawnDelay = 0;
            _animator.SetBool("Death", false );
            _animator.SetBool("Shoot", false );
            _animator.SetBool("Run", false );
        }
    }
    
    protected void HealthChanged()
    {
        if (OnHealthChanged != null)
        {
            OnHealthChanged(state.Health, MAX_HEALTH);
        }
    }

}
