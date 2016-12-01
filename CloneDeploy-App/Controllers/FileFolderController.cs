﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CloneDeploy_App.BLL;
using CloneDeploy_App.Controllers.Authorization;
using CloneDeploy_Entities;
using CloneDeploy_Entities.DTOs;
using CloneDeploy_Services;


namespace CloneDeploy_App.Controllers
{
    public class FileFolderController: ApiController
    {
        private readonly FileFolderServices _fileFolderServices;

        public FileFolderController()
        {
            _fileFolderServices = new FileFolderServices();
        }

        [GlobalAuth(Permission = "GlobalRead")]
        public IEnumerable<FileFolderEntity> Get(string searchstring = "")
        {
            return string.IsNullOrEmpty(searchstring)
                ? _fileFolderServices.SearchFileFolders()
                : _fileFolderServices.SearchFileFolders(searchstring);

        }

        [GlobalAuth(Permission = "GlobalRead")]
        public ApiStringResponseDTO GetCount()
        {

            return new ApiStringResponseDTO() {Value = _fileFolderServices.TotalCount()};

        }

        [GlobalAuth(Permission = "GlobalRead")]
        public FileFolderEntity Get(int id)
        {
            var result = _fileFolderServices.GetFileFolder(id);
            if (result == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            return result;
        }

        [GlobalAuth(Permission = "GlobalCreate")]
        public ActionResultDTO Post(FileFolderEntity fileFolder)
        {
            var result = _fileFolderServices.AddFileFolder(fileFolder);
            if (result == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            return result;
        }

        [GlobalAuth(Permission = "GlobalUpdate")]
        public ActionResultDTO Put(int id, FileFolderEntity fileFolder)
        {
            fileFolder.Id = id;
            var result = _fileFolderServices.UpdateFileFolder(fileFolder);
            if (result == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            return result;
        }

        [GlobalAuth(Permission = "GlobalDelete")]
        public ActionResultDTO Delete(int id)
        {
            var result = _fileFolderServices.DeleteFileFolder(id);
            if (result == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            return result;
        }
    }
}