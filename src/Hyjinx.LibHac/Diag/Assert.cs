using LibHac.Common;
using LibHac.Diag.Impl;
using LibHac.Os;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LibHac.Diag;

public ref struct AssertionInfo
{
    public AssertionType AssertionType;
    public string Message;
    public string Condition;
    public string FunctionName;
    public string FileName;
    public int LineNumber;
}

public enum AssertionType
{
    SdkAssert,
    SdkRequires,
    UserAssert
}

public enum AssertionFailureOperation
{
    Abort,
    Continue
}

public delegate AssertionFailureOperation AssertionFailureHandler(in AssertionInfo assertionInfo);

public static class Assert
{
    private const string AssertCondition = "ENABLE_ASSERTS";

    private static SdkMutexType _mutex = new SdkMutexType();
    private static AssertionFailureHandler _assertionFailureHandler = DefaultAssertionFailureHandler;

    private static AbortReason ToAbortReason(AssertionType assertionType)
    {
        switch (assertionType)
        {
            case AssertionType.SdkAssert:
                return AbortReason.SdkAssert;
            case AssertionType.SdkRequires:
                return AbortReason.SdkRequires;
            case AssertionType.UserAssert:
                return AbortReason.UserAssert;
            default:
                return AbortReason.Abort;
        }
    }

    private static AssertionFailureOperation DefaultAssertionFailureHandler(in AssertionInfo assertionInfo)
    {
        return AssertionFailureOperation.Abort;
    }

    private static void ExecuteAssertionFailureOperation(AssertionFailureOperation operation,
        in AssertionInfo assertionInfo)
    {
        switch (operation)
        {
            case AssertionFailureOperation.Abort:
                var abortInfo = new AbortInfo
                {
                    AbortReason = ToAbortReason(assertionInfo.AssertionType),
                    Message = assertionInfo.Message,
                    Condition = assertionInfo.Condition,
                    FunctionName = assertionInfo.FunctionName,
                    FileName = assertionInfo.FileName,
                    LineNumber = assertionInfo.LineNumber
                };

                Abort.InvokeAbortObserver(in abortInfo);
                Abort.DoAbort(abortInfo.Message);
                break;
            case AssertionFailureOperation.Continue:
                return;
            default:
                Abort.DoAbort("Unknown AssertionFailureOperation");
                break;
        }
    }

    private static void InvokeAssertionFailureHandler(in AssertionInfo assertionInfo)
    {
        AssertionFailureOperation operation = _assertionFailureHandler(in assertionInfo);
        ExecuteAssertionFailureOperation(operation, in assertionInfo);
    }

    internal static void OnAssertionFailure(AssertionType assertionType, string condition, string functionName,
        string fileName, int lineNumber)
    {
        OnAssertionFailure(assertionType, condition, functionName, fileName, lineNumber, string.Empty);
    }

    internal static void OnAssertionFailure(AssertionType assertionType, string condition, string functionName,
        string fileName, int lineNumber, string message)
    {
        // Invalidate the IPC message buffer and call SynchronizePreemptionState if necessary
        // nn::diag::detail::PrepareAbort();

        if (_mutex.IsLockedByCurrentThread())
            Abort.DoAbort();

        using ScopedLock<SdkMutexType> lk = ScopedLock.Lock(ref _mutex);

        var assertionInfo = new AssertionInfo
        {
            AssertionType = assertionType,
            Message = message,
            Condition = condition,
            FunctionName = functionName,
            FileName = fileName,
            LineNumber = lineNumber
        };

        InvokeAssertionFailureHandler(in assertionInfo);
    }

    public static void SetAssertionFailureHandler(AssertionFailureHandler assertionHandler)
    {
        _assertionFailureHandler = assertionHandler;
    }

    // ---------------------------------------------------------------------
    // True
    // ---------------------------------------------------------------------

    private static void TrueImpl(AssertionType assertionType, bool condition, string conditionText, string message,
        string functionName, string fileName, int lineNumber)
    {
        if (condition)
            return;

        OnAssertionFailure(assertionType, conditionText, functionName, fileName, lineNumber, message);
    }

