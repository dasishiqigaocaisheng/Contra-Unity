using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIInput : MonoBehaviour
{
    public static UIInput Inst { get; private set; }

    public bool Enable { get; set; }

    public UnityEvent OnConfirm { get; private set; } = new UnityEvent();

    public UnityEvent OnEscape { get; private set; } = new UnityEvent();

    public UnityEvent OnSelectLast { get; private set; } = new UnityEvent();

    public UnityEvent OnSelectNext { get; private set; } = new UnityEvent();

    private void Awake()
    {
        Inst = this;
    }

    private void Update()
    {
        if (!Enable)
            return;

        if (Keyboard.current.wKey.wasPressedThisFrame)
            OnSelectLast.Invoke();
        else if (Keyboard.current.sKey.wasPressedThisFrame)
            OnSelectNext.Invoke();
        else if (Keyboard.current.jKey.wasPressedThisFrame)
            OnConfirm.Invoke();
        else if (Keyboard.current.kKey.wasPressedThisFrame)
            OnEscape.Invoke();
    }
}
