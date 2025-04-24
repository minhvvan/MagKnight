using System;
using System.Collections.Generic;
using hvvan;
using Moon;
using UnityEngine;

public class ClearRoomField : MonoBehaviour, IObservable<bool>
{
    private List<IObserver<bool>> _observers = new List<IObserver<bool>>();
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            Notify(true);
        }
    }

    public void Subscribe(IObserver<bool> observer)
    {
        if (_observers.Contains(observer)) return;
        _observers.Add(observer);
    }

    public void Unsubscribe(IObserver<bool> observer)
    {
        if (!_observers.Contains(observer)) return;
        _observers.Remove(observer);
    }

    public void Notify(bool reached)
    {
        _observers.ForEach(observer => observer.OnNext(reached));
    }
}
