namespace Nancy.Hal.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class Extensions
    {
        internal static string ToCamelCaseString(this string input)
        {
            if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0])) return input;

            string lowerCasedFirstChar =
                char.ToLower(input[0], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

            if (input.Length > 1) lowerCasedFirstChar = lowerCasedFirstChar + input.Substring(1);

            return lowerCasedFirstChar;
        }

        internal static PropertyInfo ExtractProperty(Expression expression)
        {
            return new PropertyFinder().Find(expression);
        }

        internal static T CreateDelegate<T>(MethodInfo methodInfo, object target)
        {
            if (target != null)
            {
                return (T)(object)Delegate.CreateDelegate(typeof(T), target, methodInfo);
            }

            return (T)(object)Delegate.CreateDelegate(typeof(T), methodInfo);
        }

        private class PropertyFinder : ExpressionVisitor
        {
            private PropertyInfo _found;

            protected override Expression VisitMember(MemberExpression node)
            {
                this._found = node.Member as PropertyInfo;
                return base.VisitMember(node);
            }

            public PropertyInfo Find(Expression exp)
            {
                this.Visit(exp);
                return this._found;
            }
        }
    }

    public static class DynamicExtensions
    {
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
            {
                expando.Add(property.Name, property.GetValue(value));
            }

            return expando as ExpandoObject;
        }
    }
}