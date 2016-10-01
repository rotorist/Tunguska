using UnityEngine;
using System.Collections;

public class HandGrenade : ThrownObject
{
	public Explosive Explosive;
	public float FuseTimer;
	
	// Update is called once per frame
	void Update () 
	{
		if(Explosive.IsEnabled)
		{
			FuseTimer -= Time.deltaTime;
			if(FuseTimer <= 0)
			{
				Explosive.TriggerExplosion();
			}

			GetComponent<TrailRenderer>().time = 1.5f;
		}
	}
}
