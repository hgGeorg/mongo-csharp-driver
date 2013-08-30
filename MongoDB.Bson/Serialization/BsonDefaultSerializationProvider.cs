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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Net;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents the default serialization provider.
    /// </summary>
    internal class BsonDefaultSerializationProvider : IBsonSerializationProvider
    {
        // private static fields
        private static Dictionary<Type, Type> __serializers;
        private static Dictionary<Type, Type> __genericSerializerDefinitions;

        // static constructor
        static BsonDefaultSerializationProvider()
        {
            __serializers = new Dictionary<Type, Type>
            {
                { typeof(BitArray), typeof(BitArraySerializer) },
                { typeof(Boolean), typeof(BooleanSerializer) },
                { typeof(BsonArray), typeof(BsonArraySerializer) },
                { typeof(BsonBinaryData), typeof(BsonBinaryDataSerializer) },
                { typeof(BsonBoolean), typeof(BsonBooleanSerializer) },
                { typeof(BsonDateTime), typeof(BsonDateTimeSerializer) },
                { typeof(BsonDocument), typeof(BsonDocumentSerializer) },
                { typeof(BsonDocumentWrapper), typeof(BsonDocumentWrapperSerializer) },
                { typeof(BsonDouble), typeof(BsonDoubleSerializer) },
                { typeof(BsonInt32), typeof(BsonInt32Serializer) },
                { typeof(BsonInt64), typeof(BsonInt64Serializer) },
                { typeof(BsonJavaScript), typeof(BsonJavaScriptSerializer) },
                { typeof(BsonJavaScriptWithScope), typeof(BsonJavaScriptWithScopeSerializer) },
                { typeof(BsonMaxKey), typeof(BsonMaxKeySerializer) },
                { typeof(BsonMinKey), typeof(BsonMinKeySerializer) },
                { typeof(BsonNull), typeof(BsonNullSerializer) },
                { typeof(BsonObjectId), typeof(BsonObjectIdSerializer) },
                { typeof(BsonRegularExpression), typeof(BsonRegularExpressionSerializer) },
                { typeof(BsonString), typeof(BsonStringSerializer) },
                { typeof(BsonSymbol), typeof(BsonSymbolSerializer) },
                { typeof(BsonTimestamp), typeof(BsonTimestampSerializer) },
                { typeof(BsonUndefined), typeof(BsonUndefinedSerializer) },
                { typeof(BsonValue), typeof(BsonValueSerializer) },
                { typeof(Byte), typeof(ByteSerializer) },
                { typeof(Byte[]), typeof(ByteArraySerializer) },
                { typeof(Char), typeof(CharSerializer) },
                { typeof(CultureInfo), typeof(CultureInfoSerializer) },
                { typeof(DateTime), typeof(DateTimeSerializer) },
                { typeof(DateTimeOffset), typeof(DateTimeOffsetSerializer) },
                { typeof(Decimal), typeof(DecimalSerializer) },
                { typeof(Double), typeof(DoubleSerializer) },
                { typeof(System.Drawing.Size), typeof(DrawingSizeSerializer) },
                { typeof(Guid), typeof(GuidSerializer) },
                { typeof(Int16), typeof(Int16Serializer) },
                { typeof(Int32), typeof(Int32Serializer) },
                { typeof(Int64), typeof(Int64Serializer) },
                { typeof(IPAddress), typeof(IPAddressSerializer) },
                { typeof(IPEndPoint), typeof(IPEndPointSerializer) },
                { typeof(Object), typeof(ObjectSerializer) },
                { typeof(ObjectId), typeof(ObjectIdSerializer) },
                { typeof(Queue), typeof(QueueSerializer) },
                { typeof(SByte), typeof(SByteSerializer) },
                { typeof(Single), typeof(SingleSerializer) },
                { typeof(Stack), typeof(StackSerializer) },
                { typeof(String), typeof(StringSerializer) },
                { typeof(TimeSpan), typeof(TimeSpanSerializer) },
                { typeof(UInt16), typeof(UInt16Serializer) },
                { typeof(UInt32), typeof(UInt32Serializer) },
                { typeof(UInt64), typeof(UInt64Serializer) },
                { typeof(Uri), typeof(UriSerializer) },
                { typeof(Version), typeof(VersionSerializer) }
            };

            __genericSerializerDefinitions = new Dictionary<Type, Type>
            {
                { typeof(KeyValuePair<,>), typeof(KeyValuePairSerializer<,>) },
                { typeof(Nullable<>), typeof(NullableSerializer<>) },
                { typeof(Queue<>), typeof(QueueSerializer<>) },
                { typeof(ReadOnlyCollection<>), typeof(ReadOnlyCollectionSerializer<>) },
                { typeof(Stack<>), typeof(StackSerializer<>) }
            };
        }

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDefaultSerializer class.
        /// </summary>
        public BsonDefaultSerializationProvider()
        {
        }

        // public methods
        /// <summary>
        /// Gets the serializer for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The serializer.</returns>
        public IBsonSerializer GetSerializer(Type type)
        {
            Type serializerType;
            if (__serializers.TryGetValue(type, out serializerType))
            {
                return (IBsonSerializer)Activator.CreateInstance(serializerType);
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                Type genericSerializerDefinition;
                if (__genericSerializerDefinitions.TryGetValue(genericTypeDefinition, out genericSerializerDefinition))
                {
                    var genericSerializerType = genericSerializerDefinition.MakeGenericType(type.GetGenericArguments());
                    return (IBsonSerializer)Activator.CreateInstance(genericSerializerType);
                }
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                switch (type.GetArrayRank())
                {
                    case 1:
                        var arraySerializerDefinition = typeof(ArraySerializer<>);
                        var arraySerializerType = arraySerializerDefinition.MakeGenericType(elementType);
                        return (IBsonSerializer)Activator.CreateInstance(arraySerializerType);
                    case 2:
                        var twoDimensionalArraySerializerDefinition = typeof(TwoDimensionalArraySerializer<>);
                        var twoDimensionalArraySerializerType = twoDimensionalArraySerializerDefinition.MakeGenericType(elementType);
                        return (IBsonSerializer)Activator.CreateInstance(twoDimensionalArraySerializerType);
                    case 3:
                        var threeDimensionalArraySerializerDefinition = typeof(ThreeDimensionalArraySerializer<>);
                        var threeDimensionalArraySerializerType = threeDimensionalArraySerializerDefinition.MakeGenericType(elementType);
                        return (IBsonSerializer)Activator.CreateInstance(threeDimensionalArraySerializerType);
                    default:
                        var message = string.Format("No serializer found for array for rank {0}.", type.GetArrayRank());
                        throw new BsonSerializationException(message);
                }
            }

            if (type.IsEnum)
            {
                var enumSerializerDefinition = typeof(EnumSerializer<>);
                var enumSerializerType = enumSerializerDefinition.MakeGenericType(type);
                return (IBsonSerializer)Activator.CreateInstance(enumSerializerType);
            }

            // classes that implement IDictionary or IEnumerable are serialized using either DictionarySerializer or EnumerableSerializer
            // this does mean that any additional public properties the class might have won't be serialized (just like the XmlSerializer)
            var collectionSerializer = GetCollectionSerializer(type);
            if (collectionSerializer != null)
            {
                return collectionSerializer;
            }

            // interface values will be written with a discriminator so they can be deserialized
            if (type.IsInterface)
            {
                var discriminatedInterfaceSerializerDefinition = typeof(DiscriminatedInterfaceSerializer<>);
                var discriminatedInterfaceSerializerType = discriminatedInterfaceSerializerDefinition.MakeGenericType(new[] { type });
                return (IBsonSerializer)Activator.CreateInstance(discriminatedInterfaceSerializerType);
            }

            return null;
        }

        // private methods
        private IBsonSerializer GetCollectionSerializer(Type type)
        {
            Type implementedGenericDictionaryInterface = null;
            Type implementedGenericEnumerableInterface = null;
            Type implementedDictionaryInterface = null;
            Type implementedEnumerableInterface = null;

            var implementedInterfaces = new List<Type>(type.GetInterfaces());
            if (type.IsInterface)
            {
                implementedInterfaces.Add(type);
            }

            foreach (var implementedInterface in implementedInterfaces)
            {
                if (implementedInterface.IsGenericType)
                {
                    var genericInterfaceDefinition = implementedInterface.GetGenericTypeDefinition();
                    if (genericInterfaceDefinition == typeof(IDictionary<,>))
                    {
                        implementedGenericDictionaryInterface = implementedInterface;
                    }
                    if (genericInterfaceDefinition == typeof(IEnumerable<>))
                    {
                        implementedGenericEnumerableInterface = implementedInterface;
                    }
                }
                else
                {
                    if (implementedInterface == typeof(IDictionary))
                    {
                        implementedDictionaryInterface = implementedInterface;
                    }
                    if (implementedInterface == typeof(IEnumerable))
                    {
                        implementedEnumerableInterface = implementedInterface;
                    }
                }
            }

            // the order of the tests is important
            if (implementedGenericDictionaryInterface != null)
            {
                var keyType = implementedGenericDictionaryInterface.GetGenericArguments()[0];
                var valueType = implementedGenericDictionaryInterface.GetGenericArguments()[1];
                if (type.IsInterface)
                {
                    var dictionaryDefinition = typeof(Dictionary<,>);
                    var dictionaryType = dictionaryDefinition.MakeGenericType(keyType, valueType);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    var serializerType = serializerDefinition.MakeGenericType(type, dictionaryType);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
                else
                {
                    var serializerDefinition = typeof(IDictionaryImplementerSerializer<,,>);
                    var serializerType = serializerDefinition.MakeGenericType(type, keyType, valueType);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
            }
            else if (implementedDictionaryInterface != null)
            {
                if (type.IsInterface)
                {
                    var dictionaryType = typeof(Hashtable);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    var serializerType = serializerDefinition.MakeGenericType(type, dictionaryType);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
                else
                {
                    var serializerDefinition = typeof(IDictionaryImplementerSerializer<>);
                    var serializerType = serializerDefinition.MakeGenericType(type);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
            }
            else if (implementedGenericEnumerableInterface != null)
            {
                var itemType = implementedGenericEnumerableInterface.GetGenericArguments()[0];

                var readOnlyCollectionType = typeof(ReadOnlyCollection<>).MakeGenericType(itemType);
                if (type == readOnlyCollectionType)
                {
                    var serializerDefinition = typeof(ReadOnlyCollectionSerializer<>);
                    var serializerType = serializerDefinition.MakeGenericType(itemType);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
                else if (readOnlyCollectionType.IsAssignableFrom(type))
                {
                    var serializerDefinition = typeof(ReadOnlyCollectionSubclassSerializer<,>);
                    var serializerType = serializerDefinition.MakeGenericType(type, itemType);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
                else if (type.IsInterface)
                {
                    var listDefinition = typeof(List<>);
                    var listType = listDefinition.MakeGenericType(itemType);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    var serializerType = serializerDefinition.MakeGenericType(type, listType);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
                else
                {
                    var serializerDefinition = typeof(IEnumerableImplementerSerializer<,>);
                    var serializerType = serializerDefinition.MakeGenericType(type, itemType);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
            }
            else if (implementedEnumerableInterface != null)
            {
                if (type.IsInterface)
                {
                    var listType = typeof(ArrayList);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    var serializerType = serializerDefinition.MakeGenericType(type, listType);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
                else
                {
                    var serializerDefinition = typeof(IEnumerableImplementerSerializer<>);
                    var serializerType = serializerDefinition.MakeGenericType(type);
                    return (IBsonSerializer)Activator.CreateInstance(serializerType);
                }
            }

            return null;
        }
    }
}
