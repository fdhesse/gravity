using System;
using System.Collections.Generic;
using System.Linq;

public static class MotionControllerExtensions
{
    public static bool HasMotionType( this MotionController controller, Type type )
    {
        return controller.AnimatedMotions.Any( motion => motion.GetType() == type );
    }

    public static AnimatedMotion GetMotion( this MotionController controller, Type type )
    {
        return controller.AnimatedMotions.FirstOrDefault( motion => motion.GetType() == type );
    }

    public static List<AnimatedMotion> GetAllMotionsOfType( this MotionController controller, Type type )
    {
        return controller.AnimatedMotions.Where( motion => motion.GetType() == type ).ToList();
    }

    public static RappelDownAnimatedMotion GetRappelingMotion( this MotionController controller, int rappelDistance )
    {
        var rappelMotions = controller.GetAllMotionsOfType( typeof(RappelDownAnimatedMotion) ).Cast<RappelDownAnimatedMotion>();
        var rappelType = (RappelDownLengthType)rappelDistance;
        foreach ( var motion in rappelMotions )
        {
            if ( motion.Type == rappelType )
            {
                return motion;
            }
        }

        return null;
    }


        public static AnimatedMotion TryGetAnimatedMotionAt( this MotionController controller, int index )
        {
            // Cache the AllConditions array.
            AnimatedMotion[] allConditions = controller.AnimatedMotions;

            // If it doesn't exist or there are null elements, return null.
            if ( allConditions == null || allConditions[0] == null )
                return null;

            // If the given index is beyond the length of the array return the first element.
            if ( index >= allConditions.Length )
                return allConditions[0];

            // Otherwise return the Condition at the given index.
            return allConditions[index];
        }
}