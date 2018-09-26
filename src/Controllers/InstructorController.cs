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
			if (id == null)
			{
				return NotFound();
			}

			string sql = $@"
            select
                i.Id,
                i.FirstName,
                i.LastName,
                i.SlackHandle,
				i.Specialty
            from Instructor i
            WHERE i.Id = {id}";

			using (IDbConnection conn = Connection)
			{
				Instructor teacher = (await conn.QueryAsync<Instructor>(sql)).ToList().Single();

				if (teacher == null)
				{
					return NotFound();
				}

			return View(teacher);

			}
		}

		/* should this work with async Task<IActionResult>? or just ActionResult? */
		[HttpGet]
		public IActionResult Create()
		{
			InstructorEditViewModel viewModel = new InstructorEditViewModel(_config);
			return View(viewModel);

			/*
			using (IDbConnection taco = Connection)
			{

				InstructorEditViewModel viewModel = new InstructorEditViewModel(_config);

				viewModel.Instructor = (await taco.QueryAsync<Instructor, Cohort, Instructor>
				(
					sql,
					(teacher, group) =>
					{
						teacher.Cohort = group;
						return teacher;
					}
				)).Single();

				return View(viewModel);
			}
			*/
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(InstructorEditViewModel teacher)
		{

			Console.WriteLine(teacher.Instructor.FirstName);
			Console.WriteLine("Hello world");

			if (ModelState.IsValid)
			{
				string sql = $@"
				INSERT INTO Instructor (
					FirstName, 
					LastName, 
					Specialty, 
					SlackHandle, 
					CohortId
				)
				VALUES (
					'{teacher.Instructor.FirstName}', 
					'{teacher.Instructor.LastName}', 
					'{teacher.Instructor.Specialty}', 
					'{teacher.Instructor.SlackHandle}', 
					{teacher.Instructor.CohortId}
				)";

				Console.WriteLine(teacher.Instructor.FirstName);
				Console.WriteLine("Hello world");

				using (IDbConnection creating = Connection)
				{
					bool instructorAdded = (await creating.ExecuteAsync(sql)) > 0;
					int rowsAffected = await creating.ExecuteAsync(sql);
					if (rowsAffected > 0)
					{
						return RedirectToAction(nameof(Index));
					} else
					{
						Console.WriteLine("No rows affected");
						throw new Exception("No rows affected");
					}
				}
			}

			// ModelState was invalid, or saving the Instructor data failed. Show the form again.
			//InstructorEditViewModel currentInfo = new InstructorEditViewModel(_config);
			//currentInfo.Instructor = teacher.Instructor;
			//return RedirectToAction(nameof(Index));
			return View(teacher);

			/*
			using (IDbConnection conn = Connection)
			{
				IEnumerable<Cohort> cohorts = (await conn.QueryAsync<Cohort>("SELECT Id, Name FROM Cohort")).ToList();
				// ViewData["CohortId"] = new SelectList (cohorts, "Id", "Name", student.CohortId);
				ViewData["CohortId"] = await CohortList(student.CohortId);
			}
			*/
		}




		[HttpGet]
		public async Task<IActionResult> Edit (int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			string sql = $@"
			SELECT
                    i.Id,
                    i.FirstName,
                    i.LastName,
					i.Specialty,
                    i.SlackHandle,
                    i.CohortId,
                    c.Id,
                    c.Name
			FROM Instructor i
            JOIN Cohort c on i.CohortId = c.Id
            WHERE i.Id = {id}
			";

			using (IDbConnection taco = Connection)
			{
				InstructorEditViewModel viewModel = new InstructorEditViewModel(_config);

				viewModel.Instructor = (await taco.QueryAsync<Instructor, Cohort, Instructor>
				(
					sql,
					(teacher, group) =>
					{
						teacher.Cohort = group;
						return teacher;
					}
				)).Single();

				return View(viewModel);

			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit (int id, InstructorEditViewModel model)
		{
			if (id != model.Instructor.Id)
			{
				return NotFound();
			}

			if (!(ModelState.IsValid))
			{
				return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
			} else {
				string sql = $@"
				UPDATE Instructor 
				SET 
					FirstName = '{model.Instructor.FirstName}',
					LastName = '{model.Instructor.LastName}',
					Specialty = '{model.Instructor.Specialty}',
					SlackHandle = '{model.Instructor.SlackHandle}',
					CohortId = {model.Instructor.CohortId}
				WHERE Id = {id}
				";

				using (IDbConnection conn = Connection)
				{
					int rowsAffected = await conn.ExecuteAsync(sql);

					if (rowsAffected > 0)
					{
						return RedirectToAction(nameof(Index));
					} else {
						throw new Exception("No rows affected");					
					}
				}
			}



		}


	}
}
