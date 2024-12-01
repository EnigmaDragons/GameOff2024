using DunGen.Adapters;
using NeoFPS;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LoadingController : OnMessage<NavigateToSceneRequested, HideLoadUiRequested, SpyNavigationCompleted>
{
    [SerializeField] private CanvasGroup loadUi;
    [SerializeField] private float loadFadeDuration = 0.5f;
    [SerializeField] private bool debugLoggingEnabled;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private UnityEvent onStartedLoading;


    private bool _isLoading;
    private float _startedTransitionAt;
    private AsyncOperation _loadState;

    bool isWaitingForPathfinding;

    private void Awake() => loadUi.alpha = 0;
    
    protected override void Execute(NavigateToSceneRequested msg)
    {
        _isLoading = true;
        isWaitingForPathfinding = msg.isGameScene;
        Debug.Log(isWaitingForPathfinding);
        onStartedLoading.Invoke();
        _startedTransitionAt = Time.timeSinceLevelLoad;
        this.ExecuteAfterDelay(() =>
        {
            _loadState = SceneManager.LoadSceneAsync(msg.SceneName);
            _loadState.completed += OnLoadFinished;
        }, loadFadeDuration);
    }

    protected override void Execute(HideLoadUiRequested msg)
    {
        if (!_isLoading && loadUi.alpha <= 0f)
            loadUi.alpha = 0f;
    }

    protected override void Execute(SpyNavigationCompleted msg)
    {
        EndLoad();
    }

    private void Update()
    {
        if (!_isLoading && loadUi.alpha <= 0f)
            return;
        
        var t = Time.timeSinceLevelLoad;
        var fadeProgress =  Mathf.Min(1, (t - _startedTransitionAt) / loadFadeDuration);
        loadUi.alpha = _isLoading 
            ? Math.Max(loadUi.alpha, Mathf.Lerp(0f, 1f, fadeProgress))
            : Mathf.Lerp(1f, 0f, fadeProgress);
        if (debugLoggingEnabled)
            Debug.Log($"Loader - Alpha {loadUi.alpha} - Fade Progress {fadeProgress}");
    }

    private void OnLoadFinished(AsyncOperation _)
    {
        if (isWaitingForPathfinding)
        {
            Message.Publish(new DisablePlayerControls());
            return;
        }
        else
        {
            EndLoad();

        }

    }

    private void EndLoad()
    {
        _isLoading = false;
        _startedTransitionAt = Time.timeSinceLevelLoad;
        _loadState.completed -= OnLoadFinished;
        if (isWaitingForPathfinding)
        {
            Message.Publish(new EnablePlayerControls());
            isWaitingForPathfinding= false;
        }
    }
}
