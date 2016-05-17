using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public float XSensitivity = 0.6f;
    public float YSensitivity = 0.6f;
    private float _rotationEulerX = 0.0f;
    private float _rotationEulerY = 0.0f;

	// Use this for initialization
	void Start () {
        Vector3 angles = transform.eulerAngles;
        _rotationEulerX = transform.rotation.y;
        _rotationEulerY = transform.rotation.x;
    }
	
	// Update is called once per frame
	void Update () {
        _rotationEulerX += UnityEngine.Input.GetAxis("Mouse Y") * XSensitivity;
        _rotationEulerY -= UnityEngine.Input.GetAxis("Mouse X") * YSensitivity;

        Quaternion rotation = Quaternion.Euler(_rotationEulerX, _rotationEulerY, 0);

        transform.rotation = rotation;
	}
}
