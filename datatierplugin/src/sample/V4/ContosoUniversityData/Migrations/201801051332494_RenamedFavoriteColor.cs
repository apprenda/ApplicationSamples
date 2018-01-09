namespace ContosoUniversityData.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RenamedFavoriteColor : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Student", name: "FavoriteColor", newName: "FavColor");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.Student", name: "FavColor", newName: "FavoriteColor");
        }
    }
}
