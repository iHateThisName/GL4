using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Non-generic base class for all runtime reference ScriptableObjects.
/// Use this as the serialized field type when you don't need to know the concrete type,
/// or when a script just needs "some runtime ref" without caring about what it holds.
/// </summary>
public abstract class SO_RuntimeRef : SO_RuntimeScriptableObject
{
    /// <summary>Untyped read access. Cast the result or use the typed subclass.</summary>
    public abstract object GetValue();

    /// <summary>Untyped write access. Value must be assignable to the concrete type.</summary>
    public abstract void SetValue(object value);

    /// <summary>Clears the reference to null/empty.</summary>
    public abstract void ClearValue();
}

/// <summary>
/// Typed runtime reference holding a single value of type T.
/// Components self-register (set Value in Awake), consumers read Value.
/// Decouples producers from consumers — neither needs to know about the other.
/// </summary>
/// <typeparam name="T">The reference type to hold.</typeparam>
public abstract class SO_RuntimeRef<T> : SO_RuntimeRef where T : class
{
    [System.NonSerialized] private T value;

    public T Value
    {
        get => this.value;
        set
        {
            this.value = value;
            NotifyDataChanged();
        }
    }

    public override object GetValue() => this.value;
    public override void SetValue(object value) => Value = value as T;
    public override void ClearValue() => Value = null;

    public static implicit operator T(SO_RuntimeRef<T> runtimeRef) =>
        runtimeRef != null ? runtimeRef.value : null;

    protected override void OnReset() => this.value = null;
}

/// <summary>
/// Typed runtime reference holding a collection of T.
/// Components register/deregister themselves. Use Add/Remove instead of Set.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public abstract class SO_RuntimeCollection<T> : SO_RuntimeRef where T : class
{
    [System.NonSerialized] private List<T> items = new();

    public List<T> Items => this.items;
    public int Count => this.items.Count;

    public void Add(T item)
    {
        this.items.Add(item);
        NotifyDataChanged();
    }

    public void Remove(T item)
    {
        this.items.Remove(item);
        NotifyDataChanged();
    }

    public override object GetValue() => this.items;
    public override void SetValue(object value) { } // Collections use Add/Remove
    public override void ClearValue()
    {
        this.items.Clear();
        NotifyDataChanged();
    }

    protected override void OnReset()
    {
        if (this.items == null)
            this.items = new List<T>();
        else
            this.items.Clear();
    }
}
