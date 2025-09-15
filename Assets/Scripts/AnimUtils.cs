using System.Collections;
using UnityEngine;

public class AnimUtils : MonoBehaviour
{

    public static MonoBehaviour Instance { get; private set; }


    // Update is called once per frame
    void Update()
    {
        
    }


    //handle animation finish items because it requires a monobehavior script
    static public void waitForAnimationFinish(Animator animator, string behavior)
    {
        if (!Instance)
        {
            Instance = new GameObject("AnimUtils").AddComponent<AnimUtils>();
        }
        Instance.StartCoroutine(waitForAnim(animator, behavior));


    }

    static IEnumerator waitForAnim(Animator animator, string behavior)
    {
        //wait for the transition to end
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);
        Debug.Log("finish 1");
        yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);
        Debug.Log("finishd 2");
        //now animation has ended, call the appropriate end behavior
        switch (behavior)
        {
            case "doorOpen":
                //after door opens, queue up 2 player moves and free inputs
                Player.actionQueue.Add('w');
                Player.actionQueue.Add('w');
                Player.inputLock = false;
                break;
        }
    }
}
