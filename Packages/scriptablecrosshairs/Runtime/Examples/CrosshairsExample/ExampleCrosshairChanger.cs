using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCrosshairChanger : MonoBehaviour
{
    public float loopTime = 5;
    public Vector2 spreadRange = new Vector2(2, 5);
    CrosshairController controller;

    public Gradient colorLoop;
    public AnimationCurve lengthOverTime = AnimationCurve.Constant(0, 1, 20);

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CrosshairController>();
    }

    // Update is called once per frame
    void Update()
    {
        float t = Mathf.Repeat(Time.time / loopTime, 1);
        float p = Mathf.PingPong(Time.time / loopTime, 1);

        controller.color = colorLoop.Evaluate(t);

        //ping pong the spread between highest and lowest
        controller.spread = Mathf.Lerp(spreadRange.x, spreadRange.y, p);

        controller.UpdateCrosshairParts();
    }
}
