using System;
using Zeta.Common;
using Zeta.Game.Internals;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    class MathUtil
    {
        /* Stare at this for a while:
         * http://upload.wikimedia.org/wikipedia/commons/9/9a/Degree-Radian_Conversion.svg
         */

        internal static bool PositionIsInCircle(Vector3 position, Vector3 center, float radius)
        {
            if (center.Distance2DSqr(position) < (Math.Pow((double)radius, (double)radius)))
                return true;
            return false;
        }

        internal static bool PositionIsInsideArc(Vector3 position, Vector3 center, float radius, float rotation, float arcDegrees)
        {
            if (PositionIsInCircle(position, center, radius))
            {
                return GetIsFacingPosition(position, center, rotation, arcDegrees);
            }
            return false;
        }

        internal static bool GetIsFacingPosition(Vector3 position, Vector3 center, float rotation, float arcDegrees)
        {
            var DirectionVector = GetDirectionVectorFromRotation(rotation);
            if (DirectionVector != Vector2.Zero)
            {
                Vector3 u = position - center;
                u.Z = 0f;
                Vector3 v = new Vector3(DirectionVector.X, DirectionVector.Y, 0f);
                bool result = ((MathEx.ToDegrees(Vector3.AngleBetween(u, v)) <= arcDegrees) ? 1 : 0) != 0;
                return result;
            }
            else
                return false;
        }

        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        internal static Vector2 GetDirectionVectorFromRotation(double rotation)
        {
            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }


        #region Angle Finding
        /// <summary>
        /// Find the angle between two vectors. This will not only give the angle difference, but the direction.
        /// For example, it may give you -1 radian, or 1 radian, depending on the direction. Angle given will be the 
        /// angle from the FromVector to the DestVector, in radians.
        /// </summary>
        /// <param name="FromVector">Vector to start at.</param>
        /// <param name="DestVector">Destination vector.</param>
        /// <param name="DestVectorsRight">Right vector of the destination vector</param>
        /// <returns>Signed angle, in radians</returns>        
        /// <remarks>All three vectors must lie along the same plane.</remarks>
        public static double GetSignedAngleBetween2DVectors(Vector3 FromVector, Vector3 DestVector, Vector3 DestVectorsRight)
        {
            FromVector.Z = 0;
            DestVector.Z = 0;
            DestVectorsRight.Z = 0;

            FromVector.Normalize();
            DestVector.Normalize();
            DestVectorsRight.Normalize();

            float forwardDot = Vector3.Dot(FromVector, DestVector);
            float rightDot = Vector3.Dot(FromVector, DestVectorsRight);

            // Keep dot in range to prevent rounding errors
            forwardDot = MathEx.Clamp(forwardDot, -1.0f, 1.0f);

            double angleBetween = Math.Acos(forwardDot);

            if (rightDot < 0.0f)
                angleBetween *= -1.0f;

            return angleBetween;
        }
        public float UnsignedAngleBetweenTwoV3(Vector3 v1, Vector3 v2)
        {
            v1.Z = 0;
            v2.Z = 0;
            v1.Normalize();
            v2.Normalize();
            double Angle = (float)Math.Acos(Vector3.Dot(v1, v2));
            return (float)Angle;
        }
        /// <summary>
        /// Returns the Degree angle of a target location
        /// </summary>
        /// <param name="vStartLocation"></param>
        /// <param name="vTargetLocation"></param>
        /// <returns></returns>
        public static float FindDirectionDegree(Vector3 vStartLocation, Vector3 vTargetLocation)
        {
            return (float)RadianToDegree(NormalizeRadian((float)Math.Atan2(vTargetLocation.Y - vStartLocation.Y, vTargetLocation.X - vStartLocation.X)));
        }
        public static double FindDirectionRadian(Vector3 start, Vector3 end)
        {
            double radian = Math.Atan2(end.Y - start.Y, end.X - start.X);

            if (radian < 0)
            {
                double mod = -radian;
                mod %= Math.PI * 2d;
                mod = -mod + Math.PI * 2d;
                return mod;
            }
            return (radian % (Math.PI * 2d));
        }
        public Vector3 GetDirection(Vector3 origin, Vector3 destination)
        {
            Vector3 direction = destination - origin;
            direction.Normalize();
            return direction;
        }
        #endregion


        public static bool IntersectsPath(Vector3 obstacle, float radius, Vector3 start, Vector3 destination)
        {
            // fake-it to 2D
            obstacle.Z = 0;
            start.Z = 0;
            destination.Z = 0;

            return MathEx.IntersectsPath(obstacle, radius, start, destination);
        }

        public static bool TrinityIntersectsPath(Vector3 start, Vector3 obstacle, Vector3 destination, float distanceToObstacle = -1, float distanceToDestination = -1)
        {
            var toObstacle = distanceToObstacle >= 0 ? distanceToObstacle : start.Distance2D(obstacle);
            var toDestination = distanceToDestination >= 0 ? distanceToDestination : start.Distance2D(destination);

            if (toDestination > 500)
                return false;

            var relativeAngularVariance = GetRelativeAngularVariance(start, obstacle, destination);

            // Angular Variance at 20yd distance
            const int angularVarianceBase = 45;

            // Halve/Double required angle every 20yd; 60* @ 15yd, 11.25* @ 80yd
            var angularVarianceThreshold = Math.Min(angularVarianceBase / (toDestination / 20), 90);

            //Logger.Log("DistToObj={0} DistToDest={1} relativeAV={2} AVThreshold={3} Result={4}", 
            //    toObstacle, toDestination, relativeAngularVariance, angularVarianceThreshold, 
            //    toObstacle < toDestination && relativeAngularVariance <= angularVarianceThreshold);

            // Obstacle must be than destination
            if (toObstacle < toDestination)
            {
                // If the radius between lines (A) from start to obstacle and (B) from start to destination
                // are small enough then we know both targets are in the same-ish direction from start.
                if (relativeAngularVariance <= angularVarianceThreshold)
                {
                    return true;
                }                
            }
            return false;
        }

        public static Vector2 GetDirectionVector(Vector3 start, Vector3 end)
        {
            return new Vector2(end.X - start.X, end.Y - start.Y);
        }

        #region Angular Measure Unit Conversion
        public static double Normalize180(double angleA, double angleB)
        {
            //Returns an angle in the range -180 to 180
            double diffangle = (angleA - angleB) + 180d;
            diffangle = (diffangle / 360.0);
            diffangle = ((diffangle - Math.Floor(diffangle)) * 360.0d) - 180d;
            return diffangle;
        }
        public static float NormalizeRadian(float radian)
        {
            if (radian < 0)
            {
                double mod = -radian;
                mod %= Math.PI * 2d;
                mod = -mod + Math.PI * 2d;
                return (float)mod;
            }
            return (float)(radian % (Math.PI * 2d));
        }

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }


        #endregion
        public static double GetRelativeAngularVariance(Vector3 origin, Vector3 destA, Vector3 destB)
        {
            float fDirectionToTarget = NormalizeRadian((float)Math.Atan2(destA.Y - origin.Y, destA.X - origin.X));
            float fDirectionToObstacle = NormalizeRadian((float)Math.Atan2(destB.Y - origin.Y, destB.X - origin.X));
            return AbsAngularDiffernce(RadianToDegree(fDirectionToTarget), RadianToDegree(fDirectionToObstacle));
        }
        public static double AbsAngularDiffernce(double angleA, double angleB)
        {
            return 180d - Math.Abs(180d - Math.Abs(angleA - angleB));
        }

        #region Human Readable Headings
        public static string GetHeadingToPoint(Vector3 TargetPoint)
        {
            return GetHeading(FindDirectionDegree(Trinity.Player.Position, TargetPoint));
        }
        
        /// <summary>
        /// Gets string heading NE,S,NE etc
        /// </summary>
        /// <param name="headingDegrees">heading in degrees</param>
        /// <returns></returns>
        public static string GetHeading(float headingDegrees)
        {
            var directions = new string[] {
              //"n", "ne", "e", "se", "s", "sw", "w", "nw", "n"
                "s", "se", "e", "ne", "n", "nw", "w", "sw", "s"
            };

            var index = (((int)headingDegrees) + 23) / 45;
            return directions[index].ToUpper();
        }
        #endregion

        /// <summary>
        /// Gets the center of a given Navigation Zone
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        internal static Vector3 GetNavZoneCenter(NavZone zone)
        {
            float x = zone.ZoneMin.X + ((zone.ZoneMax.X - zone.ZoneMin.X) / 2);
            float y = zone.ZoneMin.Y + ((zone.ZoneMax.Y - zone.ZoneMin.Y) / 2);

            return new Vector3(x, y, 0);
        }

        /// <summary>
        /// Gets the center of a given Navigation Cell
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        internal static Vector3 GetNavCellCenter(NavCell cell, NavZone zone)
        {
            return GetNavCellCenter(cell.Min, cell.Max, zone);
        }

        /// <summary>
        /// Gets the center of a given box with min/max, adjusted for the Navigation Zone
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        internal static Vector3 GetNavCellCenter(Vector3 min, Vector3 max, NavZone zone)
        {
            float x = zone.ZoneMin.X + min.X + ((max.X - min.X) / 2);
            float y = zone.ZoneMin.Y + min.Y + ((max.Y - min.Y) / 2);
            float z = min.Z + ((max.Z - min.Z) / 2);

            return new Vector3(x, y, z);
        }

        
        public static Vector3 GetEstimatedPosition(Vector3 startPosition, double headingRadians, double time, double targetVelocity)
        {
            double x = startPosition.X + targetVelocity * time * Math.Sin(headingRadians);
            double y = startPosition.Y + targetVelocity * time * Math.Cos(headingRadians);
            return new Vector3((float)x, (float)y, 0);
        }

        /// <summary>
        /// Utility for Predictive Firing 
        /// </summary>
        public class Intercept
        {

            /*
                Intercept intercept = new Intercept();

                intercept.calculate (
                        ourRobotPositionX,
                        ourRobotPositionY,
                        currentTargetPositionX,
                        currentTargetPositionY,
                        curentTargetHeading_deg,
                        currentTargetVelocity,
                        bulletPower,
                        0 // Angular velocity
                );

                // Helper function that converts any angle into  
                // an angle between +180 and -180 degrees.
                    double turnAngle = normalRelativeAngle(intercept.bulletHeading_deg - robot.getGunHeading());

                // Move gun to target angle
                    robot.setTurnGunRight (turnAngle);

                    if (Math.abs (turnAngle) 
                        <= intercept.angleThreshold) {
                  // Ensure that the gun is pointing at the correct angle
                  if ((intercept.impactPoint.x > 0)
                                && (intercept.impactPoint.x < getBattleFieldWidth())
                                && (intercept.impactPoint.y > 0)
                                && (intercept.impactPoint.y < getBattleFieldHeight())) {
                    // Ensure that the predicted impact point is within 
                            // the battlefield
                            fire(bulletPower);
                        }
                    }
                }                          
             */

            public Vector2 impactPoint = new Vector2(0, 0);
            public double bulletHeading_deg;

            protected Vector2 bulletStartingPoint = new Vector2();
            protected Vector2 targetStartingPoint = new Vector2();
            public double targetHeading;
            public double targetVelocity;
            public double bulletPower;
            public double angleThreshold;
            public double distance;

            protected double impactTime;
            protected double angularVelocity_rad_per_sec;

            public void Calculate(
                // Initial bullet position x coordinate 
                    double xb,
                // Initial bullet position y coordinate
                    double yb,
                // Initial target position x coordinate
                    double xt,
                // Initial target position y coordinate
                    double yt,
                // Target heading
                    double tHeading,
                // Target velocity
                    double vt,
                // Power of the bullet that we will be firing
                    double bPower,
                // Angular velocity of the target
                    double angularVelocityDegPerSec,
                // target object's radius
                    double targetsRadius
            )
            {
                angularVelocity_rad_per_sec = DegreeToRadian(angularVelocityDegPerSec);

                bulletStartingPoint = new Vector2((float) xb, (float) yb);
                targetStartingPoint = new Vector2((float) xt, (float) yt);

                targetHeading = tHeading;
                targetVelocity = vt;
                bulletPower = bPower;
                double vb = 20 - 3 * bulletPower;

                // Start with initial guesses at 10 and 20 ticks
                impactTime = GetImpactTime(10, 20, 0.01);
                impactPoint = GetEstimatedPosition(impactTime);

                double dX = (impactPoint.X - bulletStartingPoint.X);
                double dY = (impactPoint.Y - bulletStartingPoint.Y);

                distance = Math.Sqrt(dX * dX + dY * dY);

                bulletHeading_deg = RadianToDegree(Math.Atan2(dX, dY));
                angleThreshold = RadianToDegree(Math.Atan(targetsRadius / distance));
            }

            protected Vector2 GetEstimatedPosition(double time)
            {
                double x = targetStartingPoint.X + targetVelocity * time * Math.Sin(DegreeToRadian(targetHeading));
                double y = targetStartingPoint.Y + targetVelocity * time * Math.Cos(DegreeToRadian(targetHeading));
                return new Vector2((float) x, (float) y);
            }

            private double F(double time)
            {

                double vb = 20 - 3 * bulletPower;

                Vector2 targetPosition = GetEstimatedPosition(time);
                double dX = (targetPosition.X - bulletStartingPoint.X);
                double dY = (targetPosition.Y - bulletStartingPoint.Y);

                return Math.Sqrt(dX * dX + dY * dY) - vb * time;
            }

            private double GetImpactTime(double t0,
                    double t1, double accuracy)
            {

                double X = t1;
                double lastX = t0;
                int iterationCount = 0;
                double lastfX = F(lastX);

                while ((Math.Abs(X - lastX) >= accuracy)
                        && (iterationCount < 15))
                {

                    iterationCount++;
                    double fX = F(X);

                    if ((fX - lastfX) == 0.0)
                    {
                        break;
                    }

                    double nextX = X - fX * (X - lastX) / (fX - lastfX);
                    lastX = X;
                    X = nextX;
                    lastfX = fX;
                }

                return X;
            }

        }

        public class CircularIntercept : Intercept {

            protected new Vector2 GetEstimatedPosition(double time) {
                if (Math.Abs(angularVelocity_rad_per_sec)
                        <= DegreeToRadian(0.1))
                {
                    return base.GetEstimatedPosition(time);
                }

                double initialTargetHeading = DegreeToRadian(targetHeading);
                double finalTargetHeading = initialTargetHeading
                        + angularVelocity_rad_per_sec * time;
                double x = targetStartingPoint.X - targetVelocity
                        / angularVelocity_rad_per_sec * (Math.Cos(finalTargetHeading)
                        - Math.Cos(initialTargetHeading));
                double y = targetStartingPoint.Y - targetVelocity
                        / angularVelocity_rad_per_sec
                        * (Math.Sin(initialTargetHeading)
                        - Math.Sin(finalTargetHeading));

                return new Vector2((float) x, (float) y);
            }

        }


    }
}
