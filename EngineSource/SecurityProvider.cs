using System;
using System.Security.Principal;
using System.Threading; 

namespace FlexWiki
{
    public class SecurityProvider : ContentProviderBase
    {
        public SecurityProvider(ContentProviderBase next)
            : base(next)
        {
        }

        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            if (permission != TopicPermission.Edit && permission != TopicPermission.Read)
            {
                throw new ArgumentException("Unrecognized topic permission " + permission.ToString()); 
            }

            // Do not allow the operation if the rest of the chain denies it.
            if (!Next.HasPermission(topic, permission))
            {
                return false;
            }

            ParsedTopic parsedTopic = Next.GetParsedTopic(new UnqualifiedTopicRevision(topic));


            bool explicitlyAllowedEdit = IsPrincipalListedUnderProperty(parsedTopic, Thread.CurrentPrincipal, "AllowEdit");
            bool explicityDeniedEdit = IsPrincipalListedUnderProperty(parsedTopic, Thread.CurrentPrincipal, "DenyEdit");
            bool explicitlyAllowedRead = IsPrincipalListedUnderProperty(parsedTopic, Thread.CurrentPrincipal, "AllowRead");
            bool explicitlyDeniedRead = IsPrincipalListedUnderProperty(parsedTopic, Thread.CurrentPrincipal, "DenyRead");

            if (permission == TopicPermission.Edit)
            {
                return explicitlyAllowedEdit && (!explicitlyDeniedRead || !explicityDeniedEdit); 
            }
            else if (permission == TopicPermission.Read)
            {
                return (explicitlyAllowedRead || explicitlyAllowedEdit) && !explicitlyDeniedRead; 
            }

            return false; 
        }


        private bool CaseInsenstiveEquivalent(string s1, string s2)
        {
            return string.Compare(s1, s2, true) == 0;
        }

        private bool IsPrincipalListedUnderProperty(ParsedTopic parsedTopic, IPrincipal principal, string propertyName)
        {
            TopicProperty property = null;
            if (parsedTopic.Properties.Contains(propertyName))
            {
                property = parsedTopic.Properties[propertyName];
            }

            if (property == null)
            {
                return false; 
            }

            foreach (string value in property.AsList())
            {
                if (value.ToLower().StartsWith("user:"))
                {
                    if (CaseInsenstiveEquivalent(value, "user:" + principal.Identity.Name))
                    {
                        return true; 
                    }
                }
                else if (value.ToLower().StartsWith("role:"))
                {
                    if (principal.IsInRole(value.Substring("role:".Length)))
                    {
                        return true; 
                    }
                }
            }

            return false; 

        }

    }
}
