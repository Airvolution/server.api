using server_api.Models;
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
        [Route("{groupId}/stations/{stationId}")]
        [HttpPut]
        public IHttpActionResult AddStationToGroup(int groupId, string stationId)
        {
            Group group = _repo.GetGroup(groupId);
            if(group == null){
                return NotFound();
            }

            if (group.Owner_Id == RequestContext.Principal.Identity.GetUserId()){
            
                var result = _repo.AddStationToGroup(group, stationId);
                if(result == null){
                    return NotFound();
                }
                return Ok(result);
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
        [Route("{groupId}/stations/{stationId}")]
        [HttpDelete]
        public IHttpActionResult RemoveStationFromGroup(int groupId, string stationId)
        {
            Group group = _repo.GetGroup(groupId);
            if(group == null){
                return NotFound();
            }

            if (group.Owner_Id == RequestContext.Principal.Identity.GetUserId()){
            
                var result = _repo.RemoveStationFromGroup(group, stationId);
                if(result == null){
                    return NotFound();
                }
                return Ok(result);
            }
            else
            {
                return Unauthorized();
            }
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
