class Template {
  final bool? smsVerify;
  final bool? captchaVerify;
  final bool? withdrawalRequiresVerifiedId;
  final bool? purchaseMinerRequiresVerifiedId;
  final bool? rentMinerRequiresVerifiedId;
  final bool? groupMinerRequiresVerifiedId;
  final String? androidUrl;
  final String? iosUrl;
  final String? androidMinVersion;
  final String? iosMinVersion;

  Template({
    required this.smsVerify,
    required this.captchaVerify,
    required this.withdrawalRequiresVerifiedId,
    required this.purchaseMinerRequiresVerifiedId,
    required this.rentMinerRequiresVerifiedId,
    required this.groupMinerRequiresVerifiedId,
    required this.androidUrl,
    required this.iosUrl,
    required this.androidMinVersion,
    required this.iosMinVersion,
  });

  factory Template.fromJson(Map<String, dynamic> json) {
    return Template(
      smsVerify: json['sms_verify']?.toString() == '1',
      captchaVerify: json['captcha_verify']?.toString() == '1',
      withdrawalRequiresVerifiedId:
          json['withdrawal_requires_verified_id']?.toString() == '1',
      purchaseMinerRequiresVerifiedId:
          json['purchase_miner_requires_verified_id']?.toString() == '1',
      rentMinerRequiresVerifiedId:
          json['rent_miner_requires_verified_id']?.toString() == '1',
      groupMinerRequiresVerifiedId:
          json['group_miner_requires_verified_id']?.toString() == '1',
      androidUrl: json['android_url']?.toString(),
      iosUrl: json['ios_url']?.toString(),
      androidMinVersion: json['android_min_version']?.toString(),
      iosMinVersion: json['ios_min_version']?.toString(),
    );
  }
}
