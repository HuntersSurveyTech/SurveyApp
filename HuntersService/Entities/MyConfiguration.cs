using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace HuntersService.Entities
{
   	internal sealed class MyConfiguration : DbMigrationsConfiguration<MyDbContext>
    {
        public MyConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }



        protected override void Seed(MyDbContext context)
        {


	       


        }
    }
}