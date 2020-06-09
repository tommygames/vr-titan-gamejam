using UnityEngine;
using System.Collections;

/// <summary>
/// KLA Raw Input Manager. Dispatches gesture events based on interpreted raw input.
/// </summary>
public class InputManager : MonoBehaviour 
{
	public float ScrollWheelZoomScale = 1f;

	[Range (0.001f, 1000f)]
	public float TapSqrThreshold = 1f;

	[Range (0.001f, 1000f)]
	public float DragSqrThreshold = 1f;

	public InputEventDispatcher EventDispatcher { get { return m_dispatcher; } internal set { m_dispatcher = value; } }
	public static InputManager Instance { get; private set; }

	private const float LONG_PRESS_THRESH = 0.5f; // seconds, how long it takes to dispatch a long press event
	private float m_longPressDuration = 0f; // seconds, how long we have been pressed

	private float m_longPressHintThreshold = -1.0f;
	private float m_longPressThreshold = -1.0f;

	private InputEventDispatcher m_dispatcher = new InputEventDispatcher();

	private string m_pinchAxis = "Mouse ScrollWheel";
//	private PinchEvent m_pinchEvent = new PinchEvent();
	private DragEvent m_dragEvent = new DragEvent();
	private TapEvent m_tapEvent = new TapEvent();
	private PressEvent m_pressEvent = new PressEvent();
	private LongPressEvent m_longPressEvent = new LongPressEvent();
	private ReleaseEvent m_releaseEvent = new ReleaseEvent();

	private TouchInfo m_mouseTouch = new TouchInfo(-1);
	private bool m_mouseVirtualPinch = false;

