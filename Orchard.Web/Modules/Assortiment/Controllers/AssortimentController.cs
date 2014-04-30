using System;
using System.Collections.Generic;
using System.Web;
using Orchard.Themes;
using System.Web.Mvc;
using Orchard;
using Orchard.Mvc;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Taxonomies.Models;
using Assortiment.ViewModels;
using Assortiment.Models;
using Orchard.Taxonomies.Services;
using System.Collections;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.Records;
using System.Xml.Linq;
using System.Text;
using System.Linq;
using System.Linq.Dynamic;
using Orchard.UI.Navigation;
using Orchard.Settings;
using Orchard.DisplayManagement;
using System.Web.Routing;
using System.Web.Mvc.Html;

namespace Assortiment.Controllers
{
    [Themed]
    public class AssortimentController : Controller
    {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersions;
        private readonly List<ContentItem> gefilterd = new List<ContentItem>();
        private StringBuilder finalquery = new StringBuilder();
        private readonly ISiteService _siteService;
        private dynamic Shape { get; set; }
        public AssortimentController(
                                ITaxonomyService taxonomyService,
                                IOrchardServices orchardServices,
                                IContentManager contentManager,
                                IRepository<ContentItemVersionRecord> contentItemVersions,
                                ISiteService siteService,
                                IShapeFactory shapeFactory
                              )
        {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
            _contentItemVersions = contentItemVersions;
            _siteService = siteService;
            Shape = shapeFactory;
        }

        [Themed]
        public ActionResult Index(PagerParameters pagerParameters, List<taxTermViewModel> postedTerms, int sortBy = 1, bool isAsc = true)
        {      
            if (postedTerms == null)
            {
               List<AlleWijnenIDPrijsKleurLandViewModel> Allewijnen =  ZoekAlleWijnen();
               finalquery.AppendFormat("prijs >= {0}", 0);
               List<int> filteredIds = SorteerIds(sortBy, isAsc, Allewijnen, finalquery);
               BouwWijnShapesMetPager(filteredIds, pagerParameters);
               return View(GetTermsInitialModel());
            }
            return View(GetTermsModel(postedTerms, pagerParameters, sortBy, isAsc));
        }


        [Themed]
        public List<taxTermViewModel> GetTermsModel(List<taxTermViewModel> postedTerms, PagerParameters pagerParameters, int sortBy, bool isAsc)
        {
            List<AlleWijnenIDPrijsKleurLandViewModel> Allewijnen = ZoekAlleWijnen();
            int checkifallempty = 0;
            List<taxTermViewModel> models = new List<taxTermViewModel>();
            int[] Ids = GetTaxNamesandIds();
            int andteller = 0;
            for (var i = 0; i < postedTerms.Count; i++)
            {
                var selectedTerms = new List<TermPart>();
                var postedTermIds = new string[0];
                var AllTerms = new List<TermPart>();
                if (postedTerms[i].PostedTerms == null) postedTerms[i].PostedTerms = new PostedTerms();

                if (postedTerms[i].PostedTerms.TermIds != null && postedTerms[i].PostedTerms.TermIds.Any())
                {
                    if (postedTerms[i].PostedTerms.TermIds[0].Contains(","))
                    {
                        string texttosplit = postedTerms[i].PostedTerms.TermIds[0].ToString();

                        postedTermIds = texttosplit.Split(',');
                    }
                    else
                        postedTermIds = postedTerms[i].PostedTerms.TermIds;
                }

                if (postedTermIds != null && postedTermIds.Any())
                {
                    if (andteller > 0) finalquery.Append(" AND ");
                    finalquery.Append("(");
                    checkifallempty = 1;
                    int kleur = 0;
                    int land = 0;
                    int prijs = 0;

                    for (var j = 0; j < postedTermIds.Count(); j++)
                    {
                        if (postedTerms[i].TaxonomyName == "Dranken")
                        {
                            if (kleur > 0) finalquery.Append(" OR ");
                            finalquery.AppendFormat("KleurID == {0}", postedTermIds[j]);
                            kleur++;
                        }

                        if (postedTerms[i].TaxonomyName == "Land")
                        {
                            if (land > 0) finalquery.Append(" OR ");
                            finalquery.AppendFormat("LandID == {0}", postedTermIds[j]);
                            land++;
                        }

                        if (postedTerms[i].TaxonomyName == "Prijs")
                        {
                            if (prijs > 0) finalquery.Append(" OR ");
                            if (postedTermIds[j] == "24")
                            {
                                finalquery.AppendFormat("prijs < {0}", 10);
                            }

                            if (postedTermIds[j] == "25")
                            {
                                finalquery.Append("(");
                                finalquery.AppendFormat("prijs >= {0}", 10);
                                finalquery.Append(" AND ");
                                finalquery.AppendFormat("prijs < {0}", 15);
                                finalquery.Append(")");
                            }

                            if (postedTermIds[j] == "26")
                            {
                                finalquery.AppendFormat("prijs > {0}", 15);
                            }

                            prijs++;
                        }
                    }

                    finalquery.Append(") ");
                    andteller++;
                }

                if (postedTermIds.Any())
                {
                    selectedTerms = _taxonomyService.GetTerms(Ids[i])
                     .Where(x => postedTermIds.Any(s => x.Id.ToString().Equals(s)))
                     .ToList();
                }

                models.Add(new taxTermViewModel() { AvailableTerms = _taxonomyService.GetTerms(Ids[i]), SelectedTerms = selectedTerms, PostedTerms = postedTerms[i].PostedTerms, TaxonomyName = postedTerms[i].TaxonomyName });
            }


            if (checkifallempty == 1)
            {

                List<int> filteredIds = SorteerIds(sortBy, isAsc, Allewijnen, finalquery);
                BouwWijnShapesMetPager(filteredIds, pagerParameters);

            }
            else
            {
                finalquery.AppendFormat("prijs >= {0}", 0);
                List<int> filteredIds = SorteerIds(sortBy, isAsc, Allewijnen, finalquery);
                BouwWijnShapesMetPager(filteredIds, pagerParameters);
            }

            return models;
        }


