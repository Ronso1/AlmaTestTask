using SFB;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class PopupPanelUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Blocks")]
    [SerializeField] private GameObject previewBlock;
    [SerializeField] private GameObject detailBlock;

    [Header("Preview")]
    [SerializeField] private TMP_Text previewTitle;
    [SerializeField] private TMP_Text previewShortText;

    [Header("Detail")]
    [SerializeField] private TMP_InputField titleInput;
    [SerializeField] private TMP_InputField descriptionInput;
    [SerializeField] private Image photoPreview;

    public bool IsPointerInside { get; private set; }
    public bool DetailedBlockShowed => _detailedBlockShowed;

    public bool FileDialogOpened() => _fileDialogueOpened;

    private bool _detailedBlockShowed;
    private bool _fileDialogueOpened;
    private PinUI _currentPin;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Tween _fadeTween;
    private Tween _scaleTween;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.one * 0.9f;
    }

    public bool ChangeStateOfFileDialogueWindow(bool state)
    {
        _fileDialogueOpened = state;
        return _fileDialogueOpened;
    }

    public void ShowPreview(PinUI pin, Vector2 pos)
    {
        _currentPin = pin;

        _rectTransform.anchoredPosition = pos;

        previewTitle.text = CheckTextSize(pin.Data.Title, 13);
        previewShortText.text = CheckTextSize(pin.Data.Description, 60);

        previewBlock.SetActive(true);
        detailBlock.SetActive(false);
        gameObject.SetActive(true);

        PlayShowAnimation();
    }

    private void PlayShowAnimation()
    {
        _fadeTween?.Kill();
        _scaleTween?.Kill();

        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.one * 0.9f;

        _fadeTween = _canvasGroup
            .DOFade(1f, 0.25f)
            .SetEase(Ease.OutQuad);

        _scaleTween = transform
            .DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack);
    }

    private string CheckTextSize(string text, int maxChar)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length < maxChar ? text : text.Substring(0, maxChar) + "...";
    }

    public void ShowDetail()
    {
        if (_currentPin == null) return;

        previewBlock.SetActive(false);
        detailBlock.SetActive(true);

        titleInput.text = _currentPin.Data.Title;
        descriptionInput.text = _currentPin.Data.Description;
        _detailedBlockShowed = true;

        LoadImageFromPersistent();

        detailBlock.transform.localScale = Vector3.one * 0.95f;
        detailBlock.transform
            .DOScale(1f, 0.2f)
            .SetEase(Ease.OutBack);
    }

    public void Save()
    {
        if (_currentPin == null) return;

        _currentPin.Data.Title = titleInput.text;
        _currentPin.Data.Description = descriptionInput.text;
    }

    public void Delete()
    {
        if (_currentPin == null) return;

        var userActions = UserActions.Instance;
        string path = GetImagePath();

        if (File.Exists(path))
            File.Delete(path);

        Destroy(_currentPin.gameObject);

        Hide();

        if (userActions.BlockAlreadyShowed())
            userActions.BlockHide();
    }

    public void LoadImageFromDisk()
    {
        if (_currentPin == null) return;

        var extensions = new[]
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Image",
            "",
            extensions,
            false);

        if (paths.Length == 0) return;

        _fileDialogueOpened = true;

        string sourcePath = paths[0];
        string fileName =
            System.Guid.NewGuid().ToString() +
            Path.GetExtension(sourcePath);

        string destinationPath =
            Path.Combine(Application.persistentDataPath, fileName);

        File.Copy(sourcePath, destinationPath, true);

        _currentPin.Data.ImageFileName = fileName;

        LoadImage(destinationPath);
    }

    private void LoadImageFromPersistent()
    {
        if (string.IsNullOrEmpty(_currentPin.Data.ImageFileName))
        {
            photoPreview.sprite = null;
            return;
        }

        string path = GetImagePath();
        if (path == null || !File.Exists(path))
        {
            photoPreview.sprite = null;
            return;
        }

        LoadImage(path);
    }

    private string GetImagePath()
    {
        if (_currentPin.Data.ImageFileName == null)
            return null;

        return Path.Combine(
            Application.persistentDataPath,
            _currentPin.Data.ImageFileName);
    }

    private void LoadImage(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));

        photoPreview.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }

    public void Hide()
    {
        _fadeTween?.Kill();
        _scaleTween?.Kill();

        _fadeTween = _canvasGroup
            .DOFade(0f, 0.2f)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _detailedBlockShowed = false;
                _currentPin = null;
            });

        transform
            .DOScale(0.9f, 0.2f)
            .SetEase(Ease.InQuad);
    }
}