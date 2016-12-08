﻿using System;
using BasePages;
using CloneDeploy_Web;

public partial class views_images_profiles_Default : Images
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            txtSender.Text = ImageProfile.SenderArguments;
            txtReceiver.Text = ImageProfile.ReceiverArguments;
        }
    }

    protected void buttonUpdateMulticast_OnClick(object sender, EventArgs e)
    {
        RequiresAuthorizationOrManagedImage(Authorizations.UpdateProfile, Image.Id);
        ImageProfile.SenderArguments = txtSender.Text;
        ImageProfile.ReceiverArguments = txtReceiver.Text;
        var result = Call.ImageProfileApi.Put(ImageProfile.Id,ImageProfile);
        EndUserMessage = result.Success ? "Successfully Updated Image Profile" : result.ErrorMessage;
    }
}