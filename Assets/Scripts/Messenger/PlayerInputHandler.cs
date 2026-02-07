using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Action<InputAction.CallbackContext> OnMessageApproved;
    private PlayerInput _input;

    private void Awake()
    {
        TryGetComponent(out _input);
        _input.onActionTriggered += OnInput;
    }

    private void OnInput(InputAction.CallbackContext ctx)
    {
        switch (ctx.action.name)
        {
            case "SendMessage":
                OnMessageApproved?.Invoke(ctx); 
                break;
        }
    }
}
