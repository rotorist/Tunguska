using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnterableBuilding : MonoBehaviour 
{
	public List<BuildingComponent> Components;

	private bool _revealAll;
	private float _revealTimer;
	private bool _active;

	public void NotifyHidingComponent(BuildingComponent component, float playerY)
	{
		if(component != null && playerY < component.YMin && !component.IsHidden)
		{
			HideComponent(component);


		}

		//reveal or hide other components
		foreach(BuildingComponent c in Components)
		{
			//Debug.Log("Checking building revealing component " + c.name);
			if(playerY > c.YMin && c.IsHidden)
			{
				
				RevealComponent(c);
			}
			else if(playerY < c.YMin && !c.IsHidden)
			{
				HideComponent(c);
			}
				
		}

		_revealTimer = 0;
		_active = true;
	}

	void Update()
	{
		//run reveal timer
		if(_active)
		{
			if(_revealTimer > 1)
			{
				RevealAll();
			}

			_revealTimer += Time.deltaTime;
		}
	}


	private void HideComponent(BuildingComponent component)
	{
		MeshRenderer renderer = component.GetComponent<MeshRenderer>();
		if(renderer != null)
		{
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
			component.gameObject.layer = LayerMask.NameToLayer("IgnorePlayerRaycast");
			component.IsHidden = true;
		}

		Transform [] objects1 = component.transform.GetComponentsInChildren<Transform>();
		foreach(Transform t in objects1)
		{
			renderer = t.GetComponent<MeshRenderer>();
			if(renderer != null && renderer.gameObject != component.gameObject)
			{
				//renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
				renderer.gameObject.layer = LayerMask.NameToLayer("HiddenObjects");
			}

			Transform [] objects2  = t.transform.GetComponentsInChildren<Transform>();
			foreach(Transform t2 in objects2)
			{
				renderer = t2.GetComponent<MeshRenderer>();
				if(renderer != null && renderer.gameObject != component.gameObject)
				{
					//renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
					renderer.gameObject.layer = LayerMask.NameToLayer("HiddenObjects");
				}
			}
		}
	}

	private void RevealComponent(BuildingComponent component)
	{
		MeshRenderer renderer = component.GetComponent<MeshRenderer>();
		if(renderer != null)
		{
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			component.gameObject.layer = LayerMask.NameToLayer("BuildingComponent");
			component.IsHidden = false;
		}

		Transform [] objects1 = component.transform.GetComponentsInChildren<Transform>();
		foreach(Transform t in objects1)
		{
			renderer = t.GetComponent<MeshRenderer>();
			if(renderer != null && renderer.gameObject != component.gameObject)
			{
				//renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
				renderer.gameObject.layer = LayerMask.NameToLayer("Default");
			}

			Transform [] objects2  = t.transform.GetComponentsInChildren<Transform>();
			foreach(Transform t2 in objects2)
			{
				renderer = t2.GetComponent<MeshRenderer>();
				if(renderer != null && renderer.gameObject != component.gameObject)
				{
					//renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
					renderer.gameObject.layer = LayerMask.NameToLayer("Default");
				}
			}
		}
	}

	private void RevealAll()
	{
		foreach(BuildingComponent c in Components)
		{
			RevealComponent(c);
		}
	}


}
