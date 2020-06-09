using System;
using UnityEngine;

public class Throwable: Bolt.EntityEventListener<IBaseThrowable>
{
	ThrowableManager.ThrowableType m_type;
	
	public float m_damage			= 250.0f;
	public float m_knockbackForce	= 10.0f;
	public float m_maxVelocity		= 10.0f;
	public float m_timeoutDuration	= 10.0f;
	public int	 m_sourceIndex 		= -1;
	public float m_explosionSize	= 2;
	public bool  m_testCombination	= false;

	private SphereCollider	m_collider;
	private Rigidbody		m_rigidBody;
	private OVRGrabbable	m_grabbable;

	private bool			m_grabbed = false;
	private bool			m_thrown = false;
	private bool			m_combined = false;
	
	public override void Attached()
	{
		state.AddCallback( "Type", SetType );

		m_collider = gameObject.GetComponent<SphereCollider>();
		if (m_collider == null)
		{
			Debug.LogError("No collider attached to the Throwable prefab!");
		}
		
		m_grabbable = gameObject.GetComponent<OVRGrabbable>();
		m_rigidBody = gameObject.GetComponent<Rigidbody>();

	}
	
	public void SetType()
	{
		m_type = (ThrowableManager.ThrowableType) state.Type;
		// TODO Create appropriate Prefab for type
	}

	private void Update()
	{
		if (BoltNetwork.IsClient)
		{
			transform.position = state.Position;
			transform.rotation = state.Rotation;
		}
	}

	// Only Server owns this GameObject
	public override void SimulateOwner()
	{
		if ( m_grabbable.isGrabbed && !m_rigidBody.useGravity )
//		if ( transform.position.y > 3.0f )
		{
			m_rigidBody.useGravity = true;
			ThrowableManager throwMgr = Main.Instance.GetThrowableManager().GetComponent<ThrowableManager>();
			throwMgr.TakeThrowable( m_sourceIndex );
			m_grabbed = true;
		}
		
		if ( m_grabbed && !m_grabbable.isGrabbed )
		{
			// We were grabbed, but no longer, therefore we are thrown (or dropped, whatevs)
			m_grabbed = false;
			m_thrown = true;
		}
		
		if ( m_thrown )
		{
			m_timeoutDuration -= BoltNetwork.FrameDeltaTime;
			if ( m_timeoutDuration <= 0 )
			{
				// We've existed long enough after being throw. Destroy us
				BoltEntity boltEntity = gameObject.GetComponent<BoltEntity>();
				BoltNetwork.Destroy(boltEntity);
			}
		}

		state.Position = transform.position;
		state.Rotation = transform.rotation;
	}
	
	public void DebugSetThrown( bool value )
	{
		m_thrown = value;
	}

	private void OnCollisionEnter(Collision other)
	{
		// Combine Throwables!
		if ( (m_grabbed || m_testCombination)
		     && !m_combined && BoltNetwork.IsServer 
		     && other.collider.gameObject.layer == LayerMask.NameToLayer("VRProjectile") ) 
		{
			Throwable otherThrowable = other.gameObject.GetComponent<Throwable>();
			if ( otherThrowable != null && !otherThrowable.m_combined 
			    && (otherThrowable.m_grabbed || m_testCombination) )
			{
				m_combined = true;
				Vector3 newScale = transform.localScale * 2.0f;
				transform.localScale = newScale;
				m_collider.radius *= 2.0f;
				BoltEntity otherBoltEntity = otherThrowable.gameObject.GetComponent<BoltEntity>();
				BoltNetwork.Destroy( otherBoltEntity );
				return;
			}
		}
		
		var collided = other.collider.gameObject.layer == LayerMask.NameToLayer("MobilePlayer") 
		            || other.collider.gameObject.layer == LayerMask.NameToLayer("WorldObject");
		
		if (BoltNetwork.IsServer && collided)
		{
			var hitEntity = other.collider.gameObject.GetComponent<BoltEntity>();
			var damageEvent = DamageEvent.Create();
			damageEvent.Damage = (int)m_damage;
			damageEvent.HitEntity = hitEntity;
			damageEvent.Send();
		}

		// Sink Environment
		// TODO: Check Velocity and !m_Grabbed?
		if ( BoltNetwork.IsServer && m_combined 
		    && other.collider.gameObject.layer == LayerMask.NameToLayer("EnvSegment") )
		{
			CitySegment seg = other.gameObject.GetComponentInParent<CitySegment>();
			if ( seg != null )
			{
				Debug.LogWarning("Throwable - Sinking Segment " + seg.name );
				seg.SinkSegment();
				collided = true;
			}
		}
		
		if (collided)
		{
			GameObject hitFx = Resources.Load<GameObject>("VFX/Prefabs/vfx_explosion_pre");
			var vfx = GameObject.Instantiate(hitFx, other.GetContact(0).point, Quaternion.LookRotation(other.GetContact(0).normal));
			vfx.transform.localScale = Vector3.one * m_explosionSize * (m_combined ? 2.0f : 1.0f);
			vfx.AddComponent<VFXAutoDestroy>();
			
			// Destroy This Throwable
			BoltEntity boltEntity = gameObject.GetComponent<BoltEntity>();
			BoltNetwork.Destroy(boltEntity);
		}

	}
}