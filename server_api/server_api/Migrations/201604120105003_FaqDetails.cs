namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FaqDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestionAnswerUserReviews",
                c => new
                    {
                        User_Id = c.String(nullable: false, maxLength: 128),
                        FrequentlyAskedQuestion_Id = c.Int(nullable: false),
                        UserReviewScore = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.FrequentlyAskedQuestion_Id })
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.FrequentlyAskedQuestions", t => t.FrequentlyAskedQuestion_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.FrequentlyAskedQuestion_Id);

            RenameColumn("dbo.FrequentlyAskedQuestions", "Answer", "AnswerPlainText");
            AddColumn("dbo.FrequentlyAskedQuestions", "AnswerRichText", c => c.String());
            AddColumn("dbo.FrequentlyAskedQuestions", "ViewCount", c => c.Int(nullable: false));
            AddColumn("dbo.FrequentlyAskedQuestions", "TotaUserReviewScore", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AddColumn("dbo.FrequentlyAskedQuestions", "Answer", c => c.String());
            DropForeignKey("dbo.QuestionAnswerUserReviews", "FrequentlyAskedQuestion_Id", "dbo.FrequentlyAskedQuestions");
            DropForeignKey("dbo.QuestionAnswerUserReviews", "User_Id", "dbo.Users");
            DropIndex("dbo.QuestionAnswerUserReviews", new[] { "FrequentlyAskedQuestion_Id" });
            DropIndex("dbo.QuestionAnswerUserReviews", new[] { "User_Id" });
            DropColumn("dbo.FrequentlyAskedQuestions", "TotaUserReviewScore");
            DropColumn("dbo.FrequentlyAskedQuestions", "ViewCount");
            DropColumn("dbo.FrequentlyAskedQuestions", "AnswerRichText");
            RenameColumn("dbo.FrequentlyAskedQuestions", "AnswerPlainText", "Answer");
            DropTable("dbo.QuestionAnswerUserReviews");
        }
    }
}
