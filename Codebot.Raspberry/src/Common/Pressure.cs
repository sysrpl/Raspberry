﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Codebot.Raspberry.Common
{
    /// <summary>
    /// Structure representing pressure
    /// </summary>
    public struct Pressure : IEquatable<Pressure>, IComparable<Pressure>, IComparable
    {
        const double MillibarRatio = 0.01;
        const double KilopascalRatio = 0.001;
        const double HectopascalRatio = 0.01;
        const double InchOfMercuryRatio = 0.000295301;
        const double MillimeterOfMercuryRatio = 0.00750062;
        const double MeanSeaLevelPascal = 101325;
        readonly double pascal;

        private Pressure(double pascal)
        {
            this.pascal = pascal;
        }

        /// <summary>
        /// The mean sea-level pressure (MSLP) is the average atmospheric pressure at mean sea level
        /// </summary>
        public static Pressure MeanSeaLevel => Pressure.FromPascal(MeanSeaLevelPascal);

        /// <summary>
        /// Pressure in Pa
        /// </summary>
        public double Pascal => pascal;

        /// <summary>
        /// Pressure in mbar
        /// </summary>
        public double Millibar => MillibarRatio * pascal;

        /// <summary>
        /// Pressure in kPa
        /// </summary>
        public double Kilopascal => KilopascalRatio * pascal;

        /// <summary>
        /// Pressure in hPa
        /// </summary>
        public double Hectopascal => HectopascalRatio * pascal;

        /// <summary>
        /// Pressure in inHg
        /// </summary>
        public double InchOfMercury => InchOfMercuryRatio * pascal;

        /// <summary>
        /// Pressure in mmHg
        /// </summary>
        public double MillimeterOfMercury => MillimeterOfMercuryRatio * pascal;

        /// <summary>
        /// Creates Pressure instance from pressure in Pa
        /// </summary>
        /// <param name="value">Pressure value in Pa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromPascal(double value)
        {
            return new Pressure(value);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in mbar
        /// </summary>
        /// <param name="value">Pressure value in mbar</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromMillibar(double value)
        {
            return new Pressure(value / MillibarRatio);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in kPa
        /// </summary>
        /// <param name="value">Pressure value in kPa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromKilopascal(double value)
        {
            return new Pressure(value / KilopascalRatio);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in hPa
        /// </summary>
        /// <param name="value">Pressure value in hPa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromHectopascal(double value)
        {
            return new Pressure(value / HectopascalRatio);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in inHg
        /// </summary>
        /// <param name="value">Pressure value in inHg</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromInchOfMercury(double value)
        {
            return new Pressure(value / InchOfMercuryRatio);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in mmHg
        /// </summary>
        /// <param name="value">Pressure value in mmHg</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromMillimeterOfMercury(double value)
        {
            return new Pressure(value / MillimeterOfMercuryRatio);
        }

        /// <summary>
        /// Creates a string representation of this object, in hPa.
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:F2}hPa", Hectopascal);
        }

        /// <summary>
        /// Equality member
        /// </summary>
        public bool Equals(Pressure other)
        {
            return pascal.Equals(other.pascal);
        }

        /// <summary>
        /// Equality member
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Pressure other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return pascal.GetHashCode();
        }

        /// <inheritdoc cref="IComparable{T}"/>
        public int CompareTo(Pressure other)
        {
            return pascal.CompareTo(other.pascal);
        }

        /// <inheritdoc cref="IComparable"/>
        int IComparable.CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is Pressure other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Pressure)}");
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(Pressure left, Pressure right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Unequality operator
        /// </summary>
        public static bool operator !=(Pressure left, Pressure right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Less-than operator
        /// </summary>
        public static bool operator <(Pressure left, Pressure right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Greater-than operator
        /// </summary>
        public static bool operator >(Pressure left, Pressure right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Less-than-or-equal operator
        /// </summary>
        public static bool operator <=(Pressure left, Pressure right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Greater-than-or-equal operator
        /// </summary>
        public static bool operator >=(Pressure left, Pressure right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
