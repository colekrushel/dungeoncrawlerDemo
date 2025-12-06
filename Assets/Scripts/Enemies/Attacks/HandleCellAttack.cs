using System.Collections;
using UnityEngine;

public class HandleCellAttack : MonoBehaviour
{
    [SerializeField] int chargeLength; //amount of time to display the warning wireframe before hitting the cell
    [SerializeField] GameObject cellframe;
    public StartCellAttack caller;
    bool attackStarted = false;
    float chargeTimer;
    Animator animator;
    void Start()
    {
        animator = this.GetComponent<Animator>();
        chargeTimer = chargeLength;

        //make warning frame visible
        cellframe.SetActive(true);
    }

    private void executeAttack()
    {
        cellframe.SetActive(false);
        animator = this.GetComponent<Animator>();
        //animator.Play("cellattack");
        StartCoroutine(waitforfinish());
    }

    IEnumerator waitforfinish()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
        //after finish, start new attack if applicable and kill self
        Debug.Log("attack finished; sending death signal");
        caller.onattackend();
        //animator.StopPlayback();
        Destroy(this.gameObject);
    }

    

    // Update is called once per frame
    void Update()
    {
        chargeTimer -= Time.deltaTime * 10;
        if (!attackStarted && chargeTimer < 0)
        {
            executeAttack();
            attackStarted = true;
        }
    }
}
