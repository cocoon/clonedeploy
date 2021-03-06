﻿using System;
using System.Linq;
using CloneDeploy_Web.BasePages;

namespace CloneDeploy_Web.views.computers
{
    public partial class views_computers_groups : Computers
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateGrid();
            }
        }

        protected void PopulateGrid()
        {
            var memberships = Call.ComputerApi.GetGroupMemberships(Computer.Id);
            var computerGroups =
                memberships.Select(membership => Call.GroupApi.Get(membership.GroupId)).Where(x => x != null).ToList();
            gvGroups.DataSource = computerGroups;
            gvGroups.DataBind();
        }
    }
}