        private List<AlleWijnenIDPrijsKleurLandViewModel> ZoekAlleWijnen()
        {
            int[] allewijnenIds = _contentManager.Query().ForVersion(VersionOptions.Published).List().Where(r => r.ContentType == "Wijn").Select(x => x.Id).ToArray();
            List<AlleWijnenIDPrijsKleurLandViewModel> allewijnenmetprijslandenkleurlijst = new List<AlleWijnenIDPrijsKleurLandViewModel>();
            for (int tel = 0; tel < allewijnenIds.Count(); tel++)
            {

                dynamic contentItem = _contentManager.Get(allewijnenIds[tel]);
                int prijs = Convert.ToInt32(contentItem.Wijn.Prijs.Value);
                int jaar = Convert.ToInt32(contentItem.Wijn.Jaar.Value);
                List<TermPart> termsvoorcontentitem = _taxonomyService.GetTermsForContentItem(allewijnenIds[tel]).ToList();

                allewijnenmetprijslandenkleurlijst.Add(new AlleWijnenIDPrijsKleurLandViewModel
                {
                    WijnID = allewijnenIds[tel],
                    prijs = prijs,
                    Jaar = jaar,
                    KleurID = termsvoorcontentitem[0].Id,
                    LandID = termsvoorcontentitem[1].Id
                });
            }

            return allewijnenmetprijslandenkleurlijst;
        }

        private List<taxTermViewModel> GetTermsInitialModel()
        {
            var selectedTerms = new List<TermPart>();
            int[] Ids = GetTaxNamesandIds();
            string[] taxonomyNames = new string[] { "Dranken", "Land", "Prijs" };
            List<taxTermViewModel> models = new List<taxTermViewModel>();
            
            for (int i = 0; i < Ids.Count(); i++)
            {
                
                models.Add(new taxTermViewModel() { AvailableTerms = _taxonomyService.GetTerms(Ids[i]), SelectedTerms = selectedTerms, TaxonomyName = taxonomyNames[i] });
            }

            return models;

        }


        private void BouwWijnShapesMetPager(List<int> filteredIDs, PagerParameters pagerParameters)
        {
            for (int teller = 0; teller < filteredIDs.Count(); teller++)
            {
                gefilterd.AddRange(_contentManager.Query().List().Where(r => r.Id == filteredIDs[teller]).ToList());
            }

            var displayType = "Summary";
            var contentItemShapes = gefilterd.Select(x => _contentManager.BuildDisplay(x, displayType)).Distinct();
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters.Page, pagerParameters.PageSize);
            var wijnenmetPager = contentItemShapes.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            var pagerShape = Shape.Pager(pager).TotalItemCount(gefilterd.Count());
            ViewBag.Pager = pagerShape;
            ViewBag.Allewijnen = wijnenmetPager;
        }

        private List<int> SorteerIds(int sortBy, bool isAsc, List<AlleWijnenIDPrijsKleurLandViewModel> Allewijnen, StringBuilder finalquery)
        {
            List<int> filteredIDs = new List<int>();
            ViewBag.SortBy = sortBy;
            ViewBag.IsAsc = isAsc;

            switch (sortBy)
            {

                case 1:

                    if (isAsc == true)
                    {
                        return filteredIDs = Allewijnen.AsQueryable().Where(finalquery.ToString()).OrderBy(s => s.prijs).Select(c => c.WijnID).ToList();
                    }
                    else

                        return filteredIDs = Allewijnen.AsQueryable().Where(finalquery.ToString()).OrderByDescending(s => s.prijs).Select(c => c.WijnID).ToList();
                case 2:
                    if (isAsc == true)
                    {
                        return filteredIDs = Allewijnen.AsQueryable().Where(finalquery.ToString()).OrderBy(s => s.Jaar).Select(c => c.WijnID).ToList();
                    }
                    else
                        return filteredIDs = Allewijnen.AsQueryable().Where(finalquery.ToString()).OrderByDescending(s => s.Jaar).Select(c => c.WijnID).ToList();

                default:
                    return filteredIDs = Allewijnen.AsQueryable().Where(finalquery.ToString()).OrderBy(s => s.prijs).Select(c => c.WijnID).ToList();
            }
        }

        private int[] GetTaxNamesandIds()
        {
            string[] taxonomyNames = new string[] { "Dranken", "Land", "Prijs" };
            int[] taxonomyIds = new int[taxonomyNames.Count()];
            for (int i = 0; i < taxonomyNames.Count(); i++)
            {
                taxonomyIds[i] = _taxonomyService.GetTaxonomyByName(taxonomyNames[i]).Id;
            }
            return taxonomyIds;
        }


    }
}