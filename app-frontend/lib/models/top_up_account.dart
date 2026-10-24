class TopUpAccount {
  final String? id;
  final String? bankId;
  final String? bankName;
  final String? bankType;
  final String? accountName;
  final String? accountNo;

  TopUpAccount({
    this.id,
    this.bankId,
    this.bankName,
    this.bankType,
    this.accountName,
    this.accountNo,
  });

  factory TopUpAccount.fromJson(Map<String, dynamic> json) {
    return TopUpAccount(
      id: json['id']?.toString(),
      bankId: json['bank_id']?.toString(),
      bankName: json['bank_name']?.toString(),
      bankType: json['bank_type']?.toString(),
      accountName: json['account_name']?.toString(),
      accountNo: json['account_no']?.toString(),
    );
  }
}
