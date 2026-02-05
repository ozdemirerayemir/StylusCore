namespace StylusCore.Core.Actions
{
    /// <summary>
    /// Base interface for action payloads.
    /// Payloads carry parameters for parameterized actions.
    /// </summary>
    public interface IActionPayload { }

    /// <summary>
    /// Payload for page navigation actions that specify a target page.
    /// </summary>
    public readonly record struct PageGotoPayload(int PageNumber) : IActionPayload;

    /// <summary>
    /// Payload for zoom actions with a specific zoom level.
    /// </summary>
    public readonly record struct ZoomPayload(double ZoomLevel) : IActionPayload;

    /// <summary>
    /// Payload for shape tool selection specifying shape type.
    /// </summary>
    public readonly record struct ShapeTypePayload(string ShapeType) : IActionPayload;

    /// <summary>
    /// Payload for tool parameter changes (e.g., pen width, color).
    /// </summary>
    public readonly record struct ToolParameterPayload(
        string ParameterName,
        object Value
    ) : IActionPayload;
}
