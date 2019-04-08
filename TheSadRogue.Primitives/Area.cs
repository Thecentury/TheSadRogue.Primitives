﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SadRogue.Primitives
{
	/// <summary>
	/// Represents an arbitrarily-shaped area of a map. Stores and provides access to a list of each
	/// unique position considered connected.
	/// </summary>
	public class Area : IReadOnlyArea
	{
		private readonly HashSet<Point> positionsSet;
		private List<Point> _positions;

		private int left, top, bottom, right;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Area()
		{
			left = int.MaxValue;
			top = int.MaxValue;

			right = 0;
			bottom = 0;

			_positions = new List<Point>();
			positionsSet = new HashSet<Point>();
		}

		/// <summary>
		/// Smallest possible rectangle that encompasses every position in the area.
		/// </summary>
		public Rectangle Bounds
		{
			get
			{
				if (right < left)
					return Rectangle.EMPTY;

				return new Rectangle(left, top, right - left + 1, bottom - top + 1);
			}
		}

		/// <summary>
		/// Number of (unique) positions in the currently stored list.
		/// </summary>
		public int Count { get { return _positions.Count; } }

		/// <summary>
		/// List of all (unique) positions in the list.
		/// </summary>
		public IReadOnlyList<Point> Positions { get { return _positions.AsReadOnly(); } }

		/// <summary>
		/// Gets a MapArea containing all positions in <paramref name="area1"/>, minus those that are in
		/// <paramref name="area2"/>.
		/// </summary>
		/// <param name="area1">The first MapArea.</param>
		/// <param name="area2">The second MapArea.</param>
		/// <returns>A MapArea with exactly those positions in <paramref name="area1"/> that are NOT in
		/// <paramref name="area2"/>.</returns>
		public static Area GetDifference(IReadOnlyArea area1, IReadOnlyArea area2)
		{
			var retVal = new Area();

			foreach (var pos in area1.Positions)
			{
				if (area2.Contains(pos))
					continue;

				retVal.Add(pos);
			}

			return retVal;
		}

		/// <summary>
		/// Gets a MapArea containing exactly those positions contained in both of the given MapAreas.
		/// </summary>
		/// <param name="area1">First MapArea.</param>
		/// <param name="area2">Second MapArea.</param>
		/// <returns>A MapArea containing exactly those positions contained in both of the given MapAreas.</returns>
		public static Area GetIntersection(IReadOnlyArea area1, IReadOnlyArea area2)
		{
			var retVal = new Area();

			if (!area1.Bounds.Intersects(area2.Bounds))
				return retVal;

			if (area1.Count > area2.Count)
				Swap(ref area1, ref area2);

			foreach (var pos in area1.Positions)
				if (area2.Contains(pos))
					retVal.Add(pos);

			return retVal;
		}

		/// <summary>
		/// Gets a new MapArea containing every position in one or both given map areas.
		/// </summary>
		/// <param name="area1">First MapArea.</param>
		/// <param name="area2">Second MapArea.</param>
		/// <returns>A MapArea containing only those positions in one or both of the given MapAreas.</returns>
		public static Area GetUnion(IReadOnlyArea area1, IReadOnlyArea area2)
		{
			var retVal = new Area();

			retVal.Add(area1);
			retVal.Add(area2);

			return retVal;
		}

		/// <summary>
		/// Inequality comparison -- true if the two areas do NOT contain exactly the same points.
		/// </summary>
		/// <param name="lhs">First MapArea to compare.</param>
		/// <param name="rhs">Second MapArea to compare.</param>
		/// <returns>True if the MapAreas do NOT contain exactly the same points, false otherwise.</returns>
		public static bool operator !=(Area lhs, Area rhs) => !(lhs == rhs);

		/// <summary>
		/// Creates a new MapArea with the Coords all shifted by the given vector.
		/// </summary>
		/// <param name="lhs">MapArea.</param>
		/// <param name="rhs">Coord (vector) to add.</param>
		/// <returns>
		/// A new MapArea with the positions all translated by the given amount in x and y directions.
		/// </returns>
		public static Area operator +(Area lhs, Point rhs)
		{
			var retVal = new Area();

			foreach (var pos in lhs.Positions)
				retVal.Add(pos + rhs);

			return retVal;
		}

		/// <summary>
		/// Compares for equality. Returns true if the two MapAreas are the same reference, or if
		/// they contain exactly the same points.
		/// </summary>
		/// <param name="lhs">First MapArea to compare.</param>
		/// <param name="rhs">Second MapArea to compare.</param>
		/// <returns>True if the MapAreas contain exactly the same points, false otherwise.</returns>
		public static bool operator ==(Area lhs, Area rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;

			// If one side is null (can't both be null or above would have returned)
			if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs))
				return false;

			if (lhs.Count != rhs.Count)
				return false;

			foreach (var pos in lhs.Positions)
				if (!rhs.Contains(pos))
					return false;

			return true;
		}

		/// <summary>
		/// Adds the given position to the list of points within the area if it is not already in the
		/// list, or does nothing otherwise.
		/// </summary>
		/// <remarks>
		/// Because the class uses a hash set internally to
		/// determine what points have already been added, this is an average case O(1) operation.
		/// </remarks>
		/// <param name="position">The position to add.</param>
		public void Add(Point position)
		{
			if (positionsSet.Add(position))
			{
				_positions.Add(position);

				// Update bounds
				if (position.X > right) right = position.X;
				if (position.X < left) left = position.X;
				if (position.Y > bottom) bottom = position.Y;
				if (position.Y < top) top = position.Y;
			}
		}

		/// <summary>
		/// Adds the given positions to the list of points within the area if they are not already in
		/// the list.
		/// </summary>
		/// <param name="positions">Positions to add to the list.</param>
		public void Add(IEnumerable<Point> positions)
		{
			foreach (var pos in positions)
				Add(pos);
		}

		/// <summary>
		/// Adds all positions in the given rectangle to the area, if they are not already present.
		/// </summary>
		/// <param name="rectangle">Rectangle whose points to add.</param>
		public void Add(Rectangle rectangle)
		{
			foreach (var pos in rectangle.Positions())
				Add(pos);
		}

		/// <summary>
		/// Adds the given position to the list of points within the area if it is not already in the
		/// list, or does nothing otherwise.
		/// </summary>
		/// <remarks>
		/// Because the class uses a hash set internally to
		/// determine what points have already been added, this is an average case O(1) operation.
		/// </remarks>
		/// <param name="x">X-coordinate of the position to add.</param>
		/// <param name="y">Y-coordinate of the position to add.</param>
		public void Add(int x, int y) => Add(new Point(x, y));

		/// <summary>
		/// Adds all coordinates in the given map area to this one.
		/// </summary>
		/// <param name="area">Area containing positions to add.</param>
		public void Add(IReadOnlyArea area)
		{
			foreach (var pos in area.Positions)
				Add(pos);
		}

		/// <summary>
		/// Determines whether or not the given position is considered within the area or not.
		/// </summary>
		/// <param name="position">The position to check.</param>
		/// <returns>True if the specified position is within the area, false otherwise.</returns>
		public bool Contains(Point position)
		{
			return positionsSet.Contains(position);
		}

		/// <summary>
		/// Determines whether or not the given position is considered within the area or not.
		/// </summary>
		/// <param name="x">X-coordinate of the position to check.</param>
		/// <param name="y">Y-coordinate of the position to check.</param>
		/// <returns>True if the specified position is within the area, false otherwise.</returns>
		public bool Contains(int x, int y)
		{
			return positionsSet.Contains(new Point(x, y));
		}

		/// <summary>
		/// Returns whether or not the given area is completely contained within the current one.
		/// </summary>
		/// <param name="area">Area to check.</param>
		/// <returns>
		/// True if the given area is completely contained within the current one, false otherwise.
		/// </returns>
		public bool Contains(IReadOnlyArea area)
		{
			if (!Bounds.Contains(area.Bounds))
				return false;

			foreach (var pos in area.Positions)
				if (!Contains(pos))
					return false;

			return true;
		}

		/// <summary>
		/// Same as operator==. Returns false of obj is not a MapArea.
		/// </summary>
		/// <param name="obj">Object to compare</param>
		/// <returns>
		/// True if the object given is a MapArea and is equal (contains the same points), false otherwise.
		/// </returns>
		public override bool Equals(object obj)
		{
			var area = obj as Area;
			if (area == null) return false;

			return this == area;
		}

		/// <summary>
		/// Returns hash of the underlying set.
		/// </summary>
		/// <returns>Hash code for the underlying set.</returns>
		public override int GetHashCode() => positionsSet.GetHashCode();

		/// <summary>
		/// Returns whether or not the given map area intersects the current one. If you intend to
		/// determine/use the exact intersection based on this return value, it is best to instead
		/// call the <see cref="Area.GetIntersection(IReadOnlyArea, IReadOnlyArea)"/>, and
		/// check the number of positions in the result (0 if no intersection).
		/// </summary>
		/// <param name="area">The area to check.</param>
		/// <returns>True if the given map area intersects the current one, false otherwise.</returns>
		public bool Intersects(IReadOnlyArea area)
		{
			if (!area.Bounds.Intersects(Bounds))
				return false;

			if (Count <= area.Count)
			{
				foreach (var pos in Positions)
					if (area.Contains(pos))
						return true;

				return false;
			}

			foreach (var pos in area.Positions)
				if (Contains(pos))
					return true;

			return false;
		}

		/// <summary>
		/// Removes the given position specified from the MapArea. Particularly when the Remove
		/// operation changes the bounds, this operation can be expensive, so if you must do multiple
		/// Remove operations, it would be best to group them into 1 using <see cref="Remove(IEnumerable{Point})"/>.
		/// </summary>
		/// <param name="position">The position to remove.</param>
		public void Remove(Point position) => Remove(YieldCoord(position));

		/// <summary>
		/// Removes positions for which the given predicate returns true from the MapArea.
		/// </summary>
		/// <param name="predicate">Predicate returning true for positions that should be removed.</param>
		public void Remove(Func<Point, bool> predicate)
		{
			bool recalculateBounds = false;

			foreach (var pos in _positions.Where(predicate))
			{
				if (positionsSet.Remove(pos))
					if (pos.X == left || pos.X == right || pos.Y == top || pos.Y == bottom)
						recalculateBounds = true;
			}

			_positions.RemoveAll(c => predicate(c));


			if (recalculateBounds)
				RecalculateBounds();
		}

		/// <summary>
		/// Removes the given positions from the specified MapArea.
		/// </summary>
		/// <param name="positions">Positions to remove.</param>
		public void Remove(HashSet<Point> positions)
		{
			bool recalculateBounds = false;
			foreach (var pos in positions)
				if (positionsSet.Remove(pos))
					if (pos.X == left || pos.X == right || pos.Y == top || pos.Y == bottom)
						recalculateBounds = true;

			_positions.RemoveAll(c => positions.Contains(c));

			if (recalculateBounds)
				RecalculateBounds();
		}

		/// <summary>
		/// Removes the given positions from the specified MapArea.
		/// </summary>
		/// <param name="positions">Positions to remove.</param>
		public void Remove(IEnumerable<Point> positions)
		{
			if (positions is HashSet<Point> set)
				Remove(set);
			else
				Remove(new HashSet<Point>(positions));
		}

		/// <summary>
		/// Removes the given position specified from the MapArea. Particularly when the Remove
		/// operation changes the bounds, this operation can be expensive, so if you must do multiple
		/// Remove operations, it would be best to group them into 1 using <see cref="Remove(IEnumerable{Point})"/>.
		/// </summary>
		/// <param name="x">X-coordinate of the position to remove.</param>
		/// <param name="y">Y-coordinate of the position to remove.</param>
		public void Remove(int x, int y) => Remove(new Point(x, y));

		/// <summary>
		/// Removes all positions in the given map area from this one.
		/// </summary>
		/// <param name="area">Area containing positions to remove.</param>
		public void Remove(IReadOnlyArea area) => Remove(area.Positions);

		/// <summary>
		/// Removes all positions in the given Rectangle from this MapArea.
		/// </summary>
		/// <param name="rectangle">Rectangle containing positions to remove.</param>
		public void Remove(Rectangle rectangle) => Remove(rectangle.Positions());

		/// <summary>
		/// Returns the string of each position in the MapArea, in a square-bracket enclosed list,
		/// eg. [(1, 2), (3, 4), (5, 6)].
		/// </summary>
		/// <returns>A string representation of those coordinates in the MapArea.</returns>
		public override string ToString()
		{
			var result = new StringBuilder("[");
			bool first = true;
			foreach (var item in _positions)
			{
				if (first)
					first = false;
				else
					result.Append(", ");

				result.Append(item.ToString());
			}
			result.Append("]");

			return result.ToString();
		}

		private void RecalculateBounds()
		{
			int leftLocal = int.MaxValue, topLocal = int.MaxValue;
			int rightLocal = int.MinValue, bottomLocal = int.MinValue;

			// Find new bounds
			foreach (var pos in _positions)
			{
				if (pos.X > rightLocal) rightLocal = pos.X;
				if (pos.X < leftLocal) leftLocal = pos.X;
				if (pos.Y > bottomLocal) bottomLocal = pos.Y;
				if (pos.Y < topLocal) topLocal = pos.Y;
			}

			left = leftLocal;
			right = rightLocal;
			top = topLocal;
			bottom = bottomLocal;
		}

		private static void Swap(ref IReadOnlyArea lhs, ref IReadOnlyArea rhs)
		{
			var temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		private static IEnumerable<Point> YieldCoord(Point item)
		{
			yield return item;
		}
	}
}
