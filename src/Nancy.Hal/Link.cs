using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Nancy.Hal.Configuration;

namespace Nancy.Hal
{
    using System.Reflection;

    public class Link : IEquatable<Link>
    {
        private static readonly Regex IsTemplatedRegex = new Regex(@"{.+}", RegexOptions.Compiled);

        static Link()
        {
            // hax for Mono
            // http://www.mono-project.com/docs/faq/known-issues/urikind-relativeorabsolute/
            if (Type.GetType("Mono.Runtime") != null)
            {
                var field = typeof(Uri).GetField("useDotNetRelativeOrAbsolute",
                    BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);

                if (field != null)
                    field.SetValue(null, true);
            }
        }

        public Link()
        {}

        public Link(string rel, string href, string title = null)
        {
            Rel = rel;
            Href = href;
            Title = title;
        }

        public string Rel { get; private set; }

        public string Href { get; private set; }

        public string Title { get; private set; }

        public bool IsTemplated
        {
            get
            {
                return !string.IsNullOrEmpty(Href) && IsTemplatedRegex.IsMatch(Href);
            }
        }

        /// <summary>
        /// Changes the relationship of an existing link
        /// </summary>
        /// <param name="newRel">A different rel</param>
        /// <returns>A link with new rel</returns>
        public Link ChangeRel(string newRel)
        {
            return new Link(newRel, Href, Title);
        }

		/// <summary>
		/// If this link is templated, you can use this method to make a non-templated copy, providing a title.
		/// </summary>
		/// <param name="title">The title.</param>
		/// <param name="newRel">The new relative.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public Link CreateLink(string title, string newRel, params object[] parameters) 
		{
			var lnk = ChangeRel(newRel).CreateLink(parameters);
			lnk.Title = title;
			return lnk;
		}

        /// <summary>
        /// If this link is templated, you can use this method to make a non templated copy
        /// </summary>
        /// <param name="newRel">A different rel</param>
        /// <param name="parameters">The parameters, i.e 'new {id = "1"}'</param>
        /// <returns>A non templated link</returns>
        public Link CreateLink(string newRel, params object[] parameters)
        {
            return ChangeRel(newRel).CreateLink(parameters);
        }

        /// <summary>
        /// If this link is templated, you can use this method to make a non templated copy
        /// </summary>
        /// <param name="parameters">The parameters, i.e 'new {id = "1"}'</param>
        /// <returns>A non templated link</returns>
        public Link CreateLink(params object[] parameters)
        {
            return IsTemplated ? new Link(Rel, CreateUri(parameters).ToString(), Title) : this;
        }

        private Uri CreateUri(params object[] parameters)
        {
            return new Uri(SubstituteParams(Href, parameters), UriKind.RelativeOrAbsolute);
        }

        private static string SubstituteParams(string href, params object[] parameters)
        {
            var uriTemplate = new UriTemplate(href);
            foreach (var parameter in parameters)
            {
                var dictionary = parameter as IDictionary<string, object>; //should work for ExpandoObject, DynamicDictionary, etc
                if (dictionary != null)
                {
                    foreach (var substitution in dictionary.Keys)
                    {
                        var name = substitution.ToCamelCaseString();
                        var value = dictionary[substitution];
                        var substituionValue = value == null ? null : value.ToString();
                        uriTemplate.SetParameter(name, substituionValue);
                    }
                }
                else
                {
                    foreach (var substitution in parameter.GetType().GetProperties())
                    {
                        var name = substitution.Name.ToCamelCaseString();
                        var value = substitution.GetValue(parameter, null);
                        var substituionValue = value == null ? null : value.ToString();
                        uriTemplate.SetParameter(name, substituionValue);
                    }
                }
            }

            return uriTemplate.Resolve();
        }
    
        public bool Equals(Link other)
        {
            return string.Compare(Href, other.Href, StringComparison.OrdinalIgnoreCase) == 0 &&
                  string.Compare(Rel, other.Rel, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Link)obj);
        }

        public override int GetHashCode()
        {
            var str = (string.IsNullOrEmpty(Rel) ? "norel" : Rel) + "~" + (string.IsNullOrEmpty(Href) ? "nohref" : Href);
            var h = str.GetHashCode();
            return h;
        }

        public static bool operator ==(Link left, Link right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Link left, Link right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {                 
            return string.Format("Rel={0}, Href={1}", Rel, Href);
        }
    }
}