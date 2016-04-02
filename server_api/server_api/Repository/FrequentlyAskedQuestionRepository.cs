using server_api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace server_api.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class FrequentlyAskedQuestionRepository : IDisposable
    {
        private ApplicationContext db;

        /// <summary>
        /// 
        /// </summary>
        public FrequentlyAskedQuestionRepository()
        {
            db = new ApplicationContext();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="existingContext"></param>
        public FrequentlyAskedQuestionRepository(ApplicationContext existingContext)
        {
            db = existingContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FrequentlyAskedQuestion> GetAllQuestionsAnswers()
        {
            return db.FrequentlyAskedQuestions;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}