	public void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(transform.gameObject);
	}

	public void LateUpdate()
	{
		UpdateZoomViaMouse();
	}

	public void Update()
	{
		CheckInput();
	}

	//
	public void ConfigureFromConfig()
	{
		m_longPressHintThreshold	= LONG_PRESS_THRESH;
		m_longPressThreshold		= LONG_PRESS_THRESH;

	}

	private void UpdateZoomViaMouse()
	{
		float rawAxisDelt = Input.GetAxis( m_pinchAxis );
		if (Mathf.Abs(rawAxisDelt) > 0.00001f)
		{
			float delt = rawAxisDelt * ScrollWheelZoomScale;
//			Debug.Log("Scroll Delta: " + delt);
			m_dispatcher.MouseWheel(delt, Input.mousePosition);
		}
	}

	private bool UseMouse
	{
		get {
			return Application.platform != RuntimePlatform.IPhonePlayer;
//			#if UNITY_EDITOR
//			return true;
//			#else
//			return false;
//			#endif
		}
	}

	private bool filteredOut = false;

	/// <summary>
	/// Clears the filter. Allow for continuation of a drag or press started over UI that would have originally filtered out the touch within this manager.
	/// </summary>
	public void ClearFilter()
	{
		filteredOut = false;
	}

	private void CheckInput()
	{
		if (UseMouse)
		{
			CheckInputMouse();
		}
		else
		{
			CheckInputTouch();
		}
	}

	/// <summary>
	/// This is only for testing on PC. For pinch-zoom interaction we use LeftAlt to activate the virtual pinch.
	/// Since pinching is tied to zooming and we handle zooming with a mouse-wheel delta we do not have to input
	/// meaningful two press/release positions. We dispatch these so that the handler will know when the pinch
	/// starts and ends. If in the future we want virtual pinches to mean more than zooming (if the positions
	/// of the press points matter) then we will need to refactor this.
	/// </summary>
	private void CheckInputMouse()
	{
		if (Input.GetKeyDown(KeyCode.LeftAlt))
		{
			m_mouseVirtualPinch = true;
			m_dispatcher.TwoPress(Input.mousePosition, Input.mousePosition);
		}

		if (Input.GetKeyUp(KeyCode.LeftAlt))
		{
			m_dispatcher.TwoRelease(Input.mousePosition, Input.mousePosition);
			m_mouseVirtualPinch = false;
		}

		if (!m_mouseVirtualPinch)
		{
			m_mouseTouch.Update(Input.GetMouseButton(0), Input.mousePosition, DragSqrThreshold);
			ProcessSingleTouchInfo(m_mouseTouch);
		}
	}

	private void CheckInputTouch()
	{
		// if one touch, do one touch stuff
		// if two touches, cancel any one touch thing and process two touch mode

		if (Input.touchCount == 1)
		{
			Touch t0 = Input.touches[0];

			// check to see if we need to start filtering or stop filtering the touch

			// if not filtering and we just added the touch check to see if it should be filtered out
			if (!filteredOut && t0.phase == TouchPhase.Began)
			{
				if (!InputManager.Instance.ShouldProcessTouch( t0.fingerId, t0.position ))
				{
					filteredOut = true;
					return;
				}
			}

			// if we are currently filtering a touch and there is no more touch then stop filtering
			if (filteredOut && (t0.phase == TouchPhase.Ended || t0.phase == TouchPhase.Canceled))
			{
				filteredOut = false;
				return; // stop filtering out but don't process the release of the touch
			}

			if (filteredOut)
			{
				return;
			}

			if (t0.phase == TouchPhase.Began)
			{
				// touch start, end, continue, move
				if (!m_pressEvent.Active)
				{
					m_longPressDuration = 0f;
					m_pressEvent.Active = true;
					m_pressEvent.Initialize(t0.position);
					DispatchPressEvent();
				}
			}

			if (t0.deltaPosition.sqrMagnitude > DragSqrThreshold || m_dragEvent.Active)
			{
				// touch start, end, continue, move
				if (!m_dragEvent.Active)
				{
					m_dragEvent.Active = true;
					m_dragEvent.Start(t0.position - t0.deltaPosition, t0.position);
					DispatchDragEvent();
				}
				m_dragEvent.Update(t0.position - t0.deltaPosition, t0.position);
				DispatchDragEvent();
			}

			// if we haven't dragged at all, have an active press, and have not yet dispatched a long press then count the press duration
			if (!m_dragEvent.Active && m_pressEvent.Active && !m_longPressEvent.Active)
			{
				m_longPressDuration += Time.deltaTime;
				if ( ( m_longPressThreshold > 0.0f ) && (m_longPressDuration > m_longPressThreshold) )
				{
					m_longPressEvent.Active = true;
					m_longPressEvent.Initialize(t0.position);
					DispatchLongPressEvent();
				}
				if ( !m_longPressEvent.Active && (m_longPressHintThreshold > 0.0f) && (m_longPressDuration > m_longPressHintThreshold) )
				{
					DispatchLongPressHintEvent( t0.position, (m_longPressDuration / m_longPressThreshold) );
				}
			}

			if (t0.phase == TouchPhase.Ended || t0.phase == TouchPhase.Canceled)
			{
				if (m_pressEvent.Active)
				{
					m_releaseEvent.Active = true;
					m_releaseEvent.Initialize(t0.position);
					DispatchReleaseEvent();

					if ((m_pressEvent.Position - m_releaseEvent.Position).sqrMagnitude < TapSqrThreshold)
					{
						m_tapEvent.Active = true;
						m_tapEvent.Initialize(m_releaseEvent.Position);
						DispatchTapEvent();
					}
				}

				if (m_dragEvent.Active)
				{
					m_dragEvent.End(t0.position - t0.deltaPosition, t0.position);
					DispatchDragEvent();
				}

				m_tapEvent.Active = false;
				m_releaseEvent.Active = false;
				m_pressEvent.Active = false;
				m_longPressEvent.Active = false;
				m_dragEvent.Active = false;
			}
		}
		else if (Input.touchCount >= 2)
		{
			EndActivePress();

			// ignore phase for 2 or more touches, just use the first two touches
			Touch? firstTouch = null;
			Touch? secondaryTouch = null; // may or may not be finger id 2, this is null if firstTouch is null, this is used for pinch events
			float delta = 0;

			GetTouches(out firstTouch, out secondaryTouch);

			if (firstTouch == null || secondaryTouch == null)
			{
				return; // multi touch occured where original first touch was released but other touches remained
			}

			// check to see if we need to start filtering or stop filtering the multi-touch

			// if not filtering and we just added the touches check to see if they should be filtered out (if any are filtered out then all are filtered out)
			if (!filteredOut)
			{
				if (firstTouch.Value.phase == TouchPhase.Began && !InputManager.Instance.ShouldProcessTouch( firstTouch.Value.fingerId, firstTouch.Value.position ))
				{
					filteredOut = true;
					return;
				}
				if (secondaryTouch.Value.phase == TouchPhase.Began && !InputManager.Instance.ShouldProcessTouch( secondaryTouch.Value.fingerId, secondaryTouch.Value.position )) 
				{
					filteredOut = true;
					return;
				}
			}

			// if we are currently filtering the touches and there are no more touches then stop filtering
			if (filteredOut && (secondaryTouch.Value.phase == TouchPhase.Ended || secondaryTouch.Value.phase == TouchPhase.Canceled || firstTouch.Value.phase == TouchPhase.Ended || firstTouch.Value.phase == TouchPhase.Canceled))
			{
				filteredOut = false;
				return; // stop filtering out but don't process the release of the touches
			}

			if (filteredOut)
			{
				return;
			}

			// if not filtering out the multi-touch then process it

			if (firstTouch.Value.phase == TouchPhase.Began || secondaryTouch.Value.phase == TouchPhase.Began)
			{
				m_dispatcher.TwoPress(firstTouch.Value.position, secondaryTouch.Value.position);
			}
			else if (secondaryTouch.Value.phase == TouchPhase.Ended || secondaryTouch.Value.phase == TouchPhase.Canceled || firstTouch.Value.phase == TouchPhase.Ended || firstTouch.Value.phase == TouchPhase.Canceled )
			{
				m_dispatcher.TwoRelease(firstTouch.Value.position, secondaryTouch.Value.position);
			}

			// Treat pinches as mouse wheel events. This means we don't care about relative position of touch spots. We only
			// care about the difference in magnituded between the two touches.
			// NOTE: We do not handle TwoDragStart and TwoDragEnd at this time; These are raw events that we may not need. If we do need these events
			// dispatched then we'll need to refactor this.
			if (secondaryTouch.Value.deltaPosition.sqrMagnitude > DragSqrThreshold || firstTouch.Value.deltaPosition.sqrMagnitude > DragSqrThreshold)
			{
				Vector2 prev, curr, curDelt, midPoint;
				curDelt = (secondaryTouch.Value.position - firstTouch.Value.position);
				midPoint = firstTouch.Value.position + curDelt * 0.5f;
				prev = (secondaryTouch.Value.position - secondaryTouch.Value.deltaPosition) - (firstTouch.Value.position - firstTouch.Value.deltaPosition);
				curr = (secondaryTouch.Value.position - firstTouch.Value.position);
				delta = curr.magnitude - prev.magnitude;
				float deltaPerc = delta / (float)Screen.width * 3f; // TODO - this seems awfully magical and DPI dependent. We'll likely need to normalize this somehow
				m_dispatcher.MouseWheel(deltaPerc, midPoint);
			}

			// TODO - add Twist Event - this differs from mouse wheel in that we do care about relative position of the two touches (in order to determine angle changes)

			m_tapEvent.Active = false;
			m_releaseEvent.Active = false;
			m_pressEvent.Active = false;
			m_longPressEvent.Active = false;
			m_dragEvent.Active = false;
		}
		else // no touches
		{
			EndActivePress();

			// process any pending active events that have "end" (?)
			m_pressEvent.Active = false;
			m_longPressEvent.Active = false;
			m_releaseEvent.Active = false;
			m_tapEvent.Active = false;
			m_dragEvent.Active = false;
		}

		// press - zero to one touch
		// release - one to zero touch
		// tap - press and release events complete, distance between two not far
		// dragStart - first time we detect drag (are not currently tracking dragging)
			// this can occur when:
			// movement of a single touch (no other touches exist)
			// single touch > drag
			// two touch > single touch > drag // Note: this case is special because it is likely to result from a continuation of a pinch event
												// in which the user intends to end the pinch but lets one finger linger and drag alone to cause
												// a new single drag. Due to this, and since this may result in "swipes" and momentum it will likely
												// require a cooldown timer to prevent new drag events from activating directly after a pinch
												// the trick is do we build in this cooldown timer as part of the input manager or make the event
												// handler enforce a cooldown after hearing the end of a Pinch event.
		// drag - any movement of a single press
		// dragEnd - when we are no longer dragging a single touch, 
			// this can occur when:
			// releasing a single touch
			// starting a two touch press
	}

	private void EndActivePress()
	{
		// end dragging if active
		if (m_dragEvent.Active)
		{
			m_dragEvent.End(m_dragEvent.CurrentPosition, m_dragEvent.CurrentPosition); // use it's own position so we don't teleport
			DispatchDragEvent();
			m_dragEvent.Active = false;
		}

		// Note - unlike dragging, we do not dispatch a tap event but we do a release event if there is an active press event
		if (m_pressEvent.Active)
		{
			m_releaseEvent.Active = true;
			m_releaseEvent.Initialize(m_pressEvent.Position);
			DispatchReleaseEvent();
			m_releaseEvent.Active = false;
			m_pressEvent.Active = false;
			m_longPressEvent.Active = false;
		}
	}

	private void GetTouches(out Touch? first, out Touch? secondary)
	{
		first = null;
		secondary = null;
		if (Input.touchCount == 0)
		{
			return;
		}
		for (int ndx = 0; ndx < Input.touchCount; ++ndx)
		{
			if (Input.touches[ndx].fingerId == 0)
			{
				first = Input.touches[ndx];
			}
			else 
			{
				secondary = Input.touches[ndx];
			}
		}

		// we only care about the secondary touch if the first isn't null
		if (first == null)
		{
			secondary = null;
		}
	}

	private void ProcessSingleTouchInfo(TouchInfo touchInfo)
	{
		// check to see if we need to start filtering or stop filtering the touch

		// if not filtering and we just added the touch check to see if it should be filtered out
		if (!filteredOut && touchInfo.Added)
		{
			if (!InputManager.Instance.ShouldProcessTouch( touchInfo.FingerId, touchInfo.Position ))
			{
				filteredOut = true;
				return;
			}
		}

		// if we are currently filtering a touch and there is no more touch then stop filtering
		if (filteredOut && touchInfo.Removed)
		{
			filteredOut = false;
			return; // stop filtering out but don't process the release of the touch
		}

		if (filteredOut)
		{
			return;
		}

		// dispatch event based on TouchInfo state

		// Press
		if (touchInfo.Added)
		{
			m_longPressDuration = 0f;
			m_pressEvent.Initialize(touchInfo.Position);
			m_pressEvent.Active = true;
			DispatchPressEvent();
		}

		// We must think of Movement as something that can occur at any time.
		// e.g. A release is not mutually exclusive of a drag
		if (touchInfo.Moved || m_dragEvent.Active)
		{
			// DragStart - if we are moving and the drag event hasn't yet started then start it
			if (!m_dragEvent.Active)
			{
				m_dragEvent.Active = true;
				m_dragEvent.Start(touchInfo.StartPosition, touchInfo.Position);
				DispatchDragEvent();
			}
			// Drag - any movement should dispatch a drag event
			m_dragEvent.Update(touchInfo.PreviousPosition, touchInfo.Position);
			DispatchDragEvent();
		}

		// if we haven't dragged at all, have an active press, and have not yet dispatched a long press then count the press duration
		if (!m_dragEvent.Active && m_pressEvent.Active && !m_longPressEvent.Active)
		{
			m_longPressDuration += Time.deltaTime;
			if ( (m_longPressThreshold > 0.0f) && (m_longPressDuration > m_longPressThreshold) )
			{
				m_longPressEvent.Active = true;
				m_longPressEvent.Initialize(touchInfo.Position);
				DispatchLongPressEvent();
			}
			if ( !m_longPressEvent.Active && (m_longPressHintThreshold > 0.0f) && (m_longPressDuration > m_longPressHintThreshold) )
			{
				DispatchLongPressHintEvent( touchInfo.Position, (m_longPressDuration / m_longPressThreshold) );
			}
		}

		// Release and Tap
		if (touchInfo.Removed)
		{
			m_releaseEvent.Initialize(touchInfo.Position);
			m_releaseEvent.Active = true;
			DispatchReleaseEvent();

			// if we are dragging then stop it and send DragEnd
			if (m_dragEvent.Active)
			{
				m_dragEvent.End(touchInfo.PreviousPosition, touchInfo.Position);
				DispatchDragEvent();
			}

			// if we didn't move too far from our original press position the dispatch a tap
			if ((touchInfo.Position - touchInfo.StartPosition).sqrMagnitude <= TapSqrThreshold)
			{
				m_tapEvent.Initialize(touchInfo.Position);
				m_tapEvent.Active = true;
				DispatchTapEvent();
			}

			m_dragEvent.Active = false;
			m_pressEvent.Active = false;
			m_longPressEvent.Active = false;
			m_releaseEvent.Active = false;
			m_tapEvent.Active = false;
		}
	}

	public void DispatchPressEvent()
	{
//		Debug.Log("DispatchPressEvent: " + m_pressEvent.Position + ", frame: " + Time.frameCount);
		m_dispatcher.Press(m_pressEvent.Position);
	}

	public void DispatchReleaseEvent()
	{
//		Debug.Log("DispatchReleaseEvent: " + m_releaseEvent.Position + ", frame: " + Time.frameCount);
		m_dispatcher.Release(m_releaseEvent.Position);
	}

	public void DispatchTapEvent()
	{
//		Debug.Log("DispatchTapEvent: " + m_tapEvent.Position + ", frame: " + Time.frameCount);
		m_dispatcher.Tap(m_tapEvent.Position);
	}

	public void DispatchDragEvent()
	{
//		Debug.Log("DispatchDragEvent: " + m_dragEvent.Phase + ", frame: " + Time.frameCount);
		if (m_dragEvent.Phase == InputEventPhase.Started)
		{
			m_dispatcher.DragStart(m_dragEvent.StartPosition, m_dragEvent.CurrentPosition);
//			Debug.Log(string.Format("#DragStart, prev: {0}, cur: {1}, frame: {2}", m_dragEvent.StartPosition, m_dragEvent.CurrentPosition, Time.frameCount));
		}
		else if (m_dragEvent.Phase == InputEventPhase.Updated)
		{
			m_dispatcher.Drag(m_dragEvent.PreviousPosition, m_dragEvent.CurrentPosition);
//			Debug.Log(string.Format("#Drag, prev: {0}, cur: {1}, frame: {2}", m_dragEvent.PreviousPosition, m_dragEvent.CurrentPosition, Time.frameCount));
		}
		else if (m_dragEvent.Phase == InputEventPhase.Ended)
		{
			m_dispatcher.DragEnd(m_dragEvent.PreviousPosition, m_dragEvent.CurrentPosition);
//			Debug.Log(string.Format("#DragEnd, prev: {0}, cur: {1}, frame: {2}", m_dragEvent.PreviousPosition, m_dragEvent.CurrentPosition, Time.frameCount));
		}
	}

	public void DispatchLongPressHintEvent( Vector2 pos, float normalizedDuration )
	{
		m_dispatcher.LongPressHint( pos, normalizedDuration );
	}

	public void DispatchLongPressEvent()
	{
//		Debug.Log("DispatchLongPressEvent: " + m_longPressEvent.Position + ", frame: " + Time.frameCount);
		m_dispatcher.LongPress(m_longPressEvent.Position);
	}

	/// <summary>
	/// Return tru
	/// </summary>
	/// <param name="fingerIndex">The index of the finger that just touched the screen</param>
	/// <param name="position">The new finger position if the input is let through</param>
	/// <returns>True to let the touch go through, or false to block it</returns>
	public delegate bool GlobalTouchFilterDelegate( int fingerIndex, Vector2 position );

	private GlobalTouchFilterDelegate globalTouchFilterFunc;

	/// <summary>
	/// Can specify a method to selectively prevent new touches from being processed until they are released.
	/// This can be useful to globally deny gesture events from being fired when above a region of the screen,
	/// or when the input has been consumed by another input system
	/// </summary>
	public static GlobalTouchFilterDelegate GlobalTouchFilter
	{
		get { return Instance.globalTouchFilterFunc; }
		set { Instance.globalTouchFilterFunc = value; }
	}

	protected bool ShouldProcessTouch( int fingerIndex, Vector2 position )
	{
		if( globalTouchFilterFunc != null )
		{
			return globalTouchFilterFunc( fingerIndex, position );
		}

		return true;
	}
}

