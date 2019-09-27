﻿using System;
using System.Linq;

namespace SadRogue.Primitives
{
    /// <summary>
    /// Structure representing the concept of a "direction" on a grid, and "defines" the coordinate plane via the
    /// <see cref="Direction.YIncreasesUpward"/> flag. Interacts with Point to allow easy translation
    /// of positions in a direction, and contains numerous helper functions for retrieving directions in
    /// various orders, getting direction closest to a line, etc.
    /// </summary>
    /// <remarks>
    /// The static <see cref="Direction.YIncreasesUpward"/> flag defines the way that many algorithms
    /// interpret the coordinate plane.  By default, this flag is false, meaning that the y-value of positions
    /// is assumed to DECREASE as you proceed in the direction defined by <see cref="Direction.Up"/>, and
    /// increase as you go downward.  If the coordinate plane is displayed on the screen, the origin would be
    /// the top left corner.  This default setting matches the typical console/computer graphic definition of the
    /// coordinate plane.  Setting the flag to true inverts this, so that the y-value of positions INCREASES
    /// as you proceed in the direction defined by <see cref="Direction.Up"/>.  This places the origin in the bottom
    /// left corner, and matches a typical mathmatical definition of a euclidean coordinate plane, as well as the scene
    /// coordinate plane defined by Unity and other game engines.
    /// </remarks>
    [Serializable]
    public struct Direction : IEquatable<Direction>
    {
        [NonSerialized]
        private static readonly string[] s_writeVals = Enum.GetNames(typeof(Types));

        // All directions that aren't NONE.
        [NonSerialized]
        private static readonly Types[] s_validTypes = Enum.GetValues(typeof(Types)).Cast<Types>().Skip(1).ToArray();
        [NonSerialized]
        private static readonly (int dx, int dy)[] s_deltaVals;

        [NonSerialized]
        private static bool s_yIncreasesUpward;

        [NonSerialized]
        private static bool s_initYInc;

        static Direction()
        {
            s_deltaVals = new (int, int)[9];

            // These delta values don't change so we initialize these now
            s_deltaVals[(int)Types.Left] = (-1, 0);
            s_deltaVals[(int)Types.Right] = (1, 0);
            s_deltaVals[(int)Types.None] = (0, 0);

            // Initialize direction instances to point to each type
            Up = new Direction(Types.Up);
            UpRight = new Direction(Types.UpRight);
            Right = new Direction(Types.Right);
            DownRight = new Direction(Types.DownRight);
            Down = new Direction(Types.Down);
            DownLeft = new Direction(Types.DownLeft);
            Left = new Direction(Types.Left);
            UpLeft = new Direction(Types.UpLeft);
            None = new Direction(Types.None);

            // YIncreasesUpward property setter sets all the remaining dx/dy values in the array
            s_initYInc = false;
            // Initializes rest of distance values.  Safe to do becuase nobody can be using directions, as they haven't been initialized.
            SetYIncreasesUpwardsUnsafe(false);
        }

        private Direction(Types type) => Type = type;

        /// <summary>
        /// Enum representing Direction types. Each Direction instance has a <see cref="Type"/> field
        /// which contains the corresponding value from this enum.  Useful for easy mapping of Direction
        /// types to a primitive type (for cases like a switch statement).
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Type for <see cref="Direction.None"/>.
            /// </summary>
            None,
            /// <summary>
            /// Type for <see cref="Direction.Up"/>.
            /// </summary>
            Up,

            /// <summary>
            /// Type for <see cref="Direction.UpRight"/>.
            /// </summary>
            UpRight,

            /// <summary>
            /// Type for <see cref="Direction.Right"/>.
            /// </summary>
            Right,

            /// <summary>
            /// Type for <see cref="Direction.DownRight"/>.
            /// </summary>
            DownRight,

            /// <summary>
            /// Type for <see cref="Direction.Down"/>.
            /// </summary>
            Down,

            /// <summary>
            /// Type for <see cref="Direction.DownLeft"/>.
            /// </summary>
            DownLeft,

