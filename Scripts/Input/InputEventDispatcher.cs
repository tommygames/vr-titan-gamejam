using UnityEngine;
using System.Collections;

/// <summary>
/// Common input events our game modes care about presented in a way that is useful
/// </summary>
public class InputEventDispatcher 
{
	// Tapping
	public delegate void TapEventDelegate(Vector2 pos);
	public event TapEventDelegate OnTap;

	public delegate void PressEventDelegate(Vector2 pos);
	public event PressEventDelegate OnPress;

	public delegate void LongPressHintEventDelegate( Vector2 pos, float normalizedDuration );
	public event LongPressHintEventDelegate OnLongPressHint;

	public delegate void LongPressEventDelegate(Vector2 pos);
	public event LongPressEventDelegate OnLongPress;

	public delegate void ReleaseEventDelegate(Vector2 pos);
	public event ReleaseEventDelegate OnRelease;


	// Single Press Dragging
	public delegate void DragStartEventDelegate(Vector2 startPos, Vector2 curPos);
	public event DragStartEventDelegate OnDragStart;

	public delegate void DragEventDelegate(Vector2 prevPos, Vector2 curPos);
	public event DragEventDelegate OnDrag;

	public delegate void DragEndEventDelegate(Vector2 prevPos, Vector2 endPos);
	public event DragEndEventDelegate OnDragEnd;


	// Two Tapping
	public delegate void TwoTapEventDelegate(Vector2 p1, Vector2 p2);
	public event TwoTapEventDelegate OnTwoTap;

	public delegate void TwoPressEventDelegate(Vector2 p1, Vector2 p2);
	public event TwoPressEventDelegate OnTwoPress;

	public delegate void TwoReleaseEventDelegate(Vector2 p1, Vector2 p2);
	public event TwoReleaseEventDelegate OnTwoRelease;


	// Two Press Dragging - useful for detecting Pinch and Twist
	public delegate void TwoDragStartEventDelegate(Vector2 startP1, Vector2 curP1, Vector2 startP2, Vector2 curP2);
	public event TwoDragStartEventDelegate OnTwoDragStart;

	public delegate void TwoDragEventDelegate(Vector2 prevP1, Vector2 curP1, Vector2 prevP2, Vector2 curP2);
	public event TwoDragEventDelegate OnTwoDrag;

	public delegate void TwoDragEndEventDelegate(Vector2 prevP1, Vector2 endP1, Vector2 prevP2, Vector2 endP2);
	public event TwoDragEndEventDelegate OnTwoDragEnd;


	// Mouse Wheel
	public delegate void MouseWheelEventDelegate(float delta, Vector2 pos);
	public event MouseWheelEventDelegate OnMouseWheel;

	public void Tap(Vector2 pos)
	{
		if (OnTap != null)
		{
			OnTap(pos);
		}
	}

	public void Press(Vector2 pos)
	{
		if (OnPress != null)
		{
			OnPress(pos);
		}
	}

	public void LongPressHint( Vector2 pos, float normalizedDuration )
	{
		if ( OnLongPressHint != null )
		{
			OnLongPressHint( pos, normalizedDuration );
		}
	}

	public void LongPress(Vector2 pos)
	{
		if (OnLongPress != null)
		{
			OnLongPress(pos);
		}
	}

	public void Release(Vector2 pos)
	{
		if (OnRelease != null)
		{
			OnRelease(pos);
		}
	}

	public void DragStart(Vector2 startPos, Vector2 curPos)
	{
		if (OnDragStart != null)
		{
			OnDragStart(startPos, curPos);
		}
	}

	public void Drag(Vector2 prevPos, Vector2 curPos)
	{
		if (OnDrag != null)
		{
			OnDrag(prevPos, curPos);
		}
	}

	public void DragEnd(Vector2 prevPos, Vector2 endPos)
	{
		if (OnDragEnd != null)
		{
			OnDragEnd(prevPos, endPos);
		}
	}

	public void TwoPress(Vector2 p1, Vector2 p2)
	{
		if (OnTwoPress != null)
		{
			OnTwoPress(p1, p2);
		}
	}

	public void TwoRelease(Vector2 p1, Vector2 p2)
	{
		if (OnTwoRelease != null)
		{
			OnTwoRelease(p1, p2);
		}
	}

	public void TwoTap(Vector2 p1, Vector2 p2)
	{
		if (OnTwoTap != null)
		{
			OnTwoTap(p1, p2);
		}
	}

	// Two Press Dragging - useful for detecting Pinch and Twist
	public void TwoDragStart(Vector2 startP1, Vector2 curP1, Vector2 startP2, Vector2 curP2)
	{
		if (OnTwoDragStart != null)
		{
			OnTwoDragStart(startP1, curP1, startP2, curP2);
		}
	}

	public void TwoDrag(Vector2 prevP1, Vector2 curP1, Vector2 prevP2, Vector2 curP2)
	{
		if (OnTwoDrag != null)
		{
			OnTwoDrag(prevP1, curP1, prevP2, curP2);
		}
	}

	public void TwoDragEnd(Vector2 prevP1, Vector2 endP1, Vector2 prevP2, Vector2 endP2)
	{
		if (OnTwoDragEnd != null)
		{
			OnTwoDragEnd(prevP1, endP1, prevP2, endP2);
		}
	}

	public void MouseWheel(float delta, Vector2 pos)
	{
		if (OnMouseWheel != null)
		{
			OnMouseWheel(delta, pos);
		}
	}
}
