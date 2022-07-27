using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LinqExpression = System.Linq.Expressions.Expression;


namespace ACadSharp
{
    internal class PropertyExpression<TClass, TAttribute>
        where TClass : class
        where TAttribute : Attribute
    {
        public class Prop
        {
            public Func<TClass, object> Getter { get; internal set; }

            public Action<TClass, object> Setter { get; internal set; }

            public TAttribute Attribute { get; internal set; }

            public PropertyInfo Property { get; internal set; }
        }

        private static readonly ConcurrentDictionary<string, Prop> _cache = new ConcurrentDictionary<string, Prop>();

        public IReadOnlyDictionary<string, Prop> Cache = _cache;

        public PropertyExpression(Func<PropertyInfo, TAttribute, string> keySelector)
        {
            var properties = typeof(TClass).GetProperties();
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<TAttribute>();
                if (attribute == null)
                    continue;

                var eventLogCustomType = property.DeclaringType;
                var propertyType = property.PropertyType;

                var instance = LinqExpression.Parameter(typeof(TClass));

                Func<TClass, object> getter = null;
                var getMethod = property.GetGetMethod(true);
                if (getMethod != null)
                {
                    getter =
                        LinqExpression.Lambda<Func<TClass, object>>(
                                LinqExpression.Convert(
                                    LinqExpression.Call(
                                        LinqExpression.Convert(instance, eventLogCustomType),
                                        getMethod),
                                    typeof(object)),
                                instance)
                            .Compile();
                }

                Action<TClass, object> setter = null;
                var setMethod = property.GetSetMethod(true);
                if (setMethod != null)
                {
                    var parameter = LinqExpression.Parameter(typeof(object));
                    setter =
                        LinqExpression.Lambda<Action<TClass, object>>(
                                LinqExpression.Call(
                                    LinqExpression.Convert(instance, eventLogCustomType),
                                    setMethod,
                                    LinqExpression.Convert(parameter, propertyType)),
                                instance, parameter)
                            .Compile();
                }

                _cache.TryAdd(keySelector(property, attribute), new Prop
                {
                    Getter = getter,
                    Setter = setter,
                    Property = property
                });
            }
        }

        public Prop GetProperty(string propName)
        {
            return _cache[propName];
        }
    }
}
