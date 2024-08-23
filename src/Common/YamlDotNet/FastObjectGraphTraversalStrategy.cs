using System.Collections;
using System.Diagnostics.CodeAnalysis;

using YamlDotNet.Core;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.ObjectGraphTraversalStrategies;

/// <summary>
/// An implementation of <see cref="IObjectGraphTraversalStrategy"/> that traverses
/// readable properties, collections and dictionaries.
/// </summary>
/// <remarks>
/// Should be deleted if / when these are released:
///   - https://github.com/aaubry/YamlDotNet/pull/955
/// </remarks>
internal sealed class FastObjectGraphTraversalStrategy : IObjectGraphTraversalStrategy
{
    private readonly int _maxRecursion;
    private readonly ITypeInspector _typeDescriptor;
    private readonly ITypeResolver _typeResolver;
    private readonly INamingConvention _namingConvention;
    private readonly IObjectFactory _objectFactory;

    public FastObjectGraphTraversalStrategy(ITypeInspector typeDescriptor, ITypeResolver typeResolver, int maxRecursion,
        INamingConvention namingConvention, IObjectFactory objectFactory)
    {
        if (maxRecursion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRecursion), maxRecursion, "maxRecursion must be greater than 1");
        }

        _typeDescriptor = typeDescriptor ?? throw new ArgumentNullException(nameof(typeDescriptor));
        _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));

        _maxRecursion = maxRecursion;
        _namingConvention = namingConvention ?? throw new ArgumentNullException(nameof(namingConvention));
        _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
    }

    public void Traverse<TContext>(IObjectDescriptor graph, IObjectGraphVisitor<TContext> visitor, TContext context, ObjectSerializer serializer)
    {
        Traverse(propertyDescriptor: null, graph, visitor, context, 0, serializer);
    }

    [DoesNotReturn]
    private static void MaximumRecursionThrowHelper()
    {
        throw new MaximumRecursionLevelReachedException("Too much recursion when traversing the object graph.");
    }

#pragma warning disable MA0051 // Method is too long -- Copy / paste from upstream; intented to be removed once upstream is fixed
    private void Traverse<TContext>(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, int recursionDepth, ObjectSerializer serializer)
