// (c) Copyright HutongGames, LLC 2010-2011. All rights reserved.
// Thanks to: Giyomu
// http://hutonggames.com/playmakerforum/index.php?topic=400.0

using UnityEngine;
using HutongGames.PlayMaker;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Material)]
[Tooltip("Get a material at index on a gameObject and store it in a variable")]
public class GetMaterial : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(Renderer))]
	public FsmOwnerDefault gameObject;
	public FsmInt materialIndex;
	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmMaterial material;
	[Tooltip("Get the shared material of this object. NOTE: Modifying the shared material will change the appearance of all objects using this material, and change material settings that are stored in the project too.")]
	public bool getSharedMaterial;

	public override void Reset()
	{
		gameObject = null;
		material = null;
		materialIndex = 0;
		getSharedMaterial = false;
	}

	public override void OnEnter ()
	{
		DoGetMaterial();
		Finish();
	}
	
	void DoGetMaterial()
	{
		var go = Fsm.GetOwnerDefaultTarget(gameObject);
		if (go == null)
		{
			return;
		}

		if (go.GetComponent<Renderer>() == null)
		{
			LogError("Missing Renderer!");
			return;
		}
		
		if (materialIndex.Value == 0 && !getSharedMaterial)
		{
			material.Value = go.GetComponent<Renderer>().material;
		}
		
		else if(materialIndex.Value == 0 && getSharedMaterial)
		{
			material.Value = go.GetComponent<Renderer>().sharedMaterial;
		}
	
		else if (go.GetComponent<Renderer>().materials.Length > materialIndex.Value && !getSharedMaterial)
		{
			var materials = go.GetComponent<Renderer>().materials;
			material.Value = materials[materialIndex.Value];
			go.GetComponent<Renderer>().materials = materials;
		}

		else if (go.GetComponent<Renderer>().materials.Length > materialIndex.Value && getSharedMaterial)
		{
			var materials = go.GetComponent<Renderer>().sharedMaterials;
			material.Value = materials[materialIndex.Value];
			go.GetComponent<Renderer>().sharedMaterials = materials;
		}
	}
}
