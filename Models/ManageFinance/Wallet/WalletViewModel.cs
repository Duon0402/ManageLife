namespace ManageLife.Models
{
    public class WalletViewModel
    {
        public WalletViewModel()
        {
            Wallets = new List<WalletModel>();
        }

        public List<WalletModel> Wallets { get; set; }
    }
}
