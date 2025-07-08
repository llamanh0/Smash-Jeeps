using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;

public class PlayerSkillController : NetworkBehaviour
{
    public static event Action OnTimerFinished;

    [Header("References")]
    [SerializeField] private Transform _rocketLauncherTransform;
    [SerializeField] private Transform _rocketLaunchPoint;

    [Header("Skill Settings")]
    [SerializeField] private bool _hasSkillAlready;
    [SerializeField] private float _resetDelay;

    private MysteryBoxSkillsSO _mysteryBoxSkill;
    private bool _isSkillUsed;
    private bool _hasTimerStarted;
    private float _timer;
    private float _timerMax;
    private int _mineAmountCounter;

    private void Update()
    {
        if(!IsOwner) return;

        if(Input.GetKeyDown(KeyCode.Space) && !_isSkillUsed)
        {
            ActivateSkill();
            _isSkillUsed = true;
        }

        if(_hasTimerStarted)
        {
            _timer -= Time.deltaTime;
            SkillsUI.Instance.SetTimerCounterText((int)_timer);

            if(_timer <= 0f)
            {
                OnTimerFinished?.Invoke();
                SkillsUI.Instance.SetSkillToNone();
                _hasSkillAlready = false;
                _hasTimerStarted = false;
            }
        }
    }

    public void SetupSkill(MysteryBoxSkillsSO skill)
    {
        _mysteryBoxSkill = skill;

        if(_mysteryBoxSkill.SkillType == SkillType.Rocket)
        {
            SetRocketLauncherActiveRpc(true);
        }

        _hasSkillAlready = true;
        _isSkillUsed = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetRocketLauncherActiveRpc(bool active)
    {
        _rocketLauncherTransform.gameObject.SetActive(active);
    }

    private IEnumerator ResetRocketLauncher()
    {
        yield return new WaitForSeconds(_resetDelay);
        SetRocketLauncherActiveRpc(false);
    }

    public void ActivateSkill()
    {
        if(!_hasSkillAlready) return;

        SkillManager.Instance.ActivateSkill(_mysteryBoxSkill.SkillType, transform, OwnerClientId);

        // Skill kullanıldıktan sonra hemen sıfırla (Timer değilse)
        if (_mysteryBoxSkill.SkillUsageType == SkillUsageType.None || _mysteryBoxSkill.SkillUsageType == SkillUsageType.Amount)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
        }
        else if (_mysteryBoxSkill.SkillUsageType == SkillUsageType.Timer)
        {
            _hasTimerStarted = true;
            _timerMax = _mysteryBoxSkill.SkillData.SpawnAmountOrTimer;
            _timer = _timerMax;
        }

        if(_mysteryBoxSkill.SkillType == SkillType.Rocket)
        {
            StartCoroutine(ResetRocketLauncher());
        }
    }

    private void SetSkillToNone()
    {
        if(_mysteryBoxSkill.SkillUsageType == SkillUsageType.None)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
        }

        if (_mysteryBoxSkill.SkillUsageType == SkillUsageType.Timer)
        {
            _hasTimerStarted = true;
            _timerMax = _mysteryBoxSkill.SkillData.SpawnAmountOrTimer;
            _timer = _timerMax;
        }

        if(_mysteryBoxSkill.SkillUsageType == SkillUsageType.Amount)
        {
            _mineAmountCounter = _mysteryBoxSkill.SkillData.SpawnAmountOrTimer;

            SkillManager.Instance.OnMineCountReduced += SkillManager_OnMineCountReduced;
        }
    }

    private void SkillManager_OnMineCountReduced()
    {
        _mineAmountCounter--;
        SkillsUI.Instance.SetTimerCounterText(_mineAmountCounter);

        if(_mineAmountCounter <= 0)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
            SkillManager.Instance.OnMineCountReduced -= SkillManager_OnMineCountReduced;
        }
    }

    public bool HasSkillAlready()
    {
        return _hasSkillAlready;
    }
    
    public Vector3 GetRocketLauncherPosition()
    {
        return _rocketLaunchPoint.position;
    }
}
