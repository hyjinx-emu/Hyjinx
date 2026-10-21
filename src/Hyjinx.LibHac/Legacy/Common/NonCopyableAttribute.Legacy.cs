#if IS_LEGACY_ENABLED

using System;

namespace LibHac.Common;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class NonCopyableAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Struct)]
public sealed class NonCopyableDisposableAttribute : Attribute { }

#endif