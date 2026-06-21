
using System;
using UnityEngine;

/// <summary>
/// Utility class for common mathmatical operations and conversions
/// </summary>
public static class MyMaths {
    
    /// <summary>
    /// Converts a slider ranging from 0-1 to -80db - 0db
    /// </summary>
    public static float DecibelsFrom01Volume(float volume01)
    {
        return Mathf.Log10(Mathf.Max(0.0001f, volume01) * 20.0f);
    }

    public static float RadiansToDegrees(float radians)
    {
        return radians * Mathf.Rad2Deg;
    }
    
    public static float DegreesToRadians(float degrees)
    {
        return degrees * Mathf.Deg2Rad;
    }
    
    /// <summary>
    /// Converts the direction of a 2D vector to an angle in radians
    /// </summary>
    public static float Vector2ToRadians(Vector2 vector)
    {
        return Mathf.Atan2(vector.y, vector.x);
    }
    /// <summary>
    /// Converts the direction of a 2D vector to an angle in degrees
    /// </summary>
    public static float Vector2ToDegrees(Vector2 vector)
    {
        return RadiansToDegrees(Vector2ToRadians(vector));
    }
    
    /// <summary>
    /// Converts an angle in radians to the 2D vector direction equivelant 
    /// </summary>
    public static Vector2 RadiansToVector2(float radians)
    {
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    /// <summary>
    /// Converts an angle in degrees to the 2D vector direction equivelant 
    /// </summary>
    public static Vector2 DegreesToVector2(float degrees)
    {
        return RadiansToVector2(DegreesToRadians(degrees));
    }

};