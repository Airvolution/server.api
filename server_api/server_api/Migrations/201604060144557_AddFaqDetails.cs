namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFaqDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestionAnswerUsefulnesses",
                c => new
                    {
                        userId = c.String(nullable: false, maxLength: 128),
                        frequentlyAskedQuestionId = c.Int(nullable: false),
                        UserUsefulnessScore = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.userId, t.frequentlyAskedQuestionId });
            
            AddColumn("dbo.FrequentlyAskedQuestions", "AnswerMarkDown", c => c.String());
            AddColumn("dbo.FrequentlyAskedQuestions", "ViewCount", c => c.Int(nullable: false));
            AddColumn("dbo.FrequentlyAskedQuestions", "TotaUserlUsefulnessScore", c => c.Int(nullable: false));
            RenameColumn("dbo.FrequentlyAskedQuestions", "Answer", "AnswerPlainText");
            
        }
        
        public override void Down()
        {
            RenameColumn("dbo.FrequentlyAskedQuestions", "AnswerPlainText", "Answer");
            DropColumn("dbo.FrequentlyAskedQuestions", "TotaUserlUsefulnessScore");
            DropColumn("dbo.FrequentlyAskedQuestions", "ViewCount");
            DropColumn("dbo.FrequentlyAskedQuestions", "AnswerMarkDown");
            DropTable("dbo.QuestionAnswerUsefulnesses");
        }
    }
}
