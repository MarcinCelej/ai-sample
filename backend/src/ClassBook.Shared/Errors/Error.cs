namespace ClassBook.Shared.Errors;

/// <summary>
/// An expected business failure. <see cref="Code"/> is stable and translated by the frontend;
/// <see cref="Message"/> is English and meant for developers.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type);
