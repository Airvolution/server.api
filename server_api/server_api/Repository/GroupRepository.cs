using server_api.Models;
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

        public bool AddStationToGroup(Group group, string[] id)
        {
            List<Station> stations = new List<Station>();

            foreach(var stationId in stationIds)
            {
                Station station = _ctx.Stations.Find(id);
                if (station != null)
                {
                    stations.Add(station);
                }
                else
                {
                    return false;
                }
            }

            foreach (var station in stations)
            {
                group.Stations.Add(station);
            }

            _ctx.SaveChanges();

            return true;
        }

        public bool RemoveStationFromGroup(Group group, string[] id)
        {
            List<Station> stations = new List<Station>();

            foreach (var stationId in id)
            {
                Station station = _ctx.Stations.Find(stationId);
                if (station != null)
                {
                    stations.Add(station);
                }
                else
                {
                    return false;
                }
            }

            foreach(var station in stations)
            {
                group.Stations.Remove(station);
            }
            
            _ctx.SaveChanges();

            return true;
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