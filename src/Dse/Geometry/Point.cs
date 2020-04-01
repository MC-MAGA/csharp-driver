﻿//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Dse.Geometry
{
    /// <summary>
    /// Represents a zero-dimensional object that represents a specific (X,Y) location in a two-dimensional XY-Plane.
    /// In case of Geographic Coordinate Systems, the X coordinate is the longitude and the Y is the latitude.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class Point : GeometryBase
    {
        private static readonly Regex WktRegex = new Regex(
            @"^POINT\s?\(([-0-9\.]+) ([-0-9\.]+)\)$", RegexOptions.Compiled);

        /// <summary>
        /// Returns the X coordinate of this 2D point.
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        /// Returns the Y coordinate of this 2D point.
        /// </summary>
        public double Y { get; private set; }

        /// <inheritdoc />
        protected override IEnumerable GeoCoordinates
        {
            get { return new[] {X, Y}; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="Point"/>.
        /// </summary>
        /// <param name="x">The <c>X</c> coordinate.</param>
        /// <param name="y">The <c>Y</c> coordinate.</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

#if NET45
        /// <summary>
        /// Creates a new instance of <see cref="Point"/>.
        /// </summary>
        protected Point(SerializationInfo info, StreamingContext context)
        {
            var coordinates = (double[])info.GetValue("coordinates", typeof(double[]));
            X = coordinates[0];
            Y = coordinates[1];
        }
#endif

        /// <summary>
        /// Returns a value indicating whether this instance and a specified object represent the same value.
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as Point;
            if (other == null)
            {
                return false;
            }
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return CombineHashCode(new [] { X, Y});
            // ReSharper enable NonReadonlyMemberInGetHashCode
        }

        /// <summary>
        /// Returns Well-known text (WKT) representation of the geometry object.
        /// </summary>
        public override string ToString()
        {
            return string.Format("POINT ({0} {1})", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Creates a <see cref="Point"/> instance from a 
        /// <see href="https://en.wikipedia.org/wiki/Well-known_text">Well-known Text(WKT)</see>
        /// representation of a 2D point.
        /// </summary>
        public static Point Parse(string textValue)
        {
            if (textValue == null)
            {
                throw new ArgumentNullException("textValue");
            }
            var match = WktRegex.Match(textValue);
            if (!match.Success)
            {
                throw InvalidFormatException(textValue);
            }
            return new Point(Convert.ToDouble(match.Groups[1].Value, CultureInfo.InvariantCulture), Convert.ToDouble(match.Groups[2].Value, CultureInfo.InvariantCulture));
        }
    }
}
