using UnityEngine;

public class BatSwing : MonoBehaviour
{
    public float swingSpeed = 360.0f; // degrees per second
    public float attackAngle = 90.0f;
    public bool isSwinging = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public float currentAngle = 0.0f;
    // Update is called once per frame
    void Update()
    {
        if(isSwinging)
        {
            float rotationAmount = swingSpeed * Time.deltaTime;
            
            currentAngle += rotationAmount;
            transform.Rotate(0, 0, -rotationAmount);

            if (currentAngle >= attackAngle)
            {
                Reset();
            }
        }
    }

    public void Init(float speed, float anlge)
    {
        transform.Rotate(0, 0, anlge * 0.5f);
        attackAngle = anlge;
        swingSpeed = speed;
        Reset();
    }

    public void Reset()
    {
        transform.localEulerAngles = new Vector3(0, 0, attackAngle * 0.5f);
        isSwinging = false;
        currentAngle = 0.0f;
    }

    public void Swing()
    {
        isSwinging = true;
    }
}
