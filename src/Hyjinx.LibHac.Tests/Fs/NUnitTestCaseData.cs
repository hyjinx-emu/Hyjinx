namespace LibHac.Tests.Fs;

public sealed class NUnitTestCaseData<T1, T2, T3> : List<TestCaseData>
{
    public void Add(T1 value1, T2 value2, T3 value3) => Add(new TestCaseData(value1, value2, value3));
}

public sealed class NUnitTestCaseData<T1, T2, T3, T4> : List<TestCaseData>
{
    public void Add(T1 value1, T2 value2, T3 value3, T4 value4) =>
        Add(new TestCaseData(value1, value2, value3, value4));
}

public sealed class NUnitTestCaseData<T1, T2, T3, T4, T5> : List<TestCaseData>
{
    public void Add(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) =>
        Add(new TestCaseData(value1, value2, value3, value4, value5));
}

public sealed class NUnitTestCaseData<T1, T2, T3, T4, T5, T6, T7> : List<TestCaseData>
{
    public void Add(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) =>
        Add(new TestCaseData(value1, value2, value3, value4, value5, value6, value7));
}