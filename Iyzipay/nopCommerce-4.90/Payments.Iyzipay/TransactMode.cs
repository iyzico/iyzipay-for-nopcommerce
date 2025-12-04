using System;

namespace Nop.Plugin.Payments.Iyzipay;

/// <summary>
/// Represents transact mode enumeration
/// </summary>
public enum TransactMode
{
    /// <summary>
    /// Pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Authorize
    /// </summary>
    Authorize = 1,

    /// <summary>
    /// Capture
    /// </summary>
    Capture = 2
}
