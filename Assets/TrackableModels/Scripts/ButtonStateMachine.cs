using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class ButtonStateMachine : MonoBehaviour
{
    [HideInInspector]
    public float triggerValue = 0.0f;
    [HideInInspector]
    public bool homePressed = false;
    [HideInInspector]
    public bool bumperPressed = false;

    private MLInput.Controller _controller;
    private IButtonState idleState;
    private IButtonState offsetScreenSpaceState;
    private IButtonState state;
    private bool _triggerEnabled = false;

    void Start()
    {
        MLInput.OnControllerButtonDown += OnButtonDown;
        MLInput.OnControllerButtonUp += OnButtonUp;
        _controller = MLInput.GetController(MLInput.Hand.Left);
        this.idleState = new IdleState(this);
        this.offsetScreenSpaceState = new OffsetScreenSpaceState(this);
        this.setState(this.idleState); // We always start in idle
    }

    void Update()
    {
        CheckTrigger();
    }

    void OnDestroy()
    {
        MLInput.OnControllerButtonDown -= OnButtonDown;
        MLInput.OnControllerButtonUp -= OnButtonUp;
    }

    public ButtonStateMachine()
    {
    }
    public IButtonState getIdleState()
    {
        return this.idleState;
    }
    public IButtonState getOffsetScreenSpaceState()
    {
        return this.offsetScreenSpaceState;
    }
    public void setState(IButtonState state)
    {
        if (this.state != null)
            this.state.deactivate();
        this.state = state;
        this.state.activate();
    }

    void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
    {
        if (button == MLInput.Controller.Button.Bumper)
        {
            this.bumperPressed = true;
            this.state.bumperButtonDown();
        }
        print("Button pressed");
    }

    void OnButtonUp(byte controllerId, MLInput.Controller.Button button)
    {
        if ((button == MLInput.Controller.Button.HomeTap))
        {
            this.homePressed = false;
            this.state.homeButtonUp();
        }
        if ((button == MLInput.Controller.Button.Bumper))
        {
            this.bumperPressed = false;
            this.state.bumperButtonUp();
        }
    }

    void CheckTrigger()
    {
        if (Application.isPlaying && Application.isEditor)
        {
            return;
        }
        this.triggerValue = _controller.TriggerValue;
        if (this.triggerValue > 0.5 && !this._triggerEnabled)
        {
            this.state.triggerButtonDown();
            this._triggerEnabled = true;
        }
        else if (this.triggerValue < 0.5 && this._triggerEnabled)
        {
            this.state.triggerButtonUp();
            this._triggerEnabled = false;
        }
    }
}