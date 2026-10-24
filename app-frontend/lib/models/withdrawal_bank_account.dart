class WithdrawalAccount {
  final WithdrawalAccountBank? bank;
  final WithdrawalAccountCrypto? crypto;

  WithdrawalAccount({
    this.bank,
    this.crypto,
  });

  factory WithdrawalAccount.fromJson(Map<String, dynamic> json) {
    return WithdrawalAccount(
      bank: json['bank'] != null
          ? WithdrawalAccountBank.fromJson(json['bank'])
          : null,
      crypto: json['crypto'] != null
          ? WithdrawalAccountCrypto.fromJson(json['crypto'])
          : null,
    );
  }
}

class WithdrawalAccountBank {
  final String? accountId;
  final String? bankId;
  final String? bankName;
  final String? bankImage;
  final String? accountName;
  final String? accountNo;

  WithdrawalAccountBank({
    this.accountId,
    this.bankId,
    this.bankName,
    this.bankImage,
    this.accountName,
    this.accountNo,
  });

  factory WithdrawalAccountBank.fromJson(Map<String, dynamic> json) {
    return WithdrawalAccountBank(
      accountId: json['account_id']?.toString(),
      bankId: json['bank_id']?.toString(),
      bankName: json['bank_name']?.toString(),
      bankImage: json['bank_image']?.toString(),
      accountName: json['account_name']?.toString(),
      accountNo: json['account_no']?.toString(),
    );
  }
}

class WithdrawalAccountCrypto {
  final String? accountId;
  final String? bankId;
  final String? bankName;
  final String? bankImage;
  final String? cryptoAddress;

  WithdrawalAccountCrypto({
    this.accountId,
    this.bankId,
    this.bankName,
    this.bankImage,
    this.cryptoAddress,
  });

  factory WithdrawalAccountCrypto.fromJson(Map<String, dynamic> json) {
    return WithdrawalAccountCrypto(
      accountId: json['account_id']?.toString(),
      bankId: json['bank_id']?.toString(),
      bankName: json['bank_name']?.toString(),
      bankImage: json['bank_image']?.toString(),
      cryptoAddress: json['crypto_address']?.toString(),
    );
  }
}
