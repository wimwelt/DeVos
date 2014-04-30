using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Assortiment
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Priority = 15,
                    Route = new Route(
                        "Assortiment/{postedTerms}",
                        new RouteValueDictionary {
                            {"area", "Assortiment"},
                            {"controller", "Assortiment"},
                            {"action", "Index"},
                            {"postedTerms", UrlParameter.Optional}
                            
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Assortiment"}
                        },
                        new MvcRouteHandler())
                },
                                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "Gefilterd",
                        new RouteValueDictionary {
                            {"area", "Assortiment"},
                            {"controller", "Assortiment"},
                            {"action", "IndexBack"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Assortiment"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}