using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessManagement.Models;

namespace ProcessManagement.Tests.Models
{
	class InMemoryPMS : PMSEntities
	{
		private List<Group> _db = new List<Group>();

		public Exception ExceptionToThrow
		{
			get;
			set;
		}

		public IEnumerable<Group> GetAllGroup()
		{
			return _db.ToList();
		}

		public Group FindGroup(int id)
		{
			return _db.FirstOrDefault(group => group.Id == id);
		}

		public void CreateGroup(Group groupToCreate)
		{


			_db.Add(groupToCreate);
		}

		public void DeleteGroup(int id)
		{
			_db.Remove(FindGroup(id));
		}


		public void UpdateGroup(Group groupToUpdate)
		{

			foreach (Group group in _db)
			{
				if (group.Id == groupToUpdate.Id)
				{
					_db.Remove(group);
					_db.Add(groupToUpdate);
					break;
				}
			}
		}

		public int Save()
		{
			return 1;
		}


		private bool disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					//Dispose Object Here    
				}
			}
			this.disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}