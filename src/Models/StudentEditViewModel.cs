using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Workforce.Models;

namespace Workforce.Models.ViewModels
{
    public class StudentEditViewModel
    {
        public Student Student { get; set; }

        [Display(Name="Current Cohort")]
        public List<SelectListItem> Cohorts { get; }

        private readonly IConfiguration _config;

        public IDbConnection Connection {
            get {
                return new SqlConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        public StudentEditViewModel() {}

        public StudentEditViewModel(IConfiguration config)
        {
            _config = config;

            string sql = $@"SELECT Id, Name FROM Cohort";

            using (IDbConnection conn = Connection) {
                List<Cohort> cohorts = (conn.Query<Cohort> (sql)).ToList();

                this.Cohorts = cohorts
                    .Select(li => new SelectListItem {
                        Text = li.Name,
                        Value = li.Id.ToString()
                    }).ToList();
            }


            // Add a prompt so that the <select> element isn't blank
            this.Cohorts.Insert(0, new SelectListItem {
                Text = "Choose cohort...",
                Value = "0"
            });
        }
    }
}
