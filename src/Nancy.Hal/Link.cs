namespace Nancy.Hal
{
    using System;
    using System.Text.RegularExpressions;

    using Nancy.Hal.Configuration;

    public class Link
    {
        private static readonly Regex IsTemplatedRegex = new Regex(@"{.+}", RegexOptions.Compiled);

        public Link()
        {
        }

        public Link(string rel, string href, string title = null)
        {
            this.Rel = rel;
            this.Href = href;
            this.Title = title;
        }

        public string Rel { get; set; }

        public string Href { get; set; }

        public string Title { get; set; }

        public bool IsTemplated
        {
            get
            {
                return !string.IsNullOrEmpty(this.Href) && IsTemplatedRegex.IsMatch(this.Href);
            }
        }

        public static string SubstituteParams(string href, params object[] parameters)
        {
            var uriTemplate = new UriTemplate(href);
            foreach (var parameter in parameters)
            {
                var dynamicDictionary = parameter as DynamicDictionary;
                if (dynamicDictionary != null)
                {
                    var dictionary = dynamicDictionary;
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
    
        /// <summary>
        /// If this link is templated, you can use this method to make a non templated copy
        /// </summary>
        /// <param name="newRel">A different rel</param>
        /// <param name="parameters">The parameters, i.e 'new {id = "1"}'</param>
        /// <returns>A non templated link</returns>
        public Link CreateLink(string newRel, params object[] parameters)
        {
            return new Link(newRel, this.CreateUri(parameters).ToString());
        }

        /// <summary>
        /// If this link is templated, you can use this method to make a non templated copy
        /// </summary>
        /// <param name="parameters">The parameters, i.e 'new {id = "1"}'</param>
        /// <returns>A non templated link</returns>
        public Link CreateLink(params object[] parameters)
        {
            return this.CreateLink(this.Rel, parameters);
        }

        public Uri CreateUri(params object[] parameters)
        {
            var href = this.Href;
            href = SubstituteParams(href, parameters);

            return new Uri(href, UriKind.RelativeOrAbsolute);
        }
    }
}