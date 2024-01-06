using UnityEngine;

namespace Car
{
    public struct WayPoint
    {
        public Vector3 Position;
    }
    
    public static class CarHelpers
    {
        public static WayPoint[] GenerateStraightPath(Vector3 start, Vector3 target, float noiseStrength, int points)
        {
            WayPoint[] wayPoints = new WayPoint[points];

            for (int i = 0; i < points; i++)
            {
                float posInPath = (float)i/(points-1);

                Vector3 pos = Vector3.Lerp(start, target, posInPath);

                pos.z += Random.Range(-1f, 1f) * noiseStrength;

                wayPoints[i].Position = pos;
            }
            
            return wayPoints;
        }
    }
}
