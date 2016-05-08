﻿using System;
using System.Collections.Generic;
using System.Linq;
using InterLinq.UnitTests.Artefacts;
using InterLinq.UnitTests.Artefacts.EntityFramework4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Objects.SqlClient;

namespace InterLinq.UnitTests.EntityFramework4
{
    /// <summary>
    /// Tests with the domain model of a <see cref="Company"/> with <see cref="Department">Departments</see>
    /// and <see cref="Employee"/>.
    /// </summary>
    public class CompanyExampleTest
    {

        #region Fields

        /// <summary>
        /// Context instance providing <see cref="IQueryable">IQueryables</see>.
        /// </summary>
        protected CompanyContext companyExampleContext;

        #endregion

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Count/Sum/Min/Max/Avg Tests

        /// <summary>
        /// Simply count all employees.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Simply count all employees.")]
        public void Count_01_Employees()
        {
            int numberOfEmployees = companyExampleContext.Employees.Count();
            Assert.AreEqual(13, numberOfEmployees);
        }

        /// <summary>
        /// Gets the number of female employees.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Gets the number of female employees.")]
        public void Count_02_FemaleEmployees()
        {
            int numberOfFemaleEmployees = companyExampleContext.Employees.Count(emp => !(emp.IsMale));
            Assert.AreEqual(6, numberOfFemaleEmployees);
        }

        /// <summary>
        /// Sums all salaries after a selection.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Sums all salaries after a selection.")]
        public void Sum_01_AllSalaries()
        {
            decimal totalSalary = companyExampleContext.Employees.Select(emp => emp.Salary).Sum();
            Assert.AreEqual(96000M, totalSalary);
        }

        /// <summary>
        /// Gets the minimum Grade.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Gets the minimum Grade.")]
        public void Min_01_Grade()
        {
            double minGrade = companyExampleContext.Employees.Select(emp => emp.Grade).Min();
            Assert.AreEqual(1, minGrade);
        }

        /// <summary>
        /// Selects the Departments and their employee with the lowest salary.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Selects the Departments and their employee with the lowest salary.")]
        public void Min_02_SalaryInDepartment()
        {
            var minSalaryQuery = from e in companyExampleContext.Employees
                                 group e by e.Department into g
                                 select new
                                 {
                                     Department = g.Key,
                                     MinSalary = g.Min(e => e.Salary),
                                     LowestSalaryEmployee =
                                         from empInGroup in g
                                         where empInGroup.Salary == g.Min(e => e.Salary)
                                         select empInGroup
                                 };
            var foundMinSalaries = minSalaryQuery.ToArray();
            foreach (var minSalaryEntry in foundMinSalaries)
            {
                // When the CEO is found
                if (minSalaryEntry.Department == null)
                {
                    Assert.AreEqual(1, minSalaryEntry.LowestSalaryEmployee.Count());
                    Assert.AreEqual(10000M, minSalaryEntry.LowestSalaryEmployee.First().Salary);
                }
                // When the "Instant-Guezli Research" is found
                else if (minSalaryEntry.Department.Name == "Instant-Guezli Research")
                {
                    Assert.AreEqual(1, minSalaryEntry.LowestSalaryEmployee.Count());
                    Assert.AreEqual(4000M, minSalaryEntry.LowestSalaryEmployee.First().Salary);
                }
                // When the "Instant-Guezli Production" is found
                else if (minSalaryEntry.Department.Name == "Instant-Guezli Production")
                {
                    Assert.AreEqual(1, minSalaryEntry.LowestSalaryEmployee.Count());
                    Assert.AreEqual(3000M, minSalaryEntry.LowestSalaryEmployee.First().Salary);
                }
                // When the "Instant-Guezli Import" is found
                else if (minSalaryEntry.Department.Name == "Instant-Guezli Import")
                {
                    Assert.AreEqual(1, minSalaryEntry.LowestSalaryEmployee.Count());
                    Assert.AreEqual(3500M, minSalaryEntry.LowestSalaryEmployee.First().Salary);
                }
                else
                {
                    Assert.Fail("A unknown department has been found.");
                }
            }
        }

