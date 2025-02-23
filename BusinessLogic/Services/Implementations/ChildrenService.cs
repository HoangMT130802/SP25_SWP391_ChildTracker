using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Implementations
{
    public class ChildrenService : IChildrenService
    {
        private readonly IGenericRepository<Child> _childrenRepository;

        public ChildrenService(IGenericRepository<Child> childrenRepository)
        {
            _childrenRepository = childrenRepository;
        }
    }
}
