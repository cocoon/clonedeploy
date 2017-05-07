﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CloneDeploy_Entities;
using CloneDeploy_Web.BasePages;
using CloneDeploy_Web.Helpers;

public partial class views_users_acl : Users
{
    private List<CheckBox> _listCheckBoxes;

    protected void buttonUpdate_OnClick(object sender, EventArgs e)
    {
        if (CloneDeployUser.UserGroupId != -1)
        {
            EndUserMessage = "Cannot Update. This User's ACL Is Controlled By A Group";
            return;
        }
        Call.CloneDeployUserApi.DeleteRights(CloneDeployUser.Id);
        var listOfRights =
            _listCheckBoxes.Where(x => x.Checked)
                .Select(box => new UserRightEntity {UserId = CloneDeployUser.Id, Right = box.ID})
                .ToList();
        EndUserMessage = Call.UserRightApi.Post(listOfRights).Success
            ? "Successfully Updated User ACLs"
            : "Could Not Update User ACLs";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        RequiresAuthorization(Authorizations.Administrator);

        _listCheckBoxes = new List<CheckBox>
        {
            ComputerCreate,
            ComputerRead,
            ComputerUpdate,
            ComputerDelete,
            ComputerSearch,
            ImageCreate,
            ImageRead,
            ImageUpdate,
            ImageDelete,
            ImageSearch,
            GroupCreate,
            GroupUpdate,
            GroupRead,
            GroupDelete,
            GroupSearch,
            ProfileCreate,
            ProfileDelete,
            ProfileRead,
            ProfileUpdate,
            ProfileSearch,
            GlobalCreate,
            GlobalDelete,
            GlobalRead,
            GlobalUpdate,
            AdminUpdate,
            ImageTaskUpload,
            ImageTaskDeploy,
            ImageTaskMulticast,
            ApproveImage,
            AllowOnd,
            AllowDebug,
            SmartCreate,
            SmartUpdate
        };
        if (!IsPostBack) PopulateForm();
    }

    protected void PopulateForm()
    {
        var listOfRights = Call.CloneDeployUserApi.GetRights(CloneDeployUser.Id);
        foreach (var right in listOfRights)
        {
            foreach (var box in _listCheckBoxes.Where(box => box.ID == right.Right))
                box.Checked = true;
        }
    }
}