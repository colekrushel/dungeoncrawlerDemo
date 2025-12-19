using System.Collections;
using UnityEngine;


public class HandleCellAttack : MonoBehaviour
{
    [SerializeField] int chargeLength; //amount of time to display the warning wireframe before hitting the cell
    [SerializeField] GameObject cellframe;
    public StartCellAttack caller;
    bool attackStarted = false;
    [SerializeField] bool keepObstacleAfterEnd;
    float chargeTimer;
    Animator animator;
    void Start()
    {
        animator = this.GetComponent<Animator>();
        animator.speed = 0;
        chargeTimer = chargeLength;

        //make warning frame visible
        cellframe.SetActive(true);
    }

    private void executeAttack()
    {
        animator.speed = 1;
        cellframe.SetActive(false);
        animator = this.GetComponent<Animator>();
        //animator.Play("cellattack");
        StartCoroutine(waitforfinish());
    }

    IEnumerator waitforfinish()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
        //after finish, start new attack if applicable and kill self
        caller.onattackend();
        //animator.StopPlayback();
        if (!keepObstacleAfterEnd)
        {

            Destroy(this.gameObject);
        } else
        {
            
        }
            
    }

    public void pauseanim()
    {
        //just used for status so hard code staute behavior
        animator.speed = 0;
        //mark cell as untraversible
        //hardcoded for south zone
        if (!keepObstacleAfterEnd)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Vector3 pos = this.transform.parent.localPosition;
            DungeonCell associatedCell = GridUtils.grids[Player.currentLayer].getCell((int)pos.x, (int)pos.z);
            associatedCell.traversible = false;
        }

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
