using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesAPI.Migrations
{
    /// <inheritdoc />
    public partial class propertyNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "imdbVotes",
                table: "Movies",
                newName: "IMDbVotes");

            migrationBuilder.RenameColumn(
                name: "imdbRating",
                table: "Movies",
                newName: "IMDbRating");

            migrationBuilder.RenameColumn(
                name: "imdbID",
                table: "Movies",
                newName: "IMDbID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IMDbVotes",
                table: "Movies",
                newName: "imdbVotes");

            migrationBuilder.RenameColumn(
                name: "IMDbRating",
                table: "Movies",
                newName: "imdbRating");

            migrationBuilder.RenameColumn(
                name: "IMDbID",
                table: "Movies",
                newName: "imdbID");
        }
    }
}
