using server_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Repository
{
    public class FrequentlyAskedQuestionRepository : IDisposable
    {
        private AirUDBCOE db;

        public FrequentlyAskedQuestionRepository()
        {
            db = new AirUDBCOE();
        }

        public FrequentlyAskedQuestionRepository(AirUDBCOE existingContext)
        {
            db = existingContext;
        }

        public IEnumerable<FrequentlyAskedQuestion> GetAllQuestionsAnswers()
        {
            return db.     // Stations.Find();
        }
    }
}