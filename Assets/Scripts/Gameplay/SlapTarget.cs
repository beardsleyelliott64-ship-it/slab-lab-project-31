using UnityEngine;

public class SlapTarget : MonoBehaviour
{
    SlapLabRuntime game;
    Transform head, face, body;
    float wobble;
    float lastSlap=-10f;

    public void Initialize(SlapLabRuntime g)
    {
        game=g;
        body=Part(PrimitiveType.Capsule,"Target Body",new Vector3(0,0,0),new Vector3(0.8f,1.25f,0.55f),new Color(0.12f,0.13f,0.17f),true);
        head=Part(PrimitiveType.Sphere,"Target Head",new Vector3(0,1.15f,0),new Vector3(0.62f,0.62f,0.62f),new Color(0.18f,0.19f,0.23f),true);
        face=Part(PrimitiveType.Sphere,"Face Hit Zone",new Vector3(0,1.13f,-0.39f),new Vector3(0.5f,0.36f,0.18f),new Color(0.85f,0.15f,0.2f),true);
        Part(PrimitiveType.Sphere,"Left Eye",new Vector3(-0.15f,1.2f,-0.48f),new Vector3(0.07f,0.07f,0.04f),Color.white,false);
        Part(PrimitiveType.Sphere,"Right Eye",new Vector3(0.15f,1.2f,-0.48f),new Vector3(0.07f,0.07f,0.04f),Color.white,false);
        Part(PrimitiveType.Cube,"Mouth",new Vector3(0,0.98f,-0.47f),new Vector3(0.22f,0.035f,0.03f),Color.black,false);
        Part(PrimitiveType.Cylinder,"Neck",new Vector3(0,0.55f,0),new Vector3(0.22f,0.25f,0.22f),new Color(0.1f,0.1f,0.12f),true);
        Part(PrimitiveType.Cube,"ScorePad",new Vector3(0,-1.0f,0.1f),new Vector3(1.2f,0.08f,0.5f),new Color(0.04f,0.06f,0.09f),false);
    }

    GameObject Part(PrimitiveType type,string n,Vector3 local,Vector3 scale,Color c,bool collider)
    {
        var g=GameObject.CreatePrimitive(type); g.name=n; g.transform.SetParent(transform); g.transform.localPosition=local; g.transform.localScale=scale;
        var r=g.GetComponent<Renderer>(); r.material=game.Mat(c,0.1f);
        if(!collider) Object.Destroy(g.GetComponent<Collider>());
        return g;
    }

    public void TrySlap(Vector3 point,float speed)
    {
        if(Time.time-lastSlap<0.12f) return;
        if(point.z > transform.position.z-0.15f) return;
        lastSlap=Time.time;
        float power=Mathf.Clamp(speed,0,12);
        float signed=(point.x>=transform.position.x?1f:-1f);
        wobble=Mathf.Clamp(power*0.9f,2f,11f)*signed;
        game.RegisterHit(power,point,Vector3.back);
    }

    void Update()
    {
        if(Mathf.Abs(wobble)>0.01f)
        {
            float a=Mathf.LerpAngle(0,wobble,Mathf.Clamp01(Time.deltaTime*12f));
            transform.localRotation=Quaternion.Euler(0,a,0);
            wobble=Mathf.MoveTowards(wobble,0,Time.deltaTime*22f);
        }
        else transform.localRotation=Quaternion.identity;
    }
}
