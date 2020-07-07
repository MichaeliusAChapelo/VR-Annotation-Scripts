using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRCustomGrabbable : OVRGrabbable
{
    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        m_grabbedBy = null;
        m_grabbedCollider = null;
    }
}
