using System;

namespace FullInspector {
    /// <summary>
    /// This allows a member to be conditionally hidden in the inspector depending upon the
    /// state of other variables in object. This does *not* change serialization behavior,
    /// only display behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class InspectorShowIfAttribute : Attribute {
        /// <summary>
        /// The name of the member to use as a condition. The conditional needs to
        /// either be a boolean field, a boolean property with a getter, or a no-argument
        /// method that returns a boolean.
        /// </summary>
        public string ConditionalMemberName;

        /// <summary>
        /// This allows a member to be conditionally hidden in the inspector depending upon the
        /// state of other variables in object. This does *not* change serialization behavior,
        /// only display behavior.
        /// </summary>
        /// <param name="conditionalMemberName">The name of the member to use as a condition.
        /// The conditional needs to either be a boolean field, a boolean property with a
        /// getter, or a no-argument method that returns a boolean.
        /// </param>
        public InspectorShowIfAttribute(string conditionalMemberName) {
            ConditionalMemberName = conditionalMemberName;
        }
    }

    /// <summary>
    /// This allows a member to be conditionally hidden in the inspector depending upon the
    /// state of other variables in object. This does *not* change serialization behavior,
    /// only display behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class InspectorHideIfAttribute : Attribute {
        /// <summary>
        /// The name of the member to use as a condition. The conditional needs to
        /// either be a boolean field, a boolean property with a getter, or a no-argument
        /// method that returns a boolean.
        /// </summary>
        public string ConditionalMemberName;

        /// <summary>
        /// This allows a member to be conditionally hidden in the inspector depending upon the
        /// state of other variables in object. This does *not* change serialization behavior,
        /// only display behavior.
        /// </summary>
        /// <param name="conditionalMemberName">The name of the member to use as a condition.
        /// The conditional needs to either be a boolean field, a boolean property with a
        /// getter, or a no-argument method that returns a boolean.
        /// </param>
        public InspectorHideIfAttribute(string conditionalMemberName) {
            ConditionalMemberName = conditionalMemberName;
        }
    }
}