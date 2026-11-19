using Hyjinx.Ava.Configuration;

namespace Hyjinx.Tests.Ava.Configuration;

public class ReactiveObjectTests
{
    [Test]
    public void WorksWithExpressions()
    {
        var target = new MyObject
        {
            MyValue = true
        };

        bool eventRaised = false;

        var obj = new ReactiveObject<bool>(() => target.MyValue);
        obj.Event += (_, e) =>
        {
            Assert.That(e.OldValue, Is.True);
            Assert.That(e.NewValue, Is.False);
            eventRaised = true;
        };

        Assert.That(obj.Value, Is.True);

        obj.Value = false;
        Assert.That(obj.Value, Is.False);

        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void WorksWithValues()
    {
        object value = true;
        object newValue = false;

        bool eventRaised = false;

        var obj = new ReactiveClassObject<object>(value);
        obj.Event += (_, e) =>
        {
            Assert.That(e.OldValue, Is.EqualTo(value));
            Assert.That(e.NewValue, Is.EqualTo(newValue));
            eventRaised = true;
        };

        Assert.That(obj.Value, Is.EqualTo(value));

        obj.Value = newValue;
        Assert.That(obj.Value, Is.EqualTo(newValue));

        Assert.That(eventRaised, Is.True);
    }

    private class MyObject
    {
        public bool MyValue { get; set; }
    }
}