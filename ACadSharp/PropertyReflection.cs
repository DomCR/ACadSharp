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
    internal static class PropertyExpression<TClass, TAttribute>
        where TClass : class
    {
        public class Prop
        {
            public Func<TClass, object> Getter { get; set; }
            public Action<TClass, object> Setter { get; set; }
        }

        private static readonly Dictionary<string, Prop> Cache = new Dictionary<string, Prop>();

        static PropertyExpression()
        {
            var properties = typeof(TClass).GetProperties();
            foreach(var property in properties)
            {
                Type eventLogCustomType = property.DeclaringType;
                Type propertyType = property.PropertyType;

                var instance = LinqExpression.Parameter(typeof(TClass));

                Func<TClass, object> getter = null;
                var getMethod = property.GetGetMethod();
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
                var setMethod = property.GetSetMethod();
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
                Cache.Add(property.Name, new Prop
                {
                    Getter = getter,
                    Setter = setter,
                });
            }

        }

        public static Prop GetProperty(string propName)
        {
            return Cache[propName];
        }
    }
}
