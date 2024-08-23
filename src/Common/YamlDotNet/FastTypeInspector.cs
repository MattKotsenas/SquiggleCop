using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

using YamlDotNet.Core;
using YamlDotNet.Serialization.TypeResolvers;

namespace YamlDotNet.Serialization.TypeInspectors;

/// <summary>
/// Returns the properties of a type that are readable.
/// </summary>
/// <remarks>
/// Delete this if / when https://github.com/aaubry/YamlDotNet/pull/953 is released.
/// </remarks>
internal sealed class FastTypeInspector : ITypeInspector
{
    private static readonly ConcurrentDictionary<Type, IEnumerable<IPropertyDescriptor>> PropertiesCache = new();

    public string GetEnumName(Type enumType, string name)
    {
        return Enum.GetName(enumType, name)!;
    }

    public string GetEnumValue(object enumValue)
    {
        return enumValue == null ? string.Empty : enumValue.ToString()!;
    }

    public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        return PropertiesCache.GetOrAdd(type, GetPropertiesCore);
    }

    private static IEnumerable<IPropertyDescriptor> GetPropertiesCore(Type type)
    {
        return type
            .GetProperties()
            .Where(IsValidProperty)
            .Select(p => (IPropertyDescriptor)new ReflectionPropertyDescriptor(p));
    }

    private static bool IsValidProperty(PropertyInfo property)
    {
        return property.CanRead
            && property.GetGetMethod(nonPublic: true)!.GetParameters().Length == 0;
    }

    public IPropertyDescriptor GetProperty(Type type, object? container, string name, [MaybeNullWhen(true)] bool ignoreUnmatched, bool caseInsensitivePropertyMatching)
    {
        IEnumerable<IPropertyDescriptor> candidates;

        if (caseInsensitivePropertyMatching)
        {
            candidates = GetProperties(type, container)
                .Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            candidates = GetProperties(type, container)
                .Where(p => string.Equals(p.Name, name, StringComparison.Ordinal));
        }

        using var enumerator = candidates.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return ignoreUnmatched ? null! : throw new SerializationException($"Property '{name}' not found on type '{type.FullName}'.");
        }

        var property = enumerator.Current;

        if (enumerator.MoveNext())
        {
            throw new SerializationException(
                $"Multiple properties with the name/alias '{name}' already exists on type '{type.FullName}', maybe you're misusing YamlAlias or maybe you are using the wrong naming convention? The matching properties are: {string.Join(", ", candidates.Select(p => p.Name).ToArray())}"
            );
        }

        return property;
    }

    private sealed class ReflectionPropertyDescriptor : IPropertyDescriptor
    {
        private readonly PropertyInfo _propertyInfo;
        private static readonly DynamicTypeResolver TypeResolver = new();
        private static readonly ConcurrentDictionary<PropertyInfo, Attribute[]> PropertyInfoAttributeCache = new();

        public ReflectionPropertyDescriptor(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
            ScalarStyle = ScalarStyle.Any;
            YamlConverterAttribute? converterAttribute = FirstOrDefaultOfType<YamlConverterAttribute>(GetCustomAttributes(propertyInfo));
            if (converterAttribute != null)
            {
                ConverterType = converterAttribute.ConverterType;
            }
        }

        public string Name => _propertyInfo.Name;
        public bool Required { get => Array.Exists(_propertyInfo.GetCustomAttributes(inherit: true), static x => string.Equals(x.GetType().FullName, "System.Runtime.CompilerServices.RequiredMemberAttribute", StringComparison.Ordinal)); }
        public Type Type => _propertyInfo.PropertyType;
        public Type? TypeOverride { get; set; }
        public Type? ConverterType { get; set; }

        public bool AllowNulls { get => Array.Exists(_propertyInfo.GetCustomAttributes(inherit: true), static x => string.Equals(x.GetType().FullName, "System.Runtime.CompilerServices.NullableContextAttribute", StringComparison.Ordinal)); }

        public int Order { get; set; }
        public bool CanWrite => _propertyInfo.CanWrite;
        public ScalarStyle ScalarStyle { get; set; }

        public void Write(object target, object? value)
        {
            _propertyInfo.SetValue(target, value, index: null);
        }

        public T? GetCustomAttribute<T>() where T : Attribute
        {
            return FirstOrDefaultOfType<T>(GetCustomAttributes(_propertyInfo));
        }

        private static T FirstOrDefaultOfType<T>(Attribute[] attributes) where T : Attribute
        {
            foreach (Attribute attribute in attributes)
            {
                if (attribute is T t)
                {
                    return t;
                }
            }

            return null!;
        }

        public IObjectDescriptor Read(object target)
        {
            object? propertyValue = _propertyInfo.GetValue(target, index: null);
            Type? actualType = TypeOverride ?? TypeResolver.Resolve(Type, propertyValue);
            return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
        }

        private static Attribute[] GetCustomAttributes(PropertyInfo propertyInfo)
        {
            return PropertyInfoAttributeCache.GetOrAdd(propertyInfo, p => p.GetCustomAttributes().ToArray());
        }
    }
}