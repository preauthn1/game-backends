class Country {
  final String? countryId;
  final String? countryName;
  final String? countryCode;
  final String? phoneRule;

  Country({
    required this.countryId,
    required this.countryName,
    required this.countryCode,
    required this.phoneRule,
  });

  factory Country.fromJson(Map<String, dynamic> json) {
    return Country(
      countryId: json['country_id']?.toString(),
      countryName: json['country_name']?.toString(),
      countryCode: json['country_code']?.toString(),
      phoneRule: json['phone_rule']?.toString(),
    );
  }
}
