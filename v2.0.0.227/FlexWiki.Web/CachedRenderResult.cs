using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Collections;

namespace FlexWiki.Web
{
    public class CachedRenderResult
    {
        private string _contents;
        private readonly DependencyCollection _dependencies = new DependencyCollection();
        private DateTime _whenRendered; 

        public CachedRenderResult(string contents, DependencyCollection dependencies, DateTime whenRendered)
        {
            _contents = contents;
            _dependencies.AddRange(dependencies);
            _whenRendered = whenRendered; 
        }

        public string Contents
        {
            get { return _contents; }
        }

        public DependencyCollection Dependencies
        {
            get { return _dependencies; }
        }

        public DateTime WhenRendered
        {
            get { return _whenRendered; }
        }
    }
}