        /// <summary>
        /// Select the highest foundation date.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Select the highest foundation date.")]
        public void Max_01_Foundation()
        {
            DateTime maxFoundationQuery = companyExampleContext.Departments.Select(fd => fd.Foundation).Max();
            Assert.AreEqual(new DateTime(1980, 5, 5), maxFoundationQuery);
        }

        /// <summary>
        /// Select the highest quality level.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Select the highest quality level.")]
        public void Max_02_QualityLevel()
        {
            int maxQualityLevel = companyExampleContext.Departments.Max(d => d.QualityLevel);
            Assert.AreEqual(8, maxQualityLevel);
        }

        /// <summary>
        /// Gets the average Grade of all employees.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Gets the average Grade of all employees.")]
        public void Avg_01_Grade()
        {
            double avgGrade = companyExampleContext.Employees.Average(e => e.Grade);
            Assert.AreEqual(6.0d, Math.Round(avgGrade, 0));
            // MSSQL's function AVG returns the average as int, therefore
            // the result is 6.0 and not 6.0769 as expected.
        }

        #endregion

        #region Join

        /// <summary>
        /// Use foreign key navigation to select all employees of a department.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Use foreign key navigation to select all employees of a department.")]
        public void Join_01_SimpleJoin()
        {
            var query = from dep in companyExampleContext.Departments
                        from emp in dep.Employees
                        where dep.Name == "Instant-Guezli Research"
                        select emp;
            var foundEmployees = query.ToArray();
            Assert.AreEqual(3, foundEmployees.Length);
        }

        /// <summary>
        /// Use foreign key navigation to select all departments whose manager is female.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Use foreign key navigation to select all departments whose manager is female.")]
        public void Join_02_FKNavigation()
        {
            var femManagedDepQuery = from dep in companyExampleContext.Departments
                                     where !dep.Manager.IsMale
                                     select dep;
            Department[] foundFManagedDep = femManagedDepQuery.ToArray();
            Assert.AreEqual(1, foundFManagedDep.Length);
        }

        /// <summary>
        /// make a simple join
        /// </summary>
        [TestMethod]
        [Owner("Simon Gubler")]
        [Description("make a simple join")]
        public void Join_03_Simple_Join_Operator()
        {
            var employeesQuery = from d in companyExampleContext.Departments
                                 join e in companyExampleContext.Employees on d equals e.Department
                                 where d.Name.Equals("Instant-Guezli Production")
                                 select new { DepartmentName = d.Name, EmployeeName = e.Name };

            var firstJoined = employeesQuery.First();

            Assert.AreEqual("Rainer Zufall", firstJoined.EmployeeName);
        }

        /// <summary>
        /// make a complex join with multiple joins
        /// </summary>
        [TestMethod]
        [Owner("Simon Gubler")]
        [Description("make a complex join with multiple joins")]
        public void Join_03_Multiple_Join_Operators()
        {
            var employeesQuery = from d in companyExampleContext.Departments
                                 join e in companyExampleContext.Employees on d equals e.Department
                                 join c in companyExampleContext.Companies on d.Company equals c
                                 where c.Name.Equals("Instant-Guezli")
                                 select new { DepartmentName = d.Name, EmployeeName = e.Name, CompanyName = c.Name };

            var firstJoined = employeesQuery.First();

            Assert.AreEqual("Stephanie Süss", firstJoined.EmployeeName);
        }

        #endregion

        #region OrderBy

        /// <summary>
        /// Simply order all employees by their salary.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Simply order all employees by their salary.")]
        public void OrderBy_01_Simple()
        {
            var employeeQuery = from emp in companyExampleContext.Employees
                                orderby emp.Salary
                                select emp;
            Employee[] foundEmployees = employeeQuery.ToArray();
            Assert.AreEqual(13, foundEmployees.Length);
            Employee lastEmployee = null;
            foreach (Employee emp in foundEmployees)
            {
                if (lastEmployee != null)
                {
                    Assert.IsTrue(lastEmployee.Salary <= emp.Salary, "Last employees salary was higher but shoudn't.");
                }
                lastEmployee = emp;
            }
        }

