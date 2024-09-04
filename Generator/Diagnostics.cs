using Microsoft.CodeAnalysis;

namespace Generator;

internal class Diagnostics
{
    public static DiagnosticDescriptor MethodMustBePartial = new(
        id: "DEMO001",
        title: "Demo method must be partial and not contain a body",
        messageFormat: "Method '{0}' must be declared as partial with no method body",
        category: "Demo",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
