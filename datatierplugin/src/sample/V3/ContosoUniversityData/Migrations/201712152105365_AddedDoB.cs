namespace ContosoUniversityData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDoB : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Student", "DateOfBirth", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Student", "DateOfBirth");
        }
    }
}