            /// <summary>
            /// Type for <see cref="Direction.Left"/>.
            /// </summary>
            Left,

            /// <summary>
            /// Type for <see cref="Direction.UpLeft"/>.
            /// </summary>
            UpLeft
        };

        /// <summary>
        /// Down direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction Down;

        /// <summary>
        /// Down-left direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction DownLeft;

        /// <summary>
        /// Down-right direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction DownRight;

        /// <summary>
        /// Left direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction Left;

        /// <summary>
        /// No direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction None;

        /// <summary>
        /// Right direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction Right;

        /// <summary>
        /// Up direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction Up;

        /// <summary>
        /// Up-left direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction UpLeft;

        /// <summary>
        /// Up-right direction.
        /// </summary>
        [NonSerialized]
        public static readonly Direction UpRight;

        /// <summary>
        /// Whether or not a positive y-value indicates an upward change. To set this value, use <see cref="SetYIncreasesUpwardsUnsafe(bool)"/>, however note that this is an unsafe
        /// operation in a multi-threaded environment where one or more threads may be using Directions.  It is intended that this configuration be done as part of an initialization
        /// routine.
        /// </summary>
        /// <remarks>
        /// If true, directions with an upwards component represent a positive change in y-value, and ones with downward components
        /// represent a negative change in y-value.  Changing this to false (which is the default) inverts this.
        /// </remarks>
        public static bool YIncreasesUpward => s_yIncreasesUpward;

        /// <summary>
        /// Change in x-value represented by this direction.
        /// </summary>
        public int DeltaX => s_deltaVals[(int)Type].dx;

        /// <summary>
        /// Change in y-value represented by this direction.
        /// </summary>
        public int DeltaY => s_deltaVals[(int)Type].dy;

        /// <summary>
        /// Enum type corresponding to direction being represented.
        /// </summary>
        public readonly Types Type;

        /// <summary>
        /// True if the given direction has the same Type the current one.
        /// </summary>
        /// <param name="other">Direction to compare.</param>
        /// <returns>True if the two directions are the same, false if not.</returns>
        public bool Equals(Direction other) => Type == other.Type;

        /// <summary>
        /// Same as operator == in this case; returns false if <paramref name="obj"/> is not a Direction.
        /// </summary>
        /// <param name="obj">The object to compare the current Direction to.</param>
        /// <returns>
        /// True if <paramref name="obj"/> is a Direction, and the two directions are equal, false otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is Direction c && Equals(c);

        /// <summary>
        /// Returns a hash-map value for the current object.
        /// </summary>
        /// <returns/>
        public override int GetHashCode() => Type.GetHashCode();

        /// <summary>
        /// True if the two directions have the same Type.
        /// </summary>
        /// <param name="lhs"/>
        /// <param name="rhs"/>
        /// <returns>True if the two directions are equal, false if not.</returns>
        public static bool operator ==(Direction lhs, Direction rhs) => lhs.Type == rhs.Type;

        /// <summary>
        /// True if the types are not equal.
        /// </summary>
        /// <param name="lhs"/>
        /// <param name="rhs"/>
        /// <returns>
        /// True if the types are not equal, false if they are both equal.
        /// </returns>
        public static bool operator !=(Direction lhs, Direction rhs) => !(lhs == rhs);

        // Do not change manually outside of YIncreasesUpwards functionality
        [NonSerialized]
        internal static int s_yMult;