public class TouchInfo
{
	public bool Active { get; private set; }
	public bool Added { get; private set; }
	public bool Updated { get; private set; }
	public bool Removed { get; private set; }
	public bool NoTouch { get; private set; }
	public bool Moved { get; private set; }
	public int FingerId { get; private set; }
	public Vector2 Position { get; private set; }
	public Vector2 PreviousPosition { get; private set; }
	public Vector2 StartPosition { get; private set; }
	public Vector2 DeltaPosition { get; private set; }

	public TouchInfo(int fingerId)
	{
		FingerId = fingerId;
		Active = false;
		Added = false;
		Removed = false;
		Moved = false;
		Updated = false;
		NoTouch = true;
		StartPosition = PreviousPosition = Position = Vector2.zero;
		DeltaPosition = Vector2.zero;
	}

	public void Update(bool active, Vector2 pos, float dragSqrThreshold = 0.001f)
	{
		bool prevActive = Active;
		bool prevNoTouch = NoTouch;
		Active = active;

		NoTouch = (!prevActive && !active);
		if (prevNoTouch && NoTouch) // if previously was not a touch and is still not a touch then return 
		{
			return;
		}

		if (NoTouch) // else if this is the first time we saw two consecutive frames with no touches then set the touch params 
		{
			Added = Removed = Moved = Updated = false;
			StartPosition = PreviousPosition = Position = Vector2.zero;
			DeltaPosition = Vector2.zero;
			return;
		}

		Added = (!prevActive && active);
		Removed = (prevActive && !active);
		Updated = (prevActive && active);

		if (Added)
		{
			StartPosition = PreviousPosition = Position = pos;
			DeltaPosition = Vector2.zero;
		}
		else // Updated or Removed
		{
			PreviousPosition = Position;
			Position = pos;
			DeltaPosition = Position - PreviousPosition;
		}

		Moved = DeltaPosition.sqrMagnitude > dragSqrThreshold;
		Position = pos;
	}
}

