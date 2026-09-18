using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class KillScreenUI : MonoBehaviour
{
    public static KillScreenUI Instance { get; private set; }

    public event Action OnRespawnTimerFinished;

    [Header("Smash UI")]
    [SerializeField] private RectTransform _smashUITransform;
    [SerializeField] private TMP_Text _smashedPlayerText;

    [Header("Smashed UI")]
    [SerializeField] private RectTransform _smashedUITransform;
    [SerializeField] private TMP_Text _smashedByPlayerText;
    [SerializeField] private TMP_Text _respawnTimeText;

    [Header("Settings")]
    [SerializeField] private float _scaleDuration;
    [SerializeField] private float _smashUIStayDuration;

    private float _timer;
    private bool _isTimerActive;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _smashUITransform.gameObject.SetActive(false);
        _smashedUITransform.gameObject.SetActive(false);

        _smashUITransform.localScale = Vector3.zero;
        _smashedUITransform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (_isTimerActive)
        {
            _timer -= Time.deltaTime;
            int timer = (int)_timer;
            _respawnTimeText.text = timer.ToString();

            if(_timer <= 0f)
            {
                _smashedUITransform.localScale = Vector3.zero;
                _smashedUITransform.gameObject.SetActive(false);
                _isTimerActive = false;
                _smashedByPlayerText.text = string.Empty;
                OnRespawnTimerFinished?.Invoke();
            }
        }
    }

    public void SetSmashUI(string playerName)
    {
        StartCoroutine(SetSmashUICoroutine(playerName));
    }

    private IEnumerator SetSmashUICoroutine(string playerName)
    {
        _smashUITransform.gameObject.SetActive(true);
        _smashUITransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);
        _smashedPlayerText.text = playerName;

        yield return new WaitForSeconds(_smashUIStayDuration);

        _smashUITransform.gameObject.SetActive(false);
        _smashUITransform.localScale = Vector3.zero;
        _smashedPlayerText.text = string.Empty;
    }

    public void SetSmashedUI(string playerName, int respawnTimeCounter)
    {
        _smashedUITransform.gameObject.SetActive(true);
        _smashedUITransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);

        _smashedByPlayerText.text = playerName;
        _respawnTimeText.text = respawnTimeCounter.ToString();

        _isTimerActive = true;
        _timer = respawnTimeCounter;
    }
}
