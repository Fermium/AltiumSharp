﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace AltiumSharp.BasicTypes
{
    /// <summary>
    /// Value of a parameter.
    /// </summary>
    public readonly struct ParameterValue : IEquatable<ParameterValue>
    {
        private const NumberStyles Ns = NumberStyles.Any;
        private static readonly IFormatProvider Fp = CultureInfo.InvariantCulture;
        internal static readonly string[] TrueValues = new string[] { "T", "TRUE" };
        internal static readonly string[] FalseValues = new string[] { "F", "FALSE" };
        internal static readonly char[] ListSeparators = new[] { ',', '?' };

        private readonly string _data;
        private readonly int _level;

        private char ListSeparator => ListSeparators.ElementAtOrDefault(_level);

        internal ParameterValue(string data, int level) =>
            (_data, _level) = (data, level);

        /// <summary>
        /// Gets the string representation of this parameter value.
        /// </summary>
        public override string ToString() => _data;

        /// <summary>
        /// Gets the string value of this parameter.
        /// </summary>
        public string AsString() => _data;

        /// <summary>
        /// Gets the string value of this parameter, or a default value.
        /// </summary>
        public string AsStringOrDefault(string defaultValue = default) =>
            _data ?? defaultValue;

        /// <summary>
        /// Gets the integer value of this parameter.
        /// </summary>
        public int AsInt() => int.Parse(_data, Fp);

        /// <summary>
        /// Gets the integer value of this parameter, or a default value.
        /// </summary>
        public int AsIntOrDefault(int defaultValue = default) =>
            int.TryParse(_data, Ns, Fp, out var result) ? result : defaultValue;

        /// <summary>
        /// Gets the double precision floating point value of this parameter.
        /// </summary>
        public double AsDouble() => double.Parse(_data, Fp);

        /// <summary>
        /// Gets the double precision floating point value of this parameter, or a default value.
        /// </summary>
        public double AsDoubleOrDefault(double defaultValue = default) =>
            double.TryParse(_data, Ns, Fp, out var result) ? result : defaultValue;

        /// <summary>
        /// Gets the boolean value of this parameter.
        /// </summary>
        public bool AsBool()
        {
            if (TrueValues.Contains(_data))
            {
                return true;
            }
            else if (FalseValues.Contains(_data) || _data == null)
            {
                return false;
            }
            else
            {
                throw new FormatException("Value is not a valid boolean");
            }
        }

        /// <summary>
        /// Gets the coordinate value of this parameter.
        /// </summary>
        public Coord AsCoord() => Utils.StringToCoordUnit(_data, out _);

        /// <summary>
        /// Gets the color value of this parameter.
        /// </summary>
        public Color AsColor() => ColorTranslator.FromWin32(AsInt());

        /// <summary>
        /// Gets the color value of this parameter, or a default color.
        /// </summary>
        public Color AsColorOrDefault() => ColorTranslator.FromWin32(AsIntOrDefault());

        /// <summary>
        /// Gets current value as a parameter collection.
        /// </summary>
        public ParameterCollection AsParameters() => new ParameterCollection(_data, _level + 1);

        /// <summary>
        /// Splits the current value as string and allows enumerating over the resulting values.
        /// </summary>
        /// <param name="separator">
        /// Character used to split the text of the current value into a list of parameter values.
        /// </param>
        public IEnumerable<ParameterValue> AsEnumerable(char? separator = null)
        {
            foreach (var item in _data.Split(separator ?? ListSeparator))
            {
                yield return new ParameterValue(item, '\0');
            }
        }

        /// <summary>
        /// Converts the current value to a list of parameter values.
        /// <seealso cref="AsEnumerable(char?)"/>
        /// </summary>
        /// <param name="separator">
        /// Character used to split the text of the current value into a list of parameter values.
        /// </param>
        /// <returns>
        /// List of parameters after separating them using <paramref name="separator"/>.
        /// </returns>
        public IReadOnlyList<ParameterValue> AsList(char? separator = null) => AsEnumerable(separator).ToArray();

        /// <summary>
        /// Converts the current value to a list of strings.
        /// <seealso cref="AsEnumerable(char?)"/>
        /// </summary>
        /// <param name="separator">
        /// Character used to split the text of the current value into a list of strings.
        /// </param>
        /// <returns>
        /// List of strings after separating the current value using <paramref name="separator"/>.
        /// </returns>
        public IReadOnlyList<string> AsStringList(char? separator = null) => AsEnumerable(separator).Select(p => p.AsString()).ToArray();

        /// <summary>
        /// Converts the current value to a list of integers.
        /// <seealso cref="AsEnumerable(char?)"/>
        /// </summary>
        /// <param name="separator">
        /// Character used to split the text of the current value into a list of integers.
        /// </param>
        /// <returns>
        /// List of integers after separating the current value using <paramref name="separator"/>.
        /// </returns>
        public IReadOnlyList<int> AsIntList(char? separator = null) => AsEnumerable(separator).Select(p => p.AsInt()).ToArray();

        /// <summary>
        /// Converts the current value to a list of double precision floating point numbers.
        /// <seealso cref="AsEnumerable(char?)"/>
        /// </summary>
        /// <param name="separator">
        /// Character used to split the text of the current value into a list of double precision values.
        /// </param>
        /// <returns>
        /// List of double precision values after separating the current value using <paramref name="separator"/>.
        /// </returns>
        public IReadOnlyList<double> AsDoubleList(char? separator = null) => AsEnumerable(separator).Select(p => p.AsDouble()).ToArray();

        /// <summary>
        /// Converts the current value to a list of internal coordinate values.
        /// <seealso cref="AsEnumerable(char?)"/>
        /// </summary>
        /// <param name="separator">
        /// Character used to split the text of the current value into a list of coordinates.
        /// </param>
        /// <returns>
        /// List of coordinates after separating the current value using <paramref name="separator"/>.
        /// </returns>
        public IReadOnlyList<Coord> AsCoordList(char? separator = null) => AsEnumerable(separator).Select(p => p.AsCoord()).ToArray();

        /// <summary>
        /// Tests if the current value can be possibly converted to some kind of list by
        /// splitting the text using the selected <paramref name="separator"/> character.
        /// <seealso cref="AsList(char?)"/>
        /// </summary>
        /// <param name="separator">
        /// Character used to split the text of the current value into a list of coordinates.
        /// </param>
        /// <returns>
        /// True if this value can be split into a list using the given <paramref name="separator"/>.
        /// </returns>
        public bool IsList(char? separator = null) =>
            _data.Contains(separator ?? ListSeparator);

        /// <summary>
        /// Tests if the current value can be possibly converted to a parameters list.
        /// <seealso cref="AsParameters"/>
        /// </summary>
        /// <returns>
        /// True if this value can be split into a key, value parameter list.
        /// </returns>
        public bool IsParameters() =>
            _data.Contains(ParameterCollection.EntrySeparators.ElementAtOrDefault(_level + 1));

        #region 'boilerplate'
        public override bool Equals(object obj) => obj is ParameterValue other && this.Equals(other);
        public bool Equals(ParameterValue other) => _data == other._data;
        public override int GetHashCode() => _data.GetHashCode();
        public static bool operator ==(ParameterValue left, ParameterValue right) => left.Equals(right);
        public static bool operator !=(ParameterValue left, ParameterValue right) => !(left == right);
        #endregion
    }

    /// <summary>
    /// Key, Value parameter pair.
    /// </summary>
    public readonly struct Parameter : IEquatable<Parameter>
    {
        public string Name { get; }
        public ParameterValue Value { get; }

        internal Parameter(string name, string value, int level) =>
            (Name, Value) = (name, new ParameterValue(value, level));

        /// <summary>
        /// Returns the string representation of this parameter key, value pair.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Name}={Value}";


        #region 'boilerplate'
        public override bool Equals(object obj) => obj is Parameter other && this.Equals(other);
        public bool Equals(Parameter other) => Name == other.Name && Value == other.Value;
        public override int GetHashCode() => Name.GetHashCode();
        public static bool operator ==(Parameter left, Parameter right) => left.Equals(right);
        public static bool operator !=(Parameter left, Parameter right) => !(left == right);
        #endregion
    }

    /// <summary>
    /// Stores parameters lists and allows for easy access to type conversions of its values.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class ParameterCollection : IEnumerable<(string key, ParameterValue value)>
    {
        internal static readonly char[] EntrySeparators = new[] { '|', '`' };
        private const char KeyValueSeparator = '=';
        private const char ListSeparator = ',';

        private string _data;
        private int _level;
        private string _record;
        private List<string> _keys;
        private Dictionary<string, Parameter> _parameters;

        private char EntrySeparator => EntrySeparators.ElementAtOrDefault(_level);

        public ParameterCollection()
        {
            _keys = new List<string>();
            _parameters = new Dictionary<string, Parameter>();
        }

        internal ParameterCollection(string data, int level = 0)
        {
            _keys = new List<string>();
            _parameters = new Dictionary<string, Parameter>();
            _data = data;
            _level = level;
            ParseData();
        }

        /// <summary>
        /// Parses the input <see cref="_data"/> and creates (key, value) pairs accordinly.
        /// </summary>
        private void ParseData()
        {
            // splits data into pipe-separated properties, and then each one into key=value pairs
            var sepKeyValue = new char[] { KeyValueSeparator };

            var entries = _data.Split(new char[] { EntrySeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Select((line, index) => (index, line.Split(sepKeyValue, 2)));
            foreach (var (i, entryKeyValue) in entries)
            {
                var key = (entryKeyValue.Length > 1) ? entryKeyValue[0] : "";
                var value = entryKeyValue.Last();
                if (key.ToUpperInvariant() == "RECORD")
                {
                    if (string.IsNullOrEmpty(_record))
                    {
                        _record = value;
                        AddData(key, value);
                    }
                    else if (value != _record)
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    AddData(key, value);
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="ParameterCollection"/> instance from string <paramref name="data"/>.
        /// </summary>
        /// <param name="data">Data to be used for creating the parameters list.</param>
        /// <returns>New instance of <see cref="ParameterCollection"/>.</returns>
        public static ParameterCollection FromString(string data) =>
            new ParameterCollection(data);

        /// <summary>
        /// Generates a string version of the data contained in this <see cref="ParameterCollection"/>.
        /// </summary>
        /// <returns>String representaton of the current parameters.</returns>
        public override string ToString()
        {
            return EntrySeparator + string.Join(EntrySeparator.ToString(CultureInfo.InvariantCulture),
                _keys.Select(k => _parameters[k]).Select(p => p.ToString()));
        }

        /// <summary>
        /// Internal method used for adding a new (key, data) pair.
        /// </summary>
        /// <param name="key">Key of the value to be added.</param>
        /// <param name="data">String representation of the value to be added.</param>
        private void AddData(string key, string data)
        {
            key = key?.ToUpperInvariant();
            var parameterValue = new Parameter(key, data, _level);
            if (_parameters.ContainsKey(key))
            {
                _parameters[key] = parameterValue;
            }
            else
            {
                _parameters.Add(key, parameterValue);
                _keys.Add(key);
            }
        }

        /// <summary>
        /// Adds a key with a string value.
        /// </summary>
        public void Add(string key, string value) =>
            AddData(key, value);

        /// <summary>
        /// Adds a key with a integer value.
        /// </summary>
        public void Add(string key, int value) =>
            AddData(key, value.ToString(CultureInfo.InvariantCulture));
                                              
        /// <summary>
        /// Adds a key with a double floating point value.
        /// </summary>
        public void Add(string key, double value) =>
            AddData(key, value.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Adds a key with a boolean value.
        /// </summary>
        public void Add(string key, bool value) =>
            AddData(key, value ? ParameterValue.TrueValues.First() : ParameterValue.FalseValues.First());

        /// <summary>
        /// Adds a key with a coordinate value.
        /// </summary>
        public void Add(string key, Coord value) =>
            AddData(key, Utils.CoordUnitToString(value, Unit.Mil));

        /// <summary>
        /// Adds a key with a color value.
        /// </summary>
        public void Add(string key, Color value) =>
            AddData(key, ColorTranslator.ToWin32(value).ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Removes a key entry from the parameters list collection.
        /// </summary>
        public void Remove(string key)
        {
            _parameters.Remove(key);
            _keys.Remove(key);
        }

        /// <summary>
        /// Tests if key exists in the current parameters list.
        /// </summary>
        public bool Contains(string key) => _parameters.ContainsKey(key?.ToUpperInvariant());

        /// <summary>
        /// Returns the numeric index of the given <paramref name="key"/>.
        /// </summary>
        public int IndexOf(string key) => _keys.IndexOf(key);

        /// <summary>
        /// Looks up a key from the index of it in the parameters list.
        /// </summary>
        /// <param name="index">Index of the key name to be returned.</param>
        public string GetKey(int index) => _keys[index];

        /// <summary>
        /// Gets a parameter value from its key.
        /// </summary>
        public ParameterValue this[string key]
        {
            get
            {
                if (TryGetValue(key?.ToUpperInvariant(), out var result))
                {
                    return result.Value;
                }
                else
                {
                    return new Parameter().Value;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Parameter"/> (key, value) descriptor for a given parameter index.
        /// </summary>
        public Parameter this[int index] => _parameters[_keys[index]];

        /// <summary>
        /// Tries to get a <paramref name="key"/> value, and returns it if it exists.
        /// </summary>
        /// <param name="key">Key of the value to be returned.</param>
        /// <param name="result">Output value of the selected <paramref name="key"/>.</param>
        /// <returns>True if the <paramref name="key"/> exists and false otherwise.</returns>
        public bool TryGetValue(string key, out Parameter result) =>
            _parameters.TryGetValue(key?.ToUpperInvariant(), out result);

        /// <summary>
        /// Gets the value for a parameter <paramref name="key"/>, returning a default
        /// value if it doesn't.
        /// </summary>
        /// <param name="key">Key of the value to be returned.</param>
        /// <param name="defaultValue">
        /// Default value to be returned if the <paramref name="key"/> doesn't exist.
        /// </param>
        /// <returns>Parameter value for the chosen <paramref name="key"/>.</returns>
        public ParameterValue ValueOrDefault(string key, string defaultValue = default)
        {
            if (TryGetValue(key, out var value))
            {
                return value.Value;
            }
            else
            {
                return new Parameter(key, defaultValue, ListSeparator).Value;
            }
        }

        /// <summary>
        /// Allows enumerating the existing parameters as (key, value) pairs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<(string key, ParameterValue value)> GetEnumerator()
        {
            foreach (var key in _keys)
            {
                yield return (key, _parameters[key].Value);
            }
        }

        /// <summary>
        /// Allows enumerating the existing parameters as (key, value) pairs.
        /// <seealso cref="GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns the number of existing parameters.
        /// </summary>
        public int Count => _parameters.Count;
    }
}
