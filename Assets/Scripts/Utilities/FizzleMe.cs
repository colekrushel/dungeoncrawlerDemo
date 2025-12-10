using UnityEngine;

//script to 'fizzle' this object upon instantiation, which moves it up and fades out after a timer pases
public class FizzleMe : MonoBehaviour
{
    [SerializeField] int lifetime;
    [SerializeField] float riseSpeed;
    float timer;

    // Update is called once per frame
    void Update()
    {
        this.transform.position += new Vector3(0, Time.deltaTime * riseSpeed, 0);
        timer += Time.deltaTime;
        if(timer >= lifetime)
        {
            StartCoroutine(UIUtils.fadeObject(this.gameObject, false, 1f, true));
        }
    }
}
