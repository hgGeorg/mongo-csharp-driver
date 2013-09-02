﻿/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for interfaces and base classes that delegates to the actual type interface without writing a discriminator.
    /// </summary>
    public class UndiscriminatedActualTypeSerializer : BsonBaseSerializer
    {
        // private static fields
        private static readonly IBsonSerializer __instance = new UndiscriminatedActualTypeSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UndiscriminatedActualTypeSerializer"/> class.
        /// </summary>
        public UndiscriminatedActualTypeSerializer()
            : base(typeof(object))
        {
        }

        // public static properties
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IBsonSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The document.</param>
        public override void Serialize(SerializationContext context, object value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                var serializer = BsonSerializer.LookupSerializer(actualType);
                context.SerializeWithChildContext(serializer, value);
            }
        }
    }

    /// <summary>
    /// Represents a serializer for interfaces and base classes that delegates to the actual type interface without writing a discriminator.
    /// </summary>
    public class UndiscriminatedActualTypeSerializer<TDocument> : BsonBaseSerializer<TDocument>
    {
        // private static fields
        private static readonly IBsonSerializer<TDocument> __instance = new UndiscriminatedActualTypeSerializer<TDocument>();

        // public static properties
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IBsonSerializer<TDocument> Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The document.</param>
        public override void Serialize(SerializationContext context, TDocument value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                var serializer = BsonSerializer.LookupSerializer(actualType);
                context.SerializeWithChildContext(serializer, value);
            }
        }
    }
}