        /// <summary>
        /// Simply order all employees by their salary descending.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Simply order all employees by their salary descending.")]
        public void OrderBy_02_Descending()
        {
            var employeeQuery = from emp in companyExampleContext.Employees
                                orderby emp.Salary descending
                                select emp;
            Employee[] foundEmployees = employeeQuery.ToArray();
            Assert.AreEqual(13, foundEmployees.Length);
            Employee lastEmployee = null;
            foreach (Employee emp in foundEmployees)
            {
                if (lastEmployee != null)
                {
                    Assert.IsTrue(lastEmployee.Salary >= emp.Salary, "Last employees salary was lower but shoudn't.");
                }
                lastEmployee = emp;
            }
        }

        #endregion

        #region GroupBy

        /// <summary>
        /// Use group by and max to get the maximum salary for each department.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Use group by and max to get the maximum salary for each department.")]
        public void GroupBy_01_MaxSalary()
        {
            var maxSalaryQuery = from emp in companyExampleContext.Employees
                                 group emp by emp.Department into g
                                 select new
                                 {
                                     Department = g.Key,
                                     MaxSalary = g.Max(e => e.Salary)
                                 };
            var maxSalaries = maxSalaryQuery.ToArray();
            Assert.AreEqual(4, maxSalaries.Length);
            foreach (var maxSalary in maxSalaries)
            {
                // When the CEO is found
                if (maxSalary.Department == null)
                {
                    Assert.AreEqual(20000M, maxSalary.MaxSalary);
                }
                // When the "Instant-Guezli Research" is found
                else if (maxSalary.Department.Name == "Instant-Guezli Research")
                {
                    Assert.AreEqual(6000M, maxSalary.MaxSalary);
                }
                // When the "Instant-Guezli Production" is found
                else if (maxSalary.Department.Name == "Instant-Guezli Production")
                {
                    Assert.AreEqual(6500M, maxSalary.MaxSalary);
                }
                // When the "Instant-Guezli Import" is found
                else if (maxSalary.Department.Name == "Instant-Guezli Import")
                {
                    Assert.AreEqual(5000M, maxSalary.MaxSalary);
                }
                else
                {
                    Assert.Fail("A unknown department has been found.");
                }
            }
        }

        /// <summary>
        /// Use group by to find the department with more then 25000 total salary.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Use group by to find the department with more then 25000 total salary.")]
        public void GroupBy_02_Having()
        {
            var salaryQuery = from emp in companyExampleContext.Employees
                              group emp by emp.Department into g
                              where g.Key != null
                              where g.Sum(emp => emp.Salary) > 14000
                              select new
                              {
                                  DepartmentName = g.Key.Name,
                                  MaxSalary = g.Sum(emp => emp.Salary)
                              };
            var maxSalaries = salaryQuery.ToArray();
            Assert.AreEqual(2, maxSalaries.Length);
            Assert.AreEqual("Instant-Guezli Research", maxSalaries[0].DepartmentName);
            Assert.AreEqual("Instant-Guezli Production", maxSalaries[1].DepartmentName);
            Assert.AreEqual(15000M, maxSalaries[0].MaxSalary);
        }

        /// <summary>
        /// Use group by to group employees by department and sex.
        /// </summary>
        [TestMethod]
        [Owner("Pascal Schaefer")]
        [Description("Use group by to group employees by department and sex.")]
        public void GroupBy_03_DoubleGroup()
        {
            var query = from emp in companyExampleContext.Employees
                        group emp by new { emp.Department, emp.IsMale } into g
                        select new { g.Key, g };
            var results = query.ToArray();
            Assert.AreEqual(8, results.Length);
        }

