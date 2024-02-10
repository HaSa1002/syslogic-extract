using UnityEngine;

namespace Microlayer
{
    /// <summary>
    /// Wrapper of a float with a number of overloaded operators to ease interpretation as boolean.
    /// </summary>
    public struct MicroData
    {
        /// <summary>
        /// Shorthand for new MicroData[] { 0f }
        /// </summary>
        public static readonly MicroData[] Low = { 0f };

        /// <summary>
        /// Shorthand for new MicroData[] { 1f }
        /// </summary>
        public static readonly MicroData[] High = { 1f };

        private float _value;

        /// <summary>
        /// Creates from a float. This constructor acts like an assignment.
        /// </summary>
        /// <param name="value">The value to create the struct with.</param>
        private MicroData(float value) => _value = value;

        /// <summary>
        /// Creates from a boolean. This constructor acts like an assignment. True is converted to 1, false to 0.
        /// </summary>
        /// <param name="value">True is converted to 1, false to 0.</param>
        private MicroData(bool value) => _value = value ? 1.0f : 0.0f;

        /// <summary>
        /// Explicitly converts the value to a boolean if they are greater then the threshold.
        /// The threshold is an implementation detail.
        /// </summary>
        /// <param name="data">The value to cast.</param>
        /// <returns>True if data is above the threshold, false otherwise.</returns>
        public static explicit operator bool(MicroData data) => data._value > 0.51;

        /// <summary>
        /// Explicitly casts the value back to a float. This is like an access of the raw value.
        /// </summary>
        /// <param name="data">The value to cast.</param>
        /// <returns>The held value.</returns>
        public static explicit operator float(MicroData data) => data._value;

        /// <summary>
        /// Creates from a boolean. This constructor acts like an assignment. True is converted to 1, false to 0.
        /// </summary>
        /// <param name="digitalValue">The value to create the struct with.</param>
        /// <returns></returns>
        public static implicit operator MicroData(bool digitalValue) => new(digitalValue);

        /// <summary>
        /// Creates from a boolean. This constructor acts like an assignment. True is converted to 1, false to 0.
        /// </summary>
        /// <param name="analogueValue">True is converted to 1, false to 0.</param>
        /// <returns></returns>
        public static implicit operator MicroData(float analogueValue) => new(analogueValue);

        /// <summary>
        /// Determines when this struct should be treated as true.
        /// This wrapper exists to enable interpretation of floats as bool in a simple, uniform way.
        /// This method uses the explicit bool casting operator.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool operator true(MicroData data) => (bool)data;

        /// <summary>
        /// Determines when this struct should be treated as false.
        /// This wrapper exists to enable interpretation of floats as bool in a simple, uniform way.
        /// This method uses the explicit bool casting operator.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool operator false(MicroData data) => !(bool)data;

        /// <summary>
        /// Negates the value as if it was a boolean.
        /// </summary>
        /// <param name="data">The value to negate.</param>
        /// <returns></returns>
        public static MicroData operator !(MicroData data) => !(bool)data;

        /// <summary>
        /// Enables logic OR for this struct.
        /// The result of this OR is the same as you get from a bool.
        ///
        /// ATTENTION:
        /// The very fact means there is no bitwise OR done, here!
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns></returns>
        public static MicroData operator |(MicroData x, MicroData y) => (bool)x | (bool)y;

        /// <summary>
        /// Enables logic AND for this struct.
        /// The result of this AND is the same as you get from a bool.
        ///
        /// ATTENTION:
        /// The very fact means there is no bitwise AND done, here!
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static MicroData operator &(MicroData x, MicroData y) => (bool)x & (bool)y;

        /// <summary>
        /// Enables addition using normal floats.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>The sum of both.</returns>
        public static MicroData operator +(MicroData x, MicroData y) => (float)x + (float)y;

        /// <summary>
        /// Enables subtraction using normal floats.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>The difference between both.</returns>
        public static MicroData operator -(MicroData x, MicroData y) => (float)x - (float)y;

        /// <summary>
        /// Enables multiplication using normal floats.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>The product of both.</returns>
        public static MicroData operator *(MicroData x, MicroData y) => (float)x * (float)y;

        /// <summary>
        /// Enables division using normal floats.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>The quotient of both.</returns>
        public static MicroData operator /(MicroData x, MicroData y) => (float)x / (float)y;

        /// <summary>
        /// Enables modulo using normal floats.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>The rest of the quotient of both.</returns>
        public static MicroData operator %(MicroData x, MicroData y) => (float)x % (float)y;

        /// <summary>
        /// Computes the logical XOR with the value being treated as boolean.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>A MicroData struct containing 1.0/true if only one of both values is true.</returns>
        public static MicroData operator ^(MicroData x, MicroData y) => (bool)x ^ (bool)y;

        /// <summary>
        /// Compares the raw (float) value for approximate equality.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public static bool operator ==(MicroData x, MicroData y) => Mathf.Approximately((float)x, (float)y);

        /// <summary>
        /// Compares the raw (float) value for approximate equality and negates the result.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>True if unequal, otherwise false.</returns>
        public static bool operator !=(MicroData x, MicroData y) => !(x == y);

        /// <summary>
        /// Compares the raw (float) values.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>True if x is smaller, otherwise false.</returns>
        public static bool operator <(MicroData x, MicroData y) => (float)x < (float)y;

        /// <summary>
        /// Compares the raw (float) values.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>True if x is greater, otherwise false.</returns>
        public static bool operator >(MicroData x, MicroData y) => (float)x > (float)y;

        /// <summary>
        /// Compares the raw (float) values.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>True if x is smaller or equal, otherwise false.</returns>
        public static bool operator <=(MicroData x, MicroData y) => (float)x <= (float)y;

        /// <summary>
        /// Compares the raw (float) values.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>True if x is greater or equal, otherwise false.</returns>
        public static bool operator >=(MicroData x, MicroData y) => (float)x >= (float)y;

        public override bool Equals(object obj) => obj is MicroData && (obj as MicroData?).Value == this;

        public bool Equals(MicroData other) => _value.Equals(other._value);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString()
        {
            return $"{_value} ({(bool)this})";
        }
    }
}