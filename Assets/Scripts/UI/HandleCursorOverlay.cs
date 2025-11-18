using UnityEngine;

//handle transitions for the cursor overlay ui 
public class HandleCursorOverlay : MonoBehaviour
{
    [SerializeField] static GameObject cursorDefault;
    [SerializeField] static GameObject cursorPointer;
    public static MonoBehaviour Instance { get; private set; }

    public enum cursorState { none, pointer, locked}
    public static cursorState state = cursorState.none;
    void Start()
    {
        Instance = this;
        cursorDefault = transform.Find("CursorOffset").Find("CursorBackground").gameObject;
        cursorPointer = transform.Find("CursorOffset").Find("CursorPointer").gameObject;
        cursorPointer.SetActive(false);
    }

    //called from external cursorinsidelistener scripts
    public static void setState(cursorState istate)
    {
        state = istate;
        switch (state)
        {
            case cursorState.none:
                cursorDefault.SetActive(true);
                cursorPointer.SetActive(false);
                break;
            case cursorState.pointer:
                cursorDefault.SetActive(false);
                cursorPointer.SetActive(true);
                break;
            case cursorState.locked:
                cursorDefault.SetActive(false);
                cursorPointer.SetActive(false);
                break;
        }
    }

    public static void pointerToggle()
    {

    }

    
}
