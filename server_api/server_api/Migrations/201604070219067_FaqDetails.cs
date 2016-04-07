namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FaqDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestionAnswerUsefulnesses",
                c => new
                    {
                        User_Id = c.String(nullable: false, maxLength: 128),
                        FrequentlyAskedQuestion_Id = c.Int(nullable: false),
                        UserUsefulnessScore = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.FrequentlyAskedQuestion_Id })
                .ForeignKey("dbo.FrequentlyAskedQuestions", t => t.FrequentlyAskedQuestion_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.FrequentlyAskedQuestion_Id);
            
            AddColumn("dbo.FrequentlyAskedQuestions", "AnswerPlainText", c => c.String());
            AddColumn("dbo.FrequentlyAskedQuestions", "AnswerRichText", c => c.String());
            AddColumn("dbo.FrequentlyAskedQuestions", "ViewCount", c => c.Int(nullable: false));
            AddColumn("dbo.FrequentlyAskedQuestions", "TotaUserlUsefulnessScore", c => c.Int(nullable: false));
            DropColumn("dbo.FrequentlyAskedQuestions", "Answer");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FrequentlyAskedQuestions", "Answer", c => c.String());
            DropForeignKey("dbo.QuestionAnswerUsefulnesses", "User_Id", "dbo.Users");
            DropForeignKey("dbo.QuestionAnswerUsefulnesses", "FrequentlyAskedQuestion_Id", "dbo.FrequentlyAskedQuestions");
            DropIndex("dbo.QuestionAnswerUsefulnesses", new[] { "FrequentlyAskedQuestion_Id" });
            DropIndex("dbo.QuestionAnswerUsefulnesses", new[] { "User_Id" });
            DropColumn("dbo.FrequentlyAskedQuestions", "TotaUserlUsefulnessScore");
            DropColumn("dbo.FrequentlyAskedQuestions", "ViewCount");
            DropColumn("dbo.FrequentlyAskedQuestions", "AnswerRichText");
            DropColumn("dbo.FrequentlyAskedQuestions", "AnswerPlainText");
            DropTable("dbo.QuestionAnswerUsefulnesses");
        }
    }
}
