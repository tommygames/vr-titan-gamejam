using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class VRPlayer : Bolt.EntityEventListener<IVRPlayer>
{
    public const int MAX_HEALTH = 3000;
    public System.Action<int, int> OnHealthChanged;
    public System.Action OnDeath;
    
    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        if (entity.IsOwner)
        {
            state.Health = MAX_HEALTH;
        }

        state.AddCallback("Health", HealthChanged);
    
        BoltManager.Instance.RegisterVREntity(entity);
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
            if (entity.isOwner && health == 0)
            {
                var gameOverEvent = GameOverEvent.Create();
                gameOverEvent.WinType = "VRDead";
                gameOverEvent.Send();
                // game over!
                if (OnDeath != null)
                {
                    OnDeath();
                }
            }
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