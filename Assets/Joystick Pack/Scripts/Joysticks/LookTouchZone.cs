using UnityEngine;
using UnityEngine.EventSystems;

public class LookTouchZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Vector2 lookDelta { get; private set; }
    public bool IsDragging { get; private set; }
    public Vector2 TouchPosition { get; private set; }

    private int activeFingerId = -1;
    private Vector2 lastPosition;
    private Vector2 frameDelta = Vector2.zero;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.position.x < Screen.width / 2f) return;

        activeFingerId = eventData.pointerId;
        lastPosition = eventData.position;
        TouchPosition = eventData.position;
        frameDelta = Vector2.zero;
        IsDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDragging || eventData.pointerId != activeFingerId) return;
        if (eventData.position.x < Screen.width / 2f) return;

        Vector2 rawDelta = eventData.position - lastPosition;

        float dpiScale = (Screen.dpi > 0) ? (Screen.dpi / 160f) : (Screen.width / 800f);
        dpiScale *= 2;

        frameDelta = rawDelta * dpiScale * dpiScale;

        lastPosition = eventData.position;
        TouchPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activeFingerId) return;

        IsDragging = false;
        activeFingerId = -1;
        frameDelta = Vector2.zero;
        lookDelta = Vector2.zero;
    }

    private void LateUpdate()
    {
        lookDelta = frameDelta;
        frameDelta = Vector2.zero;
    }
}
