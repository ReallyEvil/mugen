using UnityEngine;
using System.Collections;

public class Knight : SmoothMoves.TextureFunctionMonoBehaviour {
	
	private float _blinkTimeLeft;
	
	public SmoothMoves.BoneAnimation knight;
	public AudioSource swishSound;
	public AudioSource hitSound;
	
	public float minBlinkTime;
	public float maxBlinkTime;
	
	public Material oldMaterial;
	public Material newMaterial;
	
	public float speed;
	
	public Transform knightShadow;
	
	public SmoothMoves.BoneAnimation sparks;
	
	// Use this for initialization
	void Start () {
		
		knight.SwapMaterial(oldMaterial, newMaterial);
		
		knight.RegisterColliderTriggerDelegate(SwordHit);
		knight.RegisterUserTriggerDelegate(SwordSwish);
		
		knight.ReplaceBoneTexture("Weapon", textureSearchReplaceList[0]);
		knight.ReplaceBoneTexture("Weapon", textureSearchReplaceList[1]);
		
		knightShadow.parent = knight.GetBoneTransform("Root");
		knightShadow.localPosition = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			knight["Walk"].speed = 1.0f;
			knight.CrossFade("Walk");
		}
		if (Input.GetKeyUp(KeyCode.RightArrow))
		{
			knight.CrossFade("Stand");
		}
		
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			knight["Walk"].speed = -1.0f;
			knight.CrossFade("Walk");
		}
		if (Input.GetKeyUp(KeyCode.LeftArrow))
		{
			knight.CrossFade("Stand");
		}
		
		if (Input.GetKey(KeyCode.RightArrow))
		{
			knight.mLocalTransform.position += new Vector3(speed * Time.deltaTime, 0, 0);
		}

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			knight.mLocalTransform.position -= new Vector3(speed * Time.deltaTime, 0, 0);
		}
	
		if (Input.GetKeyDown(KeyCode.A))
		{
			knight.CrossFade("Attack");
		}
		
		_blinkTimeLeft -= Time.deltaTime;
		if (_blinkTimeLeft <= 0)
		{
			knight.Play("Blink");
			
			_blinkTimeLeft = UnityEngine.Random.Range(minBlinkTime, maxBlinkTime);
		}
	}
	
	public void SwordHit(SmoothMoves.ColliderTriggerEvent triggerEvent)
	{
		if (triggerEvent.boneName == "Weapon" && triggerEvent.triggerType == SmoothMoves.ColliderTriggerEvent.TRIGGER_TYPE.Enter)
		{
			hitSound.Play();
			
			sparks.mLocalTransform.position = triggerEvent.otherColliderClosestPointToBone;
			sparks.Play("Hit");
		}
	}
	
	public void SwordSwish(SmoothMoves.UserTriggerEvent triggerEvent)
	{
		if (triggerEvent.boneName == "Weapon")
		{
			swishSound.Play();
		}
	}
}