        /// <summary>
        /// Changes the value of <see cref="YIncreasesUpward"/>.  This operation is not safe to perform if another thread may be directly or indirectly using Directions.
        /// It is intended that this value be set once to match your environment as part of an initialization routine.
        /// </summary>
        /// <param name="newValue">New value to assign to <see cref="YIncreasesUpward"/>.</param>
        public static void SetYIncreasesUpwardsUnsafe(bool newValue)
        {
            if (s_yIncreasesUpward != newValue || !s_initYInc)
            {
                s_initYInc = true;
                s_yIncreasesUpward = newValue;
                s_yMult = (s_yIncreasesUpward) ? -1 : 1;

                s_deltaVals[(int)Types.Up] = (0, -1 * s_yMult);
                s_deltaVals[(int)Types.Down] = (0, 1 * s_yMult);
                s_deltaVals[(int)Types.UpLeft] = (-1, -1 * s_yMult);
                s_deltaVals[(int)Types.UpRight] = (1, -1 * s_yMult);
                s_deltaVals[(int)Types.DownLeft] = (-1, 1 * s_yMult);
                s_deltaVals[(int)Types.DownRight] = (1, 1 * s_yMult);
            }
        }

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of the given
        /// line. Rounds clockwise if the heading is exactly on a diagonal direction. Similar to
        /// <see cref="GetDirection(Point, Point)"/>, except this function returns only cardinal directions.
        /// </summary>
        /// <param name="start">Starting coordinate of the line.</param>
        /// <param name="end">Ending coordinate of the line.</param>
        /// <returns>
        /// The cardinal direction that most closely matches the heading indicated by the given line.
        /// </returns>
        public static Direction GetCardinalDirection(Point start, Point end) => GetCardinalDirection(new Point(end.X - start.X, end.Y - start.Y));

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of a line
        /// with the given delta-change values. Rounds clockwise if exactly on a diagonal. Similar to
        /// <see cref="GetDirection(Point)"/>, except this function returns only cardinal directions.
        /// </summary>
        /// <param name="deltaChange">
        /// Vector representing the change in x and change in y across the line (deltaChange.X is the
        /// change in x, deltaChange.Y is the change in y).
        /// </param>
        /// <returns>
        /// The cardinal direction that most closely matches the degree heading of the given line.
        /// </returns>
        public static Direction GetCardinalDirection(Point deltaChange)
        {
            int dx = deltaChange.X;
            int dy = deltaChange.Y;

            if (dx == 0 && dy == 0)
            {
                return None;
            }

            dy *= s_yMult;

            double angle = Math.Atan2(dy, dx);
            double degree = MathHelpers.ToDegree(angle);
            degree += 450; // Rotate angle such that it's all positives, and such that 0 is up.
            degree %= 360; // Normalize angle to 0-360

            if (degree < 45.0)
            {
                return Up;
            }

            if (degree < 135.0)
            {
                return Right;
            }

            if (degree < 225.0)
            {
                return Down;
            }

            if (degree < 315.0)
            {
                return Left;
            }

            return Up;
        }

        /// <summary>
        /// Returns the direction that most closely matches the degree heading of the given line.
        /// Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="start">Starting coordinate of the line.</param>
        /// <param name="end">Ending coordinate of the line.</param>
        /// <returns>
        /// The direction that most closely matches the heading indicated by the given line.
        /// </returns>
        public static Direction GetDirection(Point start, Point end) => GetDirection(new Point(end.X - start.X, end.Y - start.Y));


        /// <summary>
        /// Returns the direction that most closely matches the degree heading of a line with the
        /// given delta-change values. Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="deltaChange">
        /// Vector representing the change in x and change in y across the line (deltaChange.X is the
        /// change in x, deltaChange.Y is the change in y).
        /// </param>
        /// <returns>
        /// The direction that most closely matches the heading indicated by the given input.
        /// </returns>
        public static Direction GetDirection(Point deltaChange)
        {
            int dx = deltaChange.X;
            int dy = deltaChange.Y;

            if (dx == 0 && dy == 0)
            {
                return None;
            }

            dy *= s_yMult;

            double angle = Math.Atan2(dy, dx);
            double degree = MathHelpers.ToDegree(angle);
            degree += 450; // Rotate angle such that it's all positives, and such that 0 is up.
            degree %= 360; // Normalize angle to 0-360

            if (degree < 22.5)
            {
                return Up;
            }

            if (degree < 67.5)
            {
                return UpRight;
            }

            if (degree < 112.5)
            {
                return Right;
            }

            if (degree < 157.5)
            {
                return DownRight;
            }

            if (degree < 202.5)
            {
                return Down;
            }

            if (degree < 247.5)
            {
                return DownLeft;
            }

            if (degree < 292.5)
            {
                return Left;
            }

            if (degree < 337.5)
            {
                return UpLeft;
            }

            return Up;
        }

