using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Hyjinx.Ava.Configuration;

public class ReactiveObject<T>
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    private bool _isInitialized;
    private readonly PropertyAccessor _accessor;

    public event EventHandler<ReactiveEventArgs<T>> Event;

    public ReactiveObject(Expression<Func<T>> expression)
    {
        var member = (MemberExpression)expression.Body;
        var property = (PropertyInfo)member.Member;

        var propertyExpression = Expression.Property(member.Expression, property);
        var value = Expression.Parameter(typeof(T), "value");

        _accessor = new PropertyAccessor(Expression.Lambda<Func<T>>(propertyExpression).Compile(),
            Expression.Lambda<Action<T>>(Expression.Assign(propertyExpression, value), value).Compile());
    }

    public ReactiveObject(T value)
    {
        _accessor = new PropertyAccessor(() => value, _ => { });
    }

    private class PropertyAccessor
    {
        public Func<T> Get { get; }
        public Action<T> Set { get; }

        public PropertyAccessor(Func<T> getter, Action<T> setter)
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
            if (!oldIsInitialized || oldValue == null || !oldValue.Equals(value))
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