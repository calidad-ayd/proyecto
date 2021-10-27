using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ComunidadPractica
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Comunidad", action = "Portada", id = UrlParameter.Optional }
			);

		}

		/*
		 *  routes.MapRoute(
				"ThankYou",
				"{controller}/{action}/{email}/{id}"
				);
		 * */
	}
}
