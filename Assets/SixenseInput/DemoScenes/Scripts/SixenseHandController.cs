
//
// Copyright (C) 2013 Sixense Entertainment Inc.
// All Rights Reserved
//

using UnityEngine;
using System.Collections;

public class SixenseHandController : SixenseObjectController
{
	protected Animator			m_animator = null;
	protected float				m_fLastTriggerVal = 0.0f;
	public PickupZone					m_pickupZone;
	public GameObject			m_handItemSpawn;
	HandTool					m_tool;
	int							m_toolNum;
	bool						m_toolPressed;
	bool						m_isActivating;
	
	protected override void Start() 
	{
		// get the Animator
		m_animator = this.gameObject.GetComponent<Animator>();
		m_toolNum = 0;
		m_toolPressed = false;
		m_isActivating = false;

		if (Hand == SixenseHands.RIGHT)
		{
			NextTool();
		}
		
		base.Start();
	}
	
	protected override void UpdateObject( SixenseInput.Controller controller )
	{
		if ( m_animator == null )
		{
			return;
		}
		
		if ( controller.Enabled )  
		{		
			// Animation update
			UpdateAnimationInput( controller );
		}
				
		base.UpdateObject(controller);
	}
	
	
	void OnGUI()
	{
		if ( Hand == SixenseHands.UNKNOWN )
		{
			return;
		}
		
		if ( !m_enabled )
		{
			int labelWidth = 250;
			int labelPadding = 120;
			int horizOffset = Hand == SixenseHands.LEFT ? -labelWidth - labelPadding  : labelPadding;
			
			string handStr = Hand == SixenseHands.LEFT ? "left" : "right";
			GUI.Box( new Rect( Screen.width / 2 + horizOffset, Screen.height - 40, labelWidth, 30 ),  "Press " + handStr + " START to control " + gameObject.name );		
		}		
	}
	
	// Updates the animated object from controller input.
	protected void UpdateAnimationInput( SixenseInput.Controller controller)
	{
		// Tools can only be used by the right hand
		if (Hand == SixenseHands.RIGHT)
		{
			if ( Hand == SixenseHands.RIGHT ? controller.GetButton(SixenseButtons.ONE) : controller.GetButton(SixenseButtons.TWO) )
			{
				if(!m_toolPressed) {
					NextTool();
					Debug.Log ("What");
				}

				m_toolPressed = true;
				m_animator.SetBool( "Point", true );
			}
			else
			{
				m_toolPressed = false;
				m_animator.SetBool( "Point", false );
			}
		}
		else
		{
			// Grip Ball
			if ( Hand == SixenseHands.RIGHT ? controller.GetButton(SixenseButtons.TWO) : controller.GetButton(SixenseButtons.ONE)  )
			{
				m_animator.SetBool( "GripBall", true );
			}
			else
			{
				m_animator.SetBool( "GripBall", false );
			}
					
			// Hold Book
			if ( Hand == SixenseHands.RIGHT ? controller.GetButton(SixenseButtons.THREE) : controller.GetButton(SixenseButtons.FOUR) )
			{
				m_animator.SetBool( "HoldBook", true );
			}
			else
			{
				m_animator.SetBool( "HoldBook", false );
			}
		}
				
		// Fist
		float fTriggerVal = controller.Trigger;
		if (fTriggerVal > 0.01f) {
			if (Hand == SixenseHands.RIGHT)
			{
				if(m_tool && !m_isActivating) {
					m_tool.Activate();
					m_isActivating = true;
				} 
			}
			else 
			{
				m_pickupZone.Grab();
			}
		} 
		else 
		{
			if (Hand == SixenseHands.RIGHT)
			{
				m_isActivating = false;
			}
			else
			{
				m_pickupZone.Drop();
			}
		}

		fTriggerVal = Mathf.Lerp( m_fLastTriggerVal, fTriggerVal, 0.1f );
		m_fLastTriggerVal = fTriggerVal;
		
		if ( fTriggerVal > 0.01f )
		{
			m_animator.SetBool( "Fist", true );
		}
		else
		{
			m_animator.SetBool( "Fist", false );
		}
		
		m_animator.SetFloat("FistAmount", fTriggerVal);
		
		// Idle
		if ( m_animator.GetBool("Fist") == false &&  
			 m_animator.GetBool("HoldBook") == false && 
			 m_animator.GetBool("GripBall") == false && 
			 m_animator.GetBool("Point") == false )
		{
			m_animator.SetBool("Idle", true);
		}
		else
		{
			m_animator.SetBool("Idle", false);
		}
	}

	public void NextTool() {
		if (m_toolNum >= HandTool.GetNumTools ()) {
			m_toolNum = 0;
		}
		if (m_tool) {
			Destroy (m_tool.gameObject);
		}
		m_tool = HandTool.GetTool(m_toolNum);
		if(m_tool) {
			m_tool.transform.position = m_handItemSpawn.transform.position;
			m_tool.transform.parent = m_handItemSpawn.transform;
		}
		m_toolNum++;
	}
}