/// <summary>
/// Pinch event. Two touches press and hold then drag such that the distance between the two touch spots are different than
/// the distance between the two initial touch points will dispatch a PinchEvent with phase Started and PinchEvent with phase Updated.
/// If the distance between the two touches changes then a PinchEvent is dispatched with phase Updated.
/// Upon releasing at least one touch, if a PinchEvent has started a PinchEvent with phase Ended is dispatched.
/// </summary>
public class PinchEvent
{
	public Vector2 Touch0StartPosition { get; private set; }
	public Vector2 Touch0CurrentPosition { get; private set; }
	public Vector2 Touch1StartPosition { get; private set; }
	public Vector2 Touch1CurrentPosition { get; private set; }
	public InputEventPhase Phase { get; private set; }
	public bool Active { get; set; }

	private float m_delta;
	public float Delta { get { return m_delta; } private set { m_delta = value; } }

	private float m_prevMagnitude;
	public float PrevMagnitude { get { return m_prevMagnitude; } private set { m_prevMagnitude = value; } }

	private float m_magnitude;
	public float Magnitude { get { return m_magnitude; } private set { m_magnitude = value; } }

	private Vector2 m_vector;
	public Vector2 Vector { get { return m_vector; } private set { m_vector = value; } }

	public void Start(Vector2 p0, Vector2 p1)
	{
		Touch0StartPosition = Touch0CurrentPosition = p0;
		Touch1StartPosition = Touch1CurrentPosition = p1;
		m_vector = Touch0CurrentPosition - Touch1CurrentPosition;
		m_magnitude = m_prevMagnitude = m_vector.magnitude;
		m_delta = 0;
		Phase = InputEventPhase.Started;
	}

