using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Workforce.Models;
using Workforce.Models.ViewModels;
using System.Data.SqlClient;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Workforce.Controllers
{
	public class InstructorController : Controller
	{

		private readonly IConfiguration _config;

		public InstructorController(IConfiguration config)
		{
			_config = config;
		}

		public IDbConnection Connection
		{
			get
			{
				return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
			}
		}

		public async Task<IActionResult> Index()
		{
			string sql = @"
			SELECT
				i.Id,
				i.FirstName,
				i.LastName,
				i.SlackHandle,
				i.Specialty,
				c.Id,
				c.Name
			FROM Instructor i
			JOIN Cohort c ON i.CohortId = c.Id
			";

			using (IDbConnection conn = Connection)
			{
				Dictionary<int, Instructor> instructors = new Dictionary<int, Instructor>();

				var instructorQuerySet = await conn.QueryAsync<Instructor, Cohort, Instructor>(
					sql,
					(instructor, cohort) =>
					{
						if (!instructors.ContainsKey(instructor.Id))
						{
							instructors[instructor.Id] = instructor;
						}
						instructors[instructor.Id].Cohort = cohort;
						return instructor;
					}
				);

				return View(instructors.Values);
			}
		}

		public async Task<IActionResult> Details(int? id)
		{
			return View();
		}
	}
}
