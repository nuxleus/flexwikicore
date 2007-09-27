#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion
using System;
using System.Collections;

using FlexWiki;
using FlexWiki.Collections;

namespace FlexWiki.SqlProvider
{
    /// <summary>
    /// Summary description for SqlNamespaceProvider.
    /// </summary>
    public class SqlNamespaceProvider : INamespaceProvider
    {
        private string _connectionString = "Data Source=[Sql Server Machine Name];Initial Catalog=FlexwikiSqlStore;Integrated Security=true;";
        private string _contact = string.Empty;
        private string _namespace = string.Empty;
        private string _namespaceDescription = string.Empty;
        private ArrayList _parameterDescriptors;
        private readonly NamespaceProviderParameterCollection _parameters = new NamespaceProviderParameterCollection();
        private string _title = string.Empty;

        public SqlNamespaceProvider()
        {
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }
        public string Contact
        {
            get
            {
                return _contact;
            }
            set
            {
                _contact = value;
            }
        }
        public string Description
        {
            get
            {
                return "Single-namespace, content stored in Sql Database.";
            }
        }
        public string Namespace
        {
            get
            {
                return _namespace;
            }
            set
            {
                _namespace = value;
            }
        }
        public string NamespaceDescription
        {
            get
            {
                return _namespaceDescription;
            }
            set
            {
                _namespaceDescription = value;
            }
        }
        public string OwnerMailingAddress
        {
            get
            {
                return Contact;
            }
        }
        public IList ParameterDescriptors
        {
            get
            {
                if (_parameterDescriptors != null)
                    return ArrayList.ReadOnly(_parameterDescriptors);
                _parameterDescriptors = new ArrayList();
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.Namespace, "Namespace", "Name for the namespace", null, true));
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.ConnectionString, "ConnectionString", "ConnectionString to the database. Format: Data Source=[Sql Server Machine Name];Initial Catalog=FlexwikiSqlStore;Integrated Security=true;", null, true));
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.Contact, "Contact", "eMail address for contact for this namespace", null, false));
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.Title, "Title", "A short title for this namespace", null, false));
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.Description, "Description", "A description for the namespace (use Wiki formatting)", null, false));
                return ArrayList.ReadOnly(_parameterDescriptors);
            }
        }
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        /// <summary>
        /// Indicates if a namespace parameter can be edited.
        /// </summary>
        /// <param name="param">Parameter name.</param>
        /// <returns>Boolean value indicates if the parameter can be edited.</returns>
        public bool CanParameterBeEdited(string param)
        {
            // Except for Namespace name all values can be edited.
            return param != ConfigurationParameterNames.Namespace;
        }
        /// <summary>
        /// Called when a provider is first created.  Must register all associated namespaces with the federation.
        /// Can also be used to create initial content in the namespaces.
        /// </summary>
        /// <param name="aFed"></param>
        public IList CreateNamespaces(Federation aFed)
        {
            ArrayList answer = new ArrayList();

            if (SqlNamespaceProvider.Exists(Namespace, ConnectionString))
            {
                throw new Exception("Namespace with the specified name already exists.");
            }

            this.CreateNamespace(Namespace, ConnectionString);

            SqlStore store = new SqlStore();
            NamespaceManager manager = aFed.RegisterNamespace(store, Namespace, _parameters);
            manager.WriteTopic(manager.DefinitionTopicName.LocalName, "");
            manager.SetTopicPropertyValue(manager.DefinitionTopicName.LocalName, "Contact", Contact, false, "FlexWiki");
            manager.SetTopicPropertyValue(manager.DefinitionTopicName.LocalName, "Title", Title, false, "FlexWiki");
            string defaultImportedNamespaces = aFed.Configuration.DefaultImportedNamespaces;
            if (defaultImportedNamespaces != null)
            {
                manager.SetTopicPropertyValue(manager.DefinitionTopicName.LocalName, "Import", 
                    defaultImportedNamespaces, false, "FlexWiki");
            }

            // whoever is last should write a new version
            manager.SetTopicPropertyValue(manager.DefinitionTopicName.LocalName, "Description", 
                NamespaceDescription, true, "FlexWiki");
            answer.Add(Namespace);
            return ArrayList.ReadOnly(answer);

        }
        /// <summary>
        /// Check if the Namespace already exists in the database.
        /// </summary>
        /// <param name="ns">Namespace to check for existence.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Boolean vaue indicating if the namespace exists or not.</returns>
        public static bool Exists(string ns, string connectionString)
        {
            // Check if the namespace already exists in the database
            IDatabase database = new SqlDatabase();
            database.ConnectionString = connectionString;
            SqlHelper helper = new SqlHelper(database); 
            return helper.NamespaceExists(ns);
        }
        public string GetParameter(string param)
        {
            if (param == ConfigurationParameterNames.Namespace)
                return Namespace;
            else if (param == ConfigurationParameterNames.ConnectionString)
                return ConnectionString;
            else if (param == ConfigurationParameterNames.Contact)
                return Contact;
            else if (param == ConfigurationParameterNames.Title)
                return Title;
            else if (param == ConfigurationParameterNames.Description)
                return NamespaceDescription;
            else
                throw new Exception("Unknown parameter: " + param);
        }
        /// <summary>
        /// Called when a provider should instantiate and register all of its namespaces in a federation.  
        /// Note that when a provider is first created (e.g., via the admin UI) that this function is not 
        /// called.
        /// </summary>
        /// <param name="aFed"></param>
        public void LoadNamespaces(Federation aFed)
        {
            SqlStore store = new SqlStore();
            aFed.RegisterNamespace(store, Namespace, _parameters);
        }
        /// <summary>
        /// Method responsible to write the namespace config information to the  
        /// NamespaceMapFile.
        /// </summary>
        /// <param name="def"></param>
        public void SavePersistentParametersToDefinition(NamespaceProviderDefinition def)
        {
            def.SetParameter(ConfigurationParameterNames.Namespace, Namespace);
            def.SetParameter(ConfigurationParameterNames.ConnectionString, ConnectionString);
        }
        public void SetParameter(string name, string value)
        {
            if (name == ConfigurationParameterNames.Namespace)
            {
                Namespace = value;
            }
            else if (name == ConfigurationParameterNames.ConnectionString)
            {
                ConnectionString = value;
            }
            else if (name == ConfigurationParameterNames.Contact)
            {
                Contact = value;
            }
            else if (name == ConfigurationParameterNames.Description)
            {
                NamespaceDescription = value;
            }
            else if (name == ConfigurationParameterNames.Title)
            {
                Title = value;
            }

            _parameters.AddOrReplace(new NamespaceProviderParameter(name, value)); 
            
        }
        /// <summary>
        /// Called when a provider definition is changed.  Should make sure the right changes happen in the federation
        /// to reflect the updated provider definition (e.g., removing/adding/updating namespace registrations).
        /// </summary>
        /// <param name="aFed"></param>
        public void UpdateNamespaces(Federation aFed)
        {
            // just kill the old one and reload a new one
            aFed.UnregisterNamespace(aFed.NamespaceManagerForNamespace(Namespace));
            LoadNamespaces(aFed);
        }
        public IList ValidateAggregate(Federation aFed, bool isCreate)
        {
            return null;
        }
        /// <summary>
        /// Validate the parameters for creating the Namespace.
        /// </summary>
        /// <param name="param">Parameters name</param>
        /// <param name="val">Parameter value to validate.</param>
        /// <returns>returns null to indicate success otherwise returns 
        /// the error message to be displayed.</returns>
        public string ValidateParameter(Federation aFed, string param, string val, bool isCreate)
        {
            if (param == ConfigurationParameterNames.Namespace)
            {
                // Would need to be consistent with the namespace 
                // names for the FileSystemNameSpaceProvider
                if (val == "" || val == null)
                    return "Namespace can not be blank";
                if (isCreate && aFed.NamespaceManagerForNamespace(val) != null)
                    return "Namespace already exists";
            }
            else if (param == ConfigurationParameterNames.ConnectionString)
            {
                if (val == "")
                    return "ConnectionString can not be null or blank";
            }
            return null;
        }

        /// <summary>
        /// Create the specified namespace.
        /// </summary>
        /// <param name="ns">Namespace to check for existence.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        private void CreateNamespace(string ns, string connectionString)
        {
            // Create a new namespace.
            IDatabase database = new SqlDatabase();
            database.ConnectionString = connectionString; 
            new SqlHelper(database).CreateNamespace(ns);
        }
    }
}
