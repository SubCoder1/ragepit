using UnityEngine;
using UnityEngine.EventSystems;

public class FixedJoystick : Joystick
{
    private CanvasGroup canvasGroup;

    protected override void Start()
    {
        base.Start();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.3f; // Default translucent
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.8f; // More visible when touched
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.3f; // Back to translucent
        }
    }
}
