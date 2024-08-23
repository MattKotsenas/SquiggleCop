using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace YamlDotNet.Serialization.ObjectFactories;

/// <summary>
/// Creates objects using Activator.CreateInstance.
/// </summary>
/// <remarks>
/// Should be deleted if / when https://github.com/aaubry/YamlDotNet/pull/955 is released
/// </remarks>
internal sealed class FastObjectFactory : IObjectFactory
{
    private static readonly ConcurrentDictionary<Type, Type[]> TypeInterfaceCache = new();
    private static readonly ConcurrentDictionary<Type, Type> EnumerableTypeCache = new();

    private static readonly Dictionary<Type, Dictionary<Type, MethodInfo[]>> StateMethods = new()
        {
            { typeof(OnDeserializedAttribute), new Dictionary<Type, MethodInfo[]>() },
            { typeof(OnDeserializingAttribute), new Dictionary<Type, MethodInfo[]>() },
            { typeof(OnSerializedAttribute), new Dictionary<Type, MethodInfo[]>() },
            { typeof(OnSerializingAttribute), new Dictionary<Type, MethodInfo[]>() },
        };

    private static readonly Dictionary<Type, Type> DefaultGenericInterfaceImplementations = new()
        {
            { typeof(IEnumerable<>), typeof(List<>) },
            { typeof(ICollection<>), typeof(List<>) },
            { typeof(IList<>), typeof(List<>) },
            { typeof(IDictionary<,>), typeof(Dictionary<,>) },
        };

    private static readonly Dictionary<Type, Type> DefaultNonGenericInterfaceImplementations = new()
        {
            { typeof(IEnumerable), typeof(List<object>) },
            { typeof(ICollection), typeof(List<object>) },
            { typeof(IList), typeof(List<object>) },
            { typeof(IDictionary), typeof(Dictionary<object, object>) },
        };

    private readonly Settings _settings;

    public FastObjectFactory() : this(new Dictionary<Type, Type>(), new Settings())
    {
    }

    public FastObjectFactory(IDictionary<Type, Type> mappings) : this(mappings, new Settings())
    {
    }

    public FastObjectFactory(IDictionary<Type, Type> mappings, Settings settings)
    {
        foreach (var pair in mappings)
        {
            if (!pair.Key.IsAssignableFrom(pair.Value))
            {
                throw new InvalidOperationException($"Type '{pair.Value}' does not implement type '{pair.Key}'.");
            }

            DefaultNonGenericInterfaceImplementations.Add(pair.Key, pair.Value);
        }

        _settings = settings;
    }

    public object Create(Type type)
    {
        if (type.GetTypeInfo().IsInterface)
        {
            if (type.GetTypeInfo().IsGenericType)
            {
                if (DefaultGenericInterfaceImplementations.TryGetValue(type.GetGenericTypeDefinition(), out var implementationType))
                {
                    type = implementationType.MakeGenericType(type.GetGenericArguments());
                }
            }
            else
            {
                if (DefaultNonGenericInterfaceImplementations.TryGetValue(type, out var implementationType))
                {
                    type = implementationType;
                }
            }
        }

        try
        {
            return Activator.CreateInstance(type, _settings.AllowPrivateConstructors)!;
        }
        catch (Exception err)
        {
            var message = $"Failed to create an instance of type '{type.FullName}'.";
            throw new InvalidOperationException(message, err);
        }
    }

    public void ExecuteOnDeserialized(object value) => ExecuteState(typeof(OnDeserializedAttribute), value);

    public void ExecuteOnDeserializing(object value) => ExecuteState(typeof(OnDeserializingAttribute), value);

    public void ExecuteOnSerialized(object value) => ExecuteState(typeof(OnSerializedAttribute), value);

    public void ExecuteOnSerializing(object value) => ExecuteState(typeof(OnSerializingAttribute), value);

    private static void ExecuteState(Type attributeType, object value)
    {
        if (value == null)
        {
            return;
        }

        var type = value.GetType();
        var methodsToExecute = GetStateMethods(attributeType, type);

        foreach (var method in methodsToExecute)
        {
            method.Invoke(value, parameters: null);
        }
    }

