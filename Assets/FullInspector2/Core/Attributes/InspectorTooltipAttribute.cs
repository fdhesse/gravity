using System;

namespace FullInspector {
    /// <summary>
    /// Adds a tooltip to an field or property that is viewable in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class InspectorTooltipAttribute : Attribute {
        public string Tooltip;

        public InspectorTooltipAttribute(string tooltip) {
            Tooltip = tooltip;
        }
    }
}