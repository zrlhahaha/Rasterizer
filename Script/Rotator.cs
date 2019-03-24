using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {


	void Update () {
        transform.rotation = transform.rotation * Quaternion.AngleAxis(60*Time.deltaTime, transform.up) ;
        
	}
}
