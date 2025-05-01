using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("Hover", true);  // now using bool
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("Hover", false); // return to idle
    }
}
