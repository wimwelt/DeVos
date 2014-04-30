using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Taxonomies.Models;
using Orchard.ContentManagement;
using Assortiment.Models;

namespace Assortiment.ViewModels
{
    
    public class taxTermViewModel
    {
        public IEnumerable<TermPart> AvailableTerms { get; set; }
        public IEnumerable<TermPart> SelectedTerms { get; set; }
        public PostedTerms PostedTerms { get; set; }
        public string TaxonomyName { get; set; }
    }
}