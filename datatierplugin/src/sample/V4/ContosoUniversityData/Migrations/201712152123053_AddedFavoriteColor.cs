namespace ContosoUniversityData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFavoriteColor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Student", "FavoriteColor", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Student", "FavoriteColor");
        }
    }
}
