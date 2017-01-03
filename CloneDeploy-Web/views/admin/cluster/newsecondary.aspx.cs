﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CloneDeploy_Entities;
using CloneDeploy_Web.Helpers;

namespace CloneDeploy_Web.views.admin.cluster
{
    public partial class newsecondary : BasePages.Admin
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnUpdateSettings_OnClick(object sender, EventArgs e)
        {
            RequiresAuthorization(Authorizations.UpdateAdmin);
            var secondaryServer = new SecondaryServerEntity()
            {
                Name = txtServerId.Text,
                ApiURL = txtApi.Text,
                ServiceAccountName = txtAccountName.Text,
                ServiceAccountPassword = txtAccountPassword.Text
            };

            var result = Call.SecondaryServerApi.Post(secondaryServer);
            if (result.Success)
            {
                EndUserMessage = "Successfully Created SecondaryServer";
                Response.Redirect("~/views/admin/clust/editsecondary.aspx?cat=sub1&ssid=" + result.Id);
            }
            else
            {
                EndUserMessage = result.ErrorMessage;
            }
        }
    }
}