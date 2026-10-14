namespace ManageLife.Commons
{
    /// <summary>Loại ví: tiền mặt, ngân hàng, ví điện tử, thẻ tín dụng.</summary>
    public enum FinanceWalletType : byte
    {
        Cash = 0,
        Bank = 1,
        EWallet = 2,
        Credit = 3
    }
}
