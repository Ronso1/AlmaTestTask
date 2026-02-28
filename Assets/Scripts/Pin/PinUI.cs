using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PinUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    [SerializeField] private float timeToTakePin = 1f;

    public RectTransform RectTransform { get; private set; }
    public PinData Data { get; private set; }

    private bool _pinIsTaken;
    private Coroutine _dragPinCoroutine;

    private Tween _scaleTween;
    private Tween _punchTween;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();

        if (Data == null)
        {
            Data = new PinData
            {
                ID = System.Guid.NewGuid().ToString(),
                Title = "Новая метка",
                Description = "Чтобы заполнить описание метки, нажмите \"Читать дальше\"."
            };
        }
    }

    public void Initialize(Vector2 pos, float offsetY)
    {
        RectTransform.anchoredPosition =
            new Vector2(pos.x, pos.y + offsetY);

        Data.Position = RectTransform.anchoredPosition;
    }

    public void OverrideData(PinData data)
    {
        Data = data;
        RectTransform.anchoredPosition = data.Position;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnimateScale(1.15f);

        var userActions = UserActions.Instance;

        if (userActions.DraggingPin()) return;

        userActions.OnPinEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateScale(1f);

        if (IsPointerNowOverPopup())
            return;

        UserActions.Instance.OnPinExit(this);
    }

    private bool IsPointerNowOverPopup()
    {
        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<PanelLogic>() != null)
                return true;
        }

        return false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateScale(0.9f);

        _dragPinCoroutine = StartCoroutine(StartTimerToHoldPin());
        UserActions.Instance.OnStartDraggingPin();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pinIsTaken = false;

        if (_dragPinCoroutine != null)
            StopCoroutine(_dragPinCoroutine);

        AnimateScale(1.15f);

        UserActions.Instance.OnPinEnter(this);
        UserActions.Instance.OnStopDraggingPin();
    }

    private IEnumerator StartTimerToHoldPin()
    {
        yield return new WaitForSeconds(timeToTakePin);

        UserActions.Instance.TakeAPin();
        _pinIsTaken = true;

        _punchTween?.Kill();
        _punchTween = transform.DOPunchScale(
            Vector3.one * 0.25f,
            0.3f,
            10,
            0.8f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_pinIsTaken == false) return;

        RectTransform canvasRect =
            UserActions.Instance.CanvasRect;

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            UserActions.Instance.CanvasCamera,
            out localPoint);

        RectTransform.anchoredPosition = localPoint;
    }

    private void AnimateScale(float target)
    {
        _scaleTween?.Kill();

        _scaleTween = transform
            .DOScale(target, 0.2f)
            .SetEase(Ease.OutBack);
    }
}