using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntUnityEvent : UnityEvent<int> { }

public static class GameEvents
{
    public static IntUnityEvent OnBlocksRemoved = new IntUnityEvent();
}
