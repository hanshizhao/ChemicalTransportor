using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiemensCommunicator.EntityFramework.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlcConnections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false, comment: "主键ID")
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, comment: "连接名称"),
                    CpuType = table.Column<int>(type: "INTEGER", nullable: false, comment: "PLC CPU类型"),
                    Ip = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, comment: "PLC IP地址"),
                    Rack = table.Column<int>(type: "INTEGER", nullable: false, comment: "机架号"),
                    Slot = table.Column<int>(type: "INTEGER", nullable: false, comment: "插槽号"),
                    CacheRefreshInterval = table.Column<int>(type: "INTEGER", nullable: false, comment: "缓存刷新间隔（毫秒）"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, comment: "是否激活"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "创建时间"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcConnections", x => x.Id);
                },
                comment: "PLC连接配置表");

            migrationBuilder.CreateTable(
                name: "PlcGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false, comment: "主键ID")
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, comment: "分组名称"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true, comment: "分组描述"),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, comment: "排序顺序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcGroups", x => x.Id);
                },
                comment: "PLC分组表");

            migrationBuilder.CreateTable(
                name: "PlcTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false, comment: "主键ID")
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, comment: "标签名称"),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false, comment: "标签颜色")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcTags", x => x.Id);
                },
                comment: "PLC标签表");

            migrationBuilder.CreateTable(
                name: "PlcDataPoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false, comment: "主键ID")
                        .Annotation("Sqlite:Autoincrement", true),
                    ConnectionId = table.Column<long>(type: "INTEGER", nullable: false, comment: "关联的连接ID"),
                    GroupId = table.Column<long>(type: "INTEGER", nullable: true, comment: "分组ID"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, comment: "数据点名称"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true, comment: "数据点描述"),
                    DataType = table.Column<int>(type: "INTEGER", nullable: false, comment: "数据类型"),
                    DbNumber = table.Column<int>(type: "INTEGER", nullable: false, comment: "数据块编号"),
                    StartByte = table.Column<int>(type: "INTEGER", nullable: false, comment: "起始字节"),
                    BitOffset = table.Column<int>(type: "INTEGER", nullable: false, comment: "位偏移"),
                    VarType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, comment: "变量类型"),
                    DataCount = table.Column<int>(type: "INTEGER", nullable: false, comment: "数据数量"),
                    EnableCache = table.Column<bool>(type: "INTEGER", nullable: false, comment: "是否启用缓存"),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true, comment: "单位"),
                    ConversionFormula = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true, comment: "转换公式"),
                    MinValue = table.Column<decimal>(type: "TEXT", nullable: true, comment: "最小值"),
                    MaxValue = table.Column<decimal>(type: "TEXT", nullable: true, comment: "最大值"),
                    Tags = table.Column<string>(type: "TEXT", nullable: true, comment: "标签集合（JSON格式）"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "创建时间"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcDataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlcDataPoints_PlcConnections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "PlcConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlcDataPoints_PlcGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "PlcGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "PLC数据点配置表");

            migrationBuilder.CreateIndex(
                name: "IX_PlcConnections_IsActive",
                table: "PlcConnections",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PlcConnections_Name",
                table: "PlcConnections",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlcDataPoints_ConnectionId",
                table: "PlcDataPoints",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlcDataPoints_GroupId",
                table: "PlcDataPoints",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlcDataPoints_Name",
                table: "PlcDataPoints",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PlcGroups_Name",
                table: "PlcGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlcGroups_SortOrder",
                table: "PlcGroups",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PlcTags_Name",
                table: "PlcTags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlcDataPoints");

            migrationBuilder.DropTable(
                name: "PlcTags");

            migrationBuilder.DropTable(
                name: "PlcConnections");

            migrationBuilder.DropTable(
                name: "PlcGroups");
        }
    }
}
