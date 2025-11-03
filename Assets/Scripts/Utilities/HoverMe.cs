using UnityEngine;

//put this script on a 2D gameobject to give it a hover effect
public class HoverMe : MonoBehaviour
{
    //hover params
    [SerializeField] float moveSpeed; //duration of one hover loop (up and back down; starts at bottom state)
    [SerializeField] float transitionSpeed = 1; //speed to go from up to down 
    [SerializeField] float hoverHeight; //distance object will hover up before coming back down
    float distanceMoved = 0;
    float moveMult = 1;
    bool stateTransitionUpToDown = false;
    bool stateTransitionDownToUp = false;

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            float d = Time.deltaTime * moveMult * moveSpeed;
            distanceMoved += d;
            gameObject.transform.localPosition += new Vector3(0, d);
            if (distanceMoved > hoverHeight)
            {
                stateTransitionUpToDown = true;
            } else if (distanceMoved < 0)
            {
                stateTransitionDownToUp = true;
            }
            if (stateTransitionUpToDown)
            {
                if (moveMult > -1) moveMult -= Time.deltaTime * transitionSpeed;
                else stateTransitionUpToDown = false;
            } else if (stateTransitionDownToUp)
            {
                if (moveMult < 1) moveMult += Time.deltaTime * transitionSpeed;
                else stateTransitionDownToUp = false; 
            }
        }
    }

    //reset the hover animation; usually called from external triggers
    public void resetHover()
    {
        //gameObject.transform.localPosition += new Vector3(0, distanceMoved * -1);
        distanceMoved = 0;
        stateTransitionDownToUp = false;
        stateTransitionUpToDown = false;
    }
}
