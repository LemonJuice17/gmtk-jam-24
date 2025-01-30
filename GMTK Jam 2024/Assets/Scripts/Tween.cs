using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

/// <summary>
/// Moves a transform to a given position over a given period of time.
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
    /// <param name="transform"> The Tranform being moved. </param>
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

        while (Transform.position != Target)
        {
            await Task.Yield();

            float progress = ((float)(DateTime.Now - startTime).TotalSeconds) / Duration;

            Transform.position = Vector3.Lerp(origin, Target, progress);
        }

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

public enum Easing
{
    linear,
    easeIn,
    easeOut,
    easeInOut
}