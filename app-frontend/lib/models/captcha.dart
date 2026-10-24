class Captcha {
  final String? key;
  final String? imageUrl;

  Captcha({
    required this.key,
    required this.imageUrl,
  });

  factory Captcha.fromJson(Map<String, dynamic> json) {
    return Captcha(
      key: json['key']?.toString(),
      imageUrl: json['image_url']?.toString(),
    );
  }
}
