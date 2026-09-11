using UnityEngine;

public class UserInputView : MonoBehaviour, IInputSource {
    private PlayerInput _input;

    private void Start() {
        _input = new();
        _input.Enable();
    }

    public DrivetrainInputModel Read() {
        var input = _input.Game.Movement.ReadValue<Vector2>();
        var throttle = Mathf.Clamp01(input.y);
        var brake = Mathf.Clamp01(-input.y);
        var steering = input.x;

        return new(throttle, brake, steering);
    }
}