#pragma warning restore MA0051 // Method is too long
    {
        if (recursionDepth >= _maxRecursion)
        {
            MaximumRecursionThrowHelper();
        }

        if (!visitor.Enter(propertyDescriptor, value, context, serializer))
        {
            return;
        }

        recursionDepth++;

            var typeCode = Type.GetTypeCode(value.Type);
        switch (typeCode)
        {
            case TypeCode.Boolean:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
            case TypeCode.String:
            case TypeCode.Char:
            case TypeCode.DateTime:
                visitor.VisitScalar(value, context, serializer);
                break;

            case TypeCode.Empty:
                throw new NotSupportedException($"TypeCode.{typeCode} is not supported.");

            default:
                if (value.Value == null || value.Type == typeof(TimeSpan))
                {
                    visitor.VisitScalar(value, context, serializer);
                    break;
                }

                var nullableUnderlyingType = Nullable.GetUnderlyingType(value.Type);
                var optionUnderlyingType = nullableUnderlyingType ?? FsharpHelper.GetOptionUnderlyingType(value.Type);
                var optionValue = optionUnderlyingType != null ? FsharpHelper.GetValue(value) : null;

                if (nullableUnderlyingType != null)
                {
                    // This is a nullable type, recursively handle it with its underlying type.
                    // Note that if it contains null, the condition above already took care of it
                    Traverse(
                        propertyDescriptor,
                        new ObjectDescriptor(value.Value, nullableUnderlyingType, value.Type, value.ScalarStyle),
                        visitor,
                        context,
                        recursionDepth,
                        serializer
                    );
                }
                else if (optionUnderlyingType != null && optionValue != null)
                {
                    Traverse(
                        propertyDescriptor,
                        new ObjectDescriptor(FsharpHelper.GetValue(value), optionUnderlyingType, value.Type, value.ScalarStyle),
                        visitor,
                        context,
                        recursionDepth,
                        serializer
                    );
                }
                else
                {
                    TraverseObject(propertyDescriptor, value, visitor, context, recursionDepth, serializer);
                }
                break;
        }
    }

    private void TraverseObject<TContext>(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, int recursionDepth, ObjectSerializer serializer)
    {
        if (typeof(IDictionary).IsAssignableFrom(value.Type))
        {
            TraverseDictionary(propertyDescriptor, value, visitor, typeof(object), typeof(object), context, recursionDepth, serializer);
            return;
        }

        if (_objectFactory.GetDictionary(value, out var adaptedDictionary, out var genericArguments))
        {
            TraverseDictionary(propertyDescriptor, new ObjectDescriptor(adaptedDictionary, value.Type, value.StaticType, value.ScalarStyle), visitor, genericArguments![0], genericArguments[1], context, recursionDepth, serializer);
            return;
        }

        if (typeof(IEnumerable).IsAssignableFrom(value.Type))
        {
            TraverseList(propertyDescriptor!, value, visitor, context, recursionDepth, serializer);
            return;
        }

        TraverseProperties(value, visitor, context, recursionDepth, serializer);
    }

    private void TraverseDictionary<TContext>(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor dictionary, IObjectGraphVisitor<TContext> visitor, Type keyType, Type valueType, TContext context, int recursionDepth, ObjectSerializer serializer)
    {
        visitor.VisitMappingStart(dictionary, keyType, valueType, context, serializer);

        var isDynamic = dictionary.Type.FullName!.Equals("System.Dynamic.ExpandoObject", StringComparison.Ordinal);
        foreach (DictionaryEntry? entry in (IDictionary)dictionary.NonNullValue())
        {
            var entryValue = entry!.Value;
            var keyValue = isDynamic ? _namingConvention.Apply(entryValue.Key.ToString()!) : entryValue.Key;
            var key = GetObjectDescriptor(keyValue, keyType);
            var value = GetObjectDescriptor(entryValue.Value, valueType);

            if (visitor.EnterMapping(key, value, context, serializer))
            {
                Traverse(propertyDescriptor, key, visitor, context, recursionDepth, serializer);
                Traverse(propertyDescriptor, value, visitor, context, recursionDepth, serializer);
            }
        }

        visitor.VisitMappingEnd(dictionary, context, serializer);
    }

    private void TraverseList<TContext>(IPropertyDescriptor propertyDescriptor, IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, int recursionDepth, ObjectSerializer serializer)
    {
        var itemType = _objectFactory.GetValueType(value.Type);

        visitor.VisitSequenceStart(value, itemType, context, serializer);

        foreach (var item in (IEnumerable)value.NonNullValue())
        {
            Traverse(propertyDescriptor, GetObjectDescriptor(item, itemType), visitor, context, recursionDepth, serializer);
        }

        visitor.VisitSequenceEnd(value, context, serializer);
    }

    private void TraverseProperties<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, int recursionDepth, ObjectSerializer serializer)
    {
        if (context is not Nothing)
        {
            _objectFactory.ExecuteOnSerializing(value.Value!);
        }

        visitor.VisitMappingStart(value, typeof(string), typeof(object), context, serializer);

        var source = value.NonNullValue();
        foreach (var propertyDescriptor in _typeDescriptor.GetProperties(value.Type, source))
        {
            var propertyValue = propertyDescriptor.Read(source);
            if (visitor.EnterMapping(propertyDescriptor, propertyValue, context, serializer))
            {
                Traverse(propertyDescriptor: null, new ObjectDescriptor(propertyDescriptor.Name, typeof(string), typeof(string), ScalarStyle.Plain), visitor, context, recursionDepth, serializer);
                Traverse(propertyDescriptor, propertyValue, visitor, context, recursionDepth, serializer);
            }
        }

        visitor.VisitMappingEnd(value, context, serializer);

        if (context is not Nothing)
        {
            _objectFactory.ExecuteOnSerialized(value.Value!);
        }
    }

    private ObjectDescriptor GetObjectDescriptor(object? value, Type staticType)
    {
        return new ObjectDescriptor(value, _typeResolver.Resolve(staticType, value), staticType);
    }
}
