using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowsMovementController : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{
    [SerializeField] public WindowsType windowsType;
    [SerializeField] public WindowsDropArea originalParent;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private bool canDrag;
    public bool hasDrag;
    [SerializeField] private float holdTime = 3f;
    [SerializeField] private GameObject loadingParent;
    [SerializeField] private Image loadingBar;
    private Coroutine holdCoroutine;
        
    [Header("Animation Variables")]
    [SerializeField] private LeanTweenType animCurve;
    [SerializeField] [Range(0f,5f)]private float animUpTime;
    [SerializeField] [Range(1f,2f)] private float scaleSizeBig;
    [SerializeField] [Range(0f,5f)]private float animDownTime;
    [SerializeField] public  Transform upperObject;
    private Vector2 animSize;

    [SerializeField] private AudioClip popUP;
    [SerializeField] private AudioClip popDown;
    [SerializeField] private AudioClip breaAudioClip;
    

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        animSize = rectTransform.sizeDelta;
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (transform.parent.TryGetComponent(out WindowsDropArea windowFrameParent))
        {
            originalParent = windowFrameParent;
        }

    }

    private void Start()
    {
        upperObject = GameManager.Instance.upperObject;
        PlaceWindow(originalParent);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        holdCoroutine = StartCoroutine(HoldRoutine());
        hasDrag = false;
        canDrag = false;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!canDrag)
        {
            if (holdCoroutine != null)
            {
                StopCoroutine(holdCoroutine);
                
                loadingParent.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!canDrag)
        {
            if (holdCoroutine != null)
            {
                StopCoroutine(holdCoroutine);
                loadingParent.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!hasDrag)
            {
                PlaceWindow(originalParent);
                AnimateDown();
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canDrag)
        {
            hasDrag = true;
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canDrag && hasDrag)
        {
            //canvasGroup.alpha = 1.0f;
            PlaceWindow(originalParent);
            AnimateDown();
            canDrag = false;
        }
        
    }
    public void PlaceWindow(WindowsDropArea parent)
    {
        
        transform.SetParent(parent.transform);
        originalParent = parent;
        rectTransform.anchoredPosition = Vector2.zero;
        originalParent.hasWindow = true;
    }

    

    private IEnumerator HoldRoutine()
    {
        loadingBar.fillAmount = 0f;
        loadingBar.color = ClimateManager.Instance.removingColorBar;
        loadingParent.gameObject.SetActive(true);
        float hold = holdTime * ClimateManager.Instance.currentWeather.removingMultuplyDelay;
        float elapsedTime = 0f;
        while (elapsedTime <= hold)
        {
            elapsedTime += Time.deltaTime;
            loadingBar.fillAmount = elapsedTime / hold;
            yield return null;
        }
        transform.SetParent(upperObject);
        canDrag = true;
        loadingParent.SetActive(false);
        originalParent.hasWindow = false;
        AnimateUP();
    }

    private void AnimateUP()
    {
        GameManager.Instance.PlaySFX(popUP,5);
        canvasGroup.blocksRaycasts = false;
        LeanTween.pause(this.gameObject);
        LeanTween.size(rectTransform, animSize*scaleSizeBig, animUpTime).setEase(animCurve);
    }

    private void AnimateDown()
    {
        GameManager.Instance.PlaySFX(popDown,5);
        canvasGroup.blocksRaycasts = true;
        LeanTween.pause(this.gameObject);
        LeanTween.size(rectTransform, animSize, animDownTime).setEase(animCurve);
    }
    
    public void DestroyWindow(bool instant=false)
    {
        if (instant)
        {
            this.gameObject.SetActive(false);
            return;
        }
        if(this.TryGetComponent(out Image image))image.raycastTarget = false;
        transform.SetParent(upperObject);
        if (this.TryGetComponent(out Rigidbody2D rb)) rb.simulated = true;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        GameManager.Instance.PlaySFX(breaAudioClip,5);
        this.gameObject.SetActive(false);
        
    }
}