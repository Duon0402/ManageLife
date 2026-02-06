using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
	public class FileRepository : RepositoryBase<FileEntity>, IFileRepository
	{
		public FileRepository(AppDbContext context) : base(context)
		{
		}
	}
}
