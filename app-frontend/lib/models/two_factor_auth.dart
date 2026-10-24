class TwoFactorAuth {
  final String? secret;
  final String? qrCodeUrl;
  final String? qrCodeImage;

  TwoFactorAuth({
    required this.secret,
    required this.qrCodeUrl,
    required this.qrCodeImage,
  });

  factory TwoFactorAuth.fromJson(Map<String, dynamic> json) {
    return TwoFactorAuth(
      secret: [REDACTED]'secret']?.toString(),
      qrCodeUrl: json['qr_code_url']?.toString(),
      qrCodeImage: json['qr_code_image']?.toString(),
    );
  }
}
