// (c) Copyright HutongGames, LLC 2010-2011. All rights reserved.

using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Physics)]
	[Tooltip("Tests if a Game Object's Rigid Body is sleeping.")]
	public class IsSleeping : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody))]
		public FsmOwnerDefault gameObject;
		
		public FsmEvent trueEvent;
		
		public FsmEvent falseEvent;
		
		[UIHint(UIHint.Variable)]
		public FsmBool store;
		
		public bool everyFrame;
		
		public override void Reset()
		{
			gameObject = null;
			trueEvent = null;
			falseEvent = null;
			store = null;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			DoIsSleeping();
			
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoIsSleeping();
		}

		void DoIsSleeping()
		{
			var go = Fsm.GetOwnerDefaultTarget(gameObject);

			if (go == null || go.GetComponent<Rigidbody>() == null)
			{
				return;
			}
			
			var isSleeping = go.GetComponent<Rigidbody>().IsSleeping();
			store.Value = isSleeping;

			Fsm.Event(isSleeping ? trueEvent : falseEvent);
		}
	}
}

