using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObservable<T>
{
    public void Subscribe(IObserver<T> observer);
    public void Unsubscribe(IObserver<T> observer);
    public void Notify(T value);
}
