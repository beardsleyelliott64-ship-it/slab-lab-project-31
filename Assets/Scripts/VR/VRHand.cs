using UnityEngine;
using UnityEngine.XR;

public class VRHand : MonoBehaviour
{
    public InputDevice Device => device;
    InputDevice device;
    XRNode node;
    SlapLabRuntime game;
    Vector3 lastPosition;
    float speed;
    bool ready;

    public void Initialize(XRNode n, SlapLabRuntime g){node=n;game=g;transform.position=node==XRNode.LeftHand?new Vector3(-0.25f,1.35f,0.3f):new Vector3(0.25f,1.35f,0.3f);lastPosition=transform.position;}
    void Update()
    {
        if(!device.isValid) device=InputDevices.GetDeviceAtXRNode(node);
        Vector3 p; Quaternion q;
        bool gotP=device.isValid && device.TryGetFeatureValue(CommonUsages.devicePosition,out p);
        bool gotR=device.isValid && device.TryGetFeatureValue(CommonUsages.deviceRotation,out q);
        if(gotP){ transform.localPosition=Vector3.zero; transform.position=playerSpace(p); }
        if(gotR) transform.rotation=q;
        speed=(transform.position-lastPosition).magnitude/Mathf.Max(Time.deltaTime,0.0001f);
        lastPosition=transform.position;
        if(!gotP) transform.position += Vector3.zero;
        ready=gotP;
        CheckSlap();
    }
    Vector3 playerSpace(Vector3 local)
    {
        Camera cam=Camera.main;
        if(cam) return cam.transform.TransformPoint(local);
        return local;
    }
    void CheckSlap()
    {
        if(speed<2.2f) return;
        Vector3 dir=(transform.position-lastPosition).normalized;
        Vector3 start=transform.position-dir*0.18f;
        var hits=Physics.OverlapSphere(transform.position,0.10f);
        foreach(var c in hits)
        {
            var t=c.GetComponentInParent<SlapTarget>();
            if(t!=null){t.TrySlap(transform.position,speed); break;}
        }
    }
}