    [Conditional(AssertCondition)]
    public static void True([DoesNotReturnIf(false)] bool condition,
        string message = "",
        [CallerArgumentExpression(nameof(condition))] string conditionText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        TrueImpl(AssertionType.UserAssert, condition, conditionText, message, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void SdkAssert([DoesNotReturnIf(false)] bool condition,
        string message = "",
        [CallerArgumentExpression(nameof(condition))] string conditionText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        TrueImpl(AssertionType.SdkAssert, condition, conditionText, message, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void SdkRequires([DoesNotReturnIf(false)] bool condition,
        string message = "",
        [CallerArgumentExpression(nameof(condition))] string conditionText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        TrueImpl(AssertionType.SdkRequires, condition, conditionText, message, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not null
    // ---------------------------------------------------------------------

    private static void NotNullImpl<T>(AssertionType assertionType, [NotNull] T value, string valueText,
        string functionName, string fileName, int lineNumber) where T : class
    {
        if (AssertImpl.NotNull(value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void NotNull<T>([NotNull] T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class
    {
        NotNullImpl(AssertionType.UserAssert, value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNotNull<T>([NotNull] T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class
    {
        NotNullImpl(AssertionType.SdkAssert, value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNotNull<T>([NotNull] T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class
    {
        NotNullImpl(AssertionType.SdkRequires, value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not null ref
    // ---------------------------------------------------------------------

    private static void NotNullImpl<T>(AssertionType assertionType, [NotNull] ref T value, string valueText,
        string functionName, string fileName, int lineNumber)
    {
        if (AssertImpl.NotNull(ref value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void NotNull<T>([NotNull] ref T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.UserAssert, ref value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNotNull<T>([NotNull] ref T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.SdkAssert, ref value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNotNull<T>([NotNull] ref T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.SdkRequires, ref value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not null out
    // ---------------------------------------------------------------------

    private static void NotNullOutImpl<T>(AssertionType assertionType, [NotNull] out T value, string valueText,
        string functionName, string fileName, int lineNumber)
    {
        Unsafe.SkipInit(out value);

#if ENABLE_ASSERTS
        if (!Unsafe.IsNullRef(ref value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
#endif
    }

    public static void NotNullOut<T>([NotNull] out T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullOutImpl(AssertionType.UserAssert, out value, valueText, functionName, fileName, lineNumber);
    }

    internal static void SdkNotNullOut<T>([NotNull] out T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullOutImpl(AssertionType.SdkAssert, out value, valueText, functionName, fileName, lineNumber);
    }

    internal static void SdkRequiresNotNullOut<T>([NotNull] out T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullOutImpl(AssertionType.SdkRequires, out value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not null span
    // ---------------------------------------------------------------------

    private static void NotNullImpl<T>(AssertionType assertionType, Span<T> value,
        string valueText, string functionName, string fileName, int lineNumber)
    {
        if (AssertImpl.NotNull(value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void NotNull<T>(Span<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.UserAssert, value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNotNull<T>(Span<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.SdkAssert, value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNotNull<T>(Span<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.SdkRequires, value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not null read-only span
    // ---------------------------------------------------------------------

    private static void NotNullImpl<T>(AssertionType assertionType, ReadOnlySpan<T> value,
        string valueText, string functionName, string fileName, int lineNumber)
    {
        if (AssertImpl.NotNull(value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void NotNull<T>(ReadOnlySpan<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.UserAssert, value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNotNull<T>(ReadOnlySpan<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.SdkAssert, value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNotNull<T>(ReadOnlySpan<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NotNullImpl(AssertionType.SdkRequires, value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not null UniqueRef<T>
    // ---------------------------------------------------------------------

    private static void NotNullImpl<T>(AssertionType assertionType, in UniqueRef<T> value,
        string valueText, string functionName, string fileName, int lineNumber) where T : class, IDisposable
    {
        if (AssertImpl.NotNull(in value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void NotNull<T>(in UniqueRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NotNullImpl(AssertionType.UserAssert, in value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNotNull<T>(in UniqueRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NotNullImpl(AssertionType.SdkAssert, in value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNotNull<T>(in UniqueRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NotNullImpl(AssertionType.SdkRequires, in value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not null SharedRef<T>
    // ---------------------------------------------------------------------

    private static void NotNullImpl<T>(AssertionType assertionType, in SharedRef<T> value,
        string valueText, string functionName, string fileName, int lineNumber) where T : class, IDisposable
    {
        if (AssertImpl.NotNull(in value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void NotNull<T>(in SharedRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NotNullImpl(AssertionType.UserAssert, in value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNotNull<T>(in SharedRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NotNullImpl(AssertionType.SdkAssert, in value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNotNull<T>(in SharedRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NotNullImpl(AssertionType.SdkRequires, in value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Null
    // ---------------------------------------------------------------------

    private static void NullImpl<T>(AssertionType assertionType, T value, string valueText,
        string functionName, string fileName, int lineNumber) where T : class
    {
        if (AssertImpl.Null(value))
            return;

        AssertImpl.InvokeAssertionNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Null<T>(T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class
    {
        NullImpl(AssertionType.UserAssert, value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNull<T>(T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class
    {
        NullImpl(AssertionType.SdkAssert, value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNull<T>(T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class
    {
        NullImpl(AssertionType.SdkRequires, value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Null ref
    // ---------------------------------------------------------------------

    private static void NullImpl<T>(AssertionType assertionType, ref T value, string valueText,
        string functionName, string fileName, int lineNumber)
    {
        if (AssertImpl.Null(ref value))
            return;

        AssertImpl.InvokeAssertionNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Null<T>(ref T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NullImpl(AssertionType.UserAssert, ref value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNull<T>(ref T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NullImpl(AssertionType.SdkAssert, ref value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNull<T>(ref T value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        NullImpl(AssertionType.SdkRequires, ref value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Null UniqueRef<T>
    // ---------------------------------------------------------------------

    private static void NullImpl<T>(AssertionType assertionType, in UniqueRef<T> value,
        string valueText, string functionName, string fileName, int lineNumber) where T : class, IDisposable
    {
        if (AssertImpl.Null(in value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Null<T>(in UniqueRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NullImpl(AssertionType.UserAssert, in value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNull<T>(in UniqueRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NullImpl(AssertionType.SdkAssert, in value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNull<T>(in UniqueRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NullImpl(AssertionType.SdkRequires, in value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Null SharedRef<T>
    // ---------------------------------------------------------------------

    private static void NullImpl<T>(AssertionType assertionType, in SharedRef<T> value,
        string valueText, string functionName, string fileName, int lineNumber) where T : class, IDisposable
    {
        if (AssertImpl.Null(in value))
            return;

        AssertImpl.InvokeAssertionNotNull(assertionType, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Null<T>(in SharedRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NullImpl(AssertionType.UserAssert, in value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNull<T>(in SharedRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NullImpl(AssertionType.SdkAssert, in value, valueText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNull<T>(in SharedRef<T> value,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : class, IDisposable
    {
        NullImpl(AssertionType.SdkRequires, in value, valueText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // In range
    // ---------------------------------------------------------------------

    private static void InRangeImpl<T>(AssertionType assertionType, T value, T lower, T upper, string valueText,
        string lowerText, string upperText, string functionName, string fileName, int lineNumber)
        where T : IComparisonOperators<T, T, bool>
    {
        if (AssertImpl.WithinRange(value, lower, upper))
            return;

        AssertImpl.InvokeAssertionInRange(assertionType, value, lower, upper, valueText, lowerText, upperText, functionName,
            fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void InRange<T>(T value, T lowerInclusive, T upperExclusive,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(lowerInclusive))] string lowerInclusiveText = "",
        [CallerArgumentExpression(nameof(upperExclusive))] string upperExclusiveText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparisonOperators<T, T, bool>
    {
        InRangeImpl(AssertionType.UserAssert, value, lowerInclusive, upperExclusive, valueText, lowerInclusiveText,
            upperExclusiveText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkInRange<T>(T value, T lowerInclusive, T upperExclusive,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(lowerInclusive))] string lowerInclusiveText = "",
        [CallerArgumentExpression(nameof(upperExclusive))] string upperExclusiveText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparisonOperators<T, T, bool>
    {
        InRangeImpl(AssertionType.SdkAssert, value, lowerInclusive, upperExclusive, valueText, lowerInclusiveText,
            upperExclusiveText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresInRange<T>(T value, T lowerInclusive, T upperExclusive,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(lowerInclusive))] string lowerInclusiveText = "",
        [CallerArgumentExpression(nameof(upperExclusive))] string upperExclusiveText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparisonOperators<T, T, bool>
    {
        InRangeImpl(AssertionType.SdkRequires, value, lowerInclusive, upperExclusive, valueText, lowerInclusiveText,
            upperExclusiveText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Within min-max
    // ---------------------------------------------------------------------

    private static void WithinMinMaxImpl<T>(AssertionType assertionType, T value, T min, T max, string valueText,
        string minText, string maxText, string functionName, string fileName, int lineNumber)
        where T : IComparisonOperators<T, T, bool>
    {
        if (AssertImpl.WithinMinMax(value, min, max))
            return;

        AssertImpl.InvokeAssertionWithinMinMax(assertionType, value, min, max, valueText, minText, maxText, functionName,
            fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void WithinMinMax<T>(T value, T min, T max,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(min))] string minText = "",
        [CallerArgumentExpression(nameof(max))] string maxText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparisonOperators<T, T, bool>
    {
        WithinMinMaxImpl(AssertionType.UserAssert, value, min, max, valueText, minText, maxText, functionName,
            fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkWithinMinMax<T>(T value, T min, T max,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(min))] string minText = "",
        [CallerArgumentExpression(nameof(max))] string maxText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparisonOperators<T, T, bool>
    {
        WithinMinMaxImpl(AssertionType.SdkAssert, value, min, max, valueText, minText, maxText, functionName,
            fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresWithinMinMax<T>(T value, T min, T max,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(min))] string minText = "",
        [CallerArgumentExpression(nameof(max))] string maxText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparisonOperators<T, T, bool>
    {
        WithinMinMaxImpl(AssertionType.SdkRequires, value, min, max, valueText, minText, maxText, functionName,
            fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Equal
    // ---------------------------------------------------------------------

    private static void EqualImpl<T>(AssertionType assertionType, T lhs, T rhs, string lhsText, string rhsText,
        string functionName, string fileName, int lineNumber) where T : IEquatable<T>
    {
        if (AssertImpl.Equal(lhs, rhs))
            return;

        AssertImpl.InvokeAssertionEqual(assertionType, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Equal<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        EqualImpl(AssertionType.UserAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        EqualImpl(AssertionType.SdkAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        EqualImpl(AssertionType.SdkRequires, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Equal ref
    // ---------------------------------------------------------------------

    private static void EqualImpl<T>(AssertionType assertionType, ref T lhs, ref T rhs, string lhsText,
        string rhsText, string functionName, string fileName, int lineNumber) where T : IEquatable<T>
    {
        if (AssertImpl.Equal(ref lhs, ref rhs))
            return;

        AssertImpl.InvokeAssertionEqual(assertionType, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Equal<T>(ref T lhs, ref T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        EqualImpl(AssertionType.UserAssert, ref lhs, ref rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkEqual<T>(ref T lhs, ref T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        EqualImpl(AssertionType.SdkAssert, ref lhs, ref rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresEqual<T>(ref T lhs, ref T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        EqualImpl(AssertionType.SdkRequires, ref lhs, ref rhs, lhsText, rhsText, functionName, fileName,
            lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not equal
    // ---------------------------------------------------------------------

    private static void NotEqualImpl<T>(AssertionType assertionType, T lhs, T rhs, string lhsText, string rhsText,
        string functionName, string fileName, int lineNumber) where T : IEquatable<T>
    {
        if (AssertImpl.NotEqual(lhs, rhs))
            return;

        AssertImpl.InvokeAssertionNotEqual(assertionType, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void NotEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        NotEqualImpl(AssertionType.UserAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNotEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        NotEqualImpl(AssertionType.SdkAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNotEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        NotEqualImpl(AssertionType.SdkRequires, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Not equal ref
    // ---------------------------------------------------------------------

    private static void NotEqualImpl<T>(AssertionType assertionType, ref T lhs, ref T rhs, string lhsText,
        string rhsText, string functionName, string fileName, int lineNumber) where T : IEquatable<T>
    {
        if (AssertImpl.NotEqual(ref lhs, ref rhs))
            return;

        AssertImpl.InvokeAssertionNotEqual(assertionType, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void NotEqual<T>(ref T lhs, ref T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        NotEqualImpl(AssertionType.UserAssert, ref lhs, ref rhs, lhsText, rhsText, functionName, fileName,
            lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkNotEqual<T>(ref T lhs, ref T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        NotEqualImpl(AssertionType.SdkAssert, ref lhs, ref rhs, lhsText, rhsText, functionName, fileName,
            lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresNotEqual<T>(ref T lhs, ref T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IEquatable<T>
    {
        NotEqualImpl(AssertionType.SdkRequires, ref lhs, ref rhs, lhsText, rhsText, functionName, fileName,
            lineNumber);
    }

    // ---------------------------------------------------------------------
    // Less
    // ---------------------------------------------------------------------

    private static void LessImpl<T>(AssertionType assertionType, T lhs, T rhs, string lhsText, string rhsText,
        string functionName, string fileName, int lineNumber) where T : IComparable<T>
    {
        if (AssertImpl.Less(lhs, rhs))
            return;

        AssertImpl.InvokeAssertionLess(assertionType, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Less<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        LessImpl(AssertionType.UserAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkLess<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        LessImpl(AssertionType.SdkAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresLess<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        LessImpl(AssertionType.SdkRequires, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Less equal
    // ---------------------------------------------------------------------

    private static void LessEqualImpl<T>(AssertionType assertionType, T lhs, T rhs, string lhsText, string rhsText,
        string functionName, string fileName, int lineNumber) where T : IComparable<T>
    {
        if (AssertImpl.LessEqual(lhs, rhs))
            return;

        AssertImpl.InvokeAssertionLessEqual(assertionType, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void LessEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        LessEqualImpl(AssertionType.UserAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkLessEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        LessEqualImpl(AssertionType.SdkAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresLessEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        LessEqualImpl(AssertionType.SdkRequires, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Greater
    // ---------------------------------------------------------------------

    private static void GreaterImpl<T>(AssertionType assertionType, T lhs, T rhs, string lhsText, string rhsText,
        string functionName, string fileName, int lineNumber) where T : IComparable<T>
    {
        if (AssertImpl.Greater(lhs, rhs))
            return;

        AssertImpl.InvokeAssertionGreater(assertionType, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Greater<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        GreaterImpl(AssertionType.UserAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkGreater<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        GreaterImpl(AssertionType.SdkAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresGreater<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        GreaterImpl(AssertionType.SdkRequires, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Greater equal
    // ---------------------------------------------------------------------

    private static void GreaterEqualImpl<T>(AssertionType assertionType, T lhs, T rhs, string lhsText,
        string rhsText, string functionName, string fileName, int lineNumber) where T : IComparable<T>
    {
        if (AssertImpl.GreaterEqual(lhs, rhs))
            return;

        AssertImpl.InvokeAssertionGreaterEqual(assertionType, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void GreaterEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        GreaterEqualImpl(AssertionType.UserAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkGreaterEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        GreaterEqualImpl(AssertionType.SdkAssert, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresGreaterEqual<T>(T lhs, T rhs,
        [CallerArgumentExpression(nameof(lhs))] string lhsText = "",
        [CallerArgumentExpression(nameof(rhs))] string rhsText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IComparable<T>
    {
        GreaterEqualImpl(AssertionType.SdkRequires, lhs, rhs, lhsText, rhsText, functionName, fileName, lineNumber);
    }

    // ---------------------------------------------------------------------
    // Aligned
    // ---------------------------------------------------------------------

    private static void AlignedImpl<T>(AssertionType assertionType, T value, int alignment, string valueText,
        string alignmentText, string functionName, string fileName, int lineNumber) where T : IBinaryNumber<T>
    {
        if (AssertImpl.IsAligned(value, alignment))
            return;

        AssertImpl.InvokeAssertionAligned(assertionType, value, alignment, valueText, alignmentText, functionName, fileName,
            lineNumber);
    }

    [Conditional(AssertCondition)]
    public static void Aligned<T>(T value, int alignment,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(alignment))] string alignmentText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IBinaryNumber<T>
    {
        AlignedImpl(AssertionType.UserAssert, value, alignment, valueText, alignmentText, functionName, fileName,
            lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkAligned<T>(T value, int alignment,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(alignment))] string alignmentText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IBinaryNumber<T>
    {
        AlignedImpl(AssertionType.SdkAssert, value, alignment, valueText, alignmentText, functionName, fileName,
            lineNumber);
    }

    [Conditional(AssertCondition)]
    internal static void SdkRequiresAligned<T>(T value, int alignment,
        [CallerArgumentExpression(nameof(value))] string valueText = "",
        [CallerArgumentExpression(nameof(alignment))] string alignmentText = "",
        [CallerMemberName] string functionName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = 0)
        where T : IBinaryNumber<T>
    {
        AlignedImpl(AssertionType.SdkRequires, value, alignment, valueText, alignmentText, functionName, fileName,
            lineNumber);
    }
}