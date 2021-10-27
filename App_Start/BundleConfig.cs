using System.Web;
using System.Web.Optimization;

namespace ComunidadPractica
{
	public class BundleConfig
	{
		// For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			// Styles


			bundles.Add(new StyleBundle("~/Content/css/bundles").Include(
										"~/Content/css/site.css"));

			bundles.Add(new StyleBundle("~/Content/fonts/bundles").Include(
										"~/Content/fonts/FontAwesomeMin.css"));

			bundles.Add(new StyleBundle("~/Content/css/bootstrap-4.5.2/bundles").Include(
										"~/Content/css/bootstrap-4.5.2/bootstrap.css"));

			bundles.Add(new StyleBundle("~/Content/css/select2/bundles").Include(
										"~/Content/css/select2/select2.min.css"));

			// Scripts

			bundles.Add(new ScriptBundle("~/Scripts/bootstrap-4.5.2/bundles").Include(
										 "~/Scripts/bootstrap-4.5.2/bootstrap.bundle.js"));

			bundles.Add(new ScriptBundle("~/Scripts/select2/bundles").Include(
										 "~/Scripts/select2/select2.min.js"));

			bundles.Add(new ScriptBundle("~/Scripts/clickeable/bundles").Include(
										 "~/Scripts/clickeable/clickeable.js"));

			bundles.Add(new ScriptBundle("~/Scripts/jquery-3.4.1/bundles").Include(
										 "~/Scripts/jquery-3.4.1/jquery-3.4.1.js"));

			bundles.Add(new ScriptBundle("~/Scripts/jquery-validate/bundles").Include(
										 "~/Scripts/jquery-validate/jquery.validate.js",
										 "~/Scripts/jquery-validate/jquery.validate.unobtrusive.js"
										 ));



		}
	}
}
