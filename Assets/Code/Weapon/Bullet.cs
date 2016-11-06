using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour 
{
	public Weapon ParentWeapon;
	public Vector3 Velocity;
	public float Range;
	public float TotalDamage;
	public float Attack;
	public float CriticalChance;
	public float Penetration;
	public Item AmmoItem;

	public AnimationCurve DamageDropCurve;

	private float _distTraveled;
	private float _destroyTimer;
	private bool _isDestroyed;
	private Rigidbody _rigidbody;

	void Update()
	{
		if(!_isDestroyed)
		{
			_distTraveled += Velocity.magnitude * Time.deltaTime;


			if(_distTraveled > 3)
			{
				GetComponent<TrailRenderer>().enabled = true;
			}

			if(_distTraveled > Range)
			{
				GameObject.Destroy(this.gameObject);
			}
		}
		else
		{
			if(!_rigidbody.isKinematic)
			{
				_rigidbody.velocity = Vector3.zero;
				GetComponent<BoxCollider>().enabled = false;
				_rigidbody.isKinematic = true;
			}

			if(_destroyTimer < 0.5f)
			{
				_destroyTimer += Time.deltaTime;
			}
			else
			{
				GameObject.Destroy(this.gameObject);
			}
		}
	}

	public void Fire(Vector3 velocity, Weapon parentWeapon, float range, float attackRating, float impact, float critical)
	{
		_distTraveled = 0;
		_destroyTimer = 0;
		_isDestroyed = false;

		Range = range;
		ParentWeapon = parentWeapon;
		Velocity = velocity;

		_rigidbody = GetComponent<Rigidbody>();
		_rigidbody.velocity = velocity;

		TotalDamage = impact + (float)AmmoItem.GetAttributeByName("Damage").Value;
		Attack = attackRating;
		Penetration = (float)AmmoItem.GetAttributeByName("Penetration").Value;

		CriticalChance = critical;

		GetComponent<TrailRenderer>().enabled = false;
	}

	void OnCollisionEnter(Collision collision) 
	{
		Character hitCharacter = collision.collider.GetComponent<Character>();
		if(hitCharacter == ParentWeapon.Attacker)
		{
			return;
		}

		if(hitCharacter != null && hitCharacter.Faction == ParentWeapon.Attacker.Faction)
		{
			return;
		}

		if(collision.collider.GetComponent<Bullet>() != null)
		{
			return;
		}


		Vector3 pos = collision.contacts[0].point;
		Vector3 normal = collision.contacts[0].normal;

		if(collision.collider.gameObject.isStatic)
		{
			GameObject hole = GameManager.Inst.FXManager.LoadFX("Bullet_Hole_Concrete", 30, FXType.BulletHole);
			//GameObject hole = GameObject.Instantiate(Resources.Load("Bullet_Hole_Concrete")) as GameObject;
			hole.transform.position = pos + normal * 0.02f;
			hole.transform.rotation = Quaternion.LookRotation(normal * -1);
		}

		if(hitCharacter != null)
		{
			//calculate damage based on attack rating. higher the attack rating more likely the damage is near totalDamage
			float x = UnityEngine.Random.value;
			float yMax = EvaluateDamageCurve(Attack * 10, 1);
			float y = EvaluateDamageCurve(Attack * 10, x);
			float damage = TotalDamage * (y / yMax);

			float distPercent = _distTraveled / Range;
			float distDamageDrop = DamageDropCurve.Evaluate(distPercent) * damage;

			float randCritical = UnityEngine.Random.value;
			if(randCritical <= CriticalChance)
			{
				hitCharacter.SendCriticalDamage(TotalDamage, Penetration, normal, ParentWeapon.Attacker, ParentWeapon);
			}
			else
			{
				hitCharacter.SendDamage(TotalDamage, Penetration, normal, ParentWeapon.Attacker, ParentWeapon);
			}

			GameObject impact = GameManager.Inst.FXManager.LoadFX("GunshotBlood" + UnityEngine.Random.Range(1, 4).ToString(), 1, FXType.BloodSpatter);
			Debug.DrawRay(pos, normal, new Color(0, 1, 0), 3);
			impact.transform.position = pos;
			impact.transform.rotation = Quaternion.LookRotation(normal);
			impact.transform.parent = collision.collider.transform;
			ParentWeapon.Attacker.OnSuccessfulHit(hitCharacter);

		}
		else
		{
			GameObject impact = GameManager.Inst.FXManager.LoadFX("WFX_BImpact SoftBody", 0, FXType.BulletImpact);
			impact.transform.position = pos;
			impact.transform.rotation = Quaternion.LookRotation(normal);
		}

		Rigidbody otherRB = collision.collider.GetComponent<Rigidbody>();

		if (otherRB) 
		{

			Vector3 force = normal * Velocity.magnitude * 1;
			otherRB.AddForceAtPosition(force, pos);


		}

		//_rigidbody.velocity = _rigidbody.velocity * 0.4f;
		GameObject.Destroy(this.gameObject);
	}

	private float EvaluateDamageCurve(float steepness, float x)
	{
		return -1 * Mathf.Exp(-1 * steepness * x) + 1;
	}
}