    private static MethodInfo[] GetStateMethods(Type attributeType, Type valueType)
    {
        var stateDictionary = StateMethods[attributeType];

        if (stateDictionary.TryGetValue(valueType, out var methods))
        {
            return methods;
        }

        methods = valueType.GetMethods(BindingFlags.Public |
                                      BindingFlags.Instance);
        methods = methods.Where(x => x.GetCustomAttributes(attributeType, inherit: true).Length > 0).ToArray();
        stateDictionary[valueType] = methods;
        return methods;
    }

    public object? CreatePrimitive(Type type) => type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;

    private static Type? GetImplementationOfOpenGenericInterface(Type type, Type openGenericType)
    {
        if (!openGenericType.IsGenericType || !openGenericType.IsInterface)
        {
            // Note we can likely relax this constraint to also allow for matching other types
            throw new ArgumentException("The type must be a generic type definition and an interface", nameof(openGenericType));
        }

        // First check if the type itself is the open generic type
        if (IsGenericDefinitionOfType(type, openGenericType))
        {
            return type;
        }

        // Then check the interfaces
#pragma warning disable S3267 // Loops should be simplified by using the "Where" LINQ method -- Performance critical
        foreach (Type i in TypeInterfaceCache.GetOrAdd(type, static t => t.GetInterfaces()))
#pragma warning restore S3267 // Loops should be simplified by using the "Where" LINQ method
        {
            if (IsGenericDefinitionOfType(i, openGenericType))
            {
                return i;
            }
        }

        return null;

        static bool IsGenericDefinitionOfType(Type t, object? context)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == context as Type;
        }
    }

    public bool GetDictionary(IObjectDescriptor descriptor, out IDictionary? dictionary, out Type[]? genericArguments)
    {
        var genericDictionaryType = GetImplementationOfOpenGenericInterface(descriptor.Type, typeof(IDictionary<,>));
        if (genericDictionaryType != null)
        {
            genericArguments = genericDictionaryType.GetGenericArguments();
            var adaptedDictionary = Activator.CreateInstance(typeof(GenericDictionaryToNonGenericAdapter<,>).MakeGenericType(genericArguments), descriptor.Value)!;
            dictionary = adaptedDictionary as IDictionary;
            return true;
        }
        genericArguments = null;
        dictionary = null;
        return false;
    }

    public Type GetValueType(Type type)
    {
        var enumerableType = GetImplementationOfOpenGenericInterface(type, typeof(IEnumerable<>));
        if (enumerableType != null)
        {
            return EnumerableTypeCache.GetOrAdd(enumerableType, et => et.GetGenericArguments()[0]);
        }

        return typeof(object);
    }

    /// <summary>
    /// Adapts an <see cref="IDictionary{TKey, TValue}" /> to <see cref="IDictionary" />
    /// because not all generic dictionaries implement <see cref="IDictionary" />.
    /// </summary>
    private sealed class GenericDictionaryToNonGenericAdapter<TKey, TValue> : IDictionary
        where TKey : notnull
    {
        private readonly IDictionary<TKey, TValue> _genericDictionary;

#pragma warning disable S1144 // Unused private types or members should be removed -- Used by reflection
        public GenericDictionaryToNonGenericAdapter(IDictionary<TKey, TValue> genericDictionary)
        {
            _genericDictionary = genericDictionary ?? throw new ArgumentNullException(nameof(genericDictionary));
        }
#pragma warning restore S1144 // Unused private types or members should be removed

        public void Add(object key, object? value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object key)
        {
            throw new NotSupportedException();
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new DictionaryEnumerator(_genericDictionary.GetEnumerator());
        }

        public bool IsFixedSize
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotSupportedException(); }
        }

        public ICollection Keys
        {
            get { throw new NotSupportedException(); }
        }

        public void Remove(object key)
        {
            throw new NotSupportedException();
        }

        public ICollection Values
        {
            get { throw new NotSupportedException(); }
        }

        public object? this[object key]
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                _genericDictionary[(TKey)key] = (TValue)value!;
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsSynchronized
        {
            get { throw new NotSupportedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                _enumerator = enumerator;
            }

            public DictionaryEntry Entry
            {
                get
                {
                    return new DictionaryEntry(Key, Value);
                }
            }

            public object Key
            {
                get { return _enumerator.Current.Key!; }
            }

            public object? Value
            {
                get { return _enumerator.Current.Value; }
            }

            public object Current
            {
                get { return Entry; }
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}
