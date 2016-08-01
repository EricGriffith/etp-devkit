﻿//----------------------------------------------------------------------- 
// ETP DevKit, 1.1
//
// Copyright 2016 Petrotechnical Data Systems
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Energistics.Datatypes
{
    /// <summary>
    /// Represents a URI supported by the Energistics Transfer Protocol (ETP).
    /// </summary>
    public struct EtpUri
    {
        private static readonly Regex Pattern = new Regex(@"^eml:\/\/(([_\w\-]+)?\/)?((witsml|resqml|prodml|energyml)([0-9]+))(\/((obj_|cs_)?(\w+))(\(([\-\=\w]+)\))?)*?$", RegexOptions.IgnoreCase);
        private readonly Match _match;

        /// <summary>
        /// The root URI supported by the Discovery protocol.
        /// </summary>
        public const string RootUri = "/";

        /// <summary>
        /// Initializes a new instance of the <see cref="EtpUri"/> struct.
        /// </summary>
        /// <param name="uri">The URI string.</param>
        public EtpUri(string uri)
        {
            _match = Pattern.Match(uri);

            Uri = uri;
            IsValid = _match.Success;

            DataSpace = GetValue(_match, 2);
            Family = GetValue(_match, 4);
            Version = FormatVersion(GetValue(_match, 5), Family);
            ContentType = new EtpContentType(Family, Version);
            SchemaType = null;
            ObjectType = null;
            ObjectId = null;

            if (!HasRepeatValues(_match)) return;

            var last = GetObjectIds().Last();
            SchemaType = last.SchemaType;
            ObjectType = last.ObjectType;
            ObjectId = last.ObjectId;
            ContentType = new EtpContentType(Family, Version, SchemaType);
        }

        /// <summary>
        /// Gets the original URI string.
        /// </summary>
        /// <value>The URI.</value>
        public string Uri { get; }

        /// <summary>
        /// Gets the data space.
        /// </summary>
        /// <value>The data space.</value>
        public string DataSpace { get; }

        /// <summary>
        /// Gets the ML family name.
        /// </summary>
        /// <value>The ML family.</value>
        public string Family { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; }

        /// <summary>
        /// Gets the XSD type of the object.
        /// </summary>
        /// <value>The XSD type of the object.</value>
        public string SchemaType { get; }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        public string ObjectType { get; }

        /// <summary>
        /// Gets the object identifier.
        /// </summary>
        /// <value>The object identifier.</value>
        public string ObjectId { get; }

        /// <summary>
        /// Gets the content type.
        /// </summary>
        /// <value>The type of the content.</value>
        public EtpContentType ContentType { get; }

        /// <summary>
        /// Returns true if a valid URI was specified.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a base URI.
        /// </summary>
        /// <value><c>true</c> if this instance is a base URI; otherwise, <c>false</c>.</value>
        public bool IsBaseUri
        {
            get
            {
                return string.IsNullOrWhiteSpace(ObjectType)
                    && string.IsNullOrWhiteSpace(ObjectId);
            }
        }

        /// <summary>
        /// Gets the parent URI.
        /// </summary>
        /// <value>The parent URI.</value>
        public EtpUri Parent
        {
            get
            {
                if (!IsValid || IsBaseUri)
                    return this;

                var index = Uri.LastIndexOf('/');
                return new EtpUri(Uri.Substring(0, index));
            }
        }

        /// <summary>
        /// Determines whether this instance is related to the specified <see cref="EtpUri"/>.
        /// </summary>
        /// <param name="other">The other URI.</param>
        /// <returns>
        ///   <c>true</c> if the two <see cref="EtpUri"/> instances share the same family and
        ///   version; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRelatedTo(EtpUri other)
        {
            return string.Equals(Family, other.Family, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(Version, other.Version, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets a collection of <see cref="Segment"/> items.
        /// </summary>
        /// <returns>A collection of <see cref="Segment"/> items.</returns>
        public IEnumerable<Segment> GetObjectIds()
        {
            if (HasRepeatValues(_match))
            {
                var schemaTypeGroup = _match.Groups[8];
                var objectTypeGroup = _match.Groups[9];
                var objectIdGroup = _match.Groups[11];

                for (int i=0; i<objectTypeGroup.Captures.Count; i++)
                {
                    var objectType = objectTypeGroup.Captures[i].Value;
                    var schemaType = schemaTypeGroup.Captures.Count > i ? schemaTypeGroup.Captures[i].Value : string.Empty;
                    var objectId = objectIdGroup.Captures.Count > i ? objectIdGroup.Captures[i].Value : null;

                    if (!string.IsNullOrWhiteSpace(objectType))
                        schemaType += objectType;

                    yield return new Segment(
                        EtpContentType.FormatSchemaType(schemaType, Version),
                        EtpContentType.FormatObjectType(objectType),
                        objectId);
                }
            }
        }

        public EtpUri Append(string objectType, string objectId = null)
        {
            var schemaType = EtpContentType.FormatSchemaType(objectType, Version);

            if (string.IsNullOrWhiteSpace(objectId))
                return new EtpUri(Uri + "/" + schemaType);

            return new EtpUri(Uri + string.Format("/{0}({1})", schemaType, objectId));
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Uri;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is EtpUri))
                return false;

            return Equals((EtpUri)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="EtpUri" />, is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="EtpUri" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="EtpUri" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(EtpUri other)
        {
            return string.Equals(other, this, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Uri.ToLower().GetHashCode();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="EtpUri"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(EtpUri uri)
        {
            return uri.ToString();
        }

        /// <summary>
        /// Determines whether the specified URI is a root URI.
        /// </summary>
        /// <param name="uri">The URI string.</param>
        /// <returns><c>true</c> if the URI is a root URI; otherwise, <c>false</c>.</returns>
        public static bool IsRoot(string uri)
        {
            return RootUri.Equals(uri);
        }

        /// <summary>
        /// Gets the value contained within the specified match at the specified index.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <param name="index">The index.</param>
        /// <returns>The matched value found at the specified index.</returns>
        private static string GetValue(Match match, int index)
        {
            return match.Success && match.Groups.Count > index
                ? match.Groups[index].Value
                : null;
        }

        /// <summary>
        /// Determines whether the specified match contains repeating values.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns><c>true</c> if any repeating groups were matched; otherwise, <c>false</c>.</returns>
        private static bool HasRepeatValues(Match match)
        {
            return match.Success && match.Groups[9].Captures.Count > 0;
        }

        /// <summary>
        /// Formats the version number.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="family">The ML family.</param>
        /// <returns>A dot delimited version number.</returns>
        private static string FormatVersion(string version, string family)
        {
            if (string.IsNullOrWhiteSpace(version))
                return null;

            // Force WITSML versions 13 and 14 to be formatted as 1.3.1.1 and 1.4.1.1, respectively
            if ("WITSML".Equals(family, StringComparison.InvariantCultureIgnoreCase) && (version == "13" || version == "14"))
                version += "11";

            return string.Join(".", version.Trim().Select(x => x));
        }

        public struct Segment
        {
            public Segment(string schemaType, string objectType, string objectId)
            {
                SchemaType = schemaType;
                ObjectType = objectType;
                ObjectId = objectId;
            }

            public string SchemaType { get; }

            public string ObjectType { get; }

            public string ObjectId { get; }
        }
    }
}
