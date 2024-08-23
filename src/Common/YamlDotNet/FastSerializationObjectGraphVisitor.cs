using System.Diagnostics.CodeAnalysis;

using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

/// <summary>
/// Graph visitor that uses <see cref="IYamlTypeConverter"/> instances the <see cref="IEmitter"/>.
/// </summary>
/// <remarks>
/// Should be removed if / when https://github.com/aaubry/YamlDotNet/pull/956 is released.
/// </remarks>
internal sealed class FastSerializationObjectGraphVisitor : ChainedObjectGraphVisitor
{
    private readonly TypeConverterCache _typeConverterCache;
    private readonly ObjectSerializer _nestedObjectSerializer;

    public FastSerializationObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor, IEnumerable<IYamlTypeConverter> typeConverters, ObjectSerializer nestedObjectSerializer)
        : base(nextVisitor)
    {
        _typeConverterCache = new(typeConverters);
        _nestedObjectSerializer = nestedObjectSerializer;
    }

    public override bool Enter(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
    {
        //propertydescriptor will be null on the root graph object
        if (propertyDescriptor?.ConverterType != null)
        {
            var converter = _typeConverterCache.GetConverterByType(propertyDescriptor.ConverterType);
            converter.WriteYaml(context, value.Value, value.Type, serializer);
            return false;
        }

        if (_typeConverterCache.TryGetConverterForType(value.Type, out var typeConverter))
        {
            typeConverter.WriteYaml(context, value.Value, value.Type, serializer);
            return false;
        }

        if (value.Value is IYamlConvertible convertible)
        {
            convertible.Write(context, _nestedObjectSerializer);
            return false;
        }

#pragma warning disable 0618 // IYamlSerializable is obsolete
        if (value.Value is IYamlSerializable serializable)
        {
            serializable.WriteYaml(context);
            return false;
        }
#pragma warning restore

        return base.Enter(propertyDescriptor, value, context, serializer);
    }

    /// <summary>
    /// A cache / map for <see cref="IYamlTypeConverter"/> instances.
    /// </summary>
    private sealed class TypeConverterCache
    {
        private readonly IYamlTypeConverter[] _typeConverters;
        private readonly Dictionary<Type, (bool HasMatch, IYamlTypeConverter TypeConverter)> _cache = [];

        public TypeConverterCache(IEnumerable<IYamlTypeConverter>? typeConverters) : this(typeConverters?.ToArray() ?? [])
        {
        }

        public TypeConverterCache(IYamlTypeConverter[] typeConverters)
        {
            _typeConverters = typeConverters;
        }

        /// <summary>
        /// Returns the first <see cref="IYamlTypeConverter"/> that accepts the given type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to lookup.</param>
        /// <param name="typeConverter">The <see cref="IYamlTypeConverter" /> that accepts this type or <see langword="false" /> if no converter is found.</param>
        /// <returns><see langword="true"/> if a type converter was found; <see langword="false"/> otherwise.</returns>
        public bool TryGetConverterForType(Type type, [NotNullWhen(true)] out IYamlTypeConverter? typeConverter)
        {
            if (_cache.TryGetValue(type, out var result))
            {
                typeConverter = result.TypeConverter;
                return result.HasMatch;
            }

            typeConverter = LookupTypeConverter(type);

            var found = typeConverter is not null;
            _cache[type] = (found, typeConverter!);

            return found;
        }

        /// <summary>
        /// Returns the <see cref="IYamlTypeConverter"/> of the given type.
        /// </summary>
        /// <param name="converter">The type of the converter.</param>
        /// <returns>The <see cref="IYamlTypeConverter"/> of the given type.</returns>
        /// <exception cref="ArgumentException">If no type converter of the given type is found.</exception>
        /// <remarks>
        /// Note that this method searches on the type of the <see cref="IYamlTypeConverter"/> itself. If you want to find a type converter
        /// that accepts a given <see cref="Type"/>, use <see cref="TryGetConverterForType(Type, out IYamlTypeConverter?)"/> instead.
        /// </remarks>
        public IYamlTypeConverter GetConverterByType(Type converter)
        {
            // Intentially avoids LINQ as this is on a hot path
            foreach (var typeConverter in _typeConverters)
            {
                if (typeConverter.GetType() == converter)
                {
                    return typeConverter;
                }
            }

            throw new ArgumentException($"{nameof(IYamlTypeConverter)} of type {converter.FullName} not found", nameof(converter));
        }

        private IYamlTypeConverter? LookupTypeConverter(Type type)
        {
#pragma warning disable S3267 // Loops should be simplified using the "Where" LINQ method -- Performance critical

            foreach (var typeConverter in _typeConverters)
#pragma warning restore S3267
            {
                if (typeConverter.Accepts(type))
                {
                    return typeConverter;
                }
            }

            return null;
        }
    }
}
