using System;
using System.Dynamic;

namespace Ufex.API;

/// <summary>
/// Represents a specific offset within a file.
/// </summary>
/// <param name="Value"></param>
public readonly record struct FileOffset(long Value);