        /// <summary>
        /// Moves the direction counter-clockwise <paramref name="i"/> times.
        /// </summary>
        /// <param name="d"/>
        /// <param name="i"/>
        /// <returns>
        /// The given direction moved counter-clockwise <paramref name="i"/> times.
        /// </returns>
        public static Direction operator -(Direction d, int i) => (d == None) ? None : ToDirection(s_validTypes[WrapAround((int)d.Type - i - 1, 8)]);

        /// <summary>
        /// Moves the direction counter-clockwise by one.
        /// </summary>
        /// <param name="d"/>
        /// <returns>The direction one unit counterclockwise of <paramref name="d"/>.</returns>
        public static Direction operator --(Direction d) => (d == None) ? None : ToDirection(s_validTypes[WrapAround((int)d.Type - 2, 8)]);

        /// <summary>
        /// Moves the direction clockwise <paramref name="i"/> times.
        /// </summary>
        /// <param name="d"/>
        /// <param name="i"/>
        /// <returns>
        /// The given direction moved clockwise <paramref name="i"/> times.
        /// </returns>
        public static Direction operator +(Direction d, int i) => (d == None) ? None : ToDirection(s_validTypes[WrapAround((int)d.Type + i - 1, 8)]);

        /// <summary>
        /// Moves the direction clockwise by one.
        /// </summary>
        /// <param name="d"/>
        /// <returns>The direction one unit clockwise of <paramref name="d"/>.</returns>
        public static Direction operator ++(Direction d) => (d == None) ? None : ToDirection(s_validTypes[WrapAround((int)d.Type, 8)]);

        /// <summary>
        /// Gets the Direction class instance representing the direction type specified.
        /// </summary>
        /// <param name="directionType">The enum value for the direction.</param>
        /// <returns>The direction class representing the given direction.</returns>
        public static Direction ToDirection(Types directionType)
        {
            switch (directionType)
            {
                case Types.Up:
                    return Up;

                case Types.UpRight:
                    return UpRight;

                case Types.Right:
                    return Right;

                case Types.DownRight:
                    return DownRight;

                case Types.Down:
                    return Down;

                case Types.DownLeft:
                    return DownLeft;

                case Types.Left:
                    return Left;

                case Types.UpLeft:
                    return UpLeft;

                case Types.None:
                    return None;

                default:
                    throw new Exception($"Could not convert {nameof(Type)} instance to {nameof(Direction)} -- this is a bug!."); // Will not occur
            }
        }

        /// <summary>
        /// Returns true if the current direction is a cardinal direction.
        /// </summary>
        /// <returns>True if the current direction is a cardinal direction, false otherwise.</returns>
        public bool IsCardinal() => this != None && (DeltaX == 0 || DeltaY == 0);

        /// <summary>
        /// Writes the string (eg. "UP", "UP_RIGHT", etc.) for the direction.
        /// </summary>
        /// <returns>String representation of the direction.</returns>
        public override string ToString() => s_writeVals[(int)Type];

        #region Tuple Compatibility
        /// <summary>
        /// Translates the given position by one unit in the given direction.
        /// </summary>
        /// <param name="tuple"/>
        /// <param name="d"/>
        /// <returns>
        /// Tuple (tuple.y + d.DeltaX, tuple.y + d.DeltaY).
        /// </returns>
        public static (int x, int y) operator +((int x, int y) tuple, Direction d) => (tuple.x + d.DeltaX, tuple.y + d.DeltaY);
        #endregion

        private static int WrapAround(int num, int wrapTo) => (num % wrapTo + wrapTo) % wrapTo;
    }
}
