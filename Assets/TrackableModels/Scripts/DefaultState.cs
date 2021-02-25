using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IButtonState {
    void triggerButtonDown();
    void triggerButtonUp();
    void homeButtonDown();
    void homeButtonUp();
    void bumperButtonDown();
    void bumperButtonUp();
    void activate();
    void deactivate();
}

public class DefaultState : IButtonState {
    public virtual void triggerButtonDown() {
        Debug.Log("Trigger Button Down");
    }

    public virtual void triggerButtonUp() {
        Debug.Log("Trigger Button Up");
    }

    public virtual void homeButtonDown() {
        Debug.Log("Home Button Down");
    }

    public virtual void homeButtonUp() {
        Debug.Log("Home Button Up");
    }

    public virtual void bumperButtonDown() {
        Debug.Log("Bumper Button Down");
    }
    
    public virtual void bumperButtonUp() {
        Debug.Log("Bumper Button Up");
    }
    
    public virtual void activate() {
        Debug.Log("Activating state");
    }

    public virtual void deactivate() {
        Debug.Log("Deactivating state");
    }
}