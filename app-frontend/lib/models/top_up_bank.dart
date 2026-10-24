class TopUpBank {
  final String? bankId;
  final String? bankType;
  final String? bankName;
  final String? bankImage;
  final String? bankTax;
  final String? bankMinAmount;
  final String? bankMaxAmount;
  final String? currencyId;
  final String? currencyName;
  final String? depositRate;

  TopUpBank({
    this.bankId,
    this.bankType,
    this.bankName,
    this.bankImage,
    this.bankTax,
    this.bankMinAmount,
    this.bankMaxAmount,
    this.currencyId,
    this.currencyName,
    this.depositRate,
  });

  factory TopUpBank.fromJson(Map<String, dynamic> json) {
    return TopUpBank(
      bankId: json['bank_id']?.toString(),
      bankType: json['bank_type']?.toString(),
      bankName: json['bank_name']?.toString(),
      bankImage: json['bank_image']?.toString(),
      bankTax: json['bank_tax']?.toString(),
      bankMinAmount: json['bank_min_amount']?.toString(),
      bankMaxAmount: json['bank_max_amount']?.toString(),
      currencyId: json['currency_id']?.toString(),
      currencyName: json['currency_name']?.toString(),
      depositRate: json['deposit_rate']?.toString(),
    );
  }
}
