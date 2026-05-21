using System.Threading.Tasks;
using UnityEngine;
using System;

/// <summary>
/// Moves a Transform to a given position over a given period of time.
/// </summary>
public readonly struct Tween
{
    /// <summary>
    /// The duration of this tween.
    /// </summary>
    public readonly float Duration;
    /// <summary>
    /// The Transform being controlled by this tween.
    /// </summary>
    public readonly Transform Transform;
    /// <summary>
    /// The target position of this tween.
    /// </summary>
    public readonly Vector3 Target;
    /// <summary>
    /// The easing mode of this tween (linear by default).
    /// </summary>
    public readonly Easing Easing;

    /// <summary>
    /// Returns a task that is completed once the tween has finished. Can be awaited in async functions.
    /// </summary>
    public readonly Task TweenCompletion => _completionSource.Task;
    private readonly TaskCompletionSource<bool> _completionSource;

    /// <summary>
    /// Creates a tween between a Transform's current position and a given target position over a given period of time.
    /// </summary>
    /// <param name="duration"> The duration of the tween. </param>
    /// <param name="transform"> The Transform being moved. </param>
    /// <param name="target"> The target position of the tween. </param>
    /// <param name="easing"> The easing mode (currently not supported). </param>
    public Tween(float duration, Transform transform, Vector3 target, Easing easing = Easing.linear)
    {
        Duration = duration;
        Transform = transform;
        Target = target;
        Easing = easing;

        _completionSource = new();
        TweenLoop();
    }

    private async readonly void TweenLoop()
    {
        DateTime startTime = DateTime.Now;

        Vector3 origin = Transform.position;

        EasingFunction easing = new(Easing);

        float progress = 0;

        while (progress < 1)
        {
            await Task.Yield();

            progress += Time.deltaTime / Duration;
            progress = Mathf.Clamp01(progress);

            Transform.position = Vector3.Lerp(origin, Target, easing.Ease(progress));
        }

        Transform.position = Target;
        _completionSource.SetResult(true);
    }
}

/// <summary>
/// Moves a Transform to a given position over a given period of time.
/// </summary>
public readonly struct TweenRect
{
    /// <summary>
    /// The duration of this tween.
    /// </summary>
    public readonly float Duration;
    /// <summary>
    /// The RectTransform being controlled by this tween.
    /// </summary>
    public readonly RectTransform Rect;
    /// <summary>
    /// The target position of this tween.
    /// </summary>
    public readonly Vector3 Target;
    /// <summary>
    /// The easing mode of this tween (linear by default).
    /// </summary>
    public readonly Easing Easing;

    /// <summary>
    /// Returns a task that is completed once the tween has finished. Can be awaited in async functions.
    /// </summary>
    public readonly Task TweenCompletion => _completionSource.Task;
    private readonly TaskCompletionSource<bool> _completionSource;

    /// <summary>
    /// Creates a tween between a Transform's current position and a given target position over a given period of time.
    /// </summary>
    /// <param name="duration"> The duration of the tween. </param>
    /// <param name="rect"> The RectTransform being moved. </param>
    /// <param name="target"> The target position of the tween. </param>
    /// <param name="easing"> The easing mode (currently not supported). </param>
    public TweenRect(float duration, RectTransform rect, Vector3 target, Easing easing = Easing.linear)
    {
        Duration = duration;
        Rect = rect;
        Target = target;
        Easing = easing;

        _completionSource = new();
        TweenLoop();
    }

    private async readonly void TweenLoop()
    {
        DateTime startTime = DateTime.Now;

        Vector3 origin = Rect.anchoredPosition3D;

        EasingFunction easing = new(Easing);

        float progress = 0;

        while (progress < 1)
        {
            await Task.Yield();
            
            progress += Time.deltaTime / Duration;
            progress = Mathf.Clamp01(progress);

            Rect.anchoredPosition3D = Vector3.Lerp(origin, Target, easing.Ease(progress));
        }

        Rect.anchoredPosition3D = Target;
        _completionSource.SetResult(true);
    }
}

