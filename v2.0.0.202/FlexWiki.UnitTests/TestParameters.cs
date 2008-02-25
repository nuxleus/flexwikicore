using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.UnitTests.Caching;

namespace FlexWiki.UnitTests
{
    internal class TestParameters<TProvider>
    {
        private MockCache _cache;
        private Federation _federation;
        private NamespaceManager _manager;
        private TProvider _provider;
        private MockContentStore _store;

        public MockWikiApplication Application
        {
            get { return _federation.Application as MockWikiApplication; }
        }

        public MockCache Cache
        {
            get { return _cache; }
            set { _cache = value; }
        }

        public Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }

        public NamespaceManager Manager
        {
            get { return _manager; }
            set { _manager = value; }
        }

        public TProvider Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }

        public MockContentStore Store
        {
            get { return _store; }
            set { _store = value; }
        }
    }

}
