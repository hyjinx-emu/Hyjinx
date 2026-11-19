using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// A reactive object which requires a reference type and raises events when the value changes.
/// </summary>
/// <typeparam name="T">The type of value.</typeparam>
public class ReactiveClassObject<T> : ReactiveObject<T>
    where T : class
{
    public ReactiveClassObject(T value) : base(new ValueAccessor(value)) { }

    /// <summary>
    /// An <see cref="IValueAccessor"/> which directly accesses a locally held value within the accessor.
    /// </summary>
    private class ValueAccessor : IValueAccessor
    {
        private T currentValue;

        public T Get()
        {
            return currentValue;
        }

        public void Set(T value)
        {
            currentValue = value;
        }

        public ValueAccessor(T value)
        {
            currentValue = value;
        }
    }
}

/// <summary>
/// A reactive object which raises events when the value changes.
/// </summary>
/// <typeparam name="T">The type of value.</typeparam>
public class ReactiveObject<T>
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    private bool _isInitialized;
    private readonly IValueAccessor _accessor;

    public event EventHandler<ReactiveEventArgs<T>> Event;

    public ReactiveObject(Expression<Func<T>> expression)
    {
        var member = (MemberExpression)expression.Body;
        var property = (PropertyInfo)member.Member;

        var propertyExpression = Expression.Property(member.Expression, property);
        var value = Expression.Parameter(typeof(T), "value");

        _accessor = new LambdaExpressionAccessor(Expression.Lambda<Func<T>>(propertyExpression).Compile(),
            Expression.Lambda<Action<T>>(Expression.Assign(propertyExpression, value), value).Compile());
    }

    protected ReactiveObject(IValueAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    /// A mechanism to abstract how the values are interacted with by the reactive object.
    /// </summary>
    protected interface IValueAccessor
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Get();

        /// <summary>
        /// Sets the value.
        /// </summary>
        public void Set(T value);
    }

    /// <summary>
    /// An <see cref="IValueAccessor"/> which indirectly accesses the value by using lambda expressions to locate and set the real value.
    /// </summary>
    private class LambdaExpressionAccessor : IValueAccessor
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;

        public T Get()
        {
            return getter();
        }

        public void Set(T value)
        {
            setter(value);
        }

        public LambdaExpressionAccessor(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }
    }

    public T Value
    {
        get
        {
            try
            {
                _readerWriterLock.EnterReadLock();
                return _accessor.Get();
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }
        set
        {
            var oldValue = Value;
            bool oldIsInitialized = _isInitialized;

            try
            {
                _readerWriterLock.EnterWriteLock();

                _accessor.Set(value);
                _isInitialized = true;
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }

            // The value wasn't already initialized, or it has been modified raise the event.
            if (!oldIsInitialized || oldValue == null || !EqualityComparer<T>.Default.Equals(value, oldValue))
            {
                Event?.Invoke(this, new ReactiveEventArgs<T>(oldValue, value));
            }
        }
    }

    public static implicit operator T(ReactiveObject<T> obj)
    {
        return obj.Value;
    }
}

public class ReactiveEventArgs<T>
{
    public T OldValue { get; }
    public T NewValue { get; }

    public ReactiveEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}