	public void Update(Vector2 p0, Vector2 p1)
	{
		Touch0CurrentPosition = p0;
		Touch1CurrentPosition = p1;
		m_vector = Touch0CurrentPosition - Touch1CurrentPosition;
		m_prevMagnitude = m_magnitude;
		m_magnitude = m_vector.magnitude;
		m_delta = m_magnitude - m_prevMagnitude;
		Phase = InputEventPhase.Updated;
	}

	public void End(Vector2 p0, Vector2 p1)
	{
		Touch0CurrentPosition = p0;
		Touch1CurrentPosition = p1;
		Phase = InputEventPhase.Ended;
	}
}

/// <summary>
/// Drag event. Press and hold and then drag sends a DragEvent with phase Started and DragEvent with phase Updated. 
/// Any new displacement of the touch position without releasing the touch will dispatch new DragEvent instances.
/// Upon releasing the touch, if a DragEvent has started a DragEvent with phase Ended is dispatched.
/// </summary>
public class DragEvent
{
	public Vector2 StartPosition { get; private set; }
	public Vector2 CurrentPosition { get; private set; }
	public Vector2 PreviousPosition { get; private set; }
	public InputEventPhase Phase { get; private set; }
	public bool Active { get; set; }

	public void Start(Vector2 startPos, Vector2 currentPos)
	{
		StartPosition = PreviousPosition = startPos;
		CurrentPosition = currentPos;
		Phase = InputEventPhase.Started;
	}

