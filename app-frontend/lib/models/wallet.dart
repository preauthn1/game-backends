class Wallet {
  final String? id;
  final String? walletTypeId;
  final String? balance;
  final String? memberId;
  final String? walletTypeName;

  Wallet({
    this.id,
    this.walletTypeId,
    this.balance,
    this.memberId,
    this.walletTypeName,
  });

  factory Wallet.fromJson(Map<String, dynamic> json) {
    return Wallet(
      id: json['id']?.toString(),
      walletTypeId: json['wallet_type_id']?.toString(),
      balance: json['balance']?.toString(),
      memberId: json['member_id']?.toString(),
      walletTypeName: json['wallet_type_name']?.toString(),
    );
  }
}
