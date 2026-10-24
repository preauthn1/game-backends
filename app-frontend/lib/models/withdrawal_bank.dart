class WithdrawalBank {
  final String? bankId;
  final String? bankType;
  final String? bankName;
  final String? bankImage;
  final String? bankTax;
  final String? bankTaxType;
  final String? bankRate;
  final String? bankMinAmount;
  final String? bankMaxAmount;
  final String? currencyId;
  final String? currencyName;
  final String? withdrawalRate;

  WithdrawalBank({
    this.bankId,
    this.bankType,
    this.bankName,
    this.bankImage,
    this.bankTax,
    this.bankTaxType,
    this.bankRate,
    this.bankMinAmount,
    this.bankMaxAmount,
    this.currencyId,
    this.currencyName,
    this.withdrawalRate,
  });

  factory WithdrawalBank.fromJson(Map<String, dynamic> json) {
    return WithdrawalBank(
      bankId: json['bank_id']?.toString(),
      bankType: json['bank_type']?.toString(),
      bankName: json['bank_name']?.toString(),
      bankImage: json['bank_image']?.toString(),
      bankTax: json['bank_tax']?.toString(),
      bankTaxType: json['bank_tax_type']?.toString(),
      bankRate: json['bank_rate']?.toString(),
      bankMinAmount: json['bank_min_amount']?.toString(),
      bankMaxAmount: json['bank_max_amount']?.toString(),
      currencyId: json['currency_id']?.toString(),
      currencyName: json['currency_name']?.toString(),
      withdrawalRate: json['withdrawal_rate']?.toString(),
    );
  }
}
