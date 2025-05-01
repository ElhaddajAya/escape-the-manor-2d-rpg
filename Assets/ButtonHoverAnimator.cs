using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverAnimator : MonoBehaviour, IPointerEnterHandler
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetTrigger("Hover");
    }
}
