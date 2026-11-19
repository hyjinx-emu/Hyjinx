using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Hyjinx.Ava.Configuration;

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

    public ReactiveObject(T value)
    {
        _accessor = new ValueAccessor(value);
    }

    /// <summary>
    /// A mechanism to abstract how the values are interacted with by the reactive object.
    /// </summary>
    private interface IValueAccessor
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public Func<T> Get { get; }

        /// <summary>
        /// Sets the value.
        /// </summary>
        public Action<T> Set { get; }
    }

    /// <summary>
    /// An <see cref="IValueAccessor"/> which directly accesses a locally held value within the accessor.
    /// </summary>
    private class ValueAccessor : IValueAccessor
    {
        private T currentValue;

        public Func<T> Get { get; }
        public Action<T> Set { get; }

        public ValueAccessor(T value)
        {
            currentValue = value;

            Get = () => currentValue;
            Set = (newValue) => currentValue = newValue;
        }
    }

    /// <summary>
    /// An <see cref="IValueAccessor"/> which indirectly accesses the value by using lambda expressions to locate and set the real value.
    /// </summary>
    private class LambdaExpressionAccessor : IValueAccessor
    {
        public Func<T> Get { get; }
        public Action<T> Set { get; }

        public LambdaExpressionAccessor(Func<T> getter, Action<T> setter)
        {
            Get = getter;
            Set = setter;
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