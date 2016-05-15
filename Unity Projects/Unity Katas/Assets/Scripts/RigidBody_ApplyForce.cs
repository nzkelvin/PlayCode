using UnityEngine;
using System.Collections;

public class ApplyForce : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var rb = GetComponent<Rigidbody>();
        rb.AddForce(new Vector3(0, 0, 10), ForceMode.Impulse);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