        /// <summary>
        /// Reads the average salary in each department (Employees and Manager).
        /// The CEO is also included.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Reads the average salary in each department (Employees and Manager). " +
                      "The CEO is also included.")]
        public void GroupBy_04_AverageSalaryInDepartment()
        {
            var query = from e in companyExampleContext.Employees
                        group e by e.Department into g
                        select new
                        {
                            Department = g.Key,
                            AverageSalary = g.Average(emp => emp.Salary)
                        };

            var result = query.ToArray();

            Assert.AreEqual(4, result.Length);

        }

        #endregion

        #region Where

        /// <summary>
        /// Finds all Employees by a name.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Finds all Employees by a name.")]
        public void Where_01_EmployeeByName()
        {
            string name = "Sio Wernli";
            var employeesQuery = from e in companyExampleContext.Employees
                                 where e.Name == name
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(1, foundEmployees.Length);
            Assert.AreEqual(name, foundEmployees[0].Name);
        }

        /// <summary>
        /// Finds all Employees by a Department name.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Finds all Employees by a Department name.")]
        public void Where_02_EmployeeByDepartmentName()
        {
            string name = "Instant-Guezli Research";
            var employeesQuery = from e in companyExampleContext.Employees
                                 where e.Department != null &&
                                       e.Department.Name == name
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
        }

        /// <summary>
        /// Finds all Employees by a Department name.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Finds all Employees by a Department name.")]
        public void Where_03_DepartmentByFoundation()
        {
            var departmentsQuery = from d in companyExampleContext.Departments
                                   where d.Foundation >= new DateTime(1910, 1, 1)
                                   select d;

            Department[] foundDepartments = departmentsQuery.ToArray();

            Assert.AreEqual(2, foundDepartments.Length);
            Assert.AreEqual("Instant-Guezli Research", foundDepartments[0].Name);
            Assert.AreEqual("Instant-Guezli Import", foundDepartments[1].Name);
        }

        /// <summary>
        /// Finds all male Employees with a low grade (less or equal than 5).
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Finds all male Employees with a low grade (less or equal than 5).")]
        public void Where_04_MaleWithLowGrade()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 where e.IsMale && e.Grade <= 5
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Harry Bo", foundEmployees[0].Name);
            Assert.AreEqual("Rainer Zufall", foundEmployees[1].Name);
            Assert.AreEqual("Lee Mone", foundEmployees[2].Name);
        }

        /// <summary>
        /// Finds all female Employees with best grade (greater or equal than 10).
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Finds all female Employees with best grade (greater or equal than 10).")]
        public void Where_05_FemalePlusBestGradedMale()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 where !e.IsMale || e.Grade >= 10
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(8, foundEmployees.Length);
            Assert.AreEqual("Sio Wernli", foundEmployees[0].Name);
            Assert.AreEqual("Stephanie Süss", foundEmployees[1].Name);
            Assert.AreEqual("Manuela Zucker", foundEmployees[2].Name);
            Assert.AreEqual("Prof. Joe Kolade", foundEmployees[3].Name);
            Assert.AreEqual("Clara Vorteil", foundEmployees[4].Name);
            Assert.AreEqual("Anna Nass", foundEmployees[5].Name);
            Assert.AreEqual("Franziska Ner", foundEmployees[6].Name);
            Assert.AreEqual("Gertraut Sichnicht", foundEmployees[7].Name);
        }

        /// <summary>
        /// Finds best grade male (greater or equal than 10).
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Finds best grade male (greater or equal than 10).")]
        public void Where_06_BestGradedMale()
        {
            var employeesQuery = companyExampleContext.Employees
                                 .Where(e => e.IsMale)
                                 .Where(e => e.Grade >= 10);

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(2, foundEmployees.Length);
            Assert.AreEqual("Sio Wernli", foundEmployees[0].Name);
            Assert.AreEqual("Prof. Joe Kolade", foundEmployees[1].Name);
        }

        /// <summary>
        /// Finds the first employee.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Finds the first employee.")]
        public void Where_07_FirstEmployee()
        {
            Employee foundEmployee = companyExampleContext.Employees
                                     .First();
            Assert.AreEqual("Sio Wernli", foundEmployee.Name);
        }

        /// <summary>
        /// Finds the first best graded employee.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Finds the first best graded employee.")]
        public void Where_08_FirstBestGradedEmployee()
        {
            Employee foundEmployee = companyExampleContext.Employees
                                     .First(e => e.Grade >= 10);
            Assert.AreEqual("Sio Wernli", foundEmployee.Name);
        }

        #endregion

        #region Union / Concat / Intersect / Except

        /// <summary>
        /// Concats Name and Salary of all Employees with all Department names.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Concats Name and Salary of all Employees with all Department names.")]
        public void Concat_01()
        {
            var concatenationQuery = (
                                      from e in companyExampleContext.Employees
                                      select e.Name
                                     )
                                     .Concat
                                     (
                                      from e in companyExampleContext.Employees
                                      select SqlFunctions.StringConvert(e.Salary)
                                     )
                                     .Concat
                                     (
                                      from d in companyExampleContext.Departments
                                      select d.Name
                                     );

            var foundConcatenations = concatenationQuery.ToArray();

            Assert.AreEqual(29, foundConcatenations.Length);
        }

        /// <summary>
        /// Concats Name / Grade of all Employees with all Department Name / QualityLevel.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Concats Name / Grade of all Employees with all Department Name / QualityLevel.")]
        public void Concat_02()
        {
            var concatenationQuery = (
                                      from e in companyExampleContext.Employees
                                      select new { e.Name, e.Grade }
                                     )
                                     .Concat
                                     (
                                      from d in companyExampleContext.Departments
                                      select new { Name = d.Name, Grade = d.QualityLevel }
                                     );

            var foundConcatenations = concatenationQuery.ToArray();

            Assert.AreEqual(16, foundConcatenations.Length);
        }

        /// <summary>
        /// Unions the Grade of all Employees with all Department QualityLevels.
        /// Union removes duplicated values (Distinct).
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Unions the Grade of all Employees with all Department QualityLevels. " +
                      "Union removes duplicated values (Distinct).")]
        public void Union_01()
        {
            var unionQuery = (
                              from e in companyExampleContext.Employees
                              select e.Grade
                             )
                             .Union
                             (
                              from d in companyExampleContext.Departments
                              select d.QualityLevel
                             );

            var foundUnions = unionQuery.ToArray();

            Assert.AreEqual(10, foundUnions.Length);
        }

        /// <summary>
        /// Intersects the Grade of all Employees with all Department QualityLevels.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Intersects the Grade of all Employees with all Department QualityLevels.")]
        public void Intersect_01()
        {
            var intersectQuery = (
                                  from e in companyExampleContext.Employees
                                  select e.Grade
                                 )
                                 .Intersect
                                 (
                                  from d in companyExampleContext.Departments
                                  select d.QualityLevel
                                 );

            var foundIntersect = intersectQuery.ToArray();

            Assert.AreEqual(2, foundIntersect.Length);
        }

        /// <summary>
        /// Excepts the Grade of all Employees with all Department QualityLevels.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Excepts the Grade of all Employees with all Department QualityLevels.")]
        public void Except_01()
        {
            var intersectQuery = (
                                  from e in companyExampleContext.Employees
                                  select e.Grade
                                 )
                                 .Except
                                 (
                                  from d in companyExampleContext.Departments
                                  select d.QualityLevel
                                 );

            var foundIntersect = intersectQuery.ToArray();

            Assert.AreEqual(7, foundIntersect.Length);
        }

        /// <summary>
        /// Excepts the Grade of all Employees with all Department QualityLevels.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Excepts the Grade of all Employees with all Department QualityLevels.")]
        public void Distinct_01()
        {
            var employeeQuery = from e in companyExampleContext.Employees
                                where e.Department != null
                                select e.Department.Name;

            var employeeDistinctQuery = employeeQuery.Distinct();
            string[] foundEmployees = employeeDistinctQuery.ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Instant-Guezli Import", foundEmployees[0]);
            Assert.AreEqual("Instant-Guezli Production", foundEmployees[1]);
            Assert.AreEqual("Instant-Guezli Research", foundEmployees[2]);;
        }

        #endregion

        #region Skip / Take

        /// <summary>
        /// Selects all Employees. The first 10 results will be skipped.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Selects all Employees. The first 10 results will be skipped.")]
        public void Skip_01()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 orderby e.Id ascending
                                 select e;

            Employee[] foundEmployees = employeesQuery.Skip(10).ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Lee Mone", foundEmployees[0].Name);
            Assert.AreEqual("Franziska Ner", foundEmployees[1].Name);
            Assert.AreEqual("Gertraut Sichnicht", foundEmployees[2].Name);
        }

        /// <summary>
        /// Selects all Employees descending. The first 10 results will be skipped.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Selects all Employees descending. The first 10 results will be skipped.")]
        public void Skip_02()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 orderby e.Id descending
                                 select e;

            Employee[] foundEmployees = employeesQuery.Skip(10).ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Stephanie Süss", foundEmployees[0].Name);
            Assert.AreEqual("Dr. Rainer Hohn", foundEmployees[1].Name);
            Assert.AreEqual("Sio Wernli", foundEmployees[2].Name);
        }

        /// <summary>
        /// The same test as Skip_02, but the return value of skip is saved in a separate variable.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("The same test as Skip_02, but the return value of skip is saved in a separate variable.")]
        public void Skip_03()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 orderby e.Id descending
                                 select e;

            var employeesSkipQuery = employeesQuery.Skip(10);
            Employee[] foundEmployees = employeesSkipQuery.ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Stephanie Süss", foundEmployees[0].Name);
            Assert.AreEqual("Dr. Rainer Hohn", foundEmployees[1].Name);
            Assert.AreEqual("Sio Wernli", foundEmployees[2].Name);
        }

        /// <summary>
        /// Selects first 3 Employees.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Selects first 3 Employees.")]
        public void Take_01()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 orderby e.Id ascending
                                 select e;

            Employee[] foundEmployees = employeesQuery.Take(3).ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Sio Wernli", foundEmployees[0].Name);
            Assert.AreEqual("Dr. Rainer Hohn", foundEmployees[1].Name);
            Assert.AreEqual("Stephanie Süss", foundEmployees[2].Name);
        }

        /// <summary>
        /// Selects last 3 Employees.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Selects last 3 Employees.")]
        public void Take_02()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 orderby e.Id descending
                                 select e;

            Employee[] foundEmployees = employeesQuery.Take(3).ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Gertraut Sichnicht", foundEmployees[0].Name);
            Assert.AreEqual("Franziska Ner", foundEmployees[1].Name);
            Assert.AreEqual("Lee Mone", foundEmployees[2].Name);
        }

        /// <summary>
        /// Selects first 3 Employees after skipping 3 elements.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Selects first 3 Employees after skipping 3 elements.")]
        public void SkipTake_01()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 orderby e.Id ascending
                                 select e;

            Employee[] foundEmployees = employeesQuery.Skip(3).Take(3).ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Manuela Zucker", foundEmployees[0].Name);
            Assert.AreEqual("Harry Bo", foundEmployees[1].Name);
            Assert.AreEqual("Prof. Joe Kolade", foundEmployees[2].Name);
        }

        /// <summary>
        /// Selects first 3 Employees after skipping 3 elements descending.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Selects first 3 Employees after skipping 3 elements descending.")]
        public void SkipTake_02()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 orderby e.Id descending
                                 select e;

            Employee[] foundEmployees = employeesQuery.Skip(3).Take(3).ToArray();

            Assert.AreEqual(3, foundEmployees.Length);
            Assert.AreEqual("Anna Nass", foundEmployees[0].Name);
            Assert.AreEqual("Johannes Beer", foundEmployees[1].Name);
            Assert.AreEqual("Clara Vorteil", foundEmployees[2].Name);
        }

        #endregion

        #region Null

        /// <summary>
        /// Selects all Employees with no department.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("Selects all Employees with no department.")]
        public void Null_01()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 where e.Department == null
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(4, foundEmployees.Length);
            Assert.AreEqual("Sio Wernli", foundEmployees[0].Name);
        }

        #endregion

        #region String / Data Functions

        /// <summary>
        /// This sample uses the + operator to concatenate string fields.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses the + operator to concatenate string fields.")]
        public void String_01_PlusOperator()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 select new
                                 {
                                     ConcatenatedValue = "Name: " + e.Name + ", " +
                                                         "Grade: " + SqlFunctions.StringConvert((decimal)e.Grade).Trim()
                                 };

            var foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(13, foundEmployees.Length);
            Assert.AreEqual("Name: Sio Wernli, Grade: 10", foundEmployees[0].ConcatenatedValue);
        }

        /// <summary>
        /// This sample uses the Length property to find all Employees whose
        /// name is longer than 15 characters.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses the Length property to find all Employees whose " +
                      "name is longer than 15 characters.")]
        public void String_02_Length()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 where e.Name.Length > 15
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(2, foundEmployees.Length);
            Assert.AreEqual("Prof. Joe Kolade", foundEmployees[0].Name);
            Assert.AreEqual("Gertraut Sichnicht", foundEmployees[1].Name);
        }

        /// <summary>
        /// This sample uses the Contains method to find all Employees whose
        /// Name contains '.'
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses the Contains method to find all Employees whose " +
                      "Name contains '.'")]
        public void String_03_Contains()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 where e.Name.Contains(".")
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(2, foundEmployees.Length);
            Assert.AreEqual("Dr. Rainer Hohn", foundEmployees[0].Name);
            Assert.AreEqual("Prof. Joe Kolade", foundEmployees[1].Name);
        }

        /// <summary>
        /// This sample uses the IndexOf method to find the first instance of
        /// a space in each Employees contact name.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses the IndexOf method to find the first instance of " +
                      "a space in each Employees contact name.")]
        public void String_04_IndexOf()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 select new
                                 {
                                     e.Name,
                                     FirstSpaceIndex = e.Name.IndexOf(" ")
                                 };

            var foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(13, foundEmployees.Length);
            Assert.AreEqual("Sio Wernli", foundEmployees[0].Name);
            Assert.AreEqual(3, foundEmployees[0].FirstSpaceIndex);
            Assert.AreEqual("Dr. Rainer Hohn", foundEmployees[1].Name);
            Assert.AreEqual(3, foundEmployees[1].FirstSpaceIndex);
            Assert.AreEqual("Stephanie Süss", foundEmployees[2].Name);
            Assert.AreEqual(9, foundEmployees[2].FirstSpaceIndex);
        }

        /// <summary>
        /// This sample uses the StartsWith method to find Employees whose
        /// name starts with 'S'.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses the StartsWith method to find Employees whose " +
                      "name starts with 'S'.")]
        public void String_05_StartsWith()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 where e.Name.StartsWith("S")
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(2, foundEmployees.Length);
            Assert.AreEqual("Sio Wernli", foundEmployees[0].Name);
            Assert.AreEqual("Stephanie Süss", foundEmployees[1].Name);
        }

        /// <summary>
        /// This sample uses the EndsWith method to find Employees whose
        /// name ends with 'l'.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses the EndsWith method to find Employees whose " +
                      "name ends with 'l'.")]
        public void String_06_EndsWith()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 where e.Name.EndsWith("l")
                                 select e;

            Employee[] foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(2, foundEmployees.Length);
            Assert.AreEqual("Rainer Zufall", foundEmployees[0].Name);
            Assert.AreEqual("Clara Vorteil", foundEmployees[1].Name);
        }

        /// <summary>
        /// This sample uses the Substring method to return Employee names starting
        /// from the fifth letter.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses the Substring method to return Employee names starting " +
                      "from the fifth letter.")]
        public void String_07_Substring()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 select e.Name.Substring(4);

            var foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(13, foundEmployees.Length);
            Assert.AreEqual("Wernli", foundEmployees[0]);
            Assert.AreEqual("Rainer Hohn", foundEmployees[1]);
        }

        /// <summary>
        /// This sample uses the ToUpper / ToLower method to return Employee names
        /// that have been converted to uppercase / lowercase.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses the ToUpper / ToLower method to return Employee names " +
                      "that have been converted to uppercase / lowercase.")]
        public void String_08_ToUpperToLower()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 select new
                                 {
                                     e.Name,
                                     NameUpper = e.Name.ToUpper(),
                                     NameLower = e.Name.ToLower()
                                 };

            var foundEmployees = employeesQuery.ToArray();

            Assert.AreEqual(13, foundEmployees.Length);
            Assert.AreEqual("Sio Wernli", foundEmployees[0].Name);
            Assert.AreEqual("SIO WERNLI", foundEmployees[0].NameUpper);
            Assert.AreEqual("sio wernli", foundEmployees[0].NameLower);
            Assert.AreEqual("Dr. Rainer Hohn", foundEmployees[1].Name);
            Assert.AreEqual("DR. RAINER HOHN", foundEmployees[1].NameUpper);
            Assert.AreEqual("dr. rainer hohn", foundEmployees[1].NameLower);
        }

        /// <summary>
        /// This test trims the substring 'Sio ' pf the first Employee to 'Sio'.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This test trims the substring 'Sio ' pf the first Employee to 'Sio'.")]
        public void String_09_Trim()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 select e.Name.Substring(0, 4).Trim();

            var foundEmployee = employeesQuery.First();

            Assert.AreEqual("Sio", foundEmployee);
        }

        #endregion

        #region Conversion Operators

        /// <summary>
        /// This sample uses ToDictionary to immediately evaluate a query and
        /// a key expression into an Dictionary.
        /// </summary>
        [TestMethod]
        [Owner("Manuel Bauer")]
        [Description("This sample uses ToDictionary to immediately evaluate a query and " +
                      "a key expression into an Dictionary<K, T>.")]
        public void ToDictionary_01()
        {
            var employeesQuery = from e in companyExampleContext.Employees
                                 select e;

            Dictionary<int, Employee> foundEmployees = employeesQuery.ToDictionary(e => e.Id);

            Assert.AreEqual(13, foundEmployees.Count);
            Assert.AreEqual("Sio Wernli", foundEmployees[1].Name);
            Assert.AreEqual("Dr. Rainer Hohn", foundEmployees[2].Name);
        }

        #endregion

        #region Arrays

        /// <summary>
        /// Creates an Array
        /// </summary>
        [TestMethod]
        [Owner("Simon Gubler")]
        [Description("Creates an Array")]
        public void Array_01_Static()
        {

            var employeesQuery = from e in companyExampleContext.Employees
                                 select new { Name = e.Name, StaticArray = new List<string> { "oak", "fir", "spruce", "alder" } };


            Assert.AreEqual(13, employeesQuery.Count());
            Assert.AreEqual("oak", employeesQuery.ToList()[1].StaticArray[0]);
        }

        /// <summary>
        /// Search with the equivalent of the IN-Operator
        /// </summary>
        [TestMethod]
        [Owner("Simon Gubler")]
        [Description("Search with the equivalent of the IN-Operator")]
        public void Array_01_Contains()
        {
            var names = new[] { "Stephanie Süss", "Manuela Zucker", "Franziska Ner", "Dr. Rainer Hohn" };
            var employeesQuery = from e in companyExampleContext.Employees
                                 where names.Contains(e.Name)
                                 orderby e.Name
                                 select e;


            Assert.AreEqual(4, employeesQuery.Count());
            foreach (var employee in employeesQuery)
            {
                Assert.IsTrue(names.Contains(employee.Name));
            }
        }

        #endregion
    }

    /// <summary>
    /// A WCF version of the unit tests.
    /// </summary>
    /// <seealso cref="CompanyExampleTest"/>
    [TestClass]
    public class CompanyExampleTestWcf : CompanyExampleTest
    {

        #region Constructors

        /// <summary>
        /// Instance an new instance of the class <see cref="CompanyExampleTest"/>.
        /// </summary>
        public CompanyExampleTestWcf()
        {
            companyExampleContext = new CompanyContext(ClientEnvironment.GetInstanceWcf(ServiceConstants.EntityFramework4ServiceName).QueryHandler);
        }

        #endregion

    }

    /// <summary>
    /// A remoting version of the unit tests.
    /// </summary>
    /// <seealso cref="CompanyExampleTest"/>
    [TestClass]
    public class CompanyExampleTestRemoting : CompanyExampleTest
    {

        #region Constructors

        /// <summary>
        /// Instance an new instance of the class <see cref="CompanyExampleTest"/>.
        /// </summary>
        public CompanyExampleTestRemoting()
        {
            companyExampleContext = new CompanyContext(ClientEnvironment.GetInstanceRemoting(ServiceConstants.EntityFramework4ServiceName).QueryHandler);
        }

        #endregion

    }

}