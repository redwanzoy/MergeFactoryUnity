using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GoalCard : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject checkmark;
    [SerializeField] private GameObject backface;
    [SerializeField] private Animator animator;

    void Start()
    {
        animator.enabled = false;
    }

    void Update()
    {
        backface.SetActive(Vector3.Dot(Vector3.forward, transform.forward) < 0);
    }


    public void Configure(int initialAmount, Sprite icon)
    {
        amountText.text = initialAmount.ToString();
        iconImage.sprite = icon;
    }

    public void UpdateAmount(int amount)
    {
        amountText.text = amount.ToString();

        Bump();
    }

    private void Bump()
    {
        LeanTween.cancel(gameObject);

        transform.localScale = Vector3.one;
        LeanTween.scale(gameObject, Vector3.one * 1.2f, .1f)
            .setLoopPingPong(1);
    }


    public void Complete()
    {
        //gameObject.SetActive(false);

        animator.enabled = true;

        checkmark.SetActive(true);
        amountText.text = "";

        animator.Play("Complete");
    }
}
