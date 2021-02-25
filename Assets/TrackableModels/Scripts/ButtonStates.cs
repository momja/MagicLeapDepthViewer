using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class IdleState : DefaultState {
    ButtonStateMachine machine;
    public IdleState(ButtonStateMachine machine) {
        this.machine = machine;
    }

    public override void bumperButtonDown() {
        // If home is also down, change state
        if (this.machine.triggerValue > 0.5) {
            machine.setState(machine.getOffsetScreenSpaceState());
        }
    }

    public override void triggerButtonDown() {
        // If bumper is also down, change state
        if (this.machine.bumperPressed) {
            machine.setState(machine.getOffsetScreenSpaceState());
        }
    }
} 

public class OffsetScreenSpaceState : DefaultState {
    ButtonStateMachine machine;

    public OffsetScreenSpaceState(ButtonStateMachine machine) {
        this.machine = machine;
    }

    public override void triggerButtonDown() {
        if (this.machine.bumperPressed) {
            machine.setState(machine.getIdleState());
        }
    }

    public override void bumperButtonDown() {
        if (this.machine.triggerValue > 0.5) {
            machine.setState(machine.getIdleState());
        }
    }

    public override void activate() {
        // Toggle on offset screen space component
        Debug.Log("Activating Screen Offset");
        GameObject scriptManager = GameObject.Find("Camera Parent");
        ScreenSpaceOffset component = scriptManager.GetComponent<ScreenSpaceOffset>();
        component.offsetActive = true;
    }

    public override void deactivate() {
        // Toggle off offset screen space component
        Debug.Log("Deactivating Screen Offset");
        GameObject scriptManager = GameObject.Find("Camera Parent");
        ScreenSpaceOffset component = scriptManager.GetComponent<ScreenSpaceOffset>();
        component.offsetActive = false;
    }
}