	public void Update(Vector2 prevPos, Vector2 currentPos)
	{
		PreviousPosition = prevPos;
		CurrentPosition = currentPos;
		Phase = InputEventPhase.Updated;
	}

	public void End(Vector2 prevPos, Vector2 currentPos)
	{
		PreviousPosition = prevPos;
		CurrentPosition = currentPos;
		Phase = InputEventPhase.Ended;
	}
}

/// <summary>
/// Tap event. Single touch and release in the same spot.
/// </summary>
public class TapEvent
{
	public Vector2 Position { get; private set; }
	public bool Active { get; set; }

	public void Initialize(Vector2 pos)
	{
		Position = pos;
	}
}

/// <summary>
/// Press event. When a user touches the screen a PressEvent is dispatched.
/// TODO - should this occur for any touch or should this only be dispatched for one touch?
/// </summary>
public class PressEvent
{
	public Vector2 Position { get; private set; }
	public bool Active { get; set; }

	public void Initialize(Vector2 pos)
	{
		Position = pos;
	}
}

/// <summary>
/// Release event. When a user releases a touch a ReleaseEvent is dispatched.
/// </summary>
public class ReleaseEvent
{
	public Vector2 Position { get; private set; }
	public bool Active { get; set; }

	public void Initialize(Vector2 pos)
	{
		Position = pos;
	}
}

public class LongPressEvent
{
	public Vector2 Position { get; private set; }
	public bool Active { get; set; }

	public void Initialize(Vector2 pos)
	{
		Position = pos;
	}
}

public enum InputEventPhase
{
	None = 0,
	Started,
	Updated,
	Ended,
}