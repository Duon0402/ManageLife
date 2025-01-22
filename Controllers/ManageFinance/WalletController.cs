using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Models;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class WalletController : WebControllerBase
    {
        private readonly WalletService _service;

        public WalletController(AppDbContext context, ILogger? logger = null) : base(context, logger)
        {
            _service = new WalletService(context);
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new WalletViewModel();

            var rs = await _service.GetListDataAsync();
            if(rs.IsOk())
            {
                if(rs.Data.IsNotEmpty())
                {
                    viewModel.Wallets = rs.Data;
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<Result> GetListData()
        {
            var rs = await _service.GetListDataAsync();
            return rs;
        }

        [HttpGet]
        public async Task<Result> GetDataById(string walletId)
        {
            var rs = await _service.GetDataByIdAsync(walletId);
            return rs;
        }

        [HttpPost]
        public async Task<Result> Insert(WalletModel model)
        {
            var rs = await _service.InsertAsync(model);
            return rs;
        }

        [HttpPut]
        public async Task<Result> Update(WalletModel model)
        {
            var rs = await _service.UpdateAsync(model);
            return rs;
        }

        [HttpDelete]
        public async Task<Result> Delete(string walletId)
        {
            var rs = await _service.DeleteAsync(walletId);
            return rs;
        }
    }
}
