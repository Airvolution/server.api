﻿using server_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace server_api.Repository
{
    public class GroupRepository : IDisposable
    {
        private ApplicationContext _ctx;
        public GroupRepository()
        {
            _ctx = new ApplicationContext();
        }

        public GroupRepository(ApplicationContext ctx)
        {
            _ctx = ctx;
        }

        public Group GetGroup(int id)
        {
            Group group = _ctx.Groups.Find(id);
            _ctx.Entry(group).Collection(g => g.Stations).Load();
            return group;
        }

        public IEnumerable<Group> GetAllGroups(string User_Id)
        {
            var result = from stationgroup in _ctx.Groups.Include("Stations") 
                         where stationgroup.Owner_Id.Equals(User_Id) 
                         select stationgroup;

            return result.ToList();
        }

        public Group CreateGroup(Group group){
            _ctx.Groups.Add(group);
            _ctx.SaveChanges();
            return group;
        }

        public Group AddStationToGroup(Group group, string stationId)
        {
            Station station = _ctx.Stations.Find(stationId);
            if (station != null)
            {
                group.Stations.Add(station);
                _ctx.SaveChanges();
                return group;
            }
            else
            {
                return null;
            }
            
        }

        public Group RemoveStationFromGroup(Group group, string stationId)
        {
            Station station =  _ctx.Stations.Find(stationId);
            if (station != null)
            {
                group.Stations.Remove(station);
                _ctx.SaveChanges();
                return group;
            }
            else
            {
                return null;
            }
        }


        public bool DeleteGroup(Group group)
        {
            _ctx.Groups.Remove(group);
            int modified = _ctx.SaveChanges();
            if (modified > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Dispose()
        {
            _ctx.Dispose();
        }
    }
}