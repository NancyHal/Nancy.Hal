namespace Nancy.Hal.Configuration
{
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
                CultureInfo.InvariantCulture.TextInfo.ToLower(input[0]).ToString();

            if (input.Length > 1) lowerCasedFirstChar = lowerCasedFirstChar + input.Substring(1);

            return lowerCasedFirstChar;
        }

        internal static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
            {
                expando.Add(property.Name, property.GetValue(value));
            }

            return expando as ExpandoObject;
        }

        public static PropertyInfo ExtractPropertyInfo(this Expression expression)
        {
            return new PropertyFinder().Find(expression);
        }

        private class PropertyFinder : ExpressionVisitor
        {
            private PropertyInfo _found;

            protected override Expression VisitMember(MemberExpression node)
            {
                _found = node.Member as PropertyInfo;
                return base.VisitMember(node);
            }

            public PropertyInfo Find(Expression exp)
            {
                Visit(exp);
                return _found;
            }
        }
    }
}