/// <summary>
/// Rotates a Transform to a given rotation over a given period of time.
/// </summary>
public readonly struct TweenRotation
{
    /// <summary>
    /// The duration of this tween.
    /// </summary>
    public readonly float Duration;
    /// <summary>
    /// The Transform being controlled by this tween.
    /// </summary>
    public readonly Transform Transform;
    /// <summary>
    /// The target rotation of this tween.
    /// </summary>
    public readonly Quaternion Target;
    /// <summary>
    /// The easing mode of this tween (linear by default).
    /// </summary>
    public readonly Easing Easing;

    /// <summary>
    /// Returns a task that is completed once the tween has finished. Can be awaited in async functions.
    /// </summary>
    public readonly Task TweenCompletion => _completionSource.Task;
    private readonly TaskCompletionSource<bool> _completionSource;

    /// <summary>
    /// Creates a tween between a Transform's current rotation and a given target rotation over a given period of time.
    /// </summary>
    /// <param name="duration"> The duration of the tween. </param>
    /// <param name="transform"> The Transform being rotated. </param>
    /// <param name="target"> The target rotation of the tween. </param>
    /// <param name="easing"> The easing mode (currently not supported). </param>
    public TweenRotation(float duration, Transform transform, Quaternion target, Easing easing = Easing.linear)
    {
        Duration = duration;
        Transform = transform;
        Target = target;
        Easing = easing;

        _completionSource = new();
        TweenLoop();
    }

    private async readonly void TweenLoop()
    {
        DateTime startTime = DateTime.Now;

        Quaternion origin = Transform.rotation;

        EasingFunction easing = new(Easing);

        float progress = 0;

        while (progress < 1)
        {
            await Task.Yield();
            
            progress += Time.deltaTime / Duration;
            progress = Mathf.Clamp01(progress);

            Transform.rotation = Quaternion.Slerp(origin, Target, easing.Ease(progress));
        }

        Transform.rotation = Target;
        _completionSource.SetResult(true);
    }
}

/// <summary>
/// Moves a Transform to a given position over a given period of time.
/// </summary>
public readonly struct TweenValue
{
    /// <summary>
    /// The duration of this tween.
    /// </summary>
    public readonly float Duration;
    /// <summary>
    /// The starting value of this tween.
    /// </summary>
    public readonly float Origin;
    /// <summary>
    /// The target value of this tween.
    /// </summary>
    public readonly float Target;
    /// <summary>
    /// Action where the value should be set.
    /// </summary>
    public readonly Action<float> SetValue;
    /// <summary>
    /// The easing mode of this tween (linear by default).
    /// </summary>
    public readonly Easing Easing;

    /// <summary>
    /// Returns a task that is completed once the tween has finished. Can be awaited in async functions.
    /// </summary>
    public readonly Task TweenCompletion => _completionSource.Task;
    private readonly TaskCompletionSource<bool> _completionSource;

    /// <summary>
    /// Creates a tween between a Transform's current position and a given target position over a given period of time.
    /// </summary>
    /// <param name="duration"> The duration of the tween. </param>
    /// <param name="value"> The starting value. </param>
    /// <param name="target"> The target position of the tween. </param>
    /// <param name="setValue"> The lambda expression that sets the value. </param>
    /// <param name="easing"> The easing mode (currently not supported). </param>
    public TweenValue(float duration, float origin, float target, Action<float> setValue, Easing easing = Easing.linear)
    {
        Duration = duration;
        Origin = origin;
        Target = target;
        SetValue = setValue;
        Easing = easing;

        _completionSource = new();
        TweenLoop();
    }

    private async readonly void TweenLoop()
    {
        DateTime startTime = DateTime.Now;

        EasingFunction easing = new(Easing);

        float progress = 0;

        while (progress < 1)
        {
            await Task.Yield();

            progress += Time.deltaTime / Duration;
            progress = Mathf.Clamp01(progress);

            SetValue(Mathf.Lerp(Origin, Target, easing.Ease(progress)));
        }

        SetValue(Target);
        _completionSource.SetResult(true);
    }
}


/// <summary>
/// Delays an action to after a certain period of time.
/// </summary>
public readonly struct DelayedAction
{
    /// <summary>
    /// The duration of the delay.
    /// </summary>
    public readonly float Duration;
    /// <summary>
    /// The action to be called after the delay has elapsed.
    /// </summary>
    public readonly Action Action;

    public DelayedAction(float duration, Action action)
    {
        Duration = duration;
        Action = action;

        Delay();
    }

    private async readonly void Delay()
    {
        await Task.Delay((int)(Duration * 1000));
        Action();
    }
}

public struct EasingFunction
{
    public readonly Easing EasingType;
    private Func<float, float> _easingDelegate;
    public EasingFunction(Easing easing)
    {
        EasingType = easing;
        _easingDelegate = GetEasingFunction(EasingType);
    }

    public float Ease(float value) => _easingDelegate(value);

    public static Func<float, float> GetEasingFunction(Easing easing)
    {
        switch (easing)
        {
            case Easing.linear:
                return (input) => Linear(input);
            case Easing.inSine:
                return (input) => InSine(input);
            case Easing.outSine:
                return (input) => OutSine(input);
            case Easing.inOutSine:
                return (input) => InOutSine(input);
            default:
                return null;
        }
    }
    private static float Linear(float progress)
    {
        return progress;
    }
    private static float InSine(float progress)
    {
        return 1 - Mathf.Cos(progress * Mathf.PI / 2);
    }
    private static float OutSine(float progress)
    {
        return Mathf.Sin(progress * Mathf.PI / 2);
    }
    private static float InOutSine(float progress)
    {
        return -(Mathf.Cos(Mathf.PI * progress) - 1) / 2;
    }
}

public enum Easing
{
    linear,
    inSine,
    outSine,
    inOutSine
}