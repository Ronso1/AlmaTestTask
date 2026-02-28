using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PanelLogic : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private PopupPanelUI popupPanelUI;
    [SerializeField] private float hideDelay = 0.2f;

    private Coroutine _hideCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }

        UserActions.Instance.BlockShowed();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);

        _hideCoroutine = StartCoroutine(DelayedHide());
    }

    private IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(hideDelay);

        if (IsPointerOverPanel())
            yield break;

        UserActions.Instance.BlockHide();
        popupPanelUI.Hide();

        _hideCoroutine = null;
    }

    private bool IsPointerOverPanel()
    {
        var pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position =
            Mouse.current.position.ReadValue();

        var results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.GetComponentInParent<PanelLogic>() != null)
                return true;
        }

        return false;
    }
}