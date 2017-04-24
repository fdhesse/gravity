using System;
using System.Collections.Generic;
using System.Linq;

public static class PawnExtensions
{
    public static bool HasMotionType( this Pawn pawn, Type type )
    {
        return pawn.AnimatedMotions.Any( motion => motion.GetType() == type );
    }

    public static AnimatedMotion GetMotion( this Pawn pawn, Type type )
    {
        return pawn.AnimatedMotions.FirstOrDefault( motion => motion.GetType() == type );
    }

    public static List<AnimatedMotion> GetAllMotionsOfType( this Pawn pawn, Type type )
    {
        return pawn.AnimatedMotions.Where( motion => motion.GetType() == type ).ToList();
    }

    public static RappelDownAnimatedMotion GetRappelingMotion( this Pawn pawn, int rappelDistance )
    {
        var rappelMotions = pawn.GetAllMotionsOfType( typeof(RappelDownAnimatedMotion) ).Cast<RappelDownAnimatedMotion>();
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