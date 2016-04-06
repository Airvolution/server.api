namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFaqDetails2 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.QuestionAnswerUsefulnesses");
            AddColumn("dbo.QuestionAnswerUsefulnesses", "User_Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.QuestionAnswerUsefulnesses", "FrequentlyAskedQuestion_Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.QuestionAnswerUsefulnesses", new[] { "User_Id", "FrequentlyAskedQuestion_Id" });
            CreateIndex("dbo.QuestionAnswerUsefulnesses", "User_Id");
            CreateIndex("dbo.QuestionAnswerUsefulnesses", "FrequentlyAskedQuestion_Id");
            AddForeignKey("dbo.QuestionAnswerUsefulnesses", "FrequentlyAskedQuestion_Id", "dbo.FrequentlyAskedQuestions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuestionAnswerUsefulnesses", "User_Id", "dbo.Users", "Id", cascadeDelete: true);
            DropColumn("dbo.QuestionAnswerUsefulnesses", "userId");
            DropColumn("dbo.QuestionAnswerUsefulnesses", "frequentlyAskedQuestionId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuestionAnswerUsefulnesses", "frequentlyAskedQuestionId", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionAnswerUsefulnesses", "userId", c => c.String(nullable: false, maxLength: 128));
            DropForeignKey("dbo.QuestionAnswerUsefulnesses", "User_Id", "dbo.Users");
            DropForeignKey("dbo.QuestionAnswerUsefulnesses", "FrequentlyAskedQuestion_Id", "dbo.FrequentlyAskedQuestions");
            DropIndex("dbo.QuestionAnswerUsefulnesses", new[] { "FrequentlyAskedQuestion_Id" });
            DropIndex("dbo.QuestionAnswerUsefulnesses", new[] { "User_Id" });
            DropPrimaryKey("dbo.QuestionAnswerUsefulnesses");
            DropColumn("dbo.QuestionAnswerUsefulnesses", "FrequentlyAskedQuestion_Id");
            DropColumn("dbo.QuestionAnswerUsefulnesses", "User_Id");
            AddPrimaryKey("dbo.QuestionAnswerUsefulnesses", new[] { "userId", "frequentlyAskedQuestionId" });
        }
    }
}
