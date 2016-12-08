﻿using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using CloneDeploy_ApiCalls;
using CloneDeploy_Web;

namespace views.masters
{
    public partial class SiteMaster : MasterPage
    {
        public void Page_Init(object sender, EventArgs e)
        {

            if (Settings.ForceSsL == "Yes")
            {
                if (!HttpContext.Current.Request.IsSecureConnection)
                {
                    var root = Request.Url.GetLeftPart(UriPartial.Authority);
                    root = root + Page.ResolveUrl("~/");
                    root = root.Replace("http://", "https://");
                    Response.Redirect(root);
                }
            }

            if (!Request.IsAuthenticated)
                Response.Redirect("~/", true);
        }

        public void Page_Load(object sender, EventArgs e)
        {
           
        }

       
    }
}