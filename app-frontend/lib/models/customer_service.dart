class CustomerService {
  final String userId;
  final String username;
  final String phone;

  CustomerService({
    required this.userId,
    required this.username,
    required this.phone,
  });

  factory CustomerService.fromJson(Map<String, dynamic> json) {
    return CustomerService(
      userId: json['user_id'],
      username: json['user_name'],
      phone: json['phone'],
    );
  }
}
