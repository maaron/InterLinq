﻿using System.Linq;

namespace InterLinq.UnitTests.Artefacts.Objects
{
    public class CompanyContext : InterLinqContext
    {

        public CompanyContext(IQueryHandler queryHandler) : base(queryHandler) { }

        public IQueryable<Company> Companies
        {
            get { return QueryHander.Get<Company>(); }
        }

        public IQueryable<Department> Departments
        {
            get { return QueryHander.Get<Department>(); }
        }

        public IQueryable<Employee> Employees
        {
            get { return QueryHander.Get<Employee>(); }
        }

    }
}
