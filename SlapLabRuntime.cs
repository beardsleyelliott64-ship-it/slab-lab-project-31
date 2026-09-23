using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class SlapLabRuntime : MonoBehaviour
{
    public static SlapLabRuntime Instance;
    const float RoundLength = 60f;
    float timeLeft;
    int score, combo, bestCombo, hits;
    float lastHitTime = -99f;
    bool roundRunning = true;
    Text hud, message;
    SlapTarget target;
    VRHand leftHand, rightHand;
    Camera playerCamera;
    Transform worldRoot;
    AudioSource audioSource;
    readonly Color neon = new Color(0.1f, 0.85f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindObjectOfType<SlapLabRuntime>() == null)
        {
            var go = new GameObject("SlapLabRuntime");
            go.AddComponent<SlapLabRuntime>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 72;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        BuildGame();
    }

    void BuildGame()
    {
        worldRoot = new GameObject("SLAPLAB_WORLD").transform;

        playerCamera = FindObjectOfType<Camera>();
        if (!playerCamera)
        {
            var camGo = new GameObject("XR Camera");
            playerCamera = camGo.AddComponent<Camera>();
        }
        playerCamera.name = "XR Camera";
        playerCamera.transform.localPosition = new Vector3(0, 1.65f, -1.6f);
        playerCamera.clearFlags = CameraClearFlags.SolidColor;
        playerCamera.backgroundColor = new Color(0.015f, 0.018f, 0.03f);
        playerCamera.stereoTargetEye = StereoTargetEyeMask.Both;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.7f;
        audioSource.playOnAwake = false;

        BuildArena();
        BuildTarget();
        BuildHands();
        BuildHUD();
        timeLeft = RoundLength;
    }

    void BuildArena()
    {
        Material floorMat = Mat(new Color(0.035f,0.045f,0.065f), 0.15f);
        Material trimMat = Mat(neon, 0.4f);
        Cube("Floor", new Vector3(0,0,1.5f), new Vector3(5,0.1f,7), floorMat);
        Cube("BackWall", new Vector3(0,2.2f,4.8f), new Vector3(5,4.4f,0.1f), floorMat);
        Cube("LeftWall", new Vector3(-2.5f,2.2f,1.5f), new Vector3(0.1f,4.4f,6.6f), floorMat);
        Cube("RightWall", new Vector3(2.5f,2.2f,1.5f), new Vector3(0.1f,4.4f,6.6f), floorMat);
        for(int i=0;i<5;i++)
        {
            float z=-0.2f+i*1.3f;
            Cube("FloorStripe", new Vector3(0,0.061f,z), new Vector3(4.5f,0.015f,0.035f), trimMat);
        }
        for(int i=-2;i<=2;i++)
        {
            Cube("WallLight", new Vector3(i*1.0f,2.6f,4.72f), new Vector3(0.55f,0.035f,0.035f), trimMat);
        }
        Light key = new GameObject("KeyLight").AddComponent<Light>();
        key.type = LightType.Point; key.range = 12; key.intensity = 6;
        key.color = new Color(0.35f,0.65f,1f); key.transform.position = new Vector3(0,2.8f,1.2f);
        Light fill = new GameObject("FillLight").AddComponent<Light>();
        fill.type = LightType.Point; fill.range = 8; fill.intensity = 3;
        fill.color = new Color(1f,0.25f,0.12f); fill.transform.position = new Vector3(2,2,-0.2f);
    }

    void BuildTarget()
    {
        var root = new GameObject("SLAP_TARGET");
        root.transform.position = new Vector3(0,1.55f,1.8f);
        target = root.AddComponent<SlapTarget>();
        target.Initialize(this);
    }

    void BuildHands()
    {
        leftHand = CreateHand("LEFT HAND", XRNode.LeftHand, new Color(0.2f,0.7f,1f));
        rightHand = CreateHand("RIGHT HAND", XRNode.RightHand, new Color(1f,0.25f,0.45f));
    }

    VRHand CreateHand(string name, XRNode node, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name; go.transform.SetParent(worldRoot);
        go.transform.localScale = Vector3.one*0.12f;
        go.GetComponent<Renderer>().material = Mat(color, 0.25f);
        var hand = go.AddComponent<VRHand>();
        hand.Initialize(node, this);
        return hand;
    }

    void BuildHUD()
    {
        var canvasGo = new GameObject("HUD");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;
        canvas.transform.position = new Vector3(0,2.35f,1.0f);
        canvas.transform.rotation = Quaternion.Euler(0,0,0);
        canvas.transform.localScale = Vector3.one*0.0015f;
        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(1000,400);

        hud = TextUI(canvas.transform, "SLAPLAB", new Vector2(0,100), 72, TextAnchor.MiddleCenter);
        message = TextUI(canvas.transform, "SLAP TO START • 60 SECOND ROUND", new Vector2(0,-30), 38, TextAnchor.MiddleCenter);
        TextUI(canvas.transform, "MOVE YOUR CONTROLLERS FAST — HIT THE FACE", new Vector2(0,-105), 24, TextAnchor.MiddleCenter);
    }

    Text TextUI(Transform parent,string text,Vector2 pos,int size,TextAnchor anchor)
    {
        var go = new GameObject(text);
        go.transform.SetParent(parent,false);
        var r=go.AddComponent<RectTransform>(); r.sizeDelta=new Vector2(950,90); r.anchoredPosition=pos;
        var t=go.AddComponent<Text>(); t.text=text; t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize=size; t.alignment=anchor; t.color=Color.white; t.horizontalOverflow=HorizontalWrapMode.Overflow;
        return t;
    }

    void Update()
    {
        if (!roundRunning) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            timeLeft=0; roundRunning=false;
            message.text=$"ROUND OVER\nSCORE {score} • HITS {hits} • BEST COMBO {bestCombo}\nRESET THE APP TO PLAY AGAIN";
        }
        else if (hud)
            hud.text=$"SLAPLAB    {Mathf.CeilToInt(timeLeft):00}s\nSCORE {score}    COMBO x{combo}";
        if (combo>0 && Time.time-lastHitTime>2.6f) combo=0;
    }

    public void RegisterHit(float power, Vector3 point, Vector3 normal)
    {
        if (!roundRunning) return;
        hits++;
        if (Time.time-lastHitTime<2.6f) combo=Mathf.Min(combo+1,12); else combo=1;
        lastHitTime=Time.time; bestCombo=Mathf.Max(bestCombo,combo);
        int gained=Mathf.RoundToInt(Mathf.Lerp(25,500,Mathf.Clamp01(power/10f)))*(1+combo/4);
        score+=gained;
        message.text=$"SLAP!  +{gained}";
        StartCoroutine(ClearMessage());
        ImpactFX(point, power);
        HapticAll(0.12f+0.25f*Mathf.Clamp01(power/8f),0.25f);
        PlayImpact(power);
    }

    IEnumerator ClearMessage()
    {
        yield return new WaitForSeconds(0.55f);
        if(roundRunning) message.text="HIT THE FACE!";
    }

    void ImpactFX(Vector3 p,float power)
    {
        var go=new GameObject("ImpactFX"); go.transform.position=p;
        var ps=go.AddComponent<ParticleSystem>();
        var main=ps.main; main.startLifetime=0.35f; main.startSpeed=2.5f+power*0.25f; main.startSize=0.035f; main.maxParticles=35;
        var em=ps.emission; em.rateOverTime=0; em.SetBursts(new[]{new ParticleSystem.Burst(0,Mathf.Clamp(8+(int)power*2,8,35))});
        var sh=ps.shape; sh.shapeType=ParticleSystemShapeType.Sphere; sh.radius=0.05f;
        var r=ps.GetComponent<ParticleSystemRenderer>(); r.material=Mat(neon,0);
        Destroy(go,1.2f);
    }

    void PlayImpact(float power)
    {
        int n=22050; var clip=AudioClip.Create("slap",n,1,44100,false);
        float[] data=new float[n];
        for(int i=0;i<n;i++){ float t=i/(float)n; float env=Mathf.Exp(-18*t); data[i]=(Mathf.Sin(2*Mathf.PI*(90+power*12)*t)*0.65f+UnityEngine.Random.value*2f-1f)*env; }
        clip.SetData(data,0); audioSource.PlayOneShot(clip,0.2f);
        Destroy(clip,1f);
    }

    void HapticAll(float amp,float dur)
    {
        Haptic(leftHand?leftHand.Device:default,amp,dur);
        Haptic(rightHand?rightHand.Device:default,amp,dur);
    }
    void Haptic(InputDevice d,float amp,float dur){ if(d.isValid) d.SendHapticImpulse(0,amp,dur); }

    public Material Mat(Color c,float metallic)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if(m==null) m=new Material(Shader.Find("Standard"));
        m.color=c; m.SetFloat("_Metallic",metallic); m.SetFloat("_Smoothness",0.65f); return m;
    }
    GameObject Cube(string n,Vector3 p,Vector3 s,Material m)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube); g.name=n; g.transform.SetParent(worldRoot); g.transform.position=p; g.transform.localScale=s; g.GetComponent<Renderer>().material=m; return g;
    }
}
