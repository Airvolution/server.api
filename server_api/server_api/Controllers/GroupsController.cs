﻿using server_api.Models;
using server_api.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;
using System.Collections;
using System.Collections.ObjectModel;

namespace server_api.Controllers
{
    [RoutePrefix("groups")]
    public class GroupsController : ApiController
    {
        private GroupRepository _repo;
        private StationsRepository _stationsRepo;

        public GroupsController()
        {
            ApplicationContext ctx = new ApplicationContext();
            _repo = new GroupRepository(ctx);
            _stationsRepo = new StationsRepository(ctx);
        }

        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Group))]
        [Route("")]
        [HttpPost]
        public IHttpActionResult CreateGroup([FromBody] RegisterGroup newGroup)
        {
            Group group = new Group();
            group.Owner_Id = RequestContext.Principal.Identity.GetUserId();
            group.Name = newGroup.Name;
            group.Description = newGroup.Description;
            IEnumerable<Station> stations = _stationsRepo.GetMultipleStations(newGroup.StationIds);
            group.Stations = new Collection<Station>(stations.ToList());
            group = _repo.CreateGroup(group);
            return Ok(group);
        }

        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Group>))]
        [Route("")]
        [HttpGet]
        public IHttpActionResult GetAllGroups(){
            return Ok(_repo.GetAllGroups(RequestContext.Principal.Identity.GetUserId()));
        }

        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Group))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [Route("{id}")]
        [HttpGet]
        public IHttpActionResult GetGroup(int id)
        {
            Group group = _repo.GetGroup(id);
            if (group.Owner_Id == RequestContext.Principal.Identity.GetUserId())
            {
                return Ok(group);
            }
            return Unauthorized();
        }

        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Group))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [Route("{groupId}/stations/")]
        [HttpPut]
        public IHttpActionResult AddStationToGroup(int groupId, [FromUri] string[] ids)
        {
            Group group = _repo.GetGroup(groupId);
            if(group == null){
                return NotFound();
            }

            if (group.Owner_Id == RequestContext.Principal.Identity.GetUserId()){

                bool result = _repo.AddStationToGroup(group, ids);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(_repo.GetGroup(group.Id));
            }
            else
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Group))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [Route("{groupId}/stations/")]
        [HttpDelete]
        public IHttpActionResult RemoveStationFromGroup(int groupId, [FromUri] string[] ids)
        {
            Group group = _repo.GetGroup(groupId);
            if(group == null){
                return NotFound();
            }

            if (group.Owner_Id == RequestContext.Principal.Identity.GetUserId()){

                bool result = _repo.RemoveStationFromGroup(group, ids);
                if (!result)
                {
                    return NotFound();
                }
                return Ok(_repo.GetGroup(group.Id));
            }
            else
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Group))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [Route("{groupId}/stations/")]
        [HttpPost]
        public IHttpActionResult UpdateGroup([FromUri] int groupId, [FromBody] UpdateGroup data)
        {
            Group group = _repo.GetGroup(groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (group.Owner_Id == RequestContext.Principal.Identity.GetUserId())
            {
                bool result = _repo.AddStationToGroup(group, data.stationsToAdd);
                if (!result)
                {
                    return NotFound();
                }
                result = _repo.RemoveStationFromGroup(group, data.stationsToRemove);
                if (!result)
                {
                    return NotFound();
                }
            }
            else
            {
                return Unauthorized();
            }

            return Ok(_repo.GetGroup(group.Id)); ;
        }

        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [Route("{id}")]
        [HttpDelete]
        public IHttpActionResult RemoveGroup(int id){
            Group group = _repo.GetGroup(id);
            if(group == null){
                return NotFound();
            }
            if(group.Owner_Id == RequestContext.Principal.Identity.GetUserId()){
                if(_repo.DeleteGroup(group)){
                    return Ok();
                }else{
                    return InternalServerError();
                }
            }else{
                return Unauthorized();
            }
        }
    }
}
