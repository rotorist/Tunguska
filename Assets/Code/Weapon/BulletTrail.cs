using UnityEngine;
using System.Collections;

public class BulletTrail : MonoBehaviour 
{
	public Weapon ParentWeapon;
	public ParticleSystem Bullets;
	public ParticleCollisionEvent[] CollisionEvents;

	void Start() 
	{
		Bullets = GetComponent<ParticleSystem>();
		CollisionEvents = new ParticleCollisionEvent[16];
	}

	void OnParticleCollision(GameObject other) 
	{
		int safeLength = Bullets.GetSafeCollisionEventSize();
		if (CollisionEvents.Length < safeLength)
		{
			CollisionEvents = new ParticleCollisionEvent[safeLength];
		}

		int numCollisionEvents = Bullets.GetCollisionEvents(other, CollisionEvents);
		Rigidbody rb = other.GetComponent<Rigidbody>();
		int i = 0;
		while (i < numCollisionEvents) 
		{
			Vector3 pos = CollisionEvents[i].intersection;
			Vector3 normal = CollisionEvents[i].normal;

			if(other.isStatic)
			{
				GameObject hole = GameManager.Inst.FXManager.LoadFX("Bullet_Hole_Concrete", 30, FXType.BulletHole);
				//GameObject hole = GameObject.Instantiate(Resources.Load("Bullet_Hole_Concrete")) as GameObject;
				hole.transform.position = pos + normal * 0.02f;
				hole.transform.rotation = Quaternion.LookRotation(normal * -1);
			}
			Character hitCharacter = other.GetComponent<Character>();
			if(hitCharacter != null)
			{
				if(UnityEngine.Random.Range(0, 100) > 30)
				{
					float damage = UnityEngine.Random.Range(10f, 20f);
					hitCharacter.SendDamage(damage, 0, normal, ParentWeapon.Attacker, ParentWeapon);
					GameObject impact = GameManager.Inst.FXManager.LoadFX("WFX_BloodSmoke", 0, FXType.BulletImpact);
					impact.transform.position = pos;
					impact.transform.rotation = Quaternion.LookRotation(this.transform.forward);
					ParentWeapon.Attacker.OnSuccessfulHit(hitCharacter);
				}
				else
				{
					//GameManager.Inst.UIManager.BarkPanel.AddFloater(hitCharacter);
				}
			}
			else
			{
				GameObject impact = GameManager.Inst.FXManager.LoadFX("WFX_BImpact SoftBody", 0, FXType.BulletImpact);
				impact.transform.position = pos;
				impact.transform.rotation = Quaternion.LookRotation(normal);
			}

			if (rb) 
			{

				Vector3 force = CollisionEvents[i].velocity * 1;
				rb.AddForceAtPosition(force, pos);


			}
			i++;
		}
	}
}
