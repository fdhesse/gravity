using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MotionControllerExtensions
{
    public static bool HasMotionType( this MotionController controller, Type type )
    {
        if ( controller == null )
        {
            Debug.LogError( "Controller null" );
        }

        if ( controller.AnimatedMotions == null )
        {
            Debug.LogError( "AnimatedMotions null" );
        }
        foreach ( var motion in controller.AnimatedMotions )
        {
            Debug.Log( motion.Name );
        }
        return controller.AnimatedMotions.Any( motion => motion.GetType() == type );
    }

    public static AnimatedMotion GetMotion( this MotionController controller, Type type )
    {
        return controller.AnimatedMotions.FirstOrDefault( motion => motion.GetType() == type );
    }

    public static List<AnimatedMotion> GetAllMotionsOfType( this MotionController controller, Type type )
    {
        //return controller.AnimatedMotions.Where( motion => motion.GetType() == type ).ToList();
        return null;
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
}