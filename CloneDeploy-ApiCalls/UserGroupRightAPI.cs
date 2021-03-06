﻿using System.Collections.Generic;
using CloneDeploy_Entities;
using CloneDeploy_Entities.DTOs;
using RestSharp;

namespace CloneDeploy_ApiCalls
{
    public class UserGroupRightAPI : BaseAPI
    {
        private readonly ApiRequest _apiRequest;

        public UserGroupRightAPI(string resource) : base(resource)
        {
            _apiRequest = new ApiRequest();
        }

        public ActionResultDTO Post(List<UserGroupRightEntity> listOfRights)
        {
            Request.Method = Method.POST;
            Request.Resource = string.Format("api/{0}/Post/", Resource);
            Request.AddJsonBody(listOfRights);
            return _apiRequest.Execute<ActionResultDTO>(Request);
        }
    }
}