using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    internal static class ModificationDependencyRelator
    {
        internal static bool Invalidates(Modification modification, Dependency dependency)
        {
            if (modification == null)
            {
                throw new ArgumentNullException("Modification may not be null."); 
            }

            if (dependency == null)
            {
                throw new ArgumentNullException("Dependency may not be null."); 
            }

            // Dependencies: 
            // NamespaceDependency: 
            //   NamespaceExistenceDependency
            //   NamespacePermissionsDependency
            //   NamespacePropertiesDependency
            //   TopicListDependency
            // TopicDependency: 
            //   TopicContentsDependency
            //   TopicExistenceDependency
            //   TopicPermissionsDependency

            // Modifications: 
            // NamespaceModification: 
            //   NamespaceContentsDeletedModification
            //   NamespacePermissionsModification
            //   NamespacePropertiesModification
            //   TopicListModification
            // TopicModification: 
            //   TopicContentsModification
            //   TopicDeletedModification
            //   TopicPermissionsModification

            if (!AreSameNamespace(modification, dependency))
            {
                return false; 
            }

            if (modification is NamespaceContentsDeletedModification)
            {
                return true;
            }
            else if (modification is NamespacePermissionsModification)
            {
                // If the namespace permissions have changed, then it's 
                // possible that the user no longer has any access to the
                // contents of this namespace. So we flush everything. 
                return true;
            }
            else if (modification is NamespacePropertiesModification)
            {
                return dependency is NamespacePropertiesDependency; 
            }
            else if (modification is TopicListModification)
            {
                return true; 
            }
            else if (modification is TopicContentsModification)
            {
                // Might be a modification, but then again it could be a write of a new topic
                return AreSameTopic(modification, dependency) || dependency is TopicListDependency; 
            }
            else if (modification is TopicDeletedModification)
            {
                return AreSameTopic(modification, dependency) || dependency is TopicListDependency; 
            }
            else if (modification is TopicPermissionsModification)
            {
                return AreSameTopic(modification, dependency);
            }
            else
            {
                throw new ArgumentException("Unrecognized modification type: " + modification.GetType().ToString()); 
            }
        }

        private static bool AreSameNamespace(Modification modification, Dependency dependency)
        {
            string modificationNamespace;
            if (modification is NamespaceModification)
            {
                modificationNamespace = ((NamespaceModification)modification).Namespace; 
            }
            else if (modification is TopicModification)
            {
                modificationNamespace = ((TopicModification)modification).Topic.Namespace;
            }
            else
            {
                throw new ArgumentException("Unrecognized modification type: " + modification.GetType().ToString()); 
            }

            string dependencyNamespace;
            if (dependency is NamespaceDependency)
            {
                dependencyNamespace = ((NamespaceDependency)dependency).Namespace; 
            }
            else if (dependency is TopicDependency)
            {
                dependencyNamespace = ((TopicDependency)dependency).TopicName.Namespace;
            }
            else
            {
                throw new ArgumentException("Unrecognized dependency type: " + dependency.GetType().ToString()); 
            }

            return modificationNamespace.Equals(dependencyNamespace, StringComparison.InvariantCultureIgnoreCase); 
        }
        private static bool AreSameTopic(Modification modification, Dependency dependency)
        {
            if (!(modification is TopicModification))
            {
                return false; 
            }

            if (!(dependency is TopicDependency))
            {
                return false; 
            }

            return ((TopicModification)modification).Topic.Equals(((TopicDependency)dependency).TopicName); 
        }

    }
}
