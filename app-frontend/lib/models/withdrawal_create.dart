class WithdrawalCreate {
  String bankId;
  String bankTax;
  String bankTaxType;
  String accountId;
  String amount;
  String fundPassword;
  String gauthCode;
  String rate;

  WithdrawalCreate({
    required this.bankId,
    required this.bankTax,
    required this.bankTaxType,
    required this.accountId,
    required this.amount,
    required this.fundPassword,
    required this.gauthCode,
    required this.rate,
  });
}
