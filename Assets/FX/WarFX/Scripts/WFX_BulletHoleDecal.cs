using UnityEngine;
using System.Collections;

/**
 *	Handles the bullet hole decals:
 *	- Sets a random frame
 *	- Fades the material according to the defined lifetime
 *	- Optionally rotates the decal
 *	
 *	(c) 2012, Jean Moreno
**/

[RequireComponent(typeof(MeshFilter))]
public class WFX_BulletHoleDecal : MonoBehaviour
{
	static private Vector2[] quadUVs = new Vector2[]{new Vector2(0,0), new Vector2(0,1), new Vector2(1,0), new Vector2(1,1)};

	public Vector2 frames;
	public bool randomRotation = false;
	
	private float life;
	private float fadeout;
	private Color color;
	private float orgAlpha;
	
	void Awake()
	{
		color = this.GetComponent<Renderer>().material.GetColor("_TintColor");
		orgAlpha = color.a;
	}
	
	void OnEnable()
	{
		//Random UVs
		int random = Random.Range(0, (int)(frames.x*frames.y));
		int fx = (int)(random%frames.x);
		int fy = (int)(random/frames.y);
		//Set new UVs
		Vector2[] meshUvs = new Vector2[4];
		for(int i = 0; i < 4; i++)
		{
			meshUvs[i].x = (quadUVs[i].x + fx) * (1.0f/frames.x);
			meshUvs[i].y = (quadUVs[i].y + fy) * (1.0f/frames.y);
		}
		this.GetComponent<MeshFilter>().mesh.uv = meshUvs;
		
		//Random rotate
		if(randomRotation)
			this.transform.Rotate(0f,0f,Random.Range(0f,360f), Space.Self);

		fadeout = 5;
		color.a = orgAlpha;
		this.GetComponent<Renderer>().material.SetColor("_TintColor", color);

	}

	public void StartFadeOut()
	{
		life = fadeout;
		StopAllCoroutines();
		StartCoroutine("holeUpdate");
	}
	
	IEnumerator holeUpdate()
	{
		while(life > 0f)
		{
			life -= Time.deltaTime;
			if(life <= fadeout)
			{
				color.a = Mathf.Lerp(0f, orgAlpha, life/fadeout);
				this.GetComponent<Renderer>().material.SetColor("_TintColor", color);
				if(life <= 0)
				{
					GameObject.Destroy(this.gameObject);
				}
			}
			
			yield return null;
		}
	}
}
