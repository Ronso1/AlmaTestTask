using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UserActions : MonoBehaviour
{
    public static UserActions Instance;

    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private RectTransform pinsContainer;
    [SerializeField] private GameObject pinPrefab;
    [SerializeField] private PopupPanelUI popupPanelUi;

    [Header("Offsets")]
    [SerializeField] private float offsetX = 150f;
    [SerializeField] private float offsetY = 50f;
    [SerializeField] private float pinOffsetY = 30f;


    private PlayerInput _inputActions;

    private PinUI _currentPin;
    private bool _draggingPin;
    private bool _blockAlreadyShowed;

    public bool BlockAlreadyShowed() => _blockAlreadyShowed;

    public bool DraggingPin() => _draggingPin;

    public RectTransform CanvasRect => canvasRectTransform;
    public Camera CanvasCamera =>
    canvas.renderMode == RenderMode.ScreenSpaceOverlay
        ? null
        : canvas.worldCamera;

    private void Awake()
    {
        Instance = this;

        _inputActions = new PlayerInput();
        _inputActions.Enable();
        _inputActions.Interact.Touch.performed += OnUserClick;
        _inputActions.Interact.Exit.performed += OnUserExitApp;
    }

    private void OnUserExitApp(InputAction.CallbackContext context)
    {
        SaveAllPins();
        Application.Quit();
    }

    private void Start()
    {
        LoadPins();
    }

    private void OnDisable()
    {
        _inputActions.Interact.Touch.performed -= OnUserClick;
        _inputActions.Interact.Exit.performed -= OnUserExitApp;
        _inputActions.Disable();
    }

    public bool FileDialogueBoxOpened()
    {
        return popupPanelUi.FileDialogOpened();
    }

    public bool OnEnterDetailedBlock()
    {
        return popupPanelUi.ChangeStateOfFileDialogueWindow(false);
    }

    public void OnPinEnter(PinUI pin)
    {
        _currentPin = pin;

        var pos =
            pin.RectTransform.anchoredPosition +
            new Vector2(offsetX, offsetY);

        popupPanelUi.ShowPreview(pin, pos);
    }

    public void OnPinExit(PinUI pin)
    {
        if (_blockAlreadyShowed is false) popupPanelUi.Hide();
    }

    public bool DetailedShowed()
    {
        return popupPanelUi.DetailedBlockShowed;
    }

    public void BlockShowed()
    {
        _blockAlreadyShowed = true;
    }

    public void BlockHide()
    {
        _blockAlreadyShowed = false;
    }

    private void OnUserClick(InputAction.CallbackContext context)
    {
        if (IsPointerOverPin())
            return;

        CreateNewPin();
    }

    private bool IsPointerOverPin()
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
            if (result.gameObject.GetComponent<PinUI>() != null)
                return true;
        }

        return false;
    }

    private void CreateNewPin()
    {
        if (IsPointerOverBlockingUI() || _blockAlreadyShowed) return;

        Vector2 localPoint;

        var canvasRect =
            canvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Mouse.current.position.ReadValue(),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera,
            out localPoint);

        GameObject spawned =
            Instantiate(pinPrefab, pinsContainer);

        PinUI pinUI =
            spawned.GetComponent<PinUI>();

        pinUI.Initialize(localPoint, pinOffsetY);
    }

    private bool IsPointerOverBlockingUI()
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
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                return true;

            if (result.gameObject.GetComponent<TMPro.TMP_InputField>() != null)
                return true;
        }

        return false;
    }

    public void TakeAPin()
    {
        popupPanelUi.Hide();
    }

    public void SaveAllPins()
    {
        var pins =
            pinsContainer.GetComponentsInChildren<PinUI>();

        PinSaveSystem.Save(new List<PinUI>(pins));
    }

    private void LoadPins()
    {
        var saveData = PinSaveSystem.Load();

        if (saveData == null || saveData.Pins == null)
            return;

        canvas.enabled = false;

        foreach (var data in saveData.Pins)
        {
            GameObject spawned =
                Instantiate(pinPrefab, pinsContainer);

            PinUI pinUI =
                spawned.GetComponent<PinUI>();

            pinUI.OverrideData(data);
        }

        canvas.enabled = true;
    }

    public void OnStartDraggingPin()
    {
        _draggingPin = true;
    }

    public void OnStopDraggingPin()
    {
        _draggingPin = false;
    }
}
