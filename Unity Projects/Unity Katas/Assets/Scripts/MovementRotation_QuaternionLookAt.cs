using UnityEngine;
using System.Collections;
using System;

public class MovementRotation_QuaternionLookAt : MonoBehaviour {
    public Transform Target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //ImmediateLookAt();
        DelayedLookAt();
    }

    private void DelayedLookAt()
    {
        var relativePos = Target.position - this.transform.position;
        var rotation = Quaternion.LookRotation(relativePos);
        var current = transform.rotation;
        transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime);
    }

    private void ImmediateLookAt()
    {
        var relativePos = Target.position - this.transform.position;
        transform.rotation = Quaternion.LookRotation(relativePos);
    }
}
