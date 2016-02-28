using server_api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

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
            return db.FrequentlyAskedQuestions;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}